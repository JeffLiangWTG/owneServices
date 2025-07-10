using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CLREGInfoProviderAddInfo))]
	sealed class CLREGInfoProviderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CLREGInfoProvider data = Factory.New<CLREGInfoProvider>();
			CLREGInfoProviderAddInfo addInfo = new CLREGInfoProviderAddInfo(data.B7_AddInfoDataInfo);
			addInfo.CLREGInfoProvider = data;
			return addInfo;
		}
	}
}
