using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class DeclarationCarrierWrapper : IDeclarationCarrier
	{
		DeclarationCarrierWrapper(AsycudaManifestHeader asycudaManifestHeader)
		{
			this.asycudaManifestHeader = Argument.NotNull(asycudaManifestHeader, nameof(asycudaManifestHeader));
		}

		public static DeclarationCarrierWrapper NewOrNull(AsycudaManifestHeader asycudaManifestHeader)
		{
			return asycudaManifestHeader == null ? null : new DeclarationCarrierWrapper(asycudaManifestHeader);
		}

		public IIDType Id
		{
			get
			{
				IIDType result = null;
				if (asycudaManifestHeader.Carrier?.Header is OrgHeader header
				&& header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel) is OrgCusCode vatCustomsCode
				&& !vatCustomsCode.OK_CustomsRegNo.IsEmpty)
				{
					result = IDTypeWrapper.NewOrNull(vatCustomsCode.OK_CustomsRegNo);
				}
				return result;
			}
		}

		readonly AsycudaManifestHeader asycudaManifestHeader;
	}
}
