using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
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
			ValidateCustomsOwnNumber();
		}

		public void ValidateCustomsOwnNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsOwnNumberInfo);
		}

		protected void CheckCustomsOwnNumber()
		{
			if (Parent.IsMercante)
			{
				if (Parent.CustomsOwnNumber.IsEmpty)
				{
					Parent.CustomsOwnNumberInfo.AddMessageError(Res.GetString("8617842A-151B-4E30-9B7C-437C1693C1BA", "You have not entered a CE Merchant."));
				}
			}
			else
			{
				if (!BRCusEntryNumValidationHelper.CheckValidMasterUCREntryNumberFormat(Parent.CustomsOwnNumber))
				{
					Parent.CustomsOwnNumberInfo.AddMessageError(BRCusEntryNumValidationHelper.GetUCREntryNumberInvalidMessage((NoResString)"Master UCR"));
				}
			}
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();

			if (Parent.IsMercante && Parent.IsSea && Parent.AMA_Nature.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_NatureInfo);
			}
		}

		protected override void CheckAMA_OA_DeconsolidateAddress()
		{
			base.CheckAMA_OA_DeconsolidateAddress();

			if (Parent.IsMercante)
			{
				var deconsolidateAddress = Parent.DeconsolidateAddress;
				if (deconsolidateAddress == null)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_DeconsolidateAddressInfo);
				}
				else
				{
					var regNumber = deconsolidateAddress?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => (c.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ || c.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration)
					&& c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Brazil);

					if (regNumber == null)
					{
						Parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError(Res.GetString("2BC8B564-B0DF-4F06-87CD-EBB529F7E495", "The selected Deconsolidator does not have a CJN or CPF number"));
					}
				}
			}
		}
	}
}
