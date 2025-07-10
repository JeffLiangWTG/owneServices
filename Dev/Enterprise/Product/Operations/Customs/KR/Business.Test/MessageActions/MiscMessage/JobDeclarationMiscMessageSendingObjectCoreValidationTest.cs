using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationMiscMessageSendingObjectCoreValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAmendmentReason()
		{
			var parentEXP = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			parentEXP.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentEXP.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentEXP.ShouldSend = true;
			parentEXP.Validation.ValidateAmendmentReason();
			AssertHasMessageErrorContaining(parentEXP.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentEXP.AmendmentReason = "기재오류";
			AssertNoMessageErrorContaining(parentEXP.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			var parentLEX = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DR, MessageFunctions.MessageFunctionCode.Amendment);
			parentLEX.Validation.ValidateAmendmentReason();
			AssertNoErrorContaining(parentLEX.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentLEX.ShouldSend = true;
			parentLEX.ReasonCode = LocalExportAmendmentReasonCodeList.Codes._9;
			parentLEX.AmendmentReason = ZString.Empty;
			AssertHasMessageErrorContaining(parentLEX.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentLEX.ReasonCode = LocalExportAmendmentReasonCodeList.Codes._1;
			parentLEX.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentLEX.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentLEX.ReasonCode = LocalExportAmendmentReasonCodeList.Codes._2;
			parentLEX.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentLEX.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			var parentIMPDHS = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._DHS, MessageFunctions.MessageFunctionCode.Amendment);
			parentIMPDHS.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentIMPDHS.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMPDHS.ShouldSend = true;
			parentIMPDHS.Validation.ValidateAmendmentReason();
			AssertHasMessageErrorContaining(parentIMPDHS.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMPDHS.AmendmentReason = "기재오류";
			AssertNoMessageErrorContaining(parentIMPDHS.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			var parentIMP5FE = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5FE, MessageFunctions.MessageFunctionCode.Amendment);
			parentIMP5FE.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentIMP5FE.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMP5FE.ShouldSend = true;
			parentIMP5FE.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentIMP5FE.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMP5FE.AmendmentReason = "기재오류";
			AssertNoMessageErrorContaining(parentIMP5FE.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			var parentIMP5BB = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BB, MessageFunctions.MessageFunctionCode.Amendment);
			parentIMP5BB.Validation.ValidateAmendmentReason();
			AssertNoMessageErrorContaining(parentIMP5BB.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMP5BB.ShouldSend = true;
			parentIMP5BB.Validation.ValidateAmendmentReason();
			AssertHasMessageErrorContaining(parentIMP5BB.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parentIMP5BB.AmendmentReason = "기재오류";
			AssertNoMessageErrorContaining(parentIMP5BB.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckReasonCode()
		{
			var parent = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			parent.ShouldSend = true;
			parent.ReasonCode = "";
			parent.Validation.ValidateReasonCode();
			AssertHasMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parent.ReasonCode = "99";
			parent.Validation.ValidateReasonCode();
			AssertHasMessageErrorContaining(parent.ReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._11;
			parent.Validation.ValidateReasonCode();
			AssertNoMessageErrors(parent.ReasonCodeInfo);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._12;
			parent.Validation.ValidateReasonCode();
			AssertNoMessageErrors(parent.ReasonCodeInfo);
		}

		public void TestCheckFaultParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			var parent = new JobDeclarationMiscMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend, x => true);

			parent.ShouldSend = true;
			parent.FaultParty = "";
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);

			parent.FaultParty = "I";
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, ListValidation.InvalidCodeMessageError);

			parent.FaultParty = ExportImputationReasonCodeList.Codes.A;
			AssertNoMessageErrors(parent.FaultPartyInfo);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._12;
			parent.FaultParty = ExportImputationReasonCodeList.Codes.B;
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, parent.Validation.FaultPartyWithReasonCodeMessageErr);

			parent.FaultParty = ExportImputationReasonCodeList.Codes.A;
			AssertNoMessageErrors(parent.FaultPartyInfo);

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");
			parent.Validation.ValidateFaultParty();
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, parent.Validation.FaultPartyWithSupplierMessageErr);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._14;
			parent.FaultParty = ExportImputationReasonCodeList.Codes.C;
			AssertNoMessageErrors(parent.FaultPartyInfo);
		}

		public void TestShouldSend_ValidateAll()
		{
			var parent = GetJobDeclarationMiscMessageSendingObject(KRJobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._5AS, MessageFunctions.MessageFunctionCode.Extend);
			AssertNoMessageErrorContaining(parent.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parent.ShouldSend = true;
			AssertHasMessageErrorContaining(parent.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parent.ShouldSend = false;
			AssertNoMessageErrorContaining(parent.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parent.AmendmentReason = "기재오류";
			parent.FaultParty = ExportImputationReasonCodeList.Codes.C;
			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._14;
			parent.ShouldSend = true;
			AssertNoMessageErrorContaining(parent.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		JobDeclarationMiscMessageSendingObject GetJobDeclarationMiscMessageSendingObject(string messageStatus, string code, MessageFunctions.MessageFunctionCode functionCode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageStatus;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			return new JobDeclarationMiscMessageSendingObject(entry, code, functionCode, x => true);
		}
	}
}
