using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.TemporaryStorage.Business;

public class CusTempStorageRegLine : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine, ICusTempStorageRegLine
{
	public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader RegHeader => Factory.Load<CusTempStorageRegHeader>(SRL_SRH);

	protected override ZString GetTSDItemNumber() => HasManyPivotItems ? ZString.Empty : RegLineItemPivots.OfType<CusTempStorageRegLineItemPivot>().FirstOrDefault()?.RegLineItem.SRI_GoodsItemNumber.ToString() ?? ZString.Empty;

	protected override ZString GetGoodsDescription() => HasManyPivotItems ? ZString.Empty : RegLineItemPivots.OfType<CusTempStorageRegLineItemPivot>().FirstOrDefault()?.RegLineItem.SRI_GoodsDescription ?? ZString.Empty;

	protected override ZString GetCommodityCode() => HasManyPivotItems ? ZString.Empty : RegLineItemPivots.OfType<CusTempStorageRegLineItemPivot>().FirstOrDefault()?.RegLineItem.SRI_Tariff ?? ZString.Empty;

	#region Type Decider

	[ThreadSafe]
	public static new readonly CusTempStorageRegLineTypeDecider TypeDecider = new();

	#endregion

	protected override CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions() => new CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>(this);

	protected override CusTempStorageRegLineItemPivotCollection CreateNewRegLineItemPivotsCollection() => new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(this);

	protected override Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);

	protected override Type GetStorageRegLineItemPivotTypeCore() => typeof(CusTempStorageRegLineItemPivot);
}
