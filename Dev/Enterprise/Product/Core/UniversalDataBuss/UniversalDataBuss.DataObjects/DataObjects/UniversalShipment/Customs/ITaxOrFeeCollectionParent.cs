using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface ITaxOrFeeCollectionParent
	{
		List<TaxOrFee> TaxOrFeeCollection  { get; set; }
	}
}
