using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// An enumerator over a set of Types.
	/// </summary>
	public class TypeEnumerable : IEnumerable<Type>
	{
		public TypeEnumerable(bool includePrivate, params Assembly[] assemblies)
			: this(includePrivate, false, assemblies)
		{
		}

		public TypeEnumerable(bool includePrivate, bool cacheTypes, params Assembly[] assemblies)
		{
			this.includePrivate = includePrivate;
			this.assemblies = assemblies;
			this.cacheTypes = cacheTypes;
		}

		#region IEnumerable Members

		public IEnumerator<Type> GetEnumerator()
		{
			foreach (Assembly assembly in assemblies)
			{
				foreach (Type next in GetAssemblyTypes(assembly, includePrivate, cacheTypes))
				{
					yield return next;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (Type type in this)
			{
				yield return type;
			}
		}

		#endregion

		#region Implementation

		readonly bool includePrivate;
		readonly bool cacheTypes;
		readonly Assembly[] assemblies;

		static Type[] GetAssemblyTypes(Assembly assembly, bool includePrivate, bool cacheResult)
		{
			string cacheKey = assembly.FullName + "_" + includePrivate;
			CachedAssemblyTypes.TryGetValue(cacheKey, out var result);
			CachedAssemblyModuleCounts.TryGetValue(assembly, out var moduleCount);

			if (result == null ||
				moduleCount == 0 || moduleCount != assembly.GetModules().Length)
			{
				try
				{
					if (includePrivate)
					{
						result = assembly.GetTypes();
					}
					else
					{
						result = GetExportedTypes(assembly);
					}
				}
				catch (TypeLoadException)
				{
					result = Array.Empty<Type>();
				}
				catch (ReflectionTypeLoadException e)
				{
					result = e.Types;
				}
				if (cacheResult)
				{
					CachedAssemblyTypes[cacheKey] = result;
					CachedAssemblyModuleCounts[assembly] = assembly.GetModules().Length;
				}
			}
			return result;
		}

		static Type[] GetExportedTypes(Assembly ass)
		{
			Type[] result;
			if (!(ass is AssemblyBuilder))
			{
				result = ass.GetExportedTypes();
			}
			else
			{
				List<Type> onlyPrivate = new List<Type>();
				foreach (Type type in ass.GetTypes())
				{
					if (type.IsVisible)
					{
						onlyPrivate.Add(type);
					}
				}
				result = onlyPrivate.ToArray();
			}
			return result;
		}

		static Dictionary<Assembly, int> CachedAssemblyModuleCounts
		{
			get { return cachedAssemblyModuleCounts ?? (cachedAssemblyModuleCounts = new Dictionary<Assembly, int>()); }
		}
		[ThreadStatic]
		static Dictionary<Assembly, int> cachedAssemblyModuleCounts;

		static Dictionary<string, Type[]> CachedAssemblyTypes
		{
			get { return cachedAssemblyTypes ?? (cachedAssemblyTypes = new Dictionary<string, Type[]>()); }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1022:ThreadStaticSetInStaticInitializerRule", Justification = "Baseline issue")]
		[ThreadStatic]
		static Dictionary<string, Type[]> cachedAssemblyTypes = new Dictionary<string, Type[]>();

		#endregion
	}
}
