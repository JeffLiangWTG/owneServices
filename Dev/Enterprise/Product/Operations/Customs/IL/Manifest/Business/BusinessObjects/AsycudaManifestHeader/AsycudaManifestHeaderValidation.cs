//using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public sealed class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateAtLeastOnTransportMeansRecordIsRequired();
		}

		protected override void CheckAMA_ManifestNumber()
		{
			base.CheckAMA_ManifestNumber();

			var parent = Parent;
			var manifestNumberInfo = parent.AMA_ManifestNumberInfo;
			MandatoryValidation.MessageErrorIfNotEntered(manifestNumberInfo);

			if (parent.IsImport)
			{
				var manifestNumber = parent.AMA_ManifestNumber;
				var year = manifestNumber.Left(2);
				if (parent.IsSea && !manifestNumber.IsEmpty
					&& (manifestNumber.Length != 6 || !manifestNumber.IsNumbersOnlyOrEmpty ||
						!(year == ZDateTime.Today.Year.ToString().Substring(2) || year == ZDateTime.Today.AddYears(1).Year.ToString().Substring(2) || year == ZDateTime.Today.AddYears(-1).Year.ToString().Substring(2))))
				{
					manifestNumberInfo.AddMessageError(ValidationCaptions.Manifest.SeaManifestNumberFormatMust6digits);
				}

				if (parent.IsRoad && !manifestNumber.IsEmpty
					&& (manifestNumber.Length != 16 || !manifestNumber.StartsWith("I")))
				{
					manifestNumberInfo.AddMessageError(ValidationCaptions.Manifest.ImportInlandManifestMust16varchars);
				}
			}
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();

			var parent = Parent;
			if (parent.IsRoad && parent.Carrier != null)
			{
				var vatCustomsCode = parent.Carrier.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel);
				if (vatCustomsCode?.OK_CustomsRegNo.IsEmpty ?? true)
				{
					parent.AMA_OA_CarrierInfo.AddMessageError(ValidationCaptions.Manifest.VATNumberIsMissingForPartner(ValidationCaptions.Partners.Carrier));
				}
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}

		protected override void CheckAMA_OA_Declarant()
		{
			base.CheckAMA_OA_Declarant();
			var parent = Parent;

			MandatoryValidation.MessageErrorIfNotEntered(parent.AMA_OA_DeclarantInfo);

			var declarant = parent.Declarant;
			if (declarant != null)
			{
				var vatCustomsCode = declarant.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.ManifestProviderID, CountryCodes.Israel);
				if (vatCustomsCode?.OK_CustomsRegNo.IsEmpty ?? true)
				{
					parent.AMA_OA_DeclarantInfo.AddMessageError(ValidationCaptions.Manifest.CustomsManifestProviderCodeIsMissingForDeclarant);
				}
			}
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			var parent = Parent;

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.AMA_OA_ShippingAgentInfo, parent.AMA_TransportModeInfo, (ZString)TransportTypeList.Codes.Sea, message: ValidationCaptions.Manifest.ShippingAgentIsMissingCheckCarrierOrganizationCarrierAgenciesTab);

			var shippingAgent = parent.ShippingAgent;
			if (shippingAgent != null)
			{
				var vatCustomsCode = shippingAgent.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, CountryCodes.Israel);
				if (vatCustomsCode?.OK_CustomsRegNo.IsEmpty ?? true)
				{
					parent.AMA_OA_ShippingAgentInfo.AddMessageError(ValidationCaptions.Manifest.CustomsCarrierCodeIsMissingForShippingAgent);
				}
			}
		}

		protected override void CheckAMA_VehicleRegistrationCore()
		{
			// Intentionally kept blank: Validation not required
		}

		void ValidateAtLeastOnTransportMeansRecordIsRequired()
		{
			var parent = Parent;
			if (parent.IsRoad && parent.TransportMeans.Count == 0)
			{
				parent.AddRowMessageError(ValidationCaptions.Manifest.AtLeastOneTransportMeansRecordIsRequired);
			}
		}
	}
}
