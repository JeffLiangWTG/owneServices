using System;

namespace Enterprise.Integration
{
	public class RootTypeProviderAttribute : Attribute
	{
		public RootTypeProviderAttribute(string typeProviderFunctionName, string rootProviderFunctionName)
		{
			TypeProviderFunctionName = typeProviderFunctionName ?? throw new ArgumentNullException(nameof(typeProviderFunctionName));
			RootProviderFunctionName = rootProviderFunctionName ?? throw new ArgumentNullException(nameof(rootProviderFunctionName));
		}

		public string TypeProviderFunctionName { get; }
		public string RootProviderFunctionName { get; }
	}
}
