using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSCusContainerCollection))]
	public class EMCSCusContainerCollectionTest : CusContainerCollectionTest
	{
		public void TestTooManyTransports()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var containers = declaration.CusContainers;

			for (var i = 0; i < 99; i++)
			{
				containers.AddNew();
			}

			CombineAssertions(() =>
			{
				AssertEquals("99 Transports", false, containers.HasErrors());
				var extraInvalidTransport = containers.AddNew();
				AssertHasRowError(extraInvalidTransport, "There are too many Transports. Maximum of 99.");
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<EMCSJobDeclaration>();
			return new EMCSCusContainerCollection(master);
		}
	}
}
