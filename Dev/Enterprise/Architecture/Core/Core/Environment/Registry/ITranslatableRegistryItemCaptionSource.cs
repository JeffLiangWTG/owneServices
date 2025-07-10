using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface ITranslatableRegistryItemCaptionSource : IRegistryItemCaptionSource
	{
		IEnumerable<MultilingualString> GetMultilingualCaptions(object value);
	}
}
