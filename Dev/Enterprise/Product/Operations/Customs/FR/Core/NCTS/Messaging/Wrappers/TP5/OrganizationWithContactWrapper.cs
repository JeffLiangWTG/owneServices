using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class OrganizationWithContactWrapper : IOrganizationWithContact
	{
		OrganizationWithContactWrapper(OrgHeader orgHeader)
		{
			this.orgHeader = Argument.NotNull(orgHeader, nameof(orgHeader));
		}

		readonly OrgHeader orgHeader;

		public static OrganizationWithContactWrapper New(OrgHeader orgHeader) => orgHeader == null ? null : new OrganizationWithContactWrapper(orgHeader);

		public IContactPerson ContactPerson => contactPerson ?? (contactPerson = GetContactPerson());
		IContactPerson contactPerson;
		IContactPerson GetContactPerson()
		{
			var contact = orgHeader.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>().Any(alloc => alloc.PC_Type == OrgConstants.ContactAllocationType.CUS));
			return contact == null ? null : ContactPersonWrapper.New(contact);
		}

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = orgHeader.MainAddress.GetEORI().Left(17));
		string identificationNumber;

		public string Name => name ?? (name = orgHeader.OH_FullName);
		string name;

		public IAddress Address => address ?? (address = AddressWrapper.New(orgHeader.MainAddress));
		IAddress address;

		public string Role => null;
	}
}
