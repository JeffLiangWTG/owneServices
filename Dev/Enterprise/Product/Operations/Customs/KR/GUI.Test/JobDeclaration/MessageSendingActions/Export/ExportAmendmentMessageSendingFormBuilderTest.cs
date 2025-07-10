using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	class ExportAmendmentMessageSendingFormBuilderTest : TestCaseWithFactory
	{
		public void TestGetColumnStyle_5AS()
		{
			var builder = new ExportAmendmentMessageSendingFormBuilder();
			var list = builder.GetColumnStyles();

			AssertEquals(list.Length, 8);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ShouldSend), list[0].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FormattedEntryNumber), list[1].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentVersion), list[2].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentTypeDescription), list[3].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentReason), list[4].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.FaultParty), list[5].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.ReasonCode), list[6].ColumnName);
			AssertEquals(nameof(JobDeclarationAmendmentMessageSendingObject.DateOfFinalPrice), list[7].ColumnName);
		}

		public void TestGetAmendmentUserControl()
		{
			var builder = new ExportAmendmentMessageSendingFormBuilder();

			using (var userControl = builder.GetUserControl())
			{
				AssertEquals(typeof(AmendedItemsUserControl), userControl.GetType());
				AmendedItemsUserControl control = (AmendedItemsUserControl)userControl;
			}
		}
		public void TestGetAdditionalTabPages()
		{
			var builder = new ExportAmendmentMessageSendingFormBuilder();

			AssertNull(builder.GetAdditionalTabPages(null));
		}
	}

	[TestedType(typeof(MessageSendingActionForm))]
	class ExportAmendmentMessageSendingActionFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingActionForm(new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5AS), new ExportAmendmentMessageSendingFormBuilder());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders[0];

			var header = new ExportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(header))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}

			invoiceLine.JI_LinePrice = 100000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}
		JobDeclaration declaration;
	}
}
