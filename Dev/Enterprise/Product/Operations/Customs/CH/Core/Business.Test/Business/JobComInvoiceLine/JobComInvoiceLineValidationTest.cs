using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceLineValidation))]
sealed class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheck_JI_Tariff()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "";
			AssertNoMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}""", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);

			InvoiceLine.JI_Tariff = "12345678123";
			AssertNoMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}""", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);

			InvoiceLine.JI_Tariff = "12345678000";
			AssertNoMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}""", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);

			InvoiceLine.JI_Tariff = "1234567";
			AssertHasMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}"" (incomplete commodity code)", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);

			InvoiceLine.JI_Tariff = "12345678";
			AssertHasMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}"" (no statistical code)", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);

			InvoiceLine.JI_Tariff = "12345681";
			AssertHasMessageErrorContaining($@"When JI_Tariff=""{InvoiceLine.JI_Tariff}"" (incomplete statistical code)", InvoiceLine.JI_TariffInfo, ValidationMessages.JobComInvoiceLine.InvalidTariff);
		});
	}

	public void TestCheck_JI_Tariff_R129()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Condition WeightCheck1: KGM >= 5";
		var tariffWithWGTC1 = testHelper.CreateImportTariffWithConditionClass("07129081026999", UniversalReferenceConstants.CusConditionType.WeightCheck1, "[KGM] >= 5", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = tariffWithWGTC1.ZZ1_TariffCode;
		InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 4;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition not met", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeight = 6;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition met", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			InvoiceLine.JI_NetWeight = 0.004m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Weight not in KG", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetMassConfirmation = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Confirmation set", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheckJI_Tariff_R131a()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Condition WeightCheck2: [KGM]/[NAR] <= 3.5";
		var tariffWithWGTC2 = testHelper.CreateImportTariffWithConditionClass("61101100000000", UniversalReferenceConstants.CusConditionType.WeightCheck2, "[KGM]/[NAR] <= 3.5", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = tariffWithWGTC2.ZZ1_TariffCode;
		InvoiceLine.JI_CustomsThirdQuantity = 10;
		InvoiceLine.JI_CustomsThirdUnitQty = "NAR";

		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 100m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition not met", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeight = 10m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition met", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			InvoiceLine.JI_NetWeight = 0.100m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Weight not in KG", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_AdditionalUnitConfirmation = true;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Confirmation set", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheck_JI_Tariff_R125()
	{
		var formula = "VFS/[KGM] <= 50 & VFS/[KGM] >= 10";
		var testHelper = new RefCusTariffTestHelper(Factory);
		var tariffWithMVC = testHelper.CreateImportTariffWithConditionClass("61101100000000", UniversalReferenceConstants.CusConditionType.MeanValueCheck, formula, "Mean Value Check");

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
		InvoiceLine.JI_Tariff = tariffWithMVC.ZZ1_TariffCode;
		InvoiceLine.JI_LinePrice = 100;

		InvoiceLine.JI_NetWeight = 1;
		AssertHasMessageErrorContaining("MVC: Net weight not met", InvoiceLine.JI_TariffInfo, "condition is not satisfied");

		InvoiceLine.JI_NetWeight = 4;
		AssertNoMessageErrorContaining("MVC: Net weight met", InvoiceLine.JI_TariffInfo, "condition is not satisfied");
	}

	public void TestCheckJI_Tariff_ConditionWeightCheck()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Condition WeightCheck2: [KGM]/[NAR] <= 3.5";
		var tariffWithWGTC2 = tariffTestHelper.CreateExportTariffWithConditionClass("61101100000", UniversalReferenceConstants.CusConditionType.WeightCheck2, "[KGM]/[NAR] <= 3.5", conditionComment: conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.JI_Tariff = tariffWithWGTC2.ZZ1_TariffCode;
		InvoiceLine.JI_CustomsThirdQuantity = 10;
		InvoiceLine.JI_CustomsThirdUnitQty = "NAR";
		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 10m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition met", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeight = 100m;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition not met", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheckJI_Tariff_R193()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		tariffTestHelper.CreateImportTariff(UniversalReferenceConstants.Tariffs.NegligibleImportTariff);
		tariffTestHelper.CreateImportTariff(UniversalReferenceConstants.Tariffs.BeginOfIndustrialTariffs);
		tariffTestHelper.CreateExportTariff(UniversalReferenceConstants.Tariffs.NegligibleExportTariff);

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			InvoiceLine.JI_Tariff = UniversalReferenceConstants.Tariffs.NegligibleImportTariff;
			AssertHasMessageErrorContaining("Tariff not ok", InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR193);

			InvoiceLine.JI_Tariff = UniversalReferenceConstants.Tariffs.BeginOfIndustrialTariffs;
			AssertNoMessageErrorContaining($"Tariff ok for JI_Tariff '{InvoiceLine.JI_Tariff}'", InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR193);

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.ReturnedGoodsExport;
			InvoiceLine.JI_Tariff = UniversalReferenceConstants.Tariffs.NegligibleImportTariff;
			AssertNoMessageErrorContaining($"Tariff ok for JI_Procedure '{InvoiceLine.JI_Procedure}'", InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR193);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.RefinementTransportation;
			InvoiceLine.JI_Tariff = UniversalReferenceConstants.Tariffs.NegligibleExportTariff;
			AssertNoMessageErrorContaining($"Tariff ok for {Declaration.JE_MessageType}", InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR193);
		});
	}

	public void TestCheck_JI_Tariff_R336()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = UniversalReferenceConstants.TariffNumbers.SmokingTobaccoOther + "000999";
		var tobacco = InvoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = "3";

		CombineAssertions(() =>
		{
			Assert_JI_Tariff_R336("02", null, null, "Missing SOTA and Prevention additional taxes", true);
			Assert_JI_Tariff_R336("03", null, null, "Missing SOTA and Prevention additional taxes", true);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465002, null, "Missing Prevention additional taxes", true);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465202, null, "Missing Prevention additional taxes", true);
			Assert_JI_Tariff_R336("03", null, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470002, "Missing SOTA additional taxes", true);
			Assert_JI_Tariff_R336("03", null, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470202, "Missing SOTA additional taxes", true);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465002, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470002, "SOTA and Prevention additional taxes provided", false);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465002, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470202, "SOTA and Prevention additional taxes provided", false);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465202, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470002, "SOTA and Prevention additional taxes provided", false);
			Assert_JI_Tariff_R336("03", UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff465202, UniversalReferenceConstants.AdditionalTaxesTariffs.Tariff470202, "SOTA and Prevention additional taxes provided", false);
		});

		void Assert_JI_Tariff_R336(string tobaccoSubGroup, string additionalTaxSOTATariff, string additionalTaxPreventionTariff, string assertionMessage, bool expectedError)
		{
			InvoiceLine.AdditionalTaxes.RemoveAndDeleteAll();
			tobacco.CSI_SubType = tobaccoSubGroup;

			if (!string.IsNullOrEmpty(additionalTaxSOTATariff))
			{
				var additionalTaxSOTA = InvoiceLine.AdditionalTaxes.AddNew();
				additionalTaxSOTA.BZ_Tariff = additionalTaxSOTATariff;
			}

			if (!string.IsNullOrEmpty(additionalTaxPreventionTariff))
			{
				var additionalTaxPrevention = InvoiceLine.AdditionalTaxes.AddNew();
				additionalTaxPrevention.BZ_Tariff = additionalTaxPreventionTariff;
			}

			InvoiceLine.Validation.ValidateJI_Tariff();

			if (expectedError)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR336);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_FormattedTariffInfo, ValidationMessages.Plausi.MessageR336);
			}
		}
	}

	public void TestCheckJI_NetWeight_E018()
	{
		const string conditionValue = "[KGM] >= 5";
		const string conditionComment = "Condition comment " + conditionValue;

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffWithWGTC1 = tariffTestHelper.CreateExportTariffWithConditionClass(RefCusTariffTestHelper.ExportTariffHay, CusConditionType.WeightCheck1, conditionValue, conditionComment: conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.JI_Tariff = tariffWithWGTC1.ZZ1_TariffCode;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 5.001m;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Weight ok", InvoiceLine.JI_TariffInfo, conditionComment);

			InvoiceLine.JI_NetWeight = 4999m;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Weight too small", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheck_JI_Tariff_ConditionValueTypePRM()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Presentation of a permit when";
		var tariffWithPA1PRM = testHelper.CreateImportTariffWithCondition("07052971000000", Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.FederalOfficeForAgriculture, UniversalReferenceConstants.CusConditionValueType.Permit, "1", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = tariffWithPA1PRM.ZZ1_TariffCode;

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("Condition Type: PA1 FOAG - Federal Office for Agriculture / no permit", InvoiceLine.JI_TariffInfo, conditionComment);

			var permit = InvoiceLine.Permits.AddNew();
			permit.CSI_Code = "1";
			permit.CSI_IssuerType = "1";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition Type: PA1 FOAG - Federal Office for Agriculture / valid permit", InvoiceLine.JI_TariffInfo, conditionComment);

			permit.CSI_IssuerType = "2";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition Type: PA1 FOAG - Federal Office for Agriculture / invalid permit", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheck_JI_Tariff_ConditionValueTypeNCL()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Non-customs laws check";
		var tariffWith270NCL = testHelper.CreateImportTariffWithCondition("01061200000000", Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.PlantHealth, UniversalReferenceConstants.CusConditionValueType.NonCustomsLaw, "270", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = tariffWith270NCL.ZZ1_TariffCode;

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("Condition Type: N270 Plant health / no NCL", InvoiceLine.JI_TariffInfo, conditionComment);

			var ncl = InvoiceLine.NonCustomsLaws.AddNew();
			ncl.CSI_Code = "270";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition Type: N270 Plant health / valid NCL", InvoiceLine.JI_TariffInfo, conditionComment);

			ncl.CSI_Code = "66";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition Type: N270 Plant health / invalid NCL", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheck_JI_Tariff_ConditionValueTypeRST()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		const string conditionComment = "Restriction required";
		var tariffWith399RST = testHelper.CreateExportTariffWithCondition("05040039000000", Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.PermitAuthorityPrefix, UniversalReferenceConstants.CusConditionValueType.Restriction, "137", conditionComment);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.JI_Tariff = tariffWith399RST.ZZ1_TariffCode;

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("Condition Type: Permit Authority PA / no RST", InvoiceLine.JI_TariffInfo, conditionComment);

			var rst = InvoiceLine.Restrictions.AddNew();
			rst.CSI_Code = "137";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertNoMessageErrorContaining("Condition Type: Permit Authority PA / valid RST", InvoiceLine.JI_TariffInfo, conditionComment);

			rst.CSI_Code = "206";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("Condition Type: Permit Authority PA / invalid RST", InvoiceLine.JI_TariffInfo, conditionComment);
		});
	}

	public void TestCheck_JI_Tariff_R162_R201()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var prohibitedProcedures = new[] { ProcedureCodesEdec.NormalDuty, ProcedureCodesEdec.Tobacco };
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var customsReliefTariffs = new[]
		{
				tariffTestHelper.CreateImportTariff(Tariffs.NegligibleImportTariff),
				tariffTestHelper.CreateImportTariff("10000000111000")
			};
		var normalTariff = tariffTestHelper.CreateImportTariff("10000000000000");

		foreach (var prohibitedProcedure in prohibitedProcedures)
		{
			InvoiceLine.JI_Procedure = prohibitedProcedure;

			foreach (var customsReliefTariff in customsReliefTariffs)
			{
				InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
				InvoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR162R201);
			}
		}

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.ReturnedGoodsExport;
		InvoiceLine.JI_Tariff = customsReliefTariffs.First().ZZ1_TariffCode;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR162R201);

		InvoiceLine.JI_Procedure = prohibitedProcedures.First();
		InvoiceLine.JI_Tariff = normalTariff.ZZ1_TariffCode;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR162R201);

		string GetMessage() => $"Procedure - \"{InvoiceLine.JI_Procedure}\", Tariff code - \"{InvoiceLine.JI_Tariff}\"";
	}

	public void TestCheck_JI_Tariff_R175_R179()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var prohibitedProcedures = new[]
		{
				ProcedureCodesEdec.CustomsRelief, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT
			};
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var customsReliefTariff = tariffTestHelper.CreateImportTariff(Tariffs.NegligibleImportTariff);
		var normalTariff = tariffTestHelper.CreateImportTariff("10000000000000");

		foreach (var prohibitedProcedure in prohibitedProcedures)
		{
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR175R179);
		}

		InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
		InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR175R179);

		InvoiceLine.JI_Procedure = prohibitedProcedures.First();
		InvoiceLine.JI_Tariff = normalTariff.ZZ1_TariffCode;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR175R179);

		string GetMessage() => $"Procedure - \"{InvoiceLine.JI_Procedure}\", Tariff code - \"{InvoiceLine.JI_Tariff}\"";
	}

	public void TestCheck_JI_Procedure_R176()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var helper = new RefCusTariffTestHelper(Factory);
		var tariffWithCustomsFavourHintCodeIs2 = helper.CreateImportTariffWithAttribute("12340000000002", TariffAttributes.CustomsFavourHintCode, TariffAttributes.Values._2);
		var tariffWithCustomsFavourHintCodeIs1 = helper.CreateImportTariffWithAttribute("12340000000001", TariffAttributes.CustomsFavourHintCode, TariffAttributes.Values._1);
		var tariffWithoutCustomsFavourHintCode = helper.CreateImportTariff("43210000000000");
		var notAllowedProcedure = ProcedureCodesEdec.NormalDuty;
		var allowedProcedures = new[]
		{
				ProcedureCodesEdec.RefinementTransportation, ProcedureCodesEdec.RepairTransportation,
				ProcedureCodesEdec.CustomsRelief, ProcedureCodesEdec.ReturnedGoods, ProcedureCodesEdec.ReturnedGoodsVAT
			};

		InvoiceLine.JI_Tariff = tariffWithCustomsFavourHintCodeIs2.ZZ1_TariffCode;
		InvoiceLine.JI_Procedure = notAllowedProcedure;
		AssertHasMessageError(GetMessage(), InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR176);

		InvoiceLine.JI_Tariff = tariffWithCustomsFavourHintCodeIs1.ZZ1_TariffCode;
		InvoiceLine.JI_Procedure = notAllowedProcedure;
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR176);

		InvoiceLine.JI_Tariff = tariffWithoutCustomsFavourHintCode.ZZ1_TariffCode;
		InvoiceLine.JI_Procedure = notAllowedProcedure;
		AssertNoMessageError(GetMessage(), InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR176);

		foreach (var allowedProcedure in allowedProcedures)
		{
			InvoiceLine.JI_Tariff = tariffWithCustomsFavourHintCodeIs2.ZZ1_TariffCode;
			InvoiceLine.JI_Procedure = allowedProcedure;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR176);
		}

		string GetMessage() => $"Procedure: {InvoiceLine.JI_Procedure}";
	}

	public void TestCheckJI_Procedure_R359() => CombineAssertions(() =>
	{
		var messageR359 = ValidationMessages.Plausi.MessageR359;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RepairTransportation;
		AssertHasMessageError(InvoiceLine.JI_ProcedureInfo, messageR359);
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
		AssertNoMessageError(InvoiceLine.JI_ProcedureInfo, messageR359);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.RepairTransportation;
		AssertNoMessageError(InvoiceLine.JI_ProcedureInfo, messageR359);
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
		AssertNoMessageError(InvoiceLine.JI_ProcedureInfo, messageR359);
	});

	public void TestCheck_JI_Tariff_R177()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.CustomsRelief;
			InvoiceLine.JI_Tariff = "85078000040911";
			AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR177);

			InvoiceLine.JI_Tariff = "85078000000911";
			AssertHasMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR177);

			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR177);
		});
	}

	public void TestCheck_JI_Tariff_R256() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var permit = InvoiceLine.Permits.AddNew();
		permit.CSI_Code = UniversalReferenceConstants.PermitCodes.GeneralEPermit;
		permit.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.COE;

		var permitRevers = Factory.New<Permit>();
		permitRevers.CSI_Code = UniversalReferenceConstants.PermitCodes.ReversTobacco;
		permitRevers.CSI_IssuerType = UniversalReferenceConstants.PermitAuthorityCodes.STB;

		var tariffCodesListWithStatisticalCode999 = new string[] { "24021000000999", "24022010000999", "24022020000999", "24029000000999",
				"24031100000999", "24031900000999", "24039910000999", "24039990000999" };
		foreach (var tariff in tariffCodesListWithStatisticalCode999)
		{
			InvoiceLine.JI_Tariff = tariff;
			AssertHasMessageErrorContaining($"No ReversTabacco Permit on Tariff: {InvoiceLine.JI_Tariff}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);
		}
		InvoiceLine.JI_Tariff = "12345678000999";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} is not relevant for revers tabacco", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_Tariff = "24021000000123";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} does not contain statistical code of 999", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.Permits.Add(permitRevers);
		InvoiceLine.JI_Tariff = "24021000000999";
		AssertNoMessageErrorContaining($"Invoice Line with Tariff: {InvoiceLine.JI_Tariff} contains revers tabacco permit", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.Permits.Remove(permitRevers);
		var tariffCodesListStarting2401 = new string[] { "24011010000", "24012010000", "24013010000" };
		var statisticalCodeList = new string[] { "011", "012", "013" };
		InvoiceLine.JI_CustomsSecondQuantity = 2.6m;
		foreach (var (tariff, statCode) in tariffCodesListStarting2401.SelectMany(tariff => statisticalCodeList.Select(statCode => (tariff, statCode))))
		{
			InvoiceLine.JI_Tariff = tariff + statCode;
			AssertHasMessageErrorContaining($"No ReversTabacco Permit on Tariff: {InvoiceLine.JI_Tariff}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);
		}
		InvoiceLine.JI_Tariff = "24011011000011";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} is not relevant for revers tabacco", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_Tariff = "24012010000014";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} does not contain any of the following statistical codes: 011, 012 or 013", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_CustomsSecondQuantity = 2.5m;
		InvoiceLine.JI_Tariff = "24013010000012";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} with the CustomsSecondQuantity lower then 2.6 does not return R356", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_CustomsSecondQuantity = 2.6m;
		InvoiceLine.Permits.Add(permitRevers);
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageErrorContaining($"Invoice Line with Tariff: {InvoiceLine.JI_Tariff} contains revers tabacco permit", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.Permits.Remove(permitRevers);
		var tariffCodesListStarting2403 = new string[] { "24039100", "24039940" };
		InvoiceLine.JI_CustomsSecondQuantity = 5.1m;
		foreach (var tariff in tariffCodesListStarting2403)
		{
			InvoiceLine.JI_Tariff = tariff;
			AssertHasMessageErrorContaining($"No ReversTabacco Permit on Tariff: {InvoiceLine.JI_Tariff}", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);
		}
		InvoiceLine.JI_Tariff = "24039101";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} is not relevant for revers tabacco", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_CustomsSecondQuantity = 5m;
		InvoiceLine.JI_Tariff = "24039100";
		AssertNoMessageErrorContaining($"Tariff: {InvoiceLine.JI_Tariff} with the CustomsSecondQuantity lower then 5 does not return R356", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);

		InvoiceLine.JI_CustomsSecondQuantity = 5.1m;
		InvoiceLine.Permits.Add(permitRevers);
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageErrorContaining($"Invoice Line with Tariff: {InvoiceLine.JI_Tariff} contains revers tabacco permit", InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR256);
	});

	public void TestCheckSpecialMentions()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			SpecialMentionsValidationTestHelper.TestCheckSpecialMentions(InvoiceLine.SpecialMentionsInfo);
		});
	}

	public void TestCheckJI_PrimaryReference()
	{
		RefCusCodeTestHelper.CreatePrimaryPreferenceCodeList(Factory);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_PrimaryPreferenceInfo, RefCusCodeTestHelper.InvalidPrimaryPreferenceCode, RefCusCodeTestHelper.ValidPrimaryPreferenceCode);
		});
	}

	public void TestCheckJI_PrimaryReference_R166()
	{
		RefCusCodeTestHelper.CreatePrimaryPreferenceCodeList(Factory);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine1.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		instruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Provisional;

		RefCusCodeTestHelper.CreatePreasCodeList(Factory);

		CombineAssertions(() =>
		{
			instruction.CEI_DeclarationReason = "1";
			AssertNoMessageError("", instruction.CEI_DeclarationReasonInfo, ValidationMessages.Plausi.MessageR166c);

			invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			instruction.CEI_DeclarationReason = "1";
			AssertHasMessageError("Validation Check R166", invoiceLine2.JI_PrimaryPreferenceInfo, ValidationMessages.Plausi.MessageR166c);

			instruction.CEI_DeclarationReason = "2";
			AssertHasMessageError("Validation Check R166", invoiceLine2.JI_PrimaryPreferenceInfo, ValidationMessages.Plausi.MessageR166c);

			instruction.CEI_DeclarationReason = "3";
			AssertHasMessageError("Validation Check R166", invoiceLine2.JI_PrimaryPreferenceInfo, ValidationMessages.Plausi.MessageR166c);

			instruction.CEI_DeclarationReason = "4";
			AssertHasMessageError("Validation Check R166", invoiceLine2.JI_PrimaryPreferenceInfo, ValidationMessages.Plausi.MessageR166c);
		});
	}

	public void TestCheckJI_PrimaryPreference_R290()
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
		InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
		var headerDocument = InvoiceHeader.SupportingDocuments.AddNew();
		var lineDocument = InvoiceLine.SupportingDocuments.AddNew();

		var certificateCodes = RefCusCodeListLoader.GetGSPCertificateCodes(Factory, ZDateTime.Today).CodesAsString;
		var messageR290_1 = ValidationMessages.Plausi.GetMessageR290_1(certificateCodes);

		CombineAssertions(() =>
		{
			AssertHasMessageError("When no document", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);

			headerDocument.CSI_Code = RefCusCodeTestHelper.ValidSupportingDocument101;
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError($"GSP Certificate on header", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);
			headerDocument.CSI_Code = ZString.Empty;

			headerDocument.CSI_Code = RefCusCodeTestHelper.ValidSupportingDocument101;
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError($"GSP Certificate on line", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);
			lineDocument.CSI_Code = ZString.Empty;

			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError("When  not PR", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);

			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError("When non-development country", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);

			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			InvoiceLine.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.Validation.ValidateJI_PrimaryPreference();
			AssertNoMessageError("When not import", InvoiceLine.JI_PrimaryPreferenceInfo, messageR290_1);
		});
	}

	public void TestCheckJI_CountryOfOrigin_R275()
	{
		RefCusCodeTestHelper.CreateCountryOfOriginsList(Factory);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_CountryOfOriginInfo, RefCusCodeTestHelper.InvalidCountryOfOriginCode, RefCusCodeTestHelper.ValidCountryOfOriginCode);
		});
	}

	public void TestCheckJI_CountryOfOrigin_R173()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
			AssertHasMessageError($"Origin: {InvoiceLine.JI_CountryOfOrigin}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_CountryOfOriginInfo, ValidationMessages.Plausi.MessageR173);

			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			AssertNoMessageError($"Origin: {InvoiceLine.JI_CountryOfOrigin}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_CountryOfOriginInfo, ValidationMessages.Plausi.MessageR173);

			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertHasMessageError($"Origin: {InvoiceLine.JI_CountryOfOrigin}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_CountryOfOriginInfo, ValidationMessages.Plausi.MessageR173);

			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			AssertNoMessageError($"Origin: {InvoiceLine.JI_CountryOfOrigin}; Procedure: {InvoiceLine.JI_Procedure}", InvoiceLine.JI_CountryOfOriginInfo, ValidationMessages.Plausi.MessageR173);
		});
	}

	public void TestCheckJI_PrimaryReferenceRule158()
	{
		RefCusCodeTestHelper.CreateProcedureCodeList(Factory);
		RefCusCodeTestHelper.CreateOriginDocumentCodes(Factory);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		var validOriginDocumentCodes = new SupportingDocumentLookups(InvoiceLine.SupportingDocuments.AddNew()).ValidOriginDocumentCodes;
		var messageR158 = ValidationMessages.Plausi.GetMessageR158(validOriginDocumentCodes.CodesAsString);

		InvoiceLine.SupportingDocuments.RemoveAll();
		AssertHasMessageErrorContaining("PR, no Invoice document, no InvoiceHeader document", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);

		var supportingDocumentInvoiceLine = InvoiceLine.SupportingDocuments.AddNew();
		var supportingDocumentInvoiceHeader = InvoiceHeader.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			foreach (var originDocumentCode in validOriginDocumentCodes.GetAllCodes())
			{
				supportingDocumentInvoiceLine.CSI_Code = originDocumentCode;
				supportingDocumentInvoiceHeader.CSI_Code = originDocumentCode;
				InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
				AssertNoMessageError("Invoice Header: Primary Preference Code PR Origin Document valid", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);
			}

			supportingDocumentInvoiceLine.CSI_Code = "XXX";
			supportingDocumentInvoiceHeader.CSI_Code = "3";
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertNoMessageErrorContaining("PR, Invoice Line: XXX, InvoiceHeader: 3", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);

			supportingDocumentInvoiceLine.CSI_Code = "3";
			supportingDocumentInvoiceHeader.CSI_Code = "3";
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertNoMessageErrorContaining("PR, Invoice Line: 3, InvoiceHeader: 3", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);

			supportingDocumentInvoiceLine.CSI_Code = "3";
			supportingDocumentInvoiceHeader.CSI_Code = "XXX";
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertNoMessageErrorContaining("PR, Invoice Line: 3, InvoiceHeader: XXX", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);

			supportingDocumentInvoiceLine.CSI_Code = "XXX";
			supportingDocumentInvoiceHeader.CSI_Code = "XXX";
			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;
			AssertHasMessageErrorContaining("PR, Invoice Line: 3, InvoiceHeader: XXX", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);

			InvoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
			AssertNoMessageErrorContaining("NT, Invoice Line: XXX, InvoiceHeader: XXX", InvoiceLine.JI_PrimaryPreferenceInfo, messageR158);
		});
	}

	public void TestCheckJI_Procedure_Import() => AssertJI_Procedure(Common.Shared.SharedJobMessageTypeList.Codes.Import, false, RefCusCodeTestHelper.InvalidProcedureCode, RefCusCodeTestHelper.ValidImportProcedureCode);

	public void TestCheckJI_Procedure_Export() => AssertJI_Procedure(Common.Shared.SharedJobMessageTypeList.Codes.Export, true, RefCusCodeTestHelper.InvalidProcedureCode, RefCusCodeTestHelper.ValidExportProcedureCode);

	void AssertJI_Procedure(string messageType, bool mandatory, string invalidProcedureCode, string validProcedureCode)
	{
		RefCusCodeTestHelper.CreateProcedureCodeList(Factory);
		Declaration.JE_MessageType = messageType;

		CombineAssertions(() =>
		{
			if (mandatory)
			{
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_ProcedureInfo, invalidProcedureCode, validProcedureCode);
			}
			else
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_ProcedureInfo, invalidProcedureCode, validProcedureCode);
			}
		});
	}

	public void TestCheckJI_Procedure_R183a()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;

		CombineAssertions(() =>
		{
			ResetToValidValues();
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.CustomsRelief;
			AssertNoMessageError("JI_Procedure != 08", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.ExemptFromDuty;
			AssertNoMessageError("Valid values", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_ZZF_NKTaxType = TaxCodes.StandardRate;
			AssertHasMessageError("JI_ZZF_NKTaxType != 3", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_Weight = 0;
			AssertHasMessageError("JI_Weight = 0", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_LinePrice = 0;
			AssertHasMessageError("JI_Calc_StatisticalValue == 0", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_NonTradingGoods = false;
			InvoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("JI_NonTradingGoods = false", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			ResetToValidValues();
			InvoiceLine.JI_RateOverride = false;
			InvoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("JI_RateOverride = false", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);

			InvoiceLine.JI_RateOverride = true;
			InvoiceLine.JI_OverriddenRate = 1;
			InvoiceLine.Validation.ValidateJI_Procedure();
			AssertHasMessageError("JI_RateOverride = True and JI_OverriddenRate > 0", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183a);
		});

		void ResetToValidValues()
		{
			InvoiceLine.JI_ZZF_NKTaxType = TaxCodes.ExemptVat;
			InvoiceLine.JI_Weight = 1;
			InvoiceLine.JI_LinePrice = 1;
			InvoiceLine.JI_NonTradingGoods = true;
			InvoiceLine.JI_RateOverride = true;
			InvoiceLine.JI_OverriddenRate = 0;
		}
	}

	public void TestCheckJI_Procedure_R183b()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.ExemptFromDuty;

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);

		CombineAssertions(() =>
		{
			ResetToValidValues();
			AssertNoMessageError("Valid values", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);

			ResetToValidValues();
			var tariffWithCostomsFavourCode = tariffTestHelper.CreateImportTariffWithSingleRate("123400000123000");
			InvoiceLine.JI_Tariff = tariffWithCostomsFavourCode.ZZ1_TariffCode;
			AssertHasMessageError("CostomsFavourCode != 000", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);

			var tariffWithDutyRateFormulaNumber = tariffTestHelper.CreateImportTariffWithSingleRate("1234000000001230");
			InvoiceLine.JI_Tariff = tariffWithDutyRateFormulaNumber.ZZ1_TariffCode;
			AssertHasMessageError("DutyRateFormulaNumber != 000", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);

			ResetToValidValues();
			InvoiceLine.JI_WeightIncludingInnerPackage = 1;
			AssertHasMessageError("JI_WeightIncludingInnerPackage > 0", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);

			ResetToValidValues();
			InvoiceLine.JI_TareSupplementPercentage = 1;
			AssertHasMessageError("JI_TareSupplementPercentage > 0", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);

			ResetToValidValues();
			InvoiceLine.JI_StorageType = RefCusCodeTestHelper.ValidStorageTypeCode;
			AssertHasMessageError("JI_StorageType != null", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR183b);
		});

		void ResetToValidValues()
		{
			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.JI_WeightIncludingInnerPackage = 0;
			InvoiceLine.JI_TareSupplementPercentage = 0;
			InvoiceLine.JI_StorageType = ZString.Empty;
		}
	}

	public void TestCheckJIProcedure_Code11Only()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = Declaration.Invoices.AddNew();

		CombineAssertions(() =>
		{
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
			AssertNoMessageError("Check Invoice Lines Code 11", invoiceLine1.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR165);

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "XX";
			AssertNoMessageError("Check Invoice Lines Code 11", invoiceLine2.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR165);

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertHasMessageError("Check Invoice Lines Code 11", invoiceLine2.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR165);
		});
	}

	public void TestCheckJI_Procedure_R353()
	{
		AssertCheckJI_Procedure_R353(ProcedureCodesEdec.ReturnedGoods);
		AssertCheckJI_Procedure_R353(ProcedureCodesEdec.ReturnedGoodsVAT);

		void AssertCheckJI_Procedure_R353(string procedure)
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				InvoiceLine.InAndOutwardProcessingRepair = true;
				InvoiceLine.JI_Procedure = procedure;
				AssertHasMessageErrorContaining($"Procedure={procedure} and Repair is ticked", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR353);

				InvoiceLine.JI_Procedure = ProcedureCodesEdec.RepairTransportation;
				AssertNoMessageErrorContaining("Procedure != (10 OR 11) and Repair is ticked", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR353);

				InvoiceLine.InAndOutwardProcessingRepair = false;
				InvoiceLine.JI_Procedure = procedure;
				AssertNoMessageErrorContaining($"Procedure={procedure} and Repair is unticked", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR353);

				InvoiceLine.InAndOutwardProcessingRepair = true;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				InvoiceLine.JI_Procedure = procedure;
				AssertNoMessageErrorContaining($"Export Declaration, Procedure={procedure} and Repair is ticked", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR353);
			});
		}
	}

	public void TestCheckJI_Weight_R127()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 0;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_Weight = 100;
			AssertNoMessageError("When NetMass=0, Message error shouldn't be displayed", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);

			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			InvoiceLine.JI_NetWeight = 10;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_GrossMassConfirmation = false;

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight - 1;
			AssertHasMessageError("NetMass<=10kg, GrossMass too low", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight + 1;
			AssertNoMessageError("NetMass<=10kg, GrossMass ok", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight * 25 + 1;
			AssertHasMessageError("NetMass<=10kg, GrossMass too high", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);

			InvoiceLine.JI_GrossMassConfirmation = true;
			AssertNoMessageError("NetMass<=10kg, high GrossMass confirmed", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);

			InvoiceLine.JI_NetWeight = 11;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_GrossMassConfirmation = false;

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight - 1;
			AssertHasMessageError("NetMass>10kg, GrossMass too low", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_2);

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight + 1;
			AssertNoMessageError("NetMass>10kg, GrossMass ok", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_2);

			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight * 2.5m + 1;
			AssertHasMessageError("NetMass>10kg, GrossMass too high", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_2);

			InvoiceLine.JI_GrossMassConfirmation = true;
			AssertNoMessageError("NetMass>10kg, high GrossMass confirmed", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_2);

			InvoiceLine.JI_GrossMassConfirmation = false;
			InvoiceLine.JI_NetWeight = 10;
			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			InvoiceLine.JI_Weight = InvoiceLine.JI_NetWeight + 1;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertHasMessageError("NetWeightUQ considered", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_2);

			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertHasMessageError("WeightUQ considered", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR127_1);
		});
	}

	public void TestCheckJI_WeightUOM()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_WeightUQInfo, "@@", "KG");
	}

	public void TestCheckJI_NetWeight_R128()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var helper = new RefCusTariffTestHelper(Factory);
		var tariffWithOutQC1 = RefCusTariffTestHelper.ExportTariffHay;
		var tariff1QC1 = RefCusTariffTestHelper.ExportTariffGardenUmbrellas;
		var tariff3QC1 = RefCusTariffTestHelper.ExportTariffFruitJuice;
		helper.CreateImportTariff(tariffWithOutQC1);
		helper.CreateImportTariffWithAttribute(tariff1QC1, CH.Business.UniversalReferenceConstants.TariffAttributes.QuantityCode1, CH.Business.UniversalReferenceConstants.TariffAttributes.Values._1);
		helper.CreateImportTariffWithAttribute(tariff3QC1, CH.Business.UniversalReferenceConstants.TariffAttributes.QuantityCode1, CH.Business.UniversalReferenceConstants.TariffAttributes.Values._3);
		Factory.Save();

		var errorMessage = ValidationMessages.Plausi.MessageNotEntered(ValidationMessages.Plausi.R128, InvoiceLine.JI_NetWeightInfo.HumanReadableName);

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "123456789012";
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("Invalid tariff - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariffWithOutQC1;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("no QuantityCode1 - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariff3QC1;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("QuantityCode1 3 - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariff1QC1;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertHasMessageError("QuantityCode1 1 - mandatory - empty", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariff1QC1;
			InvoiceLine.JI_NetWeight = 100;
			AssertNoMessageError("QuantityCode1 1 - mandatory - entered", InvoiceLine.JI_NetWeightInfo, errorMessage);
		});
	}

	public void TestCheckJI_Weight_R267()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariff_PRKGMG = tariffTestHelper.CreateImportTariff("10000000000000");
		tariffTestHelper.AddRate(tariff_PRKGMG, PrimaryPreferenceCodes.PreferentialTariff, "2 * [KGMG]");
		var tariff_PRKGM = tariffTestHelper.CreateImportTariff("20000000000000");
		tariffTestHelper.AddRate(tariff_PRKGM, PrimaryPreferenceCodes.PreferentialTariff, "2 * [KGM]");
		var tariff_NTKGMG = tariffTestHelper.CreateImportTariff("30000000000000");
		tariffTestHelper.AddRate(tariff_NTKGMG, PrimaryPreferenceCodes.NormalTariff, "2 * [KGMG]");
		var tariff_NTKGM = tariffTestHelper.CreateImportTariff("40000000000000");
		tariffTestHelper.AddRate(tariff_NTKGM, PrimaryPreferenceCodes.NormalTariff, "2 * [KGM]");
		var tariff_PR0_NTKGMG = tariffTestHelper.CreateImportTariff("50000000000000");
		tariffTestHelper.AddRate(tariff_PR0_NTKGMG, PrimaryPreferenceCodes.PreferentialTariff, "0");
		tariffTestHelper.AddRate(tariff_PR0_NTKGMG, PrimaryPreferenceCodes.NormalTariff, "2 * [KGMG]");
		var tariff_PR0_NTKGM = tariffTestHelper.CreateImportTariff("60000000000000");
		tariffTestHelper.AddRate(tariff_PR0_NTKGM, PrimaryPreferenceCodes.PreferentialTariff, "0");
		tariffTestHelper.AddRate(tariff_PR0_NTKGM, PrimaryPreferenceCodes.NormalTariff, "2 * [KGM]");

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;

		void TestR267(bool messageExpected, TariffView tariff, string primaryPreference)
		{
			InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			InvoiceLine.JI_PrimaryPreference = primaryPreference;
			InvoiceLine.JI_GrossMassConfirmation = false;
			InvoiceLine.JI_Weight = 0;
			var assertionMessage = $"MessageType={Declaration.JE_MessageType} Preference={primaryPreference} Tariff={tariff.ZZ1_TariffCode}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR267);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR267);
			}
		}

		CombineAssertions(() =>
		{
			TestR267(true, tariff_PRKGMG, PrimaryPreferenceCodes.PreferentialTariff);
			InvoiceLine.JI_GrossMassConfirmation = true;
			AssertNoMessageError("Should be triggered by JI_GrossMassConfirmation", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR267);

			TestR267(true, tariff_NTKGMG, PrimaryPreferenceCodes.NormalTariff);
			InvoiceLine.JI_Weight = 2;
			AssertNoMessageError("Should be triggered by JI_Weight", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR267);

			TestR267(false, tariff_PRKGM, PrimaryPreferenceCodes.PreferentialTariff);
			TestR267(false, tariff_NTKGM, PrimaryPreferenceCodes.NormalTariff);
			TestR267(true, tariff_PR0_NTKGMG, PrimaryPreferenceCodes.PreferentialTariff);
			TestR267(false, tariff_PR0_NTKGM, PrimaryPreferenceCodes.PreferentialTariff);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			TestR267(false, tariff_PRKGMG, PrimaryPreferenceCodes.PreferentialTariff);
		});
	}

	public void TestCheckJI_Weight_NP70009()
	{
		RefCusTradeGroupTestHelper.CreateTestNCL0147CountryList(Factory);
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		var errorMessage = PassarValidationMessages.MessageNP70009_Weight;

		CombineAssertions(() =>
		{
			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
			InvoiceLine.JI_Weight = 5001;
			AssertNoMessageError("if is not Simplified and not in NCL0147CountryList but Weight sum > 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError("if is not Simplified but in NCL0147CountryList and Weight sum > 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError("if is Simplified and not in NCL0147CountryList and Weight sum > 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError("if is Simplified and in NCL0147CountryList and Weight sum > 5000 error", InvoiceLine.JI_WeightInfo, errorMessage);

			InvoiceLine.JI_Weight = 5000;
			AssertNoMessageError("if is Simplified and in NCL0147CountryList but Weight sum <= 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError("if is Simplified and not in NCL0147CountryList and Weight sum <= 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError("if is not Simplified but in NCL0147CountryList and Weight sum <= 5000 no error", InvoiceLine.JI_WeightInfo, errorMessage);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
			InvoiceLine.JI_Weight = 5001;
			AssertNoMessageError("NP70009 is not enabled for EDA", InvoiceLine.JI_WeightInfo, errorMessage);
		});
	}

	public void TestCheckJI_NetWeightUQ()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_NetWeight = 1;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_NetWeightUQInfo, "@@", "KG");
	}

	public void TestCheckJI_CustomsQuantity()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CustomsQuantity = -1;
		AssertNoNotifications(InvoiceLine.JI_CustomsQuantityInfo);
	}

	public void TestCheckJI_CustomsUnitQty()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CustomsUnitQty = "@@";
		AssertNoNotifications(InvoiceLine.JI_CustomsQuantityInfo);
	}

	public void TestCheckJI_CustomsSecondQuantity()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertErrorIfValueIsNegative(InvoiceLine.JI_CustomsSecondQuantityInfo);
	}

	public void TestCheckJI_CustomsSecondUnitQty()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CustomsSecondUnitQty = "@@";
		AssertNoNotifications(InvoiceLine.JI_CustomsSecondUnitQtyInfo);
	}

	public void TestCheckJI_CustomsThirdQuantity()
	{
		var messageType = SharedJobMessageTypeList.Codes.Import;
		var tariffCodeWithUOM3 = RefCusTariffTestHelper.ImportTariffBycycle;
		var tariffCodeWithoutUOM3 = RefCusTariffTestHelper.ImportTariffCodeDuck;

		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateTariffWithUOMs(tariffCodeWithUOM3, messageType, "KGMG", "KGM", "NAR");
		helper.CreateTariffWithUOMs(tariffCodeWithoutUOM3, messageType);

		Declaration.JE_MessageType = messageType;
		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = tariffCodeWithUOM3;

			InvoiceLine.JI_CustomsThirdQuantity = 0;
			AssertHasMessageError("ThirdQuantity required for CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, ValidationMessages.JobComInvoiceLine.AdditionalQuantityMissing);

			InvoiceLine.JI_CustomsThirdQuantity = 1;
			AssertNoMessageError("ThirdQuantity provided for CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, ValidationMessages.JobComInvoiceLine.AdditionalQuantityMissing);

			InvoiceLine.JI_Tariff = tariffCodeWithoutUOM3;
			InvoiceLine.JI_CustomsThirdQuantity = 0;
			AssertNoMessageError("Tariff without CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, ValidationMessages.JobComInvoiceLine.AdditionalQuantityMissing);
		});
	}

	public void TestCheckJI_CustomsThirdQuantity_NP70097()
	{
		var messageType = SharedJobMessageTypeList.Codes.Export;
		var tariffCodeWithUOM3 = RefCusTariffTestHelper.ExportTariffGardenUmbrellas;
		var tariffCodeWithoutUOM3 = RefCusTariffTestHelper.ExportTariffStraw;

		var messageError = PassarValidationMessages.MessageNP70097;

		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateTariffWithUOMs(tariffCodeWithUOM3, messageType, "KGMG", "KGM", "NAR");
		helper.CreateTariffWithUOMs(tariffCodeWithoutUOM3, messageType);

		Declaration.JE_MessageType = messageType;
		CombineAssertions(() =>
		{
			EntryInstruction.CEI_PartialDelivery = false;
			InvoiceLine.JI_Tariff = tariffCodeWithUOM3;

			InvoiceLine.JI_CustomsThirdQuantity = 0;
			AssertHasMessageError("ThirdQuantity required for CU3 without partial delivery", InvoiceLine.JI_CustomsThirdQuantityInfo, messageError);

			InvoiceLine.JI_CustomsThirdQuantity = 1;
			AssertNoMessageError("ThirdQuantity provided for CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, messageError);

			InvoiceLine.JI_Tariff = tariffCodeWithoutUOM3;
			InvoiceLine.JI_CustomsThirdQuantity = 0;
			AssertNoMessageError("Tariff without CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, messageError);

			EntryInstruction.CEI_PartialDelivery = true;
			InvoiceLine.JI_Tariff = tariffCodeWithUOM3;

			InvoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertNoMessageError("ThirdQuantity not required for CU3 with partial delivery", InvoiceLine.JI_CustomsThirdQuantityInfo, messageError);

			InvoiceLine.JI_Tariff = tariffCodeWithoutUOM3;
			InvoiceLine.Validation.ValidateJI_CustomsThirdQuantity();
			AssertNoMessageError("Tariff without CU3", InvoiceLine.JI_CustomsThirdQuantityInfo, messageError);
		});
	}

	public void TestCheckJI_CustomsThirdQuantity_NS30003()
	{
		var messageWarning = PassarValidationMessages.MessageNS30003_GreaterThanZero(InvoiceLine.JI_CustomsThirdQuantityInfo.HumanReadableName);
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;
		var propertyInfo = InvoiceLine.JI_CustomsThirdQuantityInfo;

		entryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		InvoiceLine.JI_CustomsThirdQuantity = 0;
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is not Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no warning", propertyInfo, messageWarning);
		InvoiceLine.JI_CustomsThirdQuantity = 1;
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is not Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no warning", propertyInfo, messageWarning);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		InvoiceLine.JI_CustomsThirdQuantity = 0;
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, no warning", propertyInfo, messageWarning);
		InvoiceLine.JI_CustomsThirdQuantity = 1;
		TestCaseWithFactory.AssertHasWarningContaining($"When Style is Simplified and {propertyInfo.HumanReadableName} is {propertyInfo.Value}, warning", propertyInfo, messageWarning);

		entryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.JI_CustomsThirdQuantity = 1;
		TestCaseWithFactory.AssertNoWarningContaining("Warning is not shown for EDA", propertyInfo, messageWarning);
	}

	public void TestCheckJI_CustomsThirdUnitQty()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CustomsThirdUnitQty = "@@";
		AssertNoNotifications(InvoiceLine.JI_CustomsThirdUnitQtyInfo);
	}

	public void TestCheckJI_PriceR268a()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;

		CombineAssertions(() =>
		{
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageError("JI_Price should have an error when Calc_StatisticalValue and CustomsValue are 0", InvoiceLine.JI_LinePriceInfo, ValidationMessages.Plausi.MessageR268a);

			InvoiceLine.JI_LinePrice = 100m;
			InvoiceLine.Validation.ValidateAll();
			AssertNoMessageError("JI_Price shouldn't have an error message when JI_LinePrice >= 0", InvoiceLine.JI_LinePriceInfo, ValidationMessages.Plausi.MessageR268a);

			var charge = InvoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "DED";
			charge.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			charge.J7_Amount = 100m;
			charge.J7_IsStatisticalValueApplicable = true;

			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("JI_Price shouldn't have an error message when Statistical Value >= 0 and Customs Value = 0", InvoiceLine.JI_LinePriceInfo, ValidationMessages.Plausi.MessageR268a);

			charge.J7_IsStatisticalValueApplicable = false;
			charge.J7_IsDutiable = true;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("JI_Price shouldn't have an error message when Customs Value >= 0 and Statistical Value = 0", InvoiceLine.JI_LinePriceInfo, ValidationMessages.Plausi.MessageR268a);

			charge.J7_IsDutiable = false;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageError("JI_Price should have an error message when Customs and Statistical Values = 0", InvoiceLine.JI_LinePriceInfo, ValidationMessages.Plausi.MessageR268a);
		});
	}

	public void TestCheckJI_LinePrice_NP70009()
	{
		RefCusTradeGroupTestHelper.CreateTestNCL0147CountryList(Factory);
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		var errorMessage = PassarValidationMessages.MessageNP70009_Price;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;

		CombineAssertions(() =>
		{
			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
			InvoiceLine.JI_LinePrice = 5001;
			AssertNoMessageError("if is not Simplified and not in NCL0147CountryList but price line sum > 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("if is not Simplified but in NCL0147CountryList and price line sum > 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("if is Simplified and not in NCL0147CountryList and price line sum > 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertHasMessageError("if is Simplified and in NCL0147CountryList and price line sum > 5000 error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("if is Simplified and in NCL0147CountryList and price line sum > 5000. But the currency not set no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			InvoiceLine.JI_LinePrice = 5000;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
			AssertNoMessageError("if is Simplified and in NCL0147CountryList but price line sum <= 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInEUNButNotInEUSEC;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("if is Simplified and not in NCL0147CountryList and price line sum <= 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_GoodsDestination = RefCusTradeGroupTestHelper.CountryInNCL0147CountryList;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
			InvoiceLine.Validation.ValidateJI_LinePrice();
			AssertNoMessageError("if is not Simplified but in NCL0147CountryList and price line sum <= 5000 no error", InvoiceLine.JI_LinePriceInfo, errorMessage);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
			InvoiceLine.JI_LinePrice = 5001;
			AssertNoMessageError("NP70009 is not enabled for EDA", InvoiceLine.JI_LinePriceInfo, errorMessage);
		});
	}

	public void TestCheckJI_WeightR268b()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError("JI_Weight should have an error when JI_Weight=0 and JI_NetWeight or JI_CustomsQuantity are 0", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR268b);

			InvoiceLine.JI_Weight = 100;
			AssertNoMessageError("JI_Weight shouldn't have an error message when JI_Weight > 0", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR268b);

			InvoiceLine.JI_NetWeight = 100;
			InvoiceLine.JI_Weight = 0;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError("JI_Weight should have an error message when is not defined and JI_NetWeight > 0", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR268b);

			InvoiceLine.JI_NetWeight = 0;
			InvoiceLine.JI_CustomsThirdQuantity = 100;

			InvoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError("JI_Weight should have an error message when is not defined and JI_CustomsQuantity > 0", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR268b);
		});
	}

	public void TestCheckJI_Weight_NP70001()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertNoMessageError("JI_Weight default value shouldn't have NP70001 message error", InvoiceLine.JI_WeightInfo, PassarValidationMessages.MessageNP70001);

		InvoiceLine.JI_Weight = 10;
		InvoiceLine.JI_NetWeight = 10;
		AssertNoMessageError("JI_GrossMassConfirmation unticked and JI_Weight = JI_NetWeight", InvoiceLine.JI_WeightInfo, PassarValidationMessages.MessageNP70001);

		InvoiceLine.JI_NetWeight = 15;
		AssertHasMessageError("JI_GrossMassConfirmation unticked and JI_Weight > JI_NetWeight", InvoiceLine.JI_WeightInfo, PassarValidationMessages.MessageNP70001);
	}

	public void TestCheckJI_ZZF_NKTaxType_124()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;

		CombineAssertions(() =>
		{
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.RelocationProcedure;
			AssertNoMessageError($"JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType} and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR124);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.ExemptVat;
			AssertHasMessageError($"JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType} and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR124);

			invoiceLine.JI_VATCodeConfirmation = true;
			AssertNoMessageError($"JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType} and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR124);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.StandardRate;
			AssertHasMessageError($"JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType} and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR124);

			invoiceLine.JI_VATCodeConfirmation = false;
			AssertNoMessageError($"JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType} and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR124);
		});
	}

	public void TestCheckJI_ZZF_NKTaxType_R219()
	{
		CombineAssertions(() =>
		{
			void SetPropertiesForInAndOutwardProcessing(string messageType = JobMessageTypeList.Codes.Import, string taxType = TaxCodes.ProcessingTraffic, string procedure = ProcedureCodesEdec.RefinementTransportation, string direction = InAndOutwardProcessingDirectionCodes.Active, string refinementType = InAndOutwardProcessingRefinementTypesEdec.ContractProcessing)
			{
				Declaration.JE_MessageType = messageType;
				InvoiceLine.JI_Procedure = procedure;
				InvoiceLine.InAndOutwardProcessingDirection = direction;
				InvoiceLine.InAndOutwardProcessingRefinementType = refinementType;
				InvoiceLine.JI_ZZF_NKTaxType = taxType;
			}

			SetPropertiesForInAndOutwardProcessing();
			AssertNoMessageError("Valid refinement transportation", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);

			SetPropertiesForInAndOutwardProcessing(procedure: ProcedureCodesEdec.NormalDuty);
			AssertHasMessageError("Procedure<>02", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(procedure: ProcedureCodesEdec.NormalDuty, messageType: JobMessageTypeList.Codes.Export);
			AssertNoMessageError("Procedure<>02, but not Import", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(procedure: ProcedureCodesEdec.NormalDuty, taxType: TaxCodes.StandardRate);
			AssertNoMessageError("Procedure<>02, but TaxType<>91", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);

			SetPropertiesForInAndOutwardProcessing(direction: InAndOutwardProcessingDirectionCodes.Passive);
			AssertHasMessageError("Direction<>1", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(direction: InAndOutwardProcessingDirectionCodes.Passive, messageType: JobMessageTypeList.Codes.Export);
			AssertNoMessageError("Direction<>1, but not Import", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(direction: InAndOutwardProcessingDirectionCodes.Passive, taxType: TaxCodes.StandardRate);
			AssertNoMessageError("Direction<>1, but TaxType<>91", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);

			SetPropertiesForInAndOutwardProcessing(refinementType: InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing);
			AssertHasMessageError("RefinementType<>2", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(refinementType: InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing, messageType: JobMessageTypeList.Codes.Export);
			AssertNoMessageError("RefinementType<>2, but not Import", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
			SetPropertiesForInAndOutwardProcessing(refinementType: InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing, taxType: TaxCodes.StandardRate);
			AssertNoMessageError("RefinementType<>2, but TaxType<>91", InvoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR219);
		});
	}

	public void TestCheckJI_ZZF_NKTaxType_276()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;

		CombineAssertions(() =>
		{
			invoiceLine.JI_VATCodeConfirmation = true;
			invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
			AssertHasMessageErrorContaining($"Empty JI_ZZF_NKTaxType and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_VATCodeConfirmation = false;
			AssertHasMessageErrorContaining($"Empty JI_ZZF_NKTaxType and JI_VATCodeConfirmation={invoiceLine.JI_VATCodeConfirmation}", invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestJI_ZZF_NKTaxType_R277()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var permit = invoiceLine.Permits.AddNew();
		permit.CSI_Code = PermitCodes.SingleEPermit;
		permit.CSI_IssuerType = PermitAuthorityCodes.FTA;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.RelocationProcedure;
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageError($"JE_MessageType={declaration.JE_MessageType} and JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR277);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertHasMessageErrorContaining($"JE_MessageType={declaration.JE_MessageType} and JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR277);

			permit = invoiceLine.Permits.AddNew();
			permit.CSI_Code = PermitCodes.Commitment;
			permit.CSI_IssuerType = PermitAuthorityCodes.FTA;
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageError($"JE_MessageType={declaration.JE_MessageType} and JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR277);

			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxCodes.StandardRate;
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageError($"JE_MessageType={declaration.JE_MessageType} and JI_ZZF_NKTaxType={invoiceLine.JI_ZZF_NKTaxType}", invoiceLine.JI_ZZF_NKTaxTypeInfo, ValidationMessages.Plausi.MessageR277);
		});
	}

	public void TestCheckNetDuty_R224()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		const string Switzerland = Core.Constants.CountryCodes.Switzerland;
		const string tariffCode = "61101200000000";
		const string unknownTariffCode = "9999999999999999";

		var tradeGroupAF = helper.CreateTradeGroup(Switzerland, "TradeGroupAF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupAF, "AF");
		var tradeGroupDE = helper.CreateTradeGroup(Switzerland, "TradeGroupDE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupDE, "DE");
		var tradeGroupCN = helper.CreateTradeGroup(Switzerland, "TradeGroupCN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupCN, "CN");
		var tradeGroupLS = helper.CreateTradeGroup(Switzerland, "TradeGroupLS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupLS, "LS");

		var tariffType = helper.CreateNewOrGetExistingTariffType(Switzerland, Universal.Constants.TariffTypes.Import);
		var preferencePR = helper.CreatePreferenceForCountry(UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff, "Preferential Tariff", Switzerland);
		var preferenceNT = helper.CreatePreferenceForCountry(UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff, "Normal Tariff", Switzerland);
		Factory.Save();

		var tariff = helper.CreateTariff(Switzerland, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var rateType = helper.CreateNewOrGetExistingRateType(Switzerland, "DTY");
		var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE", rateType.PK);

		var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "1.53 * [KGMG]", preferencePk: preferencePR.PK, dataGrouping: Switzerland);
		helper.CreateCusApplicability(rate, tradeGroupAF, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "3.06 * [KGMG]", preferencePk: preferenceNT.PK, dataGrouping: Switzerland);
		helper.CreateCusApplicability(rate, tradeGroupDE, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "80 * [NAR]", preferencePk: preferencePR.PK, dataGrouping: Switzerland);
		helper.CreateCusApplicability(rate, tradeGroupCN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "90 * [NAR]", preferencePk: preferenceNT.PK, dataGrouping: Switzerland);
		helper.CreateCusApplicability(rate, tradeGroupLS, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		Factory.Save();

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "AF";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When PR contains KGMG", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "DE";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When NT fallback contains KGMG", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "CN";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertHasMessageError("When PR doesn't contain KGMG", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "LS";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertHasMessageError("When NT fallback doesn't contain KGMG", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = ZString.Empty;
			InvoiceLine.JI_CountryOfOrigin = "LS";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When Tariff is empty", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = unknownTariffCode;
			InvoiceLine.JI_CountryOfOrigin = "LS";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When Tariff is unknown", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When CountryOfOrigin is empty", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "LS";
			InvoiceLine.JI_PrimaryPreference = ZString.Empty;
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When PrimaryPreference is empty", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "LS";
			InvoiceLine.JI_PrimaryPreference = "PR";
			InvoiceLine.NetDuty = true;
			AssertNoMessageError("When not Import", InvoiceLine.NetDutyInfo, ValidationMessages.Plausi.MessageR224);
		});
	}

	public void TestCheckJI_CustomsValue()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
		InvoiceLine.JI_LinePrice = 100;

		CombineAssertions(() =>
		{
			AssertNoMessageError($"CustomsValue={InvoiceLine.JI_CustomsValue} > 3*LincePrice={InvoiceLine.JI_LinePrice}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertNoMessageError($"CustomsValue={InvoiceLine.JI_CustomsValue} > 3*LincePrice={InvoiceLine.JI_LinePrice}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);

			var charge = InvoiceLine.Charges.AddNew();
			charge.J7_RX_NKCurrency = Declaration.LocalCurrencyCode;
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			charge.J7_Amount = 201;
			charge.J7_IsDutiable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertNoMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} > 3*StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertHasMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} > 3*StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);

			charge.J7_Amount = 200;
			charge.J7_IsDutiable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertNoMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} not > 3*StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertNoMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} not > 3*StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);

			charge.J7_Amount = 1;
			charge.J7_IsDutiable = false;
			charge.J7_IsStatisticalValueApplicable = true;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertHasMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} < StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertNoMessageError($"When CustomsValue={InvoiceLine.JI_CustomsValue} < StatisticalValue={InvoiceLine.JI_Calc_StatisticalValue}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);

			charge.J7_Amount = 201;
			charge.J7_IsDutiable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			InvoiceLine.JI_VATValueConfirmation = true;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertNoMessageError($"When confirmed", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertNoMessageError($"When confirmed", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);

			InvoiceLine.JI_VATValueConfirmation = false;
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ExemptFromDuty;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertNoMessageError($"When {nameof(UniversalReferenceConstants.ProcedureCodesEdec.ExemptFromDuty)}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_1);
			AssertNoMessageError($"When {nameof(UniversalReferenceConstants.ProcedureCodesEdec.ExemptFromDuty)}", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR123_2);
		});
	}

	public void TestConditionTypeDescriptionLanguage()
	{
		const string defaultConditionTypeDescription = "*** Default language condition type description***";
		const string frenchConditionTypeDescription = "*** French condition type description ***";

		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.Helper.CreateOrGetLanguage("FR", "French");
		var cusConditionType = testHelper.Helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, UniversalReferenceConstants.CusConditionType.WeightCheck1, defaultConditionTypeDescription);
		var refCusConditionTypeLanguage = Factory.New<RefCusConditionTypeLanguage>();
		refCusConditionTypeLanguage.ZXW_ZX2_ConditionType = cusConditionType.PK;
		refCusConditionTypeLanguage.ZXW_ZX6_NKLanguage = "FR";
		refCusConditionTypeLanguage.ZXW_Description = frenchConditionTypeDescription;

		var tariffWithWGTC1 = testHelper.CreateImportTariffWithConditionClass("07129081026999", UniversalReferenceConstants.CusConditionType.WeightCheck1, "[KGM] >= 5");

		GlbStaff.CurrentUser.GS_WorkingLanguage = "FR";
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = tariffWithWGTC1.ZZ1_TariffCode;
		InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_NetWeight = 4;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("French description", InvoiceLine.JI_TariffInfo, frenchConditionTypeDescription);
		});
	}

	public void TestCheckDutyRateAdditionalCode_R133b()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
			AssertNoMessageError("Single rate", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);

			InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
			AssertHasMessageError("Multiple rates (no additional code)", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);

			InvoiceLine.DutyRateAdditionalCode = InvoiceLine.Lookups.AdditionalCodesList.GetAllCodes().First();
			AssertNoMessageError("Multiple rates (with additional code)", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);

			InvoiceLine.DutyRateAdditionalCode = ZString.Empty;

			InvoiceLine.JI_Procedure = ProcedureCodesEdec.ExemptFromDuty;
			AssertNoMessageError("Procedure 08", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.ReturnedGoods;
			AssertNoMessageError("Procedure 10", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertNoMessageError("Procedure 11", InvoiceLine.DutyRateAdditionalCodeInfo, ValidationMessages.Plausi.MessageR133b);
		});
	}

	public void TestCheckDutyRateConfirmation_R133a()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
		InvoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		InvoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;

		CombineAssertions(() =>
		{
			AssertNoMessageError($"DutyRateAdditionalCode={InvoiceLine.DutyRateAdditionalCode}", InvoiceLine.DutyRateConfirmationInfo, ValidationMessages.Plausi.MessageR133a);

			InvoiceLine.DutyRateAdditionalCode = "XXX";
			AssertHasMessageError($"DutyRateAdditionalCode={InvoiceLine.DutyRateAdditionalCode}", InvoiceLine.DutyRateConfirmationInfo, ValidationMessages.Plausi.MessageR133a);

			InvoiceLine.DutyRateConfirmation = ZBool.True;
			AssertNoMessageError($"DutyRateAdditionalCode={InvoiceLine.DutyRateAdditionalCode}", InvoiceLine.DutyRateConfirmationInfo, ValidationMessages.Plausi.MessageR133a);

			InvoiceLine.DutyRateAdditionalCode = "XXX";
			InvoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
			AssertNoMessageError($"single rate tariff", InvoiceLine.DutyRateConfirmationInfo, ValidationMessages.Plausi.MessageR133a);
		});
	}

	public void TestCheck_JI_Tariff_R356()
	{
		var prohibitedProcedure = ProcedureCodesEdec.DutyFree;
		var allowedProcedure = ProcedureCodesEdec.NormalDuty;

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var customsReliefTariffs = new[]
		{
				tariffTestHelper.CreateImportTariff(Tariffs.NegligibleImportTariff),
				tariffTestHelper.CreateImportTariff("10000000111000")
			};
		var normalTariff = tariffTestHelper.CreateImportTariff("10000000000000");

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = true;
			foreach (var customsReliefTariff in customsReliefTariffs)
			{
				InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
				AssertHasMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR356);
			}

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = true;
			foreach (var customsReliefTariff in customsReliefTariffs)
			{
				InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
				AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR356);
			}

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = true;
			InvoiceLine.JI_Tariff = normalTariff.ZZ1_TariffCode;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR356);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = allowedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = true;
			foreach (var customsReliefTariff in customsReliefTariffs)
			{
				InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
				AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR356);
			}

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = false;
			foreach (var customsReliefTariff in customsReliefTariffs)
			{
				InvoiceLine.JI_Tariff = customsReliefTariff.ZZ1_TariffCode;
				AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR356);
			}
		});

		string GetMessage() => $"Procedure - \"{InvoiceLine.JI_Procedure}\", Repair - \"{InvoiceLine.InAndOutwardProcessingRepair}\", Tariff code - \"{InvoiceLine.JI_Tariff}\"";
	}

	public void TestCheck_JI_Tariff_R182()
	{
		var prohibitedProcedure = ProcedureCodesEdec.DutyFree;
		var allowedProcedure = ProcedureCodesEdec.NormalDuty;

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffWithCustomsFavourCode = tariffTestHelper.CreateImportTariff("10000000111000");
		var tariffWithoutCustomsFavourCode = tariffTestHelper.CreateImportTariff("10000000000000");

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = false;
			InvoiceLine.JI_Tariff = tariffWithCustomsFavourCode.ZZ1_TariffCode;
			AssertHasMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR182);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = false;
			InvoiceLine.JI_Tariff = tariffWithCustomsFavourCode.ZZ1_TariffCode;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR182);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = false;
			InvoiceLine.JI_Tariff = tariffWithoutCustomsFavourCode.ZZ1_TariffCode;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR182);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = allowedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = false;
			InvoiceLine.JI_Tariff = tariffWithCustomsFavourCode.ZZ1_TariffCode;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR182);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Procedure = prohibitedProcedure;
			InvoiceLine.InAndOutwardProcessingRepair = true;
			InvoiceLine.JI_Tariff = tariffWithCustomsFavourCode.ZZ1_TariffCode;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_TariffInfo, ValidationMessages.Plausi.MessageR182);
		});

		string GetMessage() => $"Procedure - \"{InvoiceLine.JI_Procedure}\", Repair - \"{InvoiceLine.InAndOutwardProcessingRepair}\", Tariff code - \"{InvoiceLine.JI_Tariff}\"";
	}

	public void TestCheckJI_Description_Import() => AssertJI_Description(JobMessageTypeList.Codes.Import);

	public void TestCheckJI_Description_Export() => AssertJI_Description(JobMessageTypeList.Codes.Export);

	void AssertJI_Description(string messageType)
	{
		Declaration.JE_MessageType = messageType;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_DescriptionInfo, $"[{ValidationMessages.Plausi.CH0004}]");
	}

	public void TestCheckJI_NetWeight_NS30092()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var helper = new RefCusTariffTestHelper(Factory);
		var tariffWithOutNMO = RefCusTariffTestHelper.ExportTariffHay;
		var tariffYNMO = RefCusTariffTestHelper.ExportTariffGardenUmbrellas;
		var tariffNNMO = RefCusTariffTestHelper.ExportTariffFruitJuice;
		helper.CreateExportTariff(tariffWithOutNMO);
		helper.CreateExportTariffWithAttribute(tariffYNMO, CH.Business.UniversalReferenceConstants.TariffAttributes.NetMassOptional, CH.Business.UniversalReferenceConstants.TariffAttributes.Values.Yes);
		helper.CreateExportTariffWithAttribute(tariffNNMO, CH.Business.UniversalReferenceConstants.TariffAttributes.NetMassOptional, CH.Business.UniversalReferenceConstants.TariffAttributes.Values.No);
		Factory.Save();

		var errorMessage = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30092, InvoiceLine.JI_NetWeightInfo.HumanReadableName);

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = "123456789012";
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("Invalid tariff - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariffWithOutNMO;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("no NetMassOptional - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariffYNMO;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertNoMessageError("NetMassOptional Y - not mandatory", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariffNNMO;
			InvoiceLine.Validation.ValidateJI_NetWeight();
			AssertHasMessageError("NetMassOptional N - mandatory - empty", InvoiceLine.JI_NetWeightInfo, errorMessage);

			InvoiceLine.JI_Tariff = tariffNNMO;
			InvoiceLine.JI_NetWeight = 100;
			AssertNoMessageError("NetMassOptional N - mandatory - entered", InvoiceLine.JI_NetWeightInfo, errorMessage);
		});
	}

	public void TestCheckJI_Weight_NP70026()
	{
		var messageNP70026 = "[NP70026] Gross Weight must be greater than 0 if the Goods Item has a Package with Quantity greater than 0 or the Package Type Bulk.";
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var package = InvoiceLine.Declaration.Packages.AddNew();
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
		var packagePivot = InvoiceLine.PackagesPivot.AddNew();
		packagePivot.CHC_JE = Declaration.PK;
		packagePivot.CHC_CW = package.PK;
		packagePivot.CHC_NumberOfPacks = 2;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Weight = 1;
			AssertNoMessageError("Weight is not 0 and the package isBulk/pivot package has NumOfPacks higher than 0, there should be no message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			InvoiceLine.JI_Weight = 0;
			AssertHasMessageError("Weight is 0 and the package isBulk/pivot package has NumOfPacks higher than 0, there should be a message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			packagePivot.CHC_NumberOfPacks = 0;
			InvoiceLine.JI_Weight = 0;
			AssertHasMessageError("Package Pivot NumberOfPacks is 0, there should be a message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
			InvoiceLine.JI_Weight = 0;
			AssertNoMessageError("Package PackType is not a Bulk Code, there should be no message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			packagePivot.CHC_NumberOfPacks = 1;
			InvoiceLine.JI_Weight = 0;
			AssertHasMessageError("Package Pivot NumberOfPacks is 1, there should be a message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			packagePivot.CHC_NumberOfPacks = 0;
			var secondPackagePivot = InvoiceLine.PackagesPivot.AddNew();
			secondPackagePivot.CHC_JE = Declaration.PK;
			secondPackagePivot.CHC_CW = package.PK;
			secondPackagePivot.CHC_NumberOfPacks = 2;
			InvoiceLine.JI_Weight = 0;
			AssertHasMessageError("A second package pivot has a NumberOfPacks > 0, there should be a message error", InvoiceLine.JI_WeightInfo, messageNP70026);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError("No message error for EDA", InvoiceLine.JI_WeightInfo, messageNP70026);
		});
	}

	public void TestCheckUNDGCodes_NS30003() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNS30003_NotAllowed(InvoiceLine.UNDGCodesInfo.HumanReadableName);
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InvoiceLine.UNDGCodesInfo, () => InvoiceLine.UNDGs.DeleteAll(), () => invoiceLine.UNDGs.AddNew(), messageError);
	});

	public void TestCheckJI_NetWeight_NS30003() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		var messageError = PassarValidationMessages.MessageNS30003_NetWeight;

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		InvoiceLine.JI_NetWeight = 0;
		AssertNoWarningContaining("When Style is not 1 and JI_NetWeight is Empty no error", InvoiceLine.JI_NetWeightInfo, messageError);

		InvoiceLine.JI_NetWeight = 1;
		AssertNoWarningContaining("When Style is not 1 and JI_NetWeight is not Empty no error", InvoiceLine.JI_NetWeightInfo, messageError);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		InvoiceLine.JI_NetWeight = 0;
		AssertNoWarningContaining("When Style is 1 and JI_NetWeight is not Empty no error", InvoiceLine.JI_NetWeightInfo, messageError);

		InvoiceLine.JI_NetWeight = 1;
		AssertHasWarningContaining("When Style is 1 and JI_NetWeight is not Empty error", InvoiceLine.JI_NetWeightInfo, messageError);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.Validation.ValidateJI_NetWeight();
		AssertNoWarningContaining("When Style is 1 and JI_NetWeight is not Empty but in EDA no error", InvoiceLine.JI_NetWeightInfo, messageError);
	});

	public void TestCheckJI_Tariff_NS30003() => CombineAssertions(() =>
	{
		string expectedMessageError = PassarValidationMessages.MessageNS30003_Tariff;
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		InvoiceLine.JI_Tariff = ZString.Empty;
		TestCaseWithFactory.AssertHasMessageError("standard message error if Ordinary and empty", InvoiceLine.JI_TariffInfo, "Tariff may not be empty");
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is not Simplified and Tariff is Empty, no Warning", InvoiceLine.JI_TariffInfo, expectedMessageError);
		InvoiceLine.JI_Tariff = "12345678000";
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is not Simplified and  Tariff is not Empty, no Warning", InvoiceLine.JI_TariffInfo, expectedMessageError);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		InvoiceLine.JI_Tariff = ZString.Empty;
		TestCaseWithFactory.AssertNoMessageError("no standard message error if Simplify and empty", InvoiceLine.JI_TariffInfo, "Tariff may not be empty");
		TestCaseWithFactory.AssertNoWarningContaining($"When Style is Simplified and Tariff is Empty, no Warning", InvoiceLine.JI_TariffInfo, expectedMessageError);
		InvoiceLine.JI_Tariff = "12345678000";
		TestCaseWithFactory.AssertHasWarningContaining($"When Style is Simplified and  Tariff is not Empty, Warning", InvoiceLine.JI_TariffInfo, expectedMessageError);

		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.JI_Tariff = "12345678000";
		TestCaseWithFactory.AssertNoWarningContaining("Warning is not shown for EDA", InvoiceLine.JI_TariffInfo, expectedMessageError);
	});

	public void TestCheck_JI_Tariff_R338acd() => CombineAssertions(() =>
	{
		EntryInstruction.JobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var messageError = ValidationMessages.Plausi.MessageR338;

		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5);
		AssertHasMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450202, AdditionalTaxesTariffs.Tariff465001);
		AssertHasMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450201, AdditionalTaxesTariffs.Tariff465002);
		AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.OtherManufacturedTobaccoOtherOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5);
		AssertNoMessageError("if tariffNumber != 2402.2010 and statisticalCode = 999 and 450_Quantity != 465_Quantity, no message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF, 3, 5);
		AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode != 999 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 5, 5);
		AssertNoMessageError("if tariffNumber = 2402.2010 and statisticalCode = 999 and 450_Quantity == 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);

		var tobacco = InvoiceLine.Tobaccos.AddNew();
		tobacco.CSI_Code = TobaccoMainGroupCodes.CutTobacco;
		tobacco.CSI_SubType = TobaccoSubGroupCodes._02;
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertHasMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450201, AdditionalTaxesTariffs.Tariff465002);
		AssertHasMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450202, AdditionalTaxesTariffs.Tariff465001);
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5);
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		tobacco.CSI_SubType = TobaccoSubGroupCodes._03;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup != 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		tobacco.CSI_SubType = TobaccoSubGroupCodes._02;
		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigars;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup != 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		tobacco.CSI_Code = TobaccoMainGroupCodes.CutTobacco;
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.StatisticalCodeOther, 5, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity = 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.SmokingTobaccoOther, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF, 3, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertNoMessageError("if tariffNumber = 2403.1900 and statisticalCode != 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.OtherManufacturedTobaccoOtherOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertNoMessageError("if tariffNumber != 2403.1900 and statisticalCode = 999 and  tobaccoMainGroup = 3 tobaccoSubGroup = 02 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);

		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
		tobacco.CSI_SubType = TobaccoSubGroupCodes._02;
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5);
		AssertHasMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450202, AdditionalTaxesTariffs.Tariff465001);
		AssertHasMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450001, AdditionalTaxesTariffs.Tariff465202);
		AssertNoMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 3, 5, AdditionalTaxesTariffs.Tariff450201, AdditionalTaxesTariffs.Tariff465002);
		AssertNoMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity but wrong 465 and 450 sub type is wrong, no message error", InvoiceLine.JI_TariffInfo, messageError);
		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigars;
		InvoiceLine.Validation.ValidateJI_Tariff();
		AssertNoMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup != 2 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		tobacco.CSI_Code = TobaccoMainGroupCodes.Cigarettes;
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.StatisticalCodeOther, 5, 5);
		AssertNoMessageError("if tariffNumber = 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity = 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.CigarettesContainingTobaccoNotMoreThan, TariffStatisticalCodes.TobaccoPrivateGoodsNoMoreOf10kgOr1000CHF, 3, 5);
		AssertNoMessageError("if tariffNumber = 2402.2020 and statisticalCode != 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);
		ChangeTariffAndTax(TariffNumbers.OtherManufacturedTobaccoOtherOther, TariffStatisticalCodes.StatisticalCodeOther, 3, 5);
		AssertNoMessageError("if tariffNumber != 2402.2020 and statisticalCode = 999 and  tobaccoMainGroup = 2 and 450_Quantity != 465_Quantity, message error", InvoiceLine.JI_TariffInfo, messageError);

		void ChangeTariffAndTax(string tariffNumber, string statisticalCode, int tax450Value, int tax465Value, string tax450Code = AdditionalTaxesTariffs.Tariff450002, string tax465Code = AdditionalTaxesTariffs.Tariff465201)
		{
			var oldTariff = InvoiceLine.JI_Tariff;
			InvoiceLine.JI_Tariff = tariffNumber + "000" + statisticalCode;
			if (oldTariff != InvoiceLine.JI_Tariff)
			{
				var additionalTariff450 = InvoiceLine.AdditionalTaxes.AddNew();
				var additionalTariff465 = InvoiceLine.AdditionalTaxes.AddNew();
				additionalTariff450.BZ_Tariff = tax450Code;
				additionalTariff465.BZ_Tariff = tax465Code;
				additionalTariff450.BZ_Qty1 = tax450Value;
				additionalTariff465.BZ_Qty1 = tax465Value;
			}
			else
			{
				var additionalTariff450 = invoiceLine.AdditionalTaxes.Where(additionalTax => additionalTax.BZ_Tariff.Contains("450")).FirstOrDefault();
				var additionalTariff465 = invoiceLine.AdditionalTaxes.Where(additionalTax => additionalTax.BZ_Tariff.Contains("465")).FirstOrDefault();
				additionalTariff450.BZ_Tariff = tax450Code;
				additionalTariff465.BZ_Tariff = tax465Code;
				additionalTariff450.BZ_Qty1 = tax450Value;
				additionalTariff465.BZ_Qty1 = tax465Value;
			}
			InvoiceLine.Validation.ValidateJI_Tariff();
		}
	});

	public void TestCheckJI_CustomsValue_R249a() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		foreach (string tariff in TariffNumbers.TobaccoQuantityBasedTaxationTariffNumbers)
		{
			InvoiceLine.JI_Tariff = tariff + "000911";
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
			InvoiceLine.JI_LinePrice = 1000;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertNoMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Price <= 1000", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR249a);
			InvoiceLine.JI_LinePrice = 1001;
			InvoiceLine.Validation.ValidateJI_CustomsValue();
			AssertHasMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Price > 1000", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR249a);
		}
		InvoiceLine.JI_Tariff = $"{TariffNumbers.CigarCherootsCigarillosContainingTobacco}000999";
		InvoiceLine.Validation.ValidateJI_CustomsValue();
		AssertNoMessageError("Statistical code not equal to 911", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR249a);

		InvoiceLine.JI_Tariff = $"{TariffNumbers.ProductsContainingTobaccoOther}000911";
		InvoiceLine.Validation.ValidateJI_CustomsValue();
		AssertNoMessageError("Tariff not in set for rule R249", InvoiceLine.JI_CustomsValueInfo, ValidationMessages.Plausi.MessageR249a);
	});

	public void TestCheckJI_Weight_R249b() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		foreach (string tariff in TariffNumbers.TobaccoQuantityBasedTaxationTariffNumbers)
		{
			InvoiceLine.JI_Tariff = tariff + "000911";
			InvoiceLine.JI_WeightUQ = "G";
			InvoiceLine.JI_Weight = 10000;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertNoMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Customs quantity <= 10 kg", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR249b);
			InvoiceLine.JI_Weight = 11000;
			InvoiceLine.Validation.ValidateJI_Weight();
			AssertHasMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Customs quantity > 10 kg", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR249b);
		}
		InvoiceLine.JI_Tariff = $"{TariffNumbers.CigarCherootsCigarillosContainingTobacco}000999";
		InvoiceLine.Validation.ValidateJI_Weight();
		AssertNoMessageError("Statistical code not equal to 911", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR249b);

		InvoiceLine.JI_Tariff = $"{TariffNumbers.ProductsContainingTobaccoOther}000911";
		InvoiceLine.Validation.ValidateJI_Weight();
		AssertNoMessageError("Tariff not in set for rule R249", InvoiceLine.JI_WeightInfo, ValidationMessages.Plausi.MessageR249b);
	});

	public void TestCheckJI_Procedure_R249e() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		foreach (string tariff in TariffNumbers.TobaccoQuantityBasedTaxationTariffNumbers)
		{
			InvoiceLine.JI_Tariff = tariff + "000911";
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.Tobacco;
			AssertNoMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Procedure = 06", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR249e);
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
			AssertHasMessageError($"Tariff = {InvoiceLine.JI_Tariff} and Procedure not equal to 06", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR249e);
		}
		InvoiceLine.JI_Tariff = $"{TariffNumbers.CigarCherootsCigarillosContainingTobacco}000999";
		InvoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError("Statistical code not equal to 911", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR249e);

		InvoiceLine.JI_Tariff = $"{TariffNumbers.ProductsContainingTobaccoOther}000911";
		InvoiceLine.Validation.ValidateJI_Procedure();
		AssertNoMessageError("Tariff not in set for rule R249", InvoiceLine.JI_ProcedureInfo, ValidationMessages.Plausi.MessageR249e);
	});

	public void TestCheckJI_RefundType()
	{
		CombineAssertions(() =>
		{
			RefCusCodeTestHelper.CreateRefundTypeList(Factory);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_RefundTypeInfo, RefCusCodeTestHelper.InvalidRefundTypeCode, RefCusCodeTestHelper.ValidRefundTypeCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.Validation.ValidateJI_RefundType();
			AssertNoMessageErrors(InvoiceLine.JI_RefundTypeInfo);
		});
	}

	public void TestCheckJI_RefundType_NS30003() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNS30003_NotEmpty(InvoiceLine.JI_RefundTypeInfo.HumanReadableName);
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		PlausiValidationTestHelper.AssertNS30003(entryInstruction, InvoiceLine.JI_RefundTypeInfo, messageError);
	});

	public void TestCheckJI_RefundReferenceNumber() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
		InvoiceLine.Validation.ValidateJI_RefundReferenceNumber();
		AssertHasMessageErrorContaining("GDRN mandatory - not entered", InvoiceLine.JI_RefundReferenceNumberInfo, "You have not entered");

		InvoiceLine.JI_RefundReferenceNumber = "24CH12345678901238";
		AssertNoMessageErrorContaining("GDRN mandatory - entered", InvoiceLine.JI_RefundReferenceNumberInfo, "You have not entered");
	});

	public void TestCheckJI_RefundGoodsItemNumber() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
		InvoiceLine.Validation.ValidateJI_RefundGoodsItemNumber();
		AssertHasMessageErrorContaining("Goods Item Number mandatory - not entered", InvoiceLine.JI_RefundGoodsItemNumberInfo, "You have not entered");

		InvoiceLine.JI_RefundGoodsItemNumber = 1;
		AssertNoMessageErrorContaining("Goods Item Number mandatory - entered", InvoiceLine.JI_RefundGoodsItemNumberInfo, "You have not entered");

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		InvoiceLine.Validation.ValidateJI_RefundGoodsItemNumber();
		AssertEquals("!ReturnedGoodsWithRefundRequest - Goods Item Number empty", 0, InvoiceLine.JI_RefundGoodsItemNumber);
		AssertNoMessageErrors("!ReturnedGoodsWithRefundRequest - Goods Item Number empty - no errors", InvoiceLine.JI_RefundGoodsItemNumberInfo);
	});

	public void TestCheckJI_RefundReason() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
		InvoiceLine.Validation.ValidateJI_RefundReason();
		AssertHasMessageErrorContaining("Refund Reason mandatory - not entered", InvoiceLine.JI_RefundReasonInfo, "You have not entered");

		InvoiceLine.JI_RefundReason = "Refund Reason";
		AssertNoMessageErrorContaining("Refund Reason mandatory - entered", InvoiceLine.JI_RefundReasonInfo, "You have not entered");

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		InvoiceLine.Validation.ValidateJI_RefundReason();
		AssertEquals("!ReturnedGoodsWithRefundRequest - Refund Reason empty", ZString.Empty, InvoiceLine.JI_RefundReason);
		AssertNoMessageErrors("!ReturnedGoodsWithRefundRequest - Refund Reason empty - no errors", InvoiceLine.JI_RefundReasonInfo);
	});

	public void TestCheckJI_WeightIncludingInnerPackage()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfValueIsNegative(InvoiceLine.JI_WeightIncludingInnerPackageInfo);
		});
	}

	public void TestJI_WeightIncludingInnerPackage_R159()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.NetDuty = true;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_WeightIncludingInnerPackageInfo);
		});
	}

	public void TestJI_WeightIncludingInnerPackage_R160()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_CustomsSecondQuantity = 10;
		InvoiceLine.JI_CustomsQuantity = 20;

		CombineAssertions(() =>
		{
			InvoiceLine.NetDuty = true;
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_WeightIncludingInnerPackage = 10000;
			AssertNoMessageError($"When NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} = {nameof(InvoiceLine.JI_CustomsSecondQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);
			InvoiceLine.JI_WeightIncludingInnerPackage = 9000;
			AssertHasMessageError($"When NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} < {nameof(InvoiceLine.JI_CustomsSecondQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);

			InvoiceLine.NetDuty = false;
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_WeightIncludingInnerPackage = 9000;
			AssertNoMessageError($"When not NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} < {nameof(InvoiceLine.JI_CustomsSecondQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);

			InvoiceLine.NetDuty = true;
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_WeightIncludingInnerPackage = 20000;
			AssertNoMessageError($"When NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} = {nameof(InvoiceLine.JI_CustomsQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);
			InvoiceLine.JI_WeightIncludingInnerPackage = 21000;
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			AssertHasMessageError($"When NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} > {nameof(InvoiceLine.JI_CustomsQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);

			InvoiceLine.NetDuty = false;
			InvoiceLine.JI_WeightIncludingInnerPackageUQ = Core.Constants.Weight.Grams;
			InvoiceLine.JI_WeightIncludingInnerPackage = 21000;
			AssertNoMessageError($"When not NetDuty and {nameof(InvoiceLine.JI_WeightIncludingInnerPackage)} > {nameof(InvoiceLine.JI_CustomsQuantity)}", InvoiceLine.JI_WeightIncludingInnerPackageInfo, ValidationMessages.Plausi.MessageR160);
		});
	}

	public void TestCheckJI_WeightIncludingInnerPackageUQ()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_WeightIncludingInnerPackage = 100;
		InvoiceLine.JI_WeightIncludingInnerPackageUQ = string.Empty;

		AssertHasErrorContaining(InvoiceLine.JI_WeightIncludingInnerPackageUQInfo, MandatoryValidation.MustBeEntered);
		ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_WeightIncludingInnerPackageUQInfo, "XX", Core.Constants.Weight.Kilograms);
	}

	public void TestCheckJI_TareSupplementPercentage()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			InvoiceLine.JI_TareSupplementPercentage = -1.5m;
			AssertHasErrorContaining(InvoiceLine.JI_TareSupplementPercentageInfo, MandatoryValidation.ValueCannotBeNegative);
			InvoiceLine.JI_TareSupplementPercentage = 0m;
			AssertNoErrorContaining(InvoiceLine.JI_TareSupplementPercentageInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	public void TestJI_PermitObligation_InvalidCodeOrEmpty()
	{
		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_PermitObligation = ZString.Empty;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_PermitObligationInfo, RefCusCodeTestHelper.InvalidPermitObligationCode, RefCusCodeTestHelper.ValidPermitObligationCode);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		InvoiceLine.Validation.ValidateJI_PermitObligation();
		AssertNoMessageErrorContaining("Permit Obligation is not mandatory for EXP Delcaration", InvoiceLine.JI_PermitObligationInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.Validation.ValidateJI_PermitObligation();
		AssertNoMessageErrorContaining("Permit Obligation is not mandatory for EDA Delcaration", InvoiceLine.JI_PermitObligationInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJI_PermitObligation_R135() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var permitObligationInfo = InvoiceLine.JI_PermitObligationInfo;

		AssertNoMessageError("Permit obligation not set", permitObligationInfo, ValidationMessages.Plausi.MessageR135);
		InvoiceLine.JI_PermitObligation = "0";
		AssertNoMessageError($"#Permits = 0, PermitObbligation != 1", permitObligationInfo, ValidationMessages.Plausi.MessageR135);
		InvoiceLine.JI_PermitObligation = "1";
		AssertNoMessageError($"#Permits = 0, PermitObbligation = 1", permitObligationInfo, ValidationMessages.Plausi.MessageR135);

		InvoiceLine.Permits.AddNew();

		InvoiceLine.JI_PermitObligation = "0";
		AssertHasMessageError($"#Permits > 0, PermitObbligation != 1", permitObligationInfo, ValidationMessages.Plausi.MessageR135);
		InvoiceLine.JI_PermitObligation = "1";
		AssertNoMessageError($"#Permits > 0, PermitObbligation = 1", permitObligationInfo, ValidationMessages.Plausi.MessageR135);
	});

	public void TestCheckJI_PermitObligation_R134abc()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		const string Switzerland = Core.Constants.CountryCodes.Switzerland;
		const string tariffCode = "01012110000911";

		RefCusCodeTestHelper.CreatePermitObligationCodeList(Factory);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var permitObligationInfo = InvoiceLine.JI_PermitObligationInfo;

		var tariffType = helper.CreateNewOrGetExistingTariffType(Switzerland, Universal.Constants.TariffTypes.Import);
		var conditionType = helper.CreateOrGetExistingRefCusConditionType(Switzerland, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.CusConditionType.FederalOfficeForAgriculture, "Test");

		Factory.Save();

		new RefCusTariffTestHelper(Factory).CreateImportTariff(RefCusTariffTestHelper.ImportTariffBeverages);
		var tariff = helper.CreateTariff(Switzerland, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.HasOptionalPermit, UniversalReferenceConstants.TariffAttributes.Values._1, tariff);

		var allCountriesTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Switzerland, "AllCountries", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(allCountriesTradeGroup, "US");

		var condition = helper.CreateOrGetExistingRefCusCondition(Switzerland, conditionType.PK, tariff.PK, "Test", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		var applicability = helper.CreateCusApplicability(condition, allCountriesTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var excludedTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Switzerland, "ExcludedTradeGroup", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(excludedTradeGroup, "DE");

		helper.CreateExcludedTradeGroup(excludedTradeGroup, applicability);
		Factory.Save();

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = tariffCode;
			InvoiceLine.JI_CountryOfOrigin = "DE";

			InvoiceLine.JI_PermitObligation = "1";
			AssertHasMessageError("Permit Obligation should have a message error because it is not 0", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ValidationMessages.Plausi.R134));

			InvoiceLine.JI_PermitObligation = "0";
			AssertNoMessageError("Permit Obligation should not have a message error", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ValidationMessages.Plausi.R134));

			InvoiceLine.JI_CountryOfOrigin = "US";
			InvoiceLine.JI_PermitObligation = "1";
			AssertHasMessageError("Permit Obligation should have a message error because it is not 2", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ValidationMessages.Plausi.R134));

			InvoiceLine.JI_PermitObligation = "2";
			AssertNoMessageError("Permit Obligation should not have a message error", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ValidationMessages.Plausi.R134));

			InvoiceLine.JI_CountryOfOrigin = ZString.Empty;
			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBeverages;
			InvoiceLine.JI_PermitObligation = "0";
			AssertNoMessageError("#Permits = 0, Tariff doesn't have 'hasOptionalPermit' attribute and PermitObbligation = 0", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ValidationMessages.Plausi.R134));
			AssertNoMessageError("#Permits = 0, Tariff doesn't have 'hasOptionalPermit' attribute and PermitObbligation = 0", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ValidationMessages.Plausi.R134));
			InvoiceLine.JI_PermitObligation = "2";
			AssertHasMessageError("#Permits = 0, Tariff doesn't have 'hasOptionalPermit' attribute and PermitObbligation = 2", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ValidationMessages.Plausi.R134));
			AssertNoMessageError("#Permits = 0, Tariff doesn't have 'hasOptionalPermit' attribute and PermitObbligation = 2", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ValidationMessages.Plausi.R134));

			InvoiceLine.Permits.AddNew();

			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBeverages;
			InvoiceLine.JI_PermitObligation = "2";
			AssertNoMessageError("#Permits > 0, Tariff doesn't have 'hasOptionalPermit' attribute and PermitObbligation != 0", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeZeroMessage(ValidationMessages.Plausi.R134));
			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
			InvoiceLine.JI_PermitObligation = "0";
			AssertNoMessageError("#Permits > 0, Tariff has 'hasOptionalPermit' attribute and PermitObbligation != 2", permitObligationInfo, ValidationMessages.Plausi.GetPermitObligationMustBeTwoMessage(ValidationMessages.Plausi.R134));
		});
	}

	public void TestCheckJI_NonCustomsLawObligation_R170ab()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawObligationCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var message = ValidationMessages.Plausi.MessageR170ab;

		CombineAssertions("No NCL record", () =>
		{
			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.NotPossible;
			AssertNoMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);

			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.Needed;
			AssertNoMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);

			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.NotNeededAccordingDeclarant;
			AssertNoMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);
		});

		InvoiceLine.NonCustomsLaws.AddNew();
		CombineAssertions("With NCL record", () =>
		{
			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.NotPossible;
			AssertHasMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);

			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.Needed;
			AssertNoMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);

			InvoiceLine.JI_NonCustomsLawObligation = UniversalReferenceConstants.NonCustomsLawObligationCodes.NotNeededAccordingDeclarant;
			AssertHasMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, message);
		});
	}

	public void TestCheckJI_NonCustomsLawObligation_R144abc()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawObligationCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var notExcludedCountry = Core.Constants.CountryCodes.Italy;
		var excludedCountry = Core.Constants.CountryCodes.Vatican;
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffFactory = GetTariffFactory();
		var assignCountryStrategy = GetAssignCountryStrategy();

		var tariffWithNoOptionalNCL = tariffFactory.Invoke(
			"10000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			null,
			null,
			null);
		var tariffWithHasOptionalNCL0 = tariffFactory.Invoke(
			"20000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			new[] { TariffAttributes.HasOptionalNCL, TariffAttributes.Values._0 },
			new[] { notExcludedCountry, excludedCountry },
			new[] { excludedCountry });
		var tariffWithHasOptionalNCL1 = tariffFactory.Invoke(
			"30000000000000",
			Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control,
			"N123",
			CusConditionValueType.NonCustomsLaw,
			"123",
			new[] { TariffAttributes.HasOptionalNCL, TariffAttributes.Values._1 },
			new[] { notExcludedCountry, excludedCountry },
			new[] { excludedCountry });

		var message1 = ValidationMessages.Plausi.MessageR144abc_1;
		var message2 = ValidationMessages.Plausi.MessageR144abc_2;

		InvoiceLine.JI_Tariff = tariffWithNoOptionalNCL.ZZ1_TariffCode;
		assignCountryStrategy.Invoke(notExcludedCountry);
		assertMessage1("No NCL, hasOptionalNCL=null", message1, AssertNoMessageError, AssertHasMessageError, AssertHasMessageError);
		assertMessage2("No NCL, hasOptionalNCL=null", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL0.ZZ1_TariffCode;
		assignCountryStrategy.Invoke(notExcludedCountry);
		assertMessage1("No NCL, hasOptionalNCL=0", message1, AssertNoMessageError, AssertHasMessageError, AssertHasMessageError);
		assertMessage2("No NCL, hasOptionalNCL=0", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		assignCountryStrategy.Invoke(notExcludedCountry);
		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL1.ZZ1_TariffCode;
		assertMessage1("No NCL, hasOptionalNCL=1, country not excluded", message1, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);
		assertMessage2("No NCL, hasOptionalNCL=1, country not excluded", message2, AssertHasMessageError, AssertHasMessageError, AssertNoMessageError);
		assignCountryStrategy.Invoke(excludedCountry);
		assertMessage1("No NCL, hasOptionalNCL=1, country excluded", message1, AssertNoMessageError, AssertHasMessageError, AssertHasMessageError);
		assertMessage2("No NCL, hasOptionalNCL=1, country excluded", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		InvoiceLine.NonCustomsLaws.AddNew();

		InvoiceLine.JI_Tariff = tariffWithNoOptionalNCL.ZZ1_TariffCode;
		assertMessage1("With NCL, hasOptionalNCL=null", message1, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);
		assertMessage2("With NCL, hasOptionalNCL=null", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL0.ZZ1_TariffCode;
		assertMessage1("With NCL, hasOptionalNCL=0", message1, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);
		assertMessage2("With NCL, hasOptionalNCL=0", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		InvoiceLine.JI_Tariff = tariffWithHasOptionalNCL1.ZZ1_TariffCode;
		assignCountryStrategy.Invoke(notExcludedCountry);
		assertMessage1("With NCL, hasOptionalNCL=1", message1, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);
		assertMessage2("With NCL, hasOptionalNCL=1", message2, AssertNoMessageError, AssertNoMessageError, AssertNoMessageError);

		void assertMessage1(string assertionMessage, string message, Action<string, ZPropertyInfo, string> whenObligation0, Action<string, ZPropertyInfo, string> whenObligation1, Action<string, ZPropertyInfo, string> whenObligation2)
		{
			assertMessage(assertionMessage, message, whenObligation0, whenObligation1, whenObligation2);
		}

		void assertMessage2(string assertionMessage, string message, Action<string, ZPropertyInfo, string> whenObligation0, Action<string, ZPropertyInfo, string> whenObligation1, Action<string, ZPropertyInfo, string> whenObligation2)
		{
			assertMessage(assertionMessage, message, whenObligation0, whenObligation1, whenObligation2);
		}

		void assertMessage(string assertionMessage, string plausiMessage, Action<string, ZPropertyInfo, string> whenObligation0, Action<string, ZPropertyInfo, string> whenObligation1, Action<string, ZPropertyInfo, string> whenObligation2)
		{
			InvoiceLine.JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.NotPossible;
			InvoiceLine.Validation.ValidateJI_NonCustomsLawObligation();
			whenObligation0(assertionMessage, InvoiceLine.JI_NonCustomsLawObligationInfo, plausiMessage);

			InvoiceLine.JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.Needed;
			InvoiceLine.Validation.ValidateJI_NonCustomsLawObligation();
			whenObligation1(assertionMessage, InvoiceLine.JI_NonCustomsLawObligationInfo, plausiMessage);

			InvoiceLine.JI_NonCustomsLawObligation = NonCustomsLawObligationCodes.NotNeededAccordingDeclarant;
			InvoiceLine.Validation.ValidateJI_NonCustomsLawObligation();
			whenObligation2(assertionMessage, InvoiceLine.JI_NonCustomsLawObligationInfo, plausiMessage);
		}

		Func<string, string, string, string, string, string[], string[], string[], TariffView>
			GetTariffFactory()
		{
			return (tariffCode, conditionClass, conditionType, valueType, conditionValue, attributes,
					countries, excludedCountries) =>
				tariffTestHelper.CreateImportTariffWithCondition(tariffCode, conditionClass, conditionType,
					valueType, conditionValue, attributes: attributes, countries: countries,
					excludedCountries: excludedCountries);
		}

		Action<ZString> GetAssignCountryStrategy()
		{
			return (ZString country) => InvoiceLine.JI_CountryOfOrigin = country;
		}
	}

	public void TestJI_NonCustomsLawObligation()
	{
		RefCusCodeTestHelper.CreateNonCustomsLawObligationCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.JI_NonCustomsLawObligation = ZString.Empty;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(InvoiceLine.JI_NonCustomsLawObligationInfo, RefCusCodeTestHelper.InvalidNonCustomsLawObligationCode, RefCusCodeTestHelper.ValidNonCustomsLawObligationCode);

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		InvoiceLine.Validation.ValidateJI_NonCustomsLawObligation();
		AssertNoMessageErrorContaining("NCL Obligation is not mandatory for EXP Delcaration", InvoiceLine.JI_NonCustomsLawObligationInfo, MandatoryValidation.YouHaveNotEntered);

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.Validation.ValidateJI_NonCustomsLawObligation();
		AssertNoMessageErrorContaining("NCL Obligation is not mandatory for EDA Delcaration", InvoiceLine.JI_NonCustomsLawObligationInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckJI_StorageCode()
	{
		RefCusCodeTestHelper.CreateStorageTypeCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeMessageError(InvoiceLine.JI_StorageTypeInfo, RefCusCodeTestHelper.InvalidStorageTypeCode, RefCusCodeTestHelper.ValidStorageTypeCode);
	}

	public void TestCheckJI_StorageTypeRule156()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateImportTariffWithAttribute(RefCusTariffTestHelper.ImportTariffBycycle, UniversalReferenceConstants.TariffAttributes.StorageType, UniversalReferenceConstants.TariffAttributes.Values.Yes);
		helper.CreateImportTariff(RefCusTariffTestHelper.ImportTariffBeverages);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		ValidationTestHelper.AssertFieldIsNotMandatory(InvoiceLine.JI_StorageTypeInfo);

		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBycycle;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(InvoiceLine.JI_StorageTypeInfo);

		InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffBeverages;
		ValidationTestHelper.AssertFieldIsNotMandatory(InvoiceLine.JI_StorageTypeInfo);
	}

	public void TestCheckJI_StorageType_R141()
	{
		RefCusCodeTestHelper.CreateStorageTypeCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		foreach (var storageCode in new[] { "2", "3", "4", "5" })
		{
			CombineAssertions($"Test for StorageCode {storageCode}", () =>
			{
				InvoiceLine.JI_StorageType = String.Empty;
				AssertNoMessageErrorContaining("No error when Export Code doesn't exists", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				InvoiceLine.JI_StorageType = storageCode;
				AssertHasMessageErrorContaining($"JI_StorageType={storageCode}", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				InvoiceLine.Validation.ValidateJI_StorageType();
				AssertNoMessageErrorContaining("No error for Import declaration", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				var permit = InvoiceLine.Permits.AddNew();
				permit.CSI_Code = PermitCodes.PeriodicTax;
				permit.CSI_IssuerType = PermitAuthorityCodes.FOCBS_MOT;
				InvoiceLine.Validation.ValidateJI_StorageType();
				AssertNoMessageErrorContaining("Permit with CSI_Code=7 and PermithAuthority=96", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				permit.CSI_Code = PermitCodes.SingleEPermit;
				InvoiceLine.Validation.ValidateJI_StorageType();
				AssertHasMessageErrorContaining("Error when Permit with CSI_Code!=7 and PermithAuthority=96", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				permit.CSI_Code = PermitCodes.PeriodicTax;
				permit.CSI_IssuerType = PermitAuthorityCodes.FOAG;
				InvoiceLine.Validation.ValidateJI_StorageType();
				AssertHasMessageErrorContaining("Error when Permit with CSI_Code=7 and PermithAuthority!=96", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR141);

				InvoiceLine.Permits.RemoveAll();
			});
		}
	}

	public void TestCheckJI_StorageType_R142()
	{
		var helper = new RefCusTariffTestHelper(Factory);
		helper.CreateImportTariffWithAttribute(RefCusTariffTestHelper.ImportTariffMineralOilOthers, UniversalReferenceConstants.TariffAttributes.CustomsFavourHintCode, UniversalReferenceConstants.TariffAttributes.Values._5);
		helper.CreateImportTariffWithAttribute(RefCusTariffTestHelper.ImportTariffMineralOilForUseAsFuel, UniversalReferenceConstants.TariffAttributes.CustomsFavourHintCode, UniversalReferenceConstants.TariffAttributes.Values._4);

		Factory.Save();

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffMineralOilOthers;
			InvoiceLine.JI_StorageType = StorageCodes.ImportHomeWithFinalTax;
			AssertJI_StorageType_R142(true, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, ZString.Empty, ZString.Empty);

			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			InvoiceLine.Validation.ValidateJI_StorageType();
			AssertJI_StorageType_R142(false, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, ZString.Empty, ZString.Empty);

			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffMineralOilForUseAsFuel;
			InvoiceLine.Validation.ValidateJI_StorageType();
			AssertJI_StorageType_R142(false, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilForUseAsFuel, TariffAttributes.Values._4, ZString.Empty, ZString.Empty);

			InvoiceLine.JI_Tariff = RefCusTariffTestHelper.ImportTariffMineralOilOthers;
			var permit = InvoiceLine.Permits.AddNew();
			permit.CSI_Code = PermitCodes.ObligationMineralOilTax;
			permit.CSI_IssuerType = PermitAuthorityCodes.FOCBS_MOT;
			InvoiceLine.JI_StorageType = StorageCodes.ImportHomeWithFinalTax;
			AssertJI_StorageType_R142(false, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, PermitCodes.ObligationMineralOilTax, PermitAuthorityCodes.FOCBS_MOT);

			InvoiceLine.JI_StorageType = StorageCodes.ImportHomeWithProvisionalTax;
			AssertJI_StorageType_R142(false, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithProvisionalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, PermitCodes.ObligationMineralOilTax, PermitAuthorityCodes.FOCBS_MOT);

			InvoiceLine.JI_StorageType = StorageCodes.TransportToApprovedWarehouse;
			AssertJI_StorageType_R142(false, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithProvisionalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, PermitCodes.ObligationMineralOilTax, PermitAuthorityCodes.FOCBS_MOT);

			permit.CSI_Code = PermitCodes.GeneralEPermit;
			InvoiceLine.JI_StorageType = StorageCodes.ImportHomeWithFinalTax;
			AssertJI_StorageType_R142(true, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, PermitCodes.GeneralEPermit, PermitAuthorityCodes.FOCBS_MOT);

			permit.CSI_Code = PermitCodes.ObligationMineralOilTax;
			permit.CSI_IssuerType = PermitAuthorityCodes.FOCBS_Origin;
			InvoiceLine.Validation.ValidateJI_StorageType();
			AssertJI_StorageType_R142(true, SharedJobMessageTypeList.Codes.Import, StorageCodes.ImportHomeWithFinalTax, RefCusTariffTestHelper.ImportTariffMineralOilOthers, TariffAttributes.Values._5, PermitCodes.ObligationMineralOilTax, PermitAuthorityCodes.FOCBS_Origin);
		});
	}

	internal void AssertJI_StorageType_R142(bool hasMessageError, ZString messageType, ZString storageType, ZString tariffCode, ZString favourHint, ZString permitType, ZString permitAuthority)
	{
		if (hasMessageError)
		{
			AssertHasMessageErrorContaining(GetMessageHint(), InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR142);
		}
		else
		{
			AssertNoMessageErrorContaining(GetMessageHint(), InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR142);
		}

		ZString GetMessageHint() => permitType.IsEmpty ?
			$"{messageType} declaration with Storage Code {storageType} / tariff {tariffCode} with customsFavourHint Code {favourHint} / no permits" :
			$"{messageType} declaration with Storage Code {storageType} / tariff {tariffCode} with customsFavourHint Code {favourHint} / permit of Type {permitType} and permitAuthority {permitAuthority}";
	}

	public void TestCheckJI_StorageType_R145a()
	{
		RefCusCodeTestHelper.CreateStorageTypeCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		foreach (var storageCode in new[] { "1", "2" })
		{
			CombineAssertions($"Test for StorageCode {storageCode}", () =>
			{
				InvoiceLine.JI_StorageType = String.Empty;
				AssertNoMessageErrorContaining("No error when Export Code doesn't exists", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);

				InvoiceLine.JI_StorageType = storageCode;
				AssertHasMessageErrorContaining($"JI_StorageType={storageCode}", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				InvoiceLine.Validation.ValidateJI_StorageType();
				AssertNoMessageErrorContaining("No error for Import declaration", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);
				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

				var additionalTax = InvoiceLine.AdditionalTaxes.AddNew();
				foreach (var additionalTaxType in new[] { AdditionalTaxesTypes.CO2CoalAndCoke, AdditionalTaxesTypes.MineralOilGasoline, "620", AdditionalTaxesTypes.MineralOilFuelsAndOthers })
				{
					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "001");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertNoMessageErrorContaining($"Additional Tax with Tariff type {additionalTaxType} and key!=000", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);

					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "000");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertHasMessageErrorContaining($"Additional Tax with Tariff type {additionalTaxType} and key=000", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);
				}

				foreach (var additionalTaxType in new[] { "700", "599", "641" })
				{
					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "001");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertHasMessageErrorContaining($"Additional Tax with Tariff type {additionalTaxType}", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145a);
				}

				InvoiceLine.AdditionalTaxes.RemoveAll();
			});
		}
	}

	public void TestCheckJI_StorageType_R145b()
	{
		RefCusCodeTestHelper.CreateStorageTypeCodeList(Factory);
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		foreach (var storageCode in new[] { "3", "4", "5" })
		{
			CombineAssertions($"Test for StorageCode {storageCode}", () =>
			{
				InvoiceLine.JI_StorageType = String.Empty;
				AssertNoMessageErrorContaining("No error when Export Code doesn't exists", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145b);

				InvoiceLine.JI_StorageType = storageCode;
				AssertNoMessageErrorContaining($"JI_StorageType={storageCode}", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145b);

				var additionalTax = InvoiceLine.AdditionalTaxes.AddNew();
				foreach (var additionalTaxType in new[] { "700", "599", "641", "739", "744" })
				{
					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "001");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertNoMessageErrorContaining($"Additional Tax with Tariff {additionalTaxType}", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145b);
				}

				foreach (var additionalTaxType in new[] { AdditionalTaxesTypes._710, AdditionalTaxesTypes._720, AdditionalTaxesTypes._730, AdditionalTaxesTypes.MineralOilGasoline, "620", AdditionalTaxesTypes.MineralOilFuelsAndOthers, AdditionalTaxesTypes.CO2HeatingOil, "741", AdditionalTaxesTypes.CO2CoalAndCoke })
				{
					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "001");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertHasMessageErrorContaining($"Additional Tax with Tariff {additionalTaxType} that ends with !=000", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145b);
					SetAdditionalTaxTypeAndKey(additionalTax, additionalTaxType, "000");
					InvoiceLine.Validation.ValidateJI_StorageType();
					AssertNoMessageErrorContaining($"Additional Tax with Tariff {additionalTaxType} that ends with 000", InvoiceLine.JI_StorageTypeInfo, ValidationMessages.Plausi.MessageR145b);
				}

				InvoiceLine.AdditionalTaxes.RemoveAll();
			});
		}
	}

	static void SetAdditionalTaxTypeAndKey(CusLineTariffDetail additionalTax, string taxType, string taxKey)
	{
		additionalTax.BZ_Tariff = taxType + "-" + taxKey;
		additionalTax.BZ_TaxType = taxType;
	}

	public void TestCheckJI_NonTradingGoods_R325()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		InvoiceLine.JI_NonTradingGoods = true;

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;
			AssertHasMessageError("When procedure 10", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			var additionalInformation = InvoiceLine.AdditionalInformations.AddNew();
			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Hochsavoyen;
			AssertHasMessageError("When procedure 10", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoodsVAT;
			AssertHasMessageError("When procedure 11", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.NormalDuty;
			AssertNoMessageError("When other procedure", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			InvoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods;

			InvoiceLine.JI_NonTradingGoods = false;
			AssertNoMessageError("Not NonTradingGoods", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			InvoiceLine.JI_NonTradingGoods = true;

			AssertHasMessageError("When neither FreezoneTraffic nor Samnaun", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic;
			AssertHasMessageError("When only FreeZoneTraffic but not Samnaun", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Samnaun;
			AssertNoMessageError("When FreeZoneTraffic and Samnaun", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
			AssertHasMessageError("When not FreeZoneTraffic but Samnaun", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			additionalInformation.CSI_ReferenceNumber = UniversalReferenceConstants.FreeZoneTradeCode.Hochsavoyen;

			AssertHasMessageError("When Import", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.Validation.ValidateJI_NonTradingGoods();
			AssertNoMessageError("Not Import", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR325);
		});
	}

	public void TestCheckJI_NonTradingGoods_R198_NP70167()
	{
		CombineAssertions(() =>
		{
			Assert_R198_NP70167(JobMessageTypeList.Codes.Import, ValidationMessages.Plausi.MessageR198);
			Assert_R198_NP70167(JobMessageTypeList.Codes.Export, PassarValidationMessages.MessageNP70167);
		});

		void Assert_R198_NP70167(string messageType, string messageError)
		{
			Declaration.JE_MessageType = messageType;

			InvoiceLine.InAndOutwardProcessingRepair = ZBool.True;
			InvoiceLine.JI_NonTradingGoods = ZBool.True;
			AssertNoMessageError($"{messageType} Repair=true", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

			InvoiceLine.JI_NonTradingGoods = ZBool.False;
			AssertHasMessageError($"{messageType} Repair=true", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

			InvoiceLine.InAndOutwardProcessingRepair = ZBool.False;
			AssertNoMessageError($"{messageType} Repair=false (should be triggered by Repair)", InvoiceLine.JI_NonTradingGoodsInfo, messageError);
		}
	}

	public void TestCheckJI_NonTradingGoods_R261()
	{
		CombineAssertions(() =>
		{
			void SetProperties(string messageType = JobMessageTypeList.Codes.Import, string procedure = ProcedureCodesEdec.RefinementTransportation, string direction = InAndOutwardProcessingDirectionCodes.Passive, string processingType = InAndOutwardProcessingProcessTypesEdec.SimplifiedProcedure, bool repair = false, bool nonTradingGoods = false)
			{
				Declaration.JE_MessageType = messageType;
				InvoiceLine.JI_Procedure = procedure;
				InvoiceLine.InAndOutwardProcessingDirection = direction;
				InvoiceLine.InAndOutwardProcessingProcessType = processingType;
				InvoiceLine.InAndOutwardProcessingRepair = repair;
				InvoiceLine.JI_NonTradingGoods = nonTradingGoods;
			}

			SetProperties();
			AssertNoMessageError("No error", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true);
			AssertHasMessageError("NonTradingGoods=true", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true, messageType: JobMessageTypeList.Codes.Export);
			AssertNoMessageError("Not Import", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true, procedure: ProcedureCodesEdec.NormalDuty);
			AssertNoMessageError("Procedure not 02", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true, direction: InAndOutwardProcessingDirectionCodes.Active);
			AssertNoMessageError("Direction not 2", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true, processingType: InAndOutwardProcessingProcessTypesEdec.DueProcedure);
			AssertNoMessageError("ProcessingType not 1", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);

			SetProperties(nonTradingGoods: true, repair: true);
			AssertNoMessageError("Repair ticked", InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR261_2);
		});
	}

	public void TestCheckJI_NonTradingGoods_NS30003() => CombineAssertions(() =>
	{
		var message = PassarValidationMessages.MessageNS30003_Ticked("Non Commercial Goods");
		PlausiValidationTestHelper.AssertNS30003(EntryInstruction, InvoiceLine.JI_NonTradingGoodsInfo, message);
	});

	public void TestCheckJI_NonTradingGoods_NP70169()
	{
		RefCusCodeTestHelper.CreateProcedureList(Factory);
		var messageError = PassarValidationMessages.MessageNP70169;

		InvoiceLine.InAndOutwardProcessingRepair = false;
		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode41Exp;
		InvoiceLine.JI_NonTradingGoods = true;
		AssertHasMessageError("When Procedure is 41, is Non Trading Goods and isn't repair, error", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		InvoiceLine.Validation.ValidateJI_NonTradingGoods();
		AssertHasMessageError("When Procedure is 50, is Non Trading Goods and isn't repair, error", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCodeDefaultValueExp;
		InvoiceLine.Validation.ValidateJI_NonTradingGoods();
		AssertNoMessageError("When Procedure isn't 41 or 50, is Non Trading Goods and isn't repair, no error", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

		EntryInstruction.CEI_Procedure = RefCusCodeTestHelper.ProcedureCode50Exp;
		InvoiceLine.InAndOutwardProcessingRepair = true;
		InvoiceLine.Validation.ValidateJI_NonTradingGoods();
		AssertNoMessageError("When Procedure is 50, is Non Trading Goods and is repair, no error", InvoiceLine.JI_NonTradingGoodsInfo, messageError);

		InvoiceLine.InAndOutwardProcessingRepair = false;
		InvoiceLine.JI_NonTradingGoods = false;
		AssertNoMessageError("When Procedure isn 50, isn't Non Trading Goods and isn't repair, no error", InvoiceLine.JI_NonTradingGoodsInfo, messageError);
	}

	public void TestCheck_JI_Tariff_R285()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var testHelper = new RefCusTariffTestHelper(Factory);
		var special9999Tariff = testHelper.CreateImportTariff(Tariffs.NegligibleImportTariff);
		var nonSpecialTariff = testHelper.CreateImportTariff("1111000000");

		CombineAssertions(() =>
		{
			InvoiceLine.JI_Tariff = special9999Tariff.ZZ1_TariffCode;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
			InvoiceLine.JI_NonTradingGoods = false;
			AssertHasMessageError(GetMessage(), InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR285);

			InvoiceLine.JI_Tariff = nonSpecialTariff.ZZ1_TariffCode;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
			InvoiceLine.JI_NonTradingGoods = false;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR285);

			InvoiceLine.JI_Tariff = special9999Tariff.ZZ1_TariffCode;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.NormalDuty;
			InvoiceLine.JI_NonTradingGoods = false;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR285);

			InvoiceLine.JI_Tariff = special9999Tariff.ZZ1_TariffCode;
			InvoiceLine.JI_Procedure = ProcedureCodesEdec.DutyFree;
			InvoiceLine.JI_NonTradingGoods = true;
			AssertNoMessageError(GetMessage(), InvoiceLine.JI_NonTradingGoodsInfo, ValidationMessages.Plausi.MessageR285);
		});

		string GetMessage() => $"Tariff - {InvoiceLine.JI_Tariff}, Procedure - {InvoiceLine.JI_Procedure}, NonTradingGoods - {InvoiceLine.JI_NonTradingGoods}";
	}

	public void TestCheckJI_NonTradingGoods_NP70000()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var messageError = PassarValidationMessages.MessageNP70000;

		CombineAssertions(() =>
		{
			Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;
			InvoiceLine.JI_NonTradingGoods = false;
			InvoiceLine.Validation.ValidateJI_NonTradingGoods();
			AssertNoMessageError(GetAssertionMessage(), InvoiceLine.JI_NonTradingGoodsInfo, messageError);

			Declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Switzerland;
			InvoiceLine.Validation.ValidateJI_NonTradingGoods();
			AssertHasMessageErrorContaining(GetAssertionMessage(), InvoiceLine.JI_NonTradingGoodsInfo, messageError);

			InvoiceLine.JI_NonTradingGoods = true;
			InvoiceLine.Validation.ValidateJI_NonTradingGoods();
			AssertNoMessageError(GetAssertionMessage(), InvoiceLine.JI_NonTradingGoodsInfo, messageError);
		});

		string GetAssertionMessage() => $"JE_GoodsDestination={Declaration.JE_GoodsDestination} JI_NonTradingGoods={InvoiceLine.JI_NonTradingGoods}";
	}

	public void TestCheckJI_GoodsReturned_NP70066() => CombineAssertions(() =>
	{
		const string anyProcedureCode = "99";

		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		AssertErrorMessage(false);
		AssertErrorMessage(true, nonTradingGoods: true);
		AssertErrorMessage(true, procedureCode: anyProcedureCode);
		AssertErrorMessage(false, goodsReturned: false, nonTradingGoods: true);
		AssertErrorMessage(false, goodsReturned: false, procedureCode: anyProcedureCode);
		AssertErrorMessage(false, messageType: CHJobMessageTypeList.Codes.Import, nonTradingGoods: true);
		AssertErrorMessage(false, messageType: CHJobMessageTypeList.Codes.Import, procedureCode: anyProcedureCode);
		AssertErrorMessage(false, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation, nonTradingGoods: true);
		AssertErrorMessage(false, messageType: CHJobMessageTypeList.Codes.ExportDeclarationActivation, procedureCode: anyProcedureCode);

		void AssertErrorMessage(bool errorExpected, string messageType = CHJobMessageTypeList.Codes.Export, bool goodsReturned = true, bool nonTradingGoods = false, string procedureCode = ProcedureCodesPassar.ExportFromFreeCirculation)
		{
			Declaration.JE_MessageType = messageType;
			entryInstruction.CEI_Procedure = procedureCode;
			InvoiceLine.JI_NonTradingGoods = nonTradingGoods;
			InvoiceLine.JI_GoodsReturned = goodsReturned;
			var assertionMessage = $"MessageType={messageType} NonTradingGoods={nonTradingGoods} ProcedureCode={procedureCode}";
			if (errorExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_GoodsReturnedInfo, PassarValidationMessages.MessageNP70066);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_GoodsReturnedInfo, PassarValidationMessages.MessageNP70066);
			}
		}
	});

	public void TestCheckJI_GoodsReturned_NS30003() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNS30003_Ticked(InvoiceLine.JI_GoodsReturnedInfo.HumanReadableName);
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		PlausiValidationTestHelper.AssertNS30003(entryInstruction, InvoiceLine.JI_GoodsReturnedInfo, messageError);
	});

	public void TestCheckJI_CusNumber_NS30003() => CombineAssertions(() =>
	{
		var messageError = PassarValidationMessages.MessageNS30003_NotEmpty(InvoiceLine.JI_CusNumberInfo.HumanReadableName);
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		InvoiceLine.JI_CEI = entryInstruction.PK;

		PlausiValidationTestHelper.AssertNS30003(entryInstruction, InvoiceLine.JI_CusNumberInfo, messageError);
	});

	public void TestCheckJI_RefundType_NP70168()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		var expectedMessageError = PassarValidationMessages.MessageNP70168_NotEntered;
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertHasMessageError("assert if Refund type equals to 1 no AdditionalInformation exist error", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.ReturnedGoodsWithRefundRequest;
		AssertNoMessageError("assert if Refund type not equals to 1 no AdditionalInformation exist no error", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		var additionalInformation = InvoiceLine.AdditionalInformations.AddNew();
		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertHasMessageError("assert if Refund type equals to 1 no AdditionalInformation A1301 exist error", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		var additionalInformation2 = InvoiceLine.AdditionalInformations.AddNew();
		additionalInformation2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		InvoiceLine.Validation.ValidateJI_RefundType();
		AssertHasMessageError("assert if Refund type equals to 1 no AdditionalInformation A1301 exist error", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);
	}

	public void TestCheck_JI_RefundType_NP70195() => CombineAssertions(() =>
	{
		var expectedErrorA1101 = PassarValidationMessages.MessageNP70195(UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol);
		var expectedErrorA1102 = PassarValidationMessages.MessageNP70195(UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol);
		var additionaInformation = InvoiceLine.AdditionalInformations.AddNew();
		additionaInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.WarehouseNumber;
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.RequestForAlcohol;
		AssertHasMessageErrorContaining("if RefoundType == 2 and no AdditionalInformation.Code == A1101, errorA1101", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1101);
		AssertHasMessageErrorContaining("if RefoundType == 2 and no AdditionalInformation.Code == A1102, errorA1102", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1102);

		var additionaInformation2 = InvoiceLine.AdditionalInformations.AddNew();
		additionaInformation2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.VolAlcohol;
		InvoiceLine.Validation.ValidateJI_RefundType();
		AssertNoMessageErrorContaining("if RefoundType == 2 and any AdditionalInformation.Code == A1101, no errorA1101", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1101);
		AssertHasMessageErrorContaining("if RefoundType == 2 and no AdditionalInformation.Code == A1102, errorA1102", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1102);

		var additionaInformation3 = InvoiceLine.AdditionalInformations.AddNew();
		additionaInformation3.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.LitresAlcohol;
		InvoiceLine.Validation.ValidateJI_RefundType();
		AssertNoMessageErrorContaining("if RefoundType == 2 and any AdditionalInformation.Code == A1102, no errorA1102", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1102);

		additionaInformation2.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
		additionaInformation3.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic;
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertNoMessageErrorContaining("if RefoundType != 2 and no AdditionalInformation.Code == A1101, no errorA1101", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1101);
		AssertNoMessageErrorContaining("if RefoundType != 2 and no AdditionalInformation.Code == A1102, no errorA1102", InvoiceLine.JI_RefundTypeInfo, expectedErrorA1102);
	});

	public void TestCheckJI_RefundType_NP70197()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		string expectedMessageError = PassarValidationMessages.MessageNP70197_NotEntered;

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.OtherRefunds;
		AssertNoMessageError("Expected no NP70197 rule error when Refund type is not equal to 3", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.RefundOfAlcoholOnBeer;
		AssertNoMessageError("Expected no NP70197 rule error when Declaration type is not Export, even if refund type equals to 3 and no AdditionalInformation A1102 provided", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		InvoiceLine.Validation.ValidateJI_RefundType();
		AssertHasMessageError("When Refund type equals to 3 and no AdditionalInformation provided, expected NP70197 rule error on JI_RefundTypeInfo", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);

		var additionalInformation = InvoiceLine.AdditionalInformations.AddNew();
		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.AlcoholOnBeerRefundLiters;
		InvoiceLine.Validation.ValidateJI_RefundType();
		AssertNoMessageError("Expected no NP70197 rule error when Refund type equals to 3 and AdditionalInformation A1102 provided", InvoiceLine.JI_RefundTypeInfo, expectedMessageError);
	}

	public void TestCheckJI_RefundType_NP70175() => CombineAssertions(() =>
	{
		string[] validCodes = { AdditionalInformationTypeCodes.A1401,
				AdditionalInformationTypeCodes.ProductMainGroup,
				AdditionalInformationTypeCodes.ProductSubgroup,
				AdditionalInformationTypeCodes.A1404,
				AdditionalInformationTypeCodes.A1405,
				AdditionalInformationTypeCodes.A1406 };
		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertNoMessageError("if no additional information and not tobacco, no error", InvoiceLine.JI_RefundTypeInfo, PassarValidationMessages.MessageNP70175);

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		AssertHasMessageError("if no additional information and JI_RefundType = 6, error", InvoiceLine.JI_RefundTypeInfo, PassarValidationMessages.MessageNP70175);

		InvoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoProductsExTaxWarehouse;
		AssertHasMessageError("if no additional information and JI_RefundType = 7, error", InvoiceLine.JI_RefundTypeInfo, PassarValidationMessages.MessageNP70175);
		AdditionalInformation addInfo;

		for (var i = 0; i < validCodes.Length; i++)
		{
			addInfo = InvoiceLine.AdditionalInformations.AddNew();
			addInfo.CSI_Code = validCodes[i];
			InvoiceLine.Validation.ValidateJI_RefundType();

			if (i == validCodes.Length - 1)
			{
				AssertNoMessageError($"if all required add info insert and JI_RefundType is Tobacco , error", InvoiceLine.JI_RefundTypeInfo, PassarValidationMessages.MessageNP70175);
			}
			else
			{
				AssertHasMessageError($"if missing additional information {validCodes.Skip(i + 1)} and JI_RefundType is Tobacco, error", InvoiceLine.JI_RefundTypeInfo, PassarValidationMessages.MessageNP70175);
			}
		}
	});

	public void TestCheckJI_RateOverride_R174() => CombineAssertions(() =>
	{
		const string message = "Rate override must be ticked and the rate must be captured (R174).";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		AssertMessage(true, ProcedureCodesEdec.ReturnedGoods);
		AssertMessage(true, ProcedureCodesEdec.ReturnedGoodsVAT);
		AssertMessage(false, ProcedureCodesEdec.ReturnedGoods, rateOverride: true);
		AssertMessage(false, ProcedureCodesEdec.ReturnedGoodsVAT, rateOverride: true);

		foreach (var procedure in typeof(ProcedureCodesEdec).GetConstantValues().Where(x => x != ProcedureCodesEdec.ReturnedGoods && x != ProcedureCodesEdec.ReturnedGoodsVAT))
		{
			AssertMessage(false, procedure);
		}

		void AssertMessage(bool messageExpected, string procedure, bool rateOverride = false)
		{
			InvoiceLine.JI_Procedure = procedure;
			InvoiceLine.JI_RateOverride = rateOverride;
			var assertionMessage = $"RateOverride={rateOverride}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
		}
	});

	public void TestCheckJI_RateOverride_R181() => CombineAssertions(() =>
	{
		const string message = "Rate override must be ticked and the rate must be 0 (R181).";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		AssertMessage(true, rateOverride: false);
		AssertMessage(true, overrideRate: 5);

		foreach (var procedure in typeof(ProcedureCodesEdec).GetConstantValues().Where(x => x != ProcedureCodesEdec.DutyFree))
		{
			AssertMessage(false, procedure: procedure, rateOverride: false);
			AssertMessage(false, procedure: procedure, overrideRate: 5);
		}

		void AssertMessage(bool messageExpected, string procedure = ProcedureCodesEdec.DutyFree, bool rateOverride = true, decimal overrideRate = 0)
		{
			InvoiceLine.JI_Procedure = procedure;
			InvoiceLine.JI_RateOverride = rateOverride;
			InvoiceLine.JI_OverriddenRate = overrideRate;
			InvoiceLine.Validation.ValidateJI_RateOverride();
			var assertionMessage = $"RateOverride={rateOverride} OverrideRate={overrideRate}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
		}
	});

	public void TestCheckJI_RateOverride_R191() => CombineAssertions(() =>
	{
		const string message = "Rate override must be ticked and the rate must be 0 (R191).";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		AssertMessage(true, rateOverride: false);
		AssertMessage(true, overrideRate: 5);

		AssertMessage(false, inAndOutwardProcessingDirection: InAndOutwardProcessingDirectionCodes.Passive, rateOverride: false);
		AssertMessage(false, inAndOutwardProcessingDirection: InAndOutwardProcessingDirectionCodes.Passive, overrideRate: 5);

		AssertMessage(false, inAndOutwardProcessingBillingType: InAndOutwardProcessingBillingTypesEdec.RefundProcedure, rateOverride: false);
		AssertMessage(false, inAndOutwardProcessingBillingType: InAndOutwardProcessingBillingTypesEdec.RefundProcedure, overrideRate: 5);

		foreach (var procedure in typeof(ProcedureCodesEdec).GetConstantValues().Where(x => x != ProcedureCodesEdec.RefinementTransportation))
		{
			AssertMessage(false, procedure: procedure, rateOverride: false);
			AssertMessage(false, procedure: procedure, overrideRate: 5);
		}

		void AssertMessage(bool messageExpected, string procedure = ProcedureCodesEdec.RefinementTransportation, string inAndOutwardProcessingDirection = InAndOutwardProcessingDirectionCodes.Active, string inAndOutwardProcessingBillingType = InAndOutwardProcessingBillingTypesEdec.SuspensiveProcedure, bool rateOverride = true, decimal overrideRate = 0)
		{
			InvoiceLine.JI_Procedure = procedure;
			InvoiceLine.InAndOutwardProcessingDirection = inAndOutwardProcessingDirection;
			InvoiceLine.InAndOutwardProcessingBillingType = inAndOutwardProcessingBillingType;
			InvoiceLine.JI_RateOverride = rateOverride;
			InvoiceLine.JI_OverriddenRate = overrideRate;
			InvoiceLine.Validation.ValidateJI_RateOverride();
			var assertionMessage = $"InAndOutwardProcessingDirection={inAndOutwardProcessingDirection} InAndOutwardProcessingBillingType={inAndOutwardProcessingBillingType} RateOverride={rateOverride} OverrideRate={overrideRate}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
		}
	});

	public void TestCheckJI_RateOverride_R247() => CombineAssertions(() =>
	{
		const string message = "Rate override must be ticked and the rate must be 0 (R247).";

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		AssertMessage(true, rateOverride: false);
		AssertMessage(true, overrideRate: 5);

		foreach (var taxType in typeof(TaxCodes).GetConstantValues().Where(x => x != TaxCodes.DeferredTaxation))
		{
			AssertMessage(false, taxType: taxType, rateOverride: false);
			AssertMessage(false, taxType: taxType, overrideRate: 5);
		}

		void AssertMessage(bool messageExpected, string taxType = TaxCodes.DeferredTaxation, bool rateOverride = true, decimal overrideRate = 0)
		{
			InvoiceLine.JI_ZZF_NKTaxType = taxType;
			InvoiceLine.JI_RateOverride = rateOverride;
			InvoiceLine.JI_OverriddenRate = overrideRate;
			InvoiceLine.Validation.ValidateJI_RateOverride();
			var assertionMessage = $"RateOverride={rateOverride} OverrideRate={overrideRate}";
			if (messageExpected)
			{
				AssertHasMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
			else
			{
				AssertNoMessageError(assertionMessage, InvoiceLine.JI_RateOverrideInfo, message);
			}
		}
	});

	public void TestCheckJI_RateOverride_R249d() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		foreach (var tariff in TariffNumbers.TobaccoQuantityBasedTaxationTariffNumbers)
		{
			InvoiceLine.JI_Tariff = tariff + "000911";
			InvoiceLine.JI_RateOverride = false;
			AssertHasMessageError($"Tariff = {InvoiceLine.JI_Tariff} and RateOverride = false", InvoiceLine.JI_RateOverrideInfo, ValidationMessages.Plausi.MessageR249d);
			InvoiceLine.JI_RateOverride = true;
			AssertNoMessageError($"Tariff = {InvoiceLine.JI_Tariff} and RateOverride = true with overridden value = 0", InvoiceLine.JI_RateOverrideInfo, ValidationMessages.Plausi.MessageR249d);
			InvoiceLine.JI_OverriddenRate = 15;
			AssertHasMessageError($"Tariff = {InvoiceLine.JI_Tariff} and RateOverride = true with overridden value not zero", InvoiceLine.JI_RateOverrideInfo, ValidationMessages.Plausi.MessageR249d);
		}
		InvoiceLine.JI_Tariff = $"{TariffNumbers.CigarCherootsCigarillosContainingTobacco}000999";
		invoiceLine.Validation.ValidateJI_RateOverride();
		AssertNoMessageError("Statistical code not equal to 911", InvoiceLine.JI_RateOverrideInfo, ValidationMessages.Plausi.MessageR249d);

		InvoiceLine.JI_Tariff = $"{TariffNumbers.ProductsContainingTobaccoOther}000911";
		invoiceLine.Validation.ValidateJI_RateOverride();
		AssertNoMessageError("Tariff not in set for rule R249", InvoiceLine.JI_RateOverrideInfo, ValidationMessages.Plausi.MessageR249d);
	});

	public void TestPackagesPivot_CH0003() => CombineAssertions(() =>
	{
		var package = Declaration.Packages.AddNew();
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;

		var otherPackage = Declaration.Packages.AddNew();
		otherPackage.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;

		var bindingLine1 = InvoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine1.IsLinked = false;

		var messageError = PassarValidationMessages.MessageCH0003;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		InvoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError(InvoiceLine, messageError);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		InvoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError(InvoiceLine, messageError);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		InvoiceLine.Validation.ValidateAll();
		AssertHasRowMessageError(InvoiceLine, messageError);

		bindingLine1.IsLinked = true;
		InvoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError(InvoiceLine, messageError);
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		InvoiceLine.Validation.ValidateAll();
		AssertNoRowMessageError(InvoiceLine, messageError);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= CreateNewInvoiceLine();
	JobComInvoiceLine invoiceLine;

	CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction entryInstruction;

	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invLine = InvoiceHeader.InvoiceLines.AddNew();
		invLine.JI_CEI = EntryInstruction.PK;
		return invLine;
	}
}
