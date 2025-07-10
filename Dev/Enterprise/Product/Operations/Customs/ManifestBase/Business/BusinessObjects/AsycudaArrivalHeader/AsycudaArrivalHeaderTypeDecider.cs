using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaArrivalHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var headerPK = new ZGuid(row[AsycudaArrivalHeader.Schema.ATH_AMA_ManifestHeader]);
				var header = headerPK.IsValid ? factory.Load<AsycudaManifestHeader>(headerPK) : null;
				if (header != null)
				{
					result = header.GetArrivalHeaderType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaArrivalHeader);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaArrivalHeader);
		}
	}
}

