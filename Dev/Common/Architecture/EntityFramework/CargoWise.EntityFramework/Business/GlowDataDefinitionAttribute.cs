using System;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Specifies name of a GLOW data definition for a class.
	/// </summary>
	/// <remarks>
	/// <para>There are two attributes that can define references to the GLOW model.</para>
	/// <list type="bullet">
	/// <item><description>
	/// <see cref="GlowInterfaceReferenceAttribute"/> - is supported by all existing code, but only supports types that are mapped to a GLOW interface.
	/// </description></item>
	/// <item><description>
	/// <see cref="GlowDataDefinitionAttribute"/> - supports both types that are mapped to a GLOW interface and types mapped to data-definitions from extensions.
	/// This attribute is ignored by code that is not upgraded to work with extensions.
	/// </description></item>
	/// </list>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class GlowDataDefinitionAttribute : Attribute
	{
		public GlowDataDefinitionAttribute(string dataDefinitionName)
		{
			DataDefinitionName = dataDefinitionName;
		}

		public string DataDefinitionName { get; }
	}
}
