using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.MultiLineAddInfos.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(CusAddInfo<GBCusAddInfo>))]
	class CusAddInfo_GBCusAddInfoTest : CusAddInfoTest<CusAddInfo<GBCusAddInfo>>
	{
		protected override IEnumerable<CusAddInfo<GBCusAddInfo>> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var result = factory.New<CusAddInfo<GBCusAddInfo>>();
			result.B7_ParentID = declaration.PK;
			result.B7_ParentTableCode = declaration.TablePrefix;
			yield return result;
		}
	}

	[TestedType(typeof(GBCusAddInfo))]
	internal class GbCusAddInfoTest : PersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBizO(Factory).Data;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBizO(factory);
		}

		CusAddInfo<GBCusAddInfo> GetNewBizO(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var gbCusAddInfosCollectionOfOneItem = new CusAddInfoCollection<GBCusAddInfo>(declaration);
			gbCusAddInfosCollectionOfOneItem.Load();
			gbCusAddInfosCollectionOfOneItem.AddNew();
			return gbCusAddInfosCollectionOfOneItem[0];
		}

		public void TestKeyToDeterimeUniqueness()
		{
			var route = (GBCusAddInfo)GetNewBusinessObject();
			route.G9_RouteOfEntry = "6";
			AssertEquals("6", route.KeyToDeterimeUniqueness);
		}
	}

	internal class GbCusAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
	}

	internal class GbCusAddInfoValidationTest : BusinessObjectValidationTestCase
	{
	}
}
