using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.Integration;
using MetaDataType = Enterprise.DocumentVisualizer.Core.MetaDataType;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UXmlMetaDataProvider : IMetaDataProvider
	{
		public UXmlMetaDataProvider(UXmlLinkManager linkManager)
		{
			Argument.NotNull(linkManager, nameof(linkManager));

			this.linkManager = linkManager;
			this.schema = new UXmlSchema();
		}

		readonly UXmlSchema schema;
		readonly UXmlLinkManager linkManager;

		#region IMetaDataProvider members

		object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
		{
			if (dynamicData == null)
			{
				return null;
			}

			object metaData = null;

			switch (metaDataType)
			{
				case MetaDataType.Identifier:
					metaData = GetIdentifier(dynamicData);
					break;

				case MetaDataType.NaturalKey:
					metaData = GetNaturalKeyForCollection(dynamicData as IDynamicDataCollection);
					break;

				case MetaDataType.MaxLength:
					metaData = dynamicData.GetMaxLength();
					break;
			}

			return metaData;
		}

		object GetIdentifier(IDynamicData dynamicData)
		{
			var dataObject = dynamicData.Value as IDataObject;

			if (dataObject == null)
			{
				return null;
			}

			var pk = linkManager.GetPK(dataObject);

			return pk.IsValid
				? pk
				: null;
		}

		object GetNaturalKeyForCollection(IDynamicDataCollection collection)
		{
			if (collection != null && schema.NaturalKeys.ContainsKey(collection.ElementType))
			{
				return schema.NaturalKeys[collection.ElementType];
			}

			return null;
		}

		#endregion
	}
}
