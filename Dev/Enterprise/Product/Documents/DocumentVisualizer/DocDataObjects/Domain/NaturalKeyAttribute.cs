using System;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class NaturalKeyAttribute : System.Attribute
	{
		public NaturalKeyAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; }
	}
}