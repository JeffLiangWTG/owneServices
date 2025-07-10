using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class MasterPasswordManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MasterPasswordManager(GlbPerson person) : base(person?.Factory ?? new BusinessObjectFactory())
		{
			Person = person;
		}

		public GlbPerson Person { get; }

		public OrgContactCollection LoginContacts
		{
			get
			{
				if (loginContacts == null && Person != null)
				{
					var webContactsFilter = GetActiveWebAccessContactsQuery();
					var lloginContacts = new OrgContactCollection(Factory, webContactsFilter);
					lloginContacts.Load();
					loginContacts = lloginContacts;
				}

				return loginContacts;
			}
		}

		OrgContactCollection loginContacts;

		public OrgContactCollection PasswordHoldingContacts
		{
			get
			{
				if (passwordHoldingContacts == null && Person != null)
				{
					var webContactsFilter = GetActiveWebAccessContactsQuery();
					webContactsFilter.AddToFilter(OrgContactSchema.OC_PasswordHash, SQLComparisonOperator.NotEqual, null);
					var lpasswordHoldingContacts = new OrgContactCollection(Factory, webContactsFilter);
					lpasswordHoldingContacts.Load();
					passwordHoldingContacts = lpasswordHoldingContacts;
				}

				return passwordHoldingContacts;
			}
		}

		OrgContactCollection passwordHoldingContacts;

		ZQuery GetActiveWebAccessContactsQuery()
		{
			var webContactsFilter = new ZQuery(OrgContactSchema.OC_PER, Person.PK);
			webContactsFilter.AddToFilter(OrgContactSchema.OC_IsActive, true);
			webContactsFilter.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);

			return webContactsFilter;
		}
	}
}
