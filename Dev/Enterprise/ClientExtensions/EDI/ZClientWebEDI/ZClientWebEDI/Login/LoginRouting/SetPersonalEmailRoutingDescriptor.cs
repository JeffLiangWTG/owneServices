using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class SetPersonalEmailRoutingDescriptor : ILoginRoutingDescriptor
	{
		public SetPersonalEmailRoutingDescriptor(OrgContact contact)
		{
			this.contact = contact;
		}
		readonly OrgContact contact;

		public bool IsRoutingRequired
		{
			get
			{
				var person = contact?.Person as EDIGlbPerson;
				return person != null && person.PER_EmailAddressInternal.IsEmpty && !person.ShouldSkipPersonalEmailPrompt() && !HasCurrentToken;
			}
		}

		public Uri RoutingUrl => new Uri("~/Login/UpdateContactInformation.aspx", UriKind.Relative);

		public void RoutingAction()
		{
		}

		bool HasCurrentToken
		{
			get
			{
				var query = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
				query.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.RegisterPersonalEmail);
				query.AddToFilter(StmAccessTokenSchema.SAT_ExpiresAt, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow);
				return contact.Factory.ExistsInDatabase(StmAccessTokenSchema.Constants.TableName, query);
			}
		}
	}
}
