using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	sealed class ICFCPEDLineEqualityComparer : IMonthlyClosingDecLineEqualityComparer, IEqualityComparer<ICFCPEDLine>
	{
		public bool Equals(ICFCPEDLine px, ICFCPEDLine py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.CessionManagementFlag, y.CessionManagementFlag) &&
			string.Equals(x.PreferentialOriginCountry, y.PreferentialOriginCountry) &&
			string.Equals(x.TobaccoRevenueStampNumber, y.TobaccoRevenueStampNumber) &&
			x.AssessmentOutwardProcessingFee.Equals(y.AssessmentOutwardProcessingFee) &&
			x.AssessmentTaxCosts.Equals(y.AssessmentTaxCosts) &&
			new ILinePreferentialTreatmentEqualityComparer().Equals(x.PreferentialTreatment, y.PreferentialTreatment) &&
			x.SpecialCase.EqualIgnoringOrder(y.SpecialCase, new IImportSpecialCaseEqualityComparer()) &&
			base.Equals(x, y));

		public int GetHashCode(ICFCPEDLine obj) => 0;
	}
}
