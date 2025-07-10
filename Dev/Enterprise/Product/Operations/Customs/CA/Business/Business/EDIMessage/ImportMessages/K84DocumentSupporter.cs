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
	public class K84DocumentSupporter : DocumentSupporter
	{
		public K84DocumentSupporter(K84Message k84Message)
			: base(k84Message)
		{
		}

		protected K84Message K84DataMessage
		{
			get { return (K84Message)BusinessObject; }
		}

		#region Overrides

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(K84Report));
			return result;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return System.Array.Empty<Core.Constants.DataContext>();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.K84Message; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CAK84ReportsCustomiseDocuments; }
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (K84DataMessage.EM_MessageSubType)
			{
				case K84ReportTypes.Codes.Daily:
					return new[] { BODocDataProvider.Get(new K84DailyReportDocumentWrapper(K84DataMessage)) };
				case K84ReportTypes.Codes.Monthly:
					return new[] { BODocDataProvider.Get(new K84MonthlyReportDocumentWrapper(K84DataMessage)) };
				case K84ReportTypes.Codes.Overdue:
					return new[] { BODocDataProvider.Get(new OverdueReleaseNoticeDocumentWrapper(K84DataMessage)) };
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
		}

		const string K84Report = ".K84Report";

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

	}
}
