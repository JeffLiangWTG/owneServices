using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocPackUnpackContainerRego : DocContainer, IDocCartageAdvice, Integration.DocumentWrappers.IDocDocPackUnpackContainerRego
	{
		protected DocPackUnpackContainerRego(CFSContainer packUnpackContainerRegistration, BusinessObjectFactory factoryToWrap)
			: base(packUnpackContainerRegistration, null, factoryToWrap)
		{
		}

		public new static DocPackUnpackContainerRego New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<CFSContainer>(pK), factory);
		}

		public static DocPackUnpackContainerRego New(CFSContainer packUnpackContainerRegistration, BusinessObjectFactory factoryToWrap)
		{
			if (packUnpackContainerRegistration == null)
			{
				return null;
			}
			else
			{
				return new DocPackUnpackContainerRego(packUnpackContainerRegistration, factoryToWrap);
			}
		}

		#region Overrides

		#region Wrapper Fields

		#region Cartage Advice Fields

		public override DocPickupDeliveryConfirm OriginJourneyOneDeliveryConfirm
		{
			get { return DocPickupDeliveryConfirm.New(CommonContainer.OriginCFSArrival, Factory); }
		}

		public override DocPickupDeliveryConfirm OriginJourneyTwoPickupConfirm
		{
			get { return DocPickupDeliveryConfirm.New(CommonContainer.OriginCFSDeparture, Factory); }
		}

		public override DocPickupDeliveryConfirm DestinationJourneyOneDeliveryConfirm
		{
			get { return DocPickupDeliveryConfirm.New(CommonContainer.DestinationCFSArrival, Factory); }
		}

		public override DocPickupDeliveryConfirm DestinationJourneyTwoPickupConfirm
		{
			get { return DocPickupDeliveryConfirm.New(CommonContainer.DestinationCFSDeparture, Factory); }
		}

		public override DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				DocDocAddress result = base.JourneyOnePickUpAddress;

				if (result == null)
				{
					if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
					{
						result = DepartureContainerParkAddress;
					}
					else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
					{
						if (LoadListConsol != null)
						{
							result = LoadListConsol.ArrivalCTOAddress;
						}
					}
				}

				return result;
			}
		}

		public override DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get
			{
				DocDocAddress result = base.JourneyOneDeliverToAddressForExport;

				if (result == null)
				{
					if (CurrentBranch != null && CurrentBranch.Organisation != null)
					{
						result = CurrentBranch.Organisation.DeliverDocAddress;
					}
				}

				return result;
			}
		}

		public override DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get
			{
				DocDocAddress result = base.JourneyOneDeliverToAddressForImport;

				if (result == null)
				{
					if (CurrentBranch != null && CurrentBranch.Organisation != null)
					{
						result = CurrentBranch.Organisation.DeliverDocAddress;
					}
				}

				return result;
			}
		}

		public override DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get
			{
				DocDocAddress result = base.JourneyTwoPickUpAddressForExport;

				if (result == null)
				{
					if (CurrentBranch != null && CurrentBranch.Organisation != null)
					{
						result = CurrentBranch.Organisation.PickUpDocAddress;
					}
				}

				return result;
			}
		}

		public override DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get
			{
				DocDocAddress result = base.JourneyTwoPickUpAddressForImport;

				if (result == null)
				{
					if (CurrentBranch != null && CurrentBranch.Organisation != null)
					{
						result = CurrentBranch.Organisation.PickUpDocAddress;
					}
				}

				return result;
			}
		}

		public override DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				DocDocAddress result = base.JourneyTwoDeliverToAddress;

				if (result == null)
				{
					if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
					{
						if (LoadListConsol != null)
						{
							result = LoadListConsol.DepartureCTOAddress;
						}
					}
					else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
					{
						result = ArrivalContainerParkAddress;
					}
				}
				return result;
			}
		}

		#endregion

		public override DocUNLOCO PortOfLoading
		{
			get { return DocUNLOCO.New(Factory, PackUnpackContainerRegistration.JC_JA_NKPortOfLoading); }
		}

		public override DocUNLOCO PortOfDischarge
		{
			get { return DocUNLOCO.New(Factory, PackUnpackContainerRegistration.JC_JB_NKPortOfDischarge); }
		}

		#endregion

		#region ZString Fields

		public ZString CartageInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (LoadListConsol != null)
				{
					result = LoadListConsol.CartageInstructions;
				}

				if (result.IsEmpty)
				{
					result = PickupOrDeliveryCartageInstructions();
				}

				return result;
			}
		}

		public override ZString FullHandlingInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (LoadListConsol != null)
				{
					result = LoadListConsol.HandlingInstructions;
				}

				if (result.IsEmpty)
				{
					result = GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, CommonContainer);
				}

				return result;
			}
		}

		public override ZString FullCartageInstructions
		{
			get { return CartageInstructions; }
		}

		public ZString Voyage
		{
			get { return PackUnpackContainerRegistration.JC_JV_VoyageFlight; }
		}

		public override ZString DepartureTruckRegistration
		{
			get { return PackUnpackContainerRegistration.JC_DepartureTruckRegistration; }
		}

		#endregion

		#region IDocIMO Members Override

		public override ZString IMOShippersRef
		{
			get { return base.IMOForwardersRef; }
		}
		public override ZString IMOForwardersRef
		{
			get { return ZString.Empty; }
		}

		#endregion

		#endregion

		#region Wrapper Fields

		public DocOrganisation SailingLine
		{
			get { return Sailing != null ? Sailing.ShippingLine : null; }
		}

		//Temporary solution!! Will be changed later when consol is refactored
		public DocLoadListConsol LoadListConsol
		{
			get { return DocLoadListConsol.New(PackUnpackContainerRegistration.Consol, Factory); }
		}

		DocTransport MostInterestingTransport
		{
			get
			{
				if (PackUnpackContainerRegistration.Consol != null)
				{
					return DocTransport.New(PackUnpackContainerRegistration.Consol, PackUnpackContainerRegistration.Consol.Transports.MostInterestingTransport, Factory);
				}
				else if (PackUnpackContainerRegistration.Sailing != null)
				{
					return DocTransport.New(PackUnpackContainerRegistration.Sailing, Factory);
				}

				return null;
			}
		}

		public override DocOrganisation ArrivalTransport
		{
			get { return DocOrganisation.New(Factory, PackUnpackContainerRegistration.JC_ArrivalTransportPK); }
		}

		public override DocOrganisation DepartureTransport
		{
			get { return DocOrganisation.New(Factory, PackUnpackContainerRegistration.JC_DepartureTransportPK); }
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalShipmentPacks
		{
			get { return PackUnpackContainerRegistration.TotalShipmentPacks; }
		}

		public ZString TotalShipmentPacksUnit
		{
			get { return PackUnpackContainerRegistration.JC_Calc_TotalPackagesUnit; }
		}

		public ZInt TotalPillagedShipmentPacks
		{
			get
			{
				ZInt total = 0;
				foreach (DocShipment shipment in ContainerShipments)
				{
					total += shipment.TotalOuterPacksPillaged;
				}
				return total;
			}
		}

		public ZInt TotalSurplusShipmentPacks
		{
			get
			{
				ZInt total = 0;
				foreach (DocShipment shipment in ContainerShipments)
				{
					if (shipment.TotalOuterPacksOutturned > shipment.OuterPacks)
					{
						total += (shipment.TotalOuterPacksOutturned - shipment.OuterPacks);
					}
				}
				return total;
			}
		}

		public ZInt TotalShortShipmentPacks
		{
			get
			{
				ZInt total = 0;
				foreach (DocShipment shipment in ContainerShipments)
				{
					if (shipment.OuterPacks > shipment.TotalOuterPacksOutturned)
					{
						total += (shipment.OuterPacks - shipment.TotalOuterPacksOutturned);
					}
				}
				return total;
			}
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal TotalShipmentWeight
		{
			get { return PackUnpackContainerRegistration.TotalShipmentWeight; }
		}

		public ZString TotalShipmentWeightUnit
		{
			get { return PackUnpackContainerRegistration.JC_Calc_TotalWeightUnit; }
		}

		public ZDecimal TotalShipmentVolume
		{
			get { return PackUnpackContainerRegistration.TotalShipmentVolume; }
		}

		public ZString TotalShipmentVolumeUnit
		{
			get { return PackUnpackContainerRegistration.JC_Calc_TotalVolumeUnit; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime PackUnpackDate
		{
			get { return PackUnpackContainerRegistration.JC_PackUnpackDate; }
		}

		public ZDateTime LCLAvailablReadonly
		{
			get { return PackUnpackContainerRegistration.JC_LCLAvailable_Readonly; }
		}

		public ZDateTime LCLStorageCommenceReadonly
		{
			get { return PackUnpackContainerRegistration.JC_LCLStorageCommences_Readonly; }
		}

		public ZDateTime FumigationCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		public ZDateTime QuarantineCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		public ZDateTime CustomsHoldCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		public ZDateTime WashingCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Washing); }
		}

		public ZDateTime SteamCleanCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.SteamCleaning); }
		}

		public ZDateTime ExtraInspectionCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.ExtraInspection); }
		}

		public ZDateTime CleaningCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Cleaning); }
		}

		public ZDateTime TailgateCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Tailgate); }
		}

		public ZDateTime QuarantineUnpackCompletionEventTime
		{
			get { return PackUnpackContainerRegistration.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineUnpack); }
		}

		public ZDateTime SailingETD
		{
			get { return PackUnpackContainerRegistration.JC_JA_E_DEP; }
		}

		public ZDateTime SailingETA
		{
			get { return PackUnpackContainerRegistration.JC_JB_E_ARV; }
		}

		public override ZDateTime BookingCutOffDate
		{
			get
			{
				ZDateTime cutOff = ZDateTime.Empty;
				if (LoadListConsol != null && LoadListConsol.Sailing != null)
				{
					cutOff = (LoadListConsol.ConsolMode == Core.Constants.ContainerModes.FCL) ? LoadListConsol.Sailing.FCLCutOff : LoadListConsol.Sailing.LCLCutOff;
				}
				else if (Sailing != null)
				{
					cutOff = (PackUnpackContainerRegistration.JC_ContainerMode == Core.Constants.ContainerModes.FCL) ? Sailing.FCLCutOff : Sailing.LCLCutOff;
				}

				return cutOff;
			}
		}

		public override ZDateTime PickUpDate
		{
			get
			{
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region ZBool Fields

		public ZBool IsFumigationCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		public ZBool IsQuarantineCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		public ZBool IsCustomsHoldCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		public ZBool IsWashingCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Washing); }
		}

		public ZBool IsSteamCleanCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.SteamCleaning); }
		}

		public ZBool IsExtraInspectionCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.ExtraInspection); }
		}

		public ZBool IsCleaningCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Cleaning); }
		}

		public ZBool IsTailgateCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Tailgate); }
		}

		public ZBool IsQuarantineUnpackCompleted
		{
			get { return PackUnpackContainerRegistration.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.QuarantineUnpack); }
		}

		#endregion

		#region ZString Fields

		public ZString UNDG_Codes
		{
			get
			{
				List<ZString> codes = new List<ZString>();

				foreach (PackLine line in PackUnpackContainerRegistration.PackLines)
				{
					foreach (UNDGDataItem dGItem in line.UNDGs)
					{
						if (dGItem.Substance != null)
						{
							ZString code = dGItem.Substance.DG_UNNO + dGItem.Substance.DG_Variant;
							if (!codes.Contains(code))
							{
								codes.Add(code);
							}
						}
					}
				}

				ZString[] codeList = codes.ToArray();
				Array.Sort(codeList);

				return ZString.Join(", ", codeList);
			}
		}

		public ZString HandlingInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				ZString containersHandlingInstruction = GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, CommonContainer);
				if (ReportName.StartsWith((NoResString)"CFS Cartage Advice"))
				{
					if (LoadListConsol != null)
					{
						result = LoadListConsol.HandlingInstructions;
					}

					if (result.IsEmpty)
					{
						result = containersHandlingInstruction;
					}
				}
				else
				{
					result = containersHandlingInstruction;
				}

				return result;
			}
		}

		public ZString TransportMode
		{
			get { return PackUnpackContainerRegistration.JC_TransportMode; }
		}

		public ZString WarningMessage
		{
			get { return PackUnpackContainerRegistration.WarningMessage; }
		}

		public DocAddress DocContainerParkAddress
		{
			get
			{
				var containerParkAddress = Factory.Load<OrgAddress>(PackUnpackContainerRegistration.ContainerYardAddress);
				return DocAddress.New(containerParkAddress, Factory);
			}
		}

		public ZString ContainerParkName
		{
			get { return (DocContainerParkAddress != null) ? DocContainerParkAddress.Organisation.Name : ZString.Empty; }
		}

		public DocAddress DocUnpackDepotAddress
		{
			get
			{
				if (PackUnpackContainerRegistration.Consol != null)
				{
					return DocAddress.New(PackUnpackContainerRegistration.Consol.UnpackDepotAddress, Factory);
				}

				return null;
			}
		}

		public ZString UnpackDepotName
		{
			get { return (DocUnpackDepotAddress != null) ? DocUnpackDepotAddress.Organisation.Name : ZString.Empty; }
		}

		public new ZString GrossWeight
		{
			get
			{
				ZDecimal result = new ZDecimal(0.0);

				if (!PackUnpackContainerRegistration.JC_GrossWeight.IsDefault)
				{
					result = new ZDecimal(base.GrossWeight);
				}
				else
				{
					result = PackUnpackContainerRegistration.JC_Calc_TotalWeight + PackUnpackContainerRegistration.JC_Calc_TareWeight;
				}

				return result.ToString(2);
			}
		}

		public DocAddress DocCTOAddress
		{
			get { return DocAddress.New(CTOAddress, Factory); }
		}

		public ZString CTOName
		{
			get { return (DocCTOAddress != null) ? DocCTOAddress.Organisation.Name : ZString.Empty; }
		}

		public ZString ClientName
		{
			[DocumentEngineObsoleteField("Replace ClientName with <CFSClient.Name>")]
			get { return CFSClient != null ? CFSClient.Name : ZString.Empty; }
		}

		public ZString ETA
		{
			[DocumentEngineObsoleteField("Replace ETA with <Sailing.DestinationETA>")]
			get { return MostInterestingTransport != null ? MostInterestingTransport.ETA.ToShortDateString() : ""; }
		}

		public ZString ETD
		{
			[DocumentEngineObsoleteField("Replace ETD with <Sailing.OriginETD>")]
			get { return MostInterestingTransport != null ? MostInterestingTransport.ETD.ToShortDateString() : ""; }
		}

		public ZString CFSName
		{
			[DocumentEngineObsoleteField("Replace CFSName with <CurrentCompany.Name>")]
			get { return GlbCompany.CurrentCompany.GC_Name; }
		}

		public ZString Vessel
		{
			[DocumentEngineObsoleteField("Replace Vessel with <Sailing.Vessel.Code>")]
			get { return PackUnpackContainerRegistration != null ? PackUnpackContainerRegistration.JC_JV_NKVessel : ZString.Empty; }
		}

		public ZString SailingVoyage
		{
			[DocumentEngineObsoleteField("Replace Voyage with <Sailing.VoyageFlight>")]
			get { return Sailing != null ? Sailing.VoyageFlight : ZString.Empty; }
		}

		public ZString DeliveryInstructions
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, PackUnpackContainerRegistration); }
		}

		public ZString ECNAndDeliveryInstructions
		{
			get
			{
				ZString result = "";
				if (!ExportDepotCustomsReference.IsEmpty)
				{
					result = Res.GetString("392ec999-453b-4de7-9a08-c90cea2a2599", "ECN: {0}", ExportDepotCustomsReference) + "\n\n";
				}
				result += DeliveryInstructions;
				return result;
			}
		}

		public override ZString ArrivalTruckDriversLicense
		{
			get { return PackUnpackContainerRegistration.JC_ArrivalTruckDriversLicense; }
		}

		public override ZString DepartureTruckDriversLicense
		{
			get { return PackUnpackContainerRegistration.JC_DepartureTruckDriversLicense; }
		}

		#endregion

		#region ZGuid Fields

		//		private OrgAddress ContainerParkAddress
		//		{
		//			get { return PackUnpackContainerRegistration.ContainerParkAddress; } 
		//		}

		OrgAddress CTOAddress
		{
			get
			{
				OrgAddress result = null;
				if (PackUnpackContainerRegistration.Consol != null)
				{
					result = PackUnpackContainerRegistration.Consol.ArrivalCTOAddress;
				}
				return result;
			}
		}

		#endregion

		#region Collections

		public DocCFSShipmentCollection CFSShipmentsCollection
		{
			get
			{
				if (cfsShipmentsCollection == null)
				{
					cfsShipmentsCollection = new DocCFSShipmentCollection(Factory);
					System.Collections.Hashtable packLineTable = GetGroupedPackLineTable();
					foreach (PackLine line in PackUnpackContainerRegistration.PackLines)
					{
						if (line.Shipment != null)
						{
							for (int i = 0; i < line.JL_PackageCount; i++)
							{
								NonPersistentCFSShipment cfsShipment = GetNewCFSShipment(line.Shipment);
								DocCFSShipment docCFSShipment = DocCFSShipment.New(cfsShipment, Factory);

								DocShipmentReceival groupPackLine = packLineTable[line.Shipment.JS_UniqueConsignRef] as DocShipmentReceival;
								if (groupPackLine != null)
								{
									docCFSShipment.TotalPackages = groupPackLine.TotalNumberOfOuterPacks;
									docCFSShipment.CFSPackageType = groupPackLine.PackLinePackageType;
								}
								cfsShipmentsCollection.Add(docCFSShipment);
							}
						}
					}
				}

				return cfsShipmentsCollection;
			}
		}
		protected DocCFSShipmentCollection cfsShipmentsCollection;

		public DocShipmentCollection ContainerShipments
		{
			get
			{
				DocShipmentCollection result = new DocShipmentCollection(Factory);
				foreach (CFSShipment shipment in PackUnpackContainerRegistration.PackUnpackShipments)
				{
					DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
					result.Add(shipmentWrapper);
				}
				return result;
			}
		}

		public DocPackLinesCollection ContainerPackCount
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(Factory);
				foreach (PackLine packShip in PackUnpackContainerRegistration.PackLines)
				{
					if (packShip.Shipment != null)
					{
						DocPackLines pack = DocPackLines.New(packShip, Factory);
						result.Add(pack);
					}
				}
				result.SortOnShipmentNumber();
				return result;
			}
		}

		public DocCFSShipmentCollection CFSShipments
		{
			get
			{
				var result = new DocCFSShipmentCollection(PackUnpackContainerRegistration.Factory);
				if (ReportName.Trim().ToUpper() == DocBaseWrapper.ImportLabel)
				{
					CFSLoadListConsol consol = PackUnpackContainerRegistration.Consol;
					JobSailing sailing = PackUnpackContainerRegistration.Sailing;
					if (consol != null || sailing != null)
					{
						foreach (DocCFSShipment shipment in CFSShipmentsCollection)
						{
							if (shipment.IsImport(consol != null ? consol.JK_RL_NKDischargePort : sailing.JX_JB_RL_NKPortOfDischarge))
							{
								result.Add(shipment);
							}
						}
					}
				}
				else if (ReportName.Trim().ToUpper() == DocBaseWrapper.TranshipmentLabel && MostInterestingTransport != null)
				{
					foreach (DocCFSShipment shipment in CFSShipmentsCollection)
					{
						if (shipment.IsTranshipment(MostInterestingTransport.PortOfDischargeCode))
						{
							result.Add(shipment);
						}
					}
				}
				else if (ReportName.Trim().ToUpper() == DocBaseWrapper.OnForwardingLabel && MostInterestingTransport != null)
				{
					foreach (DocCFSShipment shipment in CFSShipmentsCollection)
					{
						if (shipment.IsOnForwarding(MostInterestingTransport.PortOfDischargeCode))
						{
							result.Add(shipment);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		CFSContainer PackUnpackContainerRegistration
		{
			get { return (CFSContainer)WrappedObject; }
		}

		protected NonPersistentCFSShipment GetNewCFSShipment(CommonShipment shipment)
		{
			var cfsShipment = new NonPersistentCFSShipment();
			cfsShipment.CFSVessel = Vessel;
			cfsShipment.CFSVoyage = Voyage;
			cfsShipment.ClientName = ClientName;
			cfsShipment.CFSContainerNumber = ContainerNumber;

			if (shipment != null)
			{
				var shipmentReceivalWrapper = DocShipmentReceival.New(PackUnpackContainerRegistration.Factory, shipment.PK);
				cfsShipment.ShipmentNumber = shipmentReceivalWrapper.ShipmentNumber;
				cfsShipment.ETA = !shipmentReceivalWrapper.ETA.IsEmpty ? new ZString(shipmentReceivalWrapper.ETA.ToShortDateString()) : ETA;
				cfsShipment.Dest = shipmentReceivalWrapper.DestinationCode;
				cfsShipment.HBL = shipmentReceivalWrapper.HouseBill;
				cfsShipment.Marks = shipmentReceivalWrapper.MarksAndNumbersLine;

				if (ReportName.Trim().ToUpper() == DocBaseWrapper.ImportLabel || ReportName.Trim().ToUpper() == DocBaseWrapper.OnForwardingLabel)
				{
					cfsShipment.LabelConNote = shipmentReceivalWrapper.ConNote;
					cfsShipment.LabelConNoteHeading = !cfsShipment.LabelConNote.IsEmpty ? "CON" : String.Empty;
					cfsShipment.LabelConsignee = shipmentReceivalWrapper.Consignee != null ? shipmentReceivalWrapper.Consignee.Name : ZString.Empty;
					cfsShipment.LabelConsigneeHeading = "CNE";
				}
			}

			return cfsShipment;
		}

		protected System.Collections.Hashtable GetGroupedPackLineTable()
		{
			System.Collections.Hashtable table = new System.Collections.Hashtable();
			foreach (PackLine line in PackUnpackContainerRegistration.PackLines)
			{
				if (line.Shipment != null)
				{
					if (!table.ContainsKey(line.Shipment.JS_UniqueConsignRef))
					{
						DocShipmentReceival docShipmentReceival = DocShipmentReceival.New(Factory, line.Shipment.PK);
						docShipmentReceival.TotalNumberOfOuterPacks = line.JL_PackageCount;
						docShipmentReceival.PackLinePackageType = line.JL_F3_NKPackType;
						table.Add(line.Shipment.JS_UniqueConsignRef, docShipmentReceival);
					}
					else
					{
						DocShipmentReceival existingLine = table[line.Shipment.JS_UniqueConsignRef] as DocShipmentReceival;
						if (existingLine != null)
						{
							existingLine.TotalNumberOfOuterPacks += line.JL_PackageCount;
							if (existingLine.PackLinePackageType != line.JL_F3_NKPackType)
							{
								existingLine.PackLinePackageType = "PKG";
							}
						}
					}
				}
			}
			return table;
		}

		#endregion
	}
}
