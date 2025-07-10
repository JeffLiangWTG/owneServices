using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation
{
	public class SubClassRetriever
	{
		public SubClassRetriever(string enterpriseBuildPath, string[] assemblies, Type typeToFind)
		{
			this.assemblies = assemblies;
			this.TypeToFind = typeToFind;
			this.enterpriseBuildPath = enterpriseBuildPath;
		}

		public Type[] Retrieve()
		{
			List<Type> result = new List<Type>();
			foreach (Assembly assembly in GetAssemblies())
			{
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (PassesSubclassCondition(type) &&
							PassesAbstractCondition(type) &&
							PassesNestedClassCondition(type) &&
							PassesTestClassesCondition(type) &&
							PassesExclusionAttributeCondition(type))
						{
							result.Add(type);
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					throw new Exception(BuildReflectionTypeLoadExceptionMessage(assembly.FullName, ex));
				}
				catch (Exception ex)
				{
					throw new Exception("Exception occured while retrieving classes from assembly " + assembly.FullName + ".", ex);
				}
			}
			return result.ToArray();
		}

		bool PassesSubclassCondition(Type type)
		{
			return type.FullName != "<PrivateImplementationDetails>" && type.IsSubclassOf(TypeToFind);
		}

		bool PassesAbstractCondition(Type type)
		{
			return IncludeAbstractClasses || !type.IsAbstract;
		}

		bool PassesNestedClassCondition(Type type)
		{
			return IncludeNestedClasses || type.FullName.IndexOf('+') == -1;
		}

		bool PassesTestClassesCondition(Type type)
		{
			return IncludeTestClasses || (type.FullName.IndexOf(".Test") == -1 && type.FullName.IndexOf("+Test") == -1);
		}

		bool PassesExclusionAttributeCondition(Type type)
		{
			if (ExcludedAttributes != null)
			{
				foreach (Type attributeType in ExcludedAttributes)
				{
					if (type.GetCustomAttributes(attributeType, false).Length != 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		Assembly[] GetAssemblies()
		{
			List<Assembly> result = new List<Assembly>();
			foreach (string assembly in assemblies)
			{
				string assemblyPrefix = Path.Combine(enterpriseBuildPath, assembly);
				Assembly assemblyToAdd;

				try
				{
					if (File.Exists(assemblyPrefix + ".DLL"))
					{
						assemblyToAdd = Assembly.LoadFrom(assemblyPrefix + ".DLL");
					}
					else if (File.Exists(assemblyPrefix + ".EXE"))
					{
						assemblyToAdd = Assembly.LoadFrom(assemblyPrefix + ".EXE");
					}
					else
					{
						throw new FileNotFoundException("Could not find file or assembly '" + assembly + "' in '" + enterpriseBuildPath + "'.");
					}
					result.Add(assemblyToAdd);
				}
				catch (BadImageFormatException)
				{
				}
			}
			return result.ToArray();
		}

		string BuildReflectionTypeLoadExceptionMessage(string assemblyFullName, ReflectionTypeLoadException ex)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("Exception occured while retrieving classes from assembly ");
			builder.Append(assemblyFullName);
			builder.AppendLine(".");
			builder.AppendLine();
			builder.AppendLine(ex.ToString());

			for (int i = 0; i < ex.LoaderExceptions.Length; i++)
			{
				builder.AppendLine();
				builder.Append("LoaderExceptions[");
				builder.Append(i);
				builder.AppendLine("]:");
				builder.AppendLine();
				builder.Append(ex.LoaderExceptions[i].ToString());
			}

			return builder.ToString();
		}

		public bool IncludeAbstractClasses
		{
			get { return includeAbstractClasses; }
			set { includeAbstractClasses = value; }
		}

		public bool IncludeNestedClasses
		{
			get { return includeNestedClasses; }
			set { includeNestedClasses = value; }
		}

		public bool IncludeTestClasses
		{
			get { return includeTestClasses; }
			set { includeTestClasses = value; }
		}

		public Type[] ExcludedAttributes
		{
			get { return excludedAttributes; }
			set { excludedAttributes = value; }
		}

		Type TypeToFind
		{
			get { return typeToFind; }
			set
			{
				typeToFind = value;
				IncludeTestClasses = value.IsSubclassOf(typeof(TestCase)) || value == typeof(TestCase);
			}
		}

		readonly string[] assemblies;
		readonly string enterpriseBuildPath;
		bool includeAbstractClasses = true;
		bool includeNestedClasses = true;
		bool includeTestClasses;
		Type typeToFind;
		Type[] excludedAttributes;
	}
}
