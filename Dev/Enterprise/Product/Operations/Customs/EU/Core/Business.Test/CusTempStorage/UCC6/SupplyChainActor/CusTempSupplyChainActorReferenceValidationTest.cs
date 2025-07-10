using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference()
		{
			const string messageError = "Please enter a valid EORI format: 2 chars country code plus less than or equals with 15 numbers.";
			var propertyInfo = supplyChainActor.CFR_ReferenceInfo;
			CombineAssertions(() =>
			{
				supplyChainActor.CFR_Reference = "S1123456789012345";
				AssertHasMessageError(propertyInfo, messageError);

				supplyChainActor.CFR_Reference = "SB123456789012345";
				AssertNoMessageError(propertyInfo, messageError);
			});
		}

		public void TestRuleBR_PN_TS_051_TemporaryStorageBill()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			bill.SupplyChainActors.AddNew();

			var supplyChainActorReference = packedItem.SupplyChainActors.AddNew();
			supplyChainActorReference.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add supply chain actor references when bill already has at least one", supplyChainActorReference, "The Supply Chain Actor Reference should be entered at Bill level or Bill Item level, not both.");

			bill.SupplyChainActors.RemoveAndDeleteAll();
			supplyChainActorReference.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add supply chain actor references when bill has no one", supplyChainActorReference, "The Supply Chain Actor Reference should be entered at Bill level or Bill Item level, not both.");
		}

		public void TestRuleBR_PN_TS_051_TemporaryStoragePackedItem()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.SupplyChainActors.AddNew();

			var supplyChainActorReference = bill.SupplyChainActors.AddNew();
			supplyChainActorReference.Validation.ValidateAll();
			AssertHasRowMessageError("It is prohibited to add supply chain actor references when packed item already has at least one", supplyChainActorReference, "The Supply Chain Actor Reference should be entered at Bill level or Bill Item level, not both.");

			packedItem.SupplyChainActors.DeleteAll();
			supplyChainActorReference.Validation.ValidateAll();
			AssertNoRowMessageError("It is allowed to add supply chain actor references when packed item has no one", supplyChainActorReference, "The Supply Chain Actor Reference should be entered at Bill level or Bill Item level, not both.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			var bill = Factory.New<TemporaryStorageBill>();
			bill.ABL_AMA = tempHeader.PK;
			supplyChainActor = bill.SupplyChainActors.AddNew();
		}
		CusSupplyChainActorReference supplyChainActor;
	}
}
