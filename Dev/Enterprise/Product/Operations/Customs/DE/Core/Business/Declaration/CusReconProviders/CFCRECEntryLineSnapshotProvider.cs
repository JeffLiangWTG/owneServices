using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CFCRECEntryLineSnapshotProvider : IMonthlyClosingEntryLineSnapshot
	{
		public CFCRECEntryLineSnapshotProvider(ICFCRECLine line, ICFCRECHeader header)
		{
			this.line = Argument.NotNull(line, nameof(line));
			this.header = Argument.NotNull(header, nameof(header));
		}
		readonly ICFCRECLine line;
		readonly ICFCRECHeader header;

		public string CessionManagementFlag => line.CessionManagementFlag;

		public string TobaccoRevenueStampNumber => line.TobaccoRevenueStampNumber;

		public ILinePreferentialTreatment PreferentialTreatment => line.PreferentialTreatment;

		public IAmount InwardMovementAmount => null;

		public int? ReferencedSequenceNumber => null;

		public string MatterCode => null;

		public string ArticleNumber => null;

		public decimal? InvoiceAmount => null;

		public string PreferentialCountry => line.PreferentialOriginCountry;

		public string DepartureCountry => header.DepartureCountry;

		public string ForeignTradeFlag => null;

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
