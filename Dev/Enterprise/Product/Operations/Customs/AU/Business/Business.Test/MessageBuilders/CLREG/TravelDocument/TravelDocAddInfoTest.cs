using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(TravelDocAddInfo))]
	sealed class TravelDocAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			TravelDocument data = Factory.New<TravelDocument>();
			TravelDocAddInfo addInfo = new TravelDocAddInfo(data.B7_AddInfoDataInfo);
			addInfo.TravelDocument = data;
			return addInfo;
		}
	}
}
