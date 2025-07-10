using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public JobDeclaration Declaration => (JobDeclaration)BusinessObject;

		public static class DataContexts
		{
			public const string EXPEntryHeaderBO = ".EXPEntryHeaderBO";
			public const string IMPEntryHeaderBO = ".IMPEntryHeaderBO";
			public const string LEXEntryHeaderBO = ".LEXEntryHeaderBO";
			public const string EntryMessageBO = ".EntryMessageBO";
			public const string EntrySnapshot = ".EntrySnapshot";
			public const string EntryOrSnapshotBO = ".EntryOrSnapshotBO";
		}

		#region SuppressResourceStringsCheckRegion

		public static class MenuNames
		{
			public const string ExportDeclarationCertificate = "Export Declaration Certificate (Korean)";
			public const string ExportDeclarationCertificate_English = "Export Declaration Certificate (English)";
			public const string GoodsRemovalPriorToCustomsRelease = "Goods Removal Prior To Customs Release";
			public const string AgreedRateForAllLines = "Agreed Rate For All Lines";
			public const string GoldVATDeclaration = "Gold VAT Declaration";
			public const string ApplyingTaxExemptionOrSpecificUseDutyRate = "Applying Tax Exemption Or Specific Use Duty Rate";
			public const string CancellationOfImportDeclaration = "Cancellation Of Import Declaration";
			public const string CancellationOfExportDeclaration = "Cancellation Of Export Declaration";
			public const string RequestToExtendReExportDate = "Request to extend re-export date";
			public const string ExportGoodsInspectionResultReport = "Export Goods Inspection Result Report";
			public const string InspectionPlanAndResultReport = "Inspection Plan And Result Report";
			public const string NoticeOfTaxAdjustment = "Notice Of Tax Adjustment";
			public const string CorrectionNoticeOfCountryOfOrigin = "Correction Notice Of Country Of Origin";
			public const string NoticeOfFinalizedRefund = "Notice Of Finalized Refund";
			public const string NoticeOfAmendmentOrSupplementaryActions = "Notice Of Amendment Or Supplementary Actions";
			public const string NoticeOfCorrectionReviewResults = "Notice Of Correction Review Results";
			public const string NoticeOfCustomsMandatedAmendment = "Notice Of Customs-Mandated Amendment";
			public const string RefundRequest = "Refund Request";
			public const string ReImportOfExportedGoods = "Re-Import Of Exported Goods";
			public const string ImportTaxInvoiceForIndividualDeclaredCase = "Import Tax Invoice For Individual Declared Case";
			public const string InvoiceofCustomsDisbursementCharges = "Invoice of Customs Disbursement Charges";
			public const string ExportVehicleNo = "Export Vehicle No";
			public const string ApplicationofFTARate = "Application of FTA Rate";
			public const string LocalExportGoodsInspectionResultReport = "Local Export Goods Inspection Result Report";
			public const string LocalExportDeclaration = "Local Export Declaration";
			public const string AmendmentOfExportDeclaration = "Amendment Of Export Declaration";
			public const string AmendmentOfLocalExportDeclaration = "Amendment Of Local Export Declaration";
		}

		#endregion

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(DataContexts.EXPEntryHeaderBO));
			result.Add(new DataContextValue(DataContexts.IMPEntryHeaderBO));
			result.Add(new DataContextValue(DataContexts.LEXEntryHeaderBO));
			result.Add(new DataContextValue(DataContexts.EntryMessageBO));
			result.Add(new DataContextValue(DataContexts.EntrySnapshot));
			result.Add(new DataContextValue(DataContexts.EntryOrSnapshotBO));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();
			foreach (var entry in GetEntriesToPrint())
			{
				var menuName = commandBeingRun?.SU_MenuNameMultilingual.GetUnresolvedString() ?? ZString.Empty;
				var dataSourceArr = entry.GetDocumentDataSource(dataContextValue, menuName);
				foreach (var dataSource in dataSourceArr)
				{
					result.Add(BODocDataProvider.Get(dataSource));
				}
				if (DocumentGenerationActions.IsAmendmentRelevant)
				{
					var actions = DocumentGenerationActions.Find(x => x.EntryNumber == entry.EntryNumber);
					foreach (var action in actions)
					{
						if (action.IncludingCurrentDifference)
						{
							if (entry.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._830))
							{
								var amendManager = AmendmentDetailsManager.New(entry, ElectronicDocumentTypeList.Codes._5AS);
								var exportAmendmentHeader = new Export5ASHeaderCreator().Create(entry, amendManager.AmendedItems.ToArray());
								var currentAmendmentDetails = new ExportAmendmentDetails(exportAmendmentHeader, Factory, entry.PK);
								var latestSnapshotVersionNum = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged).CES_VersionNumber;
								currentAmendmentDetails.Decorate(entry, latestSnapshotVersionNum);
								result.Add(BODocDataProvider.Get(currentAmendmentDetails));
							}
						}
					}
				}
			}
			return result.Count > 0 ? result.ToArray() : base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = ZString.Empty;
			var englishMenuName = commandBeingRun?.SU_MenuNameMultilingual.GetUnresolvedString() ?? ZString.Empty;
			switch (dataContextValue.FullDataContext)
			{
				case DataContexts.EntryOrSnapshotBO:
				case DataContexts.EXPEntryHeaderBO:
				case DataContexts.IMPEntryHeaderBO:
				case DataContexts.LEXEntryHeaderBO:
				case DataContexts.EntrySnapshot:
				case DataContexts.EntryMessageBO:
					if (Declaration.CustomsEntryHeaders.Count == 0)
					{
						result = Res.GetString("4F8B184A-102E-408C-82BD-C89BEFECB636", "No entry exists.");
					}
					else if (!GetEntriesToPrint().Any())
					{
						switch (englishMenuName)
						{
							case MenuNames.InvoiceofCustomsDisbursementCharges:
								result = Res.GetString("611C76CD-37C3-44ED-A0D1-8F4A9BB2EC59", "There are no entries with an unpaid invoice. No Customs invoices have been received or all of them have been paid.");
								break;
							case MenuNames.ExportVehicleNo:
								result = Res.GetString("353A2B00-23AB-4690-9ED3-698A58938E0D", "There is no entry which has second-hand vehicles under any of its invoice lines.");
								break;
							case MenuNames.ReImportOfExportedGoods:
								result = Res.GetString("1F5C2725-AEFA-4B91-8FD8-ECB333EC4A9B", "There is no entry having goods which are previously exported.");
								break;
							case MenuNames.AmendmentOfExportDeclaration:
								result = Res.GetString("F9164ADD-C3BA-416F-A47C-B8F4943C34C3", "No entry accepted.");
								break;
							case MenuNames.CancellationOfExportDeclaration:
								result = Res.GetString("46723C87-4E77-42C3-A223-680D2F1D85D4", "Cancellation has never been sent.");
								break;
							default:
								result = Res.GetString("5A03C6D7-FC5B-402D-83FF-B83D0C5AF723", "There are no relevant entries to print a document from.");
								break;
						}
					}
					break;
			}

			return !result.IsEmpty ? result : base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}
		IEnumerable<CusEntryHeader> GetEntriesToPrint()
		{
			return DocumentGenerationActions.EntriesToDeliver;
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			var declarationDocumentEventsHandler = ObjectFactory.Get<IDocumentEventsHandler>("IDocumentEventsHandler.KR.DeclarationDocumentEventsHandler");
			declarationDocumentEventsHandler.DocumentSupporter = this;
			result.Add(declarationDocumentEventsHandler);
			return result.ToArray();
		}

		public bool AreMultipleEntriesRelevant => DocumentGenerationActions.Count > 1;

		public DocumentGeneratingActionCollection DocumentGenerationActions
		{
			get { return documentGenerationActions ?? (documentGenerationActions = new DocumentGeneratingActionCollection(Declaration)); }
		}
		DocumentGeneratingActionCollection documentGenerationActions;
	}
}
