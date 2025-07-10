using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyClearCollectionOnCopyAttribute : Attribute
	{
		public bool ClearAfterAllElementsWereCopied { get; set; }
	}
}
