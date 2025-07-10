using System.Collections.Generic;

namespace Enterprise.Messaging.Integration
{
	public interface IxTMessageAttributeProvider
	{
		Dictionary<string, string> GetMessageAttrDictionary();
	}
}
