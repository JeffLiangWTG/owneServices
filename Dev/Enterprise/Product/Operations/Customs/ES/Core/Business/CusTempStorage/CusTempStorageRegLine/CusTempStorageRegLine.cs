using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLine : EU.TemporaryStorage.Business.CusTempStorageRegLine, Integration.Customs.ES.ICusTempStorageRegLine
{
	public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Override Properties

	[ResourceStringData("345ADE38-4A1F-458E-A5D8-39AC54E869EA", Caption = "Customs Status", MediumCaption = "Status", ShortCaption = "St", FullDescription = "Customs Status for the selected packages")]
	public override ZString SRL_CustomsStatus { get => base.SRL_CustomsStatus; set => base.SRL_CustomsStatus = value; }

	[ResourceStringData("526064BD-1E9B-40E3-BC9F-E810E62EB1FC", Caption = "Marks & Numbers", MediumCaption = "Marks & Num", ShortCaption = "Marks", FullDescription = "Marks and Numbers for selected packages")]
	public override ZString SRL_PackageMarks { get => base.SRL_PackageMarks; set => base.SRL_PackageMarks = value; }

	#endregion

	public new CusTempStorageRegLineLookups Lookups => (CusTempStorageRegLineLookups)base.Lookups;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups GetNewLookups() => new CusTempStorageRegLineLookups(this);

	public new CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactions => (CusTempStorageRegLineTransactionCollection)base.CusTempStorageRegLineTransactions;

	[ChildEditable(true)]
	public CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactionsForFilter
	{
		get
		{
			if (cusTempStorageRegLineTransactionsForFilter == null)
			{
				cusTempStorageRegLineTransactionsForFilter = (CusTempStorageRegLineTransactionCollection)CreateNewCusTempStorageRegLineTransactions();
				RegisterEditableChildObject(cusTempStorageRegLineTransactionsForFilter);
			}

			return cusTempStorageRegLineTransactionsForFilter;
		}
	}
	CusTempStorageRegLineTransactionCollection cusTempStorageRegLineTransactionsForFilter;

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions() => new CusTempStorageRegLineTransactionCollection(this);

	protected override Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);

	protected override Type GetStorageRegLineItemPivotTypeCore() => typeof(CusTempStorageRegLineItemPivot);

	public override void Delete()
	{
		base.Delete();
		this.DeleteChildren<CusTempStorageRegLineTransaction>(CusTempStorageRegLineTransactionSchema.SRT_SRL);
		this.DeleteChildren<CusTempStorageRegLineItemPivot>(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line);
	}

	public ZBool HasRegLineOBLTransaction() => CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);

	public void SetRegLineLocationAndReference(BusinessObjectFactory newFactory, ZString location, ZBool shouldChangeLocation, ZString reference, ZBool shouldChangeReference)
	{
		var newFactoryRegLine = newFactory.Load<CusTempStorageRegLine>(PK);
		if (shouldChangeLocation)
		{
			newFactoryRegLine.SRL_LocationOfGoods = location;
		}
		if (shouldChangeReference)
		{
			newFactoryRegLine.SRL_OwnerReference = reference;
		}
	}

	public ZInt NumberOfItems => RegLineItemPivots.Count;

	public ZString TSDItemNumbers => string.Join(", ", RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>().Select(x => x.RegLineItem.SRI_GoodsItemNumber.ToString()).OrderBy(x => x));

	public ZString ItemCommodityCode
	{
		get
		{
			var itemCommodityCodes = RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>().Select(x => x.RegLineItem.SRI_Tariff).Where(t => !t.IsEmpty).Take(2).ToArray();
			switch (itemCommodityCodes.Length)
			{
				case 2:
					return MultipleText;

				case 1:
					return itemCommodityCodes?[0] ?? ZString.Empty;

				default:
					return ZString.Empty;
			}
		}
	}

	public ZString ItemGoodsDescription
	{
		get
		{
			var itemGoodsDescription = RegLineItemPivots.Cast<EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot>().Select(x => x.RegLineItem.SRI_GoodsDescription).Where(t => !t.IsEmpty).Take(2).ToArray();
			switch (itemGoodsDescription.Length)
			{
				case 2:
					return MultipleText;

				case 1:
					return itemGoodsDescription?[0] ?? ZString.Empty;

				default:
					return ZString.Empty;
			}
		}
	}

	ZString MultipleText => Res.GetString("57F5B52A-3D26-4CE6-BCBB-2072A65872E2", "Multiple");
}
