using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RollAddInfo))]
	sealed class RollAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var data = Factory.New<Roll>();
			var addInfo = new RollAddInfo(data.B7_AddInfoDataInfo);
			addInfo.Roll = data;
			return addInfo;
		}
	}
}
