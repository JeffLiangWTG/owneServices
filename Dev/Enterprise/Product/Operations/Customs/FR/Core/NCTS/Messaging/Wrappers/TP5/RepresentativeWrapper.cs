using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class RepresentativeWrapper : IRepresentative
	{
		RepresentativeWrapper(OrgHeader representative)
		{
			this.representative = Argument.NotNull(representative, nameof(representative));
		}

		readonly OrgHeader representative;

		public static RepresentativeWrapper New(OrgHeader representative) => representative == null ? null : new RepresentativeWrapper(representative);

		public string Status => "2";

		public IContactPerson ContactPerson => contactPerson ?? (contactPerson = GetContactPerson());
		IContactPerson contactPerson;

		IContactPerson GetContactPerson()
		{
			var contact = representative.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.Allocations.Cast<OrgContactAllocation>().Any(alloc => alloc.PC_Type == OrgConstants.ContactAllocationType.CUS));
			return contact == null ? null : ContactPersonWrapper.New(contact);
		}

		public string IdentificationNumber => identificationNumber ?? (identificationNumber = representative.MainAddress.GetEORI().Left(17));
		string identificationNumber;

		public string Name => name ?? (name = representative.OH_FullName);
		string name;

		public IAddress Address => address ?? (address = AddressWrapper.New(representative.MainAddress));
		IAddress address;

		public string Role => null;
	}
}
