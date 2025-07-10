using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeaderDocumentSupporter : DocumentSupporter
	{
		public CusMiscRequestHeaderDocumentSupporter(CusMiscRequestHeader cusMiscRequest)
			: base(cusMiscRequest)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.CusMiscRequestHeader;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.MiscRequestCustomiseDocuments;

		public CusMiscRequestHeader CusMiscRequestHeader => (CusMiscRequestHeader)BusinessObject;

		public static class DataContexts
		{
			public const string CusMiscRequest = ".CusMiscRequest";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a document menu name")]
		public static class MenuNames
		{
			public const string ApplicationforExtendedOfficeHours = "Application for Extended Office Hours";
			public const string FinalPricePeriodExtensionApplication = "Final Price Period Extension Application";
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(DataContexts.CusMiscRequest));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();
			switch (dataContextValue.FullDataContext)
			{
				case DataContexts.CusMiscRequest:
					result.Add(BODocDataProvider.Get(new MiscRequestDocumentWrapper(CusMiscRequestHeader, Factory)));
					break;
			}

			return result.Count > 0 ? result.ToArray() : base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			if (result != null)
			{
				return result;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.MiscRequest, BusinessObject) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.MiscRequest };

		protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem menuItem)
		{
			var result = base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(menuItem);
			IStatusList statusList = Factory.GetCachedValue<CustomsMessageStatusTypeList>();
			var isToBeWarned = statusList.ShouldUsersBeWarnedPriorToPrintingDocument(CusMiscRequestHeader.CMR_Status);

			if (isToBeWarned)
			{
				result.Add(new DocumentSupporterQuestion(ZString.Empty, statusList.GetDocumentPrintingWarningMessage() + " " + Res.GetString("04189860-F815-473F-B0D8-98392ADAF725", "Are you sure you wish to print this document?"), QuestionType.Warning));
			}
			return result;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.MSGBKRCTY:
					return CusMiscRequestHeader.CMR_MessageType + Core.Constants.CountryCodes.KoreaSouth;

				default:
					return base.GetFilterValue(filterName);
			}
		}
	}
}
