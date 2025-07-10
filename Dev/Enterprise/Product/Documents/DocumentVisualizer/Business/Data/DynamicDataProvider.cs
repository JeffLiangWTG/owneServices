using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class DynamicDataProvider : IDynamicDataProvider
	{
		public DynamicDataProvider(IBusiness parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
		}

		readonly IBusiness parent;
		readonly Dictionary<string, CacheValue> cache = new Dictionary<string, CacheValue>();

		public Try<IDynamicData> TryCreate(string dataContext, Guid pivotID, IDocDataObjectParameters parameters)
		{
			if (dataContext == DataContext.UXML
				&& cache.ContainsKey(dataContext))
			{
				var cacheValue = cache[dataContext];

				var result = cacheValue.Value.MakeDynamic(
					cacheValue.Manager.MetaDataProvider,
					cacheValue.Manager.ValidationProvider,
					cacheValue.Manager.TypeConverter,
					cacheValue.Manager.DynamicDataFactory);

				return Try<IDynamicData>.Success(result);
			}

			var res = parent.TryCreate(dataContext, parameters);

			if (!res.IsFaulted)
			{
				cache[dataContext] = new CacheValue(res.Value.Manager, res.Value.Value);
			}

			return res;
		}

		#region Nested Types

		sealed class CacheValue
		{
			public CacheValue(IDynamicDataManager manager, object value)
			{
				Manager = manager;
				Value = value;
			}

			public IDynamicDataManager Manager { get; }
			public object Value { get; }
		}

		#endregion
	}
}
