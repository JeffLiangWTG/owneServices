using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackedItemTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var billPK = new ZGuid(row[AsycudaPackedItem.Schema.API_ABL_Bill]);
				var bill = billPK.IsValid ? factory.Load<AsycudaBill>(billPK) : null;
				if (bill != null)
				{
					result = bill.GetPackedItemType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaPackedItem);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaPackedItem);
		}
	}
}
