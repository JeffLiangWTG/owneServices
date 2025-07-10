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
	public class WhsItemReceiveTransportationUnitWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WhsItemReceiveTransportationUnitWrapper(WhsItemReceiveTransportationUnit header, BusinessObjectFactory factory)
			: base(header, factory)
		{
		}

		#endregion

		#region Headers

		protected override ZString JobNumberHeadingCore
		{
			get { return Res.GetString("a774fd18-7ce3-4a57-9fc9-8dd6a22e136b", "RTU ID"); }
		}

		protected override ZString JobNumberCore
		{
			get { return ReceiveTransportationUnitBO.WRH_ReferenceNumber; }
		}

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
		{
			var packages = ReceiveTransportationUnitBO.PackageStates.Select(packageState => packageState.Package).Where(package => package != null).ToList();
			return new TransitPackageWrapperCollection(packages, PackageWrapperCollection.PackLevel.First, Factory);
		}

		#endregion

		#region ReceiveDriverName

		protected override ZString ReceiveDriverNameCore => ReceiveTransportationUnitBO != null ? ReceiveTransportationUnitBO.WRH_SignedBy : ZString.Empty;

		#endregion

		#region ReceiveDriverSignature

		protected override Image ReceiveDriverSignatureCore
		{
			get
			{
				if (receiveDriverSignature == null || receiveDriverSignature.IsDisposed())
				{
					receiveDriverSignature = ReceiveTransportationUnitBO != null ? new SignatureDrawer(ReceiveTransportationUnitBO.WRH_SignedBySignature).Image : null;
				}
				return receiveDriverSignature;
			}
		}

		Image receiveDriverSignature;

		#endregion

		#region Warehouse

		protected override WarehouseBOWrapper WarehouseCore
		{
			get
			{
				var warehouse = ReceiveTransportationUnitBO.Warehouse;
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
					ReceiveTransportationUnitBO.Warehouse?.WW_WarehouseNameMultilingual ?? ZString.Empty,
					Factory);
			}
		}

		ZString WarehouseTitle => Res.GetString("9f448d39-f071-429f-8b98-3c3794d64e53", "Warehouse");

		#endregion

		#region StagingLocationString

		public override LabelValuePairWrapper StagingLocationString
		{
			get
			{
				return new LabelValuePairWrapper(Res.GetString("04247421-201D-4A67-A865-9DB00BAB309D", "Staging Location"),
					ReceiveTransportationUnitBO.Location?.WLV_LocationString ?? ZString.Empty,
					Factory);
			}
		}

		#endregion

		#region VehicleReference

		public override LabelValuePairWrapper VehicleReference
		{
			get
			{
				if (ReceiveTransportationUnitBO.IsContainerUnitType)
				{
					return new LabelValuePairWrapper(Res.GetString("FA4C1F2A-299A-4800-B2F4-629E865C2AB3", "Container Reference"), ReceiveTransportationUnitBO.WRH_VehicleReference, Factory);
				}
				else
				{
					return new LabelValuePairWrapper(Res.GetString("F6E144F3-6786-449B-95EF-347B7B9CD51E", "Vehicle Reference"), ReceiveTransportationUnitBO.WRH_VehicleReference, Factory);
				}
			}
		}

		#endregion

		#region TransportCompany

		public override OrganisationWrapper TransportCompany
		{
			get { return new OrganisationWrapper(OrganisationUsageType.TransportCompany, ReceiveTransportationUnitBO.TransportCompany, Factory); }
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
			if (ReceiveTransportationUnitBO.HasContainerEquipmentDetails)
			{
				return ReceiveTransportationUnitBO.Container?.K0_Seal1 ?? ZString.Empty;
			}
			else
			{
				return ReceiveTransportationUnitBO.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.SealNumber)?.CE_EntryNum ?? ZString.Empty;
			}
		}
		ZString seal;

		#endregion

		#region Dates

		protected override ZDateTime StartTimeCore => ReceiveTransportationUnitBO.WRH_UnloadStartTime.ToLocalZDateTime();

		protected override ZDateTime GetUnloadCompleteTime() => ReceiveTransportationUnitBO.WRH_UnloadCompleteTime.ToLocalZDateTime();

		#endregion

		#region PrimaryBarcodeText

		public override ZString PrimaryBarcodeText => new TextBarcode(JobNumber).TextAs128sFontString;

		#endregion

		#region CustomerReferenceBarcode

		protected override ZString CustomerReferenceBarcodeCore => (ReceiveTransportationUnitBO != null && !string.IsNullOrEmpty(receiveTransportationUnitBO.WRH_VehicleReference)) ?
			new TextBarcode(ReceiveTransportationUnitBO.WRH_VehicleReference).TextAs128sFontString : ZString.Empty;

		#endregion

		#region JobType

		protected override ZString JobTypeCore => ReceiveTransportationUnitBO.WRH_UnitType;

		#endregion

		#region ContainerType

		protected override ZString ContainerTypeCore => ReceiveTransportationUnitBO.ContainerTypeCode;

		#endregion

		#region IsSecure

		protected override ZBool IsSecureCore => ReceiveTransportationUnitBO.WRH_IsVehicleSecure;

		#endregion

		#region GateInTime

		protected override ZDateTime GateInTimeCore => ReceiveTransportationUnitBO.WRH_GateInTime.ToLocalZDateTime();

		#endregion

		#region GateOutTime

		protected override ZDateTime GateOutTimeCore => ReceiveTransportationUnitBO.WRH_GateOutTime.ToLocalZDateTime();

		#endregion

		#region WarehouseExpectedArrivalTime

		protected override ZDateTime GetWarehouseExpectedArrivalTime => ReceiveTransportationUnitBO.ETA;

		#endregion

		#region ExpectedDispatchTime

		protected override ZDateTime ExpectedDispatchTimeCore => ReceiveTransportationUnitBO.ETD;

		#endregion

		#region ClientRequestedBillToParty

		protected override OrganisationWrapper ClientRequestedBillToPartyCore => new OrganisationWrapper(OrganisationUsageType.BillToParty, ReceiveTransportationUnitBO.ClientRequestedBillToPartyDocAddress, Factory);

		#endregion

		#region Implementation

		WhsItemReceiveTransportationUnit ReceiveTransportationUnitBO => receiveTransportationUnitBO ?? (receiveTransportationUnitBO = (WhsItemReceiveTransportationUnit)WrappedBO);
		WhsItemReceiveTransportationUnit receiveTransportationUnitBO;

		#endregion
	}
}
