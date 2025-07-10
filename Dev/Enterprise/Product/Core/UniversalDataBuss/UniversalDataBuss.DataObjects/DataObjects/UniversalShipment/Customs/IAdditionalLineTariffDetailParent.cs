using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IAdditionalLineTariffDetailParent
	{
		List<AdditionalLineTariffDetail> AdditionalLineTariffDetailCollection { get; set; }
	}
}
