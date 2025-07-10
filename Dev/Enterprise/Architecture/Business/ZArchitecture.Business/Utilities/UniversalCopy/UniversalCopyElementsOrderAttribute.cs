using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopyElementsOrderAttribute : Attribute
	{
		public UniversalCopyElementsOrderAttribute(string firstElement, string secondElement)
		{
			FirstElement = firstElement;
			SecondElement = secondElement;
		}

		public string FirstElement { get; private set; }
		public string SecondElement { get; private set; }
	}
}
