using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsDeparturePartyEmailWrapper : PartyNameWrapper, IPartyEmailProvider
	{
		public static NctsDeparturePartyEmailWrapper New(NctsHeader header)
		{
			var org = header?.DeclarantAddress?.Header ?? header?.MovementHeader?.Representative?.Address?.Header ?? header?.Principal?.Address?.Header;
			return org == null ? null : new NctsDeparturePartyEmailWrapper(header, org);
		}

		NctsDeparturePartyEmailWrapper(NctsHeader header, OrgHeader orgH)
			: base(orgH)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
		}

		readonly NctsHeader nctsHeader;

		public ZString EmailAddress => nctsHeader.DeclEmailAddr;
	}
}
