using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentDeclaration()
		{
			var objectParent = (JobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<JobDeclaration>(objectParent.ParentDeclaration);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			var testWrapper1 = new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration1, GlbStaff.CurrentUser));
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration2.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var entryInstruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;
			var entryInstruction3 = declaration2.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_SubStyle = ZString.Empty;
			var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;
			declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var testWrapper2 = new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration2, GlbStaff.CurrentUser));
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}

		public void TestBizObjValidationMessageErrors_Import()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.ZG_OtherEmailAddr = "dummy@dummy.com";
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var sendingObjectParentPDC = new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			var sendingObjectPDC = sendingObjectParentPDC.SendingObjectsCollection[0];
			sendingObjectPDC.ShouldSend = true;
			sendingObjectPDC.MessageType = DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
			AssertValidationsShownPDC(sendingObjectParentPDC.BizObjValidationMessageErrors, declaration, invoice, invoiceLine);

			var sendingObjectParentPDI = new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			var sendingObjectPDI = sendingObjectParentPDI.SendingObjectsCollection[0];
			sendingObjectPDI.ShouldSend = true;
			sendingObjectPDI.MessageType = DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration;
			AssertValidationsShownPDI(sendingObjectParentPDI.BizObjValidationMessageErrors, declaration, invoice, invoiceLine);

			var sendingObjectParentPDS = new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
			var sendingObjectPDS = sendingObjectParentPDS.SendingObjectsCollection[0];
			sendingObjectPDS.ShouldSend = true;
			sendingObjectPDS.MessageType = DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
			AssertValidationsShownPDS(sendingObjectParentPDS.BizObjValidationMessageErrors, declaration, invoice, invoiceLine);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			return new JobDeclarationMessageSendingObjectParent(new MessageSendingObject(declaration, GlbStaff.CurrentUser));
		}

		void AssertValidationsShownPDC(ZString messageErrors, JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			CombineAssertions("For PDC declaration", () =>
			{
				AssertContains("Declaration/Shipment Details/[20.2] Place", declaration.JE_ShipmentIncoTermPlaceInfo.HumanReadableName + ": " + declaration.JE_ShipmentIncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Declaration/Shipment Details/[20.2] Place/Code", declaration.ZG_AgreedPlaceCodeInfo.HumanReadableName + ": " + declaration.ZG_AgreedPlaceCodeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Declaration/Shipment Details/[21] Nationality", declaration.JE_RN_NKTransportNationalityInfo.HumanReadableName + ": " + declaration.JE_RN_NKTransportNationalityInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Declaration/Shipment Details/[18] Transport ID (Inland)", declaration.ZG_Box18TransportIDInfo.HumanReadableName + ": " + declaration.ZG_Box18TransportIDInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Supplier", invoice.JZ_OH_SupplierInfo.HumanReadableName + ": " + invoice.JZ_OH_SupplierInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Commercial Invoice Details/Incoterm", invoice.JZ_IncoTermInfo.HumanReadableName + ": " + invoice.JZ_IncoTermInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Commercial Invoice Details/Agreed Place", invoice.JZ_IncoTermPlaceInfo.HumanReadableName + ": " + invoice.JZ_IncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Commercial Invoice Details/[22] Inv. Amount", invoice.JZ_InvoiceAmountInfo.HumanReadableName + ": " + invoice.JZ_InvoiceAmountInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Supporting Documents (Invoice)", invoice.HumanReadableName + ": " + GetAtLeastOneSupportingDocumentMessage(declaration), messageErrors);
				AssertContains("Inv. Lines/Line Details/[36] Pref. Code", invoiceLine.JI_PrimaryPreferenceInfo.HumanReadableName + ": " + invoiceLine.JI_PrimaryPreferenceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			});
		}

		void AssertValidationsShownPDS(ZString messageErrors, JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			CombineAssertions("For PDS declaration", () =>
			{
				AssertNotContains("Declaration/Shipment Details/[20.2] Place", declaration.JE_ShipmentIncoTermPlaceInfo.HumanReadableName + ": " + declaration.JE_ShipmentIncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Declaration/Shipment Details/[20.2] Place/Code", declaration.ZG_AgreedPlaceCodeInfo.HumanReadableName + ": " + declaration.ZG_AgreedPlaceCodeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Declaration/Shipment Details/[21] Nationality", declaration.JE_RN_NKTransportNationalityInfo.HumanReadableName + ": " + declaration.JE_RN_NKTransportNationalityInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Declaration/Shipment Details/[18] Transport ID (Inland)", declaration.ZG_Box18TransportIDInfo.HumanReadableName + ": " + declaration.ZG_Box18TransportIDInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Supplier", invoice.JZ_OH_SupplierInfo.HumanReadableName + ": " + invoice.JZ_OH_SupplierInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/Incoterm", invoice.JZ_IncoTermInfo.HumanReadableName + ": " + invoice.JZ_IncoTermInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/Agreed Place", invoice.JZ_IncoTermPlaceInfo.HumanReadableName + ": " + invoice.JZ_IncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/[22] Inv. Amount", invoice.JZ_InvoiceAmountInfo.HumanReadableName + ": " + invoice.JZ_InvoiceAmountInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Invoice Header/Supporting Documents (Invoice)", invoice.HumanReadableName + ": " + GetAtLeastOneSupportingDocumentMessage(declaration), messageErrors);
				AssertContains("Inv. Lines/Line Details/[36] Pref. Code", invoiceLine.JI_PrimaryPreferenceInfo.HumanReadableName + ": " + invoiceLine.JI_PrimaryPreferenceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			});
		}

		void AssertValidationsShownPDI(ZString messageErrors, JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			CombineAssertions("For PDI declaration", () =>
			{
				AssertNotContains("Declaration/Shipment Details/[20.2] Place", declaration.JE_ShipmentIncoTermPlaceInfo.HumanReadableName + ": " + declaration.JE_ShipmentIncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Declaration/Shipment Details/[20.2] Place/Code", declaration.ZG_AgreedPlaceCodeInfo.HumanReadableName + ": " + declaration.ZG_AgreedPlaceCodeInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Declaration/Shipment Details/[21] Nationality", declaration.JE_RN_NKTransportNationalityInfo.HumanReadableName + ": " + declaration.JE_RN_NKTransportNationalityInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertContains("Declaration/Shipment Details/[18] Transport ID (Inland)", declaration.ZG_Box18TransportIDInfo.HumanReadableName + ": " + declaration.ZG_Box18TransportIDInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Supplier", invoice.JZ_OH_SupplierInfo.HumanReadableName + ": " + invoice.JZ_OH_SupplierInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/Incoterm", invoice.JZ_IncoTermInfo.HumanReadableName + ": " + invoice.JZ_IncoTermInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/Agreed Place", invoice.JZ_IncoTermPlaceInfo.HumanReadableName + ": " + invoice.JZ_IncoTermPlaceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Commercial Invoice Details/[22] Inv. Amount", invoice.JZ_InvoiceAmountInfo.HumanReadableName + ": " + invoice.JZ_InvoiceAmountInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
				AssertNotContains("Invoice Header/Supporting Documents (Invoice)", invoice.HumanReadableName + ": " + GetAtLeastOneSupportingDocumentMessage(declaration), messageErrors);
				AssertNotContains("Inv. Lines/Line Details/[36] Pref. Code", invoiceLine.JI_PrimaryPreferenceInfo.HumanReadableName + ": " + invoiceLine.JI_PrimaryPreferenceInfo.GetMessageErrors().ToUniqueMessageListString(), messageErrors);
			});
		}

		string GetAtLeastOneSupportingDocumentMessage(JobDeclaration declaration)
		{
			if (declaration.IsExport)
			{
				return "At least one Supporting Document of type: N380, N325 or N935 must be present at the Invoice Header level or in all of its Invoice Lines.";
			}

			return "At least one Supporting Document of type: N380, N325, N935, D005, D008, 1001 or 1003 must be present at the Invoice Header level or in all of its Invoice Lines.";
		}
	}
}
