using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DocumentWrappers.DocJobInvoicingJob;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocForwardingConsol : DocBaseConsol, Integration.DocumentWrappers.IDocForwardingConsol, IConsolDeliveryAgent, IDocJobDetail, IRequestForMissingDocuments, ITimeSlotRequest, IDocCartageAdvice
	{
		protected DocForwardingConsol(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
		}

		public static DocForwardingConsol New(DocumentImportCargoLabel documentForwardingConsol, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingConsol wrapper = null;
			if (documentForwardingConsol != null && documentForwardingConsol.Consol != null)
			{
				wrapper = New(documentForwardingConsol.Consol, factoryToWrap);
				wrapper.fIncludeConsignee = documentForwardingConsol.IncludeConsignee;
				wrapper.fIncludeConsignor = documentForwardingConsol.IncludeConsignor;
				wrapper.fIncludeHouseBill = documentForwardingConsol.IncludeHouseBill;
				wrapper.fIncludeCFSName = documentForwardingConsol.IncludeCFSName;
				wrapper.fNoOfImportCargoLabelsToPrint = documentForwardingConsol.NoOfLabelsToPrint;
			}
			return wrapper;
		}

		public static DocForwardingConsol New(DocumentCommonConsol documentConsol, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingConsol wrapper = null;
			if (documentConsol != null && documentConsol.Consol != null)
			{
				wrapper = New((ForwardingConsol)documentConsol.Consol, factoryToWrap);
				wrapper.SetFromDocumentCommonConsol(documentConsol);
			}
			return wrapper;
		}

		public static DocForwardingConsol New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<ForwardingConsol>(pK), factory);
		}

		public static DocForwardingConsol New(ForwardingConsol consol, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingConsol result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(consol, factoryToWrap);
			}
			else if (consol != null)
			{
				result = new DocForwardingConsol(consol, factoryToWrap);
			}

			return result;
		}

		public static DocForwardingConsol New(ConsolJobDocumentPrintItem jobPrintItem, BusinessObjectFactory factoryToWrap)
		{
			if (jobPrintItem == null || jobPrintItem.Consol == null)
			{
				return null;
			}
			else
			{
				DocForwardingConsol jobWrapper = DocForwardingConsol.New(jobPrintItem.Consol as ForwardingConsol, factoryToWrap);
				jobWrapper.fPrintChargeSummary = jobPrintItem.PrintChargeSummary;
				jobWrapper.fPrintChargeDetail = jobPrintItem.PrintChargeDetail;
				jobWrapper.fPrintARInvoiceAnalysis = jobPrintItem.PrintARInvoiceAnalysis;
				jobWrapper.fPrintAPInvoiceAnalysis = jobPrintItem.PrintAPInvoiceAnalysis;
				jobWrapper.fPrintJobRevenueJournalAnalysis = jobPrintItem.PrintJobRevenueJournalAnalysis;
				jobWrapper.fPrintJobByJobSummary = jobPrintItem.PrintJobByJobSummary;
				jobWrapper.fPrintContainerPackingSummary = jobPrintItem.PrintContainerPackingSummary;
				jobWrapper.isProfitLossDoc = jobPrintItem.IsProfitLossDoc;
				return jobWrapper;
			}
		}

		#region Override

		public override string ToString()
		{
			return ConsolNumber;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Name of a field")]
		public static class SDFields
		{
			public const string PlaceOfDelivery = "Place of Delivery";
			public const string PlaceOfReceipt = "Place of Receipt";
		}

		#region IAC Certification

		#region Airline Security Statement

		public ZBool IsApprovedKnownShipper
		{
			get
			{
				if (isApprovedKnownShipper == null)
				{
					isApprovedKnownShipper = new CachedProperty<ZBool>(Factory, () => Shipments.Cast<DocForwardingShipment>().All(x => x.IsApprovedKnownShipper) && Shipments.Count != 0);
				}

				return isApprovedKnownShipper.Value;
			}
		}

		CachedProperty<ZBool> isApprovedKnownShipper;

		public ZString AirlineSecurityStatement
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsApprovedKnownShipper)
				{
					result = DocumentsDataRegistry.Instance.AirlineSecurityStatementKnownShipper.Value;
				}
				else
				{
					result = DocumentsDataRegistry.Instance.AirlineSecurityStatementUnknownShipper.Value;
				}
				return result;
			}
		}

		public DocOrganisationCollection KnownOrUnknownShippers
		{
			get
			{
				var shipments = IsApprovedKnownShipper
							? Shipments.Cast<DocForwardingShipment>()
							: Shipments.Cast<DocForwardingShipment>().Where(x => !x.IsApprovedKnownShipper);

				var consignorAddresses = shipments.Select(x => x.CommonShipment.ConsignorDocumentaryAddress);
				var validAddresses = consignorAddresses.Where(x => !x.IsEmpty);

				return new DocOrganisationCollection(validAddresses, Factory);
			}
		}

		#endregion

		#endregion

		#region Import Cargo Label

		protected ZBool fIncludeHouseBill;
		public ZBool IncludeHouseBill
		{
			get
			{
				return fIncludeHouseBill;
			}
		}

		protected ZBool fIncludeCFSName;
		public ZBool IncludeCFSName
		{
			get
			{
				return fIncludeCFSName;
			}
		}

		protected ZInt fNoOfImportCargoLabelsToPrint;
		public ZInt NoOfImportCargoLabelsToPrint
		{
			get
			{
				return fNoOfImportCargoLabelsToPrint;
			}
		}

		public ZBool UseShipmentForCargoLabels
		{
			get
			{
				return (fIncludeConsignee || fIncludeConsignor || fIncludeHouseBill);
			}
		}

		#endregion

		#region Export Receival Advice Methods

		public ZString FinalDestinationForERA
		{
			get
			{
				ZString result = ZString.Empty;

				if (ConsolMode == Core.Constants.ContainerModes.FCL && Shipments.Count == 1)
				{
					result = Shipments[0].DestinationLoco != null ? Shipments[0].DestinationLoco.PortName : ZString.Empty;
				}

				if (result.IsEmpty && PortOfDischarge != null)
				{
					result = PortOfDischarge.PortName;
				}

				return result;
			}
		}

		public ZString GoodsDescriptionForERA
		{
			get
			{
				ZString result = ZString.Empty;

				if (ConsolMode == Core.Constants.ContainerModes.FCL && Shipments.Count == 1)
				{
					ZString description = Shipments[0].DescriptionForGoods;
					description = description.Replace("\n", " ");
					if (description.Length > 66)
					{
						description = description.Substring(0, 66);
					}

					result = description;
				}

				if (result.IsEmpty)
				{
					result = Res.GetString("5459763a-2d0b-42d7-8311-7bafcd38e306", "Freight All Kinds");
				}

				return result;
			}
		}

		public ZString ExportReceivalInstructionsForERA
		{
			get
			{
				ZString result = ExportReceivalInstructions;
				result = result.Replace("\n", " ");
				if (result.Length > 66)
				{
					result = result.Substring(0, 66);
				}

				return result;
			}
		}

		public ZBool HazardousShipmentForERA
		{
			get
			{
				ZBool result = ZBool.False;

				if (ConsolMode == Core.Constants.ContainerModes.FCL && Shipments.Count == 1)
				{
					result = Shipments[0].Commodity.ContainsHazardous;
				}
				else
				{
					foreach (DocForwardingShipment currentShipment in Shipments)
					{
						if (currentShipment.Commodity.ContainsHazardous)
						{
							result = ZBool.True;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZString ShipmentCustomsEntryNumber
		{
			get
			{
				if (ConsolMode == Core.Constants.ContainerModes.FCL && Shipments.Count == 1)
				{
					return Shipments[0].CustomsEntryNumber;
				}

				return ZString.Empty;
			}
		}

		public ZString ShipmentMarksAndNumbsForERA
		{
			get
			{
				ZString result = ZString.Empty;

				if (ConsolMode == Core.Constants.ContainerModes.FCL && Shipments.Count == 1)
				{
					ZString marksAndNums = Shipments[0].MarksAndNumbersLine;
					if (marksAndNums.Length > 123)
					{
						marksAndNums = marksAndNums.Substring(0, 123);
					}

					result = marksAndNums;
				}

				return result;
			}
		}

		#endregion

		#region Customs Entry Number

		public ZBool IsIceland
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland; }
		}

		public ZInt ECNCount
		{
			get
			{
				return ShipmentsWithECNs.Count;
			}
		}

		public ZString CRNECN
		{
			get
			{
				ZString result = "";
				if (AgentType == Core.Constants.AgentType.Direct)
				{
					if (DirectShipment != null && !DirectShipment.CustomsEntryNumber.IsEmpty)
					{
						result = "ECN: " + DirectShipment.CustomsEntryNumber;
					}
				}
				else
				{
					ZString customsEntryNumber = CustomsEntryNumberForExportOnly;
					if (!customsEntryNumber.IsEmpty)
					{
						result = "CRN: " + customsEntryNumber;
					}
				}

				return result;
			}
		}

		public ZString CRNBasedOnAgentType
		{
			get
			{
				ZString result = ZString.Empty;
				if (AgentType == Core.Constants.AgentType.Agent || AgentType == Core.Constants.AgentType.Charter || AgentType == Core.Constants.AgentType.Other)
				{
					result = CustomsEntryNumberForExportOnly;
				}

				return result;
			}
		}

		public ZString ShipmentPermitNumbers
		{
			get
			{
				ZString result = "";
				foreach (DocForwardingShipment shipment in Shipments)
				{
					result += shipment.CusEntryNumHeading + shipment.CustomsEntryNumber;
					result += ",   ";
				}
				result = result.TrimEndIncludingWhiteSpace(',');
				return result;
			}
		}

		public DocAddress CustomsHouse
		{
			get
			{
				if (!fCustomsHouseLoaded)
				{
					ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.IcelandCodeTypes.CustomsOfficeCode);
					query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, CustomsHouseCode);
					OrgCusCode coc = Factory.LoadTop1<OrgCusCode>(query);
					if (coc != null)
					{
						fCustomsHouse = DocAddress.New(coc.PremisesAddress ?? coc.Header.MainAddress, Factory);
					}

					fCustomsHouseLoaded = true;
				}

				return fCustomsHouse;
			}
		}

		DocAddress fCustomsHouse;
		bool fCustomsHouseLoaded;

		ZString CustomsHouseCode
		{
			get
			{
				if (!fCustomsHouseCodeLoaded)
				{
					ZString icelandCode = Core.Constants.CountryCodes.Iceland;
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
					query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, icelandCode);
					CusEntryNumber[] results = (CusEntryNumber[])Consol.Numbers.Find(query);
					fCustomsHouseCode = results.Length > 0 ? results[0].CE_EntryNum : ZString.Empty;
					fCustomsHouseCodeLoaded = true;
				}

				return fCustomsHouseCode;
			}
		}

		ZString fCustomsHouseCode;
		bool fCustomsHouseCodeLoaded;

		public ZString CRNCarrierNumberPrefix
		{
			get
			{
				return new ConsolSendingarnumerHelper(Consol as ForwardingConsol).CarrierNumberPrefixCharacter.SubstringSafe(0, 1);
			}
		}

		public ZString ConsolCRNWithSpaces
		{
			get
			{
				if (IsIceland)
				{
					Sendingarnumer sc = new Sendingarnumer(Consol.JK_CRN, " ");
					return sc.CodeWithoutCheckDigit;
				}
				else
				{
					return Consol.JK_CRN;
				}
			}
		}

		public ZString ConsolCRNWOCheckDigit
		{
			get
			{
				if (IsIceland)
				{
					Sendingarnumer sc = new Sendingarnumer(Consol.JK_CRN);
					return sc.CodeWithoutCheckDigit;
				}
				else
				{
					return Consol.JK_CRN;
				}
			}
		}

		#endregion

		#region Headings

		public ZString FreightDepotHeading
		{
			get
			{
				switch (ConsolMode)
				{
					case Core.Constants.ContainerModes.FCL:
					case Core.Constants.ContainerModes.Bulk:
					case Core.Constants.ContainerModes.Liquid:
					case Core.Constants.ContainerModes.BreakBulk:
						return Res.GetString("e83b1b54-c6ce-4534-8c3f-1a4dad748563", "WHARF");

					case Core.Constants.ContainerModes.LCL:
					case Core.Constants.ContainerModes.Other:
					default:
						return Res.GetString("16968d01-0e60-4a94-9f19-58b55f899304", "FREIGHT DEPOT");
				}
			}
		}

		public ZString ECNHeading
		{
			get
			{
				return IsExportConsol ? "ECN: " : "";
			}
		}

		public ZString OriginatingAgentHeading
		{
			get
			{
				return IsDirect ? Res.GetString("87837161-a185-42fa-a518-623dc78f748b", "CONSIGNOR") : Res.GetString("0ff80d72-046e-4d10-9d99-693f7dbcd4c1", "ORIGINATING AGENT");
			}
		}

		public ZString DestinationAgentHeading
		{
			get
			{
				return IsDirect ? Res.GetString("ff61df03-77d2-49b3-b4a4-de97759f84cb", "CONSIGNEE") : Res.GetString("af899540-690c-4de2-92de-5ed369ae10a0", "DESTINATION AGENT");
			}
		}

		public ZString SCACHeading
		{
			get
			{
				ZString result = ZString.Empty;
				if (TransportMode == Core.Constants.TransportModes.Sea && IsUSConsol)
				{
					result = "SCAC";
				}

				return result;
			}
		}

		#region Container Seals

		public ZString ContainerSealNumberHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportMode == Core.Constants.TransportModes.Air)
				{
					result = Res.GetString("e31e23ad-0388-49fa-81c5-cc5b100128b1", "Rate Class");
				}
				else if (!TransportMode.IsEmpty)
				{
					result = Res.GetString("1ef7d668-31fd-4965-95f4-dbe82098e0e3", "Seal No.");
				}

				return result;
			}
		}

		public ZBool ContainerExistsWithSeal2
		{
			get
			{
				foreach (DocContainer container in Containers)
				{
					if (!container.SealNumber2.IsEmpty)
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZBool ContainerExistsWithSeal3
		{
			get
			{
				foreach (DocContainer container in Containers)
				{
					if (!container.SealNumber3.IsEmpty)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		public ZString LastForeignPortHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportMode == Core.Constants.TransportModes.Sea && IsUSConsol)
				{
					if (!NKLastForeignPort.IsEmpty)
					{
						result = Res.GetString("afce6e74-a4cd-4f2c-b529-1e28167b3cc2", "LAST FOREIGN PORT");
					}
				}

				return result;
			}
		}

		public ZString DepartureReferenceHeading
		{
			get
			{
				return (!DepartureReference.IsEmpty) ? Res.GetString("f8be4424-c718-4c99-b197-4daa04f14a10", "DEPARTURE REFERENCE") : "";
			}
		}
		#endregion

		#region Notes

		public ZString LoadListInstructions
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.LoadListInstructions.Description, Consol);
			}
		}

		public ZString ExportReceivalInstructions
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.ExportReceivalAdviceRemarks.Description, Consol);
			}
		}

		public ZString ForwardingInstructionNotes
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.ForwardingInstructionNotes.Description, Consol);
			}
		}

		public ZString[] AllNotes
		{
			get
			{
				return GetAllNotesInStringArray(Consol);
			}
		}

		public ZString HandlingInstructions
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.HandlingInstructions.Description, Consol);
			}
		}

		public ZString CartageInstructions
		{
			get { return PickupOrDeliveryCartageInstructions(); }
		}

		public ZString DangerousGoodsHandlingInstruction
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, Consol);
			}
		}

		public ZString AgentNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.AgentNotes.Description, Consol); }
		}

		public ZString CarrierBookingRequestNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.CarrierBookingRequest.Description, Consol); }
		}

		#endregion

		#region AirwayBill Fields

		public ZString CtStatus
		{
			get { return CtStatusCore; }
		}

		protected virtual ZString CtStatusCore
		{
			get
			{
				var ctStatusesOfAllEuDeclarations = new List<ZString>();
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					var euDeclarations = (from BaseJobDeclaration d in shipment.Declarations where d.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion select d);
					if (euDeclarations.Any())
					{
						foreach (var declaration in euDeclarations)
						{
							var columnName = Enterprise.Customs.EU.Business.Declaration.JobDeclaration.Schema.ZG_CTStatusID;
							if (declaration.ZPropertyInfoHash.ContainsKey(columnName))
							{
								ZString currentCt = declaration[columnName].ToString();
								if (!currentCt.IsEmpty)
								{
									ctStatusesOfAllEuDeclarations.Add(currentCt);
								}
							}
						}
					}
					else
					{
						ZString currentCt = shipment.JS_CommunityTransitStatus;
						if (!currentCt.IsEmpty)
						{
							ctStatusesOfAllEuDeclarations.Add(currentCt);
						}
					}
				}
				var sortedList = ctStatusesOfAllEuDeclarations.ToArray().OrderBy(n => n, new CommunityTransitStatusComparer());
				return sortedList.FirstOrDefault();
			}
		}

		public ZString ConsoleRateConfirmationClosingText
		{
			get { return DocumentsDataRegistry.Instance.ConsoleRateConfirmationClosingText.Value; }
		}

		public ZString ConsoleRateConfirmationOpeningText
		{
			get { return DocumentsDataRegistry.Instance.ConsoleRateConfirmationOpeningText.Value; }
		}

		public ZString ThreeCodeMasterBillNum
		{
			get { return MasterBillNum.Length > 3 ? MasterBillNum.Substring(0, 3) : ZString.Empty; }
		}

		public ZString LastEightLetterMasterBillNum
		{
			get
			{
				ZString result = MasterBillNum.Replace(" ", "");
				result = result.Replace("-", "");

				if (result.Length > 3)
				{
					result = result.Substring(3);
				}
				else
				{
					result = "";
				}

				return result;
			}
		}

		public ZString Flight1Discharge
		{
			get
			{
				ZString result = ZString.Empty;

				if (Flight1 != null)
				{
					if (!Flight1.PortOfDischargeCode.IsEmpty)
					{
						result = Flight1.PortOfDischargeCode.Substring(2);
					}
				}

				return result;
			}
		}

		public ZString Flight2Discharge
		{
			get
			{
				ZString result = ZString.Empty;

				if (Flight2 != null)
				{
					if (!Flight2.PortOfDischargeCode.IsEmpty)
					{
						result = Flight2.PortOfDischargeCode.Substring(2);
					}
				}

				return result;
			}
		}

		public ZString Flight3Discharge
		{
			get
			{
				ZString result = "";
				if (Flight3 != null && Flight3.PortOfDischargeCode.Length > 2)
				{
					result = Flight3.PortOfDischargeCode.Substring(2);
				}
				return result;
			}
		}

		public ZString RequestedFlightDate1
		{
			get
			{
				ZString result = ZString.Empty;

				if (Flight1 != null)
				{
					if (!Flight1.VoyageFlight.IsEmpty)
					{
						result = Flight1.VoyageFlight;
					}

					if (!result.IsEmpty && !Flight1.ETD.IsEmpty)
					{
						result += "/" + Flight1.ETD.Day;
					}
				}

				return result;
			}
		}

		public ZString RequestedFlightDate2
		{
			get
			{
				ZString result = "";
				if (this.Flight2 != null)
				{
					if (!this.Flight2.VoyageFlight.IsEmpty)
					{
						result = this.Flight2.VoyageFlight;
					}

					if (!result.IsEmpty && !this.Flight2.ETD.IsEmpty)
					{
						result += "/" + this.Flight2.ETD.Day;
					}
				}
				return result;
			}
		}

		public ZString AirlineTwoLetterPrefix
		{
			get
			{
				ZString result = ZString.Empty;

				if (Flight2 != null)
				{
					if (Flight2.VoyageFlight.Length >= 2)
					{
						result = Flight2.VoyageFlight.Substring(0, 2);
					}
				}

				return result;
			}
		}

		public ZString Flight3AirlineTwoLetterPrefix
		{
			get
			{
				ZString result = ZString.Empty;

				if (Flight3 != null)
				{
					if (Flight3.VoyageFlight.Length >= 2)
					{
						result = Flight3.VoyageFlight.Substring(0, 2);
					}
				}

				return result;
			}
		}

		public ZString FirstCarrier
		{
			get { return GetCarrier(Flight1).ToUpper(); }
		}

		public ZString SecondCarrier
		{
			get { return GetCarrier(Flight2).ToUpper(); }
		}

		public ZString ThirdCarrier
		{
			get { return GetCarrier(Flight3).ToUpper(); }
		}

		public ZString CHGSCode
		{
			get { return PrepaidCollect.Length >= 2 ? PrepaidCollect.Substring(0, 2) : ZString.Empty; }
		}

		public ZString ApprovedExporterCode
		{
			get
			{
				ZString result = "";

				if (AgentType == Core.Constants.AgentType.Direct)
				{
					result = GetEXExportPermissionForConsignor();
				}
				else
				{
					if (SendingForwarder != null)
					{
						result = this.SendingForwarder.Code;
					}
				}

				return result.ToUpper();
			}
		}

		public ZString ApprovedImporterCode
		{
			get
			{
				ZString result = "";

				if (this.AgentType == Core.Constants.AgentType.Direct)
				{
					result = GetEXExportPermissionForConsignor();
				}
				else if (ReceivingForwarder != null)
				{
					result = ReceivingForwarder.Code;
				}

				return result.ToUpper();
			}
		}

		public ZString MAWBHeadingOrFirst3Chars
		{
			get
			{
				ZString result = ZString.Empty;

				if (AgentType == Constants.AgentType.Agent || AgentType == Constants.AgentType.Direct)
				{
					result = ThreeCodeMasterBillNum;
				}
				else if (IsCoLoad)
				{
					result = Res.GetString("bb5d740b-4b14-49b4-9703-b6053efd4087", "MASTER HAWB:");
				}

				return result;
			}
		}

		public ZString MAWBConsolNoOrLast8CharsOfMB
		{
			get
			{
				ZString result = ZString.Empty;

				if (AgentType == Constants.AgentType.Agent || AgentType == Constants.AgentType.Direct)
				{
					result = LastEightLetterMasterBillNum;
				}
				else if (IsCoLoad)
				{
					result = ConsolNumber;
				}

				return result;
			}
		}

		public ZString IssuingCarrierNameAndAddress
		{
			get
			{
				ZString result = "";

				if (AgentType == Core.Constants.AgentType.Agent || AgentType == Core.Constants.AgentType.Direct)
				{
					RefAirline airline = Airline;
					if (airline != null)
					{
						result =
							airline.RM_AirlineName1
							+ (airline.RM_AddressLine1 == "" ? "" : "\n" + airline.RM_AddressLine1)
							+ (airline.RM_AddressLine2 == "" ? "" : ", " + airline.RM_AddressLine2)
							+ (airline.RM_AirlineCity == "" ? "" : ", " + airline.RM_AirlineCity)
							+ (airline.RM_AirlineCountry == "" ? "" : ", " + airline.RM_AirlineCountry)
							+ (airline.RM_AirlinePostalCode == "" ? "" : ", " + airline.RM_AirlinePostalCode);
					}
				}
				else if (SendingForwarder != null)
				{
					result = SendingForwarder.PostalAddress;
				}

				return result.ToUpper();
			}
		}

		public ZString IssuingCarrierAgentName
		{
			get { return Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName.ToUpper(); }
		}

		public ZString IssuingCarrierAgentCity
		{
			get { return Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity.ToUpper(); }
		}

		public ZString IssuingCarrierAgentIATACode
		{
			get { return Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode.ToUpper(); }
		}

		public ZString IssuingCarrierAgentAccountNumber
		{
			get { return Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber.ToUpper(); }
		}

		public ZString ShipperNameAndAddress
		{
			get
			{
				ZString result = "";
				if (AgentType == Core.Constants.AgentType.Direct)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = DirectShipment.Consignor.PostalAddress;
					}
				}
				else
				{
					if (SendingForwarder != null)
					{
						result = SendingForwarder.PostalAddress;
					}
				}

				return result.ToUpper();
			}
		}

		public ZInt ShipmentsInnerPacks
		{
			get
			{
				ZInt result = 0;

				foreach (DocForwardingShipment shipment in Shipments)
				{
					result += shipment.InnerPacks;
				}

				return result;
			}
		}

		public ZDecimal ShipmentsWeight
		{
			get { return Consol.JK_TotalShipmentWeight; }
		}

		public ZString ShipmentsWeightUnit
		{
			get { return Consol.JK_TotalShipmentWeightUnit; }
		}

		#region CMR International Consignement Note

		public ZString Empty
		{
			get { return Res.GetString("9c5ea986-4046-4dfb-bcb6-28939c39896c", "DETAILS"); }
		}

		public ZString SendersInstructions
		{
			get { return Res.GetString("cbbac017-99de-437a-9662-21acc2bbe140", "SENDER'S INSTRUCTIONS"); }
		}

		public ZString ConsignorForCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = DirectShipment.Consignor.PostalAddress;
					}
				}
				else if (SendingForwarder != null)
				{
					result = SendingForwarder.PostalAddress;
				}

				return result.ToUpper();
			}
		}

		public ZString ConsigneeForCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignee != null)
					{
						result = DirectShipment.Consignee.PostalAddress;
					}
				}
				else if (ReceivingForwarder != null)
				{
					result = ReceivingForwarder.PostalAddress;
				}

				return result.ToUpper();
			}
		}

		public ZString CarrierForCMR
		{
			get
			{
				ZString result = ZString.Empty;
				if (ShippingLine != null)
				{
					result = ShippingLine.PostalAddress;
				}
				return result.ToUpper();
			}
		}

		public ZString PlaceOfDeliveryForCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignee != null)
					{
						result = GetCityStatePostCodeCountry(DirectShipment.Consignee);
					}
				}
				else if (ReceivingForwarder != null)
				{
					result = GetCityStatePostCodeCountry(ReceivingForwarder);
				}

				return result.ToUpper();
			}
		}

		public ZString PlaceDateOfTakingOverGoodsForCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = GetCityStatePostCodeCountry(DirectShipment.Consignor);
					}
				}
				else if (SendingForwarder != null)
				{
					result = GetCityStatePostCodeCountry(SendingForwarder);
				}

				return result.ToUpper();
			}
		}

		public ZString EstablishedInForCMR
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = DirectShipment.Consignor.City;
					}
				}
				else if (SendingForwarder != null)
				{
					result = SendingForwarder.City;
				}

				return result.ToUpper();
			}
		}

		public ZString OrderReferenceForCMR
		{
			get
			{
				ZString result = "";

				foreach (DocForwardingShipment shipment in Shipments)
				{
					result += (result.IsEmpty ? "" : ", ") + shipment.OrderReference;
				}

				return result.ToUpper();
			}
		}

		public ZString GoodsDescriptionForCMR
		{
			get
			{
				ZString result = "";

				foreach (DocForwardingShipment shipment in Shipments)
				{
					result += (result.IsEmpty ? "" : ", ") + shipment.GoodsDescription;
				}
				return result.ToUpper();
			}
		}

		public ZDecimal WeightInKgs
		{
			get
			{
				if (Consol.JK_TotalShipmentWeightUnit == Constants.Weight.Kilograms)
				{
					return Consol.JK_TotalShipmentWeight;
				}
				else
				{
					return Constants.Weight.ConvertSafe(Consol.JK_TotalShipmentWeight, Consol.JK_TotalShipmentWeightUnit, Constants.Weight.Kilograms);
				}
			}
		}

		public ZDecimal WeightInKgsForCMR
		{
			get { return Utilities.Round(WeightInKgs, 2); }
		}

		public ZDecimal WeightInKgsRound3
		{
			get { return Utilities.Round(WeightInKgs, 3); }
		}

		public ZDecimal VolumeInM3ForCMR
		{
			get
			{
				return Utilities.Round(Constants.Volume.ConvertSafe(Consol.JK_TotalShipmentVolume, Consol.JK_TotalShipmentVolumeUnit, Constants.Volume.CubicMetres), 2);
			}
		}

		ZString GetCityStatePostCodeCountry(DocOrganisation docOrganisation)
		{
			return (docOrganisation.City.IsEmpty ? "" : docOrganisation.City + ", ")
					+ (docOrganisation.State.IsEmpty ? "" : docOrganisation.State + " ")
					+ (docOrganisation.PostCode.IsEmpty ? "" : docOrganisation.PostCode + " ")
					+ (docOrganisation.Country != null ? docOrganisation.Country.Name : "");
		}

		#endregion

		public ZString NatureAndQuantityOfGoods
		{
			get
			{
				ZString result = "";

				if (AgentType == Constants.AgentType.Agent || IsCoLoad || AgentType == Constants.AgentType.Direct)
				{
					if (AgentType == Constants.AgentType.Direct && DirectShipment != null)
					{
						result = DirectShipment.GoodsDescription;
					}
					else if (IsCoLoad)
					{
						result = Constants.AWB.NatureAndQtyOfGoodsDetails.ConsolAsPerList;
					}

					foreach (DocContainer container in Containers)
					{
						if (result == "")
						{
							result += container.ContainerNumber;
						}
						else
						{
							result += "\n" + container.ContainerNumber;
						}
					}
				}

				return result.ToUpper();
			}
		}

		public ZString OriginPortCode3Char
		{
			get { return PortOfLoading != null ? PortOfLoading.Code.Substring(2) : ZString.Empty; }
		}

		public ZString SignatureOfIssuingCarrierOrItsAgent
		{
			get { return GlbStaff.CurrentUser.GS_FullName + " " + GlbStaff.CurrentUser.DangerousGoodsCertificateNumber; }
		}

		public ZString CurrentBranchCity
		{
			get { return GlbBranch.CurrentBranch.GB_City; }
		}

		public ZString SignatureOfShipperOrHisAgent
		{
			get { return IssuingCarrierAgentName; }
		}

		public ZDecimal ShipmentsGoodsValue
		{
			get
			{
				ZDecimal result = 0;

				foreach (DocForwardingShipment shipment in Shipments)
				{
					result += shipment.GoodsValue;
				}

				return result;
			}
		}

		public ZString ConsolCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZDecimal ChargeableWeight
		{
			get { return Consol.JK_ConsolChargeable.IsEmpty ? ShipmentsWeight : Consol.JK_ConsolChargeable; }
		}

		public ZString Destination
		{
			get { return (PortOfDischarge != null && PortOfDischarge.Code.Length > 2) ? PortOfDischarge.Code.Substring(2) : ZString.Empty; }
		}

		public ZString DestinationPortName
		{
			get { return (PortOfDischarge != null && PortOfDischarge.Code.Length > 0) ? PortOfDischarge.PortName : ZString.Empty; }
		}

		RefAirline Airline
		{
			get { return ThreeCodeMasterBillNum.Length == 3 ? RefAirline.LoadFromAirlinePrefix(Factory, ThreeCodeMasterBillNum) : null; }
		}

		public ZString AirlineName
		{
			get
			{
				RefAirline airline = Airline;
				return airline != null ? airline.RM_AirlineName1.ToUpper() : ZString.Empty;
			}
		}

		public ZString DangerousGoodsStatement
		{
			get { return DocumentsDataRegistry.Instance.DangerousGoodsStatement.Value; }
		}

		public ZBool HasDangerousGoods
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					foreach (PackLine line in shipment.OuterPackLines)
					{
						foreach (UNDGDataItem dgItem in line.UNDGs)
						{
							if (dgItem.Substance != null)
							{
								result = ZBool.True;
								break;
							}
						}
					}
				}
				return result;
			}
		}

		public DocOuterPackCollection CargoLabelOuterPacks
		{
			get
			{
				ZInt totalPacks = 0;
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					totalPacks += shipment.JS_OuterPacks;
				}
				if (totalPacks == fNoOfImportCargoLabelsToPrint || fNoOfImportCargoLabelsToPrint == 0)
				{
					return OuterPacks;
				}
				else
				{
					DocOuterPackCollection result = new DocOuterPackCollection(Factory);
					ZInt pieceCount = 0;
					for (ZInt i = 0; i < fNoOfImportCargoLabelsToPrint; i++)
					{
						OuterPack outerPack = new OuterPack();
						outerPack.Number = ++pieceCount;
						result.Add(DocOuterPack.New(outerPack, Factory));
					}
					return result;
				}
			}
		}

		public ZString DGContacts
		{
			get
			{
				ZString result = ZString.Empty;
				ZString contactInfo;
				OrgContact contact;
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.Consignor != null)
					{
						contact = Factory.Load<OrgContact>(shipment.Consignor.MiscServ.OM_OC_EXDefaultDGContact);
						if (contact != null)
						{
							contactInfo = contact.OC_ContactName + " " + shipment.Consignor.MiscServ.DGPhoneNumber;
							if (!result.Contains(contactInfo))
							{
								result += (result.IsEmpty ? "" : ", ");
								result += contactInfo;
							}
						}
					}
				}
				return result;
			}
		}

		public DocOuterPackCollection OuterPacks
		{
			get
			{
				DocOuterPackCollection result = new DocOuterPackCollection(Factory);
				ZInt pieceCount = 0;
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					for (int i = 0; i < shipment.JS_OuterPacks; i++)
					{
						OuterPack outerPack = new OuterPack();
						outerPack.DocShipment = DocForwardingShipment.New(shipment, Factory);
						outerPack.DocShipment.SequenceNumber = i + 1;
						outerPack.Number = ++pieceCount;
						result.Add(DocOuterPack.New(outerPack, Factory));
					}
				}

				return result;
			}
		}

		public DocPackLinesCollection ConsolOuterPacks
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(Factory);
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					foreach (ForwardingPackLine packLine in shipment.OuterPackLines)
					{
						result.Add(DocPackLines.New(packLine, Factory));
					}
				}
				return result;
			}
		}

		public ZInt TotalConsolPackCount
		{
			get
			{
				ZInt result = ZInt.Zero;
				foreach (DocPackLines docLine in ConsolOuterPacks)
				{
					result += docLine.PackageCount;
				}
				return result;
			}
		}

		public ZDecimal TotalActualWeight
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocPackLines docLine in ConsolOuterPacks)
				{
					result += docLine.ActualWeightLB;
				}
				return result;
			}
		}

		#endregion

		#region Delivery Agent

		DeliveryAgentOrgHeader fDeliveryAgent;

		public void SetDeliveryAgent(DeliveryAgentOrgHeader deliveryAgent)
		{
			fDeliveryAgent = deliveryAgent;
		}

		public DocAddress DeliveryAgentAddress
		{
			get
			{
				OrgAddress deliveryAgentAddress = (fDeliveryAgent == null) ? null : fDeliveryAgent.MainAddress;
				return DocAddress.New(deliveryAgentAddress, Factory);
			}
		}

		public DocContainerCollection DeliveryAgentContainers
		{
			get
			{
				DocContainerCollection result = new DocContainerCollection(Consol.Factory);
				ArrayList containerForDeliveryAgentList = new ArrayList();
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.JS_IsForwardRegistered && fDeliveryAgent != null && shipment.DeliveryAgent != null
						&& shipment.DeliveryAgent.PK.Equals(fDeliveryAgent.PK)
						&& !shipment.DeliveryAgent.PK.Equals(Consol.ReceivingForwarderPK))
					{
						foreach (PackLine packLine in shipment.OuterPackLines)
						{
							var container = packLine.GetContainer(Consol);
							if (container != null && !containerForDeliveryAgentList.Contains(container))
							{
								containerForDeliveryAgentList.Add(container);
							}
						}
					}
				}
				foreach (CommonContainer container in containerForDeliveryAgentList)
				{
					result.Add(DocContainer.New(container, Factory));
				}
				result.Sort("ContainerNumber", ListSortDirection.Ascending);
				return result;
			}
		}

		#endregion

		#region Collection

		public DocForwardingShipmentCollection ForwardRegisteredShipments
		{
			get
			{
				DocForwardingShipmentCollection shipments = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (shipment.JS_IsForwardRegistered)
					{
						if (fDeliveryAgent == null ||
							shipment.DeliveryAgent == null && fDeliveryAgent.PK.Equals(Consol.ReceivingForwarderPK) ||
							shipment.DeliveryAgent != null && shipment.DeliveryAgent.PK.Equals(fDeliveryAgent.PK) && !shipment.DeliveryAgent.PK.Equals(Consol.ReceivingForwarderPK))
						{
							DocForwardingShipment shipmentWrapper = GetNewDocShipment(shipment);
							shipmentWrapper.CurrentConsol = this;
							shipments.Add(shipmentWrapper);
						}
					}
				}
				return shipments;
			}
		}

		public DocForwardingShipmentCollection Shipments
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment shipmentWrapper in ForwardRegisteredShipments)
				{
					shipmentWrapper.IsExportConsol = IsExportConsol;
					shipmentWrapper.IsUSConsol = IsUSConsol;
					shipmentWrapper.IsSeaConsol = TransportMode == Core.Constants.TransportModes.Sea;
					shipmentCollection.Add(shipmentWrapper);
				}

				shipmentCollection.SortOnHBL();
				return shipmentCollection;
			}
		}

		public DocForwardingShipmentCollection ShipmentsWithECNs
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
				{
					if (shipment.CustomsEntryNumberType == CusEntryNumberTypes.Australia.ECN && !shipment.CustomsEntryNumber.IsEmpty)
					{
						shipmentCollection.Add(shipment);
					}
				}

				return shipmentCollection;
			}
		}

		protected DocForwardingShipmentCollection fMasterAndSubShipments;
		public DocForwardingShipmentCollection MasterAndSubShipments
		{
			get
			{
				if (fMasterAndSubShipments == null)
				{
					fMasterAndSubShipments = new DocForwardingShipmentCollection(Consol.Factory);

					foreach (DocForwardingShipment shipment in MasterShipments)
					{
						fMasterAndSubShipments.AddRange(CreateSubShipmentsForMaster(shipment));
					}
				}

				return fMasterAndSubShipments;
			}
		}

		DocForwardingShipmentCollection CreateSubShipmentsForMaster(DocForwardingShipment master)
		{
			DocForwardingShipmentCollection result = new DocForwardingShipmentCollection(Consol.Factory);

			master.CurrentConsol = this;
			result.Add(master);

			foreach (DocForwardingShipment shipment in master.ColoadShipments)
			{
				result.AddRange(CreateSubShipmentsForMaster(shipment));
			}

			return result;
		}

		public DocForwardingShipmentCollection MasterShipments
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
				{
					if (shipment.ColoadMasterShipment == null)
					{
						shipmentCollection.Add(shipment);
					}
				}
				if (SortShipmentsOnHBL == 1)
				{
					shipmentCollection.SortOnHBL();
				}
				else
				{
					shipmentCollection.SortOnShipmentNumber();
				}
				return shipmentCollection;
			}
		}

		public DocForwardingShipmentCollection SubShipments
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment masterAndSubShipment in MasterAndSubShipments)
				{
					if (masterAndSubShipment.ColoadMasterShipment != null && masterAndSubShipment.ColoadShipments.Count == 0)
					{
						shipmentCollection.Add(masterAndSubShipment);
					}
				}
				return shipmentCollection;
			}
		}

		public DocForwardingShipmentCollection UltimateShipments
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
				{
					if (shipment.ColoadShipments.Count == 0)
					{
						shipmentCollection.Add(shipment);
					}
				}

				shipmentCollection.SortOnHBL();

				return shipmentCollection;
			}
		}

		public ZString TotalPrepaidForPrintOnManifest
		{
			get { return totalPrepaidForPrintOnManifest ?? (totalPrepaidForPrintOnManifest = GetTotalPrepaidForPrintOnManifest()); }
		}
		string totalPrepaidForPrintOnManifest;

		string GetTotalPrepaidForPrintOnManifest()
		{
			ZDecimal totalInLocalCurrency = 0m;

			foreach (DocForwardingShipment shipment in ShipmentsForPrintOnManifest)
			{
				if (shipment.JobHeader != null)
				{
					foreach (DocJobCharge charge in shipment.JobHeader.JobChargesForLocalClient)
					{
						totalInLocalCurrency += charge.LocalSellAmount;
					}
				}
			}

			if (GlbBranch.CurrentBranch != null && GlbBranch.CurrentBranch.Country != null && GlbBranch.CurrentBranch.Country.LocalCurrency != null)
			{
				DocCurrency currency = DocCurrency.New(Factory, GlbBranch.CurrentBranch.Country.LocalCurrency);
				return currency.FormatMoney(totalInLocalCurrency);
			}
			else
			{
				return totalInLocalCurrency.ToString(2);
			}
		}

		public ZString TotalCollectForPrintOnManifest
		{
			get { return totalCollectForPrintOnManifest ?? (totalCollectForPrintOnManifest = GetTotalCollectForPrintOnManifest()); }
		}
		string totalCollectForPrintOnManifest;

		string GetTotalCollectForPrintOnManifest()
		{
			Dictionary<DocCurrency, ZDecimal> lookup = new Dictionary<DocCurrency, ZDecimal>(new WrappedObjectEqualityComparer<DocCurrency>());

			foreach (DocForwardingShipment shipment in ShipmentsForPrintOnManifest)
			{
				if (shipment.JobHeader != null)
				{
					foreach (DocJobCharge charge in shipment.JobHeader.JobChargesForAgentCollect)
					{
						if (charge.OSSellAmt != 0 && charge.OSSellCurrency != null)
						{
							ZDecimal existing;

							if (lookup.TryGetValue(charge.OSSellCurrency, out existing))
							{
								lookup[charge.OSSellCurrency] = existing + charge.OSSellAmt;
							}
							else
							{
								lookup.Add(charge.OSSellCurrency, charge.OSSellAmt);
							}
						}
					}
				}
			}

			List<string> result = new List<String>(lookup.Count);

			foreach (KeyValuePair<DocCurrency, ZDecimal> pair in lookup)
			{
				result.Add(pair.Key.FormatMoney(pair.Value));
			}

			return string.Join("\n", result.ToArray());
		}

		public DocForwardingShipmentCollection ShipmentsForPrintOnManifest
		{
			get
			{
				if (!PrintColoadOnManifest && PrintMastersOnManifest)
				{
					return MasterShipments;
				}
				else if (PrintColoadOnManifest && !PrintMastersOnManifest)
				{
					return UltimateShipments;
				}
				else
				{
					return MasterAndSubShipments;
				}
			}
		}

		public DocForwardingShipmentCollection ShipmentsForManifestTotalCalculations
		{
			get
			{
				return (PrintColoadOnManifest && !PrintMastersOnManifest) ? UltimateShipments : MasterShipments;
			}
		}

		public DocForwardingShipmentCollection ShipmentsForPrintOnOtherDocs
		{
			get
			{
				if (!PrintColoadOnOtherDocs && PrintMastersOnOtherDocs)
				{
					return MasterShipments;
				}
				else if (PrintColoadOnOtherDocs && !PrintMastersOnOtherDocs)
				{
					return UltimateShipments;
				}
				else
				{
					return MasterAndSubShipments;
				}
			}
		}

		public DocForwardingShipmentCollection ShipmentsForTranshipmentList
		{
			get
			{
				DocForwardingShipmentCollection shipmentCollection = new DocForwardingShipmentCollection(Consol.Factory);
				foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
				{
					if (ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKDischargePort) && shipment.DestinationLoco != null && shipment.DestinationLoco.Code != PortOfDischarge.Code)
					{
						shipmentCollection.Add(shipment);
					}
					else if (ImportExportHelper.IsBranchCountry(Consol.JK_RL_NKLoadPort) && shipment.OriginLoco != null && shipment.OriginLoco.Code != PortOfLoading.Code)
					{
						shipmentCollection.Add(shipment);
					}
				}
				return shipmentCollection;
			}
		}

		public DocPackLinesCollection PackLinesForManifest
		{
			get
			{
				return PackLinesOnConsol(true, false, ShipmentsForPrintOnManifest, true);
			}
		}

		public DocPackLinesCollection PackLines
		{
			get
			{
				return PackLinesOnConsol(true, false, null, true);
			}
		}

		internal DocPackLinesCollection PackLinesOnConsol(ZBool includePacked, ZBool includeUnPacked, DocShipmentCollection shipments, bool mergeSimilarPackLines)
		{
			DocPackLinesCollection result = DocPackLinesCollection.New(Consol.Factory);

			if (shipments == null)
			{
				shipments = MasterShipments;
			}

			foreach (DocForwardingShipment shipment in shipments)
			{
				Hashtable containerNumTable = new Hashtable();
				foreach (DocPackLines line in shipment.OuterPackLineCollection)
				{
					line.CurrentConsol = this;
					if ((line.Container != null && includePacked) || (line.Container == null && includeUnPacked))
					{
						if (!mergeSimilarPackLines || !containerNumTable.ContainsKey(line.ContainerCode))
						{
							line.ContainerManifestPackType = line.PackType;
							line.ContainerManifestVolumeUnit = shipment.UnitOfVolume;
							line.ContainerManifestWeightUnit = shipment.UnitOfWeight;
							line.ContainerTotalManifestVolumeUnit = VolumeUnit;
							line.ContainerTotalManifestWeightUnit = WeightUnit;
							AddWeightVolumePackage(line, line.ActualWeight, line.ActualWeightUQ, line.ActualVolume, line.ActualVolumeUQ, line.PackageCount);
							line.ContainerManifestMarksAndNumbers = line.MarksAndNumbers;
							result.Add(line);
							if (mergeSimilarPackLines)
							{
								containerNumTable.Add(line.ContainerCode, line);
							}
						}
						else
						{
							DocPackLines existingLine = (DocPackLines)containerNumTable[line.ContainerCode];
							if (existingLine.ContainerManifestPackType != line.PackType)
							{
								existingLine.ContainerManifestPackType = Res.GetString("2922b851-f674-42f4-b93c-238161fd7177", "Package(s)");
							}
							AddWeightVolumePackage(existingLine, line.ActualWeight, line.ActualWeightUQ, line.ActualVolume, line.ActualVolumeUQ, line.PackageCount);

							if (!existingLine.ContainerManifestMarksAndNumbers.Contains(line.MarksAndNumbers))
							{
								existingLine.ContainerManifestMarksAndNumbers += System.Environment.NewLine + line.MarksAndNumbers;
							}
						}
					}
				}
			}

			return result;
		}

		public DocPackLinesCollection PackLinesForLoadList
		{
			get
			{
				DocPackLinesCollection result = DocPackLinesCollection.New(Consol.Factory);
				bool mergeSimilarPackLines = !ReportName.Trim().EndsWith((NoResString)"Detailed", System.StringComparison.OrdinalIgnoreCase);

				result.AddRange(PackLinesOnConsol(IncludeAllShipments || IncludePacked, IncludeAllShipments || IncludeUnPacked, null, mergeSimilarPackLines));

				ZString consignorAndConsignee;
				foreach (DocPackLines docPackLine in result)
				{
					if (docPackLine.Shipment != null)
					{
						consignorAndConsignee = (IncludeConsignor) ? docPackLine.Shipment.ConsignorAsString : ZString.Empty;
						consignorAndConsignee += (IncludeConsignor && IncludeConsignee) ? (ZString)System.Environment.NewLine : ZString.Empty;
						consignorAndConsignee += (IncludeConsignee) ? docPackLine.Shipment.ConsigneeAsString : ZString.Empty;
						docPackLine.ConsignorAndConsigneeForLoadList = consignorAndConsignee;
						if (IncludeCustomsBroker)
						{
							if (IsImportDocument)
							{
								docPackLine.CustomsBrokerForLoadList = (docPackLine.Shipment.ImportBroker != null) ? Res.GetString("8307c577-d7ee-4ab9-8b04-715983f11f4a", "Broker: {0}", docPackLine.Shipment.ImportBroker.Name) : "";
							}
							else
							{
								docPackLine.CustomsBrokerForLoadList = (docPackLine.Shipment.ExportBroker != null) ? Res.GetString("8c1fe02f-e38b-4766-9ee7-e6be13ab9fbf", "Broker: {0}", docPackLine.Shipment.ExportBroker.Name) : "";
							}
						}
					}
				}

				if (!mergeSimilarPackLines)
				{
					result.Sort("ContainerPackingOrder", ListSortDirection.Ascending);
				}

				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Report Name")]
		public DocTranshipmentCollection Transhipments
		{
			get
			{
				DocTranshipmentCollection coll = new DocTranshipmentCollection(Consol.Factory);
				DocForwardingShipmentCollection shipments = null;

				if (ReportName.Contains((NoResString)"Manifest"))
				{
					shipments = ShipmentsForPrintOnManifest;
				}
				else if (ReportName == "Transhipment List")
				{
					shipments = ShipmentsForTranshipmentList;
				}
				else
				{
					shipments = ShipmentsForPrintOnOtherDocs;
				}
				if (shipments != null)
				{
					foreach (DocForwardingShipment shipment in shipments)
					{
						coll.Add(new DocTranshipment(this, shipment));
					}
				}
				return coll;
			}
		}
		#endregion

		#region DocOrgs

		public DocOrganisation InterimReceiptConsignor
		{
			get { return ConsignorOrg; }
		}

		public DocOrganisation InterimReceiptConsignee
		{
			get { return ConsigneeOrg; }
		}

		public DocOrganisation ArrivalTransportCompany
		{
			get { return DocOrganisation.New(Consol.ArrivalUnpackCFSTransportAddress, Factory); }
		}

		public DocOrganisation DepartureTransportCompany
		{
			get { return DocOrganisation.New(Consol.DeparturePackCFSTransportAddress, Factory); }
		}

		#endregion

		#region DocDocAddress

		public DocDocAddress FreightDepot
		{
			get
			{
				switch (this.ConsolMode)
				{
					case Core.Constants.ContainerModes.FCL:
					case Core.Constants.ContainerModes.Bulk:
					case Core.Constants.ContainerModes.Liquid:
					case Core.Constants.ContainerModes.BreakBulk:
						return (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP)) ? this.DepartureCTOAddress : this.ArrivalCTOAddress;

					case Core.Constants.ContainerModes.LCL:
					case Core.Constants.ContainerModes.Other:
					default:
						return (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP)) ? this.PackDepotAddress : this.UnpackDepotAddress;
				}
			}
		}

		#endregion

		#region Weight Volume Chargeables

		public ZDecimal TotalShipmentWeight //total actual weight
		{
			get
			{
				return Consol.JK_TotalShipmentWeight;
			}
		}
		public ZDecimal TotalDocumentedWeight
		{
			get
			{
				return Consol.JK_TotalDocumentedWeight;
			}
		}
		public ZDecimal TotalManifestedWeight
		{
			get
			{
				return Consol.JK_TotalManifestedWeight;
			}
		}

		public ZDecimal TotalShipmentVolume  //total actual volume
		{
			get
			{
				return Consol.JK_TotalShipmentVolume;
			}
		}
		public ZDecimal TotalDocumentedVolume
		{
			get
			{
				return Consol.JK_TotalDocumentedVolume;
			}
		}
		public ZDecimal TotalManifestedVolume
		{
			get
			{
				return Consol.JK_TotalManifestedVolume;
			}
		}

		public ZDecimal TotalActualChargeable
		{
			get
			{
				return Consol.JK_ConsolChargeable;
			}
		}
		public ZDecimal TotalDocumentedChargeable
		{
			get
			{
				return Consol.JK_TotalDocumentedChargeable;
			}
		}
		public ZDecimal TotalManifestedChargeable
		{
			get
			{
				return Consol.JK_TotalManifestedChargeable;
			}
		}

		public ZString ManifestTotalShipmentWeight
		{
			get
			{
				ZString units = ShipmentsForManifestTotalCalculations.Count > 0 ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms;

				foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
				{
					if (!Constants.Weight.IsImperial(shipment.WeightUnit))
					{
						units = Core.Constants.Weight.Kilograms;
						break;
					}
				}

				ZDecimal result = 0.00M;
				foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
				{
					ZDecimal weight;
					ZDecimal.TryParse(shipment.Weight, out weight);
					weight = Core.Constants.Weight.ConvertSafe(weight, shipment.WeightUnit, units);
					result += weight;
				}
				return FormatNumber(result) + " " + units;
			}
		}

		public ZDecimal LoadListTotalPackLineWeight
		{
			get
			{
				string unit = LoadListTotalPackLineWeightUnit;

				ZDecimal result = 0m;
				foreach (DocPackLines packline in PackLinesForLoadList)
				{
					result += Constants.Weight.ConvertSafe(packline.ContainerManifestWeight, packline.ContainerManifestWeightUnit, unit);
				}

				return result;
			}
		}

		public ZString LoadListTotalPackLineWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZString ManifestTotalShipmentVolume
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Air &&
					DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP) &&
					!Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir)
				{
					return ZString.Empty;
				}
				else
				{
					ZString units = ShipmentsForManifestTotalCalculations.Count > 0 ? Core.Constants.Volume.CubicFeet : Core.Constants.Volume.CubicMetres;

					foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
					{
						if (!Constants.Volume.IsImperial(shipment.VolumeUnit))
						{
							units = Core.Constants.Volume.CubicMetres;
							break;
						}
					}

					ZDecimal result = 0.00M;
					foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
					{
						ZDecimal volume;
						ZDecimal.TryParse(shipment.Volume, out volume);
						volume = Core.Constants.Volume.ConvertSafe(volume, shipment.VolumeUnit, units);
						result += volume;
					}
					return FormatNumber(result) + " " + units;
				}
			}
		}

		public ZDecimal LoadListTotalPackLineVolume
		{
			get
			{
				string unit = LoadListTotalPackLineVolumeUnit;

				ZDecimal result = 0m;
				foreach (DocPackLines packline in PackLinesForLoadList)
				{
					result += Constants.Volume.ConvertSafe(packline.ContainerManifestVolume, packline.ContainerManifestVolumeUnit, unit);
				}

				return result;
			}
		}

		public ZString LoadListTotalPackLineVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		public ZString ManifestTotalShipmentChargeable
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Air &&
					DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP) &&
					!Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenAir)
				{
					return ZString.Empty;
				}
				else if (TransportMode == Core.Constants.TransportModes.Sea &&
					DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP) &&
					!Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenSea)
				{
					return ZString.Empty;
				}
				else
				{
					ZDecimal result = 0.00M;
					foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
					{
						ZDecimal chargeable;
						ZDecimal.TryParse(shipment.Chargeable, out chargeable);
						if (IsAir)
						{
							chargeable = Core.Constants.Weight.ConvertSafe(chargeable, shipment.ChargeableUnit, Core.Constants.Weight.Kilograms);
						}
						else
						{
							chargeable = Core.Constants.Volume.ConvertSafe(chargeable, shipment.ChargeableUnit, Core.Constants.Volume.CubicMetres);
						}

						result += chargeable;
					}
					return FormatNumber(result) + " " + ChargeableUnit;
				}
			}
		}

		public ZInt ManifestTotalShipmentPackages
		{
			get
			{
				ZInt result = 0;
				foreach (DocForwardingShipment shipment in ShipmentsForManifestTotalCalculations)
				{
					result += shipment.OuterPacks;
				}
				return result;
			}
		}

		public ZString ExportManifestTotalVolume
		{
			get
			{
				ZString result = TotalVolume + " " + VolumeUnit;
				if (TransportMode == Core.Constants.TransportModes.Air && DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					return (Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir) ? result : ZString.Empty;
				}
				else
				{
					return result;
				}
			}
		}

		public ZString ExportManifestTotalChargeable
		{
			get
			{
				ZString result = TotalChargeable + " " + ChargeableUnit;
				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					if (TransportMode == Core.Constants.TransportModes.Air)
					{
						return (Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenAir) ? result : ZString.Empty;
					}
					else if (TransportMode == Core.Constants.TransportModes.Sea)
					{
						return (Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenSea) ? result : ZString.Empty;
					}
					else
					{
						return result;
					}
				}
				else
				{
					return result;
				}
			}
		}

		public ZString ExportManifestVolumeHeading
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Air && DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					return (Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir) ? Res.GetString("72c2f977-837e-4455-ba58-8ea44d03b23f", "VOLUME") : "";
				}
				else
				{
					return Res.GetString("d65a61ab-20db-4477-9ed2-178677bc36a9", "VOLUME");
				}
			}
		}

		public ZString ExportManifestChargeableHeading
		{
			get
			{
				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					if (TransportMode == Core.Constants.TransportModes.Air)
					{
						return (Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenAir) ? Res.GetString("d99688ca-7139-4540-84bc-a0a7e3fde266", "CHARGEABLE") : "";
					}
					else if (TransportMode == Core.Constants.TransportModes.Sea)
					{
						return (Env.Registry.ConsolManifestConsolExportDisplayChargeableWhenSea) ? Res.GetString("b34d8f2f-b0e6-430d-9eb6-b45787f763f9", "CHARGEABLE") : "";
					}
					else
					{
						return Res.GetString("d07c8ab4-8a2c-4a42-a19d-9b097f6d36ab", "CHARGEABLE");
					}
				}
				else
				{
					return Res.GetString("5f00f37a-506b-4dd3-9044-c1c85844e58f", "CHARGEABLE");
				}
			}
		}

		#endregion

		#region Forwarding Instruction

		public ZString ConsignorName
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = DirectShipment.Consignor.Name;
					}
				}
				else if (ConsignorOrg != null)
				{
					result = ConsignorOrg.Name;
				}
				return result;
			}
		}

		public ZString ConsignorPhoneNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignor != null)
					{
						result = DirectShipment.Consignor.Phone;
					}
				}
				else if (ConsignorOrg != null)
				{
					result = ConsignorOrg.Phone;
				}
				return result;
			}
		}

		public ZString Consignor
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote((IStmNoteParent)DirectShipment.WrappedObject);
						string temp = docNote.GetFieldValueAsString((NoResString)"Consignor - Shipper");
						if (!string.IsNullOrEmpty(temp))
						{
							result = temp;
						}
						else if (DirectShipment.Consignor != null)
						{
							result = DirectShipment.Consignor.SelectedAddress.ToString();
						}
					}
				}
				else if (ConsignorOrg != null)
				{
					result = ConsignorOrg.SelectedAddress.ToString();
				}
				return result;
			}
		}

		public ZString ConsigneeName
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignee != null)
					{
						result = DirectShipment.Consignee.Name;
					}
				}
				else if (ConsigneeOrg != null)
				{
					result = ConsigneeOrg.Name;
				}

				return result;
			}
		}

		public ZString ConsigneePhoneNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null && DirectShipment.Consignee != null)
					{
						result = DirectShipment.Consignee.Phone;
					}
				}
				else if (ConsigneeOrg != null)
				{
					result = ConsigneeOrg.Phone;
				}

				return result;
			}
		}

		public ZString Consignee
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote((IStmNoteParent)DirectShipment.WrappedObject);
						string temp = docNote.GetFieldValueAsString((NoResString)"Consignee");
						if (!string.IsNullOrEmpty(temp))
						{
							result = temp;
						}
						else if (DirectShipment.Consignee != null)
						{
							result = DirectShipment.Consignee.SelectedAddress.ToString();
						}
					}
				}
				else if (ConsigneeOrg != null)
				{
					result = ConsigneeOrg.SelectedAddress.ToString();
				}

				return result;
			}
		}

		public ZString Carrier
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsCoLoad && Creditor != null)
				{
					result = Creditor.PostalAddress;
				}
				else if (ShippingLine != null)
				{
					result = ShippingLine.SelectedAddress.ToString();
				}

				return result;
			}
		}

		public ZString CarrierPhoneNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsCoLoad && Creditor != null)
				{
					result = Creditor.Phone;
				}
				else if (ShippingLine != null)
				{
					result = ShippingLine.Phone;
				}

				return result;
			}
		}

		public ZString CarrierName
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsCoLoad && Creditor != null)
				{
					result = Creditor.Name;
				}
				else if (ShippingLine != null)
				{
					result = ShippingLine.Name;
				}

				return result;
			}
		}

		public ZString DirectShipmentNotifyPartyUDF
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsDirect && DirectShipment != null)
				{
					result = DefaultToBillOfLading ? DirectShipment.BillOfLading.NotifyParty : UserDefinedNotifyParty;
				}
				return result;
			}
		}

		public ZString ReceivingForwarderNotifyParty
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol.ReceivingForwarder != null)
				{
					OrgContact contact = new DefaultContactFinder(Consol.ReceivingForwarder, false).DefaultContact(ContactType.NotifyParty);
					if (contact != null)
					{
						DocContacts docContact = DocContacts.New(contact, Factory);
						result = docContact.PostalAddress;
						notifyPartyPhone = docContact.Phone;
						notifyPartyFax = docContact.Fax;
					}

					if (result.IsEmpty)
					{
						result = ReceivingForwarder.PostalAddress;
						notifyPartyPhone = ReceivingForwarder.Phone;
						notifyPartyFax = ReceivingForwarder.Fax;
					}
				}

				notifyPartyPhoneSet = true;
				notifyPartyFaxSet = true;
				return result;
			}
		}

		public ZString DirectShipmentNotifyPartyPhone
		{
			get
			{
				if (DefaultToBillOfLading)
				{
					if (DirectShipment.BillOfLading.ShipmentWrapper.NotifyParty != null)
					{
						return DirectShipment.BillOfLading.ShipmentWrapper.NotifyParty.Phone;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString DirectShipmentNotifyPartyFax
		{
			get
			{
				if (DefaultToBillOfLading)
				{
					if (DirectShipment.BillOfLading.ShipmentWrapper.NotifyParty != null)
					{
						return DirectShipment.BillOfLading.ShipmentWrapper.NotifyParty.Fax;
					}
				}

				return ZString.Empty;
			}
		}

		public ZString NotifyPartyAddress
		{
			get
			{
				ZString result = ZString.Empty;
				JobDocAddress notifyParty = ((ForwardingConsol)Consol).NotifyPartyDocumentaryAddress;
				if (notifyParty != null)
				{
					DocDocAddress addr = DocDocAddress.New(notifyParty, Factory);
					result = addr.PostalAddress;
					notifyPartyPhone = addr.Phone;
					notifyPartyFax = addr.Fax;
				}

				if (result.IsEmpty)
				{
					result = ReceivingForwarderNotifyParty;
				}

				notifyPartyPhoneSet = true;
				notifyPartyFaxSet = true;
				return result;
			}
		}

		public ZString NotifyPartyPhone
		{
			get
			{
				if (notifyPartyPhoneSet)
				{
					return notifyPartyPhone;
				}

				ZString hitAddress = NotifyPartyAddress;
				return notifyPartyPhone;
			}
		}

		ZString notifyPartyPhone;
		ZBool notifyPartyPhoneSet;

		public ZString NotifyPartyFax
		{
			get
			{
				if (notifyPartyFaxSet)
				{
					return notifyPartyFax;
				}

				ZString hitAddress = NotifyPartyAddress;
				return notifyPartyFax;
			}
		}
		ZString notifyPartyFax;
		ZBool notifyPartyFaxSet;

		ZString UserDefinedNotifyParty
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsDirect && DirectShipment != null)
				{
					DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote((IStmNoteParent)DirectShipment.WrappedObject);
					result = docNote.GetFieldValueAsString((NoResString)"Notify Party");
				}
				return result;
			}
		}

		bool DefaultToBillOfLading
		{
			get { return IsDirect && DirectShipment != null && string.IsNullOrEmpty(UserDefinedNotifyParty) && DirectShipment.BillOfLading != null; }
		}

		public ZString ShippersReference
		{
			get
			{
				return (IsDirect && DirectShipment != null) ? DirectShipment.ShippersReference : ZString.Empty;
			}
		}

		public ZString ExportPermits
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect && DirectShipment != null)
				{
					result = DirectShipment.ExportPermit + " ";
				}

				result += CustomsEntryNumberForExportOnly;

				return result.TrimEnd();
			}
		}

		public ZString FreightTerms
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						result = DirectShipment.PaymentTermDisplay.ToUpper();
					}
				}
				else if (!PrepaidCollect.IsEmpty)
				{
					result = PrepaidCollectDescription;
				}

				return result;
			}
		}

		public ZString FreightPayableAt
		{
			get
			{
				ZString result = "";

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						result = DirectShipment.FreightPayableAt;
						if (result.IsEmpty)
						{
							if (DirectShipment.IsCollect)
							{
								result = PlaceOfDelivery;
							}
							else if (DirectShipment.IsPrepaid)
							{
								result = PlaceOfReceipt;
							}
						}
					}
				}
				else if (IsPrepaid)
				{
					if (FirstLoadPort != null)
					{
						result = FirstLoadPort.PortName;
					}
				}
				else if (IsCollect)
				{
					if (LastDischargePort != null)
					{
						result = LastDischargePort.PortName;
					}
				}

				return result;
			}
		}

		public ZString MakeBillOutTo
		{
			get
			{
				return (IsDirect) ? Res.GetString("8ab12a26-9750-4c00-9446-05d13a67bae5", "Shipper") : Res.GetString("cd94c462-250e-4a72-99c8-6ae339b22f22", "Forwarder");
			}
		}

		public ZString AlertText
		{
			get
			{
				return DocumentsDataRegistry.Instance.OrderDelayAlertText.Value;
			}
		}

		public ZString PlaceOfReceipt
		{
			get
			{
				ZString result = GetDocDataValueOnly(SDFields.PlaceOfReceipt);
				if (result.IsEmpty)
				{
					DocUNLOCO loco = null;
					if (ForwardRegisteredShipments.Count > 0)
					{
						loco = ForwardRegisteredShipments[0].OriginLoco;
						if (loco != null)
						{
							foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
							{
								if (shipment.OriginLoco == null || shipment.OriginLoco.Code != loco.Code)
								{
									loco = null;
									break;
								}
							}
						}
					}

					if (loco != null)
					{
						result = loco.PortName;
					}
					else if (FirstLoadPort != null)
					{
						result = FirstLoadPort.PortName;
					}
				}

				return result;
			}
		}

		public ZString PlaceOfDelivery
		{
			get
			{
				ZString result = GetDocDataValueOnly(SDFields.PlaceOfDelivery);
				if (result.IsEmpty)
				{
					DocUNLOCO loco = null;
					if (ForwardRegisteredShipments.Count > 0)
					{
						loco = ForwardRegisteredShipments[0].DestinationLoco;
						if (loco != null)
						{
							foreach (DocForwardingShipment shipment in ForwardRegisteredShipments)
							{
								if (shipment.DestinationLoco == null || shipment.DestinationLoco.Code != loco.Code)
								{
									loco = null;
									break;
								}
							}
						}
					}

					if (loco != null)
					{
						result = loco.PortName;
					}
					else if (LastDischargePort != null)
					{
						result = LastDischargePort.PortName;
					}
				}

				return result;
			}
		}

		public ZString GoodsSummary
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						result = DirectShipment.GoodsSummary;
					}
				}
				else
				{
					switch (ConsolMode)
					{
						case Core.Constants.ContainerModes.FCL:
						case Core.Constants.ContainerModes.Groupage:
						case Core.Constants.ContainerModes.BuyersConsol:
							result = GetTotalContainersAndPackagesPerType();
							break;

						default:
							ZInt totalOuterPacks = 0;
							foreach (DocForwardingShipment shipmentWrapper in MasterShipments)
							{
								totalOuterPacks += shipmentWrapper.OuterPacks;
							}
							result += Res.GetString("0e57d350-dddc-4c69-9f67-07d7f8c5a625", "Total Package(s):") + " " + totalOuterPacks.ToString();
							break;
					}
				}

				return result;
			}
		}

		public ZBool ShowMarksAndNumbers
		{
			get
			{
				ZBool result =
					IsCoLoad
					||
					IsOther
					||
					((IsAgent || IsCharter) && (ConsolMode != Constants.ContainerModes.FCL && ConsolMode != Constants.ContainerModes.Groupage && ConsolMode != Constants.ContainerModes.BuyersConsol));

				return result;
			}
		}

		public ZBool ShowBlankTotals
		{
			get
			{
				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						foreach (IDocContainer container in DirectShipment.Containers)
						{
							if (!container.TotalAllocatedShipmentWeight.IsEmpty ||
								!container.TotalAllocatedShipmentVolume.IsEmpty ||
								!container.TotalAllocatedShipmentPackages.IsEmpty)
							{
								return ZBool.False;
							}
						}
					}
				}
				else
				{
					foreach (DocContainer container in Containers)
					{
						if (!container.TotalPackLineWeight.IsEmpty ||
							!container.TotalPackLineVolume.IsEmpty ||
							!container.TotalPackLinePackages.IsEmpty)
						{
							return ZBool.False;
						}
					}
				}

				return ZBool.True;
			}
		}

		public ZString PackType
		{
			get
			{
				ZString result = ZString.Empty;

				result = Res.GetString("4bf5ecc2-58aa-406b-840d-8c59ccba29c3", "PIECE(S)");
				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						result = DirectShipment.OuterPacksPackTypeDescription.ToUpper();
					}
				}
				else if (Shipments.Count == 1)
				{
					if (Shipments[0] != null)
					{
						result = Shipments[0].OuterPacksPackTypeDescription.ToUpper();
					}
				}

				return result;
			}
		}

		public DocOrganisation ConsigneeOrganisation
		{
			get
			{
				DocOrganisation result = null;

				if (IsDirect)
				{
					if (DirectShipment != null)
					{
						if (DirectShipment.Consignee != null)
						{
							result = DirectShipment.Consignee;
						}
					}
				}
				else if (ConsigneeOrg != null)
				{
					result = ConsigneeOrg;
				}

				return result;
			}
		}

		public ZString RegistrationNumber
		{
			get
			{
				ZString result = ZString.Empty;
				RefCountry brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);

				if ((ConsigneeOrganisation != null) && (ConsigneeOrganisation.Country != null))
				{
					switch (ConsigneeOrganisation.Country.Code)
					{
						case Core.Constants.CountryCodes.Brazil:
							result = ConsigneeOrganisation.CustomCodes.GetCustomsRegNoAndTypeForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, brazil);
							if (result.IsEmpty)
							{
								result = ConsigneeOrganisation.CustomCodes.GetCustomsRegNoAndTypeForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, brazil);
							}
							break;
					}
				}
				return result;
			}
		}

		public ZString ConsolSpecialInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description).Length > 0)
				{
					result = Consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description)[0].ST_NoteDataAsText;
				}

				return result;
			}
		}

		public ZBool DisplayPackingDetailsByContainer
		{
			get { return DocumentsDataRegistry.Instance.DisplayPackLinesByContainerOnConsol.Value; }
		}

		#endregion

		#region Consol Fields

		#region IsProfitLossDoc

		public ZBool IsProfitLossDoc
		{
			get { return isProfitLossDoc; }
		}

		ZBool isProfitLossDoc;

		#endregion

		#region ZString Fields

		public ZString QuotedAmount
		{
			get { return ZString.Empty; }
		}

		public ZString AuthorizedSignature
		{
			get { return ZString.Empty; }
		}

		public ZString Stop
		{
			get { return ZString.Empty; }
		}

		public ZString ShipperReference
		{
			get { return ZString.Empty; }
		}

		public ZString ConsigneeReference
		{
			get { return ZString.Empty; }
		}

		public ZString TrailerNo
		{
			get { return ZString.Empty; }
		}

		public ZString SealNo
		{
			get { return ZString.Empty; }
		}

		public ZString TractorNo
		{
			get { return ZString.Empty; }
		}

		public ZString EmergencyNo
		{
			get { return ZString.Empty; }
		}

		public ZString Context
		{
			get
			{
				return "CONSOL";
			}
		}

		public ZString DepartureReference
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Sea)
				{
					if (Sailing != null)
					{
						return Sailing.DepartureReference;
					}
				}
				return "";
			}
		}

		public ZString AgentSCACForExport
		{
			get
			{
				if (IsExportConsol)
				{
					return AgentSCAC;
				}

				return ZString.Empty;
			}
		}

		public ZString AgentSCAC
		{
			get
			{
				string sCACode = "";

				if (TransportMode == Core.Constants.TransportModes.Sea && IsUSConsol)
				{
					if (ReceivingForwarder != null)
					{
						sCACode = ReceivingForwarder.SCAC;
					}

					if (string.IsNullOrEmpty(sCACode))
					{
						sCACode = Res.GetString("6cdaa788-14e3-4d81-b500-3261d95d2f99", "*MISSING*");
					}
				}
				return sCACode;
			}
		}

		public ZString LineSCACForExport
		{
			get
			{
				if (IsExportConsol)
				{
					return LineSCAC;
				}

				return LineSCAC;
			}
		}

		public ZString LineSCAC
		{
			get
			{
				string sCACode = "";
				if (TransportMode == Core.Constants.TransportModes.Sea && IsUSConsol)
				{
					if (ShippingLine != null)
					{
						sCACode = ShippingLine.SCAC;
					}

					if (string.IsNullOrEmpty(sCACode))
					{
						sCACode = Res.GetString("04c8a3af-8fbb-4283-913f-ea220124f589", "*MISSING*");
					}
				}

				return sCACode;
			}
		}

		public ZString ContainerLine
		{
			get
			{
				ZString result = "";
				int count = 0;

				foreach (DocContainer currentContainer in Containers)
				{
					if (!currentContainer.ContainerNumber.IsEmpty)
					{
						count++;

						if (count > MaximumContainersOnALine)
						{
							result = result.TrimEndIncludingWhiteSpace(',') + " ...";
							break;
						}

						result += currentContainer.ContainerNumber + ", ";
					}
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		const int MaximumContainersOnALine = 7;

		public ZString MainVesselTransportInfo
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
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						result = GetTransportPlanningTransportDetails(Core.Constants.TransportPlanningType.MainVessel);
						break;
				}
				return result;
			}
		}

		public ZString PreCarriageTransportInfo
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
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						result = GetTransportPlanningTransportDetails(Core.Constants.TransportPlanningType.PreCarriage);
						break;
				}
				return result;
			}
		}

		public ZString OnForwardingTransportInfo
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
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
						result = GetTransportPlanningTransportDetails(Core.Constants.TransportPlanningType.OnForwarding);
						break;
				}
				return result;
			}
		}

		public ZString LastForeignPort
		{
			get
			{
				ZString result = "";

				if (TransportMode == Core.Constants.TransportModes.Sea && IsUSConsol)
				{
					if (!NKLastForeignPort.IsEmpty)
					{
						DocUNLOCO lastForeignPortLoco = DocUNLOCO.New(Factory, NKLastForeignPort);
						result = lastForeignPortLoco.Code + " = " + lastForeignPortLoco.PortName + ", " + lastForeignPortLoco.CountryName;
					}
				}

				return result;
			}
		}

		public ZString HBLNumbers
		{
			get
			{
				ZString result = "";

				foreach (DocForwardingShipment shipment in Shipments)
				{
					if (!shipment.HouseBill.IsEmpty)
					{
						result += shipment.HouseBill.ToUpper() + ", ";
					}
				}

				result = result.Trim();
				result = result.Trim(',');

				return result;
			}
		}

		public ZString PaymentType
		{
			get
			{
				ZString result = Res.GetString("0753942f-72ef-4f8a-9ee0-5aecbed26a98", "PREPAID");
				if (PrepaidCollect == Core.Constants.PaymentType.Collect)
				{
					result = Res.GetString("d62c59f6-8594-4d06-add8-d8ba5186a240", "COLLECT");
					if (ExportConsolRate != null && !TotalConsolCollectAmount.IsEmpty)
					{
						result += ": " + ExportConsolRate.Code + " " + TotalConsolCollectAmount;
					}
				}

				return result;
			}
		}

		public ZString VoyageLabel
		{
			get
			{
				return Consol.JK_Calc_VoyageLabel;
			}
		}

		public ZString AgentType
		{
			get
			{
				return Consol.JK_AgentType;
			}
		}

		public ZString ConsolStatus
		{
			get
			{
				return Consol.JK_ConsolStatus;
			}
		}

		public ZString CustomsReference
		{
			get
			{
				return Consol.JK_CustomsReference;
			}
		}

		public ZString PrintOptionForColoadsOnManifest
		{
			get
			{
				return Consol.JK_PrintOptionForColoadsOnManifest;
			}
		}

		public ZString PrintOptionForColoadsOnOtherDocs
		{
			get
			{
				return Consol.JK_PrintOptionForColoadsOnOtherDocs;
			}
		}

		public ZString ServiceLevel
		{
			get { return Consol.JK_AWBServiceLevel; }
		}

		public ZString ServiceLevelDescription
		{
			get
			{
				ZString result = "";
				if (Consol.ShippingLine != null)
				{
					result = Consol.ShippingLine.MiscServ.CarrierServiceLevels.GetDescriptionFromCode(Consol.JK_AWBServiceLevel);
				}

				return result;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocTransport Flight1
		{
			get
			{
				foreach (DocTransport currentTransport in TransportPlanning)
				{
					if (currentTransport.TransportMode == Core.Constants.TransportModes.Air && currentTransport.TransportType == Core.Constants.TransportPlanningType.Flight1)
					{
						return currentTransport;
					}
				}

				return null;
			}
		}

		public DocTransport Flight2
		{
			get
			{
				foreach (DocTransport currentTransport in this.TransportPlanning)
				{
					if (currentTransport.TransportMode == Core.Constants.TransportModes.Air && currentTransport.TransportType == Core.Constants.TransportPlanningType.Flight2)
					{
						return currentTransport;
					}
				}

				return null;
			}
		}

		public DocTransport Flight3
		{
			get
			{
				foreach (DocTransport currentTransport in this.TransportPlanning)
				{
					if (currentTransport.TransportMode == Core.Constants.TransportModes.Air && currentTransport.TransportType == Core.Constants.TransportPlanningType.Flight3)
					{
						return currentTransport;
					}
				}

				return null;
			}
		}

		public DocForwardingShipment DirectShipment
		{
			get
			{
				DocForwardingShipment result = null;
				if (IsDirect && ForwardRegisteredShipments.Count > 0)
				{
					result = ForwardRegisteredShipments[0];
				}

				return result;
			}
		}

		public DocCurrency ExportConsolRate
		{
			get
			{
				RefCurrency currency = Consol.FreightCostsCurrency ?? Factory.Load<RefCurrency>(GlbCompany.CurrentCompany.LocalCurrency.PK);
				return DocCurrency.New(currency, Factory);
			}
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime DateSigned
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime ConsoleETD
		{
			get
			{
				return Consol.MostInterestingTransportForBinding != null && Consol.MostInterestingTransportForBinding.Count > 0 ? Consol.MostInterestingTransportForBinding[0].JW_ETD : ZDateTime.Empty;
			}
		}

		public ZDateTime ConsoleETA
		{
			get
			{
				return Consol.MostInterestingTransportForBinding != null && Consol.MostInterestingTransportForBinding.Count > 0 ? Consol.MostInterestingTransportForBinding[0].JW_ETA : ZDateTime.Empty;
			}
		}

		public ZDateTime ActualDateLastForeignPort
		{
			get
			{
				return Consol.JK_DateLastForeignPort;
			}
		}

		public ZDateTime ActualDatePortOfFirstArrival
		{
			get
			{
				return Consol.JK_DatePortOfFirstArrival;
			}
		}

		public ZDateTime CutOffDate
		{
			get
			{
				return Consol.JK_ConsolCutOffDateLocal;
			}
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal ConsolChargeable
		{
			get { return Consol.JK_ConsolChargeable; }
		}

		public ZDecimal ConsolChargeableRate
		{
			get { return Consol.JK_ConsolChargeableRate; }
		}

		public ZDecimal ConsolExchangeRate
		{
			get { return Consol.FreightCostsExchangeRate; }
		}

		public ZDecimal TotalConsolCollectAmount
		{
			get { return Consol.FreightCostsAmount; }
		}

		public ZDecimal TotalGoodsValue
		{
			get { return 0; }
		}

		#endregion

		#region ZBool Fields

		public ZBool PrintColoadOnManifest
		{
			get { return (PrintOptionForColoadsOnManifest != FreightConstants.PrintOptionForCoLoads.MastersOnly); }
		}

		public ZBool PrintMastersOnManifest
		{
			get { return (PrintOptionForColoadsOnManifest != FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly); }
		}

		public ZBool PrintColoadOnOtherDocs
		{
			get { return (PrintOptionForColoadsOnOtherDocs != FreightConstants.PrintOptionForCoLoads.MastersOnly); }
		}

		public ZBool PrintMastersOnOtherDocs
		{
			get { return (PrintOptionForColoadsOnOtherDocs != FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly); }
		}

		#endregion

		#endregion

		#region Consol Job Profit Document

		#region Print Flags

		#region PrintChargeSummary

		public ZBool PrintChargeSummary
		{
			get { return fPrintChargeSummary; }
		}

		ZBool fPrintChargeSummary;

		#endregion

		#region PrintChargeDetail

		public ZBool PrintChargeDetail
		{
			get { return fPrintChargeDetail; }
		}

		ZBool fPrintChargeDetail;

		#endregion

		#region PrintARInvoiceAnalysis

		public ZBool PrintARInvoiceAnalysis
		{
			get { return fPrintARInvoiceAnalysis; }
		}

		ZBool fPrintARInvoiceAnalysis;

		#endregion

		#region PrintAPInvoiceAnalysis

		public ZBool PrintAPInvoiceAnalysis
		{
			get { return fPrintAPInvoiceAnalysis; }
		}

		ZBool fPrintAPInvoiceAnalysis;

		#endregion

		#region PrintJobRevenueJournalAnalysis

		public ZBool PrintJobRevenueJournalAnalysis
		{
			get { return fPrintJobRevenueJournalAnalysis; }
		}

		ZBool fPrintJobRevenueJournalAnalysis;

		#endregion

		public ZString LocalTaxTitle
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered ? (NoResString)"Local(Excl Tax)" : (NoResString)"Local Value"; }
		}

		#region PrintJobByJobSummary

		public ZBool PrintJobByJobSummary
		{
			get { return fPrintJobByJobSummary; }
		}

		ZBool fPrintJobByJobSummary;

		#endregion

		#region PrintContainerPackingSummary

		public ZBool PrintContainerPackingSummary
		{
			get { return fPrintContainerPackingSummary; }
		}

		ZBool fPrintContainerPackingSummary;

		#endregion

		#endregion

		#region NumberOfContainersForConsolJobProfitDocument

		public ZInt NumberOfContainersForConsolJobProfitDocument
		{
			get
			{
				ZInt result = 0;
				if (!IsAir)
				{
					foreach (DocContainer container in Containers)
					{
						if (container.ContainerMode == Core.Constants.ContainerModes.FCL ||
							container.ContainerMode == Core.Constants.ContainerModes.BuyersConsol ||
							container.ContainerMode == Core.Constants.ContainerModes.Groupage)
						{
							result++;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region TEUForConsolJobProfitDocument

		public ZDecimal TEUForConsolJobProfitDocument
		{
			get
			{
				ZDecimal result = 0M;
				if (!IsAir)
				{
					foreach (DocContainer container in Containers)
					{
						if (container.ContainerMode == Core.Constants.ContainerModes.FCL ||
							container.ContainerMode == Core.Constants.ContainerModes.BuyersConsol ||
							container.ContainerMode == Core.Constants.ContainerModes.Groupage)
						{
							result += container.TEU;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Collections

		#region Jobs
		public DocJobInvoicingJobCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = new DocJobInvoicingJobCollection(Factory);
					foreach (DocShipment shipment in Shipments)
					{
						if (shipment.JobHeader != null)
						{
							DocJobInvoicingJob job = DocJobInvoicingJob.New(Factory, ((BusinessObject)shipment.JobHeader.WrappedObject).PK, true, IsProfitLossDoc);
							if (job != null)
							{
								Jobs.Add(job);
							}
						}
					}
				}
				return fJobs;
			}
		}

		DocJobInvoicingJobCollection fJobs;
		#endregion

		#region AllLines

		public DocJobLineDetailCollection AllLines
		{
			get
			{
				if (fAllLines == null)
				{
					fAllLines = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fAllLines.AddRange(job.AllLines);
					}
				}
				return fAllLines;
			}
		}

		DocJobLineDetailCollection fAllLines;

		#endregion

		public DocProfitLossSummaryLineCollection AllProfitLossSummaryLines
		{
			get
			{
				if (fAllProfitLossSummaryLines == null)
				{
					fAllProfitLossSummaryLines = new DocProfitLossSummaryLineCollection(Factory);
					foreach (ProfitLossSummaryDetailView line in ProfitLoss.ProfitLossSummaryFilteredDetails)
					{
						fAllProfitLossSummaryLines.Add(DocProfitLossSummaryLine.New(line, Factory));
					}
				}
				return fAllProfitLossSummaryLines;
			}
		}
		DocProfitLossSummaryLineCollection fAllProfitLossSummaryLines;

		public DocProfitLossDetailedLineCollection AllProfitLossDetailedLines
		{
			get
			{
				if (fAllProfitLossDetailedLines == null)
				{
					fAllProfitLossDetailedLines = new DocProfitLossDetailedLineCollection(Factory);
					foreach (ProfitLossDetailView line in ProfitLoss.ProfitLossFilteredDetails)
					{
						fAllProfitLossDetailedLines.Add(DocProfitLossDetailedLine.New(line, Factory));
					}
				}
				return fAllProfitLossDetailedLines;
			}
		}
		DocProfitLossDetailedLineCollection fAllProfitLossDetailedLines;

		#region APExcludeJRJLines

		public DocJobLineDetailCollection APExcludeJRJLines
		{
			get
			{
				if (fAPExcludeJRJLines == null)
				{
					fAPExcludeJRJLines = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fAPExcludeJRJLines.AddRange(job.APExcludeJRJLines);
					}
				}
				return fAPExcludeJRJLines;
			}
		}

		DocJobLineDetailCollection fAPExcludeJRJLines;

		public DocJobLineDetailCollection APExcludeJRJLinesForProfitShare
		{
			get
			{
				if (fAPExcludeJRJLinesForProfitShare == null)
				{
					fAPExcludeJRJLinesForProfitShare = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fAPExcludeJRJLinesForProfitShare.AddRange(job.APExcludeJRJLinesForProfitShare);
					}
				}
				return fAPExcludeJRJLinesForProfitShare;
			}
		}

		DocJobLineDetailCollection fAPExcludeJRJLinesForProfitShare;

		#endregion

		#region ARExcludeJRJLines

		public DocJobLineDetailCollection ARExcludeJRJLines
		{
			get
			{
				if (fARExcludeJRJLines == null)
				{
					fARExcludeJRJLines = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fARExcludeJRJLines.AddRange(job.ARExcludeJRJLines);
					}
				}
				return fARExcludeJRJLines;
			}
		}

		DocJobLineDetailCollection fARExcludeJRJLines;

		public DocJobLineDetailCollection ARExcludeJRJLinesForProfitShare
		{
			get
			{
				if (fARExcludeJRJLinesForProfitShare == null)
				{
					fARExcludeJRJLinesForProfitShare = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fARExcludeJRJLinesForProfitShare.AddRange(job.ARExcludeJRJLinesForProfitShare);
					}
				}
				return fARExcludeJRJLinesForProfitShare;
			}
		}

		DocJobLineDetailCollection fARExcludeJRJLinesForProfitShare;

		#endregion

		#region DisplayNoARExcludeJRJInvoicesMessage

		public ZBool DisplayNoARExcludeJRJInvoicesMessage
		{
			get { return ARExcludeJRJLines.Count == 0; }
		}

		#endregion

		#region DisplayNoAPExcludeJRJInvoicesMessage

		public ZBool DisplayNoAPExcludeJRJInvoicesMessage
		{
			get { return APExcludeJRJLines.Count == 0; }
		}

		#endregion

		#region JRJLines

		public DocJobLineDetailCollection JRJLines
		{
			get
			{
				if (fJRJLines == null)
				{
					fJRJLines = new DocJobLineDetailCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fJRJLines.AddRange(job.JRJLines);
					}
				}
				return fJRJLines;
			}
		}

		DocJobLineDetailCollection fJRJLines;

		#endregion

		#endregion
		#region Totals

		internal JobProfitLoss ProfitLoss
		{
			get
			{
				if (profitLoss == null)
				{
					profitLoss = new JobProfitLoss(Consol.Factory);
					profitLoss.SetConsol(Consol as IJobCostingPlugIn);
					profitLoss.Filter.ShowReversedFilter = ZBool.False;
				}
				return profitLoss;
			}
		}
		JobProfitLoss profitLoss;

		#region TotalRevenue

		public ZDecimal TotalRevenue
		{
			get
			{
				return ProfitLoss.TotalRevenue;
			}
		}

		#endregion

		#region TotalWip

		public ZDecimal TotalWip
		{
			get
			{
				return ProfitLoss.TotalWIP;
			}
		}

		#endregion

		#region TotalIncome

		public ZDecimal TotalIncome
		{
			get { return TotalRevenue + TotalWip; }
		}

		#endregion

		#region TotalCost

		public ZDecimal TotalCost
		{
			get
			{
				return ProfitLoss.TotalCost * -1;
			}
		}

		#endregion

		#region TotalAccrual

		public ZDecimal TotalAccrual
		{
			get
			{
				return ProfitLoss.TotalAccrual * -1;
			}
		}

		#endregion

		#region TotalExpense

		public ZDecimal TotalExpense
		{
			get { return TotalCost + TotalAccrual; }
		}

		#endregion

		#region TotalRealisedAmount

		public ZDecimal TotalRealisedAmount
		{
			get { return TotalRevenue - TotalCost; }
		}

		#endregion

		#region TotalEstimatedAmount

		public ZDecimal TotalEstimatedAmount
		{
			get { return TotalWip - TotalAccrual; }
		}

		#endregion

		public ZDecimal TotalRevenueMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalRevenueRecognized;
			}
		}

		public ZDecimal TotalWIPMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalWIPRecognized;
			}
		}

		public ZDecimal TotalCostMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalCostRecognized * -1;
			}
		}

		public ZDecimal TotalACRMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalAccrualRecognized * -1;
			}
		}

		public ZDecimal TotalJobProfitRecognizedInGL
		{
			get
			{
				return TotalRevenueMovementsRecognized + TotalWIPMovementsRecognized - TotalCostMovementsRecognized - TotalACRMovementsRecognized;
			}
		}

		public ZDecimal RevenueNotRecognized
		{
			get
			{
				return ProfitLoss.TotalRevenueNotRecognized;
			}
		}

		public ZDecimal CostNotRecognized
		{
			get
			{
				return ProfitLoss.TotalCostNotRecognized * -1;
			}
		}

		public ZDecimal WIPNotRecognized
		{
			get
			{
				return ProfitLoss.TotalWIPNotRecognized;
			}
		}

		public ZDecimal ACRNotRecognized
		{
			get
			{
				return ProfitLoss.TotalAccrualNotRecognized * -1;
			}
		}

		public ZDecimal TotalJobProfitNotRecognizedInGL
		{
			get
			{
				return RevenueNotRecognized + WIPNotRecognized - CostNotRecognized - ACRNotRecognized;
			}
		}

		public DocJobInvoicingJobChargeCollection RevenueMovementsRecognized
		{
			get
			{
				if (fRevenueMovementsRecognized == null)
				{
					var temp = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						temp.AddRange(job.RevenueMovementsRecognized);
					}
					fRevenueMovementsRecognized = new DocJobInvoicingJobChargeCollection(Factory);
#if NETFRAMEWORK
					fRevenueMovementsRecognized.AddRange(temp.DistinctBy(x => ((DocJobInvoicingJobCharge)x).ChargePK));
#else
					fRevenueMovementsRecognized.AddRange(IEnumerableExtensions.DistinctBy(temp, x => ((DocJobInvoicingJobCharge)x).ChargePK));
#endif
				}
				return fRevenueMovementsRecognized;
			}
		}
		DocJobInvoicingJobChargeCollection fRevenueMovementsRecognized;

		public DocJobInvoicingJobChargeCollection RevenueMovementsRecognizedForProfitShare
		{
			get
			{
				if (fRevenueMovementsRecognizedForProfitShare == null)
				{
					var temp = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						temp.AddRange(job.RevenueMovementsRecognizedForProfitShare);
					}
					fRevenueMovementsRecognizedForProfitShare = new DocJobInvoicingJobChargeCollection(Factory);
					fRevenueMovementsRecognizedForProfitShare.AddRange(temp);
				}
				return fRevenueMovementsRecognizedForProfitShare;
			}
		}
		DocJobInvoicingJobChargeCollection fRevenueMovementsRecognizedForProfitShare;

		public DocJobInvoicingJobChargeCollection CostMovementsRecognized
		{
			get
			{
				if (fCostMovementsRecognized == null)
				{
					var temp = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						temp.AddRange(job.CostMovementsRecognized);
					}
					fCostMovementsRecognized = new DocJobInvoicingJobChargeCollection(Factory);
#if NETFRAMEWORK
					fCostMovementsRecognized.AddRange(temp.DistinctBy(x => ((DocJobInvoicingJobCharge)x).ChargePK));
#else
					fCostMovementsRecognized.AddRange(IEnumerableExtensions.DistinctBy(temp, x => ((DocJobInvoicingJobCharge)x).ChargePK));
#endif
				}
				return fCostMovementsRecognized;
			}
		}
		DocJobInvoicingJobChargeCollection fCostMovementsRecognized;

		public DocJobInvoicingJobChargeCollection CostMovementsRecognizedForProfitShare
		{
			get
			{
				if (fCostMovementsRecognizedForProfitShare == null)
				{
					var temp = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						temp.AddRange(job.CostMovementsRecognizedForProfitShare);
					}
					fCostMovementsRecognizedForProfitShare = new DocJobInvoicingJobChargeCollection(Factory);
					fCostMovementsRecognizedForProfitShare.AddRange(temp);
				}
				return fCostMovementsRecognizedForProfitShare;
			}
		}
		DocJobInvoicingJobChargeCollection fCostMovementsRecognizedForProfitShare;

		public WIPACRMovementsRecognized WIPMovementsRecognized
		{
			get
			{
				if (fWIPMovementsRecognized == null)
				{
					fWIPMovementsRecognized = new WIPACRMovementsRecognized(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fWIPMovementsRecognized.AddRange(job.WIPMovementsRecognized);
					}
				}
				return fWIPMovementsRecognized;
			}
		}
		WIPACRMovementsRecognized fWIPMovementsRecognized;

		public WIPACRMovementsRecognized ACRMovementsRecognized
		{
			get
			{
				if (fACRMovementsRecognized == null)
				{
					fACRMovementsRecognized = new WIPACRMovementsRecognized(Factory);
					foreach (DocJobInvoicingJob job in Jobs)
					{
						fACRMovementsRecognized.AddRange(job.ACRMovementsRecognized);
					}
				}
				return fACRMovementsRecognized;
			}
		}
		WIPACRMovementsRecognized fACRMovementsRecognized;

		#region TotalProfit

		public ZDecimal TotalProfit
		{
			get { return TotalRealisedAmount + TotalEstimatedAmount; }
		}

		#endregion

		#endregion

		#region Margins

		#region Profit/Cost Margin

		public ZDecimal ProfitCostMargin
		{
			get
			{
				return TotalExpense != 0 ? (TotalProfit / TotalExpense) : 0;
			}
		}

		#endregion

		#region Profit/Rev Margin

		public ZDecimal ProfitRevMargin
		{
			get
			{
				return TotalIncome != 0 ? (TotalProfit / TotalIncome) : 0;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		protected delegate DocForwardingConsol NewDelegate(ForwardingConsol consol, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected void AddWeightVolumePackage(DocPackLines line, ZDecimal weightToAdd, ZString weightUnit, ZDecimal volumeToAdd, ZString volumeUnit, ZInt packCount)
		{
			line.ContainerManifestPackage += packCount;
			line.ContainerManifestVolume += Constants.Volume.ConvertSafe(volumeToAdd, volumeUnit, line.ContainerManifestVolumeUnit);
			line.ContainerManifestWeight += Constants.Weight.ConvertSafe(weightToAdd, weightUnit, line.ContainerManifestWeightUnit);
			line.ContainerTotalManifestVolume += Constants.Volume.ConvertSafe(volumeToAdd, volumeUnit, line.ContainerTotalManifestVolumeUnit);
			line.ContainerTotalManifestWeight += Constants.Weight.ConvertSafe(weightToAdd, weightUnit, line.ContainerTotalManifestWeightUnit);
		}

		protected ZString GetEXExportPermissionForConsignor()
		{
			ZString result = ZString.Empty;

			if (DirectShipment != null && DirectShipment.Consignor != null)
			{
				if (DirectShipment.Consignor.CountryData != null && !DirectShipment.Consignor.CountryData.ExportPermissionDetails.IsEmpty)
				{
					result = DirectShipment.Consignor.CountryData.ExportPermissionDetails;
				}
				else
				{
					result = DirectShipment.Consignor.Code;
				}
			}

			return result;
		}

		protected internal ZString GetCarrier(DocTransport flight)
		{
			ZString result = ZString.Empty;

			if (flight != null && !flight.VoyageFlight.IsEmpty)
			{
				ZString code = flight.VoyageFlight.Substring(0, 2);
				var airLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, code));
				if (airLine != null)
				{
					result = airLine.RM_AirlineName1;
				}
			}

			return result;
		}

		protected internal ZBool IsUSConsol
		{
			get
			{
				ZBool result = false;

				if (PortOfDischarge != null && PortOfDischarge.Code.Substring(0, 2) == "US")
				{
					result = true;
				}
				else
				{
					foreach (DocTransport transport in TransportPlanning)
					{
						if (!transport.PortOfDischargeCode.IsEmpty && transport.PortOfDischargeCode.Substring(0, 2) == "US")
						{
							result = true;
						}
					}
				}

				return result;
			}
		}

		protected internal DocForwardingShipment GetNewDocShipment(ForwardingShipment shipment)
		{
			return DocForwardingShipment.New(shipment, Factory);
		}

		protected internal DocForwardingShipment GetNewDocShipment(ZGuid pK)
		{
			return DocForwardingShipment.New(Factory, pK);
		}

		protected internal ZString GetTransportPlanningTransportDetails(ZString transportPlanningType)
		{
			ZString result = ZString.Empty;

			foreach (DocTransport transport in TransportPlanning)
			{
				if (!transportPlanningType.IsEmpty && transport.TransportType == transportPlanningType)
				{
					ZString lloydsNumber = transport.Vessel != null ? transport.Vessel.LloydsNumber : ZString.Empty;
					result = FormatTransportDetails(transport.VesselName, transport.VoyageFlight, lloydsNumber);
					break;
				}
				else if (transport.TransportType == Core.Constants.TransportPlanningType.MainVessel)
				{
					ZString lloydsNumber = transport.Vessel != null ? transport.Vessel.LloydsNumber : ZString.Empty;
					result = FormatTransportDetails(transport.VesselName, transport.VoyageFlight, lloydsNumber);
				}
			}

			if (result.IsEmpty)
			{
				ZString lloydsNumber = Vessel == null ? ZString.Empty : Vessel.LloydsNumber;
				result = FormatTransportDetails(VesselName, VoyageNumber, lloydsNumber);
			}

			return result;
		}

		protected ZString GetTotalContainersAndPackagesPerType() //need to look for unit test and fix it!
		{
			string sQL =
				"SELECT " + RefContainerSchema.Constants.RC_Code + " AS Type, SUM(" + JobContainerSchema.Constants.JC_ContainerCount + ") AS Containers, SUM(PackageCount) AS Packages" +
				" FROM " + JobContainerSchema.Constants.SqlSchemaName + "." + JobContainerSchema.Constants.TableName +
				" JOIN " + RefContainerSchema.Constants.SqlSchemaName + "." + RefContainerSchema.Constants.TableName + " ON " + JobContainerSchema.Constants.JC_RC + " = " + RefContainerSchema.Constants.PK +
				" JOIN " + "(" +
				" SELECT " + JobContainerPackPivotSchema.Constants.J6_JC + ", SUM( " + JobPackLinesSchema.Constants.JL_PackageCount + ") AS PackageCount" +
				" FROM " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName +
				" JOIN " + JobPackLinesSchema.Constants.SqlSchemaName + "." + JobPackLinesSchema.Constants.TableName + " ON " + JobContainerPackPivotSchema.Constants.J6_JL + " = " + JobPackLinesSchema.Constants.PK +
				" JOIN " + JobShipmentSchema.Constants.SqlSchemaName + "." + JobShipmentSchema.Constants.TableName + " ON " + JobPackLinesSchema.Constants.JL_JS + " = " + JobShipmentSchema.Constants.PK +
				" WHERE " + JobShipmentSchema.Constants.JS_JS_ColoadMasterShipment + " IS NULL" +
				" GROUP BY " + JobContainerPackPivotSchema.Constants.J6_JC +
				" ) AS Pack ON " + JobContainerPackPivotSchema.Constants.J6_JC + " = " + JobContainerSchema.Constants.PK +
				" WHERE " + JobContainerSchema.Constants.JC_JK + " = @ConsolPK" +
				" GROUP BY " + RefContainerSchema.Constants.RC_Code
				;

			DynamicBusinessObjectCollection dBOC = new DynamicBusinessObjectCollection(Factory);

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@ConsolPK", this.Consol.PK, JobConsolSchema.PK);

			dBOC.Load(sQL, @params);

			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			int totalFCLPackages = 0;
			int totalOuterPacks = 0;

			foreach (DynamicBusinessObject dBO in dBOC)
			{
				ZString type = new ZString(dBO["Type"]);
				ZInt containers = new ZInt(dBO["Containers"]);
				ZInt packages = new ZInt(dBO[nameof(Packages)]);
				totalFCLPackages += packages;
				builder.Append(Res.GetString("f003b1c7-8019-452b-b3f7-a70742bac069", "{0} x {1} Container {2}{3} Package(s);", containers, type, STC_Label, packages) + " ");
			}

			foreach (DocForwardingShipment shipment in MasterShipments)
			{
				totalOuterPacks += shipment.TotalOuterPackLinePackages;
			}

			ZInt totalLCLPackages = totalOuterPacks - totalFCLPackages;

			if (totalLCLPackages > 0)
			{
				builder.Append(Res.GetString("6dee44c5-c221-4d4d-bf04-987b0fa7b3e1", "{0} LCL Package(s)", totalLCLPackages.ToString()));
			}

			return builder.ToString();
		}

		#endregion

		#region IDocJobDetail Members

		public ZString OrderNumbersForInvoice
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString OurReference
		{
			get
			{
				return ConsolNumber;
			}
		}

		public ZString SupplierAsString
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString VesselAndVoyage
		{
			get
			{
				return ConsolTransportInfo;
			}
		}

		public ZString MasterBillNumber
		{
			get
			{
				return MasterBillNum;
			}
		}

		public ZString ETAPortName
		{
			get
			{
				return PortOfDischarge != null ? PortOfDischarge.PortName : ZString.Empty;
			}
		}

		public ZString ETDPortName
		{
			get
			{
				return PortOfDischarge != null ? PortOfLoading.PortName : ZString.Empty;
			}
		}

		public ZString Service
		{
			get
			{
				return ConsolMode;
			}
		}

		public ZString PackageQuantity
		{
			get
			{
				return OuterPacks.Count.ToString();
			}
		}

		public ZString PackageType
		{
			get
			{
				return Res.GetString("965f436c-4703-4263-bcba-00c425453820", "PACKAGE");
			}
		}

		public ZString ConsolDepot
		{
			get
			{
				ZString result;

				if (IsExportConsol && PackDepotAddress != null)
				{
					result = PackDepotAddress.ToString();
				}
				else if (IsImportConsol && UnpackDepotAddress != null)
				{
					result = UnpackDepotAddress.ToString();
				}
				else
				{
					result = ZString.Empty;
				}
				return result;
			}
		}

		public ZString WeightAsString
		{
			get
			{
				return TotalWeight + " " + WeightUnit;
			}
		}

		public ZString VolumeAsString
		{
			get
			{
				return TotalVolume + " " + VolumeUnit;
			}
		}

		public ZString Note
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString ConsignorAsString
		{
			get
			{
				return Consignor;
			}
		}

		public ZString ConsigneeAsString
		{
			get
			{
				return Consignee;
			}
		}

		public ZString ShortContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerAndSealNumbers = ZString.Empty;

				if (Containers != null && Containers.Count > 0)
				{
					var container = Containers[0];
					containerAndSealNumbers += container.ContainerNumber + " / " + container.SealNumber;
					if (container.Container != null)
					{
						containerAndSealNumbers += " / " + container.Container.Code;
					}
				}

				return containerAndSealNumbers;
			}
		}

		public ZString LongContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerString = ZString.Empty;

				foreach (IDocContainer container in Containers)
				{
					containerString += "- " + container.ContainerNumber.PadRight(15, ' ');
					containerString += " - " + container.SealNumber.PadRight(20, ' ') + " - ";

					ZString containerCode = (container.Container != null) ? container.Container.Code : ZString.Empty;
					containerString += containerCode.PadRight(15, ' ') + "\n";
				}

				return containerString;
			}
		}

		public ZInt NumberOfContainers
		{
			get { return Containers != null ? Containers.Count : 0; }
		}

		public ZString MarksAndNumbersForInvoice
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString ShortGoodsDescriptionForInvoice
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString LongGoodsDescriptionForInvoice
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDateTime ETADate
		{
			get
			{
				return ETA;
			}
		}

		public ZDateTime ETDDate
		{
			get
			{
				return ETD;
			}
		}

		public ZBool TransportModeIsAir
		{
			get
			{
				return TransportMode == Core.Constants.TransportModes.Air ? ZBool.True : ZBool.False;
			}
		}

		public ZBool TransportModeIsSea
		{
			get
			{
				return TransportMode == Core.Constants.TransportModes.Sea ? ZBool.True : ZBool.False;
			}
		}

		#endregion

		#region IRequestForMissingDocuments Members

		public ZString HouseBillHeading
		{
			get { return ZString.Empty; }
		}

		public ZString HouseBillAndIssueHeading
		{
			get { return ZString.Empty; }
		}

		public ZString HouseBill
		{
			get { return ZString.Empty; }
		}

		public ZString HouseBillAndIssueDate
		{
			get { return ZString.Empty; }
		}

		public ZString DeclarationOrConsolNumber
		{
			get { return ZString.Empty; }
		}

		public ZString GoodsDescription
		{
			get { return Res.GetString("8598a2e9-f584-428b-96f7-c8fe9ebcf40e", "Consolidated Cargo"); }
		}

		public ZString OwnerRefAndOrderRef
		{
			get { return AgentsReference; }
		}

		public ZString OwnerRefAndOrderRefHeading
		{
			get { return Res.GetString("187ceab6-dfcd-4625-ac1c-223a4720cb58", "AGENTS REFERENCE"); }
		}

		public ZString Weight
		{
			get { return ShipmentsWeight.ToString(); }
		}

		public ZString Volume
		{
			get { return TotalVolume; }
		}

		public ZString TransportInfo
		{
			get { return ConsolTransportInfo; }
		}

		public ZString ConsigneeOrgHeading
		{
			get { return Res.GetString("9f1b3bf6-23e9-4f10-93c3-64fde8f03099", "RECEIVING AGENT"); }
		}

		public ZString ConsignorOrgHeading
		{
			get { return Res.GetString("d665b962-d5db-4807-bcdf-9cf1e902df86", "SENDING AGENT"); }
		}

		public DocOrganisation ConsigneeOrg
		{
			get
			{
				if ((MenuTitle == CommonConsolDocumentSupporter.DocumentTemplateTitles.ForwardingInstructionStandard || MenuTitle == CommonConsolDocumentSupporter.DocumentTemplateTitles.ForwardingInstructionDetailed) && Consol.IsSea && !Consol.IsDirect)
				{
					return DocOrganisation.New(((ForwardingConsol)Consol).MasterBillConsigneeOverrideDocumentaryAddress, Factory);
				}
				return ReceivingForwarder;
			}
		}

		public DocOrganisation ConsignorOrg
		{
			get
			{
				if ((MenuTitle == CommonConsolDocumentSupporter.DocumentTemplateTitles.ForwardingInstructionStandard || MenuTitle == CommonConsolDocumentSupporter.DocumentTemplateTitles.ForwardingInstructionDetailed) && Consol.IsSea && !Consol.IsDirect)
				{
					return DocOrganisation.New(((ForwardingConsol)Consol).MasterBillShipperOverrideDocumentaryAddress, Factory);
				}
				return SendingForwarder;
			}
		}

		public ZString RequestForMissingDocumentsInstruction
		{
			get { return Env.Registry.ShipmentRequestForMissingDocumentsClause; }
		}

		public virtual ZString ContainerNumbers
		{
			get { return ContainerLine; }
		}

		public ZString MissingRequiredDocuments
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocShipment shipment in Shipments)
				{
					if (!shipment.MissingRequiredDocuments.IsEmpty)
					{
						result += Res.GetString("058a9d7b-b1cf-4e80-8a98-b4c6e87213b6", "Shipment: {0}", shipment.ShipmentNumber) + " ";
						result += Res.GetString("9d1174ad-9cda-4971-8233-81cadaed6aeb", "House bill: {0}", shipment.HouseBill) + "\r\n";
						result += shipment.MissingRequiredDocuments + "\r\n";
					}
				}
				return result.TrimEnd();
			}
		}

		public ZString Packages
		{
			get { return TotalPackageCount.ToString(); }
		}

		#endregion

		#region ITimeSlotRequest Members

		public ZString PackingMode
		{
			get { return ConsolMode; }
		}

		public ZString BookingETA
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETA); }
		}

		public ZString BookingETD
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETD); }
		}

		public ZString EquipmentType
		{
			get { return ZString.Empty; }
		}

		public ZString FullCartageInstructions
		{
			get { return CartageInstructions; }
		}

		public ZString FullHandlingInstructions
		{
			get { return HandlingInstructions; }
		}

		public ZDateTime CartageCutOffDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					((IRoutingSupport)Consol).TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff :
					((IRoutingSupport)Consol).TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
			}
		}

		public ZDateTime CartageAvailableDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					((IRoutingSupport)Consol).TransportsIncludingRelated.LastLeg.JW_TerminalAvailabilityDate :
					((IRoutingSupport)Consol).TransportsIncludingRelated.LastLeg.JW_DepotAvailabilityDate;
			}
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExportDocument)
				{
					result = PackingMode == Constants.ContainerModes.FCL || PackingMode == Constants.ContainerModes.ULD ? FCLCutOff : LCLCutOff;
				}
				else if (IsImportDocument)
				{
					result = PackingMode == Constants.ContainerModes.FCL || PackingMode == Constants.ContainerModes.ULD ? AvailabilityDate : LCLAvailabilityDate;
				}

				return result;
			}
		}

		public ZDateTime CartageReceivalDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					((IRoutingSupport)Consol).TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences :
					((IRoutingSupport)Consol).TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
			}
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					((IRoutingSupport)Consol).TransportsIncludingRelated.LastLeg.JW_TerminalStorageDate :
					((IRoutingSupport)Consol).TransportsIncludingRelated.LastLeg.JW_DepotStorageDate;
			}
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsExportDocument)
				{
					result = PackingMode == Constants.ContainerModes.FCL || PackingMode == Constants.ContainerModes.ULD ? FCLReceivalCommences : LCLReceivalCommences;
				}
				else if (IsImportDocument)
				{
					result = PackingMode == Constants.ContainerModes.FCL || PackingMode == Constants.ContainerModes.ULD ? StorageCommences : LCLStorageCommences;
				}

				return result;
			}
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsExportDocument)
				{
					result = CartageAdvice.ReceivalsStartHeading;
				}
				else if (IsImportDocument)
				{
					result = CartageAdvice.StorageCommencesHeading;
				}

				return result;
			}
		}

		public DocDocAddress CTOAddress
		{
			get { return null; }
		}

		public DocContacts NotifyParty
		{
			get { return null; }
		}

		public DocUNLOCO OriginLoco
		{
			get { return null; }
		}

		public DocUNLOCO DestinationLoco
		{
			get { return null; }
		}

		public IDocSimpleContainerCollection SimpleContainers
		{
			get
			{
				IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
				if (Containers != null)
				{
					foreach (DocContainer container in Containers)
					{
						result.Add(container);
					}
				}
				return result;
			}
		}

		#endregion
		#region IDocCartageAdvice Members

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get { return ResString.GetMultilingualString("4d3c1115-b27f-48ef-bfb4-dc42e50b1b88", "PICKUP FROM"); }
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get { return ResString.GetMultilingualString("839be5e8-9325-4e7f-87e8-1edefc460468", "DELIVER TO"); }
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get { return (NoResString)""; }
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get { return (NoResString)""; }
		}

		#endregion

		#region Addresses

		public DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				JobDocAddress jobDocAddress = IsExportDocument ? Consol.GetDepartureCFSDocAddress : Consol.GetArrivalCTODocAddress;
				return DocDocAddress.New(jobDocAddress, Factory);
			}
		}

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get
			{
				JobDocAddress jobDocAddress = IsExportDocument ? Consol.GetDepartureCTODocAddress : Consol.GetArrivalCFSDocAddress;
				return DocDocAddress.New(jobDocAddress, Factory);
			}
		}

		public DocDocAddress JourneyTwoPickUpAddress
		{
			get { return null; }
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get { return null; }
		}

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return ""; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return ""; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return ""; }
		}

		#endregion

		#endregion

		public ZBool PrintAsContainers
		{
			get { return false; }
		}

		public ZBool PrintTwoJourneys
		{
			get { return false; }
		}

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				if (fAddressesWithWareHousing == null)
				{
					fAddressesWithWareHousing = new DocDocAddressCollection(Factory);
					var inter = this as IDocCartageAdvice;
					if (JourneyOnePickUpAddress != null)
					{
						fAddressesWithWareHousing.Add(inter.JourneyOnePickUpAddress);
					}

					if (JourneyOneDeliverToAddress != null)
					{
						fAddressesWithWareHousing.Add(inter.JourneyOneDeliverToAddress);
					}
				}
				return fAddressesWithWareHousing;
			}
		}

		DocDocAddressCollection fAddressesWithWareHousing;

		public CartageAdviceHelper CartageAdvice
		{
			get { return new CartageAdviceHelper(this, Factory); }
		}

		#endregion
	}
}
