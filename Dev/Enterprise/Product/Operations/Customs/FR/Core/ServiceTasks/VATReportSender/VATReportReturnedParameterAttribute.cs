using System;
using System.Runtime.CompilerServices;

namespace Enterprise.Customs.FR.ServiceTasks
{
	[AttributeUsage(AttributeTargets.Property)]
	sealed class VATReportReturnedParameterAttribute : Attribute
	{
		public VATReportReturnedParameterAttribute([CallerLineNumber] int order = 0)
		{
			Order = order;
		}

		public int Order { get; }
	}
}
