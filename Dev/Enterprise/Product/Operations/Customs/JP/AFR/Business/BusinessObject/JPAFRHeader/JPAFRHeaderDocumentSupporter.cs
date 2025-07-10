using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderDocumentSupporter : DocumentSupporter
	{
		public JPAFRHeaderDocumentSupporter(JPAFRHeader header)
			: base(header)
		{
		}

		public new JPAFRHeader BusinessObject => (JPAFRHeader)base.BusinessObject;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JPAFRHeader; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.AdvanceFilingRulesCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var provider = ObjectFactory.Get<Integration.Customs.JP.AFR.IHeaderDocumentWrapperProvider>();
			var result = provider.GetDocumentWrapper(BusinessObject) as DocumentWrapper;

			return (result == null) ? null : [result];
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return [DataContext.JPAFRHeader];
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.JPAFRHeader)
			{
				return Res.GetString("1756FE1C-C431-4FA8-A4C0-63121D018528", "AFR Header cannot be found.");
			}

			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}
	}
}
