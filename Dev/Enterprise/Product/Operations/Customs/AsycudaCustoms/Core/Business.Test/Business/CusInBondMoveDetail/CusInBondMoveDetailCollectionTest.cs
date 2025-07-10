using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailCollection))]
	public class CusInBondMoveDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveDetailCollection>
	{
		protected override CusInBondMoveDetailCollection GetCollectionToTest()
		{
			var master = Factory.New<CusInBondMoveHeader>();
			return new CusInBondMoveDetailCollection(master);
		}
	}
}
