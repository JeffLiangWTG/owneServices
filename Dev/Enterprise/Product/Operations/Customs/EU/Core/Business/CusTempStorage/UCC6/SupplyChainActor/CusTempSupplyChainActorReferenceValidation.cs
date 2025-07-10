using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempSupplyChainActorReferenceValidation : CusSupplyChainActorReferenceValidation
	{
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string ruleBR_PN_TS_051Message = Res.GetString("1AF7C421-4F44-42CB-B4F0-9B737651D221", "The Supply Chain Actor Reference should be entered at Bill level or Bill Item level, not both.");
		#pragma warning restore IDE0044 // Restoring IDE0044
		public CusTempSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent) : base(parent)
		{
		}

		protected new CusSupplyChainActorReference Parent => (CusSupplyChainActorReference)base.Parent;

		public override void ValidateAll()
		{
			var parent = Parent;

			parent.ClearRowNotifications();
			base.ValidateAll();

			CheckRuleBR_PN_TS_051(parent);
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			var parent = Parent;
			if (!EuEoriProviderAndValidator.ValidEORIorTCUIFormat(parent.CFR_Reference, parent.Factory))
			{
				parent.CFR_ReferenceInfo.AddMessageError(ValidationConstants.InvalidEoriFormat);
			}
		}

		void CheckRuleBR_PN_TS_051(CusSupplyChainActorReference parent)
		{
			if (parent.Parent is TemporaryStorageBill bill)
			{
				ValidateTemporaryStoragePackedItemsHaveNoSupplyChainActors(parent, bill);
			}
			else if (parent.Parent is TemporaryStoragePackedItem packedItem)
			{
				ValidateTemporaryStorageBillHasNoSupplyChainActors(parent, packedItem);
			}
		}

		void ValidateTemporaryStoragePackedItemsHaveNoSupplyChainActors(CusSupplyChainActorReference actorReference, TemporaryStorageBill bill)
		{
			if (bill.PackedItems.Any(p => p.SupplyChainActors.Count > 0))
			{
				actorReference.AddRowMessageError(ruleBR_PN_TS_051Message);
			}
		}

		void ValidateTemporaryStorageBillHasNoSupplyChainActors(CusSupplyChainActorReference actorReference, TemporaryStoragePackedItem packedItem)
		{
			if (packedItem.Bill.SupplyChainActors.Count > 0)
			{
				actorReference.AddRowMessageError(ruleBR_PN_TS_051Message);
			}
		}
	}
}
