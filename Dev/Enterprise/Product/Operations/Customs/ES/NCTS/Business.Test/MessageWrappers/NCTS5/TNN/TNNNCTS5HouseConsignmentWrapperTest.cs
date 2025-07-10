using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class TNNNCTS5HouseConsignmentWrapperTest : WrapperHelperTest<TNNNCTS5HouseConsignmentWrapper>
	{
		public void TestReferenceNumberUCR()
		{
			nctsBill.B0_ReferenceID = "Reference";
			AssertEquals("Expected filled ReferenceNumberUCR", "Reference", wrapper.ReferenceNumberUCR);
		}

		public void TestConsignmentItem()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty ConsignmentItem when no goodsItem declared", 0, wrapper.ConsignmentItem.Count);

				nctsBill.GoodsItems.AddNew();
				nctsBill.GoodsItems.AddNew();

				wrapper = GetWrapper(nctsBill);
				var consignmentItem = wrapper.ConsignmentItem;
				AssertEquals("Expected filled with 2 ConsignmentItem", 2, consignmentItem.Count);
				AssertSame("Cached ConsignmentItem", wrapper.ConsignmentItem, consignmentItem);
			});
		}

		public void TestDeclarationTypeInLines()
		{
			CombineAssertions(() =>
			{
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				goodsItem1.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected empty DeclarationType in ConsignmentItems when header's declaration type is not T", ZString.Empty, wrapper.ConsignmentItem.ToArray()[0].DeclarationType);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertEquals("Expected filled DeclarationType in ConsignmentItems when header's declaration type is T", "T1", wrapper.ConsignmentItem.ToArray()[0].DeclarationType);

				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				goodsItem2.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertContainsExactElementsInAnyOrder("Expected filled DeclarationType in ConsignmentItem when header's declaration type is T and there are multiple goodsItems with different codes", new ZString[] { "T1", "T2" }, wrapper.ConsignmentItem.Select(x => x.DeclarationType).ToArray());

				goodsItem2.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
				wrapper = GetWrapper(nctsBill);
				AssertContainsExactElementsInAnyOrder("Expected filled DeclarationType in ConsignmentItem when header's declaration type is T and there are multiple goodsItems with same codes", new ZString[] { "T1", "T1" }, wrapper.ConsignmentItem.Select(x => x.DeclarationType).ToArray());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsBill = nctsHeader.Bills.AddNew();
			wrapper = GetWrapper(nctsBill);
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		TNNNCTS5HouseConsignmentWrapper wrapper;

		TNNNCTS5HouseConsignmentWrapper GetWrapper(NctsBill bill, bool shouldDeclareCountryOfDestinationInItem = false) => new TNNNCTS5HouseConsignmentWrapper(bill, shouldDeclareCountryOfDestinationInItem);

		protected override TNNNCTS5HouseConsignmentWrapper GetProvider() => wrapper;
	}
}
