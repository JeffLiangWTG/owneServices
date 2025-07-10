using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class UniversalCopyExtraMetadataAttribute : Attribute
	{
		public bool IsMandatory { get; set; }
		public string HumanReadableName { get; set; }
		public int Priority { get; set; }
	}
}
