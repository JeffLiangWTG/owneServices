using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderCollectionFiltered))]
	public class CusAuthorisationHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAuthorisationHeaderCollectionFiltered>
	{
		[ExpectNoExceptions]
		public void TestModuleIdAttribute()
		{
			var moduleIDAttribute = typeof(CusAuthorisationHeaderCollectionFiltered).GetCustomAttribute<ModuleIDAttribute>();
			NUnit.Framework.Assert.That(moduleIDAttribute.ModuleId, NUnit.Framework.Is.EqualTo(ModuleId.CusAuthorisations));
		}

		[ExpectNoExceptions]
		public void TestMatchesFilterCore()
		{
			org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "YYY";

			var item1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			item1.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			item1.CPH_OH_PermitHolder = org2.PK;
			item1.CPH_Type = "AAA";
			item1.CPH_Number = "123";

			var item2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			item2.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			item2.CPH_OH_PermitHolder = org.PK;
			item2.CPH_Type = "BBB";
			item2.CPH_Number = "123";

			var item3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			item3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			item3.CPH_OH_PermitHolder = org.PK;
			item3.CPH_Type = "AAA";
			item3.CPH_Number = "789";

			var item4 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item4.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			item4.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			item4.CPH_OH_PermitHolder = org.PK;
			item4.CPH_Type = "AAA";
			item4.CPH_Number = "123";

			Factory.Save();

			var collection = new CusAuthorisationHeaderCollectionFiltered(Factory, "AAA", org.PK);
			collection.RefreshFromDb();

			NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.None.EqualTo(item1).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.None.EqualTo(item2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.Some.EqualTo(item3).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(collection, NUnit.Framework.Has.Some.EqualTo(item4).Using(CustomComparers.TypeComparison));
		}

		protected override CusAuthorisationHeaderCollectionFiltered GetCollectionToTest()
		{
			org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ";
			return new CusAuthorisationHeaderCollectionFiltered(Factory, "AAA", org.PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var element = (CusAuthorisationHeader)base.GetNewElementToAddToTheCollection();
			element.CPH_OH_PermitHolder = org.PK;
			element.CPH_Type = "AAA";
			element.CPH_Number = "123";

			return element;
		}

		OrgHeader org;
	}
}
