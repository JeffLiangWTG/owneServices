using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class ContactDeactivationWrapper : DeactivationWrapperBase
	{
		public ContactDeactivationWrapper(OrgContact contact) : base(contact.Factory)
		{
			Contact = contact;
		}

		public OrgContact Contact { get; }

		public override ZString Organisation => FormattableString.Invariant($"{Contact.OrgCode} - {Contact.WorkingAddressCompanyName} ({Contact.OC_Email})");
		public override ZString LicenceType => ZString.Empty;
		public override ZString SystemInfo => ZString.Empty;
		public override int ReferenceNumber => -1;
	}
}
