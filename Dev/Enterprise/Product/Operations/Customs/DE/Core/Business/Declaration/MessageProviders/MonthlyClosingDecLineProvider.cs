using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business
{
	public abstract class MonthlyClosingDecLineProvider : IMonthlyClosingDecLine
	{
		protected MonthlyClosingDecLineProvider(CusReconEntryLine reconEntryLine, bool isModificationMessage)
		{
			IsModificationMessage = isModificationMessage;
			ReconEntryLine = Argument.NotNull(reconEntryLine, nameof(reconEntryLine));
			ReconEntry = ReconEntryLine.ReconEntry;
			EntryHeader = (CusEntryHeader)ReconEntry.EntryHeader;
			EntryLine = EntryHeader.AllEntryLines.FindByLineNumber(ReconEntryLine.CRL_OriginalEntryLineNumber);
			Declaration = EntryHeader.Declaration;
			RandomInvoiceLine = EntryLine.RandomLine;
			RandomInvoiceHeader = RandomInvoiceLine.InvoiceHeader;
			InvoiceLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>();
		}

		protected readonly bool IsModificationMessage;
		protected readonly CusReconEntryLine ReconEntryLine;
		protected readonly CusReconEntry ReconEntry;
		protected readonly CusEntryLine EntryLine;
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;
		protected readonly JobComInvoiceLine RandomInvoiceLine;
		protected readonly JobComInvoiceHeader RandomInvoiceHeader;
		protected readonly IEnumerable<JobComInvoiceLine> InvoiceLines;

		public int SequenceNumber => ReconEntryLine.CRL_LineNumber;

		public int? ReferencedSequenceNumber => ReconEntryLine.CRL_OriginalEntryLineNumber;

		public string MatterCode => RandomInvoiceLine.JI_CustomAttrib1;

		public string ArticleNumber => RandomInvoiceLine.JI_PartNo;

		public decimal? InvoiceAmount => CachedValueHelper.GetValue(ref invoiceAmount, () => InvoiceLines.Sum(x => x.JI_LinePrice));
		CachedValue<decimal?> invoiceAmount;

		public string DepartureCountry => IsModificationMessage ? HeaderProvider.DepartureCountry : CurrentSnapshot.DepartureCountry ?? string.Empty;

		public bool? CompleteDeclarationFlag => true;

		public string ForeignTradeStatisticsGoodsStatus => Declaration.JE_StatisticStatus;

		public string ForeignTradeStatisticsTransactionType => RandomInvoiceHeader.JZ_ValuationCode;

		public string ForeignTradeStatisticsDestinationCountry => Declaration.JE_GoodsDestination;

		public string ForeignTradeStatisticsDestinationFederalState => CachedValueHelper.GetValue(ref foreignTradeStatisticsDestinationFederalState, () => ImportMappingHelper.GetDestinationFederalState(Declaration, ForeignTradeStatisticsDestinationCountry));
		CachedValue<string> foreignTradeStatisticsDestinationFederalState;

		public string ForeignTradeStatisticsInlandTransportMode => IsModificationMessage ? HeaderProvider.ForeignTradeStatisticsInlandTransportMode : CurrentSnapshot.ForeignTradeStatistics?.InlandTransportMode ?? string.Empty;

		public IAmount ForeignTradeStatisticsAmount => CachedValueHelper.GetValue(ref foreignTradeStatisticsAmount, () =>
		{
			var invoiceLine = InvoiceLines.FirstOrDefault(l => l.JI_CustomsSecondQuantity != ZDecimal.Zero);
			if (invoiceLine != null)
			{
				var quantity = InvoiceLines.Sum(x => x.JI_CustomsSecondQuantity);
				return new AmountProvider(quantity, invoiceLine.JI_CustomsSecondUnitQty);
			}
			else
			{
				return null;
			}
		});
		CachedValue<IAmount> foreignTradeStatisticsAmount;

		public IImportLineCustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd && !IsProcedureInE01OrE02 ? new ImportLineCustomsValueProvider(Declaration, InvoiceLines) : null);
		CachedValue<IImportLineCustomsValue> customsValue;

		public string BorderTransportMeansMode => IsModificationMessage ? HeaderProvider.BorderTransportMeansMode : CurrentSnapshot.BorderTransportMeans?.Mode ?? string.Empty;

		public string BorderTransportMeansType => IsModificationMessage ? HeaderProvider.BorderTransportMeansType : CurrentSnapshot.BorderTransportMeans?.Type ?? string.Empty;

		public string BorderTransportMeansInformation => IsModificationMessage ? HeaderProvider.BorderTransportMeansInformation : CurrentSnapshot.BorderTransportMeans?.Information ?? string.Empty;

		public string BorderTransportMeansNationality => IsModificationMessage ? HeaderProvider.BorderTransportMeansNationality : CurrentSnapshot.BorderTransportMeans?.Nationality ?? string.Empty;

		public string RequestedPreviousProcedure => throw new NotImplementedException("property should not be used");

		public string GoodsDescription => throw new NotImplementedException("property should not be used");

		public decimal NetMassMeasure => IsModificationMessage ? LineProvider.NetMassMeasure : CurrentSnapshot.NetMassMeasure;

		public bool NetMassMeasureSpecified => IsModificationMessage || CurrentSnapshot.NetMassMeasureSpecified;

		public string OriginCountry => IsModificationMessage ? LineProvider.OriginCountry : CurrentSnapshot.OriginCountry;

		public string SupplementaryInformation => IsModificationMessage ? LineProvider.SupplementaryInformation : CurrentSnapshot.SupplementaryInformation;

		public string CommodityCode => IsModificationMessage ? LineProvider.CommodityCode : CurrentSnapshot.CommodityCode;

		public IReadOnlyCollection<string> AdditionalProcedure
		{
			get
			{
				if (additionalProcedure == null)
				{
					if (IsModificationMessage)
					{
						additionalProcedure = LineProvider.AdditionalProcedure;
					}
					else
					{
						additionalProcedure = CurrentSnapshot.AdditionalProcedure?.Select(p => p.Code).ToArray() ?? Array.Empty<string>();
					}
				}
				return additionalProcedure;
			}
		}
		IReadOnlyCollection<string> additionalProcedure;

		public IReadOnlyCollection<string> SupplementaryCodes
		{
			get
			{
				if (supplementaryCodes == null)
				{
					if (IsModificationMessage)
					{
						supplementaryCodes = LineProvider.SupplementaryCodes;
					}
					else
					{
						supplementaryCodes = CurrentSnapshot.SupplementaryCodes?.Select(p => p.Code).ToArray() ?? Array.Empty<string>();
					}
				}
				return supplementaryCodes;
			}
		}
		IReadOnlyCollection<string> supplementaryCodes;

		public IImportPackage Package => throw new NotImplementedException("property should not be used");

		public decimal ForeignTradeStatisticsQuantity => LineProvider.ForeignTradeStatisticsQuantity;

		public decimal ForeignTradeStatisticsGrossMassMeasure => IsModificationMessage ? LineProvider.ForeignTradeStatisticsGrossMassMeasure : (CurrentSnapshot?.ForeignTradeStatistics?.GrossMassMeasure ?? decimal.Zero);

		public decimal AssessmentCustomsValue => !Declaration.ZG_IsHighValueOvrd ? LineProvider.AssessmentCustomsValue : decimal.Zero;

		public IReadOnlyCollection<IAmount> AssessmentAmount
		{
			get
			{
				if (assessmentAmount == null)
				{
					if (IsModificationMessage)
					{
						assessmentAmount = LineProvider.AssessmentAmount;
					}
					else
					{
						assessmentAmount = CurrentSnapshot?.Assessment?.Amount?.Select(a => AmountFromSnapshotProvider.NewOrNull(a)).ToArray() ?? Array.Empty<IAmount>();
					}
				}
				return assessmentAmount;
			}
		}
		IReadOnlyCollection<IAmount> assessmentAmount;

		public IReadOnlyCollection<IImportSpecificRate> AssessmentSpecificRate
		{
			get
			{
				if (assessmentSpecificRate == null)
				{
					if (IsModificationMessage)
					{
						assessmentSpecificRate = LineProvider.AssessmentSpecificRate;
					}
					else
					{
						assessmentSpecificRate = CurrentSnapshot.Assessment?.SpecificRate?.Select(s => new SpecificRateFromSnapshotProvider(s)).ToArray() ?? Array.Empty<IImportSpecificRate>();
					}
				}
				return assessmentSpecificRate;
			}
		}
		IReadOnlyCollection<IImportSpecificRate> assessmentSpecificRate;

		public IReadOnlyCollection<IContentInformation> AssessmentContentInformation
		{
			get
			{
				if (assessmentContentInformation == null)
				{
					if (IsModificationMessage)
					{
						assessmentContentInformation = LineProvider.AssessmentContentInformation;
					}
					else
					{
						assessmentContentInformation = CurrentSnapshot.Assessment?.ContentInformation?.Select(c => new ContentInformationFromSnapshotProvider(c)).ToArray() ?? Array.Empty<IContentInformation>();
					}
				}
				return assessmentContentInformation;
			}
		}

		IReadOnlyCollection<IContentInformation> assessmentContentInformation;

		public IReadOnlyCollection<IExciseDuty> ExciseDuty
		{
			get
			{
				if (exciseDuty == null)
				{
					if (IsModificationMessage)
					{
						exciseDuty = LineProvider.ExciseDuty;
					}
					else
					{
						exciseDuty = CurrentSnapshot.ExciseDuty?.Select(e => new ExciseDutyFromSnapshotProvider(e)).ToArray() ?? Array.Empty<IExciseDuty>();
					}
				}
				return exciseDuty;
			}
		}

		IReadOnlyCollection<IExciseDuty> exciseDuty;

		public IReadOnlyCollection<IImportLineDocument> Documents
		{
			get
			{
				if (documents == null)
				{
					if (IsModificationMessage)
					{
						documents = LineProvider.Documents;
					}
					else
					{
						documents = CurrentSnapshot?.Document?.Select(d => new ImportLineDocumentFromSnapshotProvider(d)).ToArray() ?? Array.Empty<IImportLineDocument>();
					}
				}
				return documents;
			}
		}
		IReadOnlyCollection<IImportLineDocument> documents;

		protected BusinessObjectFactory Factory => ReconEntryLine.Factory;

		protected IImportDecLine LineProvider => CachedValueHelper.GetValue(ref lineProvider, () => GetLineProvider(EntryLine));
		CachedValue<IImportDecLine> lineProvider;
		protected abstract IImportDecLine GetLineProvider(CusEntryLine entryLine);

		protected IImportDecHeader HeaderProvider => CachedValueHelper.GetValue(ref headerProvider, () => GetHeaderProvider(EntryHeader));
		CachedValue<IImportDecHeader> headerProvider;
		protected abstract IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader);

		protected DEMonthlyClosingEntryLineSnapshot CurrentSnapshot => CachedValueHelper.GetValue(ref currentSnapshot, () => LoadCurrentSnapshot());
		CachedValue<DEMonthlyClosingEntryLineSnapshot> currentSnapshot;

		protected virtual DEMonthlyClosingEntryLineSnapshot LoadCurrentSnapshot()
		{
			var result = new DEMonthlyClosingEntryLineSnapshot();
			var existingCUREntrySnapshot = ReconEntryLine.CurrentSnapshot;

			if (existingCUREntrySnapshot != null)
			{
				result = CusReconEntryLineSnapshotBuilder.Deserialize(existingCUREntrySnapshot.CRS_SnapshotXml);
			}

			return result;
		}

		protected bool IsProcedureInE01OrE02 => RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E01 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E02;
	}
}
