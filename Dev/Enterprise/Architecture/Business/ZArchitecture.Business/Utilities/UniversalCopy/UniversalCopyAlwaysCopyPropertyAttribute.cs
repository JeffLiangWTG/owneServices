using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyAlwaysCopyPropertyAttribute : Attribute
	{
		public enum CopyMode
		{
			AtTheBeginning,
			AtTheEnd,
		}

		public UniversalCopyAlwaysCopyPropertyAttribute(CopyMode mode)
		{
			Mode = mode;
		}

		public CopyMode Mode { get; private set; }
	}
}
