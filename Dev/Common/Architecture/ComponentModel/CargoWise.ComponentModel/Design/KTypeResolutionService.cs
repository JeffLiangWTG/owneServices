using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A wrapper around the design time ITypeResolutionService interface, with some additional behaviour.
	/// </summary>
	public class KTypeResolutionService : ITypeResolutionService
	{
		public KTypeResolutionService(IServiceProvider serviceProvider, ITypeResolutionService inner)
		{
			Argument.NotNull(serviceProvider, nameof(serviceProvider));
			Argument.NotNull(inner, nameof(inner));
			ServiceProvider = serviceProvider;
			Inner = inner;
		}

		public ITypeResolutionService Inner { get; private set; }

		#region GetType

		public Type GetType(string name)
		{
			return GetType(name, false);
		}

		public Type GetType(string name, bool throwOnError)
		{
			return GetType(name, throwOnError, false);
		}

		public Type GetType(string name, bool throwOnError, bool ignoreCase)
		{
			var result = GetTypeFromAdditionalTypeResolutionAssemblies(name, ignoreCase) ?? Inner.GetType(name, false, ignoreCase);
			if (result == null && throwOnError)
			{
				throw new TypeLoadException("Could not find type '" + name + "'");
			}
			return result;
		}

		Type GetTypeFromAdditionalTypeResolutionAssemblies(string name, bool ignoreCase)
		{
			if (!string.IsNullOrEmpty(name))
			{
				var commaIndex = name.IndexOf(',');
				var typeName = (commaIndex == -1) ? name : name.Substring(0, commaIndex);
				foreach (var assembly in additionalAssemblies)
				{
					var type = assembly.GetType(typeName, false, ignoreCase);
					if (type != null)
					{
						return type;
					}
				}
			}
			return null;
		}

		#endregion

		#region GetAssembly / GetPathOfAssembly / ReferenceAssembly

		public Assembly GetAssembly(AssemblyName name)
		{
			return GetAssembly(name, false);
		}

		public Assembly GetAssembly(AssemblyName name, bool throwOnError)
		{
			Assembly result = null;
			if (name != null)
			{
				foreach (var assembly in additionalAssemblies)
				{
					if (assembly.GetName().Name == name.Name)
					{
						result = assembly;
					}
				}
			}
			if (result == null)
			{
				result = Inner.GetAssembly(name, throwOnError);
			}
			return result;
		}

		public string GetPathOfAssembly(AssemblyName name)
		{
			return Inner.GetPathOfAssembly(name);
		}

		public void ReferenceAssembly(AssemblyName name)
		{
			Inner.ReferenceAssembly(name);
		}

		#endregion

		#region LoadAssemblyFrom

		public Assembly LoadAssemblyFrom(string path)
		{
			return DynamicTypeServiceType != null ? (Assembly)DynamicTypeServiceType.InvokeMember("CreateDynamicAssembly", BindingFlags.InvokeMethod, null, DynamicTypeService, new object[] { path }, CultureInfo.InvariantCulture) : null;
		}

		object DynamicTypeService
		{
			get { return ServiceProvider.GetService(DynamicTypeServiceType); }
		}

		protected virtual Type DynamicTypeServiceType
		{
			get
			{
				if (dynamicTypeServiceType == null)
				{
					var shellDesignAssembly = Assembly.Load("Microsoft.VisualStudio.Shell.Design");
					if (shellDesignAssembly != null)
					{
						dynamicTypeServiceType = shellDesignAssembly.GetType("Microsoft.VisualStudio.Shell.Design.DynamicTypeService");
					}
				}
				return dynamicTypeServiceType;
			}
		}
		Type dynamicTypeServiceType;

		#endregion

		#region AddAssembly

		public void AddAssembly(Assembly assembly)
		{
			Argument.NotNull(assembly, nameof(assembly));
			additionalAssemblies.Add(assembly);
		}
		readonly List<Assembly> additionalAssemblies = new List<Assembly>();

		#endregion

		#region Implementation

		readonly IServiceProvider ServiceProvider;

		#endregion
	}
}
