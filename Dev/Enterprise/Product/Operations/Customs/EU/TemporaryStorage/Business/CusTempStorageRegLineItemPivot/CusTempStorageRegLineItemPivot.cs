using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLineItemPivot : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineItemPivot, ICusTempStorageRegLineItemPivot
{
	public CusTempStorageRegLineItemPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ThreadSafe]
	public new static CusTempStorageRegLineItemPivotTypeDecider TypeDecider = new();

	#region SRV_SRL_Line

	public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine RegLine => regLine ??= Factory.Load<CusTempStorageRegLine>(SRV_SRL_Line);
	CusTempStorageRegLine regLine;

	#endregion

	#region SRV_SRI_Item

	public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineItem RegLineItem => regLineItem ??= Factory.Load<CusTempStorageRegLineItem>(SRV_SRI_Item);
	CusTempStorageRegLineItem regLineItem;

	#endregion

}
