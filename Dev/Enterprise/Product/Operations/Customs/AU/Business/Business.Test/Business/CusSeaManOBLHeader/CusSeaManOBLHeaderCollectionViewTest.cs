using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCollectionView))]
	public class CusSeaManOBLHeaderCollectionViewTest : Customs.Business.Testing.CusSeaManOBLHeaderCollectionViewTest<CusSeaManOBLHeaderCollectionView>
	{
		#region RemoveAndDelete

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsTrue()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeaderCollection oceanBillCol = new CusSeaManOBLHeaderCollection(tranHead);
			CusSeaManOBLHeaderCollectionView oceanBillView = new CusSeaManOBLHeaderCollectionView(oceanBillCol);
			AssertEquals("No item initially exists", 0, oceanBillView.Count);

			var oBLHeaderToDeleteMock = Factory.NewMoq<CusSeaManOBLHeader>();
			oBLHeaderToDeleteMock.Setup(m => m.CanDelete).Returns(true);
			var elementToDelete = oBLHeaderToDeleteMock.Object;
			oceanBillCol.Add(elementToDelete);
			AssertEquals("One Element must exist", 1, oceanBillView.Count);
			oceanBillView.RemoveAndDelete(elementToDelete);
			AssertEquals("Element should have been removed", 0, oceanBillView.Count);
		}

		public void TestRemoveAndDeleteRemovesElementsIfCanBeDeletedIsFalse()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeaderCollection oceanBillCol = new CusSeaManOBLHeaderCollection(tranHead);
			CusSeaManOBLHeaderCollectionView oceanBillView = new CusSeaManOBLHeaderCollectionView(oceanBillCol);
			AssertEquals("No item initially exists", 0, oceanBillView.Count);

			var oBLHeaderNotToDeleteMock = Factory.NewMoq<CusSeaManOBLHeader>();
			oBLHeaderNotToDeleteMock.Setup(m => m.CanDelete).Returns(false);
			var elementNotToDelete = oBLHeaderNotToDeleteMock.Object;
			oceanBillCol.Add(elementNotToDelete);
			AssertEquals("One Element must exist", 1, oceanBillView.Count);
			oceanBillView.RemoveAndDelete(elementNotToDelete);
			AssertEquals("One item must exist", 1, oceanBillView.Count);
		}

		#endregion

		#region Implementation

		protected override CusSeaManOBLHeaderCollectionView GetCollectionToTest()
		{
			return View;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusSeaManOBLHeader result = Factory.New<CusSeaManOBLHeader>();
			result.BO_RL_NKDischargePort = "AUSYD";
			return result;
		}

		CusSeaManOBLHeaderCollectionView fView;
		new CusSeaManOBLHeaderCollectionView View
		{
			get
			{
				if (fView == null)
				{
					fView = new CusSeaManOBLHeaderCollectionView(TranHeader.OceanBills);
					fView.DischargePort = "AUSYD";
				}
				return fView;
			}
		}

		CusSeaManTranHead fTranHeader;
		CusSeaManTranHead TranHeader
		{
			get
			{
				if (fTranHeader == null)
				{
					fTranHeader = Factory.New<CusSeaManTranHead>();
				}
				return fTranHeader;
			}
		}

		#endregion
	}
}
