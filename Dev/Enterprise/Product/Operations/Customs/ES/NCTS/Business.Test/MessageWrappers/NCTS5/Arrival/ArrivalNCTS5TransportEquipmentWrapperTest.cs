using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5TransportEquipmentWrapperTest : WrapperHelperTest<ArrivalNCTS5TransportEquipmentWrapper>
	{
		public void TestGoodsReference()
		{
			container.ItemNumbers.AddNew().CY_DataNumeric = 1;
			container.ItemNumbers.AddNew().CY_DataNumeric = 3;

			wrapper = GetWrapper(container, 2);
			var reference = wrapper.GoodsReference;

			CombineAssertions(() =>
			{
				AssertEquals("Expected 2 Reference when declared", 2, reference.Count);
				AssertSame("Cached Reference", wrapper.GoodsReference, reference);

				var goodsReferenceArray = reference.ToArray();

				AssertEquals("For first goodsReference expected filled DeclarationGoodsItemNumber", "1", goodsReferenceArray[0].DeclarationGoodsItemNumber);
				AssertEquals("For first goodsReference expected filled SequenceNumber", "1", goodsReferenceArray[0].SequenceNumber);

				AssertEquals("For second goodsReference expected empty DeclarationGoodsItemNumber", "3", goodsReferenceArray[1].DeclarationGoodsItemNumber);
				AssertEquals("For second goodsReference expected filled SequenceNumber", "2", goodsReferenceArray[1].SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<NctsContainer>();
			wrapper = GetWrapper(container, 2);
		}

		NctsContainer container;
		ArrivalNCTS5TransportEquipmentWrapper wrapper;

		ArrivalNCTS5TransportEquipmentWrapper GetWrapper(NctsContainer container, ZShort seqNum) => new ArrivalNCTS5TransportEquipmentWrapper(container, seqNum);

		protected override ArrivalNCTS5TransportEquipmentWrapper GetProvider() => wrapper;
	}
}
