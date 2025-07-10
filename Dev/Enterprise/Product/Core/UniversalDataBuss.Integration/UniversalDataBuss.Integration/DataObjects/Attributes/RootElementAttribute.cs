using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RootElementAttribute : Attribute
	{
		public RootElementAttribute(string rootElementName)
		{
			this.RootElementName = rootElementName;
		}

		public string RootElementName { get; private set; }
	}
}
