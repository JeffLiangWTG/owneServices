using System;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class TermsAndConditionsRoutingDescriptor : ILoginRoutingDescriptor
	{
		public TermsAndConditionsRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
		}
		readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				bool result = false;
				if (contact != null)
				{
					result = new EDIOrgHeaderWebContract(contact).NeedSignWebContract || new EDIOrgContactWebContract(contact).NeedSignWebContract;
				}
				return result;
			}
		}
		public Uri RoutingUrl => new Uri("~/Login/TermsAndConditions.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}
	}
}
