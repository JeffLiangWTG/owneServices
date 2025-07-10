using System;
using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class FlattenedIntoAttributesAttribute : Attribute
	{
		public FlattenedIntoAttributesAttribute(string baseElementPropertyName, bool flattenEvenWithOldNamespace = false)
		{
			this.BaseElementPropertyName = Argument.NotNull(baseElementPropertyName, "string baseElementPropertyName");
			this.FlattenEvenWithOldNamespace = Argument.NotNull(flattenEvenWithOldNamespace, "bool flattenEvenWithOldNamespace");
		}

		public string BaseElementPropertyName { get; private set; }
		public bool FlattenEvenWithOldNamespace { get; private set; }
	}
}
