using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidation
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();
			Helper.CheckABL_ManifestQty();
		}

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();

			var info = Parent.ABL_GoodsLocationInfo;
			ListValidation.MessageErrorIfInvalidCode(info);

			if (Parent.Header?.IsNVC01BondedLocationAmendmentSendingInProgress ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckABL_E_DEP()
		{
			base.CheckABL_E_DEP();

			if (Parent.IsHCH)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_DEPInfo);
			}
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			Helper.CheckABL_GoodsDescription();
		}

		protected override void CheckABL_NetWeight()
		{
			base.CheckABL_NetWeight();
			Helper.CheckABL_NetWeight();
		}

		protected override void CheckABL_ShipperStreet2()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperStreet2();
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperCity();
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperPostcode();
			}
		}

		protected override void CheckABL_ShipperPhone()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperPhone();
			}
		}
		protected override void CheckABL_ConsigneeStreet2()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ConsigneeStreet2();
			}
		}

		protected override void CheckABL_ConsigneeCity()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ConsigneeCity();
			}
		}
		protected override void CheckABL_ConsigneePostcode()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ConsigneePostcode();
			}
		}

		protected override void CheckABL_ConsigneePhone()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ConsigneePhone();
			}
		}

		protected override void CheckABL_NotifyPartyStreet2()
		{
			if (Parent.IsNVC)
			{
				Helper.CheckABL_NotifyPartyStreet2();
			}
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			if (Parent.IsNVC)
			{
				Helper.CheckABL_NotifyPartyCity();
			}
		}

		protected override void CheckABL_NotifyPartyPostcode()
		{
			if (Parent.IsNVC)
			{
				Helper.CheckABL_NotifyPartyPostcode();
			}
		}

		protected override void CheckABL_NotifyPartyPhone()
		{
			if (Parent.IsNVC)
			{
				Helper.CheckABL_NotifyPartyPhone();
			}
		}

		AsycudaBillValidationHelper Helper => asycudaBillValidationHelper ??= new AsycudaBillValidationHelper(Parent);

		AsycudaBillValidationHelper asycudaBillValidationHelper;
	}
}
