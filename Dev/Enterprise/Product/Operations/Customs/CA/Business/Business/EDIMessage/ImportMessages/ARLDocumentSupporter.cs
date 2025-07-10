using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class ARLDocumentSupporter : DocumentSupporter
	{
		public ARLDocumentSupporter(ARLMessage aRLMessage)
			: base(aRLMessage)
		{
		}

		protected ARLMessage ARLDataMessage
		{
			get { return (ARLMessage)BusinessObject; }
		}

		#region Overrides

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(ARLReport));
			return result;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return System.Array.Empty<Core.Constants.DataContext>();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CAARLMessage; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CAK84ReportsCustomiseDocuments; }
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (ARLDataMessage.EM_MessageSubType)
			{
				case ARLMessageTypes.Codes.DailyNotice:
					return new[] { BODocDataProvider.Get(new ARLDailyNoticeDocumentWrapper(ARLDataMessage)) };
				case ARLMessageTypes.Codes.StatementOfAccount:
					return new[] { BODocDataProvider.Get(new ARLStatementOfAccountDocumentWrapper(ARLDataMessage)) };
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		const string ARLReport = ".ARLReport";

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

	}
}
