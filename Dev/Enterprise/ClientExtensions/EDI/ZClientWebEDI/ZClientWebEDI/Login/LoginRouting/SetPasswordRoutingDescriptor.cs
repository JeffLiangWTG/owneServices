using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class SetPasswordRoutingDescriptor : ILoginRoutingDescriptor
	{
		public SetPasswordRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
		}
		readonly OrgContact contact;

		public bool IsRoutingRequired => contact != null && !contact.HasPassword;

		public Uri RoutingUrl => new Uri("~/Login/InitialLoginSetPassword.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
