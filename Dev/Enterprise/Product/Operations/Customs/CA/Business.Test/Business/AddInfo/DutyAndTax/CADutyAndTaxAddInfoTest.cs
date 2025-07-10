using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CADutyAndTaxAddInfo))]
	sealed class CADutyAndTaxAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<DutyAndTax>();
			var addInfo = new CADutyAndTaxAddInfo(parent.B7_AddInfoDataInfo);
			AssertEquals("Parent", parent, addInfo.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CADutyAndTaxAddInfo(Factory.New<DutyAndTax>().B7_AddInfoDataInfo);
		}
	}
}
