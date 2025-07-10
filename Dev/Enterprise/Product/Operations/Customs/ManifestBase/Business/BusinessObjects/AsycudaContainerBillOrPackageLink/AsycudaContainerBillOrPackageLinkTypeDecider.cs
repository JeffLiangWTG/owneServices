using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaContainerBillOrPackageLinkTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var billPK = new ZGuid(row[AsycudaContainerBillOrPackageLink.Schema.APC_ABL_Bill]);
				var bill = billPK.IsValid ? factory.Load<AsycudaBill>(billPK) : null;
				if (bill == null)
				{
					var packPK = new ZGuid(row[AsycudaContainerBillOrPackageLink.Schema.APC_APA_Pack]);
					var pack = packPK.IsValid ? factory.Load<AsycudaPack>(packPK) : null;
					bill = pack?.Bill;
				}
				result = bill?.GetPackageContainerLinkType();
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaContainerBillOrPackageLink);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaContainerBillOrPackageLink);
		}
	}
}

