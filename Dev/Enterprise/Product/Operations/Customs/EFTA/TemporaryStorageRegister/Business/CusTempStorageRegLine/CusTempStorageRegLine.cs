using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLine : AutoCusTempStorageRegLine, ICusTempStorageRegLine
{
	public CusTempStorageRegLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	#region Type Decider

	[ThreadSafe]
	public static readonly CusTempStorageRegLineTypeDecider TypeDecider = new ();

	#endregion

	[RelatedBusinessObject(nameof(RegHeader))]
	public override ZGuid SRL_SRH
	{
		get { return base.SRL_SRH; }
		set { base.SRL_SRH = value; }
	}

	public virtual CusTempStorageRegHeader RegHeader => Factory.Load<CusTempStorageRegHeader>(SRL_SRH);

	ICusTempStorageRegHeader ICusTempStorageRegLine.RegHeader => RegHeader;

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_LineNumber", Caption = "Line Number", ShortCaption = "Line No.")]
	public override ZInt SRL_LineNumber
	{
		get => base.SRL_LineNumber;
		set => base.SRL_LineNumber = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_OwnerReferenceType", Caption = "Owner Reference Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.OwnerReferenceTypeList))]
	public override ZString SRL_OwnerReferenceType
	{
		get => base.SRL_OwnerReferenceType;
		set => base.SRL_OwnerReferenceType = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_OwnerReference", Caption = "Owner Reference Number")]
	public override ZString SRL_OwnerReference
	{
		get => base.SRL_OwnerReference;
		set => base.SRL_OwnerReference = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_LocationOfGoods", Caption = "Location of Goods")]
	public override ZString SRL_LocationOfGoods
	{
		get => base.SRL_LocationOfGoods;
		set => base.SRL_LocationOfGoods = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_GoodsDescription", Caption = "Goods Description")]
	public override ZString SRL_GoodsDescription
	{
		get => base.SRL_GoodsDescription;
		set => base.SRL_GoodsDescription = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_GrossWeightUQ", Caption = "Gross Weight UQ")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.WeightUQList))]
	public override ZString SRL_GrossWeightUQ
	{
		get => base.SRL_GrossWeightUQ;
		set => base.SRL_GrossWeightUQ = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_LimitDate", Caption = "Limit Date")]
	public override ZDate SRL_LimitDate
	{
		get => base.SRL_LimitDate;
		set => base.SRL_LimitDate = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_PackagesRemaining", Caption = "Packages Remaining")]
	public override ZInt SRL_PackagesRemaining
	{
		get => base.SRL_PackagesRemaining;
		set => base.SRL_PackagesRemaining = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_PackageType", Caption = "Package Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.PackageTypeList))]
	public override ZString SRL_PackageType
	{
		get => base.SRL_PackageType;
		set => base.SRL_PackageType = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_CustomsStatus", Caption = "Customs Status")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.CustomsStatusList))]
	public override ZString SRL_CustomsStatus
	{
		get => base.SRL_CustomsStatus;
		set => base.SRL_CustomsStatus = value;
	}

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|SRL_UnionStatus", Caption = "Union Status")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegLineLookups.UnionStatusList))]
	public override ZString SRL_UnionStatus
	{
		get => base.SRL_UnionStatus;
		set => base.SRL_UnionStatus = value;
	}

	[ResourceStringData("FAAB7586-4722-44B6-8371-0FF371B18A2D", Caption = "Custodian EORI")]
	public override ZString SRL_CustodianIdentifier
	{
		get => base.SRL_CustodianIdentifier;
		set => base.SRL_CustodianIdentifier = value;
	}

	[ResourceStringData("AEAEA303-0656-4123-911C-6E7A9A90D6C4", Caption = "Disposal Entitled Trader")]
	public override ZString SRL_GoodsOwnerIdentifierBranchNo
	{
		get => base.SRL_GoodsOwnerIdentifierBranchNo;
		set => base.SRL_GoodsOwnerIdentifierBranchNo = value;
	}

	[ResourceStringData("594A2C09-0F1F-4717-86FF-DB18201DF30D", Caption = "Branch")]
	public override ZString SRL_CustodianIdentifierBranchNo
	{
		get => base.SRL_CustodianIdentifierBranchNo;
		set => base.SRL_CustodianIdentifierBranchNo = value;
	}

	[ResourceStringData("A2C7F6E7-AE22-4CEA-BEF9-218C5625C073", Caption = "Marks & Numbers", MediumCaption = "Marks & Numbers", ShortCaption = "Marks", FullDescription = "Marks and Numbers for selected packages")]
	public override ZString SRL_PackageMarks { get => base.SRL_PackageMarks; set => base.SRL_PackageMarks = value; }

	[ResourceStringData("1551ACF8-B53E-451D-B47D-5F6B76C55335", Caption = "Owner ID", MediumCaption = "Owner ID", ShortCaption = "Owner ID", FullDescription = "Owner's Identification Number")]
	public override ZString SRL_GoodsOwnerIdentifier { get => base.SRL_GoodsOwnerIdentifier; set => base.SRL_GoodsOwnerIdentifier = value; }

	[ResourceStringData("Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine|PackagesRemainingCalculated", Caption = "Packages Remaining", MediumCaption = "Packs Remaining", ShortCaption = "Packs Remain", FullDescription = "Packages Remaining Amount")]
	public ZInt PackagesRemainingCalculated => Factory.GetValue(ref cachedPackagesRemainingCalculated, GetPackagesRemainingCalculated);
	CachedProperty<ZInt> cachedPackagesRemainingCalculated;

	ZInt GetPackagesRemainingCalculated() => CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus != CusTempStorageRegLineTransactionStatusList.Codes.Deleted).Sum(t => t.SRT_PackageQty);

	public ZPropertyInfo PackagesRemainingCalculatedInfo => GetZPropertyInfo(nameof(PackagesRemainingCalculated));

	[ResourceStringData("Enterprise.Customs.Business.CusTempStorage.CusTempStorageRegLine|GrossWeightRemainingCalculated", Caption = "Gross Weight Remaining", MediumCaption = "Gross Weight Rem.", ShortCaption = "GWT Rem.", FullDescription = "Gross Weight Remaining Amount")]
	public ZDecimal GrossWeightRemainingCalculated => Factory.GetValue(ref cachedGrossWeightRemainingCalculated, GetGrossWeightRemainingCalculated);
	CachedProperty<ZDecimal> cachedGrossWeightRemainingCalculated;

	ZDecimal GetGrossWeightRemainingCalculated() => CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus != CusTempStorageRegLineTransactionStatusList.Codes.Deleted).Sum(t => t.SRT_GrossWeight);

	public ZPropertyInfo GrossWeightRemainingCalculatedInfo => GetZPropertyInfo(nameof(GrossWeightRemainingCalculated));

	public bool IsPackageTypeBulk => Lookups.BulkPackageUnitTypeList.ContainsCode(SRL_PackageType);

	public bool IsPackageTypeFrame => SRL_PackageType == UniversalReferenceConstants.PackageType.Frame;

	public bool IsClosed => SRL_CustomsStatus == UniversalReferenceConstants.TemporaryStorageStatus.Closed;

	public bool IsOpen => SRL_CustomsStatus == UniversalReferenceConstants.TemporaryStorageStatus.Open;

	[ResourceStringData("Enterprise.Customs.Business.CusTempStorage.CusTempStorageRegLine|LiabilityAmountRemainingCalculated", Caption = "Liability Amount Remaining", MediumCaption = "Bond Amount Rem.", ShortCaption = "Bond Amt. Rem.", FullDescription = "Remaining Liability Amount")]
	public ZDecimal BondAmountRemainingCalculated => Factory.GetValue(ref cachedBondAmountRemainingCalculated, GetBondAmountRemainingCalculated);
	CachedProperty<ZDecimal> cachedBondAmountRemainingCalculated;

	ZDecimal GetBondAmountRemainingCalculated() => CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus != CusTempStorageRegLineTransactionStatusList.Codes.Deleted).Sum(t => t.SRT_BondAmount);

	public ZPropertyInfo BondAmountRemainingCalculatedInfo => GetZPropertyInfo(nameof(BondAmountRemainingCalculated));

	public ZInt OriginalPackagesQuantity => Factory.GetCached(ref originalPackagesQty, CalculateOriginalPackagesQuantity);
	CachedProperty<ZInt> originalPackagesQty;

	public ZString TSDItemNumber => Factory.GetCached(ref tsdItemNumber, GetTSDItemNumber);
	CachedProperty<ZString> tsdItemNumber;

	public ZString GoodsDescription => Factory.GetCached(ref goodsDescription, GetGoodsDescription);
	CachedProperty<ZString> goodsDescription;

	public ZString CommodityCode => Factory.GetCached(ref commodityCode, GetCommodityCode);
	CachedProperty<ZString> commodityCode;

	public ZBool HasManyPivotItems => Factory.GetCached(ref hasManyPivotItems, EvaluateHasManyPivotItems);
	CachedProperty<ZBool> hasManyPivotItems;

	protected virtual ZInt CalculateOriginalPackagesQuantity() => CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance).Sum(t => t.SRT_PackageQty);
	protected virtual ZString GetTSDItemNumber() => ZString.Empty;
	protected virtual ZString GetGoodsDescription() => ZString.Empty;
	protected virtual ZString GetCommodityCode() => ZString.Empty;
	protected virtual ZBool EvaluateHasManyPivotItems() => this.RegLineItemPivots?.Count > 1;

	#region CusTempStorageRegLineTransactions

	[ChildEditable(true)]
	public CusTempStorageRegLineTransactionCollection CusTempStorageRegLineTransactions
	{
		get
		{
			if (cusTempStorageRegLineTransactions == null)
			{
				cusTempStorageRegLineTransactions = CreateNewCusTempStorageRegLineTransactions();
				RegisterEditableChildObject(cusTempStorageRegLineTransactions);
			}
			return cusTempStorageRegLineTransactions;
		}
	}
	CusTempStorageRegLineTransactionCollection cusTempStorageRegLineTransactions;

	ICusTempStorageRegLineTransactionCollection<ICusTempStorageRegLineTransaction> ICusTempStorageRegLine.CusTempStorageRegLineTransactions => CusTempStorageRegLineTransactions;

	protected virtual CusTempStorageRegLineTransactionCollection CreateNewCusTempStorageRegLineTransactions() => new CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>(this);

	#endregion

	#region OpeningBalanceTransaction

	public ICusTempStorageRegLineTransaction OpeningBalanceTransaction => openingBalanceTransaction ??= GetOpeningBalanceTransaction();
	ICusTempStorageRegLineTransaction openingBalanceTransaction;

	protected ICusTempStorageRegLineTransaction GetOpeningBalanceTransaction() => CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_TransactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);

	#endregion

	#region RegLineItemPivots

	[ChildEditable(true)]
	public CusTempStorageRegLineItemPivotCollection RegLineItemPivots
	{
		get
		{
			if (regLineItemPivots == null)
			{
				regLineItemPivots = CreateNewRegLineItemPivotsCollection();
				RegisterEditableChildObject(regLineItemPivots);
			}
			return regLineItemPivots;
		}
	}

	CusTempStorageRegLineItemPivotCollection regLineItemPivots;

	protected virtual CusTempStorageRegLineItemPivotCollection CreateNewRegLineItemPivotsCollection() => new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(this);

	#endregion

	public IRegLineItemQuantityCollection<IRegLineItemQuantity> RegLineItemQuantities
	{
		get
		{
			if (regLineItemQuantities == null)
			{
				var collection = new RegLineItemQuantityCollection();
				foreach (var regLineItemPivot in RegLineItemPivots)
				{
					collection.Add(RegLineItemQuantity.LoadNew((CusTempStorageRegLineItemPivot)regLineItemPivot));
				}
				regLineItemQuantities = collection;
			}
			return regLineItemQuantities;
		}
	}

	IRegLineItemQuantityCollection<IRegLineItemQuantity> regLineItemQuantities;

	public Type GetStorageRegLineTransactionType() => GetStorageRegLineTransactionCore();

	protected virtual Type GetStorageRegLineTransactionCore() => typeof(CusTempStorageRegLineTransaction);

	protected internal void UpdatePackagesRemaining()
	{
		SRL_PackagesRemaining = CusTempStorageRegLineTransactions.Where(t => t.SRT_TransactionStatus != CusTempStorageRegLineTransactionStatusList.Codes.Deleted).Sum(t => t.SRT_PackageQty);
	}

	public Type GetStorageRegLineItemPivotType() => GetStorageRegLineItemPivotTypeCore();

	protected virtual Type GetStorageRegLineItemPivotTypeCore() => typeof(CusTempStorageRegLineItemPivot);
}
