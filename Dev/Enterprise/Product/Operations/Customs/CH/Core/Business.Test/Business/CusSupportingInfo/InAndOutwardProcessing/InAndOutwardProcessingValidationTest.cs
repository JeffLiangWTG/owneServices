using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InAndOutwardProcessingValidation))]
sealed class InAndOutwardProcessingValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_SubType()
	{
		RefCusCodeTestHelper.CreateInAndOutwardDirectionList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(InAndOutwardProcessing.CSI_SubTypeInfo, RefCusCodeTestHelper.InvalidSimpleCode, RefCusCodeTestHelper.ValidSimpleCode);
	}

	public void TestCheckCSI_SubType_R205()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertMandatoryIfRefinementTransportation(InAndOutwardProcessing.CSI_SubTypeInfo);
	}

	public void TestCheckCSI_SubType_R206()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InAndOutwardProcessing.Repair = ZBool.True;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InAndOutwardProcessing.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered, "When Repair is true");

			InAndOutwardProcessing.Repair = ZBool.False;
			InAndOutwardProcessing.CSI_SubType = ZString.Empty;
			AssertNoNotifications(InAndOutwardProcessing.CSI_SubTypeInfo);
			InAndOutwardProcessing.Repair = ZBool.True;
			AssertHasMessageErrorContaining("Should be triggered by CSI_Status", InAndOutwardProcessing.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_Code()
	{
		RefCusCodeTestHelper.CreateInAndOutwardRefinementTypeList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(InAndOutwardProcessing.CSI_CodeInfo, RefCusCodeTestHelper.InvalidSimpleCode, RefCusCodeTestHelper.ValidSimpleCode);
	}

	public void TestCheckCSI_Code_R205()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AssertMandatoryIfRefinementTransportation(InAndOutwardProcessing.CSI_CodeInfo);
	}

	public void TestCheckCSI_Code_NS30098_Required() => CombineAssertions(() =>
	{
		AssertNS30098_Mandatory(InAndOutwardProcessing.CSI_CodeInfo, "Refinement Type");
	});

	public void TestCheckCSI_Procedure()
	{
		RefCusCodeTestHelper.CreateInAndOutwardProcessTypeList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, RefCusCodeTestHelper.InvalidSimpleCode, RefCusCodeTestHelper.ValidSimpleCode);
	}

	public void TestCheckCSI_Procedure_R205()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AssertMandatoryIfRefinementTransportation(InAndOutwardProcessing.CSI_ProcedureInfo);
	}

	public void TestCheckCSI_Procedure_R208()
	{
		void SetPropertiesForError()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			InvoiceLine.JI_ZZF_NKTaxType = TaxCodes.StandardRate;
			InvoiceLine.JI_NonTradingGoods = ZBool.True;
			InAndOutwardProcessing.Repair = ZBool.False;
			InAndOutwardProcessing.CSI_SubType = InAndOutwardProcessingDirectionCodes.Active;
			InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		}

		CombineAssertions(() =>
		{
			SetPropertiesForError();
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertHasMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InvoiceLine.JI_ZZF_NKTaxType = TaxCodes.ExemptVat;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InvoiceLine.JI_NonTradingGoods = ZBool.False;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InAndOutwardProcessing.Repair = ZBool.True;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InAndOutwardProcessing.CSI_SubType = InAndOutwardProcessingDirectionCodes.Passive;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);

			SetPropertiesForError();
			InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageError(InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR208);
		});
	}

	public void TestCheckCSI_ProcedureR361()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.RefinementTransportation;
			InvoiceLine.InAndOutwardProcessingDirection = UniversalReferenceConstants.InAndOutwardProcessingDirectionCodes.Active;
			InvoiceLine.InAndOutwardProcessingProcessType = UniversalReferenceConstants.InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
			AssertHasMessageErrorContaining(InvoiceLine.InAndOutwardProcessingProcessTypeInfo, ValidationMessages.Plausi.MessageR361);

			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			InvoiceLine.InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageErrorContaining(InvoiceLine.InAndOutwardProcessingProcessTypeInfo, ValidationMessages.Plausi.MessageR361);

			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.RefinementTransportation;
			InvoiceLine.InAndOutwardProcessingDirection = UniversalReferenceConstants.InAndOutwardProcessingDirectionCodes.Passive;
			InvoiceLine.InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageErrorContaining(InvoiceLine.InAndOutwardProcessingProcessTypeInfo, ValidationMessages.Plausi.MessageR361);

			InvoiceLine.InAndOutwardProcessingDirection = UniversalReferenceConstants.InAndOutwardProcessingDirectionCodes.Active;
			InvoiceLine.InAndOutwardProcessingProcessType = UniversalReferenceConstants.InAndOutwardProcessingProcessTypesEdec.DueProcedure;
			AssertNoMessageErrorContaining(InvoiceLine.InAndOutwardProcessingProcessTypeInfo, ValidationMessages.Plausi.MessageR361);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.InAndOutwardProcessingProcessType = UniversalReferenceConstants.InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
			AssertNoMessageErrorContaining(InvoiceLine.InAndOutwardProcessingProcessTypeInfo, ValidationMessages.Plausi.MessageR361);
		});
	}

	public void TestCSI_IssuerType()
	{
		RefCusCodeTestHelper.CreateInAndOutwardBillingTypeList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(InAndOutwardProcessing.CSI_IssuerTypeInfo, RefCusCodeTestHelper.InvalidSimpleCode, RefCusCodeTestHelper.ValidSimpleCode);
	}

	public void TestCheckCSI_IssuerType_NP70163() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNP70163;

		var invoiceLine = InAndOutwardProcessing.Parent;
		var entryInstruction = invoiceLine.Declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertMessage(true);
		AssertMessage(false, ceiProcedure: ProcedureCodesPassar.ExportFromFreeCirculation);
		AssertMessage(false, csiIssuerType: InAndOutwardProcessingBillingTypesPassar.NonCollection);
		AssertMessage(false, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation);
		AssertMessage(false, messageType: CHJobMessageTypeList.Codes.Import);

		void AssertMessage(bool messageExpected, string messageType = CHJobMessageTypeList.Codes.Export, string ceiProcedure = ProcedureCodesPassar.OutwardProcessing, string csiIssuerType = InAndOutwardProcessingBillingTypesPassar.Refund)
		{
			invoiceLine.Declaration.JE_MessageType = messageType;
			entryInstruction.CEI_Procedure = ceiProcedure;
			InAndOutwardProcessing.CSI_IssuerType = csiIssuerType;
			var assertionMessage = $"JE_MessageType={messageType} CEI_Procedure={ceiProcedure}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InAndOutwardProcessing.CSI_IssuerTypeInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InAndOutwardProcessing.CSI_IssuerTypeInfo, messageError);
			}
		}
	});

	public void TestCheckCSI_ProcedureR190() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertNoMessageErrorContaining("No message error when JI_procedure and CSI_Procedure are not set", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when JI_Procedure=02 and CSI_Procedure is not set", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);

		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		AssertHasMessageErrorContaining("Message error should be visible when JI_Procedure=02 and CSI_Procedure=1", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);

		var permit = InvoiceLine.Permits.AddNew();
		permit.CSI_IssuerType = PermitAuthorityCodes.FOCBS_Other;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when a Permit issued by Permit Authority 98 is defined", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when JI_Procedure!=02", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		AssertNoMessageErrorContaining("No message error when CSI_Procedure != Due Proecure", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR190);
	});

	public void TestCSI_ProcedureR229() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertNoMessageErrorContaining("No message error when JI_procedure and CSI_Procedure are not set", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when JI_Procedure=02 and CSI_Procedure is not set", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);

		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		AssertHasMessageErrorContaining("Message error should be visible when JI_Procedure=02 and CSI_Procedure=1", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);

		var customsOffice = InvoiceLine.NotifyCustomsOffices.AddNew();
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when at least one Notify Customs Office is defined", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageErrorContaining("No message error when JI_Procedure!=02", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		AssertNoMessageErrorContaining("No message error when CSI_Procedure != Due Procedure", InAndOutwardProcessing.CSI_ProcedureInfo, ValidationMessages.Plausi.MessageR229);
	});

	public void TestCheckCSI_Procedure_NS30098_Mandatory() => CombineAssertions(() =>
	{
		AssertNS30098_Mandatory(InAndOutwardProcessing.CSI_ProcedureInfo, "Process Type");
	});

	public void TestCheckCSI_IssuerType_R205()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AssertMandatoryIfRefinementTransportation(InAndOutwardProcessing.CSI_IssuerTypeInfo);
	}

	void AssertMandatoryIfRefinementTransportation(ZPropertyInfo targetPropertyInfo)
	{
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetPropertyInfo, MandatoryValidation.YouHaveNotEntered, $"JI_Procedure={InvoiceLine.JI_Procedure}");

			InvoiceLine.JI_Procedure = ZString.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetPropertyInfo, MandatoryValidation.YouHaveNotEntered, $"JI_Procedure={InvoiceLine.JI_Procedure}");

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			AssertHasMessageErrorContaining("Validation should have been triggered by JI_Procedure", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_Procedure_NP70165()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		string expectedMessageError = PassarValidationMessages.MessageNP70165;
		CombineAssertions(() =>
		{
			EntryInstruction.CEI_Procedure = UniversalReferenceConstants.ProcedureCodesPassar.OutwardProcessing;
			InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
			AssertNoMessageErrorContaining("Assert if CEI_Procedure is 50 and CSI_Procedure is 2 no error", InAndOutwardProcessing.CSI_ProcedureInfo, expectedMessageError);

			InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SpecialProcedure;
			AssertNoMessageErrorContaining("Assert if CEI_Procedure is 50 and CSI_Procedure is not 2 no error", InAndOutwardProcessing.CSI_ProcedureInfo, expectedMessageError);

			EntryInstruction.CEI_Procedure = UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation;
			InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
			AssertNoMessageErrorContaining("Assert if CEI_Procedure is not 50 and CSI_Procedure is not 2 no error", InAndOutwardProcessing.CSI_ProcedureInfo, expectedMessageError);

			InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
			AssertHasMessageErrorContaining("Assert if CEI_Procedure is not 50 and CSI_Procedure is 2 error", InAndOutwardProcessing.CSI_ProcedureInfo, expectedMessageError);
		});
	}

	public void TestCheckCSI_Description_R206()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			InAndOutwardProcessing.Repair = ZBool.True;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InAndOutwardProcessing.CSI_DescriptionInfo);

			InAndOutwardProcessing.Repair = ZBool.False;
			InAndOutwardProcessing.CSI_Description = ZString.Empty;
			AssertNoNotifications(InAndOutwardProcessing.CSI_DescriptionInfo);
			InAndOutwardProcessing.Repair = ZBool.True;
			AssertHasMessageErrorContaining("Should be triggered by CSI_Status", InAndOutwardProcessing.CSI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_Description_R358()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoMessageErrorContaining("No message error when JI_procedure and CSI_SubType are not set", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			InAndOutwardProcessing.Validation.ValidateCSI_Description();
			AssertNoMessageErrorContaining("No message error when JI_Procedure=02 and CSI_SubType is not set", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			InAndOutwardProcessing.CSI_SubType = InAndOutwardProcessingDirectionCodes.Active;
			InAndOutwardProcessing.Validation.ValidateCSI_Description();
			AssertHasMessageErrorContaining("Message error should be visible when JI_Procedure=02 and CSI_SubType=2", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InAndOutwardProcessing.Validation.ValidateCSI_Description();
			AssertNoMessageErrorContaining("No message error when is declaration is Export", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InAndOutwardProcessing.CSI_Description = "Repair reason";
			AssertNoMessageErrorContaining("No message error when a Repair reason is defined", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
			InAndOutwardProcessing.Validation.ValidateCSI_Description();
			AssertNoMessageErrorContaining("No message error when JI_Procedure!=04", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			InAndOutwardProcessing.CSI_SubType = InAndOutwardProcessingDirectionCodes.Passive;
			InAndOutwardProcessing.Validation.ValidateCSI_Description();
			AssertNoMessageErrorContaining("No message error when CSI_SubType != Active", InAndOutwardProcessing.CSI_DescriptionInfo, ValidationMessages.Plausi.MessageR358);
		});
	}

	public void TestCheckCSI_Description_NS30098_Mandatory()
	{
		const string messageError = @"[NS30098] When Procedure is ""50-Outward processing (temporary exportation of goods for treatment, processing and repair)"", or Procedure is ""20-Export from free circulation"" and Repair is ticked, Reason is mandatory.";

		AssertMandatory(true, ProcedureCodesPassar.OutwardProcessing, isRepair: false);
		AssertMandatory(true, ProcedureCodesPassar.OutwardProcessing, isRepair: true);
		AssertMandatory(false, ProcedureCodesPassar.ExportFromFreeCirculation, isRepair: false);
		AssertMandatory(true, ProcedureCodesPassar.ExportFromFreeCirculation, isRepair: true);
		AssertMandatory(false, ProcedureCodesPassar.ReExportAfterInwardProcessing, isRepair: false);
		AssertMandatory(false, ProcedureCodesPassar.ReExportAfterInwardProcessing, isRepair: true);
		AssertMandatory(false, ProcedureCodesPassar.OutwardProcessing, isRepair: false, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation);
		AssertMandatory(false, ProcedureCodesPassar.OutwardProcessing, isRepair: false, messageType: CHJobMessageTypeList.Codes.Import);

		void AssertMandatory(bool isMandatoryExpected, string ceiProcedure, bool isRepair, string messageType = CHJobMessageTypeList.Codes.Export, [CallerLineNumber] int lineNumber = 0)
		{
			var assertionMessage = $"[{lineNumber}] {messageType}: CEI_Procedure={ceiProcedure} Repair={isRepair}";
			Declaration.JE_MessageType = messageType;
			EntryInstruction.CEI_Procedure = ceiProcedure;
			InAndOutwardProcessing.Repair = isRepair;
			InAndOutwardProcessing.CSI_Description = ZString.Empty;
			if (isMandatoryExpected)
			{
				AssertHasMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
			}
			InAndOutwardProcessing.CSI_Description = "XXX";
			AssertNoMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
		}
	}

	public void TestCheckCSI_Description_NS30098_Applicable()
	{
		const string messageError = @"[NS30098] When Procedure is ""20-Export from free circulation"" and Repair is not ticked, Reason must be empty.";

		AssertApplicable(false, ProcedureCodesPassar.ExportFromFreeCirculation, isRepair: false);
		AssertApplicable(true, ProcedureCodesPassar.ExportFromFreeCirculation, isRepair: true);
		AssertApplicable(true, ProcedureCodesPassar.OutwardProcessing);
		AssertApplicable(true, ProcedureCodesPassar.ReExportAfterInwardProcessing);
		AssertApplicable(true, ProcedureCodesPassar.ExportFromFreeCirculation, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation);
		AssertApplicable(true, ProcedureCodesPassar.ExportFromFreeCirculation, messageType: CHJobMessageTypeList.Codes.Import);

		void AssertApplicable(bool isApplicable, string ceiProcedure, bool isRepair = false, string messageType = CHJobMessageTypeList.Codes.Export, [CallerLineNumber] int lineNumber = 0)
		{
			var assertionMessage = $"[{lineNumber}] {messageType}: CEI_Procedure={ceiProcedure} Repair={isRepair}";
			Declaration.JE_MessageType = messageType;
			EntryInstruction.CEI_Procedure = ceiProcedure;
			InAndOutwardProcessing.Repair = isRepair;
			InAndOutwardProcessing.CSI_Description = "XXX";
			if (!isApplicable)
			{
				AssertHasMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
			}
			InAndOutwardProcessing.CSI_Description = ZString.Empty;
			AssertNoMessageError(assertionMessage, InAndOutwardProcessing.CSI_DescriptionInfo, messageError);
		}
	}

	public void TestCheckCSI_IssuerType_R261()
	{
		CombineAssertions(() =>
		{
			void SetProperties(string messageType = JobMessageTypeList.Codes.Import, string procedure = ProcedureCodesEdec.RefinementTransportation, string direction = InAndOutwardProcessingDirectionCodes.Passive, string processingType = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure, bool repair = false, string billingType = InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure, bool nonTradingGoods = false)
			{
				Declaration.JE_MessageType = messageType;
				InvoiceLine.JI_Procedure = procedure;
				InvoiceLine.JI_NonTradingGoods = nonTradingGoods;
				InAndOutwardProcessing.CSI_SubType = direction;
				InAndOutwardProcessing.CSI_Procedure = processingType;
				InAndOutwardProcessing.Repair = repair;
				InAndOutwardProcessing.CSI_IssuerType = billingType;
			}

			SetProperties();
			AssertNoMessageError("No error", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(billingType: InAndOutwardProcessingBillingTypesEdec.RefundProcedure);
			AssertHasMessageError("BillingType<>1", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(nonTradingGoods: true);
			AssertNoMessageError("Should not rely on NonTradingGoods anymore", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(messageType: JobMessageTypeList.Codes.Export);
			AssertNoMessageError("Not Import", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(procedure: ProcedureCodesEdec.NormalDuty);
			AssertNoMessageError("Procedure not 02", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(direction: InAndOutwardProcessingDirectionCodes.Active);
			AssertNoMessageError("Direction not 2", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(processingType: InAndOutwardProcessingProcessTypesEdec.DueProcedure);
			AssertNoMessageError("ProcessingType not 1", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);

			SetProperties(repair: true);
			AssertNoMessageError("Repair ticked", InAndOutwardProcessing.CSI_IssuerTypeInfo, ValidationMessages.Plausi.MessageR261_1);
		});
	}

	public void TestCheckCSI_IssuerType_NS30098_Mandatory() => CombineAssertions(() =>
	{
		AssertNS30098_Mandatory(InAndOutwardProcessing.CSI_IssuerTypeInfo, "Billing Type");
	});

	public void TestCheckRepair_NS30003() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30003_Ticked("Repair");

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertMessage(false, InputControlCodes.Simplified, ZBool.False);
		AssertMessage(true, InputControlCodes.Simplified, ZBool.True);
		AssertMessage(false, InputControlCodes.Ordinary, ZBool.True);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertMessage(false, InputControlCodes.Simplified, ZBool.True);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertMessage(false, InputControlCodes.Simplified, ZBool.True);

		void AssertMessage(bool messageExpected, string ceiStyle, ZBool repair, [CallerLineNumber] int lineNumber = 0)
		{
			EntryInstruction.CEI_Style = ceiStyle;
			InAndOutwardProcessing.Repair = repair;
			InAndOutwardProcessing.Validation.ValidateAll();
			var assertionMessage = $"[{lineNumber}] JE_MessageType={Declaration.JE_MessageType} CEI_Style={ceiStyle}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InAndOutwardProcessing.RepairInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InAndOutwardProcessing.RepairInfo, message);
			}
		}
	});

	public void TestCheckCSI_CustomsOffice() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNotifyCustomsOfficeList(Factory);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeMessageError(InAndOutwardProcessing.CSI_CustomsOfficeInfo, RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InAndOutwardProcessing.CSI_CustomsOffice = "@@1";
		AssertNoNotifications("No validation foir EDA", InAndOutwardProcessing.CSI_CustomsOfficeInfo);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		InAndOutwardProcessing.CSI_CustomsOffice = "@@2";
		AssertNoNotifications("No validation for Import", InAndOutwardProcessing.CSI_CustomsOfficeInfo);
	});

	public void TestCheckPermit_NP70162() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var messageError = PassarValidationMessages.MessageNP70162;

		InvoiceLine.EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InvoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertHasMessageError("No Permit", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		var restriction = invoiceLine.Restrictions.AddNew();
		restriction.CSI_ReferenceNumber = "Test Reference";
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertHasMessageError("No Permit Code", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		restriction.CSI_Code = RefCusCodeTestHelper.RestrictionExceptionCodeOther;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageError("No Error Message, ProcedureCode = 41", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		restriction.CSI_ReferenceNumber = ZString.Empty;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertHasMessageError("No Permit Reference Number", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		InvoiceLine.EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		restriction.CSI_ReferenceNumber = "Test Reference";
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageError("No Error Message, ProcedureCode = 50", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		InvoiceLine.EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		InAndOutwardProcessing.Validation.ValidateCSI_Procedure();
		AssertNoMessageError("ProcedureCode = 20", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);

		InvoiceLine.EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InvoiceLine.InAndOutwardProcessingProcessType = InAndOutwardProcessingProcessTypesEdec.SpecialProcedure;
		AssertNoMessageError("ProcessingType = 3", InvoiceLine.InAndOutwardProcessingProcessTypeInfo, messageError);
	});

	#region NS30003

	public void TestCheckCSI_Code_NS30003()
	{
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InAndOutwardProcessing.CSI_CodeInfo, PassarValidationMessages.MessageNS30003_InwardOutward);
	}

	public void TestCheckCSI_Procedure_NS30003()
	{
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InAndOutwardProcessing.CSI_ProcedureInfo, PassarValidationMessages.MessageNS30003_InwardOutward);
	}

	public void TestCheckCSI_IssuerType_NS30003()
	{
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InAndOutwardProcessing.CSI_IssuerTypeInfo, PassarValidationMessages.MessageNS30003_InwardOutward);
	}

	public void TestCheckCSI_Description_NS30003()
	{
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InAndOutwardProcessing.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30003_InwardOutward);
	}

	public void TestCheckCSI_CustomsOffice_NS30003()
	{
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InAndOutwardProcessing.CSI_CustomsOfficeInfo, PassarValidationMessages.MessageNS30003_InwardOutward);
	}

	#endregion

	void AssertNS30098_Mandatory(ZPropertyInfo propertyInfo, string propertyCaption)
	{
		var messageError = $@"[NS30098] When Procedure is ""50-Outward processing (temporary exportation of goods for treatment, processing and repair)"", or ""41-Re-Export after inward processing"" {propertyCaption} is mandatory.";

		AssertMandatory(true, ProcedureCodesPassar.OutwardProcessing);
		AssertMandatory(true, ProcedureCodesPassar.ReExportAfterInwardProcessing);
		AssertMandatory(false, ProcedureCodesPassar.ExportFromFreeCirculation);
		AssertMandatory(false, ProcedureCodesPassar.OutwardProcessing, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation);
		AssertMandatory(false, ProcedureCodesPassar.OutwardProcessing, messageType: CHJobMessageTypeList.Codes.Import);

		void AssertMandatory(bool messageExpected, string ceiProcedureCode, string messageType = CHJobMessageTypeList.Codes.Export, [CallerLineNumber] int lineNumber = 0)
		{
			var assertionMessage = $"[{lineNumber}] {messageType}: CEI_ProcedureCode={ceiProcedureCode}";
			Declaration.JE_MessageType = messageType;
			EntryInstruction.CEI_Procedure = ceiProcedureCode;
			propertyInfo.Value = ZString.Empty;
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, propertyInfo, messageError);
			}
			else
			{
				AssertNoMessageError(assertionMessage, propertyInfo, messageError);
			}
			propertyInfo.Value = new ZString("X");
			AssertNoMessageError(assertionMessage, propertyInfo, messageError);
		}
	}

	public void TestCheckCSI_CustomsOffice_NS30098_NotApplicable() => CombineAssertions(() =>
	{
		string expectedMessageError = PassarValidationMessages.MessageNS30098_CustomsOfficeNotApplicable;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		InAndOutwardProcessing.CSI_CustomsOffice = "Test";
		AssertHasMessageErrorContaining("When Procedure code is 20 and custom office is not empty show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		InAndOutwardProcessing.CSI_CustomsOffice = ZString.Empty;
		AssertNoMessageErrorContaining("When Procedure code is 20 and custom office is empty not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is 41 and CSI_Procedure is 1 not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is 50 and CSI_Procedure is 1 not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is 50 and CSI_Procedure is not 1 not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is 41 and CSI_Procedure is not 1 not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is not 41 or 50 and CSI_Procedure is not 1 not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);
	});

	public void TestCheckCSI_CustomsOffice_NS30098_Mandatory() => CombineAssertions(() =>
	{
		string expectedMessageError = PassarValidationMessages.MessageNS30098_CustomsOfficeMandatory;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		InAndOutwardProcessing.CSI_CustomsOffice = ZString.Empty;
		AssertHasMessageErrorContaining("When CEI_Procedure code is 41 and CSI_Procedure is 1 and CustomOffice Empty show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertHasMessageErrorContaining("When CEI_Procedure code is 50 and CSI_Procedure is 1 and CustomOffice Empty show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		InAndOutwardProcessing.CSI_CustomsOffice = "Test";
		AssertNoMessageErrorContaining("When CEI_Procedure code is 50 and CSI_Procedure is 1 and CustomOffice not Empty not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure;
		InAndOutwardProcessing.CSI_CustomsOffice = ZString.Empty;
		AssertNoMessageErrorContaining("When CEI_Procedure code is 50 and CSI_Procedure is not 1 and CustomOffice Empty not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		InAndOutwardProcessing.CSI_Procedure = InAndOutwardProcessingProcessTypesEdec.DueProcedure;
		InAndOutwardProcessing.Validation.ValidateCSI_CustomsOffice();
		AssertNoMessageErrorContaining("When CEI_Procedure code is not 41 or 50 and CSI_Procedure is  1 and CustomOffice not Empty not show message error", InAndOutwardProcessing.CSI_CustomsOfficeInfo, expectedMessageError);
	});

	public void TestCheckCSI_Code_NS30098_NotApplicable()
	{
		AssertNS30098_NotApplicable(InAndOutwardProcessing.CSI_CodeInfo);
	}

	public void TestCheckCSI_Procedure_NS30098_NotApplicable()
	{
		AssertNS30098_NotApplicable(InAndOutwardProcessing.CSI_ProcedureInfo);
	}

	public void TestCheckCSI_IssuerType_NS30098_NotApplicable() 
	{
		AssertNS30098_NotApplicable(InAndOutwardProcessing.CSI_IssuerTypeInfo);
	}

	void AssertNS30098_NotApplicable(ZPropertyInfo propertyInfo) => CombineAssertions(() =>
	{
		var expectedMessageError = PassarValidationMessages.MessageNS30098_NotApplicableForProcedure20(propertyInfo.HumanReadableName);
		var value = "X";
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		propertyInfo.SetValueFromString(value);
		AssertHasMessageErrorContaining($"When Procedure code is 20 and {propertyInfo.HumanReadableName} is not empty show message error", propertyInfo, expectedMessageError);

		propertyInfo.SetValueFromString(ZString.Empty);
		AssertNoMessageErrorContaining($"When Procedure code is 20 and {propertyInfo.HumanReadableName} is empty  not show message error", propertyInfo, expectedMessageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		propertyInfo.SetValueFromString(value);
		AssertNoMessageErrorContaining($"When Procedure code is not 20 and {propertyInfo.HumanReadableName} is not empty not show message error", propertyInfo, expectedMessageError);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction entryInstruction;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateNewInvoiceLine();
	JobComInvoiceLine invoiceLine;

	InAndOutwardProcessing InAndOutwardProcessing => inAndOutwardProcessing ??= InvoiceLine.InAndOutwardProcessing;
	InAndOutwardProcessing inAndOutwardProcessing;

	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invLine = InvoiceHeader.InvoiceLines.AddNew();
		invLine.JI_CEI = EntryInstruction.PK;
		return invLine;
	}
}
