using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class RepresentativeWrapper : IRepresentative
	{
		RepresentativeWrapper(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly TemporaryStorageHeader header;

		public string Name => OrgHeader?.OH_FullName;

		public OrgHeader OrgHeader => orgHeader ?? (orgHeader = header.Representative.Header);
		OrgHeader orgHeader;

		public ICommunication Communication => communication ?? (communication = GetCommunication());

		ICommunication communication;

		ICommunication GetCommunication()
		{
			OrgContact orgContact = null;
			if (header.Representative != null)
			{
				orgContact = GetOrgContact(header);
			}

			return orgContact != null ? CommunicationWrapper.New(orgContact) : null;
		}

		static OrgContact GetOrgContact(AsycudaManifestHeader header)
		{
			var orgAddress = header.Representative;
			var contact = orgAddress.Header.Contacts.OfType<OrgContact>().FirstOrDefault(x => x.WorkingAddressPK == orgAddress.PK && x.Allocations.OfType<OrgContactAllocation>().Any(y => y.PC_Type == OrgConstants.ContactAllocationType.CUS));
			return contact;
		}

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = header.Representative.GetEuIdentificationNumber(header.AMA_RN_NKCountry, true));
		string identificationNumber;

		public byte Status => status != 0 ? status : (status = GetStatus());
		byte status;

		byte GetStatus()
		{
			return (byte)(header.Representative == header.Declarant ? 3 : 2);
		}

		public static RepresentativeWrapper New(TemporaryStorageHeader header) => header?.Representative == null ? null : new RepresentativeWrapper(header);
	}
}
