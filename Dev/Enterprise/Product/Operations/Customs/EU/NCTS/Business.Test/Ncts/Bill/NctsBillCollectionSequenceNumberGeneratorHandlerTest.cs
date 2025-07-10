using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillCollectionSequenceNumberGeneratorHandlerTest : TestCaseWithFactory
	{
		public void TestOnCollectionCountChange_InvalidArguments__DoesNotCallAnyRecalculationMethods()
		{
			var nctsBill = billCollection.AddNew();
			nctsBill.SequenceNumber = 0;
			CombineAssertions("Invalid Arguments", () =>
			{
				NctsBillCollectionSequenceNumberGeneratorHandler.OnCollectionCountChange(null, new CollectionCountChangedEventArgs(true, nctsBill));
				AssertEquals("When sender is null, Sequence Number", (ZShort)0, nctsBill.SequenceNumber);

				NctsBillCollectionSequenceNumberGeneratorHandler.OnCollectionCountChange(billCollection, null);
				AssertEquals("When CountChangedEventArgs is null, Sequence Number", (ZShort)0, nctsBill.SequenceNumber);
			});
		}

		public void TestOnCollectionCountChange_ItemAdded_CallsRecalculateWhenAdded()
		{
			billCollection.CollectionCountChange += NctsBillCollectionSequenceNumberGeneratorHandler.OnCollectionCountChange;
			var bill = billCollection.AddNew();

			AssertEquals("When a new bill is added to the collection, Sequence Number", (ZShort)1, bill.SequenceNumber);
		}

		public void TestOnCollectionCountChange_ItemRemoved_CallsRecalculateWhenRemoved()
		{
			billCollection.CollectionCountChange += NctsBillCollectionSequenceNumberGeneratorHandler.OnCollectionCountChange;
			var bill1 = billCollection.AddNew();
			var bill2 = billCollection.AddNew();
			var bill3 = billCollection.AddNew();

			bill2.Delete();

			CombineAssertions(() =>
			{
				AssertEquals("Bill1.SequenceNumber", (ZShort)1, bill1.SequenceNumber);
				AssertEquals("Bill2.SequenceNumber", (ZShort)2, bill3.SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			billCollection = new NctsBillCollection<NctsBill>(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsBillCollection<NctsBill> billCollection;
	}
}
