using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IRegistryItemCaptionSource : ICustomizableDataCaptionSource
	{
		bool IsTranslatable { get; }

		IEnumerable<ResourceString> DefaultStrings { get; }

		IEnumerable<string> GetCaptions(object value);
	}
}
