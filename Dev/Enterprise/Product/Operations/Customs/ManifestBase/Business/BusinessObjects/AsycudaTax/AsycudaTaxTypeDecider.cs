using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTaxTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var supporter = GetAsycudaTaxTypeSupporter<AsycudaBill>(row, factory, AsycudaTax.Schema.AET_ABL)
					?? GetAsycudaTaxTypeSupporter<AsycudaPackedItem>(row, factory, AsycudaTax.Schema.AET_API_AsycudaPackedItem);

				if (supporter != null)
				{
					result = supporter.GetAsycudaTaxType();
				}
			}
			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaTax);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaTax);
		}

		IAsycudaTaxTypeSupporter GetAsycudaTaxTypeSupporter<T>(DataRow row, BusinessObjectFactory factory, string columnName) where T : EnterpriseBusinessObject
		{
			var bizOPK = new ZGuid(row[columnName]);
			return bizOPK.IsValid ? factory.Load<T>(bizOPK) as IAsycudaTaxTypeSupporter : null;
		}
	}
}
