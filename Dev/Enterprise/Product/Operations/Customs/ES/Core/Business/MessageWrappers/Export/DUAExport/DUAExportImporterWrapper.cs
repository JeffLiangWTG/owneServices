using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportImporterWrapper : DUAExportPartyWrapper
	{
		public new static DUAExportImporterWrapper New(OrgHeader orgHeader)
		{
			var orgAddress = orgHeader?.MainAddress;
			return orgAddress == null ? null : new DUAExportImporterWrapper(orgAddress);
		}

		DUAExportImporterWrapper(OrgAddress orgAddress)
			: base(orgAddress)
		{
		}

		protected override ZString IdCore => orgHeader.MainAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Spain ? base.IdCore : null;
	}
}
