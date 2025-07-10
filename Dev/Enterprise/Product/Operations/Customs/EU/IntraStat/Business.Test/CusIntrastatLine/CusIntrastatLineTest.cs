using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatLine))]
	sealed class CusIntrastatLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var header = Factory.New<CusIntrastatHeader>();
			var line = Factory.New<CusIntrastatLine>();
			line.CIL_CIH_Header = header.PK;
			AssertEquals(header, line.Header);
		}

		public void TestIsImport()
		{
			AssertEquals("precondition header.IsImport no county", true, transaction.IsImport);
			AssertEquals("line.IsImport no country", true, transactionLine.IsImport);
			transaction.CIH_CountryOfSupply = Core.Constants.CountryCodes.Latvia;
			AssertEquals("precondition header.IsImport from current country",false, transactionLine.IsImport);
			AssertEquals("line.IsImport from current country", false, transactionLine.IsImport);
			transaction.CIH_CountryOfSupply = Core.Constants.CountryCodes.France;
			AssertEquals("precondition header.IsImport from country other than current", true, transaction.IsImport);
			AssertEquals("line.IsImport from country other than current", true, transactionLine.IsImport);
		}

		public void TestCIL_RX_NKCurrency()
		{
			AssertEquals(true, transactionLine.CIL_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals("precondition EUR", Core.Constants.CurrencyCodes.EuropeanUnion, transaction.Company.GC_RX_NKLocalCurrency);
			AssertEquals("defaulted EUR", Core.Constants.CurrencyCodes.EuropeanUnion, transaction.CusIntrastatLines.AddNew().CIL_RX_NKCurrency);
			using var currencyChangeDisposable = GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Poland);
			AssertEquals("precondition PLN", Core.Constants.CurrencyCodes.Poland, transaction.Company.GC_RX_NKLocalCurrency);
			AssertEquals("defaulted PLN", Core.Constants.CurrencyCodes.Poland, transaction.CusIntrastatLines.AddNew().CIL_RX_NKCurrency);
		}

		public void TestCIL_MassInKilogramsUnit()
		{
			AssertEquals(true, transactionLine.CIL_MassInKilogramsUnitInfo.ReadOnly);
			AssertEquals("KG", transactionLine.CIL_MassInKilogramsUnit);
		}

		public void TestCIL_FormattedTariff()
		{
			transactionLine.CIL_Tariff = "1234567890";
			AssertEquals("CIL_FormattedTariff", "1234.56.78 90", transactionLine.CIL_FormattedTariff);
			transactionLine.CIL_FormattedTariff = "9876.54.32 10000";
			AssertEquals("CIL_FormattedTariff", "9876.54.32 1000", transactionLine.CIL_FormattedTariff);
			AssertEquals("CIL_Tariff", "987654321000", transactionLine.CIL_Tariff);
		}

		public void TestTariffFormatter()
		{
			AssertType<TariffFormatterThirteen>("EU Default", ((ITariffFormatProvider)transactionLine).TariffFormatter);

			using var country = GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Germany);
			transactionLine = IntrastatTestDataHelper.New(Factory).NewCusIntrastatLineWithValidData();
			AssertType<TariffFormatterEleven>("Country Specific", ((ITariffFormatProvider)transactionLine).TariffFormatter);
		}

		public void TestCaptions() => CombineAssertions(() =>
		{
			AssertCaption(nameof(CusIntrastatLine.CIL_DescriptionOfGoods), "Goods Description");
			AssertCaption(nameof(CusIntrastatLine.CIL_InvoiceValue), "Invoice Amount");
			AssertCaption(nameof(CusIntrastatLine.CIL_StatisticalValue), "Statistical Value");
			AssertCaption(nameof(CusIntrastatLine.CIL_RX_NKCurrency), "Currency");
			AssertCaption(nameof(CusIntrastatLine.CIL_MassInKilograms), "Mass");
			AssertCaption(nameof(CusIntrastatLine.CIL_MassInKilogramsUnit), "Mass UQ");
			AssertCaption(nameof(CusIntrastatLine.CIL_RN_NKCountryOfOrigin), "Country of Origin", "Origin");
			AssertCaption(nameof(CusIntrastatLine.CIL_SupplementaryQuantity), "Supplementary Quantity", "Supp. Qty.");
			AssertCaption(nameof(CusIntrastatLine.CIL_SupplementaryQuantityUnit), "Supplementary Quantity Unit", "Supp. UQ");
			AssertCaption(nameof(CusIntrastatLine.CIL_Region), "Region");
			AssertCaption(nameof(CusIntrastatLine.CIL_FormattedTariff), "Tariff");
			AssertCaption(nameof(CusIntrastatLine.CIL_Tariff), "Tariff");

			static void AssertCaption(string propertyName, string caption, string shortCaption = "")
			{
				var data = DataBoundResourceStrings.GetDataForProperty(typeof(CusIntrastatLine), propertyName);
				AssertEquals($"{propertyName} Caption", caption, data.Caption);
				AssertEquals($"{propertyName} Short Caption", shortCaption, data.ShortCaption);
			}
		});

		protected override BusinessObject GetNewBusinessObject() => transactionLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => IntrastatTestDataHelper.New(factory).NewCusIntrastatLineWithValidData();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => transactionLine;

		protected override void SetUp()
		{
			var helper = IntrastatTestDataHelper.New(Factory);
			transaction = helper.NewCusIntrastatHeaderWithValidData();
			transactionLine = helper.NewCusIntrastatLineWithValidData(transaction);
		}
		CusIntrastatHeader transaction;
		CusIntrastatLine transactionLine;
	}

	[TestedType(typeof(CusIntrastatLine))]
	sealed class CusIntrastatLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var header = (CusIntrastatHeader)NewParentObject();

			var line = IntrastatTestDataHelper.New(Factory).NewCusIntrastatLineWithValidData(header);

			return line;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var header = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
			return header;
		}

		protected override IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk => new[]
		{
			CusIntrastatMergedLineSchema.CIM_CIG_Group,
		};
	}
}
