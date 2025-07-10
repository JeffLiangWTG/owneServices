using System;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[AttributeUsage(AttributeTargets.Method)]
	sealed class ExemptFromDateSetInRelevantTestListenerAttribute : Attribute
	{
		public ExemptFromDateSetInRelevantTestListenerAttribute() { }
	}
}
