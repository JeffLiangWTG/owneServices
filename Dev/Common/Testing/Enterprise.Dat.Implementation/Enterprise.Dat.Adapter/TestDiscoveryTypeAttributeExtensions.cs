using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation
{
	static class TestDiscoveryTypeAttributeExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly ConcurrentDictionary<string, List<Attribute>> attributeCache = new ConcurrentDictionary<string, List<Attribute>>();
		internal static IEnumerable<T> GetCustomAttributesCached<T>(this Type type, bool inherit = true) where T : Attribute
		{
			var cacheKey = $"Type:{type.FullName} For:{typeof(T).FullName}";
			return attributeCache.GetOrAdd
			(
				cacheKey,
				_ => CustomAttributeExtensions.GetCustomAttributes<T>(type, inherit).OfType<Attribute>().ToList()
			)
			.OfType<T>();
		}

		static T GetCustomAttributeCached<T>(this Type type) where T : Attribute
		{
			return type.GetCustomAttributesCached<T>(true).FirstOrDefault();
		}

		internal static IEnumerable<T> GetCustomAttributesCached<T>(this Assembly assembly) where T : Attribute
		{
			var cacheKey = $"Assembly:{assembly.FullName} For:{typeof(T).FullName}";
			return attributeCache.GetOrAdd
			(
				cacheKey,
				_ => CustomAttributeExtensions.GetCustomAttributes<T>(assembly).OfType<Attribute>().ToList()
			)
			.OfType<T>();
		}

		internal static T GetCustomAttributeCached<T>(this Assembly assembly) where T : Attribute
		{
			return GetCustomAttributesCached<T>(assembly).FirstOrDefault();
		}

		internal static TargetFrameworksAttribute GetTargetFrameworksAttribute(this Type testCaseType, MethodInfo method)
		{
			return method.GetCustomAttribute<TargetFrameworksAttribute>()
				?? testCaseType.GetCustomAttributeCached<TargetFrameworksAttribute>()
				?? testCaseType.Assembly.GetCustomAttributeCached<TargetFrameworksAttribute>();
		}

		internal static bool HasAttribute<TAttribute>(this Type t, MethodInfo m) where TAttribute : Attribute
		{
			var usage = GetAttributeUsages<TAttribute>();

			if (usage.HasFlag(AttributeTargets.Class) && GetCustomAttributeCached<TAttribute>(t) != null)
			{
				return true;
			}

			if (m != null
				&& usage.HasFlag(AttributeTargets.Method)
				&& Attribute.IsDefined(m, typeof(TAttribute), true))
			{
				return true;
			}

			return usage.HasFlag(AttributeTargets.Assembly) && t.Assembly.GetCustomAttributeCached<TAttribute>() != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly ConcurrentDictionary<string, AttributeTargets> attributeUsagesCache = new ConcurrentDictionary<string, AttributeTargets>();
		static AttributeTargets GetAttributeUsages<T>() where T : Attribute
		{
			var key = typeof(T).FullName;
			return attributeUsagesCache.GetOrAdd(key, _ =>
			{
				var usageAttribute = typeof(T).GetCustomAttributeCached<AttributeUsageAttribute>();
				if (usageAttribute == null)
				{
					return AttributeTargets.All;
				}

				return usageAttribute.ValidOn;
			});
		}
	}
}
