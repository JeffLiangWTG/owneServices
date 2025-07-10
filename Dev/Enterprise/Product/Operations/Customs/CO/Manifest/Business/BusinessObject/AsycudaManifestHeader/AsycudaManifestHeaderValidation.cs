using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateCargoDisposition();
			ValidateTravelDocumentType();
			ValidateDeliveryMode();
		}

		#region CargoDisposition

		public void ValidateCargoDisposition()
		{
			ValidateCalculatedProperty(Parent.CargoDispositionInfo);
		}

		protected void CheckCargoDisposition()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CargoDispositionInfo);
			}
		}

		#endregion

		#region TravelDocumentType

		public void ValidateTravelDocumentType()
		{
			ValidateCalculatedProperty(Parent.TravelDocumentTypeInfo);
		}

		protected void CheckTravelDocumentType()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TravelDocumentTypeInfo);
			}
		}

		#endregion

		#region DeliveryMode

		public void ValidateDeliveryMode()
		{
			ValidateCalculatedProperty(Parent.DeliveryModeInfo);
		}

		protected void CheckDeliveryMode()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DeliveryModeInfo);
			}
		}

		#endregion

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();

			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
			}
		}

		protected override void MandatoryCheckOfCustomsOffice()
		{
			if (Parent.IsSea)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_CustomsOfficeInfo);
			}
		}

		protected override void CheckAMA_ContainerMode()
		{
			base.CheckAMA_ContainerMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_ContainerModeInfo);
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();

			if (!Parent.AMA_OA_Carrier.IsEmpty)
			{
				var carrier = Parent.Carrier;

				if (carrier != null)
				{
					if (carrier.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Colombia)
					{
						var carrierNIT = carrier.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT)?.OK_CustomsRegNo ?? ZString.Empty;

						if (carrierNIT.IsEmpty)
						{
							Parent.AMA_OA_CarrierInfo.AddMessageError(ResString.GetMultilingualString("A0240405-1BD5-4F7C-B4EF-BAC65D2ACBF0", "The selected Carrier should have NIT"));
						}
					}
				}
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_ShippingAgentInfo);

			if (!Parent.AMA_OA_ShippingAgent.IsEmpty)
			{
				var agent = Parent.ShippingAgent;

				if (agent != null)
				{
					if (agent.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Colombia)
					{
						var agentNIT = agent.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == ColombiaOrgCusCodeInfo.OrgCusCodes.NIT)?.OK_CustomsRegNo ?? ZString.Empty;

						if (agentNIT.IsEmpty)
						{
							Parent.AMA_OA_ShippingAgentInfo.AddMessageError(ResString.GetMultilingualString("378A7E35-EB77-41C7-B979-E94C0612FB62", "The selected Shipping Agent should have NIT"));
						}
					}
				}
			}
		}
	}
}
