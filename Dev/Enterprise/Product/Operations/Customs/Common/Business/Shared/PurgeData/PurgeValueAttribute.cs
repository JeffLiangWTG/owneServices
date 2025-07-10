using System;

namespace Enterprise.Customs.Common
{
	/// <summary>
	/// Decorate business object properties or proxied properties with this to have the value wiped based on the condition property. 
	/// Specify a condition property in the attribute consumption for which value in property should be wipe from the value of condition property.
	/// Specify no condition property in the attribute consumption will return true. 
	/// You can find an example about how to use this attribute in PurgeValueHelper.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class PurgeValueAttribute : Attribute
	{
		public PurgeValueAttribute(string conditionPropertyName = "")
		{
			ConditionPropertyName = conditionPropertyName;
		}

		public string ConditionPropertyName { get; }
	}
}
