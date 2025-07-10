using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSCusContainerCollection))]
	public class EMCSCusContainerCollectionTest : EU.EMCS.Business.Testing.EMCSCusContainerCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var master = Factory.New<EMCSJobDeclaration>();
			return new EMCSCusContainerCollection(master);
		}
	}
}
