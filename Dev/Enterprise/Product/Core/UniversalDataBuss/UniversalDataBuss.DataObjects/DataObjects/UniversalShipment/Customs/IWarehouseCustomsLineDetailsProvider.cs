using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IWarehouseCustomsLineDetailsProvider
	{
		IEnumerable<IWarehouseCustomsLineDetails> GetLineDetails();
	}
}
