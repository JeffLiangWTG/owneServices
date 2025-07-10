using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PartyDeliveryPlaceProvider : PartyAddressProvider, IEMCSPartyDeliveryPlace
	{
		protected PartyDeliveryPlaceProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
		{
			var destinationTypeCode = (jobDocAddress.Parent as EMCSJobDeclaration)?.JE_MessageSubType ?? ZString.Empty;
			var countryCode = jobDocAddress.E2_RN_NKCountryCode;
			var vatCodeType = countryCode.GetVATCodeType();

			var codeType = string.Empty;
			if (destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse)
			{
				codeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			}
			else if (destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationTaxWarehouse
				&& destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationDirectDelivery)
			{
				codeType = vatCodeType;
			}

			if (jobDocAddress.E2_AddressOverride)
			{
				TraderId = jobDocAddress.E2_GovRegNumType == codeType ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
			}
			else
			{
				var org = jobDocAddress.Organisation;
				if (codeType == vatCodeType)
				{
					TraderId = org.GetVATRegistrationNumber(countryCode);
				}
				else if (codeType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID)
				{
					TraderId = jobDocAddress.Address.GetCustomsRegNoIgnoringCountry(codeType);
				}
			}

			if (destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse && !jobDocAddress.E2_AddressOverride)
			{
				City = ZString.Empty;
				Address = ZString.Empty;
				Postcode = ZString.Empty;
			}
			else
			{
				City = base.City;
				Address = base.Address;
				Postcode = base.Postcode;
			}

			if (destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationDirectDelivery && !jobDocAddress.E2_AddressOverride)
			{
				Name = ZString.Empty;
			}
			else
			{
				Name = base.Name;
			}
		}

		public static new PartyDeliveryPlaceProvider NewOrNull(JobDocAddress jobDocAddress) => jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyDeliveryPlaceProvider(jobDocAddress) : null;

		public string TraderId { get; }

		public new ZString City { get; }

		public new ZString Address { get; }

		public new ZString Postcode { get; }

		public new ZString Name { get; }
	}
}
