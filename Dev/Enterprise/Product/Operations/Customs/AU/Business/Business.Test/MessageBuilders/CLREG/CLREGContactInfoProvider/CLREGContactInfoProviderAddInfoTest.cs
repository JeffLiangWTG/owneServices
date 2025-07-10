using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CLREGContactInfoProviderAddInfo))]
	sealed class CLREGContactInfoProviderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var data = Factory.New<CLREGContactInfoProvider>();
			var addInfo = new CLREGContactInfoProviderAddInfo(data.B7_AddInfoDataInfo);
			addInfo.CLREGContactInfoProvider = data;
			return addInfo;
		}
	}
}
