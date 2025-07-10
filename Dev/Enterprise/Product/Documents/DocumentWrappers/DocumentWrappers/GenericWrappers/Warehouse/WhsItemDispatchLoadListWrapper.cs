using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemDispatchLoadListWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WhsItemDispatchLoadListWrapper(WhsItemDispatchLoadList dispatchLoadList, BusinessObjectFactory factory)
			: base(dispatchLoadList, factory)
		{
		}

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore
		{
			get { return Res.GetString("a670e9ee-17f7-4aa1-93c9-9ec3a4fa9c10", "Dispatch Load List ID"); }
		}

		protected override ZString JobNumberCore
		{
			get { return DispatchLoadListBO.WDL_JobID; }
		}

		#endregion

		#region SecondaryHeaders

		protected override ZString SecondaryHeadingCore => Res.GetString("e7e666be-ff1f-4417-9ed4-0ea173453ee3", "Dispatch Load List Reference Number");

		protected override ZString SecondaryNumberCore => DispatchLoadListBO != null ? DispatchLoadListBO.WDL_ReferenceNumber : ZString.Empty;

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = DispatchLoadListBO.Warehouse;
				return warehouse != null
					? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory))
					: null;
			}
		}

		#endregion

		#region WarehouseName

		public override LabelValuePairWrapper WarehouseName
		{
			get
			{
				return new LabelValuePairWrapper(WarehouseTitle,
					DispatchLoadListBO.Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty,
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("9f448d39-f071-429f-8b98-3c3794d64e53", "Warehouse");

		#endregion

		#region StagingLocation

		public override LabelValuePairWrapper StagingLocationString
		{
			get
			{
				return new LabelValuePairWrapper(Res.GetString("3574c8da-11ab-449a-b2b5-2acacf3f8c7a", "Staging Location"),
					DispatchLoadListBO.Location?.WLV_LocationString ?? ZString.Empty,
					Factory);
			}
		}

		#endregion

		#region MasterBill

		protected override ZString MasterBillCore
		{
			get
			{
				var masterBillReference = DispatchLoadListBO.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
				return masterBillReference?.CE_EntryNum ?? ZString.Empty;
			}
		}

		protected override ZString MasterBillHeadingCore => HeadingHelper.GetMasterBillHeading(DispatchLoadListBO.WDL_TransportMode);

		#endregion

		#region DispatchTransportationUnits

		protected override WhsItemDispatchTransportationUnitWrapperCollection GetDispatchTransportationUnits()
		{
			return new WhsItemDispatchTransportationUnitWrapperCollection(DispatchLoadListBO.DispatchTransportationUnits, Factory);
		}

		#endregion

		public override ZString AssignedLoader
		{
			get
			{
				var assignedLoaderReference = DispatchLoadListBO.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
				return assignedLoaderReference?.CE_EntryNum ?? ZString.Empty;
			}
		}

		#region AllDCNsAreAuthorized

		public override ZBool AllDCNsAreAuthorized => !DispatchLoadListBO.PackageStates.Any(packageState => !packageState.DispatchConsignment.WDC_IsAuthorizedForDispatch);

		#endregion

		#region IsAwaitingForwardingChanges

		protected override ZBool IsAwaitingForwardingChangesCore => DispatchLoadListBO.WDL_IsAwaitingForwardingChanges;

		#endregion

		#region IsReadyToStage

		protected override ZBool IsReadyToStageCore => DispatchLoadListBO.WDL_IsReadyToStage;

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = DispatchLoadListBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();

			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region TotalInnerPackLines

		protected override ZShort TotalInnerPackLinesCore => (ZShort)DispatchLoadListBO.PackageStates.Where(pkg => pkg.Package.KP_KPH_PackageHeader == ZGuid.Empty
			&& pkg.Package.KP_KP_TopHandlingUnitPackage != ZGuid.Empty
			&& pkg.TopHandlingUnit.WPS_UnitType == "HU")
			.Sum(pkg => pkg.Package.KP_PackageQty);

		#endregion

		#region TotalInners

		protected override ZShort TotalInnersCore => (ZShort)DispatchLoadListBO.PackageStates.Where(pkg => pkg.Package.KP_KP_TopHandlingUnitPackage != ZGuid.Empty
			&& pkg.TopHandlingUnit.WPS_UnitType == "HU"
			&& pkg.WPS_UnitType != "HU")
			.Sum(pkg => pkg.Package.KP_PackageQty);

		#endregion

		#region TotalOverpacks

		protected override ZShort TotalOverpacksCore => (ZShort)DispatchLoadListBO.PackageStates.Count(pkg => pkg.WPS_UnitType == "OVP" && pkg.Package.KP_KP_ParentPackage == ZGuid.Empty);

		#endregion

		#region JobType

		protected override ZString JobTypeCore => DispatchLoadListBO.LoadListType;

		#endregion

		#region Destination

		public override PlaceAndDateWrapper Destination
		{
			get { return DispatchLoadListBO.WDL_RL_NKLastDischargePort.IsEmpty ?  null : new PlaceAndDateWrapper(DispatchLoadListBO.WDL_RL_NKLastDischargePort, ZDateTime.Empty, ZDateTime.Empty, Factory); }
		}

		#endregion

		#region TransportMode

		protected override ZString TransportModeCore => DispatchLoadListBO.WDL_TransportMode;

		#endregion

		#region CutoffTime

		protected override ZDateTime CutoffTimeCore => DispatchLoadListBO.WDL_CTOCutOffTime.ToLocalZDateTime();

		#endregion

		#region ExpectedDispatchTime

		protected override ZDateTime ExpectedDispatchTimeCore => DispatchLoadListBO.WDL_ExpectedDispatchTime.ToLocalZDateTime();

		#endregion

		#region CompleteTime

		protected override ZDateTime CompleteTimeCore => DispatchLoadListBO.WDL_CompleteTime.ToLocalZDateTime();

		#endregion

		#region Implementation

		WhsItemDispatchLoadList DispatchLoadListBO => dispatchLoadListBO ?? (dispatchLoadListBO = (WhsItemDispatchLoadList)WrappedBO);
		WhsItemDispatchLoadList dispatchLoadListBO;

		#endregion

		#region IsFinalised

		protected override ZBool IsFinalisedCore => DispatchLoadListBO.IsFinalised;

		#endregion

	}
}
