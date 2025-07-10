using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSLineMergerTest : EU.Business.Testing.LineMergerTest
	{
		protected override Type ExpectedDutyCalculatorStrategyType => typeof(CDSDutyCalculatorStrategy);

		protected override Type[] ExpectedEntryCreationStrategiesType => new[] { typeof(C88CreationStrategy) };

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new CDSLineMerger((JobDeclaration)declaration);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		protected override Type GetSupportingDocumentType() => typeof(SupportingDocument);

		public void TestCusEntryLineFeeForNI()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dgEUN = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("XI", parent: dgEUN);
			var dgGB = helper.CreateNewOrGetExistingDataGrouping("GB");
			helper.CreateNewOrGetExistingDataGrouping("CDS", parent: dgGB);

			var tariffTypeEUN = helper.CreateNewOrGetExistingTariffType("EUN", "IMP");
			var tariffTypeGB = helper.CreateNewOrGetExistingTariffType("GB", "IMP");
			Factory.Save();
			var rtEUN = helper.CreateNewOrGetExistingRateType("EUN", "DTY");
			var rtGB = helper.CreateNewOrGetExistingRateType("GB", "DTY");
			rtEUN.ZZR_CustomsValueFormula = "CV";
			rtGB.ZZR_CustomsValueFormula = "CV";

			Factory.Save();
			var eunRC = helper.CreateCusRateCode(Factory, "A00", rtEUN.PK);
			var gbRC = helper.CreateCusRateCode(Factory, "A00", rtGB.PK);

			var prefEUN = helper.CreatePreferenceForCountry("100", "1233", "EUN");
			var prefGB = helper.CreatePreferenceForCountry("100", "1233", "GB");

			var tariffEUN = helper.CreateTariff("EUN", tariffTypeEUN.PK, "11111111", startDate, endDate);
			var tariffGB = helper.CreateTariff("GB", tariffTypeGB.PK, "11111111", startDate, endDate);
			Factory.Save();
			var rateEUN = helper.CreateRefCusRate(tariffEUN.PK, eunRC.PK, startDate, endDate, "VFD * 0.5", preferencePk: prefEUN.PK, dataGrouping: "EUN");
			var rateGB = helper.CreateRefCusRate(tariffGB.PK, gbRC.PK, startDate, endDate, "VFD * 0.2", preferencePk: prefGB.PK, dataGrouping: "GB");

			var tgEUN = helper.CreateTradeGroup("EUN", "ABC", startDate, endDate);
			var tgGB = helper.CreateTradeGroup("GB", "ABC", startDate, endDate);
			helper.AddCountry(tgEUN, "CN", startDate.Date, endDate.Date);
			helper.AddCountry(tgGB, "CN", startDate.Date, endDate.Date);

			Factory.Save();

			helper.CreateCusApplicability(rateEUN.PK, tgEUN, startDate, endDate);
			helper.CreateCusApplicability(rateGB.PK, tgGB, startDate, endDate);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GBP";
			invoice.JZ_InvoiceAmount = 10000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_Tariff = "11111111";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_PrimaryPreference = "100";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Check header exists", 1, declaration.ActiveEntryHeaders.Count);

			var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];
			AssertEquals("Check Fees exist", 1, entryLine.Fees.Count);

			AssertEquals("Check GB Fee Type", "A00", entryLine.Fees[0].CF_ChargeType);
			AssertEquals("Check GB Fee Amount", 2000m, entryLine.Fees[0].CF_ChargeAmount);

			declaration.JE_NorthernIrelandMode = "NII";
			declaration.JE_NiGoodsAtRiskOfMovingToROI = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Check Fees exist", 1, entryLine.Fees.Count);
			AssertEquals("Check EUN Fee Type", "A50", entryLine.Fees[0].CF_ChargeType);
			AssertEquals("Check EUN Fee Amount", 5000m, entryLine.Fees[0].CF_ChargeAmount);

			declaration.JE_NiGoodsAtRiskOfMovingToROI = false;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Check GB Fee Type", "A00", entryLine.Fees[0].CF_ChargeType);
			AssertEquals("Check GB Fee Amount", 2000m, entryLine.Fees[0].CF_ChargeAmount);
		}

		protected override EU.Business.Testing.LineMergerTestHelper GetLineMergerTestHelper() => new LineMergerTestHelper(Factory);
	}
}
