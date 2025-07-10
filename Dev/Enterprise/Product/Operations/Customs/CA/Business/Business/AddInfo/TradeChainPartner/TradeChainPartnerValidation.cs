using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerValidation : CusAddInfoValidation
	{
		public TradeChainPartnerValidation(TradeChainPartner parent) : base(parent)
		{
		}

		public new TradeChainPartner Parent => (TradeChainPartner)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCA_Org();
			ValidateCA_Address();
		}

		public void ValidateCA_Org()
		{
			ValidateCalculatedProperty(Parent.CA_OrgInfo);
		}

		protected void CheckCA_Org()
		{
			Parent.CAOrgAddress.ClearRowNotifications();
			MandatoryValidation.CheckEntered(Parent.CA_OrgInfo);
			TypeValidation.CheckValidGuid(Parent.CAOrgAddress.OrganisationPKInfo);
		}

		public void ValidateCA_Address()
		{
			ValidateCalculatedProperty(Parent.CA_AddressInfo);
		}

		protected void CheckCA_Address()
		{
			if (!Parent.CA_Type.IsEmpty && !Parent.CA_Address.IsEmpty)
			{
				var address = Parent.OrganizationAddress;
				var country = address.Country.Code;
				var info = Parent.CA_AddressInfo;
				if (Parent.CA_Type == TradeChainPartnersTypeList.Codes.C)
				{
					if (country != Core.Constants.CountryCodes.Canada)
					{
						info.AddMessageError(Res.GetString("96E1F929-0873-44FA-A0ED-611E7F04C6D9", "Country/Region for Consignee Address should be Canada."));
					}
					else
					{
						CAAddressValidator.StateValidation(address.StateCode, info, Core.Constants.CountryCodes.Canada);
						CAAddressValidator.PostCodeValidation(address.Postcode, info, Core.Constants.CountryCodes.Canada);
					}
				}
				else if (Parent.CA_Type == TradeChainPartnersTypeList.Codes.V)
				{
					if (country != Core.Constants.CountryCodes.UnitedStates && country != Core.Constants.CountryCodes.Mexico)
					{
						info.AddMessageError(Res.GetString("217A35DF-89EF-47B9-8765-04274D7D604E", "Country/Region for Vendor Address should be United States or Mexico."));
					}
					else
					{
						CAAddressValidator.StateValidation(address.StateCode, info, Core.Constants.CountryCodes.UnitedStates);
						CAAddressValidator.PostCodeValidation(address.Postcode, info, Core.Constants.CountryCodes.UnitedStates);
					}
				}
			}
		}
	}
}
