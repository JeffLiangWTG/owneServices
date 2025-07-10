using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusVehicleCollection))]
	sealed class CusVehicleCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CusVehicleCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusVehicle = Factory.New<CusVehicle>();
			cusVehicle.CVH_ModelName = "자동차";
			return cusVehicle;
		}
	}
}
