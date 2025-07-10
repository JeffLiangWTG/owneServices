using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderDocumentSupporter : DocumentSupporter
	{
		public CusTempStorageJobHeaderDocumentSupporter(CusTempStorageJobHeader parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		CusTempStorageJobHeader JobHeader => (CusTempStorageJobHeader)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.TempStorageHeader;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.AdvanceFilingRulesCustomiseDocuments;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => System.Array.Empty<DocumentWrapper>();

		protected override DataContext[] GetSupportedDataContexts() => new DataContext[] { DataContext.TempStorageHeader };

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.BKRCTY:
					return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(JobHeader.CountryCode);
				default:
					return base.GetFilterValue(filterName);
			}
		}
	}
}
