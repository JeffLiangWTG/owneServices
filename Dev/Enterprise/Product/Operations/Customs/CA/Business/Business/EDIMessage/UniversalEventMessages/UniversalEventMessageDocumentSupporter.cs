using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business
{
	public class UniversalEventMessageDocumentSupporter : DocumentSupporter
	{
		public UniversalEventMessageDocumentSupporter(UniversalEventMessage universalEventMessage) : base(universalEventMessage)
		{
		}

		protected UniversalEventMessage UniversalEventMessage => (UniversalEventMessage)BusinessObject;

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (UniversalEventMessage.EM_MessageSubType)
			{
				case UniversalEventMessageTypes.Codes.D4Notices:
					return new[] { BODocDataProvider.Get(new D4NoticeDocumentWrapper(UniversalEventMessage, YesNoList.Codes.Yes)) };
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext,
			IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(UniversalEventReport));
			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts() => System.Array.Empty<Constants.DataContext>();

		public override BusinessContext BusinessContext => BusinessContext.CAUniversalEvent;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CAUniversalEventReportsCustomiseDocuments;

		public const string UniversalEventReport = ".UniversalEventReport";

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
