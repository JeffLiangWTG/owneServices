using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	[TestedType(typeof(TurkeyEInvoicingObjectFactory))]
	class TurkeyEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();

		protected override string ExpectedApTransactionListRequestBatchId => "TR:AP:GetInboxInvoiceListBatch";

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(populateShipments: true);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(TurkeyGlobalXUEFunctionalityProvider);

		public override void TestApTransactionListRequestBatchId()
		{
			var countryFactory = GetCountryFactory();
			AssertNull(countryFactory.ApTransactionListRequestBatchId);

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			{
				AssertEquals(ExpectedApTransactionListRequestBatchId, countryFactory.ApTransactionListRequestBatchId);
			}
		}
	}
}
