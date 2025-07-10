using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class UniversalCopyInstanceTypeAttribute : Attribute
	{
		public Type InstanceType { get; set; }
		public string CreationMethod { get; set; }
		public string GetSourceMethod { get; set; }
		public bool ShouldSyncTreeNodes { get; set; }
	}
}
