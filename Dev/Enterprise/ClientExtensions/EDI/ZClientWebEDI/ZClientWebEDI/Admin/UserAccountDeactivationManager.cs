using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class UserAccountDeactivationManager : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UserAccountDeactivationManager(GlbPerson person) : base(person?.Factory)
		{
			Person = person;
		}

		public GlbPerson Person { get; }

		public List<DeactivationWrapperBase> ContactUserAccountWrappers
		{
			get
			{
				if (contactUserAccountWrappers == null)
				{
					var referenceNumber = 0;
					contactUserAccountWrappers = new List<DeactivationWrapperBase>();
					DisplayedContacts = new List<ContactGrouping>();
					foreach (var contact in Person.ContactCollection.Cast<OrgContact>().OrderBy(x => x.OrgCode))
					{
						if (contact.OC_IsActive && contact.OC_WebAccessEnabled)
						{
							var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, contact.PK);
							userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);
							var userAccountsBase = Factory.Load<EdiCustomerUserAccount>(userAccountQuery).OrderBy(x => x.Database.LD_LicenceType).ToArray();

							if (userAccountsBase.Any())
							{
								contactUserAccountWrappers.Add(new ContactDeactivationWrapper(contact));
								var contactGrouping = new ContactGrouping(contact);
								DisplayedContacts.Add(contactGrouping);

								foreach (var account in userAccountsBase)
								{
									contactUserAccountWrappers.Add(new UserAccountDeactivationWrapper(account, referenceNumber));
									contactGrouping.UserAccounts.Add(account);
									referenceNumber++;
								}
							}
						}
					}
				}

				return contactUserAccountWrappers;
			}
		}

		public List<ContactGrouping> DisplayedContacts
		{
			get;
			private set;
		}

		List<DeactivationWrapperBase> contactUserAccountWrappers;

		public class ContactGrouping
		{
			public ContactGrouping(OrgContact contact)
			{
				Contact = contact;
			}

			public OrgContact Contact { get; }

			public List<EdiCustomerUserAccount> UserAccounts = new List<EdiCustomerUserAccount>();
		}
	}
}
