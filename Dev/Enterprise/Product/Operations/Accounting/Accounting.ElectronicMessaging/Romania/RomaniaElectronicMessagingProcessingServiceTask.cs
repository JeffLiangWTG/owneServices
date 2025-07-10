using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Romania;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedServiceBusinessObjectBinding(
	RomaniaElectronicMessagingProcessingServiceTask.Code,
	AccEInvoicingTransactionPivotSchema.Constants.TableName,
	new[]
	{
		AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + "=" + EInvoicingPivotState.Queued,
		AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCodes.Romania,
	},
	null)
]

[assembly: HostedService(
	RomaniaElectronicMessagingProcessingServiceTask.Code,
	"E-Reporting Invoice Processing Service Task For Romania",
	"ACC",
	typeof(RomaniaElectronicMessagingProcessingServiceTask),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = CountryCodes.Romania,
	IsMandatory = true,
	IsScheduleReadOnly = false,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "12hours",
	DefaultScheduleRunEvery = "6hours",
	ActiveByDefault = true)
]
namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("RomaniaElectronicMessagingProcessingServiceTask is under implementation.")]
	public class RomaniaElectronicMessagingProcessingServiceTask : GlobalElectronicMessagingProcessingServiceTask
	{
		public RomaniaElectronicMessagingProcessingServiceTask()
			: base(dataProvider: new RomaniaElectronicMessagingProcessingServiceTaskDataProvider())
		{
		}

		public const string Code = "ERO";
		public override string CountryCode => CountryCodes.Romania;

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
		{
			var suffix = AccountingElectronicMessagingRegistry.Instance.eInvoicingServicePointSuffix.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return new RomaniaEDIInterchangeCreator(company, CountryFactory, suffix);
		}
	}

	internal class RomaniaElectronicMessagingProcessingServiceTaskDataProvider : ElectronicMessagingProcessingServiceTaskDataProvider
	{
		public override IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithPeriodicRequests(string countryCode)
		{
			var activeCompanies = GlbCompany.GetActiveCompanies(countryCode).Select(x => x.PK);

			if (activeCompanies.Any())
			{
				var factory = new BusinessObjectFactory();
				var pivotQuery = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot));
				pivotQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Delivered);

				var batchQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);
				batchQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_Status, EInvoicingBatchState.Ready);
				batchQuery.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, activeCompanies);

				pivotQuery.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, batchQuery, JoinCondition.And);

				var pivots = factory.Load<AccEInvoicingTransactionPivot>(pivotQuery);

				var pivotWithEmptyBatchQuery = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot));
				pivotWithEmptyBatchQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Delivered);
				pivotWithEmptyBatchQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_AIB, null);
				pivotWithEmptyBatchQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, activeCompanies);
				var pivotsWithEmptyBatch = factory.Load<AccEInvoicingTransactionPivot>(pivotWithEmptyBatchQuery);

				return pivots.Union(pivotsWithEmptyBatch).Select(x => x.AIP_GC).ToHashSet();
			}

			return Array.Empty<ZGuid>();
		}
	}
}
