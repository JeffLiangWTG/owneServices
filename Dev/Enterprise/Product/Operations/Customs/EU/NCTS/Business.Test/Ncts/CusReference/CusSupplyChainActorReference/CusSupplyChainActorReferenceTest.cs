using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusSupplyChainActorReference))]
	sealed class CusSupplyChainActorReferenceTest : CusSupplyChainActorReferenceAbstractTest<CusSupplyChainActorReference>
	{
		public void TestCFR_Reference_Caption()
		{
			var supplyChainActorReference = GetNewBusinessObject() as CusSupplyChainActorReference;
			NCTSTestHelper.AssertCaptions(supplyChainActorReference.CFR_ReferenceInfo, "Identification (TCUI/EORI)", "Identification", "ID");
		}

		public void TestNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var supplyChainActorReferenceHeader = header.CusSupplyChainActors.AddNew();

			var bill = header.Bills.AddNew();
			var supplyChainActorReferenceBill = bill.CusSupplyChainActorReferences.AddNew();

			var goodsItems = bill.GoodsItems.AddNew();
			var supplyChainActorReferenceGoodsItem = goodsItems.CusSupplyChainActorReferences.AddNew();

			var movementHeader = header.MovementHeader;
			var supplyChainActorReferenceMovementHeader = movementHeader.CusSupplyChainActors.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Header Level", header.PK, supplyChainActorReferenceHeader.NctsHeader.PK);
				AssertEquals("Bill Level", header.PK, supplyChainActorReferenceBill.NctsHeader.PK);
				AssertEquals("Goods Item Level", header.PK, supplyChainActorReferenceGoodsItem.NctsHeader.PK);
				AssertEquals("Movement Header Level", header.PK, supplyChainActorReferenceMovementHeader.NctsHeader.PK);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			return bill.CusSupplyChainActorReferences.AddNew();
		}
	}

	public abstract class CusSupplyChainActorReferenceAbstractTest<T> : CusReferenceAbstractTest<T> where T : CusSupplyChainActorReference
	{
		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			if (FillWithValidData(header.CusSupplyChainActors.AddNew()) is T supplyChainActor1)
			{
				yield return supplyChainActor1;
			}
			if (FillWithValidData(header.Bills.AddNew().CusSupplyChainActorReferences.AddNew()) is T supplyChainActor2)
			{
				yield return supplyChainActor2;
			}
			if (FillWithValidData(header.Bills.AddNew().GoodsItems.AddNew().CusSupplyChainActorReferences.AddNew()) is T supplyChainActor3)
			{
				yield return supplyChainActor3;
			}
			if (FillWithValidData(header.MovementHeader.CusSupplyChainActors.AddNew()) is T supplyChainActor4)
			{
				yield return supplyChainActor4;
			}
		}

		CusSupplyChainActorReference FillWithValidData(CusSupplyChainActorReference reference)
		{
			reference.CFR_Code = SupplyChainActorRoleList.Codes.CS;
			reference.CFR_Reference = "111";
			return reference;
		}
	}
}
