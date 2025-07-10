using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class MyAccountAmbiguousLogin : NonPersistentBusinessObject
	{
		public MyAccountAmbiguousLogin(OrgContact[] contacts)
		{
			Contacts = contacts;
		}

		public IReadOnlyList<OrgContact> Contacts { get; }
	}
}
