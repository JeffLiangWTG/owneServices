using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaContainerTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var headerPK = new ZGuid(row[AsycudaContainer.Schema.ACN_AMA_Manifest]);
				var header = headerPK.IsValid ? factory.Load<AsycudaManifestHeader>(headerPK) : null;
				if (header != null)
				{
					result = header.GetContainerType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaContainer);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaContainer);
		}
	}
}

