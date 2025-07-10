using System.Collections.Generic;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ISharedDataSetUpdater
			{
				string Name { get; }
				IEnumerable<string> DataSetNames { get; }
			}
		}
	}
}
