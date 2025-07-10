using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IMonthlyClosingDecLineEqualityComparer : IEqualityComparer<IMonthlyClosingDecLine>
	{
		public bool Equals(IMonthlyClosingDecLine px, IMonthlyClosingDecLine py) => ComparerHelper.Compare(px, py, (x, y) =>
			x.SequenceNumber.Equals(y.SequenceNumber) &&
			x.ReferencedSequenceNumber.Equals(y.ReferencedSequenceNumber) &&
			string.Equals(x.MatterCode, y.MatterCode) &&
			string.Equals(x.ArticleNumber, y.ArticleNumber) &&
			x.InvoiceAmount.Equals(y.InvoiceAmount) &&
			string.Equals(x.DepartureCountry, y.DepartureCountry) &&
			x.CompleteDeclarationFlag.Equals(y.CompleteDeclarationFlag) &&
			string.Equals(x.ForeignTradeStatisticsGoodsStatus, y.ForeignTradeStatisticsGoodsStatus) &&
			string.Equals(x.ForeignTradeStatisticsTransactionType, y.ForeignTradeStatisticsTransactionType) &&
			string.Equals(x.ForeignTradeStatisticsDestinationCountry, y.ForeignTradeStatisticsDestinationCountry) && //
			string.Equals(x.ForeignTradeStatisticsDestinationFederalState, y.ForeignTradeStatisticsDestinationFederalState) &&
			string.Equals(x.ForeignTradeStatisticsInlandTransportMode, y.ForeignTradeStatisticsInlandTransportMode) &&
			new IAmountEqualityComparer().Equals(x.ForeignTradeStatisticsAmount, y.ForeignTradeStatisticsAmount) &&
			new IImportLineCustomsValueEqualityComparer().Equals(x.CustomsValue, y.CustomsValue) &&
			string.Equals(x.BorderTransportMeansMode, y.BorderTransportMeansMode) &&
			string.Equals(x.BorderTransportMeansType, y.BorderTransportMeansType) &&
			string.Equals(x.BorderTransportMeansInformation, y.BorderTransportMeansInformation) &&
			string.Equals(x.BorderTransportMeansNationality, y.BorderTransportMeansNationality) &&
			x.NetMassMeasure.Equals(y.NetMassMeasure) &&
			string.Equals(x.OriginCountry, y.OriginCountry) &&
			string.Equals(x.SupplementaryInformation, y.SupplementaryInformation) &&
			string.Equals(x.CommodityCode, y.CommodityCode) &&
			x.AdditionalProcedure.EqualIgnoringOrder(y.AdditionalProcedure) &&
			x.SupplementaryCodes.EqualIgnoringOrder(y.SupplementaryCodes) &&
			x.ForeignTradeStatisticsQuantity.Equals(y.ForeignTradeStatisticsQuantity) &&
			x.ForeignTradeStatisticsGrossMassMeasure.Equals(y.ForeignTradeStatisticsGrossMassMeasure) &&
			x.AssessmentCustomsValue.Equals(y.AssessmentCustomsValue) &&
			x.AssessmentAmount.EqualIgnoringOrder(y.AssessmentAmount, new IAmountEqualityComparer()) &&
			x.AssessmentSpecificRate.EqualIgnoringOrder(y.AssessmentSpecificRate, new IImportSpecificRateEqualityComparer()) &&
			x.AssessmentContentInformation.EqualIgnoringOrder(y.AssessmentContentInformation, new IContentInformationEqualityComparer()) &&
			x.ExciseDuty.EqualIgnoringOrder(y.ExciseDuty, new IExciseDutyEqualityComparer()) &&
			x.Documents.EqualIgnoringOrder(y.Documents, new IImportLineDocumentEqualityComparer()));

		public int GetHashCode(IMonthlyClosingDecLine obj) => 0;
	}
}
