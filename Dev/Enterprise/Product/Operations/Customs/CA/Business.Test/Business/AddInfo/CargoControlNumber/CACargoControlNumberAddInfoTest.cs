using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACargoControlNumberAddInfo))]
	sealed class CACargoControlNumberAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<CargoControlNumber>();
			var addInfo = new CACargoControlNumberAddInfo(parent.B7_AddInfoDataInfo);
			AssertEquals("Parent", parent, addInfo.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CACargoControlNumberAddInfo(Factory.New<CargoControlNumber>().B7_AddInfoDataInfo);
		}
	}
}
