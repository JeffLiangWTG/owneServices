using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}
		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();

			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
			}
		}

		protected override void CheckAMA_LloydsNumber()
		{
			base.CheckAMA_LloydsNumber();

			if (Parent.IsSea)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.AMA_LloydsNumberInfo);
			}
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();

			if (Parent.IsSea && !Parent.AMA_LloydsNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_VesselNameInfo);
			}
		}

		public void ValidateLastForeignPort()
		{
			ValidateCalculatedProperty(Parent.LastForeignPortInfo);
		}

		protected void CheckLastForeignPort()
		{
			if (Parent.IsSea && Parent.AMA_Nature == ShipmentTypeList.Codes.Import23)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.LastForeignPortInfo);
			}
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_ShippingAgentInfo);
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			if (Parent.AMA_OA_Carrier.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
			}
			else
			{
				base.CheckAMA_OA_CarrierMandatory();

				if (Parent.IsSea && SEA309Helper.CarrierCode(Parent).IsEmpty)
				{
					Parent.AMA_OA_CarrierInfo.AddMessageError(ResString.GetMultilingualString("B802BFA4-07E5-4C33-A947-60BD4E4CE0F1", "The selected Carrier must have the CAAT entered."));
				}
			}
		}

		protected override void MandatoryCheckOfCustomsOffice()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_CustomsOfficeInfo);
			}
		}
	}
}
