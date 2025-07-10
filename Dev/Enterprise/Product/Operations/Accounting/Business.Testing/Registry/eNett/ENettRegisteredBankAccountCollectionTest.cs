using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ENettRegisteredBankAccountCollection))]
	public class ENettRegisteredBankAccountCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ENettRegisteredBankAccountCollection>
	{
		#region Implementation

		protected override ENettRegisteredBankAccountCollection GetCollectionToTest()
		{
			return new ENettRegisteredBankAccountCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ENettRegisteredBankAccount();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ENettRegisteredBankAccountCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion

		public void TestDuplicateRegistryEntry()
		{
			ENettRegisteredBankAccountCollection collection = new ENettRegisteredBankAccountCollection();
			ZGuid testGuid = ZGuid.NewZGuid();

			ENettRegisteredBankAccountCollection eNettRegisteredBankAccountCollection = new ENettRegisteredBankAccountCollection();
			ENettRegisteredBankAccount eNettRegisteredBankAccount = new ENettRegisteredBankAccount();
			eNettRegisteredBankAccount.BankAccountPK = testGuid;
			eNettRegisteredBankAccount.IsDefault = false;
			eNettRegisteredBankAccountCollection.Add(eNettRegisteredBankAccount);
			AssertEquals("Duplicate item not expected", false, eNettRegisteredBankAccountCollection.IsDuplicateItem(eNettRegisteredBankAccount));

			ENettRegisteredBankAccount duplicateENettRegisteredBankAccount = new ENettRegisteredBankAccount();
			duplicateENettRegisteredBankAccount.BankAccountPK = testGuid;
			duplicateENettRegisteredBankAccount.IsDefault = true;
			eNettRegisteredBankAccountCollection.Add(duplicateENettRegisteredBankAccount);
			AssertEquals("Duplicate item expected", true, eNettRegisteredBankAccountCollection.IsDuplicateItem(duplicateENettRegisteredBankAccount));
		}

		public void TestOneAndOnlyOneDefaultItemForEachCurrency()
		{
			ENettRegisteredBankAccountCollection collection = new ENettRegisteredBankAccountCollection();
			BusinessObject cur1Account1 = Factory.New<AccBankAccount>();
			cur1Account1.FillWithValidTestData();
			BusinessObject cur1Account2 = Factory.New<AccBankAccount>();
			cur1Account2.FillWithValidTestData();
			BusinessObject cur2Account1 = Factory.New<AccBankAccount>();
			cur2Account1.FillWithValidTestData();
			BusinessObject cur2Account2 = Factory.New<AccBankAccount>();
			cur2Account2.FillWithValidTestData();
			BusinessObject currency1 = Factory.New<RefCurrency>();
			currency1.FillWithValidTestData();
			BusinessObject currency2 = Factory.New<RefCurrency>();
			currency2.FillWithValidTestData();
			cur1Account1[AccBankAccountSchema.Constants.AB_RX_NKAccountCurrency] = cur1Account2[AccBankAccountSchema.Constants.AB_RX_NKAccountCurrency] = ((IRefCurrency)currency1).RX_Code;
			cur2Account1[AccBankAccountSchema.Constants.AB_RX_NKAccountCurrency] = cur2Account2[AccBankAccountSchema.Constants.AB_RX_NKAccountCurrency] = ((IRefCurrency)currency2).RX_Code;
			Factory.Save();

			ENettRegisteredBankAccount account1 = collection.AddNew();
			account1.BankAccountPK = cur1Account1.PK;
			AssertEquals("The only account for a currency should always be default", true, account1.IsDefault);
			account1.IsDefault = false;
			AssertEquals("The only account for a currency should stay default regardless", true, account1.IsDefault);

			ENettRegisteredBankAccount account2 = collection.AddNew();
			account2.BankAccountPK = cur1Account2.PK;
			AssertEquals("Another account for the same currency should not come default", false, account2.IsDefault);
			account2.IsDefault = true;
			AssertEquals("The first account should become non-default", false, account1.IsDefault);
			AssertEquals("The second account should become default", true, account2.IsDefault);

			ENettRegisteredBankAccount account3 = collection.AddNew();
			account3.BankAccountPK = cur2Account1.PK;
			AssertEquals("The only account for a currency should always be default", true, account3.IsDefault);
			account3.IsDefault = false;
			AssertEquals("The only account for a currency should stay default regardless", true, account3.IsDefault);

			ENettRegisteredBankAccount account4 = collection.AddNew();
			account4.BankAccountPK = cur2Account2.PK;
			AssertEquals("Another account for the same currency should not come default", false, account4.IsDefault);
			account4.IsDefault = true;
			AssertEquals("The first account should become non-default", false, account3.IsDefault);
			AssertEquals("The second account should become default", true, account4.IsDefault);
			AssertEquals("The default account for a different currency should stay default", true, account2.IsDefault);

			collection.Remove(account2);
			AssertEquals("Default account removed, new default should be selected", true, account1.IsDefault);
			AssertNoExceptionThrown("Nothing bad should happen here", () => collection.Remove(account1));
		}
	}
}
