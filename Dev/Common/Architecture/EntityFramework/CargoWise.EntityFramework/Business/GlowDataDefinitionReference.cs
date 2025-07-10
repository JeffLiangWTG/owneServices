using System;
using System.Linq;

namespace CargoWise.EntityFramework
{
	public sealed class GlowDataDefinitionReference
	{
		public GlowDataDefinitionReference(string dataDefinitionName)
		{
			DataDefinitionName = dataDefinitionName;
		}

		public string DataDefinitionName { get; }

		/// <summary>
		/// Returns information about GLOW data definition for the given CW1 type.
		/// </summary>
		/// <remarks>
		/// <para> This method supports both <see cref="GlowDataDefinitionAttribute"/> and <see cref="GlowInterfaceReferenceAttribute"/>.</para>
		/// <para><see cref="GlowDataDefinitionAttribute"/> has a higher priority than <see cref="GlowInterfaceReferenceAttribute"/>.</para>
		/// </remarks>
		public static GlowDataDefinitionReference FromType(Type componentType)
		{
			if (componentType == null)
			{
				return null;
			}

			// there can only be one attribute since AllowMultiple is set to false for GlowDataDefinitionAttribute
			var attribute = componentType.GetCustomAttributes(typeof(GlowDataDefinitionAttribute), true).Cast<GlowDataDefinitionAttribute>().FirstOrDefault();
			if (attribute != null)
			{
				return new GlowDataDefinitionReference(attribute.DataDefinitionName);
			}

			var interfaceName = GlowInterfaceReferenceAttribute.GetDataDefinitionNameFromType(componentType, tryToFindOutIfNotSpecified: true);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterface(interfaceName);
			if (interfaceType != null)
			{
				return new GlowDataDefinitionReference(interfaceName);
			}

			return null;
		}
	}
}
