using System;
using System.Reflection;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Provides weak reference to GLOW declaration model interface.
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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public sealed class GlowInterfaceReferenceAttribute : Attribute
	{
		public GlowInterfaceReferenceAttribute(string interfaceName)
		{
			InterfaceName = interfaceName;
		}

		public string InterfaceName { get; private set; }

		const string GlowModelInterfacesAssemblyName = "CargoWise.Glow.Model.Interfaces";
		const string GlowModelInterfacesNamespace = "CargoWise.Glow.Model.Interfaces.";

		/// <summary>
		/// Returns a GLOW interface type for the given CW1 type.
		/// </summary>
		/// <remarks>
		/// This method completely ignores <see cref="GlowDataDefinitionAttribute"/>.
		/// </remarks>
		public static Type GetGlowInterfaceFromType(Type componentType, bool tryToFindOutIfNotSpecified = false)
		{
			string interfaceName = GetDataDefinitionNameFromType(componentType, tryToFindOutIfNotSpecified);
			return GetGlowInterface(interfaceName);
		}

		internal static string GetDataDefinitionNameFromType(Type componentType, bool tryToFindOutIfNotSpecified = false)
		{
			if (componentType == null)
			{
				return null;
			}

			object[] attributes = componentType.GetCustomAttributes(typeof(GlowInterfaceReferenceAttribute), true);
			GlowInterfaceReferenceAttribute glowReferenceAttribute = attributes.Length > 0 ? attributes[0] as GlowInterfaceReferenceAttribute : null;

			return glowReferenceAttribute != null
				? glowReferenceAttribute.InterfaceName
				: tryToFindOutIfNotSpecified
					? CalcInterfaceNameFromType(componentType)
					: null;
		}

		static string CalcInterfaceNameFromType(Type componentType)
		{
			if (componentType.IsInterface)
			{
				return componentType.Name;
			}

			while (componentType != null && componentType != typeof(object) && !componentType.Name.StartsWith((NoResString)"Auto", StringComparison.Ordinal))
			{
				componentType = componentType.BaseType;
			}

			if (componentType != null && componentType.Name.StartsWith((NoResString)"Auto", StringComparison.Ordinal))
			{
				return "I" + Singularize(componentType.Name.Substring(4));
			}

			return null;
		}

		static string Singularize(string entityName)
		{
			#region SuppressResourceStringsCheckRegion

			if (!entityName.EndsWith("Address", StringComparison.Ordinal))
			{
				entityName = entityName.TrimEnd('s');
			}
			return entityName;

			#endregion
		}

		public static Type GetGlowInterface(string interfaceName)
		{
			if (!string.IsNullOrEmpty(interfaceName) && GlowModelAssembly != null)
			{
				return GlowModelAssembly.GetType(interfaceName, false) ?? GlowModelAssembly.GetType(GlowModelInterfacesNamespace + interfaceName, false);
			}
			return null;
		}

		static Assembly GlowModelAssembly
		{
			get { return glowModelAssembly ?? (glowModelAssembly = AssemblyLoader.LoadAssembly(GlowModelInterfacesAssemblyName)); }
		}
		[ThreadStatic]
		static Assembly glowModelAssembly;
	}
}
