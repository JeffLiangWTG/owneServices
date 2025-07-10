using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusContainerCollection))]
	class CusContainerCollectionTest : Customs.Business.Testing.CusContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CusContainerCollection(declaration, Factory);
		}

		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusContainers = new CusContainerCollection(declaration, Factory);
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				var container1 = cusContainers.AddNew();
				AssertEquals("JE_TransportMode = 'SEA', no default for CO_FCL_LCL_AIR", ZString.Empty, container1.CO_FCL_LCL_AIR);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				var container2 = cusContainers.AddNew();
				AssertEquals("JE_TransportMode = 'AIR', CO_FCL_LCL_AIR defaults to 'ULD'", Core.Constants.ContainerModes.ULD, container2.CO_FCL_LCL_AIR);
			});
		}
	}
}
