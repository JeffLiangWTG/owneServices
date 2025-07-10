using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class H7DeclarantWrapper : PartyIdWrapper, IH7Declarant
{
	public H7DeclarantWrapper(AsycudaBill bill) : base(bill.Header.Declarant?.Header)
	{
		declarant = bill.Header.Declarant;
		contactPerson = declarant?.Header.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CUS);
	}

	public IPartyContactProvider ContactInfo
	{
		get
		{
			return contactPerson == null
				? null
				: PartyContactWrapper.New(contactPerson.OC_ContactName, contactPerson.OC_Email, contactPerson.OC_Mobile);
		}
	}

	protected override ZString IdCore => orgHeader.GetEOROrNIFCode();

	public ZBool IsImporter => false;

	public ZString Address => declarant?.OA_Address1 + " " + declarant?.OA_Address2;

	public ZString City => declarant.City;

	public ZString PostCode => declarant.Postcode;

	public ZString Country => declarant.OA_RN_NKCountryCode;

	public ZString Name => declarant.Header.OH_FullName;

	readonly OrgAddress declarant;
	readonly OrgContact contactPerson;
}
