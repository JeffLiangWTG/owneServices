using System;

namespace Enterprise.DocumentEngineIntegration.DocumentParsing
{
	/// <summary>
	/// Use this attribute to ignore the macro translation of the property and exclude it from Data Fields
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class DocumentMacroIgnoreAttribute : Attribute
	{
	}
}
