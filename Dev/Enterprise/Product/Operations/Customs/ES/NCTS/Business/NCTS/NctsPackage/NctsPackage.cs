using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business;

[SystemDefinedValues]
public class NctsPackage : EU.NCTS.Business.NctsPackage, Integration.Customs.ES.INctsPackage, ICheckablePackage
{
	public NctsPackage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsPackage.Schema
	{
		public const string IsDataLoadFromDeparture = "IsDataLoadFromDeparture";
	}

	public new NctsPackage PackDifference => (NctsPackage)base.PackDifference;

	protected override void OnUnitTypeChange()
	{
		base.OnUnitTypeChange();
		if (IsPhase5Arrival)
		{
			if (IsUnitTypeFR)
			{
				B5_UnitCount = 1;
				B5_MarksAndNumbers = ZString.Empty;
			}
			else
			{
				B5_PackageID = ZString.Empty;
				B5_Brand = ZString.Empty;
				B5_Model = ZString.Empty;
			}
		}
	}

	public ZBool IsDataLoadFromDeparture
	{
		get => this.GetSystemDefinedValue<ZBool>(Schema.IsDataLoadFromDeparture);
		set
		{
			var oldValue = IsDataLoadFromDeparture;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(Schema.IsDataLoadFromDeparture, value);
				IsDataLoadFromDepartureInfo.RefreshBinding(oldValue);
			}
		}
	}
	public ZPropertyInfo IsDataLoadFromDepartureInfo => GetZPropertyInfo(Schema.IsDataLoadFromDeparture);

	[ResourceStringData("C01BEC4B-C0E2-4E6D-B069-557E9C741521", Caption = "VIN", MediumCaption = "VIN", ShortCaption = "VIN", FullDescription = "Vehicle VIN")]
	public override ZString B5_PackageID { get => base.B5_PackageID; set => base.B5_PackageID = value; }

	[MaxLength(35)]
	public override ZString B5_Brand { get => base.B5_Brand; set => base.B5_Brand = value; }

	[MaxLength(35)]
	public override ZString B5_Model { get => base.B5_Model; set => base.B5_Model = value; }

	public ZString EffectiveUnitType => UnloadedStatus == NctsUnloadedStateList.Codes.DIF ? PackDifference.B5_UnitType : B5_UnitType;

	public ZString EffectiveMarksAndNumbers
	{
		get
		{
			ZString result;

			var status = UnloadedStatus;
			if (status == NctsUnloadedStateList.Codes.DIF)
			{
				if (EffectiveUnitType == RefCusCodeList.PackageType.Frame)
				{
					result = PackDifference.B5_PackageID + VehiclesSeparator + PackDifference.B5_Brand + VehiclesSeparator + PackDifference.B5_Model;
				}
				else
				{
					result = PackDifference.B5_MarksAndNumbers;
				}
			}
			else
			{
				if (EffectiveUnitType == RefCusCodeList.PackageType.Frame)
				{
					result = B5_PackageID + VehiclesSeparator + B5_Brand + VehiclesSeparator + B5_Model;
				}
				else
				{
					result = B5_MarksAndNumbers;
				}
			}

			return result;
		}
	}

	const string VehiclesSeparator = ":";

	protected override CusInvPackValidation GetNewPhase4Validation() => new NctsPackagePhase4Validation(this);

	protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackagePhase5Validation(this);

	protected override bool B5_TypeOfDifferenceReadOnlyCore => base.B5_TypeOfDifferenceReadOnlyCore || IsUnloadingRemarksReadOnlySpain;
	protected override bool B5_UnitCountReadOnly => base.B5_UnitCountReadOnly || IsPhase5ArrivalUnitTypeFR || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsVehicles;
	protected override bool B5_UnitTypeReadOnly => base.B5_UnitTypeReadOnly || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsVehicles;
	protected override bool B5_MarksAndNumbersReadOnly => base.B5_MarksAndNumbersReadOnly || IsPhase5ArrivalUnitTypeFR || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsVehicles;
	protected override bool B5_PackageIDReadOnly => base.B5_PackageIDReadOnly || IsPhase5ArrivalNoUnitTypeFR || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsNotVehicles;
	protected override bool B5_BrandReadOnly => base.B5_BrandReadOnly || IsPhase5ArrivalNoUnitTypeFR || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsNotVehicles;
	protected override bool B5_ModelReadOnly => base.B5_ModelReadOnly || IsPhase5ArrivalNoUnitTypeFR || IsUnloadingRemarksReadOnlySpain || IsPhase5DepartureAndIsNotVehicles;

	bool IsVehicles => Parent is NctsDepartureCargoDesc goodsItem && goodsItem.IsVehicles;

	internal bool IsPhase5DepartureAndIsVehicles => IsPhase5Departure && IsVehicles;

	bool IsPhase5DepartureAndIsNotVehicles => IsPhase5Departure && !IsVehicles;

	public bool IsUnloadingRemarksReadOnlySpain => Factory.GetValue(ref isUnloadingRemarksReadOnlySpain, () => Parent?.Header?.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader &&
							arrivalMovementHeader.IsUnloadingRemarksReadOnlySpain);

	CachedProperty<bool> isUnloadingRemarksReadOnlySpain;

	bool IsUnitTypeFR => B5_UnitType == RefCusCodeList.PackageType.Frame;

	bool IsPhase5Arrival => IsPhase5 && IsArrivalMovement;

	bool IsPhase5ArrivalUnitTypeFR => IsPhase5Arrival && IsUnitTypeFR;

	bool IsPhase5ArrivalNoUnitTypeFR => IsPhase5Arrival && !IsUnitTypeFR;

	protected override CusInvPackLookups GetNewLookups() => new NctsPackageLookups(this);

	protected override EU.NCTS.Business.NonPersistentContainerPivotPhase5Collection GetContainersPivotsCore() => new NonPersistentContainerPivotPhase5Collection(this);

	#region ICheckablePackage

	IEnumerable<ICheckablePackage> ICheckablePackage.GetRelatedEntryPackages()
	{
		var currentGoodsItem = Parent;

		var goodsItems = IsPhase5Arrival ? currentGoodsItem?.Bill?.ArrivalGoodsItems : IsPhase5 ? currentGoodsItem?.Bill?.GoodsItems : currentGoodsItem?.MoveHeader?.GoodsItems;

		return IsArrivalMovement ?
			goodsItems?.Cast<NctsArrivalCargoDesc>().SelectMany(x => x.Packages.Cast<NctsPackage>()).ToArray() ?? System.Array.Empty<ICheckablePackage>() :
			goodsItems?.Cast<NctsDepartureCargoDesc>().SelectMany(x => x.Packages.Cast<NctsPackage>()).ToArray() ?? System.Array.Empty<ICheckablePackage>();
	}

	public override ZString B5_MarksAndNumbers
	{
		get => base.B5_MarksAndNumbers;
		set
		{
			var oldValue = B5_MarksAndNumbers;
			base.B5_MarksAndNumbers = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != B5_MarksAndNumbers)
			{
				Validation.ValidateB5_UnitCount();
			}
		}
	}

	protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
	{
		if (property.Name == NctsPackage.Schema.B5_TypeOfDifference && IsUnloadingRemarksReadOnlyOrNoChangesToReport)
		{
			return true;
		}
		else if (property.Name == NctsPackage.Schema.B5_GrossWeight && B5_TypeOfDifference != NctsUnloadedStateList.Codes.MIS)
		{
			return false;
		}
		else
		{
			return base.GetShouldPropertiesBeReadOnly(property);
		}
	}

	ZString ICheckablePackage.UnitType => B5_UnitType;

	ZString ICheckablePackage.MarksAndNumbers => B5_MarksAndNumbers;

	ZLong ICheckablePackage.UnitCount => B5_UnitCount;

	ZPropertyInfo ICheckablePackage.UnitCountInfo => B5_UnitCountInfo;

	#endregion

	public override void OnSaving()
	{
		if (IsPhase5Arrival && IsUnloadingRemarksReadOnlyOrNoChangesToReport)
		{
			if (!B5_GrossWeightInfo.HasChanges)
			{
				if (IsDataLoadFromDeparture)
				{
					IsDataLoadFromDeparture = false;
				}
				else
				{
					Delete();
				}
			}
		}
		base.OnSaving();
	}

	public override void OnSaved(bool saveSucceeded)
	{
		base.OnSaved(saveSucceeded);
		NoReadOnlyPackages();
	}

	void NoReadOnlyPackages()
	{
		if (IsPhase5Arrival)
		{
			Parent.Header.GetGoodsItems()
				.Cast<NctsArrivalCargoDesc>()
				.Select(x => x.Packages)
				.ForEach(x => x.SetReadOnlyIncludingChildren(false));
			RefreshBindingIncludingChildren();
			Parent.Header.Bills.RefreshBindingIncludingChildren();
		}
	}

	bool IsUnloadingRemarksReadOnlyOrNoChangesToReport => IsUnloadingRemarksReadOnly || Parent.Header.ArrivalMovementHeader.BM_NoChangesToReport;
}
