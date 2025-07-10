using System;
using NUnit.Framework;

namespace WTG.TestHelpers.IISExpress
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class RequiresIISExpressAttribute : DatCapabilityRequirementAttribute
	{
		public RequiresIISExpressAttribute()
			: base("IISEXPRESS10")
		{
		}
	}
}
