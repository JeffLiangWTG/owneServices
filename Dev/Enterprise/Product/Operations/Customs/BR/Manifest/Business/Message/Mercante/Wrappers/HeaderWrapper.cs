using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business
{
	internal class HeaderWrapper : IHeader
	{
		internal HeaderWrapper(AsycudaManifestHeader header)
		{
			this.header = header;
		}
		readonly AsycudaManifestHeader header;

		int IHeader.QuantityOfHouseBills => header?.Bills?.Count ?? 0;

		string IHeader.DesconsolidatorAgent
		{
			get
			{
				return header.DeconsolidateAddress?.Header?.CustomsCodes.Cast<OrgCusCode>().
					FirstOrDefault(c => (c.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ || c.OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration)
					&& c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Brazil)?.OK_CustomsRegNo ?? ZString.Empty;
			}
		}

		string IHeader.MasterNumber => header.CustomsOwnNumber;

		string IHeader.Company
		{
			get
			{
				return header.ShippingAgent?.Header?.CustomsCodes.Cast<OrgCusCode>().
					FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.NVOCCReference && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Brazil)?.
					OK_CustomsRegNo ?? ZString.Empty;
			}
		}
	}
}
