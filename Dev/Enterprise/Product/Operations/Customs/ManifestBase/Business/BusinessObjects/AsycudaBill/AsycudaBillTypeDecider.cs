using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var headerPK = new ZGuid(row[AsycudaBill.Schema.ABL_AMA]);
				var header = headerPK.IsValid ? factory.Load<AsycudaManifestHeader>(headerPK) : null;
				if (header != null)
				{
					result = header.GetBillType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaBill);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaBill);
		}
	}
}

