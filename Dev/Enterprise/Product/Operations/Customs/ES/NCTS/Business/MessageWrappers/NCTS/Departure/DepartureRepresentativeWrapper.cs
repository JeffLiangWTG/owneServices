using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureRepresentativeWrapper : PartyWrapper
	{
		public static DepartureRepresentativeWrapper New(NctsHeader header)
		{
			OrgAddress representativeOrgAddress = null;

			var representativeOrg = header?.MovementHeader?.Representative?.Address?.Header;
			var principalOrg = header?.Principal?.Address?.Header;

			if (representativeOrg != null)
			{
				var representativeId = representativeOrg.GetIDCode();
				var principalId = principalOrg.GetIDCode();

				representativeOrgAddress = representativeId.IsEmpty || representativeId != principalId ? representativeOrg.MainAddress : null;
			}

			return representativeOrgAddress == null ? null : new DepartureRepresentativeWrapper(representativeOrgAddress);
		}

		DepartureRepresentativeWrapper(OrgAddress address)
			: base(address)
		{
		}
	}
}

