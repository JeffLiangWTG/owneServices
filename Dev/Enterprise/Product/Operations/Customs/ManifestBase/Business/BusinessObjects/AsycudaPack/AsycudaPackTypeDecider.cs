using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var billPK = new ZGuid(row[AsycudaPack.Schema.APA_ABL_Bill]);
				var bill = billPK.IsValid ? factory.Load<AsycudaBill>(billPK) : null;
				if (bill != null)
				{
					result = bill.GetPackType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaPack);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaPack);
		}
	}
}

