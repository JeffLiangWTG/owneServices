using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class SetMasterPasswordRoutingDescriptor : ILoginRoutingDescriptor
	{
		public SetMasterPasswordRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
		}
		readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				var result = false;
				if (contact != null)
				{
					var person = contact.Person;
					var relatedContacts = person.ContactCollection;
					result = !person.HasPassword && relatedContacts.Count > 1 && relatedContacts.Cast<OrgContact>().Any(x => x.HasPassword);
				}
				return result;
			}
		}

		public Uri RoutingUrl => new Uri("~/Login/RegisterMasterPassword.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
