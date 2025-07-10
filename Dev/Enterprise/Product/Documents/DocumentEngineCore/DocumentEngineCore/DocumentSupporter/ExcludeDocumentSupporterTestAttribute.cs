using System;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExcludeDocumentSupporterTestAttribute : Attribute
	{
		public ExcludeDocumentSupporterTestAttribute()
		{ }
	}
}
