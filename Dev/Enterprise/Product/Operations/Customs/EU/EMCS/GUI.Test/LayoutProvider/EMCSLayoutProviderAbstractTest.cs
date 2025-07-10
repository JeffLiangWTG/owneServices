using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestsSubclassesOf(typeof(IEMCSLayoutProvider))]
	public abstract class EMCSLayoutProviderAbstractTest<T> : TestCaseWithFactory
		where T : IEMCSLayoutProvider, new()
	{
		public void TestInvoiceLineDetailsPanelLayoutWithGrid()
		{
			AssertEquals(ExpectedInvoiceLineDetailsPanelLayoutWithGridType, provider.GetInvoiceLineDetailsPanelLayoutWithGrid(CountryOrGroupingCode).GetType());
		}

		public void TestDeclarationOrganizationsPanelLayout()
		{
			AssertEquals(ExpectedDeclarationOrganizationsPanelLayoutType, provider.DeclarationOrganizationsPanelLayout.GetType());
		}

		protected abstract Type ExpectedInvoiceLineDetailsPanelLayoutWithGridType { get; }

		protected abstract Type ExpectedDeclarationOrganizationsPanelLayoutType { get; }

		public void TestEMCSLayoutProviderType()
		{
			AssertType<T>(provider);
		}

		protected abstract string CountryOrGroupingCode { get; }

		protected override void SetUp()
		{
			base.SetUp();
			provider = EMCSLayoutProvider.GetLayoutProvider(CountryOrGroupingCode);
		}
		IEMCSLayoutProvider provider;
	}
}
