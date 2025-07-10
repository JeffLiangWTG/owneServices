using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupDocumentSupporter : DocumentSupporter
	{
		public IncidentManagementGroupDocumentSupporter(IncidentManagementGroup professionalServicesQuote)
			: base(professionalServicesQuote)
		{
		}

		protected IncidentManagementGroup IncidentManagementGroup
		{
			get { return (IncidentManagementGroup)BusinessObject; }
		}

		public override BusinessContext BusinessContext => BusinessContext.IncidentGroup;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.CustomerServiceIncidentManagementGroup; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, IncidentManagementGroup);
			}
			else
			{
				return [null];
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return [Core.Constants.DataContext.GenericFreightJob];
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			return GetSupportedBODataSourcesFor(typeof(IncidentManagementGroup));
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return [BODocDataProvider.Get(BusinessObject)];
		}
	}
}
