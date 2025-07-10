using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public interface IPreValidateTraderDataProvider
	{
		IDataContextDataObject GetDataContext();
		List<Context> GetContextCollection();
	}
}
