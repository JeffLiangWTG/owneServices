using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AppDomainWrappers.Net;
using CargoWise.Application.InversionOfControl;
using CargoWise.Common;

namespace CargoWise.Application
{
	static class DesignTimeObjectFactory
	{
		public static object GetDesignerSafe(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));

			object result = null;
#if DEBUG
			if (IsVisualStudio)
			{
				ObjectDefinition objectDefinition = GetObjectDefinition(name);
				if (objectDefinition != null && objectDefinition.TypeName != null)
				{
					int commaIndex = objectDefinition.TypeName.IndexOf(",", StringComparison.Ordinal);
					if (commaIndex != -1)
					{
						string typeName = objectDefinition.TypeName.Substring(0, commaIndex).Trim();
						string assemblyName = objectDefinition.TypeName.Substring(commaIndex + 1).Trim();
						Assembly assembly = LoadAssembly(assemblyName);
						if (assembly != null)
						{
							Type type = assembly.GetType(typeName);
							if (type != null)
							{
								result = objectDefinition.FactoryMethodName == null ? Activator.CreateInstance(type) : type.InvokeMember(objectDefinition.FactoryMethodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
							}
						}
					}
				}
			}
#endif
			return result ?? (result = ObjectFactory.GetWithoutSecurityCheck(name));
		}

		public static Type GetTypeDesignerSafe(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			Type result = null;
#if DEBUG
			if (IsVisualStudio)
			{
				ObjectDefinition definition = GetObjectDefinition(name);
				if (definition != null && definition.TypeName != null)
				{
					int commaIndex = definition.TypeName.IndexOf(",", StringComparison.Ordinal);
					if (commaIndex != -1)
					{
						string typeName = definition.TypeName.Substring(0, commaIndex).Trim();
						string assemblyName = definition.TypeName.Substring(commaIndex + 1).Trim();
						Assembly assembly = LoadAssembly(assemblyName);
						if (assembly != null)
						{
							result = assembly.GetType(typeName);
						}
					}
				}
			}
#endif
			return result ?? (result = ObjectFactory.GetTypeWithoutSecurityCheck(typeof(ObjectFactory.EmptyType), name));
		}

		static ObjectDefinition GetObjectDefinition(string name)
		{
			var objectDefinitions = ObjectFactory.GetObjectDefinitions();
			return objectDefinitions.FirstOrDefault(objectDefinition => String.Equals(objectDefinition.Name, name));
		}

		static Assembly LoadAssembly(string assemblyName)
		{
			var appDomainWrapper = new AppDomainWrapper();
			var assemblies = appDomainWrapper.GetAssemblies();
			return (from assembly in assemblies where assembly.GetName().Name == "CargoWise.Common" select assembly.GetType("CargoWise.Common.AssemblyLoader") into assemblyLoader select (Assembly)assemblyLoader.InvokeMember("LoadAssembly", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { assemblyName })).FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose name")]
		internal static bool IsVisualStudio
		{
			get
			{
				return isVisualStudio ?? (bool)(isVisualStudio = (Process.GetCurrentProcess().ProcessName == "devenv"));
			}
			set
			{
				isVisualStudio = value;
			}
		}

		[ThreadStatic]
		static bool? isVisualStudio;
	}
}
