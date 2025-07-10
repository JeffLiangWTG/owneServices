using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ReportingBookCollectionProvider))]
	class ReportingBookCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccReportingBookCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccReportingBook;
	}

	[TestedType(typeof(ReportingBookWithLocalCurrencyCollectionProvider))]
	class ReportingBookWithLocalCurrencyCollectionProviderTest : ReportingBookCollectionProviderTest
	{
		public void TestValidationAndDefaultAdded()
		{
			var filterField = new LookupField(new BusinessObjectFactory());
			filterField.SetCollectionProvider(Provider);

			Provider.AddValidationAndDefault(filterField, null);
			AssertEquals("Conditional validator has been added", 1, filterField.Validators.Count);
			AssertEquals("Conditional validator has expected type", typeof(ReportingBookWithLocalCurrencyLookupTypeValidator), filterField.Validators[0].GetType());
		}

		public void TestValidationForReportingBookCurrency()
		{
			var factory = new BusinessObjectFactory();
			var reportingBookWithNoCurrency = factory.NewWithValidTestData<AccReportingBook>();
			reportingBookWithNoCurrency.ARB_RX_NKCurrency = string.Empty;

			var reportingBookWithLocalCurrency = factory.NewWithValidTestData<AccReportingBook>();
			reportingBookWithLocalCurrency.ARB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var reportingBookWithForeignCurrency = factory.NewWithValidTestData<AccReportingBook>();
			reportingBookWithForeignCurrency.ARB_RX_NKCurrency = "USD";

			var filterField = new LookupField(factory);
			filterField.SetCollectionProvider(Provider);

			Provider.AddValidationAndDefault(filterField, null);

			filterField.Value = reportingBookWithNoCurrency.PK.ToGuid();
			AssertEquals(string.Empty, filterField.ValidationError);

			filterField.Value = reportingBookWithLocalCurrency.PK.ToGuid();
			AssertEquals(string.Empty, filterField.ValidationError);

			filterField.Value = reportingBookWithForeignCurrency.PK.ToGuid();
			AssertEquals("This report can only be generated for Reporting Books in Local Reporting Currency only.", filterField.ValidationError);
		}
	}
}
