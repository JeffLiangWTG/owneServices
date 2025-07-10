using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.KR.Business
{
	public class DocumentGeneratingActionCollection : NonPersistentBusinessObjectCollection<DocumentGeneratingAction>
	{
		public DocumentGeneratingActionCollection(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			Argument.NotNull(declaration, "declaration");

			this.declaration = declaration;
			PopulateElements(null);
		}

		readonly JobDeclaration declaration;
		public ZBool IsStatusRelevant { get; private set; }
		public ZBool IsStatementRelevant { get; private set; }
		public ZBool IsAmendmentRelevant { get; private set; }

		void PopulateElements(string englishMenuName)
		{
			var isRelevant = GetRelevantFilterFor(englishMenuName);
			IsStatusRelevant = GetStatusRelevantFor(englishMenuName);
			IsStatementRelevant = GetStatementRelevantFor(englishMenuName);
			IsAmendmentRelevant = GetAmendmentRelevantFor(englishMenuName);

			foreach (CusEntryHeader entry in declaration.CustomsEntryHeaders)
			{
				if (isRelevant(entry))
				{
					Add(new DocumentGeneratingAction(entry, entry.EntryNumber, englishMenuName));
				}
			}

			if (Count == 1)
			{
				this[0].ToBeDelivered = true;
			}
		}

		ZBool GetStatusRelevantFor(string englishMenuName)
		{
			ZBool result = ZBool.True;
			switch (englishMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment:
				case JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin:
				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund:
				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfAmendmentOrSupplementaryActions:
				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults:
				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment:
				case JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase:
				case JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges:
					result = ZBool.False;
					break;

				default:
					break;
			}
			return result;
		}
		ZBool GetStatementRelevantFor(string englishMenuName)
		{
			ZBool result = ZBool.False;
			switch (englishMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges:
					result = ZBool.True;
					break;

				default:
					break;
			}
			return result;
		}

		ZBool GetAmendmentRelevantFor(string englishMenuName)
		{
			ZBool result = ZBool.False;
			switch (englishMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
					result = ZBool.True;
					break;

				default:
					break;
			}
			return result;
		}

		Predicate<CusEntryHeader> GetRelevantFilterFor(string englishMenuName)
		{
			Predicate<CusEntryHeader> result = (CusEntryHeader entry) => true;
			if (!string.IsNullOrEmpty(englishMenuName))
			{
				string messageType = GetMessageType(englishMenuName);

				if (!string.IsNullOrEmpty(messageType))
				{
					result = (CusEntryHeader entry) => entry.Messages.Cast<EDIMessage>().Any(m => m.EM_MessageType == messageType);
				}
				else
				{
					switch (englishMenuName)
					{
						case JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines:
							result = (CusEntryHeader entry) => !(entry.EntryInstruction?.CEI_AgreedDutyRatePreferenceCode ?? ZString.Empty).IsEmpty;
							break;
						case JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges:
							result = (CusEntryHeader entry) => entry.IndividualStatements != null && entry.IndividualStatements.Length > 0;
							break;
						case JobDeclarationDocumentSupporter.MenuNames.RefundRequest:
							result = (CusEntryHeader entry) => entry.GetDocumentDataSource(new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntrySnapshot), englishMenuName).FirstOrDefault() != null;
							break;
						case JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo:
							result = (CusEntryHeader entry) =>
							{
								var wrapper = entry.GetDocumentDataSource(new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO), englishMenuName).FirstOrDefault() as ExportEntryHeaderWrapper;
								if (wrapper != null)
								{
									foreach (var entryLine in wrapper.Header.EntryLines)
									{
										if (entryLine.InvoiceLines.Cast<IExportInvoiceLine>().SelectMany(x => x.VehicleNumbers).Any())
										{
											return true;
										}
									}
								}
								return false;
							};
							break;
						case JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods:
							result = (CusEntryHeader entry) =>
							{
								var wrapper = entry.GetDocumentDataSource(new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO), englishMenuName).FirstOrDefault() as ImportEntryHeaderWrapper;
								return (wrapper?.ImportPreviousExpDecLines.Count ?? 0) > 0;
							};
							break;
						case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
							result = (CusEntryHeader entry) => entry.Snapshots.DoesSnapshotExist(ElectronicDocumentTypeList.Codes._830);
							break;
						case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration:
							result = (CusEntryHeader entry) => entry.GetDocumentDataSource(new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO), englishMenuName).FirstOrDefault() != null;
							break;
						case JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration:
							result = (CusEntryHeader entry) => entry.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._DKJ);
							break;
					}
				}
			}
			return result;
		}

		string GetMessageType(string englishMenuName)
		{
			string result = string.Empty;

			switch (englishMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease:
					result = ElectronicDocumentTypeList.Codes._5BD;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration:
					result = ElectronicDocumentTypeList.Codes._5TM;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate:
					result = ElectronicDocumentTypeList.Codes._5FN;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration:
					result = ElectronicDocumentTypeList.Codes._5BF;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate:
					result = ElectronicDocumentTypeList.Codes._D72;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment:
					result = ElectronicDocumentTypeList.Codes._5WN;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin:
					result = ElectronicDocumentTypeList.Codes._5GU;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund:
					result = ElectronicDocumentTypeList.Codes._5UO;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfAmendmentOrSupplementaryActions:
					result = ElectronicDocumentTypeList.Codes._5GV;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults:
					result = ElectronicDocumentTypeList.Codes._5TW;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment:
					result = ElectronicDocumentTypeList.Codes._5TV;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase:
					result = ElectronicDocumentTypeList.Codes._5FV;
					break;

				default:
					break;
			}
			return result;
		}

		public int CountSelected => this.Cast<DocumentGeneratingAction>().Count(x => x.ToBeDelivered);
		public IEnumerable<CusEntryHeader> EntriesToDeliver => this.Cast<DocumentGeneratingAction>().Where(x => x.ToBeDelivered).Select(x => x.Entry);

		public void InitialiseFor(IStmMenuItem menu)
		{
			RemoveAll();
			PopulateElements(menu.SU_MenuNameMultilingual.GetUnresolvedString());
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("The method is not supported.");
	}
}
