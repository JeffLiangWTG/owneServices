using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Jordan;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Jordan
{
	[TestedType(typeof(JordanEInvoicingObjectFactory))]
	class JordanEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		public void TestGetPivotStatus_WhenEnableEInvoicingFunctionality()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var pivot = GetPivot();
				AssertEquals("Status should be QUE.", Constants.EInvoicingPivotState.Queued, pivot.AIP_Status);
			}
		}

		public void TestGetPivotStatus_WhenDisableEInvoicingFunctionality()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(false, ZDateTime.Today.AddDays(-10).ToDateTime()))
			{
				var pivot = GetPivot();
				AssertEquals("Status should be DCD.", Constants.EInvoicingPivotState.Discarded, pivot.AIP_Status);
			}
		}

		public void TestGetPivotStatus_WhenEReportingComplianceDateLaterThanToday()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicingJordan(true, ZDateTime.Today.AddDays(10).ToDateTime()))
			{
				AssertNull("Pivot should be null.", GetPivot());
			}
		}

		protected override ZString MessageTypeAssignedInCountryObjectFactory => JordanEInvoiceAPICommandList.Codes.SubmitTransaction;

		AccEInvoicingTransactionPivot GetPivot()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", GlbCompany.CurrentCompany.LocalCurrency, 100m, TestObjectCreator.Debtor, "TXI");
			TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

			Factory.Save();

			return Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
		}

		#region override

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new JordanEInvoicingObjectFactory();

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(JordanEInvoicingAdditionalDataItemsProvider);

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override Type GetExpectedCredentialsLoaderType() => typeof(JordanCredentialLoader);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(EInvoicingDataValidatorForJordan);

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
