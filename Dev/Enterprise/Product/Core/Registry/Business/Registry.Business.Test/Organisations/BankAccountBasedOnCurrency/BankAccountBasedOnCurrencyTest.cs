using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BankAccountBasedOnCurrency))]
	sealed class BankAccountBasedOnCurrencyTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateCurrency()
		{
			BankAccountBasedOnCurrencyCollection collection = new BankAccountBasedOnCurrencyCollection();
			collection.AddNew();
			AssertNoErrors("Precondition: Language should not have errors.", collection[0].CurrencyInfo);

			collection[0].Currency = "!@#";
			AssertHasError(collection[0].CurrencyInfo, "Enter a valid selection.");

			collection[0].Currency = "USD";
			AssertNoErrors(collection[0].CurrencyInfo);

			collection[0].Currency = "";
			AssertHasError(collection[0].CurrencyInfo, "Please enter a value.");

			collection[0].Currency = "USD";
			collection.AddNew();
			collection[1].Currency = "USD";
			AssertHasError(collection[1].CurrencyInfo, "There must be only one line for each currency.");

			collection[1].Currency = "AUD";
			AssertNoErrors(collection[1].CurrencyInfo);
		}

		public void TestValidateBankAccount()
		{
			AssertNoErrors("Precondition: AccountsOrderBeginsWith should not have errors.", BizObj.BankAccountInfo);

			BizObj.BankAccount = ZGuid.NewZGuid();
			AssertHasError(BizObj.BankAccountInfo, "Enter a valid selection.");

			BizObj.BankAccount = ZGuid.Empty;
			AssertHasError(BizObj.BankAccountInfo, "Please enter a value.");

			BusinessObject account = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccBankAccount)));
			Factory.Save();
			BizObj.BankAccount = (ZGuid)account["PK"];
			AssertNoErrors(BizObj.BankAccountInfo);
		}

		public void TestRunPreSaveValidationCore()
		{
			BizObj.Currency = "!@#";
			BizObj.BankAccount = ZGuid.NewZGuid();

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.CurrencyInfo);
			AssertHasErrors(BizObj.BankAccountInfo);
		}

		public void TestCurrencyList()
		{
			IActiveBusinessObjectCollection fCurrencyList = (IActiveBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory });
			AssertEquals("Count", fCurrencyList.Count, BizObj.CurrencyList.Count);
		}

		public void TestCurrencyList_ShouldBeThreadSafe()
		{
			BizObj.Currency = "AUD";
			var currencyDescriptionFromCurrentThread = BizObj.CurrencyDescription;
			var threadID = BizObj.CurrencyList.Factory.ThreadSentry.OwnerThread.ThreadID;
			var currentThread = Thread.CurrentThread.ManagedThreadId;

			AssertNotNullOrEmpty(currencyDescriptionFromCurrentThread);
			AssertEquals(currentThread, threadID);

			ErrorReporter.Clear();

			var otherThread = new Thread(new ThreadStart(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var currencyDescriptionFromOtherThread = BizObj.CurrencyDescription;
					AssertEquals(currencyDescriptionFromCurrentThread, currencyDescriptionFromOtherThread);
				}
			}));

			otherThread.Start();
			otherThread.Join();

			var lastError = ErrorReporter.LastExceptionReported;

			AssertNull(lastError);
		}

		public void TestBankAccountList()
		{
			BusinessObject account = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IAccBankAccount)));
			account["AB_GC"] = Environment.Env.CurrentCompany.PK;
			Factory.Save();
			object[] parameters = new object[] { Factory };
			BusinessObjectCollection fBankAccountList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccBankAccountCollection>(), parameters);
			fBankAccountList.Load();
			AssertEquals("Count", fBankAccountList.Count, BizObj.BankAccountList.Count);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BankAccountBasedOnCurrency result = new BankAccountBasedOnCurrency(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			result.Currency = "AUD";
			result.BankAccount = ZGuid.Empty;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		new BankAccountBasedOnCurrency BizObj
		{
			get { return (BankAccountBasedOnCurrency)base.BizObj; }
		}

		#endregion
	}
}
