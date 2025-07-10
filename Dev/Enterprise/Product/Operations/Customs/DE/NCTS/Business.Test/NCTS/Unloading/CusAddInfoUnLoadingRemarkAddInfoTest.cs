using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(CusAddInfoUnloadingRemarkAddInfo))]
	public class CusAddInfoUnloadingRemarkAddInfoTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CusAddInfoUnloadingRemarkAddInfo>
	{
		protected override IEnumerable<CusAddInfoUnloadingRemarkAddInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			yield return header.UnloadingRemark.Parent;
		}
	}
}
