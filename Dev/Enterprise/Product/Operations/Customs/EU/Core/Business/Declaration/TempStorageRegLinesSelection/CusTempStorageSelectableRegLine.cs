using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusTempStorageSelectableRegLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		static string Multiple => Res.GetString("977C9D7F-C107-4E32-B840-0B7CCB843203", "Multiple");

		public static class Schema
		{
			public const string PreviousReferenceType = "PreviousReferenceType";
			public const string PreviousReferenceNumber = "PreviousReferenceNumber";
			public const string TSDNumber = "TSDNumber";
			public const string ArrivalDate = "ArrivalDate";
			public const string Owner = "Owner";
			public const string OriginalPackagesQty = "OriginalPackagesQty";
			public const string PackagesQtyOnHand = "PackagesQtyOnHand";
			public const string PackageType = "PackageType";
			public const string PackageMarks = "PackageMarks";
			public const string PackagesToDraw = "PackagesToDraw";
			public const string GrossWeightOnHand = "GrossWeightOnHand";
			public const string GrossWeightToDraw = "GrossWeightToDraw";
			public const string TSDItemNumber = "TSDItemNumber";
			public const string CommodityCode = "CommodityCode";
			public const string GoodsDescription = "GoodsDescription";
		}

		ZInt packagestoDraw;
		ZDecimal grossWeightToDraw;

		public CusTempStorageSelectableRegLine(ICusTempStorageRegLine storageRegLine)
		{
			this.regLine = storageRegLine;
		}
		readonly ICusTempStorageRegLine regLine;

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PreviousReferenceType", Caption = "Previous Reference Type")]
		public ZString PreviousReferenceType => ZString.Empty;
		public ZPropertyInfo PreviousReferenceTypeInfo => GetZPropertyInfo(nameof(PreviousReferenceType));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PreviousReferenceNumber", Caption = "Previous Reference Number")]
		public ZString PreviousReferenceNumber => ZString.Empty;
		public ZPropertyInfo PreviousReferenceNumberInfo => GetZPropertyInfo(nameof(PreviousReferenceNumber));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|TSDNumber", Caption = "TSD Number")]
		public ZString TSDNumber => regLine.RegHeader.SRH_Reference;
		public ZPropertyInfo TSDNumberInfo => GetZPropertyInfo(nameof(TSDNumber));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|ArrivalDate", Caption = "Arrival Date")]
		public ZDate ArrivalDate => regLine.RegHeader.SRH_ArrivalDate;
		public ZPropertyInfo ArrivalDateInfo => GetZPropertyInfo(nameof(ArrivalDate));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|Owner", Caption = "Owner")]
		public ZString Owner => regLine.SRL_GoodsOwnerIdentifier;
		public ZPropertyInfo OwnerInfo => GetZPropertyInfo(nameof(Owner));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|OriginalPackagesQty", Caption = "Original Packages Qty")]
		public ZInt OriginalPackagesQty => regLine.OriginalPackagesQuantity;
		public ZPropertyInfo OriginalPackagesQtyInfo => GetZPropertyInfo(nameof(OriginalPackagesQty));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PackagesQtyOnHand", Caption = "Packages Qty on Hand")]
		public ZInt PackagesQtyOnHand => regLine.PackagesRemainingCalculated;
		public ZPropertyInfo PackagesQtyOnHandInfo => GetZPropertyInfo(nameof(PackagesQtyOnHand));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PackageType", Caption = "Package Type")]
		public ZString PackageType => regLine.SRL_PackageType;
		public ZPropertyInfo PackageTypeInfo => GetZPropertyInfo(nameof(PackageType));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PackageMarks", Caption = "Package Marks")]
		public ZString PackageMarks => regLine.SRL_PackageMarks;
		public ZPropertyInfo PackageMarksInfo => GetZPropertyInfo(nameof(PackageMarks));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|PackagesToDraw", Caption = "Packages to Draw")]
		public ZInt PackagesToDraw
		{
			get => packagestoDraw;
			set
			{
				PackagesToDrawInfo.ClearAllNotifications();
				if(value > PackagesQtyOnHand)
				{
					PackagesToDrawInfo.AddError(Res.GetString("97D9166A-22EC-4765-9F11-E5F1C1F28445", "Please enter a Packages quantity less or equal to what's available on hand."));
				}
				packagestoDraw = value;
				PackagesToDrawInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PackagesToDrawInfo => GetZPropertyInfo(nameof(PackagesToDraw));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|GrossWeightOnHand", Caption = "Gross Weight on Hand")]
		public ZDecimal GrossWeightOnHand => regLine.GrossWeightRemainingCalculated;
		public ZPropertyInfo GrossWeightOnHandInfo => GetZPropertyInfo(nameof(GrossWeightOnHand));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|GrossWeightToDraw", Caption = "Gross Weight to Draw")]
		public ZDecimal GrossWeightToDraw
		{
			get => grossWeightToDraw;
			set
			{
				GrossWeightToDrawInfo.ClearAllNotifications();
				if(value > this.GrossWeightOnHand)
				{
					GrossWeightToDrawInfo.AddError(Res.GetString("76E2678E-4E05-4E01-9E10-9062C138B1B4", "Please enter a Weight quantity less or equal to what's available on hand."));
				}
				grossWeightToDraw = value;
				GrossWeightToDrawInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo GrossWeightToDrawInfo => GetZPropertyInfo(nameof(GrossWeightToDraw));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|TSDItemNumber", Caption = "TSD Item Number")]
		public ZString TSDItemNumber => regLine.HasManyPivotItems ? Multiple : regLine.TSDItemNumber;
		public ZPropertyInfo TSDItemNumberInfo => GetZPropertyInfo(nameof(TSDItemNumber));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|CommodityCode", Caption = "Commodity Code")]
		public ZString CommodityCode => regLine.HasManyPivotItems ? Multiple : regLine.CommodityCode;
		public ZPropertyInfo CommodityCodeInfo => GetZPropertyInfo(nameof(CommodityCode));

		[ResourceStringData("Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageSelection.CusTempStorageSelectableRegLine|GoodsDescription", Caption = "Goods Description")]
		public ZString GoodsDescription => regLine.HasManyPivotItems ? Multiple : regLine.GoodsDescription;
		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		public ZString CustomsLocation => regLine.RegHeader.Premises.SRP_CustomsLocation;

		public IRegLineItemQuantityCollection<IRegLineItemQuantity> RegLineItemQuantities => regLine.RegLineItemQuantities;
	}
}
