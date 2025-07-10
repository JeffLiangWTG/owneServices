using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class CusRefRateCodeViewComparer : IComparer<CusRefRateCodeView>
	{
		public int Compare(CusRefRateCodeView x, CusRefRateCodeView y)
		{
			if (x.ZY1_RateType != y.ZY1_RateType && x.RateType != null && y.RateType != null)
			{
				if (DependsOn(x.RateType, y.RateType))
				{
					return 1;
				}
				if (DependsOn(y.RateType, x.RateType))
				{
					return -1;
				}
			}
			return x.ZY1_RateCode.CompareTo(y.ZY1_RateCode);
		}

		bool DependsOn(RefCusRateType x, RefCusRateType y) => !x.ZZR_CustomsValueFormula.IsEmpty && x.ZZR_CustomsValueFormula.Contains(y.ZZR_RateType);
	}
}
