using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineItemPivotTypeDecider : TypeDecider
{
	public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
	{
		Type result = null;
		if (row != null)
		{
			result = GetTypeFromRegLine(row, factory);
			result = result ?? GetTypeFromRegLineItem(row, factory);
		}

		return result ?? GetTypeForNew();
	}

	public override Type GetTypeForNew() => typeof(CusTempStorageRegLineItemPivot);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLineItemPivot);

	public virtual Type GetRegLineType() => typeof(CusTempStorageRegLine);

	public virtual Type GetRegLineItemType() => typeof(CusTempStorageRegLineItem);

	Type GetTypeFromRegLine(DataRow row, BusinessObjectFactory factory)
	{
		var lineId = new ZGuid(row[CusTempStorageRegLineItemPivot.Schema.SRV_SRL_Line]);
		var line = lineId.IsValid
			? factory.Load(GetRegLineType(), lineId)
			: null;
		if (line is CusTempStorageRegLine cusTempStorageRegLine)
		{
			return cusTempStorageRegLine.GetStorageRegLineItemPivotType();
		}

		return null;
	}

	Type GetTypeFromRegLineItem(DataRow row, BusinessObjectFactory factory)
	{
		var lineId = new ZGuid(row[CusTempStorageRegLineItemPivot.Schema.SRV_SRI_Item]);
		var line = lineId.IsValid
			? factory.Load(GetRegLineItemType(), lineId)
			: null;
		if (line is CusTempStorageRegLineItem cusTempStorageRegLineItem)
		{
			return cusTempStorageRegLineItem.GetStorageRegLineItemPivotType();
		}

		return null;
	}
}
