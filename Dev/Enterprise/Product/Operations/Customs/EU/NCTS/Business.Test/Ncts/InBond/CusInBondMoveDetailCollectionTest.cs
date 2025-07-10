using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetailCollection))]
	class CusInBondMoveDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveDetailCollection>
	{
		protected override CusInBondMoveDetailCollection GetCollectionToTest()
		{
			return new CusInBondMoveDetailCollection(NctsHeader.ArrivalMovementHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return NctsHeader.ArrivalMovementHeader.MovementDetails.AddNew();
		}

		protected NctsHeader NctsHeader
		{
			get
			{
				if (nctsHeader == null)
				{
					nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				}

				return nctsHeader;
			}
		}
		NctsHeader nctsHeader;
	}
}
