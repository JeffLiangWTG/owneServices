using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(CusIntrastatHeader))]
	sealed class CusIntrastatHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Intrastat - Transaction", transaction.HumanReadableName);
		}

		public void TestIsImport()
		{
			AssertEquals("Default", true, transaction.IsImport);
			transaction.CIH_CountryOfSupply = Core.Constants.CountryCodes.Latvia;
			AssertEquals(false, transaction.IsImport);
			transaction.CIH_CountryOfSupply = Core.Constants.CountryCodes.France;
			AssertEquals(true, transaction.IsImport);
		}

		public void TestCaptions() => CombineAssertions(() =>
		{
			AssertCaption(nameof(CusIntrastatHeader.CIH_TradersReference), "Traders Reference");
			AssertCaption(nameof(CusIntrastatHeader.CIH_ConsigneeVAT), "Consignee VAT");
			AssertCaption(nameof(CusIntrastatHeader.CIH_NatureOfTransaction), "Nature of Transaction", "Nature of Tran.");
			AssertCaption(nameof(CusIntrastatHeader.CIH_SupplierVAT), "Supplier VAT");
			AssertCaption(nameof(CusIntrastatHeader.CIH_OH_Supplier), "Supplier Code");
			AssertCaption(nameof(CusIntrastatHeader.CIH_SupplierName), "Supplier Name");
			AssertCaption(nameof(CusIntrastatHeader.CIH_OH_Consignee), "Consignee Code");
			AssertCaption(nameof(CusIntrastatHeader.CIH_ConsigneeName), "Consignee Name");
			AssertCaption(nameof(CusIntrastatHeader.CIH_CountryOfSupply), "Country Of Supply");
			AssertCaption(nameof(CusIntrastatHeader.CIH_CountryOfReceipt), "Country Of Receipt");
			AssertCaption(nameof(CusIntrastatHeader.CIH_TransactionDate), "Transaction Date");
			AssertCaption(nameof(CusIntrastatHeader.CIH_ModeOfTransport), "Mode Of Transport");
			AssertCaption(nameof(CusIntrastatHeader.CIH_IncoTerm), "Incoterm");

			static void AssertCaption(string propertyName, string caption, string shortCaption = "")
			{
				var data = DataBoundResourceStrings.GetDataForProperty(typeof(CusIntrastatHeader), propertyName);
				AssertEquals($"{propertyName} Caption", caption, data.Caption);
				AssertEquals($"{propertyName} Short Caption", shortCaption, data.ShortCaption);
			}
		});

		public void TestDefaultsCompany()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, Factory.New<CusIntrastatHeader>().CIH_GC_Company);
		}

		public void TestCountryCode()
		{
			AssertEquals(Core.Constants.CountryCodes.Latvia, transaction.CountryCode);
			using var countryChangeDisposable = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			AssertEquals(Core.Constants.CountryCodes.Germany, transaction.CountryCode);
		}

		protected override BusinessObject GetNewBusinessObject() => transaction;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => IntrastatTestDataHelper.New(factory).NewCusIntrastatHeaderWithValidData();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => transaction;

		protected override void SetUp()
		{
			transaction = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
		}
		CusIntrastatHeader transaction;
	}

	[TestedType(typeof(CusIntrastatHeader))]
	sealed class CusIntrastatHeaderClusterKeyTest : ClusterKeyMasterMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var report = IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData();
			return report;
		}
	}
}
