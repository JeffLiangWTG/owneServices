using System;

namespace Enterprise.DocumentEngineIntegration.DocumentParsing
{
	/// <summary>
	/// Use this attribute to exclude the property from Data Fields
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DocumentFieldExcludeFromMapAttribute : Attribute
	{
	}
}
