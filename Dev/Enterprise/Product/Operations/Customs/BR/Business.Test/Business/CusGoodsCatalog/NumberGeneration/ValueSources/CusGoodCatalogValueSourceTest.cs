using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class CusGoodCatalogValueSourceTest : TestCaseWithFactory
	{
		public void TestDirection()
		{
			OrgHeader branchOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader companyOrg = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = companyOrg.PK;

			GlbBranch branch = company.Branches.AddNew();

			branch.GB_OH_OrgProxy = branchOrg.PK;

			Generator.Context = new NumberGeneratorContext(company.PK, branch.PK, ZGuid.Empty);
			AssertEquals("I", ValueProviders[Keys.Direction].GetValue(Generator, ""));

			catalog.CGC_Type = GoodsCatalogTypeList.Codes.Export;
			AssertEquals("E", ValueProviders[Keys.Direction].GetValue(Generator, ""));

			catalog.CGC_Type = ZString.Empty;
			AssertEquals("O", ValueProviders[Keys.Direction].GetValue(Generator, ""));
		}

		NumberGeneratorValueProviderCollection ValueProviders
		{
			get
			{
				if (valueProviders == null)
				{
					valueProviders = new NumberGeneratorValueProviderCollection();
					catalog = Factory.New<CusGoodsCatalog>();
					catalog.CGC_Type = GoodsCatalogTypeList.Codes.Import;
					valueProviders.AddRange(new GoodCatalogValueSource(catalog));
				}
				return valueProviders;
			}
		}
		NumberGeneratorValueProviderCollection valueProviders;
		CusGoodsCatalog catalog;

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}
		NumberGenerator generator;
	}
}
