using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageLineTypeDecider : TypeDecider
		, Integration.Customs.DE.ICusTempStorageLineTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var storageDecPK = row != null ? new ZGuid(row[CusTempStorageLine.Schema.TSL_STH]) : ZGuid.Invalid;
			var storageDec = storageDecPK.IsValid ? factory.Load<EU.Business.CusTempStorage.CusTempStorageDec>(storageDecPK) : null;

			return GetTypeByStorageDec(row, factory, storageDec);
		}

		Type GetTypeByStorageDec(DataRow row, BusinessObjectFactory factory, EU.Business.CusTempStorage.CusTempStorageDec storageDec)
		{
			Type result;
			if (storageDec is CUSPRLCusTempStorageDec)
			{
				result = typeof(CUSPRLCusTempStorageLine);
			}
			else if (storageDec is CUSPCSCusTempStorageDec)
			{
				result = GetLineTypeForCusTempStorageDecFromLineParent(row, factory, typeof(CUSPCSConsolidatedCusTempStorageLine), typeof(CUSPCSSplitCusTempStorageLine));
			}
			else if (storageDec is PRLCONCusTempStorageDec)
			{
				result = GetLineTypeForCusTempStorageDecToLineParent(row, factory, typeof(PRLCONCusTempStorageLineToConsolidate), typeof(PRLCONConsolidatedCusTempStorageLine));
			}
			else
			{
				result = typeof(EU.Business.CusTempStorage.CusTempStorageLine);
			}
			return result;
		}

		Type GetLineTypeForCusTempStorageDecFromLineParent(DataRow row, BusinessObjectFactory factory, Type fromLineType, Type toLineType)
		{
			var pivots = factory.Load<CusTempStorageLinePivot>(new ZQuery(CusTempStorageLinePivotSchema.SLR_TSL_ToLine, row[CusTempStorageLine.Schema.PK]));
			return pivots.Any() ? toLineType : fromLineType;
		}

		Type GetLineTypeForCusTempStorageDecToLineParent(DataRow row, BusinessObjectFactory factory, Type fromLineType, Type toLineType)
		{
			var pivots = factory.Load<CusTempStorageLinePivot>(new ZQuery(CusTempStorageLinePivotSchema.SLR_TSL_FromLine, row[CusTempStorageLine.Schema.PK]));
			return pivots.Any() ? fromLineType : toLineType;
		}

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForNew() => null;
	}
}
