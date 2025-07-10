using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CusSCAOceanBillController))]
	sealed class CusSCAOceanBillControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var oceanBill = Factory.New(typeof(CusSCAOceanBill));
			Factory.Save();
			return oceanBill;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CusSCAOceanBill;
	}
}
