using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public static class ARContactHelper
	{
		public static Dictionary<Guid, OrgContact> GetContacts(BusinessObjectFactory factory, IEnumerable<Guid> orgPks)
		{
			var query = GetContactQuery(orgPks, true);
			var contacts = factory.Load<OrgContact>(query);
			var orgPkToContact = contacts.GroupBy(x => x.OC_OH.ToGuid()).ToDictionary(x => x.Key, y => y.First());

			var missingOrgs = orgPks.Where(x => !orgPkToContact.ContainsKey(x)).ToList();
			if (missingOrgs.Count > 0)
			{
				query = GetContactQuery(missingOrgs, false);
				contacts = factory.Load<OrgContact>(query);
				foreach (var orgGroup in contacts.GroupBy(x => x.OC_OH.ToGuid()))
				{
					orgPkToContact.Add(orgGroup.Key, orgGroup.First());
				}
			}

			return orgPkToContact;
		}

		static ZQuery GetContactQuery(IEnumerable<Guid> orgPks, bool isDefaultContact)
		{
			var query = new ZDBOnlyQuery(typeof(OrgContact));
			query.AddToFilter(OrgContactSchema.OC_IsActive, true);
			query.AddToFilter(OrgContactSchema.OC_OH, orgPks);
			var docReceivablesQuery = new ZDBOnlySubQuery(typeof(OrgDocument), OrgDocumentSchema.OD_OC);
			docReceivablesQuery.AddToFilter(OrgDocumentSchema.OD_DefaultContact, isDefaultContact);
			docReceivablesQuery.AddToFilter(OrgDocumentSchema.OD_DocumentGroup, new string[] { ContactType.Receivables.Code, ContactType.All.Code });
			query.AddSubQuery(docReceivablesQuery, JoinCondition.And);
			return query;
		}

		public static OrgContact FindRelatedReceivableContact(OrgHeader org)
		{
			if (org != null)
			{
				var orgPkToContact = GetContacts(org.Factory, new Guid[] { org.PK.ToGuid() });
				if (orgPkToContact.Count > 0)
				{
					return orgPkToContact.First().Value;
				}
			}

			return null;
		}
	}
}
