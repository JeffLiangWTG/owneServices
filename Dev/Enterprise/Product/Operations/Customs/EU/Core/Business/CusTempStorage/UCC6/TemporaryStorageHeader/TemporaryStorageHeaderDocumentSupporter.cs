using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderDocumentSupporter : DocumentSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.EuPnts;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.None;

		public TemporaryStorageHeaderDocumentSupporter(TemporaryStorageHeader header)
			: base(header)
		{
			_header = header;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			if (filterName == DocumentFilters.CTYEG)
			{
				return EconomicGroupList.Codes.EuropeanUnion;
			}

			return base.GetFilterValue(filterName);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Core.Constants.DataContext.EuPnts)
			{
				return Res.GetString("F629E218-980B-431B-B804-0E9948A15231", "Temporary Storage Header cannot be found");
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.EuPnts)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapperWithoutException(_header.Configuration.TemporaryStorageHeaderDocumentWrapperClass, BusinessObject) };
			}

			return Array.Empty<DocumentWrapper>();
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.EuPnts };
		}

		readonly TemporaryStorageHeader _header;
	}
}
