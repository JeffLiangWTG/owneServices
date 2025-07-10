using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageStateWrapper : GenericWrapper
	{
		public PackageStateWrapper(PackageWrapperFromPkgPackage packageWrapper, BusinessObjectFactory factory)
			: base(packageWrapper != null ? (BusinessObject)packageWrapper.WrappedObject : factory.GetNull<PkgPackage>(), factory)
		{
			var package = WrappedBO as PkgPackage;

			this.PackageState =
				package == null ?
				factory.GetNull<WhsItemPackageState>() :
				factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.PK));
		}

		public PackageStateWrapper(WhsItemPackageState packageState, BusinessObjectFactory factory)
			: base(packageState, factory)
		{
			this.PackageState = packageState ?? factory.GetNull<WhsItemPackageState>();
		}

		#region ReceiveConsignment

		public FreightWrapper ReceiveConsignment
		{
			get
			{
				FreightWrapper consignmentWrapper = null;
				if (PackageState != null)
				{
					var consignment = PackageState.ReceiveConsignment;
					if (consignment != null)
					{
						consignmentWrapper = FreightWrapper.New(consignment, Factory).FirstOrDefault();
					}
				}
				return consignmentWrapper;
			}
		}

		#endregion

		#region ReceiveExpectedPacking

		public FreightWrapper ReceiveASN
		{
			get
			{
				FreightWrapper wrapper = null;
				if (PackageState != null)
				{
					var receiveASN = PackageState.ReceiveASN;
					if (receiveASN != null)
					{
						wrapper = FreightWrapper.New(receiveASN, Factory).FirstOrDefault();
					}
				}
				return wrapper;
			}
		}

		#endregion

		#region Warehouse

		public WarehouseBOWrapper Warehouse
		{
			get
			{
				WarehouseBOWrapper warehouseWrapper = null;
				if (PackageState != null)
				{
					var warehouse = PackageState.Warehouse;

					warehouseWrapper = warehouse != null
						? Factory.GetCachedValue(warehouse.PK.ToString(), () => new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory))
						: new WarehouseBOWrapper(WarehouseTitle, warehouse, Factory);
				}

				return warehouseWrapper;
			}
		}

		ZString WarehouseTitle => Res.GetString("PackageStateWrapper|WarehouseTitle", "Warehouse");

		#endregion

		#region DispatchConsignment

		public FreightWrapper DispatchConsignment
		{
			get
			{
				FreightWrapper consignmentWrapper = null;
				if (PackageState != null)
				{
					var consignment = PackageState.DispatchConsignment;
					if (consignment != null)
					{
						consignmentWrapper = FreightWrapper.New(consignment, Factory).FirstOrDefault();
					}
				}
				return consignmentWrapper;
			}
		}

		#endregion

		#region DispatchTransportationUnit

		public FreightWrapper DispatchTransportationUnit
		{
			get
			{
				FreightWrapper headerWrapper = null;
				if (PackageState != null)
				{
					var unit = PackageState.DispatchTransportationUnit;
					if (unit != null)
					{
						headerWrapper = FreightWrapper.New(unit, Factory).FirstOrDefault();
					}
				}
				return headerWrapper;
			}
		}

		#endregion

		#region ReceiveTransportationUnit

		public FreightWrapper ReceiveTransportationUnit
		{
			get
			{
				FreightWrapper headerWrappers = null;
				if (PackageState != null)
				{
					var unit = PackageState.ReceiveTransportationUnit;
					if (unit != null)
					{
						headerWrappers = FreightWrapper.New(unit, Factory).FirstOrDefault();
					}
				}
				return headerWrappers;
			}
		}

		#endregion

		#region PlannedReceiveTransportationUnits

		public FreightWrapperCollection PlannedReceiveTransportationUnits
		{
			get
			{
				FreightWrapperCollection headerWrappers = null;
				if (PackageState != null && PackageState.ReceiveASN != null)
				{
					var rtuUnits = PackageState.ReceiveASN.PlannedReceiveTransportationUnits.ToList();
					if (rtuUnits != null)
					{
						headerWrappers = new FreightWrapperCollection(rtuUnits, Factory);
					}
				}
				return headerWrappers;
			}
		}

		#endregion

		#region ConsignmentID

		public ZString ConsignmentID
		{
			get
			{
				var result = "";

				if (PackageState != null)
				{
					var rcnConsignment = PackageState.ReceiveConsignment;
					if (rcnConsignment != null)
					{
						result = rcnConsignment.WRC_ConsignmentID;
					}

					if (string.IsNullOrWhiteSpace(result))
					{
						var dcnConsignment = PackageState.DispatchConsignment;
						if (dcnConsignment != null)
						{
							result = dcnConsignment.WDC_ConsignmentID;
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region DispatchConsignmentID

		public ZString DispatchConsignmentID
		{
			get
			{
				var result = ZString.Empty;

				if (PackageState != null && PackageState.DispatchConsignment != null)
				{
					var dcnConsignment = PackageState.DispatchConsignment;
					if (dcnConsignment != null)
					{
						result = dcnConsignment.WDC_ConsignmentID;
					}
				}

				return result;
			}
		}

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get
			{
				var result = ZString.Empty;
				if (PackageState != null)
				{
					var dispatchLoadList = PackageState.DispatchLoadList;
					if (dispatchLoadList != null)
					{
						var masterBillReferenceInDLL = dispatchLoadList.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
						result = masterBillReferenceInDLL?.CE_EntryNum ?? ZString.Empty;
					}

					if (result.IsEmpty)
					{
						var consignment = PackageState.ReceiveConsignment;
						if (consignment != null)
						{
							var masterBillReference = PackageState.ReceiveConsignment.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
							result = masterBillReference?.CE_EntryNum ?? ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Arrived

		public ZDateTime Arrived
		{
			get
			{
				var result = ZDateTimeOffset.Empty;
				if (PackageState != null)
				{
					result = PackageState.WPS_UnloadedTime;
				}

				return result.ToLocalZDateTime();
			}
		}

		#endregion

		#region HasArrived

		public ZBool HasArrived => PackageState?.LastLocation != null;

		#endregion

		#region Departed

		public ZDateTime Departed
		{
			get
			{
				var result = ZDateTimeOffset.Empty;
				if (PackageState != null && PackageState.DispatchTransportationUnit != null)
				{
					result = PackageState.DispatchTransportationUnit.WDH_GateOutTime;
				}

				return result.ToLocalZDateTime();
			}
		}

		#endregion

		#region HasDeparted

		public ZBool HasDeparted => Departed.IsValid;

		#endregion

		#region Status

		public CodeAndDescriptionWrapper Status
		{
			get
			{
				var result = CodeAndDescriptionWrapper.Empty;
				if (PackageState != null)
				{
					var code = (ZString)PackageState[WhsItemPackageStateSchema.WPS_Status];
					if (TWHStatus.ContainsCode(code))
					{
						result = new CodeAndDescriptionWrapper(code, TWHStatus.GetDescriptionFromCode(code), Factory);
					}
				}
				return result;
			}
		}

		#endregion

		#region Flags

		public ZBool IsPicked => PackageState != null && PackageState.IsPicked;

		public ZBool IsLoaded => PackageState != null && PackageState.IsLoaded;

		public ZBool IsDamaged => PackageState != null && PackageState.Package != null && PackageState.Package.KP_IsDamaged;

		public ZBool IsHandlingUnit => PackageState != null && PackageState.WPS_IsHandlingUnit;

		public ZBool IsHighRisk => PackageState != null && PackageState.WPS_IsHighRisk;

		public ZBool IsHighRiskAuthorized => PackageState != null && PackageState.WPS_IsHighRiskAuthorized;

		#endregion

		#region Location

		public ZString Location
		{
			get
			{
				ZString result = ZString.Empty;

				if (PackageState != null)
				{
					var receiveLocation = PackageState.WPS_WL_LastLocation;

					var location = Factory.Load<WhsLocation>(receiveLocation);
					if (location != null)
					{
						result = location.WLV_LocationString;
					}
				}

				return result;
			}
		}

		#endregion

		#region PackageState

		readonly WhsItemPackageState PackageState;

		#endregion

		#region LoadedTime

		public ZDateTime LoadedTime => (PackageState?.WPS_LoadedTime ?? ZDateTimeOffset.Empty).ToLocalZDateTime();

		#endregion

		#region UnloadNotYetProcessedTime

		public ZDateTime UnloadedNotYetProcessedTime => (PackageState?.WPS_UnloadedNotYetProcessedTime ?? ZDateTimeOffset.Empty).ToLocalZDateTime();

		#endregion

		#region TransferRelatedTimes

		ZDateTime GetLatestTransferTime(ZString transferType, Func<WhsItemTransferLine, ZDateTimeOffset> timeSelector)
		{
			if (PackageState?.PK == null || PackageState.PK == ZGuid.Empty)
			{
				return ZDateTime.Empty;
			}

			var query = new ZDBOnlyQuery(typeof(WhsItemTransferLine));

			var headers = new ZDBOnlySubQuery(typeof(WhsItemTransferHeader), WhsItemTransferLineSchema.WTF_WTH_TransitTransferHeader);
			headers.AddToFilter(WhsItemTransferHeaderSchema.WTH_TransferType, SQLComparisonOperator.Equal, transferType);
			headers.AddToFilter(JoinCondition.And, WhsItemTransferLineSchema.WTF_WPS_PackageState, SQLComparisonOperator.Equal, PackageState.PK);

			query.AddSubQuery(headers, JoinCondition.And);

			var transferLines = Factory.Load<WhsItemTransferLine>(query);

			return (transferLines?.Where(t => timeSelector(t) != ZDateTimeOffset.Empty).OrderByDescending(timeSelector).FirstOrDefault() is { } line ? timeSelector(line) : ZDateTimeOffset.Empty).ToLocalZDateTime();
		}

		public ZDateTime PutawayTransferPickTime => GetLatestTransferTime(TransferTypes.Codes.PUT, t => t.WTF_PickTime);

		public ZDateTime PutawayTransferPutTime => GetLatestTransferTime(TransferTypes.Codes.PUT, t => t.WTF_PutTime);

		public ZDateTime CrossDockTransferPickTime => GetLatestTransferTime(TransferTypes.Codes.XDK, t => t.WTF_PickTime);
		
		public ZDateTime CrossDockTransferPutTime => GetLatestTransferTime(TransferTypes.Codes.XDK, t => t.WTF_PutTime);

		public ZDateTime PickTransferPickTime => GetLatestTransferTime(TransferTypes.Codes.PIC, t => t.WTF_PickTime);

		public ZDateTime PickTransferPutTime => GetLatestTransferTime(TransferTypes.Codes.PIC, t => t.WTF_PutTime);

		#endregion

		#region JobType

		public ZString JobType
		{
			get
			{
				var result = ZString.Empty;

				if (PackageState != null)
				{
					result = PackageState.WPS_UnitType;
				}

				return result;
			}
		}

		#endregion

		TransitWarehouseStatuses TWHStatus
		{
			get
			{
				if (twhstatus == null)
				{
					twhstatus = new TransitWarehouseStatuses();
				}
				return twhstatus;
			}
		}

		TransitWarehouseStatuses twhstatus;
	}
}
