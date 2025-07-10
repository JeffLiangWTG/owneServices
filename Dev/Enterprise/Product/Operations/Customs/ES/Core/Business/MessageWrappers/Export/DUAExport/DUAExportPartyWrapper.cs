using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportPartyWrapper : PartyWrapper, IDUAExportPartyProvider
	{
		public new static DUAExportPartyWrapper New(OrgHeader orgHeader)
		{
			var orgAddress = orgHeader?.MainAddress;
			return orgAddress == null ? null : new DUAExportPartyWrapper(orgAddress);
		}

		protected DUAExportPartyWrapper(OrgAddress orgAddress)
			: base(orgAddress)
		{
		}

		public ZString OrganizationCodeQualifier => orgHeader.OH_Category == OrgConstants.Category.NaturalPersonIndividual ? (ZString)"P" : ZString.Empty;
	}
}
