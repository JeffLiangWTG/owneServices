using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class SupersededLoginRoutingDescriptor : ILoginRoutingDescriptor
	{
		public SupersededLoginRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
		}

		protected readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				var supersededHelper = new OrgContactSupersededHelper(contact);
				return supersededHelper.HasValidContacts;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string")]
		public Uri RoutingUrl => new Uri(contact.Person.HasPassword ? "~/Login/LoginRedirection.aspx" : "~/Login/LoginSuperseded.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
