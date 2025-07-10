using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class MetaDataProviderForTest : IMetaDataProvider
	{
		public Func<IDynamicData, MetaDataType, object> GetMetaDataImplementer { get; set; }

		object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
		{
			return GetMetaDataImplementer != null
				? GetMetaDataImplementer(dynamicData, metaDataType)
				: null;
		}
	}
}