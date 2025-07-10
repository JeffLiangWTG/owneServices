using System;

namespace Enterprise.Customs.Common
{
	/// <summary>
	/// Decorate business object properties or proxied properties with this to have the value wiped based on the condition property. 
	/// Specify a condition property in the attribute consumption for which value in property should be wipe from the opposite value of condition property.
	/// Specify no condition property in the attribute consumption will return false. 
	/// You can find an example about how to use this attribute in PurgeValueExceptHelper.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class PurgeValueExceptAttribute : Attribute
	{
		public PurgeValueExceptAttribute(string conditionPropertyName = "")
		{
			ConditionPropertyName = conditionPropertyName;
		}

		public string ConditionPropertyName { get; }
	}
}
