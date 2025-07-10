using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// An enumerator for sub-classes of a given type.
	/// </summary>
	public class SubtypeEnumerable : FilteredTypeEnumerable
	{
		readonly string ns = "";

		public SubtypeEnumerable(IEnumerable inner, params Type[] baseTypes)
			: base(inner)
		{ this.baseTypes = baseTypes; }

		public SubtypeEnumerable(bool includePrivate, Assembly[] assemblies, params Type[] baseTypes)
			: this(new TypeEnumerable(includePrivate, ReduceAssembliesForOptimisation(assemblies, baseTypes)), baseTypes)
		{
		}

		public SubtypeEnumerable(bool includePrivate, Assembly[] assemblies, Type baseType, string ns)
			: this(includePrivate, assemblies, new Type[] { baseType }, ns)
		{
		}

		public SubtypeEnumerable(bool includePrivate, Assembly[] assemblies, Type[] baseTypes, string ns)
			: this(includePrivate, assemblies, baseTypes)
		{ this.ns = ns; }

		/// <summary>
		/// Get the base type whose sub-classes will be enumerated on.
		/// </summary>
		public Type[] BaseTypes
		{ get { return (Type[])baseTypes.Clone(); } }

		readonly Type[] baseTypes;

		protected override bool MatchesFilter(Type type)
		{
			bool result = (string.IsNullOrEmpty(ns) || type.Namespace == ns);
			foreach (Type baseType in BaseTypes)
			{
				result =
					result &&
					baseType.IsAssignableFrom(type) &&
					type != baseType;
			}
			return result;
		}

		static Assembly[] ReduceAssembliesForOptimisation(Assembly[] assemblies, Type[] baseTypes)
		{
			Assembly[] result = assemblies;
			bool isABaseNonSystemType = false;
			foreach (Type baseType in baseTypes)
			{
				if (ReflectionUtil.IsMaybeNonSystemAssemblyName(baseType.Assembly.FullName))
				{
					isABaseNonSystemType = true;
					break;
				}
			}
			if (isABaseNonSystemType)
			{
				result = GetMostlyNonSystemAssemblies(result);
			}
			return result;
		}

		static Assembly[] GetMostlyNonSystemAssemblies(Assembly[] assemblies)
		{
			List<Assembly> result = new List<Assembly>();
			foreach (Assembly assembly in assemblies)
			{
				if (ReflectionUtil.IsMaybeNonSystemAssemblyName(assembly.FullName))
				{
					result.Add(assembly);
				}
			}
			return result.ToArray();
		}
	}
}
