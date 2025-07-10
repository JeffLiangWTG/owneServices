using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRDutyWrapperForInvoiceLineTest : TestCaseWithFactory
	{
		public void TestFactory()
		{
			AssertEquals("Factory", invoiceLine.Factory, wrapper.Factory);
		}

		public void TestFirstQty()
		{
			invoiceLine.JI_CustomsQuantity = 10m;
			AssertEquals("FirstQty", 10m, wrapper.FirstQty);
		}

		public void TestSecondQty()
		{
			invoiceLine.AddInfo.ZA_QT2 = 10m;
			AssertEquals("SecondQty", 10m, wrapper.SecondQty);
		}

		public void TestOtherDutyFactory()
		{
			invoiceLine.AddInfo.ZA_ODF = 10m;
			AssertEquals("OtherDutyFactor", 10m, wrapper.OtherDutyFactor);
		}

		public void TestManualDutyAmount()
		{
			invoiceLine.AddInfo.ZA_DTY = 10m;
			AssertEquals("ManualDutyAmount", 10m, wrapper.ManualDutyAmount.Amount);
		}

		public void TestDumpingAndCountervailingDuty()
		{
			var mockLine = Factory.NewMoq<JobComInvoiceLine>();
			mockLine.Setup(m => m.JI_Calc_DumpingDuty).Returns(1m);
			mockLine.Setup(m => m.JI_Calc_CountervailingDuty).Returns(2m);
			var invoiceLine = mockLine.Object;
			var wrapper = new CMRDutyWrapperForInvoiceLine(invoiceLine, false);
			AssertEquals("DumpingDuty", 1m, wrapper.DumpingDuty);
			AssertEquals("CountervailingDuty", 2m, wrapper.CountervailingDuty);
		}

		[TestDate(2005, 1, 1)]
		public void TestRandomLineDutyData()
		{
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			invoiceLine.JI_Tariff = "0000.00.11 22";
			invoiceLine.AddInfo.ZA_GSTE = "FOOD";
			invoiceLine.AddInfo.ZA_LCTQ = "Y";
			invoiceLine.AddInfo.ZA_WETE = "BLAH";
			invoiceLine.JI_CustomsUnitQty = "LA";
			invoiceLine.AddInfo.ZA_UQ2 = "L";

			invoice.AddInfo.ZA_PST = "AAA";
			invoiceLine.AddInfo.ZA_RNO = "222";
			invoiceLine.AddInfo.ZA_CL2 = "22221100";

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "000";
			invoiceLine.AddInfo.ZA_TR2 = "111";
			invoiceLine.AddInfo.ZA_TRN = "555";
			invoiceLine.AddInfo.ZA_ICN = "ICN12345";

			DutyDataFromInvoiceLine result = wrapper.RandomLineDutyData;

			AssertEquals("Duty Date", new ZDateTime(2005, 1, 1), result.EffectiveDutyDate);
			AssertEquals("First Tariff number", "00000011", result.FirstTariffNumber);
			AssertEquals("Stat Code", "22", result.StatCode);
			AssertEquals("IsGST Exempt", true, result.IsGSTExempt);
			AssertEquals("Is LCT Payable", false, result.IsLCTPayable);
			AssertEquals("Is LCT Exempt", true, result.IsLCTExempt);
			AssertEquals("Is WET exempt", true, result.IsWETExempt);
			AssertEquals("First Unit Qty", "LA", result.FirstUQ);
			AssertEquals("Second Unit Qty", "L", result.SecondUQ);
			AssertEquals("Preference", "AAA", result.Preference);
			AssertEquals("Rate Number", "222", result.RateNumber);
			AssertEquals("Second tariff number", "22221100", result.SecondTariffNumber);
			AssertEquals("Treatment code", "000", result.FirstTreatmentCode);
			AssertEquals("Second Treatment Code", "111", result.SecondTreatmentCode);
			AssertEquals("Treatment code rate number", "555", result.TreatmentRateNumber);
			AssertEquals("ICN", "ICN12345", result.ICN);

			//Treatment info excluded
			invoiceLine.AddInfo.ZA_LCTI = "Y";
			invoiceLine.AddInfo.ZA_LCTE = "FEV";
			invoiceLine.AddInfo.ZA_LCTQ = ZString.Empty;
			wrapper = new CMRDutyWrapperForInvoiceLine(invoiceLine, true);
			result = wrapper.RandomLineDutyData;

			AssertEquals("Duty Date", new ZDateTime(2005, 1, 1), result.EffectiveDutyDate);
			AssertEquals("First Tariff number", "00000011", result.FirstTariffNumber);
			AssertEquals("Stat Code", "22", result.StatCode);
			AssertEquals("IsGST Exempt", true, result.IsGSTExempt);
			AssertEquals("Is LCT Payable", true, result.IsLCTPayable);
			AssertEquals("Is LCT Exempt", false, result.IsLCTExempt);
			AssertEquals("Is WET exempt", true, result.IsWETExempt);
			AssertEquals("First Unit Qty", "LA", result.FirstUQ);
			AssertEquals("Second Unit Qty", "L", result.SecondUQ);
			AssertEquals("Preference", "AAA", result.Preference);
			AssertEquals("Rate Number", "222", result.RateNumber);
			AssertEquals("ICN", "ICN12345", result.ICN);
			AssertEquals("Second tariff number", "22221100", result.SecondTariffNumber);
			AssertEquals("Treatment code is empty as the info is excluded", "", result.FirstTreatmentCode);
			AssertEquals("Second Treatment Code as the info is excluded", "", result.SecondTreatmentCode);
			AssertEquals("Treatment code rate number as the info is excluded", "", result.TreatmentRateNumber);
			AssertEquals("LSTE", "FEV", result.LCTE);
		}

		public void TestIsGSTExempt()
		{
			CMRTreatmentRatePeriodCharacteristic treatmentRate = CMRTreatmentRatePeriodCharacteristic.New(Factory);
			treatmentRate.TR_CharacteristicCode = ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports);
			treatmentRate.TR_TreatmentRatePeriodSnapshotPreferenceSchemeType = "AAA";
			treatmentRate.TR_TreatmentRatePeriodSnapshotRateNumber = "000";
			treatmentRate.TR_TreatmentRatePeriodSnapshotCode = "111";

			invoiceLine.JI_Tariff = "0000.00.11 22";
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "111";
			invoiceLine.AddInfo.ZA_PST = "AAA";
			invoiceLine.AddInfo.ZA_TRN = "000";

			AssertEquals("GSTE in AddInfo", "", invoiceLine.AddInfo.ZA_GSTE);
			AssertEquals("Treatment deems GSTE", true, invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption);
			AssertEquals("IsGSTE", true, wrapper.RandomLineDutyData.IsGSTExempt);
		}

		public void TestIsNature20()
		{
			invoiceLine.JI_IsPackToBondForLine = true;
			AssertEquals("IsNature 20", true, wrapper.IsNature20);

			invoiceLine.JI_IsPackToBondForLine = false;
			AssertEquals("IsNature 20", false, wrapper.IsNature20);
		}

		public void TestIsSubjectToDutyAndTax()
		{
			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "351";
			Assert("IsSubjectToDutyAndTax", !wrapper.IsSubjectToDutyAndTax);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "352";
			Assert("IsSubjectToDutyAndTax", !wrapper.IsSubjectToDutyAndTax);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "354";
			Assert("IsSubjectToDutyAndTax", !wrapper.IsSubjectToDutyAndTax);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = "353";
			Assert("IsSubjectToDutyAndTax", wrapper.IsSubjectToDutyAndTax);

			invoiceLine.AddInfo.ZA_TreatmentCode_Hidden = ZString.Empty;
			Assert("IsSubjectToDutyAndTax", wrapper.IsSubjectToDutyAndTax);
		}

		public void IsDutyAndTaxEstimatedForWH()
		{
			var importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			invoiceLine.Declaration.JE_OH_Importer = importer.PK;
			Assert("IsNotLowValueShipment", ((ICMRDutyData)wrapper).IsNotLowValueShipment);
			invoiceLine.Declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			invoiceLine.JI_IsPackToBondForLine = true;
			Assert("IsDutyAndTaxEstimated", ((ICMRDutyData)wrapper).IsDutyAndTaxEstimatedForWH);
			invoiceLine.Declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = false;
			Assert("IsDutyAndTaxEstimated", !((ICMRDutyData)wrapper).IsDutyAndTaxEstimatedForWH);
			invoiceLine.Declaration.Importer.MiscServ.OM_IMShowDutyOnWarehouseEntries = true;
			invoiceLine.JI_IsPackToBondForLine = false;
			Assert("IsDutyAndTaxEstimated", !((ICMRDutyData)wrapper).IsDutyAndTaxEstimatedForWH);
		}

		public void TestIsGSTDeferred()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			AssertEquals("IsGST deferred", false, wrapper.IsGSTDeferred);

			testDec.JE_OH_Importer = importer.PK;
			AssertEquals("IsGST deferred", true, wrapper.IsGSTDeferred);

			importer.MiscServ.OM_IMIsGSTDeferred = false;
			AssertEquals("IsGST deferred", false, wrapper.IsGSTDeferred);
		}

		public void TestCustomsValue()
		{
			AssertEquals("Customs value", 10000m, wrapper.CustomsValue);
		}

		public void TestTransportAndInsuranceInAUD()
		{
			invoiceLine.AddInfo.ZA_TILV = "100AUD";
			AssertEquals("Transport and Insurance in AUD", 100m, wrapper.TransportAndInsuranceInAUD);
		}

		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		CMRDutyWrapperForInvoiceLine wrapper;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			wrapper = new CMRDutyWrapperForInvoiceLine(invoiceLine, false);
		}
	}
}
