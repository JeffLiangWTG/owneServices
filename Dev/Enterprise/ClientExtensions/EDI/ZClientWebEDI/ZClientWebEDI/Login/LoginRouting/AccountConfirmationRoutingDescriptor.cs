using System;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class AccountConfirmationRoutingDescriptor : ILoginRoutingDescriptor
	{
		public AccountConfirmationRoutingDescriptor(OrgContact contact)
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
					result = (contact as EDIOrgContact)?.GetMostRecentUnlinkedUserAccount() != null;
				}

				return result;
			}
		}
		public Uri RoutingUrl => new Uri("~/Login/AccountConfirmation.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
