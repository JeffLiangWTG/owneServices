using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Attribute To exclude property from being tested by PersistentBusinessObjectTestCase
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class SettingInvalidDateOnDateTimePropertyTestExclude : Attribute
	{
	}
}
