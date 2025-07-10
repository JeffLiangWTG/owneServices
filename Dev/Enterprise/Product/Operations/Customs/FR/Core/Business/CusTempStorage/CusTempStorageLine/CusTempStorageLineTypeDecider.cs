using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageLineTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var storageDecPK = row != null ? new ZGuid(row[CusTempStorageLine.Schema.TSL_STH]) : ZGuid.Invalid;
			var storageDec = storageDecPK.IsValid ? factory.Load<EU.Business.CusTempStorage.CusTempStorageDec>(storageDecPK) : null;

			return GetTypeByStorageDec(storageDec);
		}

		Type GetTypeByStorageDec(EU.Business.CusTempStorage.CusTempStorageDec storageDec)
		{
			Type result;
			if (storageDec is ISTCusTempStorageDec)
			{
				result = typeof(ISTCusTempStorageLine);
			}
			else if (storageDec is FRCCusTempStorageDec)
			{
				result = typeof(FRCCusTempStorageLine);
			}
			else if (storageDec is LADTCusTempStorageDec)
			{
				result = typeof(LADTCusTempStorageLine);
			}
			else
			{
				result = typeof(EU.Business.CusTempStorage.CusTempStorageLine);
			}
			return result;
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
