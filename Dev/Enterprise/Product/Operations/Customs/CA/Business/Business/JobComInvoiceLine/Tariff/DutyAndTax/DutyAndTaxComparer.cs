using System.Collections.Generic;

namespace Enterprise.Customs.CA.Business
{
	class DutyAndTaxComparer : IComparer<DutyAndTax>
	{
		#region Implementation of IComparer<DutyAndTax>

		public int Compare(DutyAndTax x, DutyAndTax y)
		{
			var result = x.C1_TaxType.CompareTo(y.C1_TaxType);
			if (result == 0)
			{
				result = x.C1_Code.CompareTo(y.C1_Code);
			}

			if (result == 0)
			{
				result = x.C1_ExemptCode.CompareTo(y.C1_ExemptCode);
			}

			if (result == 0)
			{
				result = x.C1_RateType.CompareTo(y.C1_RateType);
			}

			if (result == 0)
			{
				result = x.C1_UnitOfMeasure.CompareTo(y.C1_UnitOfMeasure);
			}

			if (result == 0)
			{
				result = x.C1_Rate.CompareTo(y.C1_Rate);
			}

			if (result == 0)
			{
				result = x.C1_PreviousTranNumber.CompareTo(y.C1_PreviousTranNumber);
			}

			if (result == 0)
			{
				result = x.C1_PreviousTranLine.CompareTo(y.C1_PreviousTranLine);
			}

			return result;
		}

		#endregion
	}
}
