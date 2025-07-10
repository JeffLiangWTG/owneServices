using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class MetaDataProviderForLogRelatedTest : IMetaDataProvider
	{
		public object GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
		{
			if (metaDataType == MetaDataType.Identifier)
			{
				var bizObj = dynamicData.Value as BusinessObject;

				return bizObj != null
					? bizObj.PK
					: ZGuid.Empty;
			}

			return null;
		}
	}
}
