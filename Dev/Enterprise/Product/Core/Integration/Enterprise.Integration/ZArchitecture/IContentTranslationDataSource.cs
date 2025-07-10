using System.Collections.Generic;

namespace Enterprise.Integration
{
	public interface IContentTranslationDataSource
	{
		IEnumerable<IContentTranslationDataItem> GetTranslatableResources();
	}

	public interface IContentTranslationDataItem
	{
		string Id { get; }
		IDictionary<string, string> Resources { get; }
	}
}
