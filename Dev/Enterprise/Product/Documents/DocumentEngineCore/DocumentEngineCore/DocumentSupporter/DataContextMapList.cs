using System;
using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public class DataContextMapList : List<DataContextMapList.MapElement>
	{
		public DataContextMapList(DocumentSupporter documentSupporter)
		{
			var result = new Dictionary<Type, MapElement>();

			foreach (var dataContextValue in documentSupporter.SupportedDataContexts)
			{
				var typeFromDataContext = GetTypeFromDataContext(dataContextValue);

				if (typeFromDataContext != null)
				{
					MapElement mapType;
					if (result.TryGetValue(typeFromDataContext, out mapType))
					{
						mapType.UpdateDataContextIdentifierIfShorter(dataContextValue.FullDataContext);
					}
					else
					{
						result.Add(typeFromDataContext, new MapElement(dataContextValue.FullDataContext, typeFromDataContext));
					}
				}
			}

			this.AddRange(result.Values);
			this.Sort(new MapElementComparer());
		}

		Type GetTypeFromDataContext(DataContextValue dataContextValue)
		{
			Type type = null;
			if (dataContextValue.IsBusinessObjectType)
			{
				type = dataContextValue.BusinessObjectType;
			}
			else
			{
				var genericWrapperLoader = GenericWrapperLoader.GetFromDataContext(dataContextValue.DataContext);
				if (genericWrapperLoader != null)
				{
					type = genericWrapperLoader.GetWrapperType();
				}
			}
			return type;
		}

		public class MapElement
		{
			public MapElement(string dataContextIdentifier, Type topLevelDataSourceType)
			{
				if (dataContextIdentifier == null)
				{
					throw new ArgumentNullException(nameof(dataContextIdentifier));
				}

				this.DataContextIdentifier = dataContextIdentifier;
				this.TopLevelDataSourceType = topLevelDataSourceType;
			}

			public string DataContextIdentifier
			{
				get;
				private set;
			}

			public readonly Type TopLevelDataSourceType;

			internal void UpdateDataContextIdentifierIfShorter(string dataContextIdentifier)
			{
				if (!string.IsNullOrEmpty(dataContextIdentifier) && dataContextIdentifier.Length < this.DataContextIdentifier.Length)
				{
					this.DataContextIdentifier = dataContextIdentifier;
				}
			}
		}

		class MapElementComparer : IComparer<MapElement>
		{
			public int Compare(MapElement x, MapElement y)
			{
				int result = x.DataContextIdentifier.StartsWith(".").CompareTo(y.DataContextIdentifier.StartsWith("."));
				if (result == 0)
				{
					result = x.DataContextIdentifier.CompareTo(y.DataContextIdentifier);
				}
				return result;
			}
		}
	}
}
