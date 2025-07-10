using System;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class DistinctEmailRoutingDescriptor : ILoginRoutingDescriptor
	{
		public DistinctEmailRoutingDescriptor(EdiCustomerUserAccount userAccount)
		{
			this.userAccount = userAccount;
		}
		readonly EdiCustomerUserAccount userAccount;

		public bool IsRoutingRequired
		{
			get
			{
				return userAccount != null
					&& userAccount.EUA_ContactRelationshipStatus == ContactRelationshipStatusList.Codes.DistinctEmailRequired;
			}
		}

		public Uri RoutingUrl => new Uri("~/Login/DistinctEmailRequired.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
