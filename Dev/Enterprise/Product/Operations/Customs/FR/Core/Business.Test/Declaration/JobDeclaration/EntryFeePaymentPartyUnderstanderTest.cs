using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class EntryFeePaymentPartyUnderstanderTest : TestCaseWithFactory
	{
		#region ShouldBrokerPayThisFee
		public void TestShouldBrokerPayThisFee()
		{
			CreateCusRateCode();
			var logger = new DetailedLoggerForTest();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_PaymentMethod = "A";
			dec.JE_DeclarantType = "IND";
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryheader.MergedLines.AddNew();
			var methodOfPayment = "2";
			var lineFeeA00deferred = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 60m, methodOfPayment);
			var lineFeeA00fas = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 9m, methodOfPayment);
			var lineFeeB00deferred = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 71m, methodOfPayment);
			var lineFeeB00fas = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat, 8m, methodOfPayment);
			var lineFeeA30fas = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 1m, methodOfPayment);
			var lineFeeA40 = AddFee(cusEntryLine, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, 1m, methodOfPayment);  // Not configured in the RefZZ DB, so won't be found by the charge getter
			var lineFeeXYZ = AddFee(cusEntryLine, "XYZ", 89m, methodOfPayment);  // fake fee

			var chargesGetter = CargoWise.Common.ServiceLocator.GetService<ICustomsCharges>(dec);
			var charges = chargesGetter.GetCustomsCharges(logger);

			AssertEquals("A00, B00, A30 will be mapped to DTY, VAT and ADD, Charges found should be 3.", 3, charges.Length);
			AssertEquals("Find DTY", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.Dty));
			AssertEquals("Find VAT", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.Vat));
			AssertEquals("Find ADD", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.AntiDumpingDuty));
			Assert("DTY Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and 2MoP=A is always paid by broker - include in rating"));
			logger.Logs.Clear();

			var autoRatingInfo = dec.RatingAdapter as IAutoRatingCustomsInfo;
			AssertNotNull(autoRatingInfo);
		}

		public void TestShouldBrokerPayThisFeeIfMethodsOfPaymentIs1()
		{
			var orgHeaderWithDAN = Factory.New<OrgHeader>();
			orgHeaderWithDAN.OH_Code = "HasDAN";
			orgHeaderWithDAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("DAN", "11111111", "FR");
			var orgHeaderWithoutDAN = Factory.New<OrgHeader>();
			orgHeaderWithoutDAN.OH_Code = "NoDAN";
			CreateCusRateCode();

			var logger = new DetailedLoggerForTest();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_PaymentMethod = "R";
			dec.JE_DefermentAccountNumber = "11111111";
			var methodOfPayment = "1";
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			dec.JE_OA_DeclarantAddress = orgHeaderWithDAN.MainAddress.PK;
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R and matching Declarant's DAN=11111111, include in rating"));
			logger.Logs.Clear();

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_OA_DeclarantAddress = ZGuid.Empty;
			dec.JE_OH_Importer = orgHeaderWithDAN.PK;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R but matching Importer's DAN=11111111, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("No matching DAN in Declarant and Importer_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R and no matching DAN=11111111 in Declarant and Importer, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_OH_Importer = orgHeaderWithoutDAN.PK;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("No matching DAN in Declarant and Importer_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R and no matching DAN=11111111 in Declarant and Importer, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_OA_DeclarantAddress = orgHeaderWithDAN.MainAddress.PK;
			dec.JE_PaymentMethod = "A";
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("JE_PaymentMethod is not equal to R_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=A and matching Declarant's DAN=11111111, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_PaymentMethod = "R";
			dec.JE_DefermentAccountNumber = "2222222";
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("No matching DAN in Declarant and Importer_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R and no matching DAN=2222222 in Declarant and Importer, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_DefermentAccountNumber = ZString.Empty;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("JE_DefermentAccountNumber is empty_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1, 2MoP=R and no DAN, exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_DefermentAccountNumber = "11111111";
			dec.JE_PaymentMethod = ZString.Empty;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert("JE_PaymentMethod is empty_DTY", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with 1MoP=1 and no 2MoP is always exclude in rating"));
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=1 is always included in rating"));
			logger.Logs.Clear();
		}

		public void TestShouldBrokerPayThisFeeIfMethodsOfPaymentIs2()
		{
			CreateCusRateCode();
			var logger = new DetailedLoggerForTest();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_PaymentMethod = "A";
			dec.JE_DeclarantType = "IND";
			var methodOfPayment = "2";
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_PaymentMethod = "R";
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and 2MoP=R, exclude in rating"));

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_PaymentMethod = "M";
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and 2MoP=M is always paid by broker - include in rating"));

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_DeclarantType = ZString.Empty;
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and 2MoP=M is always paid by broker - include in rating"));

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));
			logger.Logs.Clear();

			dec.JE_DeclarantType = "DIR";
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and 2MoP=M is always paid by broker - include in rating"));

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));

			dec.JE_PaymentMethod = ZString.Empty;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY, 1MoP=2 and no 2MoP, exclude in rating"));

			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with 1MoP=2 is always included in rating"));
			logger.Logs.Clear();
		}

		public void TestShouldBrokerPayThisFeeIfMethodsOfPaymentIs6()
		{
			var orgHeaderWithALT = Factory.New<OrgHeader>();
			orgHeaderWithALT.OH_Code = "HasALT";
			orgHeaderWithALT.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("ALT", "11111111", "FR");
			var orgHeaderWithoutALT = Factory.New<OrgHeader>();
			orgHeaderWithoutALT.OH_Code = "NoALT";
			CreateCusRateCode();

			var logger = new DetailedLoggerForTest();
			var dec = Factory.New<JobDeclaration>();
			dec.ZG_VATDeferType = "A";
			dec.ZG_VATDeferNumber = "11111111";
			var methodOfPayment = "6";
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			dec.JE_OA_DeclarantAddress = orgHeaderWithALT.MainAddress.PK;
			AssertEquals(true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=6 and matching Declarant's ALT=11111111, include in rating"));

			dec.JE_OA_DeclarantAddress = ZGuid.Empty;
			dec.JE_OH_Importer = orgHeaderWithALT.PK;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=6 but matching Importer's ALT=11111111, exclude in rating"));
			logger.Logs.Clear();

			dec.ZG_VATDeferNumber = "22222222";
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=6 and no matching ALT=22222222 in Declarant and Importer, exclude in rating"));
			logger.Logs.Clear();

			dec.ZG_VATDeferNumber = "11111111";
			dec.JE_OA_DeclarantAddress = orgHeaderWithoutALT.MainAddress.PK;
			dec.JE_OH_Importer = orgHeaderWithoutALT.PK;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=6 and no matching ALT=11111111 in Declarant and Importer, exclude in rating"));
			logger.Logs.Clear();

			dec.JE_OA_DeclarantAddress = orgHeaderWithALT.MainAddress.PK;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with MoP=6 is always exclude in rating"));

			dec.ZG_VATDeferNumber = ZString.Empty;
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, methodOfPayment, logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=6 and no ALT, exclude in rating"));
			logger.Logs.Clear();
		}

		public void TestShouldBrokerPayThisFeeIfMethodsOfPaymentIsTheOthers()
		{
			CreateCusRateCode();
			var logger = new DetailedLoggerForTest();
			var dec = Factory.New<JobDeclaration>();
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			AssertEquals(false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, "3", logger));
			Assert(logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=3 is always exclude for autorating"));
			logger.Logs.Clear();
		}

		CusEntryLineFee AddFee(CusEntryLine line, ZString feeType, ZDecimal amount, ZString methodOfPayment)
		{
			var fee = line.Fees.AddOrUpdate(feeType, amount);
			fee.CF_MethodOfPayment = methodOfPayment;
			return fee;
		}

		void CreateCusRateCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, RefCusRateTypes.Dty, "Duty");
			var cvd = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, RefCusRateTypes.CountervailingDuty, "Countervailing Duty");
			var add = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.France, RefCusRateTypes.AntiDumpingDuty, "Anti-Dumping Duty");
			dut.ZZR_IsPayable = true;
			cvd.ZZR_IsPayable = true;
			add.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, cvd.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty, cvd.PK);
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Y", "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Yes);
			Factory.Save();
		}
		#endregion
	}
}
