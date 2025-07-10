using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public sealed class BindToAttribute : Attribute
	{
		public BindToAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}