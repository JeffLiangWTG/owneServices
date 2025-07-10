using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class DeclarationSubmitterWrapper : IDeclarationSubmitter
	{
		DeclarationSubmitterWrapper(ZString id)
		{
			this.id = id;
		}

		public static DeclarationSubmitterWrapper NewOrNull(AsycudaManifestHeader asycudaManifestHeader)
		{
			if (asycudaManifestHeader?.Declarant?.Header is OrgHeader header
				&& header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel) is OrgCusCode vatCustomsCode
				&& !vatCustomsCode.OK_CustomsRegNo.IsEmpty)
			{
				return new DeclarationSubmitterWrapper(vatCustomsCode.OK_CustomsRegNo);
			}
			return null;
		}

		public IIDType Id => IDTypeWrapper.NewOrNull(id);

		readonly ZString id;
	}
}
