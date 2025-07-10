using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface IGlowUrlProvider
	{
		Uri TryGenerateUrl(string endpoint);
		Uri TryGenerateUrl(string endpoint, string jobDescription);
		Uri TryGenerateUrl(string endpoint, string jobDescription, IEnumerable<(string Name, string Value)> additionalQueryStrings);
	}
}
