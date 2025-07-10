using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocBaseConsol : DocBaseWrapper
	{
		public static DocBaseConsol New(CommonConsol consol, BusinessObjectFactory factoryToWrap)
		{
			return new DocBaseConsol(consol, factoryToWrap);
		}

		protected DocBaseConsol(CommonConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
		}

		public CommonConsol Consol
		{
			get { return (CommonConsol)WrappedObject; }
		}

		#region Virtuals

		public virtual DocUNLOCO FirstLoadPort
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_RL_NKLoadPort); }
		}

		public virtual DocUNLOCO LastDischargePort
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_RL_NKDischargePort); }
		}

		public virtual DocUNLOCO PortOfDischarge
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_JX_JB_RL_NKPortOfDischarge); }
		}

		public virtual DocUNLOCO LastLegPortOfDischarge
		{
			get
			{
				var transports = TransportPlanning.Cast<DocTransport>();
				return transports.LastOrDefault().PortOfDischarge;
			}
		}

		public virtual DocUNLOCO PortOfLoading
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_JX_JA_RL_NKPortOfLoading); }
		}

		public virtual DocUNLOCO FirstLegPortOfLoading
		{
			get
			{
				var transports = TransportPlanning.Cast<DocTransport>();
				return transports.FirstOrDefault().PortOfLoading;
			}
		}

		public ZString PortOfDischargeIATA
		{
			get { return PortOfDischarge != null ? (PortOfDischarge.IATA.IsEmpty ? PortOfDischarge.Code : PortOfDischarge.IATA) : ZString.Empty; }
		}

		public ZString PortOfLoadingIATA
		{
			get { return PortOfLoading != null ? (PortOfLoading.IATA.IsEmpty ? PortOfLoading.Code : PortOfLoading.IATA) : ZString.Empty; }
		}

		public virtual ZDateTime LastLegETA
		{
			get
			{
				var transports = TransportPlanning.Cast<DocTransport>();
				return transports.LastOrDefault().ETA;
			}
		}

		public virtual ZDateTime FirstLegETD
		{
			get
			{
				var transports = TransportPlanning.Cast<DocTransport>();
				return transports.FirstOrDefault().ETD;
			}
		}

		public virtual ZDateTime ETA
		{
			get { return Consol.JK_JX_JB_E_ARV; }
		}

		public virtual ZString ETAString
		{
			get
			{
				if (!this.TransportMode.IsEmpty)
				{
					return (this.TransportMode == Core.Constants.TransportModes.Sea) ? ETA.ToShortDateString() : ETA.ToLongTimeString();
				}

				return ZString.Empty;
			}
		}

		public virtual ZDateTime ETD
		{
			get { return Consol.JK_JX_JA_E_DEP; }
		}

		public virtual ZString ETDString
		{
			get
			{
				if (!this.TransportMode.IsEmpty)
				{
					return (this.TransportMode == Core.Constants.TransportModes.Sea) ? ETD.ToShortDateString() : ETD.ToLongTimeString();
				}

				return ZString.Empty;
			}
		}

		public ZString ATDString
		{
			get
			{
				ZString result = "";

				if (!this.TransportMode.IsEmpty)
				{
					result = (this.TransportMode == Core.Constants.TransportModes.Sea) ? ATD.ToShortDateString() : ATD.ToLongTimeString();
				}

				return result;
			}
		}

		public virtual ZString ContainerInfoForInvoice
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocContainer currentContainer in Containers)
				{
					result += currentContainer.ContainerNumber + "/" + currentContainer.ContainerMode;
					if (currentContainer.Container != null)
					{
						result += "/" + currentContainer.Container.Code;
					}

					result += ", ";
				}

				result = result.TrimEndIncludingWhiteSpace(',');
				return result;
			}
		}

		DocContainerCollectionHelper FreightContainerSupport
		{
			get { return freightContainerSupport ?? (freightContainerSupport = new DocContainerCollectionHelper(MaximumContainers, true)); }
		}
		DocContainerCollectionHelper freightContainerSupport;

		public virtual ZString ContainerInfoForInvoiceLine
		{
			get
			{
				return FreightContainerSupport.ContainerNumberAndType(Containers.ToIDocSimpleContainerCollection());
			}
		}

		public virtual ZString ContainerNumberOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocContainer currentContainer in Containers)
				{
					if (currentContainer != null)
					{
						result += currentContainer.ContainerNumber + System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public virtual ZString ContainerModeOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocContainer currentContainer in Containers)
				{
					if (currentContainer != null)
					{
						result += currentContainer.ContainerMode + System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public virtual ZString ContainerTypeOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocContainer currentContainer in Containers)
				{
					if (currentContainer != null)
					{
						result += currentContainer.Container != null ?
									currentContainer.Container.Code + System.Environment.NewLine :
									System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public virtual ZBool PrintPageWithContainerNumber
		{
			get
			{
				return (Containers.Count > MaximumContainers && AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value);
			}
		}

		const int MaximumContainers = 4;

		public virtual ZString ContainerInfoForNotes
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocContainer currentContainer in Containers)
				{
					result += currentContainer.ContainerNumber + "/" + currentContainer.ContainerMode + "/" + currentContainer.SealNumber;
					if (currentContainer.Container != null)
					{
						result += "/" + currentContainer.Container.Code;
					}

					result += ", ";
				}

				result = result.TrimEndIncludingWhiteSpace(',');
				return result;
			}
		}

		public virtual DocOrganisation ShippingLine
		{
			get
			{
				DocOrganisation result = DocOrganisation.New(Consol.ShippingLineAddress, Factory);
				if (result == null)
				{
					if (Consol.Schedule != null && Consol.Schedule.Voyage != null)
					{
						result = DocOrganisation.New(Consol.Schedule.Voyage.Line, Factory);
					}
				}
				return result;
			}
		}

		public virtual DocOrganisation Creditor
		{
			get { return DocOrganisation.New(Consol.CreditorAddress, Factory); }
		}

		public virtual ZString EmailSubjectNumber
		{
			get { return ConsolNumber; }
		}

		public ZString ReportNameWithoutCFS
		{
			get { return ReportName.Replace(" - CFS", ""); }
		}

		#endregion

		#region Common Consol

		protected void SetFromDocumentCommonConsol(DocumentCommonConsol documentCommonConsol)
		{
			if (documentCommonConsol != null)
			{
				fIncludeConsignee = documentCommonConsol.IncludeConsignee;
				fIncludeConsignor = documentCommonConsol.IncludeConsignor;
				fIncludeCustomsBroker = documentCommonConsol.IncludeCustomsBroker;
				fIncludeAllShipments = documentCommonConsol.IncludeAllShipments;
				fIncludePacked = documentCommonConsol.IncludePacked;
				fIncludeUnPacked = documentCommonConsol.IncludeUnPacked;
				ContainerToPrint = DocContainer.New(documentCommonConsol.ContainerToPrint, Factory);
			}
		}

		protected ZBool fIncludeConsignee;
		public ZBool IncludeConsignee
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludeConsignee; }
		}

		protected ZBool fIncludeConsignor;
		public ZBool IncludeConsignor
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludeConsignor; }
		}

		ZBool fIncludeCustomsBroker;
		public ZBool IncludeCustomsBroker
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludeCustomsBroker; }
		}

		ZBool fIncludeAllShipments;
		public ZBool IncludeAllShipments
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludeAllShipments; }
		}

		ZBool fIncludePacked;
		public ZBool IncludePacked
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludePacked; }
		}

		ZBool fIncludeUnPacked;
		public ZBool IncludeUnPacked
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIncludeUnPacked; }
		}

		public DocContainer ContainerToPrint { get; private set; }

		#endregion

		#region Document Constants

		public ZInt SortShipmentsOnHBL
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.SortShipmentsOnHBL, 1); }
		}

		public ZInt NumberOfTransportPlanningRows
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransportPlanningRows, 1); }
		}

		#endregion

		#region Headings

		public ZString HBLOrHAWBHeading
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Air)
				{
					return Res.GetString("7c7d9396-4039-4e00-8fc5-c1939e7b7378", "HAWB:");
				}
				else
				{
					return Res.GetString("cc380315-ba24-4169-b467-c55bbf433d15", "HBL:");
				}
			}
		}

		public ZString TransportHeading
		{
			get
			{
				string transportHeading = "";
				if (!this.TransportMode.IsEmpty)
				{
					switch (this.TransportMode)
					{
						case Core.Constants.TransportModes.Air:
						case Core.Constants.TransportModes.AirSea:
							transportHeading = Res.GetString("62fc1a95-2505-487d-9626-89f3cbb15a39", "FLIGHT & DATE");
							break;

						case Core.Constants.TransportModes.Rail:
						case Core.Constants.TransportModes.Road:
							transportHeading = Res.GetString("fb7a4f00-ee63-44dc-be31-0ffcd0feee91", "JOURNEY NAME / JOURNEY NUMBER");
							break;

						default:
						case Core.Constants.TransportModes.Sea:
						case Core.Constants.TransportModes.SeaAir:
							transportHeading = Res.GetString("c3aeba91-1bad-46d4-ba5c-9168bbe92bf6", "VESSEL / VOYAGE / IMO(Lloyds)");
							break;
					}
				}

				return transportHeading;
			}
		}

		public ZString MasterBillHeading
		{
			get
			{
				switch (this.TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("69c0a722-9639-49df-99db-a2a4d310155a", "MAWB");

					case Core.Constants.TransportModes.Sea:
						return Res.GetString("4e2a7ab2-38cd-4d81-bc12-12001e4d6a30", "OCEAN BILL OF LADING");

					default:
						return Res.GetString("1c8b45bf-f024-4448-b594-c17b56cd38d3", "MASTER");
				}
			}
		}

		public ZString MasterBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(MasterBillHeading, MasterBillIssueDate); }
		}

		public ZString TransportLabelHeading
		{
			get
			{
				string transportHeading = "";
				if (!this.TransportMode.IsEmpty)
				{
					if (this.TransportMode.Trim() == Core.Constants.TransportModes.Air)
					{
						transportHeading = Res.GetString("fbe7324b-3ea2-4d19-95ee-ae4c37bb9f1c", "FLIGHT");
					}
					else if (this.TransportMode.Trim() == Core.Constants.TransportModes.Sea)
					{
						transportHeading = Res.GetString("11d72a41-f0d7-4263-a78f-fd40d8459c59", "VESSEL");
					}
				}

				return transportHeading;
			}
		}

		public ZString MasterBillLabelHeading
		{
			get
			{
				switch (this.TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("69c0a722-9639-49df-99db-a2a4d310155a", "MAWB");

					case Core.Constants.TransportModes.Sea:
						return Res.GetString("e190290d-5e01-4c40-84b9-7300934e9189", "OBL");

					default:
						return Res.GetString("1c8b45bf-f024-4448-b594-c17b56cd38d3", "MASTER");
				}
			}
		}

		public ZString CRNHeading
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
				{
					return Res.GetString("56ccc513-0a74-4fd2-aadf-07600a40054f", "CRN");
				}
				else
				{
					return IsExportConsol ? Res.GetString("56ccc513-0a74-4fd2-aadf-07600a40054f", "CRN") : "";
				}
			}
		}

		public ZString RequestForProfitShareOpeningText
		{
			get { return DocumentsDataRegistry.Instance.RequestForProfitShareOpeningText_Consol.Value; }
		}

		public ZString RequestForProfitShareDocumentHeader
		{
			get { return Res.GetString("a1c479c7-0949-4000-9738-a5874b4671fb", "Request for Consol Profit Share Credit Note"); }
		}

		#endregion

		#region Icelandic

		public ZInt TotalShipments
		{
			get { return Consol.Shipments.Count; }
		}

		Regex GGGGTemplate
		{
			get
			{
				if (ggggTemplate == null)
				{
					ggggTemplate = new Regex("\\w?(\\d{3,4})");
				}
				return ggggTemplate;
			}
		}
		Regex ggggTemplate;

		public ZString FirstAbbreviatedCustomsShipmentNumber
		{
			get
			{
				ZString result = ZString.Empty;
				int minNum = int.MaxValue;
				foreach (CommonShipment sh in Consol.Shipments)
				{
					if (!sh.CustomsEntryNumber.IsEmpty)
					{
						Sendingarnumer code = new Sendingarnumer(sh.CustomsEntryNumber);
						if (code != null)
						{
							string currentCarrierNumber = code.CarrierNumber;
							Match match = GGGGTemplate.Match(currentCarrierNumber);
							if (match.Success && match.Groups.Count == 2)
							{
								int curnum = Convert.ToInt32(match.Groups[1].Value);
								if (minNum > curnum)
								{
									result = currentCarrierNumber;
									minNum = curnum;
								}
							}
						}
					}
				}

				return result;
			}
		}

		public ZString LastAbbreviatedCustomsShipmentNumber
		{
			get
			{
				ZString result = ZString.Empty;
				int maxNum = int.MinValue;
				foreach (CommonShipment sh in Consol.Shipments)
				{
					if (!sh.CustomsEntryNumber.IsEmpty)
					{
						Sendingarnumer code = new Sendingarnumer(sh.CustomsEntryNumber);
						if (code != null)
						{
							string currentCarrierNumber = code.CarrierNumber;
							Match match = GGGGTemplate.Match(currentCarrierNumber);
							if (match.Success && match.Groups.Count == 2)
							{
								int curnum = Convert.ToInt32(match.Groups[1].Value);
								if (maxNum < curnum)
								{
									result = currentCarrierNumber;
									maxNum = curnum;
								}
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Weight

		public ZString TotalWeight
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(Consol.GetTotalShipmentWeightForDoc(displayOption));
			}
		}

		public ZString WeightUnit
		{
			get { return Consol.JK_TotalShipmentWeightUnit; }
		}

		#endregion

		#region Volume

		public ZString TotalVolume
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(Consol.GetTotalShipmentVolumeForDoc(displayOption));
			}
		}

		public ZString VolumeUnit
		{
			get { return Consol.JK_TotalShipmentVolumeUnit; }
		}

		#endregion

		#region Chargeable

		public ZString TotalChargeable
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(Consol.GetTotalShipmentChargeableForDoc(displayOption));
			}
		}

		public ZString ChargeableUnit
		{
			get { return Consol.JK_ConsolChargeableUnit; }
		}

		public ZString FreightChargeType
		{
			get
			{
				if (PrepaidCollect.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (PrepaidCollect == Core.Constants.PaymentType.Prepaid)
				{
					return Res.GetString("17724b7d-dfe2-40ce-bca3-6000183edc8f", "FREIGHT PREPAID");
				}
				else
				{
					return Res.GetString("bfece5c2-e8ef-4d15-bdf4-c4659b381152", "FREIGHT COLLECT");
				}
			}
		}

		#endregion

		#region Doc Organisation Fields

		public DocOrganisation SendingForwarder
		{
			get { return DocOrganisation.New(Consol.SendingForwarderAddress, Factory); }
		}

		public DocOrganisation ReceivingForwarder
		{
			get { return DocOrganisation.New(Consol.ReceivingForwarderAddress, Factory); }
		}

		public DocOrganisation NotifyPartyOrganisation
		{
			get { return DocOrganisation.New(((ForwardingConsol)Consol).NotifyPartyDocumentaryAddress, Factory); }
		}

		#endregion

		#region Doc Address Fields

		public DocDocAddress ContainerParkEmptyPickupAddress
		{
			get { return DocDocAddress.New(Consol.ContainerYardEmptyPickupAddress, Factory); }
		}

		public DocDocAddress ContainerParkEmptyReturnAddress
		{
			get { return DocDocAddress.New(Consol.ContainerYardEmptyReturnAddress, Factory); }
		}

		public DocDocAddress ArrivalCTOAddress
		{
			get { return DocDocAddress.New(Consol.ArrivalCTOAddress, Factory); }
		}

		public DocDocAddress UnpackDepotAddress
		{
			get { return DocDocAddress.New(Consol.UnpackDepotAddress, Factory); }
		}

		public DocDocAddress PackDepotAddress
		{
			get { return DocDocAddress.New(Consol.PackDepotAddress, Factory); }
		}

		public DocDocAddress DepartureCTOAddress
		{
			get { return DocDocAddress.New(Consol.DepartureCTOAddress, Factory); }
		}

		public DocDocAddress GoodsAvailableAt
		{
			get
			{
				DocDocAddress goodsAvailAt = this.UnpackDepotAddress ?? this.ArrivalCTOAddress;
				if (this.TransportMode == Core.Constants.TransportModes.Sea && this.ConsolMode == Core.Constants.ContainerModes.FCL)
				{
					if (this.ArrivalCTOAddress != null)
					{
						goodsAvailAt = this.ArrivalCTOAddress;
					}
				}
				return goodsAvailAt;
			}
		}

		#endregion

		#region Doc Wrappers

		public DocVessel Vessel
		{
			get { return !Consol.JK_JX_JV_NKVessel.IsEmpty ? DocVessel.New(Consol.Factory, Consol.JK_JX_JV_NKVessel) : null; }
		}

		public DocSailing Sailing
		{
			get { return DocSailing.New(Consol.Schedule, Factory); }
		}

		#endregion

		#region Consol Fields

		public ZGuid ConsolPK
		{
			get { return Consol.PK; }
		}

		public ZInt NoOfOriginalBills
		{
			get { return Convert.ToInt16(Consol.JK_NoOriginalBills); }
		}

		public ZInt NoOfCopyBills
		{
			get { return Convert.ToInt16(Consol.JK_NoCopyBills); }
		}

		public ZString AgentsReference
		{
			get { return Consol.JK_AgentsReference; }
		}

		public ZString TransportMode
		{
			get { return Consol.JK_TransportMode; }
		}

		public ZString TransportModeDescription
		{
			get { return Consol.JK_TransportMode_List.GetDescriptionFromCode(TransportMode); }
		}

		public ZString HeadingTransportMode
		{
			get
			{
				ZString heading = TransportModeDescription;
				ZString wordToRemove = Res.GetString("ac74d80e-afb5-42f3-a235-92b5e506842a", "Freight");

				if (heading.EndsWith(wordToRemove))
				{
					heading = heading.RemoveSafe(heading.LastIndexOf(wordToRemove), wordToRemove.Length);
				}

				return heading.Trim();
			}
		}

		public ZString VesselName
		{
			get { return Consol.JK_JX_JV_NKVessel; }
		}

		public ZString ConsolNumber
		{
			get { return Consol.JK_UniqueConsignRef; }
		}

		public ZString VoyageNumber
		{
			get { return Consol.JK_JX_JV_VoyageFlight; }
		}

		public ZString UnpackDepotCompanyName
		{
			get { return (UnpackDepotAddress != null) ? UnpackDepotAddress.CompanyName : ZString.Empty; }
		}

		public ZString SecondJobNumber
		{
			get { return ""; }
		}

		public ZString SecondJobNumberHeading
		{
			get { return ""; }
		}

		public ZDateTime DocsCutOff
		{
			get
			{
				return Consol.Transports.MostInterestingTransport.JW_DocumentaryCutOff;
			}
		}

		public ZDateTime FCLCutOff
		{
			get { return Consol.Transports.MostInterestingTransport.JW_TerminalCutOff; }
		}

		public ZDateTime LCLCutOff
		{
			get { return Consol.Transports.MostInterestingTransport.JW_DepotCutOff; }
		}

		public ZDateTime AvailabilityDate
		{
			get { return Consol.Transports.MostInterestingTransport.JW_TerminalAvailabilityDate; }
		}

		public ZDateTime StorageCommences
		{
			get { return Consol.Transports.MostInterestingTransport.JW_TerminalStorageDate; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return Consol.Transports.MostInterestingTransport.JW_DepotAvailabilityDate; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return Consol.Transports.MostInterestingTransport.JW_DepotStorageDate; }
		}

		public ZDateTime BondDate
		{
			get { return (ETA != ZDateTime.Empty) ? ETA.AddMonths(2).AddDays(-ETA.Day) : ZDateTime.Empty; }
		}

		public ZString CustomsEntryNumberForExportOnly
		{
			get
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland)
				{
					return Consol.JK_CRN;
				}
				else
				{
					return IsExportConsol ? Consol.JK_CRN : ZString.Empty;
				}
			}
		}

		public ZBool IsAir
		{
			get { return Consol.IsAir; }
		}

		public ZBool IsSea
		{
			get { return Consol.IsSea; }
		}

		public ZBool IsNeutralMaster
		{
			get { return Consol.JK_IsNeutralMaster; }
		}

		public ZBool IsDirect
		{
			get { return Consol.IsDirect; }
		}

		public ZBool IsCoLoad
		{
			get { return Consol.IsCoLoad; }
		}

		public ZBool IsGatewayCoLoad => Consol.IsCoLoad && Consol.IsSendingOrReceivingForwarderGateway;

		public ZBool IsCoLoadOrGatewayCoLoad => Consol.IsCoLoad;

		public ZBool IsAgent
		{
			get { return Consol.IsAgent; }
		}

		public ZBool IsAgentGateway => Consol.IsAgent && Consol.IsSendingOrReceivingForwarderGateway;

		public ZBool IsGatewayConsolType => Consol.IsGatewayConsol;

		public ZBool IsCharter
		{
			get { return Consol.IsCharter; }
		}

		public ZBool IsOther
		{
			get { return Consol.JK_AgentType == Core.Constants.AgentType.Other; }
		}

		public ZBool IsImportConsol
		{
			get
			{
				//Country of Discharge port = Country of current company
				return PortOfDischarge != null && PortOfDischarge.CountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
		}

		public ZBool IsExportConsol
		{
			get
			{
				//Country of Loading port = Country of current company
				return PortOfLoading != null && PortOfLoading.CountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
		}

		public ZBool IsPrepaid
		{
			get { return Consol.JK_PrepaidCollect == Core.Constants.PaymentType.Prepaid; }
		}

		public ZBool IsCollect
		{
			get { return Consol.JK_PrepaidCollect == Core.Constants.PaymentType.Collect; }
		}

		public ZString MasterBillNum
		{
			get { return Consol.JK_MasterBillNum; }
		}

		public ZString MasterBillNumberWithHyphen
		{
			get { return Consol.IsAir && Consol.JK_MasterBillNum.Length > 3 ? Consol.JK_MasterBillNum.InsertSafe(3, "-") : Consol.JK_MasterBillNum; }
		}

		public ZString NKPortOfDischarge
		{
			get { return Consol.JK_JX_JB_RL_NKPortOfDischarge; }
		}

		public ZString NKPortOfLoading
		{
			get { return Consol.JK_JX_JA_RL_NKPortOfLoading; }
		}

		public ZString NKFirstForeignPort
		{
			get { return Consol.JK_RL_NKFirstForeignPort; }
		}

		public ZString NKLastForeignPort
		{
			get { return Consol.JK_RL_NKLastForeignPort; }
		}

		public ZString NKPortOfFirstArrival
		{
			get { return Consol.JK_RL_NKPortOfFirstArrival; }
		}

		public ZString MasterBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(MasterBillNum, MasterBillIssueDate); }
		}

		public virtual ZDateTime ATA
		{
			get { return Consol.JK_JX_JB_A_ARV.IsEmpty ? Consol.JK_JX_JB_E_ARV : Consol.JK_JX_JB_A_ARV; }
		}

		public virtual ZDateTime ATD
		{
			get { return Consol.JK_JX_JA_A_DEP.IsEmpty ? Consol.JK_JX_JA_E_DEP : Consol.JK_JX_JA_A_DEP; }
		}

		public ZString BookingReference
		{
			get { return Consol.JK_BookingReference; }
		}

		public ZString ConsolMode
		{
			get { return Consol.JK_ConsolMode; }
		}

		public ZString PreCarriageVessel
		{
			get
			{
				ZString result = "";
				foreach (DocTransport docConsolTransportPlanning in this.TransportPlanning)
				{
					if (docConsolTransportPlanning.TransportType == Core.Constants.TransportPlanningType.PreCarriage)
					{
						result = docConsolTransportPlanning.VesselName;
					}
				}
				return result;
			}
		}

		internal DocTransport MainVesselLeg
		{
			get
			{
				var transports = TransportPlanning.Cast<DocTransport>();
				return transports.FirstOrDefault(docTransport => docTransport.TransportType == Core.Constants.TransportPlanningType.MainVessel && !docTransport.VesselName.IsEmpty);
			}
		}

		public ZBool IsCFS
		{
			get { return Consol.JK_IsCFS; }
		}

		public ZBool IsForwarding
		{
			get { return Consol.JK_IsForwarding; }
		}

		public ZString ConsolCRN
		{
			get { return Consol.JK_CRN; }
		}

		public ZString CRNCarrierNumer
		{
			get
			{
				ZString result = ZString.Empty;
				if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
				{
					Sendingarnumer sc = new Sendingarnumer(Consol.JK_CRN);
					result = sc.CarrierNumber;
				}
				return result;
			}
		}

		public ZString PrepaidCollect
		{
			get { return Consol.JK_PrepaidCollect; }
		}

		public ZString PrepaidCollectDescription
		{
			get
			{
				ZString result = ZString.Empty;

				string temp = Consol.JK_PrepaidCollect_List.GetDescriptionFromCode(PrepaidCollect);
				if (temp != null)
				{
					result = temp.ToUpper();
				}

				return result;
			}
		}

		public ZString ConsolTransportInfo
		{
			get
			{
				ZString result = ZString.Empty;
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
						result = GetAirTransportDetails;
						break;

					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
						result = VesselName + " / " + VoyageNumber;
						break;

					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						ZString lloydsNumber = Vessel == null ? ZString.Empty : Vessel.LloydsNumber;
						result = FormatTransportDetails(VesselName, VoyageNumber, lloydsNumber);
						break;
				}
				return result;
			}
		}

		public ZString TransportInfoWithDate
		{
			get
			{
				ZString result = ZString.Empty;
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
						result = FormatTransportDetails(VoyageNumber, NKPortOfDischarge, ATDString == ZString.Empty ? ETDString : ATDString);
						break;

					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						result = FormatTransportDetails(VesselName, VoyageNumber, ATDString == ZString.Empty ? ETDString : ATDString);
						break;
				}

				return result;
			}
		}

		public ZDecimal TotalPackageCount
		{
			get { return Consol.JK_TotalShipmentQuantity; } //total outer packs
		}

		public ZDateTime DateFirstForeignPort
		{
			get { return Consol.JK_DateFirstForeignPort; }
		}

		public ZDateTime DateLastForeignPort
		{
			get { return Consol.JK_DateLastForeignPort; }
		}

		public ZDateTime DatePortOfFirstArrival
		{
			get { return Consol.JK_DatePortOfFirstArrival; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return Consol.Transports.MostInterestingTransport.JW_DepotReceivalCommences; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return Consol.Transports.MostInterestingTransport.JW_TerminalReceivalCommences; }
		}

		public ZDateTime StorageDate
		{
			get { return Consol.Transports.MostInterestingTransport.JW_TerminalStorageDate; }
		}

		public ZDateTime MasterBillIssueDate
		{
			get { return IsAir ? Consol.JK_MasterBillIssueDate : ZDateTime.Empty; }
		}

		public ZString DefaultReleaseType
		{
			get
			{
				return (ZString)ReleaseType_List.GetDescriptionFromCode(DocumentsDataRegistry.Instance.ReleaseType.Value);
			}
		}

		public ZString ReleaseTypeCode
		{
			get
			{
				return Consol.JK_ReleaseType;
			}
		}

		public ZString ReleaseType
		{
			get
			{
				ZString result = "";

				if (!ReleaseTypeCode.IsEmpty)
				{
					result = ReleaseType_List.GetDescriptionFromCode(ReleaseTypeCode);
				}

				return result;
			}
		}

		CodeDescriptionPairList ReleaseType_List
		{
			get
			{
				return Consol.JK_ReleaseType_List;
			}
		}

		#endregion

		#region Collections

		public ZInt TransportPlanningCount
		{
			get { return (TransportPlanning != null) ? TransportPlanning.Count : 0; }
		}

		public DocTransportCollection TransportPlanning
		{
			get
			{
				DocTransportCollection transportCollection = new DocTransportCollection(Consol.Factory);
				foreach (Transport transport in Consol.Transports)
				{
					transportCollection.Add(DocTransport.New(Consol, transport, Factory));
				}

				transportCollection.Sort("LegOrder", ListSortDirection.Ascending);
				return transportCollection;
			}
		}

		public DocContainerCollection Containers
		{
			get
			{
				DocContainerCollection containerCollection = GetContainerCollection();
				containerCollection.Sort("ContainerNumber", ListSortDirection.Ascending);
				return containerCollection;
			}
		}

		public DocContainerCollection ContainersNoSortingOrder
		{
			get { return GetContainerCollection(); }
		}

		#endregion

		#region *Transports

		public DocTransport ArrivalTransport
		{
			get { return DocTransport.New(Consol, Consol.Transports.ArrivalTransport, Factory); }
		}

		public DocTransport DepartureTransport
		{
			get { return DocTransport.New(Consol, Consol.Transports.DepartureTransport, Factory); }
		}

		public DocTransport ExportTransport
		{
			get { return DocTransport.New(Consol, Consol.Transports.ExportTransport, Factory); }
		}

		public DocTransport ImportTransport
		{
			get { return DocTransport.New(Consol, Consol.Transports.ImportTransport, Factory); }
		}

		#endregion

		#region DocManager barcode properties

		protected override ZString DocManagerUniqueID
		{
			get { return ConsolNumber; }
		}

		#endregion

		#region GetFirstAndSecondLegAirTransportDetails

		protected internal ZString GetFirstAndSecondLegAirTransportDetails()
		{
			ZString result = "";
			DocTransport firstLeg = null;
			DocTransport secondLeg = null;

			if (TransportPlanningSortedByTransportType.Count > 0)
			{
				firstLeg = TransportPlanningSortedByTransportType[0];
			}
			if (TransportPlanningSortedByTransportType.Count > 1)
			{
				secondLeg = TransportPlanningSortedByTransportType[1];
			}

			bool bothTransportLegsFound = (firstLeg != null && secondLeg != null);
			if (!bothTransportLegsFound && TransportPlanningOtherSortedByETD.Count > 0)
			{
				if (firstLeg == null)
				{
					firstLeg = TransportPlanningOtherSortedByETD[0];
				}
				else
				{
					secondLeg = TransportPlanningOtherSortedByETD[0];
				}
				if (secondLeg == null && TransportPlanningOtherSortedByETD.Count > 1)
				{
					secondLeg = TransportPlanningOtherSortedByETD[1];
				}
			}

			if (firstLeg != null)
			{
				ZString discPort = (firstLeg.PortOfDischarge != null) ? firstLeg.PortOfDischarge.Code : ZString.Empty;
				result = FormatFlightDetails(firstLeg.VoyageFlight, discPort, firstLeg.ETD);
			}

			if (secondLeg != null)
			{
				result += " -> ";
				ZString discPort = (secondLeg.PortOfDischarge != null) ? secondLeg.PortOfDischarge.Code : ZString.Empty;
				result += FormatFlightDetails(secondLeg.VoyageFlight, discPort, secondLeg.ETD);
			}

			return result;
		}

		protected DocTransportCollection TransportPlanningSortedByTransportType
		{
			get
			{
				if (fOrderedTransportPlanning == null)
				{
					fOrderedTransportPlanning = new DocTransportCollection(Consol.Factory);
					ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_TransportType, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.TransportPlanningType.Other);
					filter.OrderBy = JobConsolTransportSchema.Constants.JW_TransportType;

					Transport[] transports = (Transport[])Consol.Transports.Find(filter);
					foreach (Transport transport in transports)
					{
						fOrderedTransportPlanning.Add(DocTransport.New(Consol, transport, Factory));
					}
				}

				return fOrderedTransportPlanning;
			}
		}

		protected DocTransportCollection TransportPlanningOtherSortedByETD
		{
			get
			{
				if (fOrderedOtherTransportPlanning == null)
				{
					fOrderedOtherTransportPlanning = new DocTransportCollection(Consol.Factory);
					ZQuery filter = new ZQuery(JobConsolTransportSchema.JW_TransportType, Enterprise.Core.Constants.TransportPlanningType.Other);

					Transport[] transports = (Transport[])Consol.Transports.Find(filter);
					foreach (Transport transport in transports)
					{
						fOrderedOtherTransportPlanning.Add(DocTransport.New(Consol, transport, Factory));
					}

					fOrderedOtherTransportPlanning.Sort(new SortInfo("ETD", ListSortDirection.Ascending));
				}

				return fOrderedOtherTransportPlanning;
			}
		}

		DocTransportCollection fOrderedTransportPlanning;
		DocTransportCollection fOrderedOtherTransportPlanning;

		#endregion

		#region STC Label For US Bound

		public ZString STC_Label
		{
			get { return Consol.ConsolPassesThroughCountry(Core.Constants.CountryCodes.UnitedStates) ? "" : Res.GetString("446a553d-ae08-45f5-a955-6b64b974c565", "STC") + " "; }
		}

		#endregion

		#region Implementation

		protected internal ZString GetAirTransportDetails
		{
			get
			{
				ZString result = ZString.Empty;
				if (TransportPlanning.Count == 1)
				{
					result = GetAirTransportAndLegDetails();
				}
				else
				{
					result = GetFirstAndSecondLegAirTransportDetails();
				}

				return result;
			}
		}

		ZString GetAirTransportAndLegDetails()
		{
			ZString result = ZString.Empty;
			ZString transportLegLoadPort = TransportPlanning[0].PortOfLoadingCode;
			ZString transportLegDischargePort = TransportPlanning[0].PortOfDischargeCode;

			if (transportLegDischargePort == NKPortOfDischarge && transportLegLoadPort == NKPortOfLoading)
			{
				result = FormatFlightDetails(VoyageNumber, NKPortOfDischarge, DepartureTime);
			}
			else
			{
				result = FormatFlightDetails(VoyageNumber, NKPortOfDischarge, DepartureTime);
				result += " -> ";
				result += FormatFlightDetails(TransportPlanning[0].VoyageFlight, transportLegDischargePort, TransportPlanning[0].ETD);
			}
			return result;
		}

		protected internal ZDateTime DepartureTime
		{
			get { return this.ATD.IsEmpty ? this.ETD : this.ATD; }
		}

		protected internal ZString FormatFlightDetails(ZString voyageNo, ZString portOfDischarge, ZDateTime etd)
		{
			ZString departureDate = Suppression.GetValue(etd.ToShortDateString(), Consol, SuppressFields.ETD, ZString.Empty, DocumentContactType);
			return FormatTransportDetails(voyageNo, portOfDischarge, departureDate);
		}

		protected internal ZString FormatTransportDetails(ZString string1, ZString string2, ZString string3)
		{
			ZString result = ZString.Empty;

			if (!string1.IsEmpty)
			{
				result = string1;
			}
			else
			{
				result = "    ";
			}

			if (!string2.IsEmpty)
			{
				result += " / " + string2;
			}
			else
			{
				result += " /    ";
			}

			if (!string3.IsEmpty)
			{
				result += " / " + string3;
			}
			else
			{
				result += " /    ";
			}

			return result;
		}

		protected DocContainerCollection GetContainerCollection()
		{
			DocContainerCollection containerCollection = new DocContainerCollection(Consol.Factory);
			foreach (CommonContainer container in Consol.Containers)
			{
				containerCollection.Add(DocContainer.New(container, Factory));
			}
			return containerCollection;
		}

		#endregion
	}
}
