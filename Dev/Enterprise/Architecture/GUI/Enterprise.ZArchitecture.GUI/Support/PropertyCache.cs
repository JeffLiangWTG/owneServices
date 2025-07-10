using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Enterprise.ZArchitecture.GUI
{
	public static class PropertyCache
	{
		static readonly ConcurrentDictionary<(Type, string), PropertyInfo> topProperties = new ConcurrentDictionary<(Type, string), PropertyInfo>();

		/// <summary>
		/// <para>Returns property of the given type with the given name.</para>
		/// <para>If there are more than one property with the given name, then returns property that is declared in closest type in the hierarchy.</para>
		/// <para>If there are no properties with the given name, then returns null.</para>
		/// </summary>
		public static PropertyInfo GetTopProperty(Type type, string propertyName)
		{
			var key = (type, propertyName);
			return topProperties.GetOrAdd(key, _ => GetTopPropertyNoCache(type, propertyName));
		}

		static PropertyInfo GetTopPropertyNoCache(Type type, string propertyName)
		{
			PropertyInfo result = null;

			try
			{
				result = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			}
			catch (AmbiguousMatchException)
			{
				while (result == null && type != null)
				{
					result = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
					type = type.BaseType;
				}
			}

			return result;
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Debugging")]
		internal static int ClearTopPropertiesCache()
		{
			var count = topProperties.Count;
			topProperties.Clear();
			return count;
		}
#endif
	}
}
