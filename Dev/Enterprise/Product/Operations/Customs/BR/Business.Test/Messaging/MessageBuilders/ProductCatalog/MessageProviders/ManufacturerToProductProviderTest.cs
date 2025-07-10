using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class ManufacturerToProductProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ManufacturerToProductProvider.New(null));
			AssertType<ManufacturerToProductProvider>(ManufacturerToProductProvider.New(Factory.New<ForeignOperator>()));
		}

		[TestDate(2024, 07, 17, 18, 00, 00)]
		public void TestProperties()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "BRB";
			owner.OH_FullName = "TEST COMPANY";
			owner.PrimaryRegistrationNumber.Number = "75.400.331/0001-15";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);

			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_OH_Owner = owner.PK;
			catalog.CGC_AuthorityIdentifier = "12345";

			var productionInfo = catalog.ForeignOperators.AddNew();
			productionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.Active;
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now;
			productionInfo.CountryCode = "BR";
			productionInfo.AuthorityCode = "OPE_1";

			var provider1 = ManufacturerToProductProvider.New(productionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Sequence", 0, provider1.Sequence);
				AssertEquals("RootCpfCnpj", "75400331", provider1.RootCpfCnpj);
				AssertEquals("ForeignOperatorCode", "OPE_1", provider1.ForeignOperatorCode);
				AssertEquals("ManufacturerCpfCnpj", string.Empty, provider1.ManufacturerCpfCnpj);
				AssertEquals("Known", true, provider1.Known);
				AssertEquals("ProductCode", 12345, provider1.ProductCode);
				AssertEquals("Link", true, provider1.Link);
				AssertEquals("CountryCode", "BR", provider1.CountryCode);
			});

			productionInfo.CGI_CustomsStatus = CustomsPostedStatusList.Codes.DeletePending;
			productionInfo.CGI_SystemCreateTimeUtc = ZDateTime.Now;
			productionInfo.CountryCode = "BR";

			var provider2 = ManufacturerToProductProvider.New(productionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Sequence", 0, provider2.Sequence);
				AssertEquals("RootCpfCnpj", "75400331", provider2.RootCpfCnpj);
				AssertEquals("ForeignOperatorCode", "OPE_1", provider2.ForeignOperatorCode);
				AssertEquals("ManufacturerCpfCnpj", string.Empty, provider2.ManufacturerCpfCnpj);
				AssertEquals("Known", true, provider2.Known);
				AssertEquals("ProductCode", 12345, provider2.ProductCode);
				AssertEquals("Link", false, provider2.Link);
				AssertEquals("CountryCode", "BR", provider2.CountryCode);
			});
		}

		public void TestAuthorityCode()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "12345";
			var productionInfo = catalog.ForeignOperators.AddNew();

			var provider = ManufacturerToProductProvider.New(productionInfo);
			AssertEquals(null, provider.ForeignOperatorCode);

			productionInfo.AuthorityCode = "OPE_1";
			AssertEquals("OPE_1", provider.ForeignOperatorCode);
		}

		public void TestKnown()
		{
			var catalog = Factory.New<CusGoodsCatalog>();
			catalog.CGC_AuthorityIdentifier = "12345";
			var foreignOperator = catalog.ForeignOperators.AddNew();

			var provider = ManufacturerToProductProvider.New(foreignOperator);
			Assert("Known", !provider.Known);

			foreignOperator.AuthorityCode = "OPE_1";
			foreignOperator.CGI_Reference = "BR";
			Assert("Known", provider.Known);

			foreignOperator.AuthorityCode = ZString.Empty;
			foreignOperator.CGI_Reference = ZString.Empty;
			Assert("Known", !provider.Known);

			foreignOperator.CGI_BFR_ForeignOperator = Factory.New<CusBRForeignOperator>().PK;
			Assert("Known", provider.Known);
		}
	}
}
