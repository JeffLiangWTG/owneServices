using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class LoginOptionsExemptionRoutingDescriptor : ILoginRoutingDescriptor
	{
		public LoginOptionsExemptionRoutingDescriptor(EdiCustomerUserAccount userAccount)
		{
			this.userAccount = userAccount;
		}
		readonly EdiCustomerUserAccount userAccount;

		public bool IsRoutingRequired => userAccount != null && CanBypassLoginOptions(userAccount);

		public Uri RoutingUrl => null;

		public void RoutingAction()
		{
			userAccount.ActivateContactRelationshipAndSave();
		}

		public static bool CanBypassLoginOptions(EdiCustomerUserAccount user)
		{
			var factory = user.Factory;

			var contact = user.WebAccessContact;
			if (contact == null)
			{
				return false;
			}

			var isLoginOptionsAvailable = (!contact.OC_PasswordHash.IsEmpty || !contact.Person.PER_PasswordHash.IsEmpty || !contact.Person.PER_EmailAddress.IsEmpty);
			if (isLoginOptionsAvailable)
			{
				return false;
			}

			var isReactivatedAccountWithNoVerificationOptions = !user.EUA_IsContactRelationshipActive
				&& contact.Person.ContactCollection.Count == 1
				&& user.IsAwaitingActivation;
			if (isReactivatedAccountWithNoVerificationOptions)
			{
				return true;
			}

			var isNonProductionSystemUserRequireVerification = (user.Database.LD_LicenceType != DatabaseTypes.Codes.Production && !user.EUA_IsContactRelationshipActive);
			if (!isNonProductionSystemUserRequireVerification)
			{
				return false;
			}

			var productionUsersQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			productionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, user.EUA_OC_WebAccessContact);
			productionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_Email, user.EUA_Email);
			productionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);
			productionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsContactRelationshipActive, true);
			var productionDbSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), EdiCustomerUserAccountSchema.EUA_LD);
			productionDbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
			productionUsersQuery.AddSubQuery(productionDbSubQuery, JoinCondition.And);

			var hasAtLeastOneActiveVerifiedLinkedProductionSystemUser = factory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, productionUsersQuery);
			if (!hasAtLeastOneActiveVerifiedLinkedProductionSystemUser)
			{
				return false;
			}

			var unverifiedNonProductionUsersQuery = new ZDBOnlyQuery(typeof(EdiCustomerUserAccount));
			unverifiedNonProductionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, user.EUA_OC_WebAccessContact);
			unverifiedNonProductionUsersQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsEmailVerificationRequired, true);
			var nonProductionDbSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), EdiCustomerUserAccountSchema.EUA_LD);
			nonProductionDbSubQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, SQLComparisonOperator.NotEqual, DatabaseTypes.Codes.Production);
			unverifiedNonProductionUsersQuery.AddSubQuery(nonProductionDbSubQuery, JoinCondition.And);

			var allLinkedNonProductionUsersAreVerified = !factory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, unverifiedNonProductionUsersQuery);
			return allLinkedNonProductionUsersAreVerified;
		}
	}
}
