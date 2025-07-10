using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillScreeningTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var billPK = new ZGuid(row[AsycudaBillScreening.Schema.ASR_ABL]);
				var supporter = billPK.IsValid ? factory.Load<AsycudaBill>(billPK) as IAsycudaBillScreeningTypeSupporter : null;
				if (supporter != null)
				{
					result = supporter.GetAsycudaBillScreeningType();
				}
			}

			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaBillScreening);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaBillScreening);
		}
	}
}

