using System;

namespace CargoWise.Common
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class OperationActionReadOnlyMemberAttribute : Attribute
	{
		public string PropertyName { get; }

		public OperationActionReadOnlyMemberAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}
