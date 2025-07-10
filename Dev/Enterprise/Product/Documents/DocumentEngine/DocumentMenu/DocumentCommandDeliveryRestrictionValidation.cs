using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	public class DocumentCommandDeliveryRestrictionValidation : StmMenuDeliveryRestrictionValidation
	{
		public DocumentCommandDeliveryRestrictionValidation(AutoStmMenuDeliveryRestriction parent) : base(parent)
		{
			Validatee = (DocumentCommandDeliveryRestriction)parent;
		}

		protected readonly DocumentCommandDeliveryRestriction Validatee;

		protected override void CheckSDR_DeliveryRestrictionType()
		{
			base.CheckSDR_DeliveryRestrictionType();

			MandatoryValidation.CheckEntered(Validatee.SDR_DeliveryRestrictionTypeInfo, Res.GetString("E8A2F3BD-677E-4B23-8A01-ACE2459D1C27", "Delivery Restriction"));
			ListValidation.ErrorIfInvalidCode(Validatee.SDR_DeliveryRestrictionTypeInfo, Validatee.DeliveryRestrictionTypeList);

			if (Validatee.SDR_DeliveryRestrictionType == nameof(DeliveryRestrictionType.NON) &&
				Validatee.SDR_RN_NKDestinationCountryCode.IsEmpty &&
				Validatee.SDR_RN_NKOriginCountryCode.IsEmpty)
			{
				Validatee.SDR_DeliveryRestrictionTypeInfo.AddError(Res.GetString("93454FFF-F436-47E7-8D89-E10CCDB847B6", "At least one of the Origin/Delivery cannot be empty."));
			}
		}

		protected override void CheckSDR_DeliveryRestrictionMacro()
		{
			base.CheckSDR_DeliveryRestrictionMacro();

			if (Validatee.SDR_DeliveryRestrictionType == nameof(DeliveryRestrictionType.UDF) && Validatee.SDR_DeliveryRestrictionMacro.IsEmpty)
			{
				Validatee.SDR_DeliveryRestrictionMacroInfo.AddError(Res.GetString("447D806D-E408-42DF-84BF-A26D8F913913", "User defined delivery restriction macro cannot be empty."));
			}
		}

		protected override void CheckSDR_RN_NKOriginCountryCode()
		{
			base.CheckSDR_RN_NKOriginCountryCode();
			ListValidation.ErrorIfInvalidCode(Validatee.SDR_RN_NKOriginCountryCodeInfo, Validatee.Lookups.OriginCountryCodes);
		}

		protected override void CheckSDR_RN_NKDestinationCountryCode()
		{
			base.CheckSDR_RN_NKDestinationCountryCode();
			ListValidation.ErrorIfInvalidCode(Validatee.SDR_RN_NKDestinationCountryCodeInfo, Validatee.Lookups.DestinationCountryCodes);
		}
	}
}
