using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLFunctionAddInfo))]
	class BLLFunctionAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new BLLFunctionAddInfo(Factory.New<BLLFunctionInfo>().B7_AddInfoDataInfo);
	}
}
