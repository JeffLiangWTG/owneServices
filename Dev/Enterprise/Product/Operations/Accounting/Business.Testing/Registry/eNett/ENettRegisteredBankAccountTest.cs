using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ENettRegisteredBankAccount))]
	public class ENettRegisteredBankAccountTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateChargeCodePK()
		{
			BusinessObject bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			BizObj.RunPreSaveValidation();
			Assert("BankAccountPK should have errors", BizObj.BankAccountPKInfo.HasErrors());

			BizObj.BankAccountPK = ZGuid.Empty;
			Assert("BankAccountPK should have errors", BizObj.BankAccountPKInfo.HasErrors());

			BizObj.BankAccountPK = bankAccount.PK;
			Assert("BankAccountPK should not have any errors", !BizObj.BankAccountPKInfo.HasErrors());
		}

		[ExpectException(typeof(RegistryValidationException))]
		public void TestDuplicateRegistryEntry()
		{
			ENettRegisteredBankAccountCollection collection = new ENettRegisteredBankAccountCollection();
			ZGuid testGuid = ZGuid.NewZGuid();

			ENettRegisteredBankAccount eNettRegisteredBankAccount = collection.AddNew();
			eNettRegisteredBankAccount.BankAccountPK = testGuid;
			eNettRegisteredBankAccount.IsDefault = false;

			AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			ENettRegisteredBankAccountCollection value = AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value;
			Assert("Validation should pass", !eNettRegisteredBankAccount.HasErrors);

			ENettRegisteredBankAccount duplicateRegistry = collection.AddNew();
			duplicateRegistry.BankAccountPK = testGuid;
			duplicateRegistry.IsDefault = false;

			AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public void TestOneAndOnlyOneDefaultItem()
		{
			ENettRegisteredBankAccountCollection collection = new ENettRegisteredBankAccountCollection();

			ENettRegisteredBankAccount account1 = collection.AddNew();
			account1.BankAccountPK = ZGuid.NewZGuid();
			AssertEquals("The only account in the collection should always be default", true, account1.IsDefault);
			account1.IsDefault = false;
			AssertEquals("The only account in the collection should be default regardless", true, account1.IsDefault);

			ENettRegisteredBankAccount account2 = collection.AddNew();
			account2.BankAccountPK = ZGuid.NewZGuid();
			AssertEquals("The first account should remain default", true, account1.IsDefault);
			AssertEquals("The newly added account should not be default", false, account2.IsDefault);
			account2.IsDefault = true;
			AssertEquals("The first account should become non-default", false, account1.IsDefault);
			AssertEquals("The second account should become default", true, account2.IsDefault);
			account2.IsDefault = false;
			AssertEquals("The first account should stay non-default", false, account1.IsDefault);
			AssertEquals("The second account should stay default", true, account2.IsDefault);
		}

		#region Test ZPropertyInfos

		public void TestNewZPropertyInfos()
		{
			TestZPropertyInfo(BizObj.BankAccountPKInfo, ENettRegisteredBankAccount.Schema.BankAccountPK);
			TestZPropertyInfo(BizObj.IsDefaultInfo, ENettRegisteredBankAccount.Schema.IsDefault);
		}

		void TestZPropertyInfo(ZPropertyInfo propertyInfo, string expectedName)
		{
			AssertNotNull("ZPropertyInfo for " + propertyInfo.Name + " was null", propertyInfo);
			AssertEquals("PropertyInfo.Name", expectedName, propertyInfo.Name);
		}

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BusinessObject bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			BizObj.BankAccountPK = bankAccount.PK;
			BizObj.IsDefault = false;

			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ENettRegisteredBankAccount BizObj
		{
			get { return (ENettRegisteredBankAccount)base.BizObj; }
		}

		#endregion
	}
}
