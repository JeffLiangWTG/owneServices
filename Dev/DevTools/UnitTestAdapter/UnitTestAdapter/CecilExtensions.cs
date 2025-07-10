using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Mono.Cecil;

namespace CWNUnit.TestAdapter
{
	public static class CecilExtensions
	{
		public static TypeDefinition GetMonoCecilTypeDefinition(Type type, AssemblyDefinition assemblyDefinition = null)
		{
			if (assemblyDefinition == null)
			{
				if (!AssemblyDefinitionCache.TryGetValue(type.Assembly.FullName, out assemblyDefinition) && !string.IsNullOrEmpty(type.Assembly.Location))
				{
					try
					{
						assemblyDefinition = AssemblyDefinition.ReadAssembly(type.Assembly.Location, new ReaderParameters { ReadSymbols = true });
					}
					catch
					{
					}
				}

				if (assemblyDefinition != null)
				{
					AssemblyDefinitionCache[type.Assembly.FullName] = assemblyDefinition;
				}
				else
				{
					return null;
				}
			}

			var typeDefinition = assemblyDefinition.Modules.Select(m => m.GetType(type.FullName)).FirstOrDefault(t => t != null);
			if (typeDefinition == null && type.FullName.IndexOf('+') >= 0)
			{
				var subtypes = type.FullName.Split('+');
				typeDefinition = assemblyDefinition.MainModule.GetType(subtypes[0]);
				for (int i = 1; i < subtypes.Length && typeDefinition != null; i++)
				{
					typeDefinition = typeDefinition.NestedTypes.FirstOrDefault(t => t.Name == subtypes[i]);
				}
			}

			return typeDefinition;
		}

		public static MethodDefinition GetMonoCecilMethodDefinition(MethodInfo method, TypeDefinition typeDefinition = null)
		{
			if (typeDefinition == null)
			{
				typeDefinition = GetMonoCecilTypeDefinition(method.DeclaringType);

				if (typeDefinition == null)
				{
					return null;
				}
			}

			var methodDefinition = typeDefinition.Methods.FirstOrDefault(m =>
				m.Name == method.Name &&
				!m.IsAbstract &&
				!m.IsStatic &&
				!m.ContainsGenericParameter &&
				m.Parameters.Count == method.GetParameters().Length);

			return methodDefinition;
		}

		public static MethodDefinition GetMonoCecilMethodDefinition(string assemblyFileName, string typeName, string methodName)
		{
			MethodInfo methodInfo = null;

			try
			{
				if (!AssemblyFileCache.TryGetValue(assemblyFileName, out var assembly))
				{
					// Need to load Assembly and use Reflection, because need to get DeclaringType of the test method, which is not specified in TestDescriptor.
					assembly = Assembly.LoadFile(assemblyFileName);
					AssemblyFileCache.Add(assemblyFileName, assembly);
				}

				methodInfo = assembly
					.GetType(typeName)
					?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			if (methodInfo != null)
			{
				return GetMonoCecilMethodDefinition(methodInfo);
			}

			return null;
		}

		public static void ClearCache()
		{
			assemblyDefinitionCache = null;
		}

		static Dictionary<string, AssemblyDefinition> AssemblyDefinitionCache =>
			assemblyDefinitionCache ?? (assemblyDefinitionCache = new Dictionary<string, AssemblyDefinition>());

		[ThreadStatic]
		static Dictionary<string, AssemblyDefinition> assemblyDefinitionCache;

		static Dictionary<string, Assembly> AssemblyFileCache =>
			assemblyFileCache ?? (assemblyFileCache = new Dictionary<string, Assembly>());

		[ThreadStatic]
		static Dictionary<string, Assembly> assemblyFileCache;
	}
}
