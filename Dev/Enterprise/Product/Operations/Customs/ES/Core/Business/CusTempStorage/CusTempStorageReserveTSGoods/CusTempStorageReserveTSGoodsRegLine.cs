using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageReserveTSGoodsRegLine : NonPersistentBusinessObject, IObsoleteValidation, INotifyPropertyUpdated
{
	public static class Schema
	{
		public const string TSDNumber = "TSDNumber";
		public const string TSDItemNumber = "TSDItemNumber";
		public const string PackageType = "PackageType";
		public const string Location = "Location";
		public const string Reference = "Reference";

		public const string RemainingPackageQty = "RemainingPackageQty";
		public const string PackagesToUse = "PackagesToUse";

		public const string RemainingGrossWeight = "RemainingGrossWeight";
		public const string GrossWeightToUse = "GrossWeightToUse";
	}

	public CusTempStorageReserveTSGoodsRegLine(ICusTempStorageRegLine regLine, int totalPackages, decimal totalGrossWeight)
	{
		line = regLine;
		this.totalPackages = totalPackages;
		this.totalGrossWeight = totalGrossWeight;
	}

	public static CusTempStorageReserveTSGoodsRegLine New(ICusTempStorageRegLine regLine, int totalPackages, decimal totalGrossWeight) => new(regLine, totalPackages, totalGrossWeight);

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|TSDNumber", Caption = "TSD Number")]
	public ZString TSDNumber => line.RegHeader.SRH_Reference;
	public ZPropertyInfo TSDNumberInfo => GetZPropertyInfo(nameof(TSDNumber));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|TSDItem", Caption = "TSD Item")]
	public ZString TSDItemNumber => line.TSDItemNumber;
	public ZPropertyInfo TSDItemNumberInfo => GetZPropertyInfo(nameof(TSDItemNumber));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|PackageType", Caption = "Package Type")]
	public ZString PackageType => line.SRL_PackageType;
	public ZPropertyInfo PackageTypeInfo => GetZPropertyInfo(nameof(PackageType));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|Location", Caption = "Location")]
	public ZString Location => line.SRL_LocationOfGoods;
	public ZPropertyInfo locationInfo => GetZPropertyInfo(nameof(Location));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|Reference", Caption = "Reference")]
	public ZString Reference => line.SRL_OwnerReference;
	public ZPropertyInfo ReferenceInfo => GetZPropertyInfo(nameof(Reference));

	public ZBool IsPackageTypeBulk => line.IsPackageTypeBulk;

	#region Packages

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|RemainingPackages", Caption = "Remaining Packages")]
	public ZInt RemainingPackageQty => line.PackagesRemainingCalculated;
	public ZPropertyInfo RemainingPackageQtyInfo => GetZPropertyInfo(nameof(RemainingPackageQty));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|PackagesToUse", Caption = "Packages to use")]
	public ZInt PackagesToUse
	{
		get => packagesToUse;
		set
		{
			if (value != packagesToUse && value >= 0 && value <= totalPackages)
			{
				PackagesToUseInfo.ClearAllNotifications();
				packagesToUse = value;
				PackagesToUseInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidatePackagesToUse();
				}
				OnNotifyPropertyUpdated(Schema.PackagesToUse);
			}
		}
	}

	public ZPropertyInfo PackagesToUseInfo => GetZPropertyInfo(nameof(PackagesToUse));

	#endregion

	#region GrossWeight

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|RemainingGrossWeight", Caption = "Remaining Gross Weight")]
	public ZDecimal RemainingGrossWeight => line.GrossWeightRemainingCalculated;
	public ZPropertyInfo RemainingGrossWeightInfo => GetZPropertyInfo(nameof(RemainingGrossWeight));

	[ResourceStringData("Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageReserveTSGoodsRegLine|GrossWeightToUse", Caption = "Gross Weight to use")]
	public ZDecimal GrossWeightToUse
	{
		get => grossWeightToUse;
		set
		{
			if (value != grossWeightToUse && value >= 0 && value <= totalGrossWeight)
			{
				GrossWeightToUseInfo.ClearAllNotifications();
				grossWeightToUse = value;
				GrossWeightToUseInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateGrossWeightToUse();
				}
				OnNotifyPropertyUpdated(Schema.GrossWeightToUse);
			}
		}
	}
	public ZPropertyInfo GrossWeightToUseInfo => GetZPropertyInfo(nameof(GrossWeightToUse));

	#endregion

	protected override ZGuid GetPK()
	{
		return line.PK;
	}

	#region INotifyPropertyUpdated

	void OnNotifyPropertyUpdated(string propertyName)
	{
		if (!suspendNotifications)
		{
			PropertyUpdated?.Invoke(this, new PropertyUpdatedEventArgs(propertyName));
		}
	}

	public void SuspendNotifications()
	{
		suspendNotifications = true;
	}

	public void ResumeNotifications()
	{
		suspendNotifications = false;
	}

	public event EventHandler<PropertyUpdatedEventArgs> PropertyUpdated;
	bool suspendNotifications;

	#endregion

	readonly ICusTempStorageRegLine line;
	readonly ZInt totalPackages;
	readonly ZDecimal totalGrossWeight;
	ZInt packagesToUse;
	ZDecimal grossWeightToUse;

	public CusTempStorageReserveTSGoodsRegLineValidation Validation => new(this);
}
