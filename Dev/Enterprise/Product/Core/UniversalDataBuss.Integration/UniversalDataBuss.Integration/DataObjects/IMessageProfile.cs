using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IMessageProfile
	{
		SchemaFilterType? GetFilterType();

		Dictionary<string, IEnumerable<string>> GetFilterElements();

		bool IsValid();
	}
}
