using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class SCWRECEntryLineSnapshotProvider : IMonthlyClosingEntryLineSnapshot
	{
		public SCWRECEntryLineSnapshotProvider(ISCWRECLine line, ISCWRECHeader header)
		{
			this.line = line;
			this.header = header;
		}
		readonly ISCWRECLine line;
		readonly ISCWRECHeader header;

		public string CessionManagementFlag => null;

		public string TobaccoRevenueStampNumber => null;

		public ILinePreferentialTreatment PreferentialTreatment => new MonthlyClosingSnapshotLinePreferentialTreatmentProvider(line.RequestedPreferentialTreatment);

		public IAmount InwardMovementAmount => line.InwardMovementAmount;

		public int? ReferencedSequenceNumber => null;

		public string MatterCode => null;

		public string ArticleNumber => null;

		public decimal? InvoiceAmount => null;

		public string PreferentialCountry => null;

		public string DepartureCountry => header.DepartureCountry;

		public string ForeignTradeFlag => header.ForeignTradeImportEarlyClearanceFlag;

		public bool? CompleteDeclarationFlag => null;

		public string ForeignTradeStatisticsGoodsStatus => null;

		public string ForeignTradeStatisticsTransactionType => null;

		public string ForeignTradeStatisticsDestinationCountry => null;

		public string ForeignTradeStatisticsDestinationFederalState => null;

		public string ForeignTradeStatisticsInlandTransportMode => header.ForeignTradeStatisticsInlandTransportMode;

		public IAmount ForeignTradeStatisticsAmount => null;

		public IImportLineCustomsValue CustomsValue => null;

		public string BorderTransportMeansMode => header.BorderTransportMeansMode;

		public string BorderTransportMeansType => header.BorderTransportMeansType;

		public string BorderTransportMeansInformation => header.BorderTransportMeansInformation;

		public string BorderTransportMeansNationality => header.BorderTransportMeansNationality;

		public int SequenceNumber => line.SequenceNumber;

		public string RequestedPreviousProcedure => string.Empty;

		public string GoodsDescription => string.Empty;

		public decimal NetMassMeasure => line.NetMassMeasure;

		public bool NetMassMeasureSpecified => true;

		public string OriginCountry => line.OriginCountry;

		public string SupplementaryInformation => line.SupplementaryInformation;

		public string CommodityCode => line.CommodityCode;

		public IReadOnlyCollection<string> AdditionalProcedure => additionalProcedure ?? (additionalProcedure = line.AdditionalProcedure);
		IReadOnlyCollection<string> additionalProcedure;

		public IReadOnlyCollection<string> SupplementaryCodes => supplementaryCodes ?? (supplementaryCodes = line.SupplementaryCodes);
		IReadOnlyCollection<string> supplementaryCodes;

		public IImportPackage Package => null;

		public decimal ForeignTradeStatisticsQuantity => decimal.Zero;

		public decimal ForeignTradeStatisticsGrossMassMeasure => line.ForeignTradeStatisticsGrossMassMeasure;

		public decimal AssessmentCustomsValue => line.AssessmentCustomsValue;

		public IReadOnlyCollection<IAmount> AssessmentAmount => assessmentAmount ?? (assessmentAmount = line.AssessmentAmount);
		IReadOnlyCollection<IAmount> assessmentAmount;

		public IReadOnlyCollection<IImportSpecificRate> AssessmentSpecificRate => assessmentSpecificRate ?? (assessmentSpecificRate = line.AssessmentSpecificRate);
		IReadOnlyCollection<IImportSpecificRate> assessmentSpecificRate;

		public IReadOnlyCollection<IContentInformation> AssessmentContentInformation => assessmentContentInformation ?? (assessmentContentInformation = line.AssessmentContentInformation);
		IReadOnlyCollection<IContentInformation> assessmentContentInformation;

		public IReadOnlyCollection<IExciseDuty> ExciseDuty => exciseDuty ?? (exciseDuty = line.ExciseDuty);
		IReadOnlyCollection<IExciseDuty> exciseDuty;

		public IReadOnlyCollection<IImportLineDocument> Documents => documents ?? (documents = line.Documents);
		IReadOnlyCollection<IImportLineDocument> documents;
	}
}
