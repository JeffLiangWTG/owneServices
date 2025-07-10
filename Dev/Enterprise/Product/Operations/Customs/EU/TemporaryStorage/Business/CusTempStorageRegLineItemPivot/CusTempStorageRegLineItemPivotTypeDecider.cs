using System;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public sealed class CusTempStorageRegLineItemPivotTypeDecider : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineItemPivotTypeDecider
{
	public override Type GetTypeForNew() => typeof(CusTempStorageRegLineItemPivot);

	public override Type GetTypeForBinding() => typeof(CusTempStorageRegLineItemPivot);

	public override Type GetRegLineType() => typeof(CusTempStorageRegLine);

	public override Type GetRegLineItemType() => typeof(CusTempStorageRegLineItem);
}
