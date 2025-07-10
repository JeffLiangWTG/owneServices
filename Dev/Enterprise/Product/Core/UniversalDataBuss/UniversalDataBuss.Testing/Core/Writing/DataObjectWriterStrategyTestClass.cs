using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public class DataObjectWriterStrategyTestClass : IDataObjectWriterStrategy
	{
		public DataObjectWriterStrategyTestClass(Func<string, bool> isAllowSet)
		{
			this.isAllowSet = isAllowSet;
		}
		readonly Func<string, bool> isAllowSet;
		public bool IsAllowSet(string fieldName) => isAllowSet(fieldName);
	}
}
