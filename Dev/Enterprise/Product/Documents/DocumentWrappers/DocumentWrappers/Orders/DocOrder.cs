using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Freight;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocOrder : DocBaseWrapper, Integration.DocumentWrappers.IDocOrder, IPreAlert, IRequestForMissingDocuments
	{
		#region Constructors

		protected DocOrder(Order order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		public static DocOrder New(Order order, BusinessObjectFactory factoryToWrap)
		{
			DocOrder result = null;

			if (order != null)
			{
				var overridden = OverridableNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden(order, factoryToWrap);
				}
				else
				{
					return new DocOrder(order, factoryToWrap);
				}
			}

			return result;
		}

		protected delegate DocOrder NewDelegate(Order order, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Overrides

		public override string ToString()
		{
			return JobNumber;
		}

		#endregion

		#region ZString Fields
		public ZString OrderNumber
		{
			get { return Order.JD_OrderNumber; }
		}

		public ZString ClientVisibleJobNotes
		{
			get
			{
				StmNote[] notes = this.Order.Notes.FindByDescription(PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description);
				return (notes.Length == 0) ? null : notes[0].ST_NoteDataAsText;
			}
		}

		public ZString OrderAndSplitNumber
		{
			get { return (Order != null) ? Order.JD_OrderNumberAndSplit : ZString.Empty; }
		}

		public ZString AdditionalTerms
		{
			get { return Order.JD_AdditionalTerms; }
		}

		public ZString ArrivalVoyage
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (Order.Shipment.ArrivalConsol != null)
					{
						result = Order.Shipment.ArrivalConsol.JK_JX_JV_VoyageFlight;
					}
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_VoyageFlightNo;
				}
				else
				{
					result = Order.JD_ArrivalVoyage;
				}

				return result;
			}
		}

		public ZString BookingConfRef
		{
			get { return Order.JD_BookingConfRef; }
		}

		public ZString ContainerMode
		{
			get
			{
				ZString result;

				if (IsShipmentAttached)
				{
					result = Order.Shipment.JS_PackingMode;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_ContainerMode;
				}
				else
				{
					result = Order.JD_ContainerMode;
				}

				return result;
			}
		}

		public ZString CustomAttrib1
		{
			get { return Order.JD_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return Order.JD_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return Order.JD_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return Order.JD_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return Order.JD_CustomAttrib5; }
		}

		public ZString FirstBuyerContact
		{
			get { return Order.JD_FirstBuyerContact; }
		}

		public ZString SecondBuyerContact
		{
			get { return Order.JD_SecondBuyerContact; }
		}

		public ZString DepartureVoyage
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (Order.Shipment.DepartureConsol != null)
					{
						result = Order.Shipment.DepartureConsol.JK_JX_JV_VoyageFlight;
					}
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_VoyageFlightNo;
				}
				else
				{
					result = Order.JD_DepartureVoyage;
				}

				return result;
			}
		}

		public ZString IncoTerm
		{
			get
			{
				ZString result;

				if (IsShipmentAttached)
				{
					result = Order.Shipment.JS_INCO;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_ShipmentIncoTerm;
				}
				else
				{
					result = Order.JD_IncoTerm;
				}

				return result;
			}
		}

		public ZString IntermediateVoyage
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (IntermediateConsol != null)
					{
						result = IntermediateConsol.JK_JX_JV_VoyageFlight;
					}
				}
				else if (!IsDeclarationAttached)
				{
					result = Order.JD_IntermediateVoyage;
				}

				return result;
			}
		}

		public ZString InvoiceNumber
		{
			get { return Order.JD_InvoiceNumber; }
		}

		public ZString OrderStatus
		{
			get { return Order.JD_OrderStatus; }
		}

		public ZString ServiceLevelCode
		{
			get
			{
				ZString result = Order.JD_RS_NKServiceLevel_NI;
				if (IsShipmentAttached)
				{
					result = Order.Shipment.JS_RS_NKServiceLevel;
				}
				return result;
			}
		}

		public ZString ServiceLevelDescription
		{
			get
			{
				ZString result = "";
				if (IsShipmentAttached)
				{
					if (Order.Shipment.ServiceLevel != null)
					{
						result = Order.Shipment.ServiceLevel.RS_DescriptionMultilingual;
					}
				}
				else if (Order.ServiceLevel_NI != null)
				{
					result = Order.ServiceLevel_NI.RS_DescriptionMultilingual;
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZString ShipmentConsigneeContact
		{
			get { return IsShipmentAttached ? Order.Shipment.ConsigneeDocumentaryAddress.E2_Contact : ZString.Empty; }
		}

		public ZString ShipmentConsignorContact
		{
			get { return IsShipmentAttached ? Order.Shipment.ConsignorDocumentaryAddress.E2_Contact : ZString.Empty; }
		}

		public ZString ArrivalVessel
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (Order.Shipment.ArrivalConsol != null)
					{
						result = Order.Shipment.ArrivalConsol.JK_JX_JV_NKVessel;
					}
				}
				else if (IsDeclarationAttached)
				{
					BaseJobDeclaration declaration = (BaseJobDeclaration)Order.Declaration;

					if (declaration.IsImport)
					{
						result = declaration.JE_VesselName;
					}
				}
				else
				{
					result = Order.JD_RV_NKArrivalVessel;
				}

				return result;
			}
		}

		public ZString DepartureVessel
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (Order.Shipment.DepartureConsol != null)
					{
						result = Order.Shipment.DepartureConsol.JK_JX_JV_NKVessel;
					}
				}
				else if (IsDeclarationAttached)
				{
					BaseJobDeclaration declaration = (BaseJobDeclaration)Order.Declaration;

					if (declaration.IsExport)
					{
						result = declaration.JE_VesselName;
					}
				}
				else
				{
					result = Order.JD_RV_NKDepartureVessel;
				}

				return result;
			}
		}

		ForwardingConsol IntermediateConsol
		{
			get
			{
				if (IsShipmentAttached && Order.Shipment.Consols.Count > 2)
				{
					foreach (ForwardingConsol shipConsol in Order.Shipment.Consols)
					{
						if (shipConsol != Order.Shipment.ArrivalConsol && shipConsol != Order.Shipment.DepartureConsol)
						{
							return shipConsol;
						}
					}
				}

				return null;
			}
		}

		public ZString IntermediateVessel
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					if (IntermediateConsol != null)
					{
						result = IntermediateConsol.JK_JX_JV_NKVessel;
					}
				}
				else if (!IsDeclarationAttached)
				{
					result = Order.JD_RV_NKIntermediateVessel;
				}

				return result;
			}
		}

		public ZString TransportMode
		{
			get
			{
				ZString result;
				if (IsShipmentAttached)
				{
					result = Order.Shipment.JS_TransportMode;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_TransportMode;
				}
				else
				{
					result = Order.JD_TransportMode;
				}
				return result;
			}
		}

		public ZBool TransportModeIsSea
		{
			get
			{
				ZBool result = ZBool.False;
				if (IsShipmentAttached)
				{
					result = Order.Shipment.IsSea;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).IsSea;
				}
				else
				{
					result = Order.IsSeaTransport;
				}
				return result;
			}
		}

		public ZString TransportModeDescription
		{
			get
			{
				ZString result;
				if (IsShipmentAttached)
				{
					result = Order.Shipment.Lookups.JS_TransportMode_List.GetDescriptionFromCode(Order.Shipment.JS_TransportMode);
				}
				else if (IsDeclarationAttached)
				{
					BaseJobDeclaration declaration = (BaseJobDeclaration)Order.Declaration;
					result = declaration.Lookups.TransportTypeList.GetDescriptionFromCode(declaration.JE_TransportMode);
				}
				else
				{
					result = Order.JD_TransportMode_List.GetDescriptionFromCode(Order.JD_TransportMode);
				}

				return result;
			}
		}

		public ZString PortOfLoadingName
		{
			get { return (Order.PortOfLoading == null) ? ZString.Empty : Order.PortOfLoading.RL_PortName; }
		}

		public ZString AllClientVisibleNotes
		{
			get
			{
				ZString result = "";
				foreach (StmNote note in this.Order.Notes.GetAllNotes())
				{
					if (note.ST_NoteType == nameof(StmNoteVisibility.PUB))
					{
						result += note.ST_NoteDataAsText + "\n\n";
					}
				}
				if (this.Order.Buyer != null)
				{
					foreach (StmNote note in this.Order.Buyer.Notes.GetAllNotes())
					{
						if (note.ST_NoteType == nameof(StmNoteVisibility.PUB))
						{
							result += note.ST_NoteDataAsText + "\n\n";
						}
					}
				}
				if (this.Order.Supplier != null)
				{
					foreach (StmNote note in this.Order.Supplier.Notes.GetAllNotes())
					{
						if (note.ST_NoteType == nameof(StmNoteVisibility.PUB))
						{
							result += note.ST_NoteDataAsText + "\n\n";
						}
					}
				}
				return result;
			}
		}

		public ZString JobNumberHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.JobNumberHeading;
				}
				return Res.GetString("3b096093-8f7a-47ea-88e2-fda048b34c3e", "ORDER");
			}
		}

		public ZString NotAllocatedWeight
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.NotAllocatedWeight;
				}
				return "";
			}
		}
		public ZString NotAllocatedVolume
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.NotAllocatedVolume;
				}
				return "";
			}
		}
		public ZString NotAllocatedPackages
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.NotAllocatedPackages;
				}
				return "";
			}
		}

		public ZString AlertText
		{
			get
			{ return DocumentsDataRegistry.Instance.OrderDelayAlertText.Value; }
		}

		public ZString ReleaseType
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ReleaseType;
				}

				return Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised");
			}
		}

		public ZString OrderNumbers
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.OrderNumbers;
				}

				return JobNumber;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				ZString result;

				if (IsShipmentAttached)
				{
					result = Shipment.GoodsDescription;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_GoodsDescription;
				}
				else
				{
					result = Order.JD_OrderGoodsDescription;
				}

				return result;
			}
		}

		public DocDocAddress GoodsAvailableAtOrder
		{
			get
			{
				return DocDocAddress.New(Order.GoodsAvailableAtAddress, Factory);
			}
		}

		public DocDocAddress GoodsDeliveredToOrder
		{
			get
			{
				return DocDocAddress.New(Order.GoodsDeliveredToAddress, Factory);
			}
		}

		public ZString CommodityCode
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.Commodity.Code;
				}

				return Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised");
			}
		}

		public ZString CommodityDescription
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.Commodity.Description;
				}

				return ZString.Empty;
			}
		}

		public ZString Weight
		{
			get { return (IsShipmentAttached) ? Shipment.Weight : (ZString)OrderWeight.ToString(); }
		}

		public ZString WeightUnit
		{
			get { return IsShipmentAttached ? Shipment.WeightUnit : UnitOfWeight; }
		}

		public ZString Volume
		{
			get { return IsShipmentAttached ? Shipment.Volume : (ZString)OrderVolume.ToString(); }
		}

		public ZString VolumeUnit
		{
			get { return IsShipmentAttached ? Shipment.VolumeUnit : (ZString)UnitOfVolume.ToString(); }
		}

		public ZString Chargeable
		{
			get { return IsShipmentAttached ? Shipment.Chargeable : (ZString)Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised"); }
		}

		public ZString ChargeableUnit
		{
			get { return IsShipmentAttached ? Shipment.ChargeableUnit : ZString.Empty; }
		}

		public DocUNLOCO OriginLoco
		{
			get
			{
				DocUNLOCO result = null;
				if (IsShipmentAttached)
				{
					result = Shipment.OriginLoco;
				}
				else if (IsDeclarationAttached)
				{
					result = DocUNLOCO.New(((BaseJobDeclaration)Order.Declaration).Origin, Factory);
				}
				else if (Order.GoodsAvailableAt != null)
				{
					result = DocUNLOCO.New(Order.GoodsAvailableAt, Factory);
				}

				return result;
			}
		}

		public ZString Origin
		{
			get { return (OriginLoco != null) ? GetPrintablePortFromDocUNLOCO(OriginLoco) : ZString.Empty; }
		}

		public ZString HouseBill
		{
			get
			{
				ZString result;

				if (IsShipmentAttached)
				{
					result = Shipment.HouseBill;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_HouseBill;
				}
				else if (!Order.JD_Waybill.IsEmpty)
				{
					result = Order.JD_Waybill;
				}
				else
				{
					result = Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised");
				}

				return result;
			}
		}

		public ZString Destination
		{
			get { return (DestinationLoco != null) ? GetPrintablePortFromDocUNLOCO(DestinationLoco) : ZString.Empty; }
		}

		public DocUNLOCO DestinationLoco
		{
			get
			{
				DocUNLOCO result = null;

				if (IsShipmentAttached)
				{
					result = Shipment.DestinationLoco;
				}
				else if (IsDeclarationAttached)
				{
					result = DocUNLOCO.New(((BaseJobDeclaration)Order.Declaration).FinalDestination, Factory);
				}
				else if (Order.GoodsDeliveredTo != null)
				{
					result = DocUNLOCO.New(Order.GoodsDeliveredTo, Factory);
				}

				return result;
			}
		}

		public ZString HouseBillHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.HouseBillHeading;
				}

				return Res.GetString("f16386a9-1c63-4fc5-9a1b-ef07e88ec389", "HOUSE BILL");
			}
		}

		public ZString HouseBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(HouseBillHeading, HouseBillIssueDate); }
		}

		public ZString Packages
		{
			get
			{
				ZString result = "";

				if (IsShipmentAttached)
				{
					result = Shipment.Packages;
				}
				else
				{
					if (ContainerMode == Core.Constants.ContainerModes.FCL)
					{
						ZDecimal totalPacks = 0;

						foreach (DocOrderLine currentOrder in OrderLines)
						{
							totalPacks += currentOrder.OuterPacks;
						}

						result = totalPacks.ToString();
					}
					else
					{
						result = OrderPacks.ToString() + " " + PacksType;
					}
				}

				return result;
			}
		}

		public ZString HazCat
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.HazCat;
				}

				return ZString.Empty;
			}
		}

		public ZString SealNumberHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.SealNumberHeading;
				}

				return Res.GetString("46f1a216-0e9c-41d7-ba0e-ea5c8d3944ff", "COUNT");
			}
		}

		public ZString SealHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.SealHeading;
				}

				return Res.GetString("46f1a216-0e9c-41d7-ba0e-ea5c8d3944ff", "COUNT");
			}
		}

		public ZString ContainerWeightHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ContainerWeightHeading;
				}

				return ZString.Empty;
			}
		}

		public ZString ContainerVolumeHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ContainerVolumeHeading;
				}

				return ZString.Empty;
			}
		}

		public ZString ContainerPackageHeading
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ContainerPackageHeading;
				}
				return ZString.Empty;
			}
		}

		public ZString NotifyPartyPostalAddress
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.NotifyPartyPostalAddress;
				}

				return Res.GetString("d61251fc-604c-4c6b-9581-bb29b548e945", "NOTIFY CONSIGNEE");
			}
		}

		public ZString UltimateNotification
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.UltimateNotification;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString HeadingTransportMode
		{
			get
			{
				ZString heading = ZString.Empty;

				if (IsShipmentAttached)
				{
					heading = Shipment.HeadingTransportMode;
				}
				else
				{
					if (TransportMode == Core.Constants.TransportModes.Sea && (ContainerMode == Core.Constants.ContainerModes.FCL || ContainerMode == Core.Constants.ContainerModes.LCL))
					{
						heading += ContainerMode + " ";
					}

					heading += Order.JD_TransportMode_List.GetDescriptionFromCode(TransportMode);
				}

				return heading;
			}
		}

		public ZString PreAlertArrivalNoticeRemarks
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, Order); }
		}

		public ZString PacksType
		{
			get { return Order.JD_F3_NKPackType.IsEmpty ? (ZString)Core.Constants.PkgUnit.Package : Order.JD_F3_NKPackType; }
		}

		public ZString UnitOfVolume
		{
			get { return Order.JD_UnitOfVolume.IsEmpty ? (ZString)Core.Constants.Volume.CubicMetres : Order.JD_UnitOfVolume; }
		}

		public ZString UnitOfWeight
		{
			get { return Order.JD_UnitOfWeight.IsEmpty ? (ZString)Core.Constants.Weight.Kilograms : Order.JD_UnitOfWeight; }
		}

		ZString GetPrintablePortFromDocUNLOCO(DocUNLOCO port)
		{
			ZString result = "";

			if (port != null)
			{
				result = port.Code + " = " + port.PortName + ", " + port.CountryName;
			}

			return result;
		}

		public ZString ShipmentBrokerageNumber
		{
			get { return Order.JD_CalcShipmentBrokerageNumber; }
		}

		public ZString ShipmentBrokerageNumberHeading
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsShipmentAttached)
				{
					result = Res.GetString("3c1bed78-b2be-439c-8101-5088a9c46e58", "SHIPMENT NO:");
				}
				else if (IsDeclarationAttached)
				{
					result = Res.GetString("53e16bbe-832e-4ffc-ab54-15317c79a511", "DECLARATION NO:");
				}

				return result;
			}
		}
		#endregion

		#region ZInt Fields

		public ZInt OrderPacks
		{
			get { return Order.JD_Packs; }
		}

		public ZInt OrderShipmentConsolsCount
		{
			get { return Order.IsShipmentAttached ? Order.Shipment.Consols.Count : 0; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime ActualARV
		{
			get { return Order.GetMilestoneActualDate(Events.Arrival).ToZDateTime(); }
		}
		public ZDateTime ActualCCC
		{
			get { return Order.GetMilestoneActualDate(Events.CustomsCommenced).ToZDateTime(); }
		}
		public ZDateTime ActualCLR
		{
			get { return Order.GetMilestoneActualDate(Events.CustomsCleared).ToZDateTime(); }
		}
		public ZDateTime ActualDEP
		{
			get { return Order.GetMilestoneActualDate(Events.Departure).ToZDateTime(); }
		}
		public ZDateTime ActualEXW
		{
			get { return Order.GetMilestoneActualDate(Events.ExWorks).ToZDateTime(); }
		}
		public ZDateTime ActualIST
		{
			get { return Order.GetMilestoneActualDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime(); }
		}
		public ZDateTime ActualPUP
		{
			get { return Order.GetMilestoneActualDate(Events.DeliveryCartageAdvised).ToZDateTime(); }
		}
		public ZDateTime ActualRCV
		{
			get { return Order.GetMilestoneActualDate(Events.GateIn).ToZDateTime(); }
		}
		public ZDateTime ActualUNP
		{
			get { return Order.GetMilestoneActualDate(Events.CargoAvailable).ToZDateTime(); }
		}
		public ZDateTime ActualUS1
		{
			get { return Order.JD_ActualUserDate1; }
		}
		public ZDateTime ActualUS2
		{
			get { return Order.JD_ActualUserDate2; }
		}

		public ZDateTime BookingConfDate
		{
			get { return Order.JD_BookingConfDate; }
		}

		public ZDateTime CustomDate1
		{
			get { return Order.JD_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return Order.JD_CustomDate2; }
		}

		public ZDateTime DepartureVesselCutoffDate
		{
			get { return Order.JD_DepartureVesselCutoffDate; }
		}

		public ZDateTime EstimatedARV
		{
			get { return Order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime(); }
		}

		public ZDateTime DepartureETA
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsShipmentAttached)
				{
					if (Order.Shipment.DepartureConsol != null)
					{
						result = Order.Shipment.DepartureConsol.Transports.ArrivalTransport.JW_ETA;
					}
				}
				else if (IsDeclarationAttached)
				{
					if (((BaseJobDeclaration)Order.Declaration).IsExport)
					{
						result = ((BaseJobDeclaration)Order.Declaration).JE_DateAtFinalDestination;
					}
				}
				else
				{
					result = Order.JD_E_ARV_1stIntermediate;
				}
				return result;
			}
		}

		public ZDateTime IntermediateETA
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsShipmentAttached && IntermediateConsol != null)
				{
					result = IntermediateConsol.Transports.ArrivalTransport.JW_ETA;
				}
				else if (!IsDeclarationAttached)
				{
					result = Order.JD_E_ARV_2ndIntermediate;
				}
				return result;
			}
		}

		public ZDateTime EstimatedCCC
		{
			get { return Order.GetMilestoneEstimatedDate(Events.CustomsCommenced).ToZDateTime(); }
		}

		public ZDateTime EstimatedCLR
		{
			get { return Order.GetMilestoneEstimatedDate(Events.CustomsCleared).ToZDateTime(); }
		}

		public ZDateTime EstimatedDEP
		{
			get { return Order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime(); }
		}

		public ZDateTime IntermediateETD
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsShipmentAttached && IntermediateConsol != null)
				{
					result = IntermediateConsol.JK_JX_JA_E_DEP;
				}
				else if (!IsDeclarationAttached)
				{
					result = Order.JD_E_DEP_2;
				}

				return result;
			}
		}

		public ZDateTime ArrivalETD
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsShipmentAttached)
				{
					if (Order.Shipment.ArrivalConsol != null)
					{
						result = Order.Shipment.ArrivalConsol.JK_JX_JA_E_DEP;
					}
				}
				else if (IsDeclarationAttached)
				{
					if (((BaseJobDeclaration)Order.Declaration).IsImport)
					{
						result = ((BaseJobDeclaration)Order.Declaration).JE_DateAtOrigin;
					}
				}
				else
				{
					result = Order.JD_E_DEP_3;
				}

				return result;
			}
		}

		public ZDateTime EstimatedEXW
		{
			get { return Order.GetMilestoneEstimatedDate(Events.ExWorks).ToZDateTime(); }
		}

		public ZDateTime EstimatedIST
		{
			get { return Order.GetMilestoneEstimatedDate(Events.DeliveryCartageCompleteFinalised).ToZDateTime(); }
		}

		public ZDateTime EstimatedPUP
		{
			get { return Order.GetMilestoneEstimatedDate(Events.DeliveryCartageAdvised).ToZDateTime(); }
		}

		public ZDateTime EstimatedRCV
		{
			get { return Order.GetMilestoneEstimatedDate(Events.GateIn).ToZDateTime(); }
		}

		public ZDateTime EstimatedUS1
		{
			get { return Order.JD_EstimateUserDate1; }
		}

		public ZDateTime EstimatedUS2
		{
			get { return Order.JD_EstimateUserDate2; }
		}

		public ZDateTime FollowUpDate
		{
			get { return Order.JD_FollowUpDate; }
		}

		public ZDateTime InvoiceDate
		{
			get { return Order.JD_InvoiceDate; }
		}

		public ZDateTime OrderDate
		{
			get { return Order.JD_OrderDate; }
		}

		public ZDateTime ReqExWorks
		{
			get { return Order.JD_ExWorksRequiredBy; }
		}

		public ZDateTime ReqInStore
		{
			get { return Order.JD_DeliveryRequiredBy; }
		}

		public ZDateTime HouseBillIssueDate
		{
			get { return IsShipmentAttached ? Shipment.HouseBillIssueDate : ZDateTime.Empty; }
		}

		public ZString HouseBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(HouseBill, HouseBillIssueDate); }
		}

		public ZDateTime MasterBillIssueDate
		{
			get { return IsShipmentAttached ? Shipment.MasterBillIssueDate : ZDateTime.Empty; }
		}

		public ZString MasterBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(MasterBillNum, MasterBillIssueDate); }
		}
		#endregion

		#region ZBool Fields

		public ZBool IsCancelled
		{
			get { return Order.JD_IsCancelled; }
		}

		public ZBool CustomFlag1
		{
			get { return Order.JD_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return Order.JD_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return Order.JD_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return Order.JD_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return Order.JD_CustomFlag5; }
		}

		public ZBool IsUnattachedOrder
		{
			get { return !IsShipmentAttached && !IsDeclarationAttached ? ZBool.True : ZBool.False; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal CustomDecimal1
		{
			get { return Order.JD_CustomDecimal1; }
		}
		public ZDecimal CustomDecimal2
		{
			get { return Order.JD_CustomDecimal2; }
		}
		public ZDecimal CustomDecimal3
		{
			get { return Order.JD_CustomDecimal3; }
		}
		public ZDecimal CustomDecimal4
		{
			get { return Order.JD_CustomDecimal4; }
		}
		public ZDecimal CustomDecimal5
		{
			get { return Order.JD_CustomDecimal5; }
		}

		public ZDecimal EstimatedExchangeRate
		{
			get { return Order.JD_EstimatedExchangeRate; }
		}

		public ZDecimal OrderWeight
		{
			get { return Order.JD_ActualWeight; }
		}

		public ZDecimal OrderVolume
		{
			get { return Order.JD_ActualVolume; }
		}

		#endregion

		#region Wrapper Fields

		public DocForwardingShipment Shipment
		{
			get { return Order.JD_JS.IsValid ? DocForwardingShipment.New(Order.Factory, Order.JD_JS) : null; }
		}

		public DocOrganisation ReceivingAgent
		{
			get
			{
				DocOrganisation result;
				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.ReceivingForwarder;
				}
				else if (Order.ReceivingAgent != null)
				{
					result = DocOrganisation.New(Order.Factory, Order.ReceivingAgent.PK);
				}
				else
				{
					result = null;
				}

				return result;
			}
		}

		public DocOrganisation SendingAgent
		{
			get
			{
				DocOrganisation result;
				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.SendingForwarder;
				}
				else if (Order.SendingAgent != null)
				{
					result = DocOrganisation.New(Order.Factory, Order.SendingAgent.PK);
				}
				else
				{
					result = null;
				}

				return result;
			}
		}

		public DocUNLOCO GoodsDeliveredTo
		{
			get
			{
				DocUNLOCO result;
				ZString uNLOCO;
				if (IsShipmentAttached)
				{
					uNLOCO = Order.Shipment.JS_RL_NKDestination;
				}
				else if (IsDeclarationAttached)
				{
					uNLOCO = ((BaseJobDeclaration)Order.Declaration).JE_RL_NKFinalDestination;
				}
				else if (!Order.JD_RL_NKGoodsDeliveredTo.IsEmpty)
				{
					uNLOCO = Order.JD_RL_NKGoodsDeliveredTo;
				}
				else
				{
					uNLOCO = ZString.Empty;
				}

				result = DocUNLOCO.New(Order.Factory, uNLOCO);
				return result;
			}
		}

		public DocUNLOCO GoodsReceivedAt
		{
			get
			{
				DocUNLOCO result;
				ZString uNLOCO;
				if (IsShipmentAttached)
				{
					uNLOCO = Order.Shipment.JS_RL_NKOrigin;
				}
				else if (IsDeclarationAttached)
				{
					uNLOCO = ((BaseJobDeclaration)Order.Declaration).JE_RL_NKOrigin;
				}
				else if (!Order.JD_RL_NKGoodsAvailableAt.IsEmpty)
				{
					uNLOCO = Order.JD_RL_NKGoodsAvailableAt;
				}
				else
				{
					uNLOCO = ZString.Empty;
				}

				result = DocUNLOCO.New(Order.Factory, uNLOCO);
				return result;
			}
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(Order.OrderCurrency, Factory); }
		}

		public DocOrganisation Consignor
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.Consignor;
				}
				else if (IsDeclarationAttached)
				{
					return DocOrganisation.New(((BaseJobDeclaration)Order.Declaration).SupplierDocumentaryAddress, Order.Factory);
				}

				return DocOrganisation.New(Order.Supplier, Factory);
			}
		}

		public ZString ConsignorAddress
		{
			get { return Consignor != null ? Consignor.PostalAddress : ZString.Empty; }
		}

		public DocOrganisation Consignee
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.Consignee;
				}
				else if (IsDeclarationAttached)
				{
					return DocOrganisation.New(((BaseJobDeclaration)Order.Declaration).ImporterDocumentaryAddress, Order.Factory);
				}

				return DocOrganisation.New(Order.Buyer, Factory);
			}
		}

		public ZString ConsigneeAddress
		{
			get { return Consignee != null ? Consignee.PostalAddress : ZString.Empty; }
		}

		public ZInt NotifyPartyCount
		{
			get { return 1; }
		}

		public DocContacts NotifyParty
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.NotifyParty;
				}

				return null;
			}
		}

		public DocContacts NotifyParty2
		{
			get { return null; }
		}

		public DocContacts NotifyParty3
		{
			get { return null; }
		}

		public DocOrganisation ImportBroker
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ImportBroker;
				}

				DocOrganisation result = null;
				OrgSupplierBuyerLink link = Order.SupplierBuyerLink;
				if (link != null)
				{
					OrgSupBuyLinkTrnMode linkMode = link.OrgSupBuyLinkTrnModes.Find(TransportMode, ContainerMode);
					if (linkMode != null && linkMode.ImportCustomsAgent != null)
					{
						result = DocOrganisation.New(linkMode.ImportCustomsAgent, Factory);
					}
				}

				if (result == null && Order.Buyer != null)
				{
					OrgHeader cusBroker = Order.Buyer.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, TransportMode, ContainerMode, Order.JD_RL_NKPortOfDischarge);
					if (cusBroker != null)
					{
						result = DocOrganisation.New(cusBroker, Factory);
					}
				}
				return result;
			}
		}

		public DocJobRequiredDocumentCollection RequiredDocuments
		{
			get { return new DocJobRequiredDocumentCollection(Order.RequiredDocuments, Factory); }
		}

		#endregion

		#region Routing Order Fields

		public Image DocumentLogo
		{
			get
			{
				Image result = null;
				if (Consignee != null && Consignee.MiscServ != null && Consignee.MiscServ.ClientDocumentLogo.Length > 0)
				{
					try
					{
						MemoryStream stream = new MemoryStream(Consignee.MiscServ.ClientDocumentLogo);
						result = Image.FromStream(stream);
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException()) { throw; }
						result = null;
					}
				}

				if (result == null)
				{
					result = SystemDataRegistry.Instance.CompanyLogo.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				}
				return result;
			}
		}

		public DocOrganisationCollection RecommendedAgents
		{
			get
			{
				DocOrganisationCollection result = new DocOrganisationCollection(Factory);

				if (SendingAgent != null)
				{
					result.Add(SendingAgent);
				}

				if (result.Count == 0 && PortOfLoading != null)
				{
					OrgAddress agent = PortOfLoading.RefUNLOCO.GetPublishedAgent(Order.JD_TransportMode, AgentDirectionList.Codes.Export);
					if (agent != null)
					{
						result.Add(DocOrganisation.New(agent, Factory));
					}
				}

				if (result.Count == 0 && Consignor != null && Consignor.Loco != null)
				{
					OrgAddress agent = Consignor.Loco.RefUNLOCO.GetPublishedAgent(Order.JD_TransportMode, AgentDirectionList.Codes.Export);
					if (agent != null)
					{
						result.Add(DocOrganisation.New(agent, Factory));
					}
				}

				return result;
			}
		}

		public DocContacts ConsigneeContact
		{
			get
			{
				DocContacts result = null;

				if (IsShipmentAttached)
				{
					result = DocContacts.New(Order.Shipment.ConsigneeDocumentaryAddress, Factory);
				}
				else
				{
					DocOrganisation docOrg = DocOrganisation.New(Order.Buyer, Factory);
					result = (docOrg != null) ? docOrg.DefaultContact(ContactType.Consignee) : null;
				}

				return result;
			}
		}

		public DocContacts ConsignorContact
		{
			get
			{
				DocContacts result = null;

				if (IsShipmentAttached)
				{
					result = DocContacts.New(Order.Shipment.ConsignorDocumentaryAddress, Factory);
				}
				else
				{
					DocOrganisation docOrg = DocOrganisation.New(Order.Supplier, Factory);
					result = (docOrg != null) ? docOrg.DefaultContact(ContactType.Consignor) : null;
				}

				return result;
			}
		}

		public ZString RoutingOrderOpeningText
		{
			get { return Env.Registry.RoutingOrderOpeningText; }
		}

		public ZString RoutingOrderClosingText
		{
			get { return Env.Registry.RoutingOrderClosingText; }
		}

		#endregion

		#region ZByte Fields

		public ZByte OrderNumberSplit
		{
			get { return Order.JD_OrderNumberSplit; }
		}

		#endregion

		#region Collections

		public IDocContainerCollection Containers
		{
			get
			{
				IDocContainerCollection result = new IDocContainerCollection(Factory);
				if (IsShipmentAttached)
				{
					OverrideDocumentDirectionAfterItsSetByTheReport_HACK_DoNotUse_ToBeRemoved(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV);
					result = Shipment.Containers;
				}
				else
				{
					foreach (OrderContainer currentContainer in Order.PlannedContainers)
					{
						result.Add(DocOrderContainer.New(currentContainer, Factory));
					}
				}

				return result;
			}
		}

		public IDocSimpleContainerCollection SimpleContainers
		{
			get
			{
				IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
				if (IsShipmentAttached)
				{
					result = Shipment.SimpleContainers;
				}
				else if (IsDeclarationAttached)
				{
					if (DeclarationForCurrentBranch != null)
					{
						result = DeclarationForCurrentBranch.SimpleContainers;
					}
				}
				else
				{
					foreach (OrderContainer currentContainer in Order.PlannedContainers)
					{
						result.Add(DocOrderContainer.New(currentContainer, Factory));
					}
				}

				return result;
			}
		}

		public DocOrderLineCollection OrderLines
		{
			get
			{
				DocOrderLineCollection result = new DocOrderLineCollection(Factory);
				foreach (OrderLine line in Order.OrderLines)
				{
					result.Add(DocOrderLine.New(line, Factory));
				}
				return result;
			}
		}

		public DocOrderLineDeliverContainerCollection AllDeliverContainers
		{
			get
			{
				DocOrderLineDeliverContainerCollection result = new DocOrderLineDeliverContainerCollection(Factory);

				if (this.Order.OrderLines.Count == 0)
				{
					this.Order.OrderLines.AddNew();
				}
				foreach (DocOrderLine line in OrderLines)
				{
					if (((OrderLine)line.WrappedObject).Deliveries.Count == 0)
					{
						((OrderLine)line.WrappedObject).Deliveries.AddNew();
					}
					DocOrderLineDeliveryCollection sortedDeliveries = line.Deliveries;
					sortedDeliveries.Sort(new SortInfo("DeliverPointCode", ListSortDirection.Ascending));

					foreach (DocOrderLineDelivery delivery in sortedDeliveries)
					{
						if (((OrderLineDelivery)delivery.WrappedObject).Containers.Count == 0)
						{
							((OrderLineDelivery)delivery.WrappedObject).Containers.AddNew();
						}
						foreach (DocOrderLineDeliverContainer container in delivery.Containers)
						{
							result.Add(container);
						}
					}
				}
				return result;
			}
		}

		public DocOrderLineCollection OrderLinesWithUndefinedProduct
		{
			get
			{
				var result = new DocOrderLineCollection(Factory);

				result.AddRange(
					OrderLines
						.OfType<DocOrderLine>()
						.Where(line => !line.Partno.IsEmpty && !SupplierPartExists(line))
				);

				return result;
			}
		}

		bool SupplierPartExists(DocOrderLine line) => Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, line.Partno)) != null;

		#endregion

		#region Order Fields put back in to fix wow document

		[Obsolete]
		public DocOrganisation Supplier
		{
			get { return Consignor; }
		}

		[Obsolete]
		public DocOrganisation Buyer
		{
			get { return Consignee; }
		}

		#endregion

		#region DocManager barcode properties

		protected override ZString DocManagerUniqueID
		{
			get { return Order.JD_OrderNumber; }
		}

		#endregion

		#region Implementation

		protected Order Order
		{
			get { return (Order)WrappedObject; }
		}

		public ZBool IsShipmentAttached
		{
			get { return Order.IsShipmentAttached; }
		}

		public ZBool IsDeclarationAttached
		{
			get { return Order.IsDeclarationAttached; }
		}

		#endregion

		#region IPreAlert Members

		public DocTransportCollection CompleteRouting
		{
			get
			{
				DocTransportCollection result;

				if (Shipment != null)
				{
					result = Shipment.CompleteRouting;
				}
				else if (DeclarationForCurrentBranch != null)
				{
					result = DeclarationForCurrentBranch.CompleteRouting;
				}
				else
				{
					result = new DocTransportCollection(Order.Factory);
					switch (Order.PlanningVoyageState)
					{
						case PlanningVoyageState.OneVoyage:
							result.Add(DocTransport.New(Order, OrderSource.Leg.SingleLeg, Order.Factory));
							break;

						case PlanningVoyageState.TwoVoyage:
							result.Add(DocTransport.New(Order, OrderSource.Leg.Departure, Order.Factory));
							result.Add(DocTransport.New(Order, OrderSource.Leg.Arrival, Order.Factory));
							break;

						case PlanningVoyageState.ThreeVoyage:
							result.Add(DocTransport.New(Order, OrderSource.Leg.Departure, Order.Factory));
							result.Add(DocTransport.New(Order, OrderSource.Leg.Intermediate, Order.Factory));
							result.Add(DocTransport.New(Order, OrderSource.Leg.Arrival, Order.Factory));
							break;
					}
				}

				return result;
			}
		}

		public ZString PortDisplayMode
		{
			get { return "LoadDischargeCollectDeliver"; }
		}

		public ZBool ShowChargesOnArrivalNotice
		{
			get { return false; }
		}

		public ZBool ShowExchangeRatesOnArrivalNotice
		{
			get { return false; }
		}

		public DocBaseJobDeclaration DeclarationForCurrentBranch
		{
			get
			{
				DocBaseJobDeclaration result = null;
				BaseJobDeclaration declaration = Factory.Load<BaseJobDeclaration>(Order.JD_JE);

				if (declaration != null && declaration.Branch != null)
				{
					if (declaration.Branch.GB_Code == CurrentBranch.Code)
					{
						result = DocBaseJobDeclaration.New(declaration, Factory);
					}
				}

				return result;
			}
		}

		public ZString PreAlertReferenceHeading
		{
			get { return Res.GetString("28c14e7b-e967-4663-be30-6f158df221fd", "ORDER NUMBERS / REFERENCE"); }
		}

		public ZString PreAlertReference
		{
			get
			{
				DocBaseJobDeclaration declaration = DeclarationForCurrentBranch;

				if (declaration == null)
				{
					return OrderNumbers;
				}
				else
				{
					if (!OrderNumbers.Contains(declaration.OwnerRef))
					{
						return OrderNumbers + " " + declaration.OwnerRef;
					}
					else
					{
						return OrderNumbers;
					}
				}
			}
		}

		public ZString PreAlertDocumentHeader
		{
			get { return UltimateNotification + HeadingTransportMode + " " + ReportName; }
		}

		public ZString JobNumber
		{
			get { return IsShipmentAttached ? Shipment.ShipmentNumber : Order.JD_OrderNumber; }
		}

		public ZString JobNumberAndSplit
		{
			get { return IsShipmentAttached ? Shipment.ShipmentNumber : Order.JD_OrderNumberSplit == 0 ? Order.JD_OrderNumber : Order.JD_OrderNumberAndSplit; }
		}

		public ZString SecondJobNumberHeading
		{
			get { return Res.GetString("1bfc01c9-3861-4f10-92ab-cbc9fbf588c5", "SPLIT:"); }
		}

		public ZString SecondJobNumber
		{
			get { return OrderAndSplitNumber; }
		}

		public ZString AvailableDateHeading
		{
			get { return AvailableDate.IsEmpty ? "" : Res.GetString("0a4085b0-0157-4564-a0ae-7a29609945ef", "AVAILABLE DATE:"); }
		}

		public ZDateTime AvailableDate
		{
			get { return IsShipmentAttached ? Shipment.AvailableDate : Order.GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime(); }
		}

		public ZString StorageStartsHeading
		{
			get { return Res.GetString("ca56a948-2480-4ac6-9f80-12dfef26efdf", "STORAGE STARTS:"); }
		}

		public ZString StorageStartsDate
		{
			get { return IsShipmentAttached ? Shipment.StorageStartsDate : new ZString(Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised")); }
		}

		public ZString BrokerName
		{
			get
			{
				if (IsShipmentAttached)
				{
					return Shipment.ImportBrokerName;
				}
				else
				{
					return ImportBroker != null ? ImportBroker.Name : new ZString(Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised"));
				}
			}
		}

		public ZString TransportInfo
		{
			get
			{
				ZString result = "";

				if (TransportMode == Core.Constants.TransportModes.Sea)
				{
					if (!ArrivalVessel.IsEmpty)
					{
						result = ArrivalVessel;
					}
					else if (!IntermediateVessel.IsEmpty)
					{
						result = IntermediateVessel;
					}
					else if (!DepartureVessel.IsEmpty)
					{
						result = DepartureVessel;
					}
				}

				if (!result.IsEmpty)
				{
					result += " / ";
				}

				if (!ArrivalVoyage.IsEmpty)
				{
					result += ArrivalVoyage;
				}
				else if (!IntermediateVoyage.IsEmpty)
				{
					result += IntermediateVoyage;
				}
				else if (!DepartureVoyage.IsEmpty)
				{
					result += DepartureVoyage;
				}

				return result;
			}
		}

		public ZString CollectedFromETDString
		{
			get { return IsShipmentAttached ? Shipment.ETDString.ToString() : Order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime().ToShortDateString(); }
		}

		public ZString DeliveredToETAString
		{
			get { return IsShipmentAttached ? Shipment.ETAString.ToString() : Order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime().ToShortDateString(); }
		}

		public ZString LoadingETDString
		{
			get { return IsShipmentAttached ? Shipment.ETDString.ToString() : Order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime().ToShortDateString(); }
		}

		public ZString DischargeETAString
		{
			get { return IsShipmentAttached ? Shipment.ETAString.ToString() : Order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime().ToShortDateString(); }
		}

		public ZString ArrivalReference
		{
			get { return ZString.Empty; }
		}

		public ZString KANumber
		{
			get
			{
				return "";
			}
		}

		public ZString CTOArrivalBerth
		{
			get
			{
				return "";
			}
		}

		public ZString Context
		{
			get { return "ORDER"; }
		}

		public ZString MarksAndNumbers
		{
			get { return new ZString(); }
		}

		public DocCommodityCollection Commodity
		{
			get { return new DocCommodityCollection(Factory); }
		}

		public ZString TransportHeading
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Sea)
				{
					return Res.GetString("0f32415f-2fd1-420b-853d-7b75d013ab40", "VESSEL / VOYAGE");
				}
				else if (TransportMode == Core.Constants.TransportModes.Air)
				{
					return Res.GetString("3815bd2d-87f7-4255-9567-dce7c234158d", "FLIGHT");
				}
				else if (TransportMode == Core.Constants.TransportModes.Rail)
				{
					return Res.GetString("196376e3-a39a-43d3-baf6-cb8533649516", "JOURNEY NAME / JOURNEY NUMBER");
				}

				return ZString.Empty;
			}
		}

		public ZString MasterBillHeading
		{
			get { return Res.GetString("53b44fd3-e611-43fe-98bd-4bc864db1be5", "MASTER BILL NUMBER"); }
		}

		public ZString MasterBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(MasterBillHeading, MasterBillIssueDate); }
		}

		public ZString MasterBillNum
		{
			get
			{
				ZString result;
				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.MasterBillNum;
				}
				else if (IsDeclarationAttached)
				{
					result = ((BaseJobDeclaration)Order.Declaration).JE_MasterBill;
				}
				else if (!Order.JD_MasterWaybill.IsEmpty)
				{
					result = Order.JD_MasterWaybill;
				}
				else
				{
					result = Res.GetString("fde3fe66-c131-4641-a4e1-db2e490e4926", "To Be Advised");
				}
				return result;
			}
		}

		public DocDocAddress GoodsAvailableAt
		{
			get
			{
				if (IsShipmentAttached && !Order.Shipment.ConsignorPickupAddress.IsEmpty)
				{
					return DocDocAddress.New(Order.Shipment.ConsignorPickupAddress, Factory);
				}
				else if (IsDeclarationAttached)
				{
					var supplierPickupAddress = ((BaseJobDeclaration)Order.Declaration).SupplierPickupAddress;
					if (!supplierPickupAddress.IsEmpty)
					{
						return DocDocAddress.New(supplierPickupAddress, Factory);
					}
				}

				return DocDocAddress.New(Order.GoodsAvailableAtAddress, Factory);
			}
		}

		public DocDocAddress UnpackAt
		{
			get
			{
				return null;
			}
		}

		public DocOrganisation ShippingLine
		{
			get
			{
				DocOrganisation result = null;
				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.ShippingLine;
				}
				else
				{
					if (Order.Carrier != null)
					{
						result = DocOrganisation.New(Order.Carrier, Factory);
					}
				}

				return result;
			}
		}

		public ZString PortOfLoadingAsString
		{
			get { return GetPrintablePortFromDocUNLOCO(PortOfLoading); }
		}

		public DocUNLOCO PortOfLoading
		{
			get
			{
				DocUNLOCO result;

				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.PortOfLoading;
				}
				else if (IsDeclarationAttached)
				{
					result = DocUNLOCO.New(Order.Factory, ((BaseJobDeclaration)Order.Declaration).JE_RL_NKPortOfLoading);
				}
				else if (Order.PortOfLoading != null)
				{
					result = DocUNLOCO.New(Order.PortOfLoading, Factory);
				}
				else if (Order.GoodsAvailableAt != null)
				{
					result = DocUNLOCO.New(Order.GoodsAvailableAt, Factory);
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public ZString PortOfDischargeAsString
		{
			get { return (PortOfDischarge != null) ? GetPrintablePortFromDocUNLOCO(PortOfDischarge) : ZString.Empty; }
		}

		public DocUNLOCO PortOfDischarge
		{
			get
			{
				DocUNLOCO result;

				if (IsShipmentAttached && Order.Shipment.Consols.Count > 0)
				{
					result = Shipment.Consol.PortOfDischarge;
				}
				else if (IsDeclarationAttached)
				{
					result = DocUNLOCO.New(Order.Factory, ((BaseJobDeclaration)Order.Declaration).JE_RL_NKPortOfArrival);
				}
				else if (Order.PortOfDischarge != null)
				{
					result = DocUNLOCO.New(Order.PortOfDischarge, Factory);
				}
				else if (Order.GoodsDeliveredTo != null)
				{
					result = DocUNLOCO.New(Order.GoodsDeliveredTo, Factory);
				}
				else
				{
					result = null;
				}

				return result;
			}
		}

		public TrackingConstants.BusinessContext TrackingBusinessContext
		{
			get { return TrackingConstants.BusinessContext.Order; }
		}

		public ZGuid TrackingBusinessObjectPK
		{
			get { return Order.PK; }
		}

		#endregion

		#region IRequestForMissingDocuments Members

		public ZString DeclarationOrConsolNumber
		{
			get { return ZString.Empty; }
		}

		public ZString EmailSubjectNumber
		{
			get { return OrderNumber; }
		}

		public ZString MissingRequiredDocuments
		{
			get { return RequiredDocuments.MissingRequiredDocuments; }
		}

		public ZString ShipmentOrBrokerageNumber
		{
			get { return OrderNumber; }
		}

		public ZString RequestForMissingDocumentsInstruction
		{
			get { return Env.Registry.ShipmentRequestForMissingDocumentsClause; }
		}

		public ZString ContainerNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (IDocContainer currentContainer in Containers)
				{
					result += currentContainer.ContainerNumber + ", ";
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString ETAString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, EstimatedARV); }
		}

		public ZString ETDString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, EstimatedDEP); }
		}

		public DocUNLOCO FinalDestination
		{
			get
			{
				DocUNLOCO result = null;

				if (DeclarationForCurrentBranch != null)
				{
					result = DeclarationForCurrentBranch.FinalDestination;
				}

				return result;
			}
		}

		public ZDateTime DateAtFinalDestination
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (DeclarationForCurrentBranch != null)
				{
					result = DeclarationForCurrentBranch.DateAtFinalDestination;
				}

				return result;
			}
		}

		public ZString OwnerRefAndOrderRef
		{
			get
			{
				ZString result = ZString.Empty;

				if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.IsImportMessage
					&& !DeclarationForCurrentBranch.OwnerRef.IsEmpty && !OrderNumbers.Contains(DeclarationForCurrentBranch.OwnerRef))
				{
					result += DeclarationForCurrentBranch.OwnerRef + " ";
				}

				result += OrderNumbers.Replace("\n", " ");
				return result;
			}
		}

		public ZString OwnerRefAndOrderRefHeading
		{
			get { return Res.GetString("28c14e7b-e967-4663-be30-6f158df221fd", "ORDER NUMBERS / REFERENCE"); }
		}

		public ZString ConsigneeOrgHeading
		{
			get { return Res.GetString("55c307c6-37cf-4819-8ee8-727a2b12581f", "CONSIGNEE"); }
		}

		public ZString ConsignorOrgHeading
		{
			get { return Res.GetString("0c4dd524-8b15-4dd2-ad48-e93d341032f7", "CONSIGNOR"); }
		}

		public DocOrganisation ConsigneeOrg
		{
			get { return Consignee; }
		}

		public DocOrganisation ConsignorOrg
		{
			get { return Consignor; }
		}

		#endregion

		#region Notes

		public ZString SpecialInstructionNote
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.SpecialInstructions.Description, Order); }
		}

		public ZString AgentNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.AgentNotes.Description, Order); }
		}

		#endregion

		public ZString MostRecentOrderManagementUpdateNote
		{
			get
			{
				ZString result = ZString.Empty;

				if (Order != null && Order.Notes != null)
				{
					StmNote[] notes = Order.Notes.FindByDescription(PredefinedNoteTypes.Instance.OrderManagementUpdate.Description);
					if (notes.Length > 0)
					{
						Array.Sort(notes, new StmNoteComparer());
						result = notes[0].ST_NoteDataAsText;
					}
				}
				return result;
			}
		}

		class StmNoteComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				ZDateTime xDate = ((StmNote)x).ST_CreatedDateUtc;
				ZDateTime yDate = ((StmNote)y).ST_CreatedDateUtc;
				return yDate.CompareTo(xDate);
			}

			#endregion
		}
	}
}
