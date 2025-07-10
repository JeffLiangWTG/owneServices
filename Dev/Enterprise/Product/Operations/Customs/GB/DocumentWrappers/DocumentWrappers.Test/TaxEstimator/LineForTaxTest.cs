using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	class LineForTaxTest : TestCaseWithFactory
	{
		public void TestDutyFromUniversalEngine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			(var stdTradeGroup, var stdPreference, var dtyTariff, var dtyRateType) = PopulateTestReferenceData(declaration.GetDefaultDataGroupingCode(Customs.Business.DefaultDataGroupingType.Tariff));
			var rateCodeDtyA00 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dtyRateType.PK);
			var tariffDtyRateA00 = RefDataHelper.CreateRefCusRate(dtyTariff.PK, rateCodeDtyA00.PK, startDate, endDate, rateFormula: "VFD*0.7 + 4*[HLT] + 10*[LPA]", preferencePk: stdPreference.PK);
			RefDataHelper.CreateCusApplicability(tariffDtyRateA00.PK, stdTradeGroup, startDate, endDate);

			tariffDtyRateA00.Factory.Save();
			Factory.Save();

			var invLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invLine1.JI_PrimaryPreference = stdPreference.ZZS_Preference;
			invLine1.JI_Tariff = dtyTariff.ZZ1_TariffCode;
			invLine1.JI_CustomsQuantity = 3;
			invLine1.JI_CustomsUnitQty = "KGM";
			invLine1.JI_CustomsSecondQuantity = 2000;
			invLine1.JI_CustomsSecondUnitQty = "LTR";
			invLine1.JI_CustomsThirdQuantity = 50;
			invLine1.JI_CustomsThirdUnitQty = "LPA";
			// No longer need tax lines

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine1);
			entryLine.CL_CustomsValue = 20;

			var line = new LineForTax(entryLine, Factory);
			AssertContains(@"A00 % 70 14.00 A00 HLT 4 80.00 A00 LPA 10 500.00", Regex.Replace(line.Duties, @"\s+", " "));
			AssertEquals(594m, line.TotalDutyDue);
		}

		(CusRefTradeGroupView, CusRefPreferenceView, TariffView, RefCusRateType) PopulateTestReferenceData(string dataGrouping)
		{
			var stdTradeGroup = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", startDate, endDate);
			var impTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var stdPreference = RefDataHelper.CreatePreferenceView("STD", "Standard", dataGrouping);
			Factory.Save();
			var dtyTariff = RefDataHelper.CreateTariff(dataGrouping, impTariffType.PK, "1111111", startDate, endDate, taxOrFeeCode: "DTY");
			var dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "Duty");
			return (stdTradeGroup, stdPreference, dtyTariff, dtyRateType);
		}
		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		readonly ZDateTime startDate = ZDateTime.Today.AddYears(-1);
		readonly ZDateTime endDate = ZDateTime.Today.AddYears(1);
	}
}
