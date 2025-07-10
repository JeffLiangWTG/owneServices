using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PartyContactWrapper : IPartyContactProvider
	{
		public static PartyContactWrapper New(OrgAddress orgAddress) => orgAddress?.Header == null ? null : new PartyContactWrapper(orgAddress.Header.OH_FullName, orgAddress.OA_Email, orgAddress.OA_Phone);

		public static PartyContactWrapper New(JobDocAddress jobDocAddress) => jobDocAddress?.Organisation == null ? null : jobDocAddress.E2_AddressOverride ? new PartyContactWrapper(jobDocAddress.E2_Contact, jobDocAddress.E2_Email, jobDocAddress.E2_Phone) : PartyContactWrapper.New(jobDocAddress.Address);

		public static PartyContactWrapper New(JobDocAddress docAddress, ZBool contactOverride)
		{
			if (contactOverride)
			{
				return New(docAddress.E2_Contact, docAddress.E2_Email, docAddress.E2_Phone);
			}
			return !docAddress.E2_Contact.IsEmpty
			? New(docAddress.Contact.OC_ContactName, docAddress.Contact.Email, docAddress.Contact.OC_Phone)
			: New(docAddress.Address);
		}

		public static PartyContactWrapper New(ZString name, ZString email, ZString phoneNumber) => new PartyContactWrapper(name, email, phoneNumber);

		PartyContactWrapper(ZString name, ZString email, ZString phoneNumber)
		{
			Name = name;
			Email = email;
			PhoneNumber = phoneNumber;
		}

		public ZString Name { get; }

		public ZString Email { get; }

		public ZString PhoneNumber { get; }
	}
}
