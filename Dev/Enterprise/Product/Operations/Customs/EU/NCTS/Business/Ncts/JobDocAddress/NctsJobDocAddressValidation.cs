using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsJobDocAddressValidation : JobDocAddressValidation
	{
		public NctsJobDocAddressValidation(JobDocAddress addressToValidate, NctsCommonCargoDesc goodsItem)
			: base(addressToValidate)
		{
			GoodsItem = goodsItem;
		}

		public NctsJobDocAddressValidation(JobDocAddress addressToValidate, NctsHeader header)
			: base(addressToValidate)
		{
			Header = header;
		}

		void MarkParentAsNeedingValidation()
		{
			if (Header != null && Header.HasChanges)
			{
				Header.MarkAsNeedingValidation();
				Header.MovementHeader?.MarkAsNeedingValidation(); // Stops LightValidation tests failing on addresses
			}
			if (GoodsItem != null && GoodsItem.HasChanges)
			{
				GoodsItem.MarkAsNeedingValidation(); // Stops LightValidation tests failing on addresses
			}
		}

		protected override void CheckE2_AddressOverride()
		{
			base.CheckE2_AddressOverride();
			MarkParentAsNeedingValidation();
		}

		protected override void CheckE2_AddressType()
		{
			base.CheckE2_AddressType();
			MarkParentAsNeedingValidation();
		}

		protected override void CheckE2_ParentID()
		{
			base.CheckE2_ParentID();
			MarkParentAsNeedingValidation();
		}

		protected override void CheckE2_ParentTableCode()
		{
			base.CheckE2_ParentTableCode();
			MarkParentAsNeedingValidation();
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			MarkParentAsNeedingValidation();
		}

		protected bool IsArrivalMovement => GetHeader()?.IsArrivalMovement ?? false;
		protected bool IsDepartureMovement => GetHeader()?.IsDepartureMovement ?? false;
		protected bool IsPhase5 => GetHeader()?.IsPhase5 ?? false;
		protected bool IsPhase5Arrival => GetHeader()?.IsPhase5Arrival ?? false;
		protected bool IsPhase5Departure => GetHeader()?.IsPhase5Departure ?? false;

		protected NctsHeader GetHeader() => Header ?? GoodsItem?.Header;

		protected NctsHeader Header { get; }
		protected NctsCommonCargoDesc GoodsItem { get; }
	}
}
