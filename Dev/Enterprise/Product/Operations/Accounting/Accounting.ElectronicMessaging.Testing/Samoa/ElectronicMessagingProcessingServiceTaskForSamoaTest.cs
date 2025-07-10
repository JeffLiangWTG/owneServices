using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForSamoa))]
	public class ElectronicMessagingProcessingServiceTaskForSamoaTest : TaxCoreElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForSamoa>
	{
		protected override ZString CountryCode => CountryCodes.WesternSamoa;

		protected override ElectronicMessagingProcessingServiceTaskForSamoa GetCountrySpecificServiceTask() => new SamoaElectronicMessagingProcessingServiceTask_ForTest();

		protected override void AssertLogTextWhenThereIsNoTransactionBatch(string log)
		{
			AssertContains("Information|Samoa E-Reporting Invoice Processing service task started.", log);
			AssertContains("Information|There is no Electronic Invoice to send.", log);
			AssertContains("Information|Samoa E-Reporting Invoice Processing service task completed.", log);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						null,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.WesternSamoa),
				};
			}
		}

		#region Inner Class

		public class SamoaElectronicMessagingProcessingServiceTask_ForTest : ElectronicMessagingProcessingServiceTaskForSamoa
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch(company, CountryCode, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}

		#endregion
	}
}
