using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static class CustomerUserAccountRelationshipStatusResolver
	{
		public static void MergeToSameContact(ZGuid sourceUserAccountPk, ZGuid targetUserAccountPk)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var sourceUserAccount = factory.Load<EdiCustomerUserAccount>(sourceUserAccountPk);
			var targetUserAccount = factory.Load<EdiCustomerUserAccount>(targetUserAccountPk);

			var sourceContact = sourceUserAccount.WebAccessContact;
			var targetContact = targetUserAccount.WebAccessContact;
			targetUserAccount.EUA_OC_WebAccessContact = sourceUserAccount.EUA_OC_WebAccessContact;
			targetUserAccount.EUA_ContactRelationshipStatus = sourceUserAccount.EUA_ContactRelationshipStatus = "";
			targetUserAccount.EUA_IsContactRelationshipActive = sourceUserAccount.EUA_IsContactRelationshipActive = true;

			if (targetContact != null)
			{
				targetContact.OC_Email = "";
				targetContact.OC_IsActive = false;
				targetContact.OC_WebAccessEnabled = false;
			}

			factory.Save();

			if (!sourceContact.OC_PER.IsEmpty && targetContact != null && !targetContact.OC_PER.IsEmpty && sourceContact.OC_PER != targetContact.OC_PER)
			{
				using (var personMerger = new PersonMerger(sourceContact.Person, targetContact.Person))
				{
					personMerger.Merge();
				}
			}
		}

		public static void MergeToContact(EdiCustomerUserAccount userAccount, OrgContact targetContact)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var userAccountReloaded = factory.Load<EdiCustomerUserAccount>(userAccount.PK);
			var oldContact = userAccountReloaded.WebAccessContact;

			if (oldContact != null)
			{
				oldContact.OC_Email = "";
				oldContact.OC_IsActive = false;
				oldContact.OC_WebAccessEnabled = false;
			}

			if (!userAccountReloaded.EUA_Email.EqualsIgnoringCase(targetContact.OC_Email))
			{
				userAccountReloaded.EUA_IsEmailOverridden = true;
			}

			userAccountReloaded.EUA_OC_WebAccessContact = targetContact.PK;
			userAccountReloaded.EUA_IsContactRelationshipActive = true;
			userAccountReloaded.EUA_ContactRelationshipStatus = string.Empty;

			factory.Save();

			if (oldContact != null && !oldContact.OC_PER.IsEmpty && !targetContact.OC_PER.IsEmpty && oldContact.OC_PER != targetContact.OC_PER)
			{
				using (var personMerger = new PersonMerger(targetContact.Person, oldContact.Person))
				{
					personMerger.Merge();
				}
			}
		}

		public static void ClearRelationshipStatus(IEnumerable<ZGuid> sourceUserAccountPks)
		{
			var filter = new ZQuery(EdiCustomerUserAccountSchema.PK, sourceUserAccountPks);
			var reader = new FilteredBusinessObjectReader<EdiCustomerUserAccount>(filter, new BusinessObjectFactory());
			reader.BatchSize = 100;
			reader.SaveBeforeLoadNextEnabled = true;
			foreach (EdiCustomerUserAccount userAccount in reader)
			{
				userAccount.EUA_ContactRelationshipStatus = "";
				userAccount.EUA_IsContactRelationshipActive = true;
			}
		}
	}
}
