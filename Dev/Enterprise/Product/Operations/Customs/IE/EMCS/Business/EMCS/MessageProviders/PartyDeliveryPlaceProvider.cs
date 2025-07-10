using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class PartyDeliveryPlaceProvider : PartyAddressProvider, IEMCSPartyDeliveryPlace
	{
		public static new PartyDeliveryPlaceProvider NewOrNull(JobDocAddress jobDocAddress)
			=> jobDocAddress != null && jobDocAddress.IsValidAddress ? new PartyDeliveryPlaceProvider(jobDocAddress) : null;

		PartyDeliveryPlaceProvider(JobDocAddress jobDocAddress)
			: base(jobDocAddress)
		{
			var destinationTypeCode = (jobDocAddress.Parent as EMCSJobDeclaration)?.JE_MessageSubType ?? ZString.Empty;

			if (destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationDirectDelivery
				&& destinationTypeCode != EMCSDestinationTypeList.Codes.DestinationReturnPlaceOfDispatchConsignor)
			{
				var countryCode = jobDocAddress.E2_RN_NKCountryCode;
				var vatCodeType = countryCode.GetVATCodeType();
				var codeType = string.Empty;
				if (destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse)
				{
					codeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
				}
				else
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
					TraderId = codeType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID ? jobDocAddress.Address.GetCustomsRegNoIgnoringCountry(codeType) : org.GetVATRegistrationNumber(countryCode);
				}
			}
			else
			{
				TraderId = ZString.Empty;
			}

			if (destinationTypeCode == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse && !jobDocAddress.E2_AddressOverride)
			{
				City = ZString.Empty;
				StreetAndNumber = ZString.Empty;
				Postcode = ZString.Empty;
			}
			else
			{
				City = base.City;
				StreetAndNumber = base.StreetAndNumber;
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

		public string TraderId { get; }

		public new ZString City { get; }

		public new ZString StreetAndNumber { get; }

		public new ZString Postcode { get; }

		public new ZString Name { get; }
	}
}
