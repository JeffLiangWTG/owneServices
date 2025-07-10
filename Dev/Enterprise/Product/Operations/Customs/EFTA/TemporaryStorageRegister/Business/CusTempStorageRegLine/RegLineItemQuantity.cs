using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class RegLineItemQuantity : NonPersistentBusinessObject, IRegLineItemQuantity
{
	RegLineItemQuantity(CusTempStorageRegLineItemPivot regLineItemPivot)
		: base(regLineItemPivot.Factory)
	{
		this.regLineItemPivot = regLineItemPivot;
	}
	readonly CusTempStorageRegLineItemPivot regLineItemPivot;

	public ZDecimal SRV_GrossWeight => regLineItemPivot.SRV_GrossWeight;

	public ICusTempStorageRegLineItem RegLineItem => regLineItemPivot.RegLineItem;

	public static RegLineItemQuantity LoadNew(CusTempStorageRegLineItemPivot regLineItemPivot)
	{
		Argument.NotNull(regLineItemPivot, nameof(regLineItemPivot));
		var regLine = new RegLineItemQuantity(regLineItemPivot);
		return regLine;
	}
}
