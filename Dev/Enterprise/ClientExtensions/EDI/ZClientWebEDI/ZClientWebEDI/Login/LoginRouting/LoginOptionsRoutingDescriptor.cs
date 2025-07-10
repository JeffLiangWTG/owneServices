using System;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LoginOptionsRoutingDescriptor : ILoginRoutingDescriptor
	{
		public LoginOptionsRoutingDescriptor(EdiCustomerUserAccount userAccount, OrgContact contact)
		{
			this.userAccount = userAccount;
			this.contact = contact;
		}
		readonly EdiCustomerUserAccount userAccount;
		readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				if (userAccount == null || contact == null)
				{
					return false;
				}
				return EDIDataRegistry.Instance.EnableMyAccountPersonalPasswordProtection.Value && !userAccount.EUA_IsContactRelationshipActive;
			}
		}

		public Uri RoutingUrl => new Uri("~/Login/LoginOptions.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
