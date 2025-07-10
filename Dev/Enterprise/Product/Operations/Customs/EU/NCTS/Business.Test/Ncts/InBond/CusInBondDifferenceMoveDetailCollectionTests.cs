using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondDifferenceMoveDetailCollection))]
	class CusInBondDifferenceMoveDetailCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var collection = (CusInBondDifferenceMoveDetailCollection)Collection;
			var difference = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("B9_UnloadedState", NctsUnloadedStateList.Codes.NEW, difference.B9_UnloadedState);
				AssertEquals("B9_SeqNo", "1", difference.B9_SeqNo);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => GetCollection();

		CusInBondDifferenceMoveDetailCollection GetCollection()
		{
			return new CusInBondDifferenceMoveDetailCollection(cusInBondMoveDetail);
		}

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			cusInBondMoveDetail = header.Bills.AddNew().MovementDetail;
		}
		CusInBondMoveDetail cusInBondMoveDetail;
	}
}
