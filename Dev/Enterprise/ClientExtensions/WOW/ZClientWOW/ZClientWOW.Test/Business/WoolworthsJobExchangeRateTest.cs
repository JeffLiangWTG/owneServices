using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsJobExchangeRate))]
	public class WoolworthsJobExchangeRateTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		[ExpectNoExceptions]
		public void TestGetExchangeRateWithNoJob()
		{
			TestWoolworthsJobExchangeRate exRate = Factory.New<TestWoolworthsJobExchangeRate>();
			ZDecimal value = exRate.GetExchangeRate(fCurrency.RX_Code);
		}

		[ExpectNoExceptions]
		public void TestGetExchangeRateWithNoDeclaration()
		{
			TestWoolworthsJobExchangeRate exRate = Factory.New<TestWoolworthsJobExchangeRate>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			ZDecimal value = exRate.GetExchangeRate(fCurrency.RX_Code);
		}

		[ExpectNoExceptions]
		public void TestGetExchangeRateWithNoDate()
		{
			TestWoolworthsJobExchangeRate exRate = Factory.New<TestWoolworthsJobExchangeRate>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			ZDecimal value = exRate.GetExchangeRate(fCurrency.RX_Code);
		}

		[ExpectNoExceptions]
		public void TestGetExchangeRate()
		{
			TestWoolworthsJobExchangeRate exRate = Factory.New<TestWoolworthsJobExchangeRate>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			declaration.JE_DateOfArrival = ZDateTime.Now;
			ZDecimal value = exRate.GetExchangeRate(fCurrency.RX_Code);
		}

		#region Implementation
		protected RefCurrency fCurrency;
		protected override void SetUp()
		{
			base.SetUp();
			fCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(Accounting.Business.JobInvoicing.ExchangeRate));
		}

		protected class TestWoolworthsJobExchangeRate : WoolworthsJobExchangeRate
		{
			public TestWoolworthsJobExchangeRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZDecimal GetExchangeRate(ZString currencyNK)
			{
				return base.GetExchangeRate(currencyNK);
			}
		}
		#endregion
	}
}
