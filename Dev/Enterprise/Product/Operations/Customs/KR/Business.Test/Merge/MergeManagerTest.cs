using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MergeManagerTest : TestCaseWithFactory
	{
		public void TestLineMergeType()
		{
			var manager = new MergeManager(Factory.New<JobDeclaration>());
			AssertEquals(typeof(LineMerger), manager.GetNewLineMerger().GetType());
		}

		public void TestSupportAmendment()
		{
			var manager = new MergeManager(Factory.New<JobDeclaration>());
			Assert(manager.GetNewLineMerger().SupportsAmendments);
		}

		public void TestGetReasonCannotMerge()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals("You can't merge this declaration because UNIPASS Declarant ID is not entered.\r\nPlease Go to Registry -> Customs -> South Korea -> UNIPASS Declarant ID and enter a code.", declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			AssertNullOrEmpty(declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals("You have changed the shipment type and you can't merge this declaration any more as messages have already been sent.", declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			AssertNullOrEmpty(declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.Messages.RemoveAndDeleteAll();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._01;
			message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			AssertEquals("The merge of this Declaration failed because there is already a message sent and the Export Type was changed to the 5DQ type.", declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._02;
			AssertNullOrEmpty(declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.JE_MessageSubType = CargoWise.Types.ZString.Empty;
			AssertEquals("Please enter the Export Type to proceed with Merge.", declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));

			declaration.JE_MessageSubType = "XX";
			AssertEquals("A valid value must be entered in the Export Type to proceed with Merge.", declaration.MergeManager.CheckAndGetPrerequisiteConditions(notifier));
		}
	}
}
