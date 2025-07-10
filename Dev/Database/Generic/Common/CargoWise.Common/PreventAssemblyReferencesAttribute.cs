using System;

namespace CargoWise.Common
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class PreventAssemblyReferencesAttribute : Attribute
	{
		public PreventAssemblyReferencesAttribute()
		{
		}

		public PreventAssemblyReferencesAttribute(params string[] allowedReferencePartialPaths)
		{
			AllowedReferencePartialPaths = allowedReferencePartialPaths;
		}

		public string[] AllowedReferencePartialPaths { get; set; }
		public string[] AllowedDebugOnlyReferencePartialPaths { get; set; }
	}
}
