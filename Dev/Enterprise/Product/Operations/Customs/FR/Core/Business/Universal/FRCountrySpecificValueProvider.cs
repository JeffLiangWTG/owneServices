using System.Collections.Generic;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class FRCountrySpecificValueProvider : Integration.Customs.ICountrySpecificValueProvider
	{
		public IDictionary<string, decimal> GetCountrySpecificValueList(Integration.Customs.ICusEntryLine entryLine)
		{
			return entryLine is CusEntryLine line ? new Dictionary<string, decimal>
			{
				{ UniversalReferenceConstants.RefCusRateTypeFormula.STATVAL, line.CL_StatisticalValue }
			} : new Dictionary<string, decimal>();
		}
	}
}
