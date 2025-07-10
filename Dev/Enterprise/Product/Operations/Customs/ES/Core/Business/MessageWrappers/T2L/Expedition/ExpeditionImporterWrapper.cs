using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionImporterWrapper : PartyWrapper
	{
		public new static ExpeditionImporterWrapper New(JobDocAddress docAddress)
		{
			var orgAddress = docAddress?.Address;
			return orgAddress == null ? null : new ExpeditionImporterWrapper(orgAddress);
		}

		ExpeditionImporterWrapper(OrgAddress orgAddress)
		: base(orgAddress)
		{
		}

		protected override ZString IdCore => orgAddress.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Spain ? base.IdCore : null;
	}
}
