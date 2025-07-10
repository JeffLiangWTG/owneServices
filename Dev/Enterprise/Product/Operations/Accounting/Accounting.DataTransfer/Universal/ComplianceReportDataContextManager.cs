using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class ComplianceReportDataContextManager : EventDataContextManager<AccComplianceReport>, ITransactionBatchDataContextManager
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.ComplianceReport; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.UniqueReportID; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery { IsNoResultQuery = true };
		}

		public bool ManagesTransactionBatches
		{
			get { return true; }
		}

		public ITopLevelDataObjectWriter GetTransactionBatchDataObjectWriter(IDataWritingManager writeManager)
		{
			return new ComplianceReportWriter(writeManager);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ComplianceReportEventParentFinder(factory, this, logger);
		}

		class ComplianceReportEventParentFinder : EventParentFinder
		{
			internal ComplianceReportEventParentFinder(BusinessObjectFactory factory, ComplianceReportDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				AccComplianceReport[] result = null;

				if (logger.TopLevelDataContext != null && logger.TopLevelDataContext.DataTargetCollection != null && logger.TopLevelDataContext.DataTargetCollection.Any())
				{
					var uniqueID = logger.TopLevelDataContext.DataTargetCollection.FirstOrDefault(x => x.Type.HasValue && x.Type.Value == nameof(DataContextType.ComplianceReport));
					if (uniqueID != null && uniqueID.Key.HasValue)
					{
						var keys = uniqueID.Key.Value.Split('-');
						if (keys.Length == 6)
						{
							var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, keys[0]);
							var branch = keys[1].IsEmpty ? null : factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, keys[1]);

							var filter = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, company.PK);
							if (branch != null)
							{
								filter.AddToFilter(AccComplianceReportSchema.ACR_GB_Branch, branch.PK);
							}
							filter.AddToFilter(AccComplianceReportSchema.ACR_ReportType, keys[3]);

							ZDateTime parsedDate;
							if (ZDateTime.TryParseExact(keys[4], out parsedDate, "yyyyMMdd"))
							{
								filter.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, parsedDate);
							}
							if (ZDateTime.TryParseExact(keys[5], out parsedDate, "yyyyMMdd"))
							{
								filter.AddToFilter(AccComplianceReportSchema.ACR_DateTo, parsedDate);
							}

							result = factory.Load<AccComplianceReport>(filter);
						}
					}
				}

				return result;
			}
		}

		public bool UseIncomingTransactionBatchData(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return false;
		}

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject, IXmlSessionTracker logger, IUniversalObjectFactory factory)
		{
			return KeysResult.NoMatch();
		}
	}
}