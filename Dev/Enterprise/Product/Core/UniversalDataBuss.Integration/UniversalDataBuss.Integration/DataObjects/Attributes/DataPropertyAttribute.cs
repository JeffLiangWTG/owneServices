using System;

namespace Enterprise.UniversalDataBuss.Integration
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class DataPropertyAttribute : Attribute
	{
	}
}
