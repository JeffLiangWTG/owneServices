using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus
{
	public sealed class EdiAccountVerificationStatusCollection : NonPersistentBusinessObjectCollection<EdiAccountVerificationStatus>
	{
		readonly EDIOrgContact contact;

		public EdiAccountVerificationStatusCollection(BusinessObjectFactory factory, EDIOrgContact contact) : base(factory)
		{
			this.contact = contact;
			Fetch();
		}

		EdiCustomerUserAccount[] userAccounts;
		public IReadOnlyList<EdiCustomerUserAccount> UserAccounts
		{
			get
			{
				if (userAccounts == null)
				{
					var ediCustomerUserAccountQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
					ediCustomerUserAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, SQLComparisonOperator.Equal, contact.PK);

					var orQuery = new ZQuery();
					orQuery.AddToFilter(new ZQuery(EdiCustomerUserAccountSchema.EUA_IsContactRelationshipActive, false));
					orQuery.AddToFilter(new ZQuery(EdiCustomerUserAccountSchema.EUA_ContactRelationshipStatus, SQLComparisonOperator.NotEqual, ZString.Empty), JoinCondition.Or);
					ediCustomerUserAccountQuery.AddToFilter(orQuery);

					userAccounts = Factory.Load<EdiCustomerUserAccount>(ediCustomerUserAccountQuery);
				}

				return userAccounts;
			}
		}

		void Fetch()
		{
			if (!contact.OC_IsActive)
			{
				return;
			}

			foreach (var userAccount in UserAccounts)
			{
				var licenceDatabase = userAccount.Database;
				if (licenceDatabase == null)
				{
					continue;
				}

				var accountVerificationStatusEntry = new EdiAccountVerificationStatus(Factory)
				{
					ContactRelationshipStatus = userAccount.EUA_ContactRelationshipStatus,
					Product = licenceDatabase.LD_Product,
					LicenceType = licenceDatabase.LD_LicenceType,
					ServerCode = licenceDatabase.LD_ServerCode,
					IsActive = userAccount.EUA_IsActive,
					UserID = userAccount.EUA_UserID,
					FullName = userAccount.EUA_FullName,
					Email = userAccount.EUA_Email
				};

				Add(accountVerificationStatusEntry);
			}
		}

		public bool RelationshipPromptRequired => Count > 0;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EdiAccountVerificationStatus(Factory);
		}

		public void Reload()
		{
			userAccounts = null;
			RemoveAll();
			Fetch();
		}
	}
}
