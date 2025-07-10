using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemDispatchTransportationUnitWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WhsItemDispatchTransportationUnitWrapper(WhsItemDispatchTransportationUnit header, BusinessObjectFactory factory)
			: base(header, factory)
		{
		}

		#endregion

		#region DispatchDriverName

		protected override ZString DispatchDriverNameCore => DispatchTransportationUnitBO != null ? DispatchTransportationUnitBO.WDH_SignedBy : ZString.Empty;

		#endregion

		#region DispatchDriverSignature

		protected override Image DispatchDriverSignatureCore
		{
			get
			{
				if (dispatchDriverSignature == null || dispatchDriverSignature.IsDisposed())
				{
					dispatchDriverSignature = DispatchTransportationUnitBO != null ? new SignatureDrawer(DispatchTransportationUnitBO.WDH_SignedBySignature).Image : null;
				}
				return dispatchDriverSignature;
			}
		}

		Image dispatchDriverSignature;

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore
		{
			get { return Res.GetString("5f8759b9-4b17-422e-bdd0-a35a77cbf7a7", "DTU ID"); }
		}

		protected override ZString JobNumberCore
		{
			get { return DispatchTransportationUnitBO.WDH_ReferenceNumber; }
		}

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = DispatchTransportationUnitBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();
			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = DispatchTransportationUnitBO.Warehouse;
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
					DispatchTransportationUnitBO.Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty,
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("9f448d39-f071-429f-8b98-3c3794d64e53", "Warehouse");

		#endregion

		#region ClientRequestedBillToParty

		protected override OrganisationWrapper ClientRequestedBillToPartyCore => new OrganisationWrapper(OrganisationUsageType.BillToParty, DispatchTransportationUnitBO.ClientRequestedBillToPartyDocAddress, Factory);

		#endregion

		#region VehicleReference

		public override LabelValuePairWrapper VehicleReference
		{
			get
			{
				if (DispatchTransportationUnitBO.IsContainerUnitType)
				{
					return new LabelValuePairWrapper(Res.GetString("bd5ada0c-43d2-41d3-9d05-6c7e16169de4", "Container Reference"), DispatchTransportationUnitBO.WDH_VehicleReference, Factory);
				}
				else
				{
					return new LabelValuePairWrapper(Res.GetString("e764132e-ccc8-4063-9f7d-608776f0a575", "Vehicle Reference"), DispatchTransportationUnitBO.WDH_VehicleReference, Factory);
				}
			}
		}

		#endregion

		#region NewWarehouseContainerLineWrapperCollection

		protected override ContainerWrapperCollection NewWarehouseContainerLineWrapperCollection()
		{
			var container = DispatchTransportationUnitBO.Container?.Package;
			return new ContainerWrapperCollection(container, Factory);
		}

		#endregion

		#region Seal

		protected override ZString SealCore
		{
			get
			{
				if (seal.IsEmpty)
				{
					seal = GetSeal();
				}
				return seal;
			}
		}

		ZString GetSeal()
		{
			if (DispatchTransportationUnitBO.HasContainerEquipmentDetails)
			{
				return DispatchTransportationUnitBO.Container?.K0_Seal1 ?? ZString.Empty;
			}
			else
			{
				return DispatchTransportationUnitBO.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.SealNumber)?.CE_EntryNum ?? ZString.Empty;
			}
		}
		ZString seal;

		#endregion

		#region PrimaryBarcodeText

		public override ZString PrimaryBarcodeText => new TextBarcode(JobNumber).TextAs128sFontString;

		#endregion

		#region CustomerReferenceBarcode

		protected override ZString CustomerReferenceBarcodeCore => (DispatchTransportationUnitBO != null && !string.IsNullOrEmpty(DispatchTransportationUnitBO.WDH_VehicleReference)) ?
			new TextBarcode(DispatchTransportationUnitBO.WDH_VehicleReference).TextAs128sFontString : ZString.Empty;

		#endregion

		#region DispatchLoadLists

		protected override WhsItemDispatchLoadListWrapperCollection GetDispatchLoadLists()
		{
			return new WhsItemDispatchLoadListWrapperCollection(DispatchTransportationUnitBO.DispatchLoadLists, Factory);
		}

		#endregion

		#region JobType

		protected override ZString JobTypeCore => DispatchTransportationUnitBO.WDH_UnitType;

		#endregion

		#region NextDischargePort

		protected override ZString NextDischargePortCore => DispatchTransportationUnitBO.WDH_RL_NKNextDischarge;

		#endregion

		#region StartTime

		protected override ZDateTime StartTimeCore => DispatchTransportationUnitBO.WDH_LoadStartTime.ToLocalZDateTime();

		#endregion

		#region CompleteTime

		protected override ZDateTime CompleteTimeCore => DispatchTransportationUnitBO.WDH_LoadCompleteTime.ToLocalZDateTime();

		#endregion

		#region FinalizedTime

		protected override ZDateTime FinalizedTimeCore => DispatchTransportationUnitBO.WDH_FinalisedTime.ToLocalZDateTime();

		#endregion

		#region GateInTime

		protected override ZDateTime GateInTimeCore => DispatchTransportationUnitBO.WDH_GateInTime.ToLocalZDateTime();

		#endregion

		#region GateOutTime

		protected override ZDateTime GateOutTimeCore => DispatchTransportationUnitBO.WDH_GateOutTime.ToLocalZDateTime();

		#endregion

		#region ContainerType

		protected override ZString ContainerTypeCore => DispatchTransportationUnitBO.ContainerTypeCode;

		#endregion

		#region Implementation

		WhsItemDispatchTransportationUnit DispatchTransportationUnitBO => dispatchTransportationUnitBO ?? (dispatchTransportationUnitBO = (WhsItemDispatchTransportationUnit)WrappedBO);
		WhsItemDispatchTransportationUnit dispatchTransportationUnitBO;

		#endregion
	}
}
