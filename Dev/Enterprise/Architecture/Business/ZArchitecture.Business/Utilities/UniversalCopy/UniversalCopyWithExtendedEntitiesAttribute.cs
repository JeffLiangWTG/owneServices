using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyWithExtendedEntitiesAttribute : Attribute
	{
		public string StartCopyMethod { get; set; }
		public string FinishCopyMethod { get; set; }
		public bool IgnoreAllElementsExceptSpecificallyMarked { get; set; }
	}
}
