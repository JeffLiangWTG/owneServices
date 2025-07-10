using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[SingleObjectAroundARow]
public class CusTempStorageRegLineItemPivot : AutoCusTempStorageRegLineItemPivot
	, Integration.Customs.TemporaryStorage.ICusTempStorageRegLineItemPivot
	, IPivotBusinessObject
{
	public CusTempStorageRegLineItemPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[ThreadSafe]
	public static CusTempStorageRegLineItemPivotTypeDecider TypeDecider = new();

	#region SRV_SRL_Line

	[RelatedBusinessObject(nameof(RegLine))]
	public override ZGuid SRV_SRL_Line { get => base.SRV_SRL_Line; set => base.SRV_SRL_Line = value; }

	public virtual CusTempStorageRegLine RegLine => regLine ??= Factory.Load<CusTempStorageRegLine>(SRV_SRL_Line);
	CusTempStorageRegLine regLine;

	#endregion

	#region SRV_SRI_Item

	[RelatedBusinessObject(nameof(RegLineItem))]
	public override ZGuid SRV_SRI_Item { get => base.SRV_SRI_Item; set => base.SRV_SRI_Item = value; }

	public virtual CusTempStorageRegLineItem RegLineItem => regLineItem ??= Factory.Load<CusTempStorageRegLineItem>(SRV_SRI_Item);
	CusTempStorageRegLineItem regLineItem;

	#endregion

	[ResourceStringData("4CC4E954-83F2-4D6C-9046-04063C669C59", Caption = "Gross Weight", MediumCaption = "Gross Weight", ShortCaption = "GWT", FullDescription = "Goods Gross Weight")]
	public override ZDecimal SRV_GrossWeight { get => base.SRV_GrossWeight; set => base.SRV_GrossWeight = value; }

	public virtual ZGuid Relation1ID { get => SRV_SRL_Line; set => SRV_SRL_Line = value; }

	public virtual BusinessObject Relation1Object => RegLine;

	public virtual ZGuid Relation2ID { get => SRV_SRI_Item; set => SRV_SRI_Item = value; }

	public virtual BusinessObject Relation2Object => RegLineItem;
}
