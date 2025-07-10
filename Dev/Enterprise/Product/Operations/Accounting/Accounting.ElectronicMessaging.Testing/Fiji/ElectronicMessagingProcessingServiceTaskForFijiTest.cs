using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForFiji))]
	public class ElectronicMessagingProcessingServiceTaskForFijiTest : TaxCoreElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForFiji>
	{
		protected override ZString CountryCode => CountryCodes.Fiji;

		protected override ElectronicMessagingProcessingServiceTaskForFiji GetCountrySpecificServiceTask() => new TaxCoreElectronicMessagingProcessingServiceTask_ForTest();

		protected override (bool value, string reason) ExpectedServiceTaskIsMandatory => (false, "Cannot be mandatory if need to delete FJ company, and if has any FJ company, process controller will run it");

		protected override void AssertLogTextWhenThereIsNoTransactionBatch(string log)
		{
			AssertContains("Information|Fiji E-Reporting Invoice Processing service task started.", log);
			AssertContains("Information|There is no Electronic Invoice to send.", log);
			AssertContains("Information|Fiji E-Reporting Invoice Processing service task completed.", log);
		}

		#region Inner Class

		public class TaxCoreElectronicMessagingProcessingServiceTask_ForTest : ElectronicMessagingProcessingServiceTaskForFiji
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch(company, CountryCode, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}

		#endregion
	}
}
