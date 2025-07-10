using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(MergeMethodRegistryItem))]
	sealed class MergeMethodRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new MergeMethodRegistryItem(string.Empty, null, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, string.Empty);
		}

		public void TestMergeByLookup()
		{
			var factory = new BusinessObjectFactory();
			var branchType = ObjectFactory.GetType("IGlbBranch");
			var companyType = ObjectFactory.GetType("IGlbCompany");

			BusinessObject twCompany = NewCompanyAndBranch(factory, branchType, companyType, "TW", "TWX");
			BusinessObject caCompany = NewCompanyAndBranch(factory, branchType, companyType, "CA", "CAX");
			BusinessObject cnCompany = NewCompanyAndBranch(factory, branchType, companyType, "CN", "CNX");
			BusinessObject noCompany = NewCompanyAndBranch(factory, branchType, companyType, "NO", "NOX");
			factory.Save();

			var list = new CountrySepecificMergeMethodListProvider(OLookUpEditType.CommercialInvoiceMergeMethod);
			var implForTest = new MergeMethodRegistryItemImplForTest(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, null, list);
			implForTest.GetRegistryItemPKCoreForTest(twCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, TRD, PNO, TRF", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(caCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(noCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("MAX, NON", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, NOP, TRF, TRD, PNO, PNP", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(twCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, TRD, PNO, TRF", implForTest.LookUp.CodesAsString);

			implForTest.GetRegistryItemPKCoreForTest(caCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertEquals("NON, NOP, TRF, TRD, CLS, CLD, PNO, PNP, TRM", implForTest.LookUp.CodesAsString);
		}

		BusinessObject NewCompanyAndBranch(BusinessObjectFactory factory, Type branchType, Type companyType, string countryCode, string code)
		{
			var company = factory.New(companyType);
			company["GC_RN_NKCountryCode"] = countryCode;
			company["GC_Code"] = code;

			var branch = factory.New(branchType);
			branch["GB_GC"] = company.PK;
			branch["GB_Code"] = code;
			return company;
		}

		public class MergeMethodRegistryItemImplForTest : MergeMethodRegistryItemImpl
		{
			public MergeMethodRegistryItemImplForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, ICodeDescriptionPairListProvider lookUpList)
			: base(name, category, caption, hint, storage, options, defaultValue, lookUpList)
			{
			}

			public Guid GetRegistryItemPKCoreForTest(Guid companyPk, Guid branchPk, Guid departmentPk)
			{
				return GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
			}

			public CodeDescriptionPairList LookUp => lookUpList.CodeDescriptionPairList;
		}
	}
}
