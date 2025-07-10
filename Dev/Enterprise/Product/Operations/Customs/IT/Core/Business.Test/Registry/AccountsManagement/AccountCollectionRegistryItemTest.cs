using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(AccountCollectionRegistryItem))]
sealed class AccountCollectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AccountCollection>
{
	protected override StronglyTypedRegistryItem<AccountCollection, AccountCollection> GetNewRegistryItem()
	{
		return new AccountCollectionRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, new AccountCollection());
	}

	protected override AccountCollection ValidValue => new AccountCollection();

	public void TestAccountCollectionGroupedByCompany()
	{
		var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
		var accountsManagement = ITAccountsManagementRegistry.Instance.AccountsManagement;
		var currentAccountCollection = accountsManagement.GetValueWithoutFallback(currentCompanyPk, Guid.Empty, Guid.Empty);

		AssertEquals(0, currentAccountCollection.Count);

		new AccountCollectionTestBuilder(currentCompanyPk)
			.AppendAccount("01234567890-123", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("01234567890-456", "1235").AppendAccountDetail("1235-DEC1", "DEC1")
			.Build();

		var accountCollectionsGroupedByCompany = accountsManagement.AccountCollectionsGroupedByCompany;

		AssertNotNull("AccountCollectionGroupedByCompany should be not null", accountCollectionsGroupedByCompany);
		AssertEquals("AccountCollectionGroupedByCompany count", 1, accountCollectionsGroupedByCompany.Count());

		var accountCollection = accountCollectionsGroupedByCompany.SingleOrDefault(x => x.CompanyPK == currentCompanyPk);
		CombineAssertions("AccountCollection", () =>
		 {
			 AssertEquals("Company PK", GlbCompany.CurrentCompany.PK, accountCollection.CompanyPK);
			 AssertEquals("AccountCollection count", 2, accountCollection.Accounts.Count);
			 var accountNumberCollection = accountCollection.Accounts.Cast<Account>().Select(x => x.AccountNumber);
			 AssertCollectionContains("01234567890-123", accountNumberCollection);
			 AssertCollectionContains("01234567890-456", accountNumberCollection);
		 });

		//Add a new company
		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.GC_RN_NKCountryCode = "DE";
		otherCompany.Branches.AddNew();
		Factory.Save();

		new AccountCollectionTestBuilder(otherCompany.PK)
			.AppendAccount("09876543210-321", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		AssertEquals("AccountCollectionGroupedByCompany count", 1, accountsManagement.AccountCollectionsGroupedByCompany.Count());

		accountsManagement.ResetAccountCollectionsGroupedByCompany();
		AssertEquals("AccountCollectionGroupedByCompany count", 2, accountsManagement.AccountCollectionsGroupedByCompany.Count());

		accountCollection = accountsManagement.AccountCollectionsGroupedByCompany.SingleOrDefault(x => x.CompanyPK == otherCompany.PK);
		CombineAssertions("AccountCollection", () =>
		{
			AssertEquals("Company PK", otherCompany.PK, accountCollection.CompanyPK);
			AssertEquals("AccountCollection count", 1, accountCollection.Accounts.Count);
			var accountNumberCollection = accountCollection.Accounts.Cast<Account>().Select(x => x.AccountNumber);
			AssertCollectionContains("09876543210-321", accountNumberCollection);
		});
	}

	public void TestAccountCollectionsGroupedByCompanyInEditing()
	{
		var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
		var accountsManagement = ITAccountsManagementRegistry.Instance.AccountsManagement;

		new AccountCollectionTestBuilder(currentCompanyPk)
			.AppendAccount("01234567890-123", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("01234567890-456", "1235").AppendAccountDetail("1235-DEC1", "DEC1")
			.Build();

		var accountCollectionsGroupedByCompany = accountsManagement.AccountCollectionsGroupedByCompany;

		AssertNotNull("AccountCollectionGroupedByCompany should be not null", accountCollectionsGroupedByCompany);
		AssertEquals("AccountCollectionGroupedByCompany count", 1, accountCollectionsGroupedByCompany.Count());

		var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
		anotherCompany.GC_RN_NKCountryCode = "DE";
		anotherCompany.Branches.AddNew();
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(anotherCompany.PK)
			.AppendAccount("09876543210-321", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.AppendAccount("09876543210-654", "BBBB").AppendAccountDetail("BBBB-DEC1", "DEC1")
			.Build();

		accountsManagement.AddToAccountCollectionInEditingCache(accountCollection, new FallbackLevel(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		AssertEquals("AccountCollectionGroupedByCompany", 2, accountsManagement.AccountCollectionsGroupedByCompany.Count());

		AssertEquals("PRE-CONDITION for subsequent assertions", "AAAA", accountCollection[0].AccountNode);
		accountCollection[0].AccountNode = "EEEE";
		var newAccount = accountCollection.AddNew();
		newAccount.AccountNumber = "99999999999-999";
		newAccount.AccountNode = "9999";
		var accountDetail = newAccount.AccountDetails.AddNew();
		accountDetail.InternalCode = "9999-DEC1";
		accountDetail.DeclarantCode = "DEC1";

		accountsManagement.AddToAccountCollectionInEditingCache(accountCollection, new FallbackLevel(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		var updatedCacheForAnotherCompany = accountsManagement.AccountCollectionsGroupedByCompany.SingleOrDefault(x => x.CompanyPK == anotherCompany.PK);
		AssertEquals("Expected cached accounts count for 'anotherCompany'", 3, updatedCacheForAnotherCompany.Accounts.Count);
		AssertEquals("Accounts[0].AccountNode has changed", "EEEE", updatedCacheForAnotherCompany.Accounts[0].AccountNode);
		AssertEquals("Accounts[1].AccountNode has NOT changed", "BBBB", updatedCacheForAnotherCompany.Accounts[1].AccountNode);
		AssertEquals("Accounts[2].AccountNode has been added", "9999", updatedCacheForAnotherCompany.Accounts[2].AccountNode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
	}
}

[TestedType(typeof(AccountCollectionRegistryDataType))]
sealed class AccountCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AccountCollectionRegistryDataType>
{
	protected override string ExpectedEditorName
	{
		get { return "AccountManagementRegistryItemEditor"; }
	}

	protected override AccountCollectionRegistryDataType GetNewDataType()
	{
		return new AccountCollectionRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var collection = new AccountCollection();
		var account = collection.AddNew();
		account.AccountNumber = "12345678901-123";
		account.AccountPassword = "password";
		account.AccountNode = "1234";
		var accountDetail = account.AccountDetails.AddNew();
		accountDetail.InternalCode = "1234-DEC1";
		accountDetail.DeclarantCode = "DEC1";
		accountDetail.AuthorizedUser = "111111-123";

		var collection2 = new AccountCollection();
		var account2 = collection2.AddNew();
		account2.AccountNumber = "09876543210-987";
		account2.AccountPassword = "drowssap";
		account2.AccountNode = "4321";
		var account2Detail = account2.AccountDetails.AddNew();
		account2Detail.InternalCode = "4321-DEC1";
		account2Detail.DeclarantCode = "DEC1";
		account2Detail.AuthorizedUser = "111111-123";

		return new ValidSampleAndBinaryValueInDB[]
		{
			new ValidSampleAndBinaryValueInDB(collection, new AccountCollectionRegistryDataType().Serialise(collection)),
			new ValidSampleAndBinaryValueInDB(collection2, new AccountCollectionRegistryDataType().Serialise(collection2))
		};
	}

	public override void TestGetSetValidValues()
	{
		var factory = new BusinessObjectFactory();
		factory.New<OrgHeader>().OH_Code = "DEC1";
		factory.Save();
		base.TestGetSetValidValues();
	}
}
