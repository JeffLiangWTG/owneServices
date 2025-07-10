using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.KR;

namespace Enterprise.Customs.KR.Business
{
	public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, IOrgCusCodeValidation
	{
		public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
		{
			this.parent = (OrgCusCode)parent;
		}
		readonly OrgCusCode parent;

		protected void ValidateGBR()
		{
			var alphaNumerics = parent.OK_CustomsRegNo.KeepAlphanumericCharacters();
			if (alphaNumerics.Length != 10 || alphaNumerics.KeepNumericCharacters() != alphaNumerics)
			{
				parent.OK_CustomsRegNoInfo.AddError(Res.GetString("96AE6FFB-6F30-47DF-8D48-2AB4D8F2E70C", "Business Number (GBR) must be entered as 10 digits."));
			}
		}

		protected override void ValidateCarrierCode()
		{
			base.ValidateCarrierCode();
			ListValidation.MessageErrorIfInvalidCode(parent.OK_CustomsRegNoInfo, ResString.GetMultilingualString("381BEAEF-87F5-4CCA-8EFD-97C2EE2FFADD", "The code entered does not exist in the Korea Forwarder IDs Reference database."));
		}

		public void ValidateUnipassID()
		{
			if (parent.Header.GetIsIndividual())
			{
				if (parent.OK_CodeType == Messaging.Constants.IdentificationType.UnipassIDForOrganization)
				{
					parent.OK_CodeTypeInfo.AddError(Res.GetString("6836801F-0BFC-4BC3-91B0-053437DA3548", "Individuals cannot use code 06."));
				}
			}
			else
			{
				if (parent.OK_CodeType == Messaging.Constants.IdentificationType.UnipassIDForIndividual)
				{
					parent.OK_CodeTypeInfo.AddError(Res.GetString("AAD04FDB-F4CB-469A-8240-E47B28CDC3E3", "Businesses cannot use code 05."));
				}
			}
		}

		protected override void CheckOK_CustomsRegNo()
		{
			base.CheckOK_CustomsRegNo();

			if (!parent.OK_CustomsRegNoInfo.HasErrors() && parent.CountryIs(Core.Constants.CountryCodes.KoreaSouth))
			{
				switch (parent.OK_CodeType)
				{
					case OrgCusCode.CodeTypes.GovBusinessCode:
						ValidateGBR();
						break;
					case OrgCusCode.CodeTypes.CarrierCode:
						ValidateCarrierCode();
						break;
				}
			}
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();
			switch (parent.OK_CodeType)
			{
				case Messaging.Constants.IdentificationType.UnipassIDForIndividual:
				case Messaging.Constants.IdentificationType.UnipassIDForOrganization:
					ValidateUnipassID();
					break;
			}
		}
	}
}
