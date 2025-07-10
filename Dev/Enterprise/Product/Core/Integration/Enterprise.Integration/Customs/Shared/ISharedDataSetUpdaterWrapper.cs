using System.Collections.Generic;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ISharedDataSetUpdaterWrapper
			{
				void SetLogger(ILogger logger);
				IEnumerable<ISharedDataSetUpdater> GetAllDataSetUpdater();
				bool Update(string tableName, string dataSet);
				void Dispose();
			}
		}
	}
}
