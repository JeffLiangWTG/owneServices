using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(IImportInvoiceLineValidationDecider))]
	public abstract class ImportInvoiceLineValidationDeciderTest<T> : TestCaseWithFactory
		where T : class, IImportInvoiceLineValidationDecider
	{
		public void TestIsRuleC0002Active()
		{
			AssertEquals(ExpectedIsRuleC0002Active, decider.IsRuleC0002Active);
		}
		protected abstract bool ExpectedIsRuleC0002Active { get; }

		public void TestIsRuleC0820ActiveForJI_SupplementaryCode1()
		{
			AssertEquals(ExpectedIsRuleC0820ActiveForJI_SupplementaryCode1, decider.IsRuleC0820ActiveForJI_SupplementaryCode1);
		}
		protected abstract bool ExpectedIsRuleC0820ActiveForJI_SupplementaryCode1 { get; }

		public void TestIsRuleC0820ActiveForJI_Tariff()
		{
			AssertEquals(ExpectedIsRuleC0820ActiveForJI_Tariff, decider.IsRuleC0820ActiveForJI_Tariff);
		}
		protected abstract bool ExpectedIsRuleC0820ActiveForJI_Tariff { get; }

		public void TestIsRuleCD0111Active()
		{
			AssertEquals(ExpectedIsRuleCD0111Active, decider.IsRuleCD0111Active);
		}
		protected abstract bool ExpectedIsRuleCD0111Active { get; }

		public void TestIsRuleCD5151ActiveForZG_CountryOfSupply() => AssertEquals("IsRuleCD5151ActiveForZG_CountryOfSupply", ExpectedIsRuleCD5151ActiveForZG_CountryOfSupply, decider.IsRuleCD5151ActiveForZG_CountryOfSupply);
		protected abstract bool ExpectedIsRuleCD5151ActiveForZG_CountryOfSupply { get; }

		public void TestIsRuleCD5161ActiveForJI_CountryOfOrigin() => AssertEquals("IsRuleCD5161ActiveForJI_CountryOfOrigin", ExpectedIsRuleCD5161ActiveForJI_CountryOfOrigin, decider.IsRuleCD5161ActiveForJI_CountryOfOrigin);
		protected abstract bool ExpectedIsRuleCD5161ActiveForJI_CountryOfOrigin { get; }

		public void TestIsRuleCD9102Active()
		{
			AssertEquals(ExpectedIsRuleCD9102Active, decider.IsRuleCD9102Active);
		}
		protected abstract bool ExpectedIsRuleCD9102Active { get; }

		public void TestIsRuleC0627Active()
		{
			AssertEquals(ExpectedIsRuleC0627Active, decider.IsRuleC0627Active);
		}
		protected abstract bool ExpectedIsRuleC0627Active { get; }

		public void TestIsRuleC0710ActiveForJI_CountryOfOrigin()
		{
			AssertEquals(ExpectedIsRuleC0710ActiveForJI_CountryOfOrigin, decider.IsRuleC0710ActiveForJI_CountryOfOrigin);
		}
		protected abstract bool ExpectedIsRuleC0710ActiveForJI_CountryOfOrigin { get; }

		public void TestIsRuleC0710ActiveForJI_LinePrice()
		{
			AssertEquals(ExpectedIsRuleC0710ActiveForJI_LinePrice, decider.IsRuleC0710ActiveForJI_LinePrice);
		}
		protected abstract bool ExpectedIsRuleC0710ActiveForJI_LinePrice { get; }

		public void TestIsRuleC0710ActiveForForZG_CountryOfSupply()
		{
			AssertEquals(ExpectedIsRuleC0710ActiveForZG_CountryOfSupply, decider.IsRuleC0710ActiveForZG_CountryOfSupply);
		}
		protected abstract bool ExpectedIsRuleC0710ActiveForZG_CountryOfSupply { get; }

		public void TestIsRuleC0919Active()
		{
			AssertEquals(ExpectedIsRuleC0919Active, decider.IsRuleC0919Active);
		}
		protected abstract bool ExpectedIsRuleC0919Active { get; }

		public void TestIsRuleR0012Active()
		{
			AssertEquals(ExpectedIsRuleR0012Active, decider.IsRuleR0012Active);
		}
		protected abstract bool ExpectedIsRuleR0012Active { get; }

		public void TestIsRuleC0624Active()
		{
			AssertEquals(ExpectedIsRuleC0624Active, decider.IsRuleC0624Active);
		}
		protected abstract bool ExpectedIsRuleC0624Active { get; }

		public void TestIsRuleC0936Active()
		{
			AssertEquals(ExpectedIsRuleC0936Active, decider.IsRuleC0936Active);
		}
		protected abstract bool ExpectedIsRuleC0936Active { get; }

		public void TestIsRuleC0728()
		{
			AssertEquals(ExpectedIsRuleC0728Active, decider.IsRuleC0728Active);
		}
		protected abstract bool ExpectedIsRuleC0728Active { get; }

		public void TestAllowMultipleRequestedProcedure()
		{
			AssertEquals(ExpectedAllowMultipleRequestedProcedure, decider.AllowMultipleRequestedProcedure);
		}
		protected abstract bool ExpectedAllowMultipleRequestedProcedure { get; }

		public void TestIsRuleR0222()
		{
			AssertEquals(ExpectedIsRuleR0222Active, decider.IsRuleR0222Active);
		}
		protected abstract bool ExpectedIsRuleR0222Active { get; }

		public void TestIsRuleR0223()
		{
			AssertEquals(ExpectedIsRuleR0223Active, decider.IsRuleR0223Active);
		}
		protected abstract bool ExpectedIsRuleR0223Active { get; }

		public void TestIsRuleR0224()
		{
			AssertEquals(ExpectedIsRuleR0224Active, decider.IsRuleR0224Active);
		}
		protected abstract bool ExpectedIsRuleR0224Active { get; }

		protected override void SetUp()
		{
			base.SetUp();
			decider = GetNewValidationDecider();
		}

		protected virtual T GetNewValidationDecider()
		{
			return Activator.CreateInstance<T>();
		}

		protected T decider;
	}
}
