using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DocManagerIncludingRelatedObjectsInfo))]
	sealed class DocManagerIncludingRelatedObjectsInfoTest : DocManagerInfoTestCase
	{
		public void TestAllRelatedBusinessObjects()
		{
			var mb = Factory.New<CusMAWB>();
			var hb1 = mb.ChildBills.AddNew();
			var hb2 = mb.ChildBills.AddNew();
			var ub1 = mb.AllUnderbonds.AddNew();
			var ub2 = hb1.AllUnderbonds.AddNew();
			var ub3 = hb2.AllUnderbonds.AddNew();
			var ub4 = hb2.AllUnderbonds.AddNew();

			var relatedBizObj = ((IDocManagerSupport)mb).DocManagerInfo.RelatedObjects;
			AssertEquals("Related biz objecs to master bill", 6, relatedBizObj.Length);
			Assert(relatedBizObj.Contains(hb1));
			Assert(relatedBizObj.Contains(hb2));
			Assert(relatedBizObj.Contains(ub1));
			Assert(relatedBizObj.Contains(ub2));
			Assert(relatedBizObj.Contains(ub3));
			Assert(relatedBizObj.Contains(ub4));
		}

		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<CusMAWB>();

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var mb = Factory.New<CusMAWB>();
			var hb1 = mb.ChildBills.AddNew();
			var hb2 = mb.ChildBills.AddNew();
			_ = mb.AllUnderbonds.AddNew();
			_ = hb1.AllUnderbonds.AddNew();
			_ = hb2.AllUnderbonds.AddNew();
			_ = hb2.AllUnderbonds.AddNew();
			return mb;
		}
	}
}
