using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class BrokeragePlugInTest : EU.GUI.Testing.BrokeragePlugInTest
{
	protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugIn(Shipment);

	protected override BaseJobDeclaration GetDeclaration() => Factory.New<JobDeclaration>();

	protected override Type ExpectedTopLevelMenuType => typeof(EDIMenu);

	public void TestAskToOverride01DISupportingDocumentsWhenSaving()
	{
		const string notificationMessage = "Do you want to update the data for document 01DI with a placeholder ('X')?";

		var organisationWithDOI = Factory.New<OrgHeader>();
		organisationWithDOI.OH_Code = "IMPORTER";
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DOI", permitHolder: organisationWithDOI.PK, "X", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var shipment = Factory.New<ForwardingShipment>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_JS = shipment.PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Factory.Save();

		using (var plugin = new BrokeragePlugIn(shipment))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = organisationWithDOI.PK;
			entryInstruction.ZG_UseDeclarationOfIntent = true;
			var supportingDocument01DI1 = invoice.SupportingDocuments.AddNew();
			var supportingDocument01DI2 = invoice.SupportingDocuments.AddNew();
			supportingDocument01DI1.CSI_Code = supportingDocument01DI2.CSI_Code = Business.UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
			supportingDocument01DI1.CSI_ReferenceNumber = "X";
			supportingDocument01DI2.CSI_ReferenceNumber = "20123111223312345123456";
			CombineAssertions("Should ask", () =>
			{
				Assert(declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
				(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
				AssertEquals(notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});

			invoice.SupportingDocuments.RemoveAndDelete(supportingDocument01DI2);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions("Should not ask", () =>
			{
				Assert(!declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
				(plugin as IShowPreSaveDialog).ShowPreSaveDialogs();
				AssertNotEquals(notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}
}
