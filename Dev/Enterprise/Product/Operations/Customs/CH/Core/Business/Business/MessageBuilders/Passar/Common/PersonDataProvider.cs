using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class PersonDataProvider : IPerson
{
	public static PersonDataProvider New(OrgHeader organisation, GlbStaff staff) => organisation == null && staff == null ? null : new PersonDataProvider(organisation, staff);

	PersonDataProvider(OrgHeader organisation, GlbStaff staff)
	{
		this.organisation = organisation;
		this.staff = staff;
	}

	readonly OrgHeader organisation;
	readonly GlbStaff staff;

	public string IdentificationNumber => identificationNumber ?? (identificationNumber = organisation?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.BID).ReturnNullIfEmpty());
	string identificationNumber;

	public string AeoReferenceNumber => aeoReferenceNumber ?? (aeoReferenceNumber = organisation?.GetAEONumber().ReturnNullIfEmpty());
	string aeoReferenceNumber;

	public IContactPerson ContactPerson => contactPerson ?? (contactPerson = StaffContactPersonDataProvider.New(staff));
	IContactPerson contactPerson;
}
