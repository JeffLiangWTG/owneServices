using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	public abstract class AccountingDocumentWrapperTestCase : DocumentWrapperTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			CurrentCurrencySymbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
		}
		string CurrentCurrencySymbol;

		protected override void TearDown()
		{
			AssertEquals("Currency symbol has changed during test and not been reset", CurrentCurrencySymbol, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol);
			base.TearDown();
		}

		protected TResult RecreateTestingDocWrapper<T, TResult>(T bizo, Func<T, BusinessObjectFactory, TResult> createWrapper)
			where T : BusinessObject
			where TResult : DocumentWrapper
		{
			Factory.Save();
			bizo.Factory.Save();
			ReleaseFactory();
			return createWrapper(Factory.Load<T>(bizo.PK), Factory);
		}
	}
}
