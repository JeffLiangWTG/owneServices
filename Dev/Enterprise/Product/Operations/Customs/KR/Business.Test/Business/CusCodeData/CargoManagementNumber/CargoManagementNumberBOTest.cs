using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CargoManagementNumberBO))]
	sealed class CargoManagementNumberBOTest : CusCodeDataTest<CargoManagementNumberBO>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.CargoManagementNumber, cargoManagementNumber.CY_Type);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return cargoManagementNumber;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CargoManagementNumberBO>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			cargoManagementNumber = bill.CargoManagementNumbers.AddNew();
			Factory.Save();
		}
		CargoManagementNumberBO cargoManagementNumber;
	}
}
