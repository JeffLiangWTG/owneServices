using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CalculateFreightBizObj))]
	class CalculateFreightBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("No freight charge is available, Currency should be defaulted to EUR.", "EUR", bizObj.Currency);
			AssertEquals("No insurance charge is available, InsuranceCurrency should be defaulted to EUR.", "EUR", bizObj.InsuranceCurrency);

			var airFreightCharge = invoice.Charges.AddNew();
			airFreightCharge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			airFreightCharge.J7_Amount = 400m;
			airFreightCharge.J7_RX_NKCurrency = "USD";

			var airInsuranceCharge = invoice.Charges.AddNew();
			airInsuranceCharge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			airInsuranceCharge.J7_Amount = 325m;
			airInsuranceCharge.J7_RX_NKCurrency = "AUD";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should be defaulted with common currency code of charges of type AFT found.", "USD", bizObj.Currency);
			AssertEquals("InsuranceCurrency should be defaulted with common currency code of charges of type ANS found.", "AUD", bizObj.InsuranceCurrency);

			var airFreightCharge2 = invoice.Charges.AddNew();
			airFreightCharge2.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			airFreightCharge2.J7_Amount = 400m;
			airFreightCharge2.J7_RX_NKCurrency = "EUR";

			var airInsuranceCharge2 = invoice.Charges.AddNew();
			airInsuranceCharge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			airInsuranceCharge2.J7_Amount = 325m;
			airInsuranceCharge2.J7_RX_NKCurrency = "EUR";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should not be defaulted because all charges of type AFT found don't share same currency.", ZString.Empty, bizObj.Currency);
			AssertEquals("InsuranceCurrency should not be defaulted because all charges of type ANS found don't share same currency.", ZString.Empty, bizObj.InsuranceCurrency);
		}

		public void TestAmountAndInsuranceAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("No freight charge is available, Freight base amount should be 0.", 0m, bizObj.Amount);
			AssertEquals("No insurance charge is available, Insrance base amount should be 0.", 0m, bizObj.InsuranceAmount);

			var landFreightCharge1 = invoice.Charges.AddNew();
			landFreightCharge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
			landFreightCharge1.J7_Amount = 100m;
			landFreightCharge1.J7_RX_NKCurrency = "EUR";

			var landFreightCharge2 = invoice.Charges.AddNew();
			landFreightCharge2.J7_ChargeType = UCCCustomsChargeTypeList.Codes.TransportCostsCharge;
			landFreightCharge2.J7_Amount = 200m;
			landFreightCharge2.J7_RX_NKCurrency = "EUR";

			var airFreightCharge1 = invoice.Charges.AddNew();
			airFreightCharge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			airFreightCharge1.J7_Amount = 400m;
			airFreightCharge1.J7_RX_NKCurrency = "EUR";

			var airFreightCharge2 = invoice.Charges.AddNew();
			airFreightCharge2.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			airFreightCharge2.J7_Amount = 600m;
			airFreightCharge2.J7_RX_NKCurrency = "EUR";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Freight base amount should be the sum of charges of type AFT.", 1000m, bizObj.Amount);

			var airInsuranceCharge1 = invoice.Charges.AddNew();
			airInsuranceCharge1.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			airInsuranceCharge1.J7_Amount = 325m;
			airInsuranceCharge1.J7_RX_NKCurrency = "EUR";

			var airInsuranceCharge2 = invoice.Charges.AddNew();
			airInsuranceCharge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			airInsuranceCharge2.J7_Amount = 125m;
			airInsuranceCharge2.J7_RX_NKCurrency = "EUR";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Insurance charges should be excluded from freight base amount.", 1000m, bizObj.Amount);
			AssertEquals("Insurance base amount should be the sum of charges of type ANS.", 450m, bizObj.InsuranceAmount);

			var airFreightCharge3 = invoice.Charges.AddNew();
			airFreightCharge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			airFreightCharge3.J7_Amount = 600m;
			airFreightCharge3.J7_RX_NKCurrency = "USD";

			var airInsuranceCharge3 = invoice.Charges.AddNew();
			airInsuranceCharge3.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			airInsuranceCharge3.J7_Amount = 125m;
			airInsuranceCharge3.J7_RX_NKCurrency = "USD";

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Transport charges should not be summed because of insconsitency in currencies", 0m, bizObj.Amount);
			AssertEquals("Insurance charges should not be summed because of insconsitency in currencies", 0m, bizObj.InsuranceAmount);

			airFreightCharge1.J7_RX_NKCurrency = ZString.Empty;
			airFreightCharge2.J7_RX_NKCurrency = ZString.Empty;
			airFreightCharge3.J7_RX_NKCurrency = ZString.Empty;
			airInsuranceCharge1.J7_RX_NKCurrency = ZString.Empty;
			airInsuranceCharge2.J7_RX_NKCurrency = ZString.Empty;
			airInsuranceCharge3.J7_RX_NKCurrency = ZString.Empty;

			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Transport charges should be summed because currencies are consistent even if empty.", 1600m, bizObj.Amount);
			AssertEquals("Insurance charges should be summed because currencies are consistent even if empty.", 575m, bizObj.InsuranceAmount);
		}

		public void TestCreatedChargesIsIncludedInITOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			bizObj.Amount = 1000m;
			bizObj.InsuranceAmount = 2000m;
			bizObj.Percentage = 75m;
			bizObj.PercentageInEUBorder = 20m;
			bizObj.PercentageDomestic = 5m;

			bizObj.IsFreightIncludedInLines = false;
			bizObj.IsInsuranceIncludedInLines = false;
			bizObj.Calculate();
			AssertEquals(6, invoice.Charges.Count);
			AssertEquals(0, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT));

			bizObj.IsFreightIncludedInLines = true;
			bizObj.Calculate();
			AssertEquals(3, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			AssertEquals(0, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge));

			bizObj.IsInsuranceIncludedInLines = true;
			bizObj.Calculate();
			AssertEquals(3, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			AssertEquals(3, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge));

			bizObj.IsFreightIncludedInLines = false;
			bizObj.Calculate();
			AssertEquals(0, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge));
			AssertEquals(3, invoice.Charges.Cast<JobComInvCharge>().Count(c => c.J7_IsIncludedInITOT && c.J7_ChargeType == FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge));
		}

		public void TestBizObj_IsInsuranceIncludedInLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var insuranceCharge1 = invoice.Charges.AddNew();

			insuranceCharge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			insuranceCharge1.J7_IsIncludedInITOT = false;
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsInsuranceIncludedInLines);

			insuranceCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsInsuranceIncludedInLines);

			insuranceCharge1.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
			insuranceCharge1.J7_IsIncludedInITOT = false;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsInsuranceIncludedInLines);

			insuranceCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(true, bizObj.IsInsuranceIncludedInLines);
		}

		public void TestBizObj_IsFreightIncludedInLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var freightCharge1 = invoice.Charges.AddNew();

			freightCharge1.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
			freightCharge1.J7_IsIncludedInITOT = false;
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_ChargeType = ChargeCodeList.Codes.FRFreightToEUBorderCode;
			freightCharge1.J7_IsIncludedInITOT = false;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(false, bizObj.IsFreightIncludedInLines);

			freightCharge1.J7_IsIncludedInITOT = true;
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals(true, bizObj.IsFreightIncludedInLines);
		}

		public void TestNew()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentOutEU", "Desc.", codeType, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentInEu", "Desc.", codeType, Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentDomestic", "Desc.", codeType, Core.Constants.CountryCodes.France);

			var cusCode1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.France, codeType, "AAA", "aaaa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew("PercentOutEU", "70");
			cusCode1.Attributes.AddNew("PercentInEu", "20");
			cusCode1.Attributes.AddNew("PercentDomestic", "10");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Percentage", 0m, bizObj.Percentage);
			AssertEquals("PercentageInEUBorder", 0m, bizObj.PercentageInEUBorder);
			AssertEquals("PercentageDomestic", 0m, bizObj.PercentageDomestic);

			declaration.JE_IATALoadPort = "BBB";
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Percentage", 0m, bizObj.Percentage);
			AssertEquals("PercentageInEUBorder", 0m, bizObj.PercentageInEUBorder);
			AssertEquals("PercentageDomestic", 0m, bizObj.PercentageDomestic);

			declaration.JE_IATALoadPort = "AAA";
			bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			AssertEquals("Percentage", 70m, bizObj.Percentage);
			AssertEquals("PercentageInEUBorder", 20m, bizObj.PercentageInEUBorder);
			AssertEquals("PercentageDomestic", 10m, bizObj.PercentageDomestic);

			foreach (CodeDescriptionPair pair in new AirRouteTypeList())
			{
				declaration.JE_AirRouteType = pair.Code;
				bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
				AssertEquals("Percentage", 70m, bizObj.Percentage);
				AssertEquals("PercentageInEUBorder", 20m, bizObj.PercentageInEUBorder);
				AssertEquals("PercentageDomestic", 10m, bizObj.PercentageDomestic);
			}
		}

		public void TestProperties()
		{
			var bizObj = (CalculateFreightBizObj)GetNewBusinessObject();
			bizObj.Amount = 1000m;
			AssertEquals("AmountToEUBorder", 0m, bizObj.AmountToEUBorder);
			AssertEquals("AmountInEUBorder", 0m, bizObj.AmountAfterEUBorder);
			AssertEquals("AmountAfterEUBorder", 0m, bizObj.AmountDomestic);

			bizObj.Percentage = 10m;
			bizObj.PercentageInEUBorder = 20m;
			bizObj.PercentageDomestic = 70m;
			AssertEquals("AmountToEUBorder", 100m, bizObj.AmountToEUBorder);
			AssertEquals("AmountInEUBorder", 200m, bizObj.AmountAfterEUBorder);
			AssertEquals("AmountDomestic", 700m, bizObj.AmountDomestic);
		}

		public void TestAirTransportSubModeHasNoEffectOnCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.France;
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "France").ZZZ_ZZZ_Grouping = eun.PK;

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Airline Codes and Percentages for EU AIR freight calculation");
			var cusCode1 = helper.CreateCusCodeList(countryCode, codeType, "AAA", "Anaa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentOutEU", "Desc.", codeType, countryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentInEu", "Desc.", codeType, countryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("PercentDomestic", "Desc.", codeType, countryCode);
			cusCode1.Attributes.AddNew("PercentOutEU", "70");
			cusCode1.Attributes.AddNew("PercentInEu", "12");
			cusCode1.Attributes.AddNew("PercentDomestic", "18");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_IATALoadPort = "AAA";
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			bizObj.Amount = 100m;

			AssertEquals("AmountToEUBorder", 70m, bizObj.AmountToEUBorder);
			AssertEquals("AmountInEUBorder", 12m, bizObj.AmountAfterEUBorder);
			AssertEquals("AmountDomestic", 18m, bizObj.AmountDomestic);

			foreach (CodeDescriptionPair airRoute in new AirRouteTypeList())
			{
				declaration.JE_AirRouteType = airRoute.Code;
				invoice.Charges.RemoveAndDeleteAll();
				bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
				bizObj.Amount = 100m;
				bizObj.InsuranceAmount = 100m;
				bizObj.Calculate();

				AssertEquals($"{airRoute.Code}.AmountToEUBorder", 70m, bizObj.AmountToEUBorder);
				AssertEquals($"{airRoute.Code}.AmountInEUBorder", 12m, bizObj.AmountAfterEUBorder);
				AssertEquals($"{airRoute.Code}.AmountDomestic", 18m, bizObj.AmountDomestic);
				AssertEquals($"{airRoute.Code}.InsuranceAmountToEUBorder", 70m, bizObj.InsuranceAmountToEUBorder);
				AssertEquals($"{airRoute.Code}.InsuranceAmountInEUBorder", 12m, bizObj.InsuranceAmountInEUBorder);
				AssertEquals($"{airRoute.Code}.InsuranceAmountDomestic", 18m, bizObj.InsuranceAmountDomestic);
			}
		}

		public void TestCalculate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var invoice = declaration.Invoices.AddNew();
			AssertEquals(0, invoice.Charges.Count);

			var bizObj = CalculateFreightBizObj.New(invoice.Charges, declaration);
			Assert(!bizObj.Calculate());
			AssertEquals(0, invoice.Charges.Count);

			bizObj.Amount = 1000m;
			bizObj.InsuranceAmount = 1000m;
			bizObj.Percentage = 0m;
			bizObj.PercentageDomestic = 100m;
			AssertEquals(1000m, bizObj.AmountDomestic);

			Assert(bizObj.Calculate());
			AssertEquals(2, invoice.Charges.Count);
			var charge1 = invoice.Charges[0];
			Assert(!charge1.J7_IsDutiable);
			Assert(charge1.J7_IsGSTApplicable);
			Assert(!charge1.J7_IsStatisticalValueApplicable);
			var charge2 = invoice.Charges[1];
			Assert(!charge2.J7_IsDutiable);
			Assert(charge2.J7_IsGSTApplicable);
			Assert(!charge2.J7_IsStatisticalValueApplicable);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return CalculateFreightBizObj.New(invoice.Charges, declaration);
		}
	}
}
