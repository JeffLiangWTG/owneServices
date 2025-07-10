using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITopLevelDataObjectReader
	{
		BusinessObject ReadIntoTopLevelBusinessObject();
		void ReadIntoBusinessObject(ref BusinessObject targetBO);
		BusinessObject GetExistingBusinessObject();
		IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism();
	}
}
