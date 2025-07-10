using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Enterprise.Metadata.Integration;

namespace Enterprise.Metadata.Business
{
	public static class MetadataFactory
	{
		static readonly Lazy<ImmutableDictionary<MetadataContext, Type>> LazyMetadatas = new Lazy<ImmutableDictionary<MetadataContext, Type>>(InitializeMetadatas);

		public static ImmutableDictionary<MetadataContext, Type> Metadatas
		{
			get { return LazyMetadatas.Value; }
		}

		static ImmutableDictionary<MetadataContext, Type> InitializeMetadatas()
		{
			var metadatas = new Dictionary<MetadataContext, Type>();
			var assembly = Assembly.GetExecutingAssembly();

			foreach (var type in assembly.GetTypes())
			{
				if (IsClassOrSubclassOf(type, typeof(EnterpriseBusinessObject)))
				{
					var attributes = type.GetCustomAttributes(typeof(MetadataAttribute), false);

					if (attributes.Length > 0)
					{
						var attribute = attributes[0];
						var context = ((MetadataAttribute)attribute).Context;
						if (metadatas.ContainsKey(context))
						{
							throw new InvalidOperationException(string.Format("Context '{0}' already exists.", context.ToString()));
						}

						metadatas.Add(context, type);
					}
				}
			}

			return metadatas.ToImmutableDictionary();
		}

		static bool IsClassOrSubclassOf(Type type1, Type type2)
		{
			return type1 == type2 || type1.IsSubclassOf(type2);
		}
	}
}