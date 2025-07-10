using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IImportDecLineEqualityComparer : IEqualityComparer<IImportDecLine>
	{
		public bool Equals(IImportDecLine px, IImportDecLine py) => ComparerHelper.Compare(px, py, (x, y) =>
			x.SequenceNumber.Equals(y.SequenceNumber) &&
			string.Equals(x.RequestedPreviousProcedure, y.RequestedPreviousProcedure) &&
			string.Equals(x.GoodsDescription, y.GoodsDescription) &&
			x.NetMassMeasure.Equals(y.NetMassMeasure) &&
			string.Equals(x.OriginCountry, y.OriginCountry) &&
			string.Equals(x.SupplementaryInformation, y.SupplementaryInformation) &&
			string.Equals(x.CommodityCode, y.CommodityCode) &&
			x.ForeignTradeStatisticsQuantity.Equals(y.ForeignTradeStatisticsQuantity) &&
			x.ForeignTradeStatisticsGrossMassMeasure.Equals(y.ForeignTradeStatisticsGrossMassMeasure) &&
			x.AssessmentCustomsValue.Equals(y.AssessmentCustomsValue) &&
			x.SupplementaryCodes.EqualIgnoringOrder(y.SupplementaryCodes) &&
			x.AdditionalProcedure.EqualIgnoringOrder(y.AdditionalProcedure) &&
			new IImportPackageEqualityComparer().Equals(x.Package, y.Package) &&
			x.AssessmentAmount.EqualIgnoringOrder(y.AssessmentAmount, new IAmountEqualityComparer()) &&
			x.AssessmentSpecificRate.EqualIgnoringOrder(y.AssessmentSpecificRate, new IImportSpecificRateEqualityComparer()) &&
			x.AssessmentContentInformation.EqualIgnoringOrder(y.AssessmentContentInformation, new IContentInformationEqualityComparer()) &&
			x.ExciseDuty.EqualIgnoringOrder(y.ExciseDuty, new IExciseDutyEqualityComparer()) &&
			x.Documents.EqualIgnoringOrder(y.Documents, new IImportLineDocumentEqualityComparer()));

		public int GetHashCode(IImportDecLine obj) => 0;
	}
}
