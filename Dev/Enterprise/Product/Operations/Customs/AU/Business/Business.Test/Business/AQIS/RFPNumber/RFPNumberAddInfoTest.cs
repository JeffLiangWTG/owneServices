using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RFPNumberAddInfo))]
	sealed class RFPNumberAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new RFPNumberAddInfo(Factory.New<RFPNumber>().B7_AddInfoDataInfo);
	}
}
