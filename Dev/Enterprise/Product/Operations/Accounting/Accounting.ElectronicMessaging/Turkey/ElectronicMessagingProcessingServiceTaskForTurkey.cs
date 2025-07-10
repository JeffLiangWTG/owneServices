using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Turkey;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	ElectronicMessagingProcessingServiceTaskForTurkey.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Turkey,
	},
	null)
]

[assembly: HostedService(
	ElectronicMessagingProcessingServiceTaskForTurkey.Code,
	"E-Reporting Invoice Processing Service Task For Turkey",
	"ACC",
	typeof(ElectronicMessagingProcessingServiceTaskForTurkey),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Turkey,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "1hours",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	#region SuppressResourceStringsCheckRegion

	// Murray Note: new countries should inherit from GlobalElectronicMessagingProcessingServiceTask rather than ElectronicMessagingProcessingServiceTask.
	// Please talk to Murray if you think it is a good idea to inherit from ElectronicMessagingProcessingServiceTask for your new country.
	public class ElectronicMessagingProcessingServiceTaskForTurkey : ElectronicMessagingProcessingServiceTask
	{
		public const string Code = "ETR";
		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			var countryFactory = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);
			return new EDIInterchangeCreatorForTurkey(company, countryFactory);
		}

		public ElectronicMessagingProcessingServiceTaskForTurkey()
			: base(dataProvider: new ElectronicMessagingProcessingServiceTaskDataProviderForTurkey())
		{
		}

		public override string CountryCode => CountryCodes.Turkey;

		public override ZString MessageName => "Electronic Invoice";

		public override ZString TaskName => "Turkey E-Invoice Processing";

		protected override EInvoicingBatchCreatorBase GetEInvoiceBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForTurkey(company);
		}

		protected override BaseEInvoicingDataValidator GetEInvoicingDataValidator(GlbCompany company)
		{
			return new EInvoicingDataValidatorForTurkey(company);
		}
	}

	internal class ElectronicMessagingProcessingServiceTaskDataProviderForTurkey : ElectronicMessagingProcessingServiceTaskDataProvider
	{
		public override IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithPeriodicRequests(string countryCode)
		{
			var result = new HashSet<ZGuid>();

			var sqlQuery = $@"SELECT DISTINCT {AccEInvoicingTransactionPivotSchema.Constants.AIP_GC}
FROM	{AccEInvoicingTransactionPivotSchema.Constants.SqlSchemaName}.{AccEInvoicingTransactionPivotSchema.Constants.TableName} 
WHERE	{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode} = @CountryCode
AND	(
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = '{EInvoicingPivotState.Queued}'
	OR (
		{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = '{EInvoicingPivotState.Sent}'
		AND {AccEInvoicingTransactionPivotSchema.Constants.AIP_ActionType} = '{EInvoicingPivotActionType.StatusCheck}'
	)
)";     // Inline SQL is not translatable.

			using (var command = Db.Connection.Command(sqlQuery))   // We are not using a factory here for performance.
			{
				command.AddParameter("@CountryCode", SqlDbType.Char, countryCode);

				result.UnionWith(DataUtils.GetListOfValuesFromCommand(command).Select(x => new ZGuid(x)));
			}

			var apTransactionListRequestBatchId = GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode)?.ApTransactionListRequestBatchId;

			if (!string.IsNullOrEmpty(apTransactionListRequestBatchId))
			{
				var enabledCompanyPKs = GetPKsOfCompaniesThatEnabledEInvoicing(countryCode);
				var timesToCreatePILBatch = new Dictionary<ZGuid, ZDateTime>();
				foreach (var companyPk in enabledCompanyPKs)
				{
					timesToCreatePILBatch.Add(companyPk, getLatestTimeToCreatePILBatch(companyPk));
				}
				var apBatchQuery = new ZQuery(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber, apTransactionListRequestBatchId);
				apBatchQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, enabledCompanyPKs);
				apBatchQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, timesToCreatePILBatch.Values.Min());
				var activeAPBatches = new BusinessObjectFactory().Load<AccEInvoicingBatch>(apBatchQuery);

				var companiesWithActiveBatchePKs = activeAPBatches.Where(x => x.AIB_SystemLastEditTimeUtc >= timesToCreatePILBatch[x.AIB_GC]).Select(x => x.AIB_GC);
				var enabledCompaniesWithoutActiveBatchePKs = enabledCompanyPKs.Except(companiesWithActiveBatchePKs);

				result.UnionWith(enabledCompaniesWithoutActiveBatchePKs);
			}

			ZDateTime getLatestTimeToCreatePILBatch(ZGuid companyPk) =>
				ZDateTime.UtcNow.AddMinutes(-AccountingMasterFilesRegistry.Instance.APListAutomatedRequestSchedule.GetValueWithoutFallback(companyPk.ToGuid(), Guid.Empty, Guid.Empty));

			return result; //returning empty list if no companies need to send AP List requests
		}
	}

	#endregion
}
