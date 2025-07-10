using System;

namespace Enterprise.ResourceStrings.Business
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class CodeStringFinderHintAttribute : Attribute
	{
		public CodeStringFinderHintAttribute(Type type, string methodName)
		{
		}
	}
}
