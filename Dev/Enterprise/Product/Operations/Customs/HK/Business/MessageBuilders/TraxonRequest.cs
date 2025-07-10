using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.HK.Business;
using Enterprise.Customs.HK.Business.MessageBuilders;
using Enterprise.Edifact;
using Enterprise.Edifact.D95A.Elements;
using Enterprise.Edifact.D95A.Messages.CUSEXP;
using Enterprise.Edifact.D95A.Segments;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.Traxon.MessageBuilders
{
	public class TraxonMessageGenerator
	{
		public TraxonMessageGenerator(TraxonConsolStatus consolStatus, int maxShipmentsPerMessage = 999)
		{
			if (consolStatus.Consol == null)
			{
				throw new ApplicationException("A valid Consol must be passed to the TraxonRequest constructor");
			}

			this.consolStatus = consolStatus;
			this.maxShipmentsPerMessage = maxShipmentsPerMessage;
		}
		readonly int maxShipmentsPerMessage;
		readonly TraxonConsolStatus consolStatus;

		protected ForwardingConsol Consol
		{
			get { return consolStatus.Consol; }
		}

		public void GenerateSendMessages()
		{
			GenerateMessages(false);
		}

		public void GenerateWithdrawMessages()
		{
			GenerateMessages(true);
		}

		protected void GenerateMessages(bool isCancel)
		{
			int totalCount;
			var collectionOfCollection = GetSplitShipments(out totalCount);
			for (int i = 0; i < collectionOfCollection.Count; i++)
			{
				TraxonRequest request = new TraxonRequest(Consol, collectionOfCollection[i], i * maxShipmentsPerMessage, totalCount);
				AddMessage(isCancel ? request.GenerateWithdraw() : request.GenerateSend());
			}
		}

		void AddMessage(ZString messageText)
		{
			TraxonMessage manifestMessage = consolStatus.OrderedMessages.AddNew();
			manifestMessage.EM_LinkUniqueID = Consol.PK;
			manifestMessage.EM_MessageText = messageText;
			Consol.Messages.Load();
		}

		List<List<ForwardingShipment>> GetSplitShipments(out int totalCount)
		{
			totalCount = 0;
			var result = new List<List<ForwardingShipment>>();
			var shipmentsList = new List<ForwardingShipment>();

			int x = 0;
			while (x < Consol.Shipments.Count)
			{
				var shipment = Consol.Shipments[x++];

				if (ShouldReportToTraxon(shipment))
				{
					totalCount++;
					shipmentsList.Add(shipment);
					if (shipmentsList.Count == maxShipmentsPerMessage)
					{
						result.Add(shipmentsList);
						shipmentsList = new List<ForwardingShipment>();
					}
				}
			}

			if (shipmentsList.Count > 0)
			{
				result.Add(shipmentsList);
			}

			return result;
		}

		bool ShouldReportToTraxon(ForwardingShipment shipment)
		{
			var parentShipment = shipment.CoLoadMasterShipment;

			return parentShipment == null || (!parentShipment.IsAssemblyMaster && !parentShipment.IsCoLoadMaster && !parentShipment.IsBlindCoLoadMaster && !parentShipment.IsHighVolumeLowValueMaster);
		}
	}

	public class TraxonRequest
	{
		public TraxonRequest(ForwardingConsol consol, List<ForwardingShipment> collection, int shipmentGroupOffset, int totalShipmentCountForAllMessages)
		{
			message = new CUSEXPMessage();
			this.consol = consol;
			this.collection = collection;
			this.shipmentGroupOffset = shipmentGroupOffset;
			this.totalShipmentCountForAllMessages = totalShipmentCountForAllMessages;
		}

		#region Constants
		const string Kilograms = "KGM";
		internal const int WeightDecimals = 1;
		#endregion

		readonly CUSEXPMessage message;
		readonly ForwardingConsol consol;
		readonly List<ForwardingShipment> collection;
		readonly int shipmentGroupOffset;
		readonly int totalShipmentCountForAllMessages;

		public ZString GenerateSend()
		{
			return GenerateSingleMessage(false);
		}
		public ZString GenerateWithdraw()
		{
			return GenerateSingleMessage(true);
		}

		protected ZString GenerateSingleMessage(bool cancel)
		{
			UNHSegment();
			BGMSegment(cancel);
			LOCSegment();
			Group1NADSegment();
			SegmentGroup3 group3 = message.Group3.InstantiateAChildAndAddItToChildrenCollection();
			Group3TDTSegment(group3);
			Group3DTMSegment(group3);
			SegmentGroup6 group6 = message.Group6.InstantiateAChildAndAddItToChildrenCollection();
			Group6RFFSegment(group6);
			Group6CNTSegment(group6, totalShipmentCountForAllMessages);

			ZDecimal totalGrossWeight = 0;
			int totalNumberOfPackages = 0;
			int currentShipment = 0;
			foreach (ForwardingShipment shipment in collection)
			{
				var weightInKG = shipment.GetWeightInKG();
				currentShipment++;
				totalGrossWeight += weightInKG;

				int currentShipmentNumberOfPackages = shipment.JS_OuterPacks;

				totalNumberOfPackages = totalNumberOfPackages + currentShipmentNumberOfPackages;
				SegmentGroup7 group7 = message.Group6[0].Group7.InstantiateAChildAndAddItToChildrenCollection();

				var houseBill = shipment.JS_HouseBill.ToUpper().KeepAlphanumericCharacters();
				if (houseBill.Length > 12 && houseBill.StartsWith(CommonShipment.PreAllocatedHouseBillPrefix))
				{
					houseBill = houseBill.SubstringSafe(3);
				}

				Group7CNISegment(group7, currentShipment, houseBill);

				var shippingLoadAndCount = shipment.AWBHeader.EH_ShippingLoadAndCount > 0 ? (int)shipment.AWBHeader.EH_ShippingLoadAndCount : currentShipmentNumberOfPackages;
				Group7CNTSegment(group7, currentShipmentNumberOfPackages, shippingLoadAndCount);
				Group7MEASegment(group7, weightInKG.ToString(WeightDecimals));
				Group7LOCSegment(group7, shipment);
				Group7NamesAndAddresses(group7, shipment);
				SegmentGroup8 group8 = group7.Group8.InstantiateAChildAndAddItToChildrenCollection();
				Group8GDSSegment(group8);
				Group8FTXSegment(group8, shipment);
				Group9PACSegment(group7.Group9.InstantiateAChildAndAddItToChildrenCollection(), shipment);
				Group11MOASegment(group7.Group11.InstantiateAChildAndAddItToChildrenCollection(), shipment);
				Group14DOCSegment(group7.Group14.InstantiateAChildAndAddItToChildrenCollection(), shipment);
				Group15FTXSegment(group7, shipment);
			}

			CNTSegment(totalGrossWeight.ToString(WeightDecimals), totalNumberOfPackages.ToString());
			UNTSegment();
			return message.ToString(new UNOACharacterSet());
		}

		#region Implementation

		void UNHSegment()
		{
			UNHSegment uNH = message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageReferenceNumber = ConsolReferenceNumber;
			uNH.MessageIdentifier.MessageType = "CUSEXP";
			uNH.MessageIdentifier.MessageVersionNumber = "D";
			uNH.MessageIdentifier.MessageReleaseNumber = "95A";
			uNH.MessageIdentifier.ControllingAgency = "UN";
			uNH.CommonAccessReference = EDIMessage.MessageNumberPlaceHolder;
		}

		ZString ConsolReferenceNumber
		{
			get { return "HMF" + consol.JK_MasterBillNum.SubstringSafe(3, 7) + "X" + consol.JK_MasterBillNum.Left(3); }
		}

		void BGMSegment(bool cancel)
		{
			BGMSegment bGM = message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.CustomsManifest;
			bGM.DocumentMessageName.DocumentMessageName = "EXPRESS CONSIGNMENT MANIFEST";
			bGM.DocumentMessageNumber = consol.JK_MasterBillNum + "/" + consol.JK_UniqueConsignRef;
			if (cancel)
			{
				bGM.MessageFunctionCoded = MessageFunctionCodedList.Deletion;
			}
			else
			{
				bGM.MessageFunctionCoded = MessageFunctionCodedList.Replace;
			}
		}

		void LOCSegment()
		{
			LOCSegment lOC1 = message.LOC.InstantiateAChildAndAddItToChildrenCollection();
			LOCSegment lOC2 = message.LOC.InstantiateAChildAndAddItToChildrenCollection();

			lOC1.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDeparture;
			var portOfLoading = consol.JK_RL_NKLoadPort;
			if (!portOfLoading.IsEmpty)
			{
				var loadingUNLOCO = consol.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, portOfLoading));
				if (loadingUNLOCO != null && !loadingUNLOCO.RL_IATA.IsEmpty)
				{
					portOfLoading = loadingUNLOCO.RL_IATA;
				}
			}
			lOC1.LocationIdentification.PlaceLocationIdentification = portOfLoading.Right(3);

			lOC2.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDestination;
			var portOfDischarge = consol.IsAir ? consol.Transports?.LastTransportWithTransportMode(Constants.TransportModes.Air)?.JW_RL_NKDiscPort ?? consol.JK_RL_NKDischargePort : consol.JK_RL_NKDischargePort;
			if (!portOfDischarge.IsEmpty)
			{
				var dischargeUNLOCO = consol.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, portOfDischarge));
				if (dischargeUNLOCO != null && !dischargeUNLOCO.RL_IATA.IsEmpty)
				{
					portOfDischarge = dischargeUNLOCO.RL_IATA;
				}
			}
			lOC2.LocationIdentification.PlaceLocationIdentification = portOfDischarge.Right(3);
		}

		void CNTSegment(ZString totalGrossWeight, ZString totalNumberOfPackages)
		{
			CNTSegment cNT1 = message.CNT.InstantiateAChildAndAddItToChildrenCollection();
			CNTSegment cNT2 = message.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT1.Control.ControlQualifier = ControlQualifierList.TotalGrossWeight;
			cNT1.Control.ControlValue = totalGrossWeight;
			cNT1.Control.MeasureUnitQualifier = Kilograms;
			cNT2.Control.ControlQualifier = ControlQualifierList.TotalPieces;
			cNT2.Control.ControlValue = totalNumberOfPackages;
		}

		void Group1NADSegment()
		{
			NADSegment nAD = message.Group1.InstantiateAChildAndAddItToChildrenCollection().NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyQualifier = PartyQualifierList.ContactParty;
			nAD.PartyIdentificationDetails.PartyIdIdentification = HKDataRegistry.Instance.CosacAgentCode.Value;
		}

		void Group3TDTSegment(SegmentGroup3 group3)
		{
			TDTSegment tDT = group3.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tDT.TransportStageQualifier = TransportStageQualifierList.AtDestination;
			tDT.ConveyanceReferenceNumber = consol.JK_JX_JV_VoyageFlight;
		}

		void Group3DTMSegment(SegmentGroup3 group3)
		{
			DTMSegment dTM = group3.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.ArrivalDateTimeEstimated;
			dTM.DateTimePeriod.DateTimePeriod = consol.JK_JX_JB_E_ARV.ToString("yyMMdd");
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Yymmdd;
		}

		void Group6RFFSegment(SegmentGroup6 group6)
		{
			RFFSegment rFF = group6.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceQualifier = ReferenceQualifierList.MasterAirWaybillNumber;
			rFF.Reference.ReferenceNumber = consol.JK_MasterBillNum.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
		}

		void Group6CNTSegment(SegmentGroup6 group6, int totalShipmentCountForAllMessages)
		{
			CNTSegment cNT = group6.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlQualifier = ControlQualifierList.TotalNumberOfConsignments;
			cNT.Control.ControlValue = totalShipmentCountForAllMessages.ToString();
		}

		void Group7CNISegment(SegmentGroup7 group7, int currentShipment, ZString houseBill)
		{
			int shipmentCount = shipmentGroupOffset + currentShipment;
			CNISegment cNI = group7.CNI.InstantiateAChildAndAddItToChildrenCollection();
			cNI.ConsolidationItemNumber = shipmentCount.ToString();
			cNI.DocumentMessageDetails.DocumentMessageNumber = houseBill;
		}

		void Group7CNTSegment(SegmentGroup7 group7, int numberOfPackages, int shipperLoadCount)
		{
			CNTSegment cNT = group7.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cNT.Control.ControlQualifier = ControlQualifierList.TotalPieces;
			cNT.Control.ControlValue = numberOfPackages.ToString();

			if (shipperLoadCount > 0)
			{
				var shipperLoadCountCNT = group7.CNT.InstantiateAChildAndAddItToChildrenCollection();
				shipperLoadCountCNT.Control.ControlQualifier = ControlQualifierList.AlgebraicTotalOfTheQuantityValuesInLineItemsInAMessage;
				shipperLoadCountCNT.Control.ControlValue = shipperLoadCount.ToString();
			}
		}

		void Group7MEASegment(SegmentGroup7 group7, ZString actualWeight)
		{
			MEASegment mEA = group7.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mEA.MeasurementApplicationQualifier = MeasurementApplicationQualifierList.Weights;
			mEA.ValueRange.MeasureUnitQualifier = Kilograms;
			mEA.ValueRange.MeasurementValue = actualWeight;
		}

		void Group7LOCSegment(SegmentGroup7 group7, ForwardingShipment shipment)
		{
			LOCSegment lOC1 = group7.LOC.InstantiateAChildAndAddItToChildrenCollection();
			LOCSegment lOC2 = group7.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC1.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDeparture;
			var originCode = shipment.JS_RL_NKOrigin;
			if (!originCode.IsEmpty)
			{
				var originUNLOCO = shipment.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, originCode));
				if (originUNLOCO != null && !originUNLOCO.RL_IATA.IsEmpty)
				{
					originCode = originUNLOCO.RL_IATA;
				}
			}
			lOC1.LocationIdentification.PlaceLocationIdentification = originCode.Right(3);

			lOC2.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDestination;
			var destinationCode = shipment.JS_RL_NKDestination;
			if (!destinationCode.IsEmpty)
			{
				var destinationUNLOCO = shipment.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, destinationCode));
				if (destinationUNLOCO != null && !destinationUNLOCO.RL_IATA.IsEmpty)
				{
					destinationCode = destinationUNLOCO.RL_IATA;
				}
			}
			lOC2.LocationIdentification.PlaceLocationIdentification = destinationCode.Right(3);
		}

		void PopulateGroup7NAD(SegmentGroup7 group7, PartyQualifierList partyQualifier, IOrgDetails org)
		{
			var nAD = group7.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyQualifier = partyQualifier;
			var contactDetails = org.ContactDetails;
			if (contactDetails.IsEmpty)
			{
				var phoneNum = FormatPhoneNumber(org.PhoneNo);
				if (!phoneNum.IsEmpty)
				{
					contactDetails = Core.Constants.AWB.ContactCodes.TELEPHONE + " " + phoneNum;
				}
				else
				{
					var faxNum = FormatPhoneNumber(org.FaxNo);
					if (!faxNum.IsEmpty)
					{
						contactDetails = Core.Constants.AWB.ContactCodes.FAX + " " + faxNum;
					}
				}
			}

			nAD.NameAndAddress.NameAndAddressLine1 = contactDetails;
			nAD.PartyName.PartyName1 = org.Name.Left(35);
			nAD.CountryCoded = org.Country.Left(2);
			nAD.Street.StreetAndNumberPOBox1 = org.Address1.Left(35);
			nAD.Street.StreetAndNumberPOBox2 = org.Address2.Left(35);
			nAD.CountrySubEntityIdentification = org.State.Left(9);
			nAD.CityName = org.City.Left(35);
			nAD.PostcodeIdentification = org.PostCode.Left(9);
		}

		void Group7NamesAndAddresses(SegmentGroup7 group7, ForwardingShipment shipment)
		{
			var consigneeDetails = shipment.GetConsigneeOrgDetails();
			consigneeDetails.ContactDetails = GetContactDetails(shipment, OrganisationTypes.Consignee);
			PopulateGroup7NAD(group7, PartyQualifierList.Consignee, consigneeDetails);

			var consignorDetails = shipment.GetConsignorOrgDetails();
			consignorDetails.ContactDetails = GetContactDetails(shipment, OrganisationTypes.Consignor);
			PopulateGroup7NAD(group7, PartyQualifierList.Consignor, consignorDetails);

			var notifyPartyDetails = shipment.GetNotifyPartyOrgDetails();
			if (!notifyPartyDetails.IsEmpty)
			{
				PopulateGroup7NAD(group7, PartyQualifierList.NotifyParty, notifyPartyDetails);
			}
		}

		void Group8GDSSegment(SegmentGroup8 group8)
		{
			GDSSegment gDS = group8.GDS.InstantiateAChildAndAddItToChildrenCollection();
			gDS.NatureOfCargo.NatureOfCargoCoded = NatureOfCargoCodedList.GetFromString("12");
		}

		void Group8FTXSegment(SegmentGroup8 group8, ForwardingShipment shipment)
		{
			var fTX = group8.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectQualifier = TextSubjectQualifierList.GoodsDescription;
			fTX.TextLiteral.FreeText1 = shipment.JS_GoodsDescription.Left(70);
		}

		void Group9PACSegment(SegmentGroup9 group9, ForwardingShipment shipment)
		{
			ZString marksAndNumbers = MarksAndNumbersNoteTextFromShipment(shipment);
			if (!marksAndNumbers.IsEmpty)
			{
				PACSegment pAC = group9.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pAC.NumberOfPackages = "";
				Group9PCISegment(group9, marksAndNumbers);
			}
		}

		void Group9PCISegment(SegmentGroup9 group9, ZString marksAndNumbers)
		{
			PCISegment pCI = group9.PCI.InstantiateAChildAndAddItToChildrenCollection();
			pCI.MarkingInstructionsCoded = MarkingInstructionsCodedList.MarkFreeText;
			PopulateLabels(pCI.MarksLabels, marksAndNumbers);
		}

		void Group11MOASegment(SegmentGroup11 group11, ForwardingShipment shipment)
		{
			ExportAWBHeader airWayBill;
			if (shipment.IsExport())
			{
				airWayBill = shipment.AWBHeader;
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.DeclaredTotalCustomsValue, MonetaryAmountTypeQualifierList.NoDeclaredValueForCustoms, airWayBill.EH_CustomsValue, airWayBill.EH_Currency);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.DeclaredValueForCarriage, MonetaryAmountTypeQualifierList.NoDeclaredValueForCarriage, airWayBill.EH_DeclaredValue, airWayBill.EH_Currency);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.Insurance, MonetaryAmountTypeQualifierList.NoAmountOfInsurance, airWayBill.EH_InsuranceValue, airWayBill.EH_Currency);
			}
			else
			{
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.DeclaredTotalCustomsValue, MonetaryAmountTypeQualifierList.NoDeclaredValueForCustoms, shipment.JS_GoodsValue, shipment.JS_RX_NKGoodsValueCurr);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.NoDeclaredValueForCarriage, "0.00", shipment.JS_RX_NKGoodsValueCurr);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.NoAmountOfInsurance, "0.00", shipment.JS_RX_NKGoodsValueCurr);
			}
			airWayBill = GetAirWaybill(shipment);
			if (airWayBill != null)
			{
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("331"), airWayBill.EH_TotalWeightCOL);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("332"), airWayBill.EH_TotalWeightPPD);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("333"), airWayBill.EH_OtherChargesDueAgentCOL + airWayBill.EH_OtherChargesDueCarrierCOL);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("334"), airWayBill.EH_OtherChargesDueAgentPPD + airWayBill.EH_OtherChargesDueCarrierPPD);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("335"), airWayBill.EH_ValuationCOL);
				FillMOASegment(group11, MonetaryAmountTypeQualifierList.GetFromString("336"), airWayBill.EH_ValuationPPD);
			}
		}

		void FillMOASegment(SegmentGroup11 group11, MonetaryAmountTypeQualifierList qualifier1, MonetaryAmountTypeQualifierList qualifier2, ZDecimal monetaryAmount, ZString currency)
		{
			FillMOASegment(group11, !monetaryAmount.IsEmpty ? qualifier1 : qualifier2, monetaryAmount.ToString(2), currency);
		}

		void FillMOASegment(SegmentGroup11 group11, MonetaryAmountTypeQualifierList qualifier, ZString monetaryAmount, ZString currency)
		{
			MOASegment mOA = group11.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = monetaryAmount;
			mOA.MonetaryAmount.CurrencyCoded = currency;
		}

		void FillMOASegment(SegmentGroup11 group11, MonetaryAmountTypeQualifierList qualifier, ZDecimal valueToCheck)
		{
			if (!valueToCheck.IsEmpty)
			{
				MOASegment mOA = group11.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
			}
		}

		void Group14DOCSegment(SegmentGroup14 group14, ForwardingShipment shipment)
		{
			var traxonLicenses = shipment.GetTraxonLicenseNumbersFromShipment();

			foreach (ZString license in traxonLicenses)
			{
				if (!license.IsEmpty)
				{
					DOCSegment dOC = group14.DOC.InstantiateAChildAndAddItToChildrenCollection();
					dOC.DocumentMessageName.DocumentMessageNameCoded = shipment.IsExport() ? DocumentMessageNameCodedList.ExportLicence : DocumentMessageNameCodedList.ImportLicence;
					dOC.DocumentMessageName.DocumentMessageName = license.Trim();
				}
			}
		}

		string GetACINumbers(ForwardingShipment shipment)
		{
			var aciNumbersFromConsol = consol.Numbers.Cast<CusEntryNumber>()
				.Where(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt && c.CE_EntryType ==
					CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference && !c.CE_EntryNum.IsEmpty)
				.Select(c => c.CE_EntryNum).ToArray();
			var aciNumbersFromShipment = shipment.Numbers.Cast<CusEntryNumber>()
				.Where(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt && c.CE_EntryType ==
					CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference && !c.CE_EntryNum.IsEmpty)
				.Select(c => c.CE_EntryNum).ToArray();

			return string.Join(",", aciNumbersFromConsol.Concat(aciNumbersFromShipment));
		}

		void Group15FTXSegment(SegmentGroup7 group7, ForwardingShipment shipment)
		{
			const int maxFreeTextCount = 9;
			var goodsDescription = DetailedGoodsDescriptionFromShipment(shipment).TrimEnd();
			var aciNumbers = new ZString(GetACINumbers(shipment));
			var egConfiguredInRegistry = HKDataRegistry.Instance.SendOtherCustomsInformation.Value.Any(g => g == Core.Constants.CountryGuids.Egypt);
			var sendAciNumber = egConfiguredInRegistry && !string.IsNullOrWhiteSpace(aciNumbers);
			var segmentLoopCount = 0;
			if (!goodsDescription.IsEmpty)
			{
				var startIndex = 0;
				var maxSegmentLoop = maxFreeTextCount - (sendAciNumber ? 1 : 0);
				while (segmentLoopCount < maxSegmentLoop && startIndex < goodsDescription.Length)
				{
					var subGoodsDescription = goodsDescription.SubstringSafe(startIndex, 65);
					if (subGoodsDescription.IsEmpty)
					{
						break;
					}
					else
					{
						var ftxSegment = PopulateBaseDataOnFtx(group7, TextSubjectQualifierList.GoodsDescription);
						ftxSegment.TextLiteral.FreeText1 = subGoodsDescription;
						startIndex += subGoodsDescription.Length;

						segmentLoopCount++;
					}
				}
			}
			if (sendAciNumber)
			{
				var ftxSegment = PopulateBaseDataOnFtx(group7, TextSubjectQualifierList.GoodsDescription);
				ftxSegment.TextLiteral.FreeText1 = string.Concat(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference, " ", aciNumbers.SubstringSafe(0, 61));
			}

			var isAcas = shipment.SendACASForShipment();
			if (shipment.SendOtherInfoForShipment(consol) || isAcas)
			{
				if (HKDataRegistry.Instance.SendInfoFromShipments.Value)
				{
					PopulateOtherInfoFromShipment(group7, shipment, isAcas);
				}
				else if (shipment.AWBHeader is ExportAWBHeader airWayBill)
				{
					PopulateOtherInfoFromAWB(group7, airWayBill, isAcas);
				}
			}

			if (HKDataRegistry.Instance.SendHsCode.Value)
			{
				var ftxAddedCount = 0;
				var maxSegmentLoop = maxFreeTextCount - segmentLoopCount;
				if (maxSegmentLoop > 0)
				{
#if NETFRAMEWORK
					foreach (var packline in shipment.OuterPackLines.Select(x => x.JL_HarmonisedCode.SubstringSafe(0, 6)).Where(x => x != ZString.Empty).Chunk(8))
#else
					foreach (var packline in IEnumerableExtensions.Chunk(shipment.OuterPackLines.Select(x => x.JL_HarmonisedCode.SubstringSafe(0, 6)).Where(x => x != ZString.Empty), 8))
#endif
					{
						PopulateHarmonizedCode(group7, $"HSCODE/{ZString.Join("/", packline.ToArray())}"); // Message Prefix
						ftxAddedCount++;

						if (ftxAddedCount >= maxSegmentLoop)
						{
							break;
						}
					}
				}
			}
		}

		void PopulateOtherInfoFromAWB(SegmentGroup7 group7, ExportAWBHeader airWayBill, bool isAcas)
		{
			var consigneeCustomsInfo = new OtherCustomsInformation
			{
				CountryCode = airWayBill.EH_ConsigneeCountryCode,
				ContactPerson = airWayBill.EH_ConsigneeContactName,
				ContactCode = airWayBill.EH_ConsigneeContactCode,
				ContactNumber = airWayBill.EH_ConsigneeContactDetail,
				ContactEmail = isAcas ? airWayBill.EH_ConsigneeContactEmail : ZString.Empty,
				TraderNoType = airWayBill.EH_ConsigneeTraderNoType,
				TraderNo = airWayBill.EH_ConsigneeTraderNo,
				Identifier = InformationIdentifierList.Consignee
			};
			PopulateOtherCustomsInformation(group7, consigneeCustomsInfo);

			var consignorCustomsInfo = new OtherCustomsInformation
			{
				CountryCode = airWayBill.EH_ShipperCountryCode,
				ContactCode = airWayBill.EH_ShipperContactCode,
				ContactNumber = airWayBill.EH_ShipperContactDetail,
				ContactEmail = isAcas ? airWayBill.EH_ShipperContactEmail : ZString.Empty,
				TraderNoType = airWayBill.EH_ShipperTraderNoType,
				TraderNo = airWayBill.EH_ShipperTraderNo,
				Identifier = InformationIdentifierList.Shipper
			};
			PopulateOtherCustomsInformation(group7, consignorCustomsInfo);

			var notifyPartyCustomsInfo = new OtherCustomsInformation
			{
				CountryCode = airWayBill.EH_AlsoNotifyCountryCode,
				ContactPerson = airWayBill.EH_AlsoNotifyContactName,
				ContactCode = airWayBill.EH_AlsoNotifyContactCode,
				ContactNumber = airWayBill.EH_AlsoNotifyContactDetail,
				TraderNoType = airWayBill.EH_AlsoNotifyTraderNoType,
				TraderNo = airWayBill.EH_AlsoNotifyTraderNo,
				Identifier = InformationIdentifierList.AlsoNotify
			};
			PopulateOtherCustomsInformation(group7, notifyPartyCustomsInfo);

			if (isAcas)
			{
				var acasCountryHandler = airWayBill.GetACASCountryHandler();
				bool hasAccountHolderAndName = acasCountryHandler.GetCustomerAccountHolderAndName(out string accountHolder, out string accountName);
				bool hasAccountIssuerAndNumber = acasCountryHandler.GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber);

				if (hasAccountHolderAndName || hasAccountIssuerAndNumber)
				{
					var accountCustomsInfo = new OtherCustomsInformation
					{
						CountryCode = Constants.CountryCodes.UnitedStates,
						AccountHolder = accountHolder,
						AccountName = accountName,
						AccountIssuer = accountIssuer,
						AccountNumber = accountNumber,
						TraderNo = string.Empty,
						TraderNoType = string.Empty,
						Identifier = InformationIdentifierList.Customs
					};
					PopulateOtherCustomsInformation(group7, accountCustomsInfo);
				}
			}
		}

		void PopulateOtherInfoFromShipment(SegmentGroup7 group7, ForwardingShipment shipment, bool isAcas)
		{
			TraderRegistrationDetails registrationDetails;

			if (shipment.Consignee is OrgHeader consignee)
			{
				registrationDetails = new TraderRegistrationDetails(consignee);
				var consigneeDocumentaryAddress = shipment.ConsigneeDocumentaryAddress;

				var consigneeCustomsInfo = new OtherCustomsInformation
				{
					CountryCode = consignee.CountryCode,
					ContactPerson = consigneeDocumentaryAddress.E2_Contact,
					ContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE,
					ContactNumber = consigneeDocumentaryAddress.E2_Phone,
					ContactEmail = isAcas ? consigneeDocumentaryAddress.E2_Email : ZString.Empty,
					TraderNoType = registrationDetails.RegistrationType,
					TraderNo = registrationDetails.RegistrationNumber,
					Identifier = InformationIdentifierList.Consignee
				};
				PopulateOtherCustomsInformation(group7, consigneeCustomsInfo);
			}

			if (shipment.Consignor is OrgHeader consignor)
			{
				registrationDetails = new TraderRegistrationDetails(consignor);
				var consignorDocumentaryAddress = shipment.ConsignorDocumentaryAddress;
				var consignorCustomsInfo = new OtherCustomsInformation
				{
					CountryCode = consignor.CountryCode,
					ContactPerson = consignorDocumentaryAddress.E2_Contact,
					ContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE,
					ContactNumber = consignorDocumentaryAddress.E2_Phone,
					ContactEmail = isAcas ? consignorDocumentaryAddress.E2_Email : ZString.Empty,
					TraderNoType = registrationDetails.RegistrationType,
					TraderNo = registrationDetails.RegistrationNumber,
					Identifier = InformationIdentifierList.Shipper
				};
				PopulateOtherCustomsInformation(group7, consignorCustomsInfo);
			}

			if (shipment.NotifyParty is OrgHeader notifyParty)
			{
				registrationDetails = new TraderRegistrationDetails(notifyParty);
				var notifyPartyDocumentaryAddress = shipment.NotifyPartyDocumentaryAddress;

				var notifyPartyCustomsInfo = new OtherCustomsInformation
				{
					CountryCode = notifyParty.CountryCode,
					ContactPerson = notifyPartyDocumentaryAddress.E2_Contact,
					ContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE,
					ContactNumber = notifyPartyDocumentaryAddress.E2_Phone,
					TraderNoType = registrationDetails.RegistrationType,
					TraderNo = registrationDetails.RegistrationNumber,
					Identifier = InformationIdentifierList.AlsoNotify
				};
				PopulateOtherCustomsInformation(group7, notifyPartyCustomsInfo);
			}

			if (isAcas)
			{
				var acasCountryHandler = GetAirWaybill(shipment)?.GetACASCountryHandler();
				bool hasAccountHolderAndName = acasCountryHandler.GetCustomerAccountHolderAndName(out string accountHolder, out string accountName);
				bool hasAccountIssuerAndNumber = acasCountryHandler.GetCustomerAccountIssuerAndNumber(out string accountIssuer, out string accountNumber);

				if (hasAccountHolderAndName || hasAccountIssuerAndNumber)
				{
					var accountCustomsInfo = new OtherCustomsInformation
					{
						CountryCode = Constants.CountryCodes.UnitedStates,
						AccountHolder = accountHolder,
						AccountName = accountName,
						AccountIssuer = accountIssuer,
						AccountNumber = accountNumber,
						TraderNo = string.Empty,
						TraderNoType = string.Empty,
						Identifier = InformationIdentifierList.Customs
					};
					PopulateOtherCustomsInformation(group7, accountCustomsInfo);
				}
			}
		}

		class TraderRegistrationDetails
		{
			public TraderRegistrationDetails(OrgHeader trader)
			{
				GetRelevantTraderRegistrationDetails(trader);
			}

			#region RegistrationNumber

			public ZString RegistrationNumber
			{
				get { return fregistrationNumber; }
				set { fregistrationNumber = value; }
			}
			ZString fregistrationNumber;

			#endregion

			#region RegistrationType

			public ZString RegistrationType
			{
				get { return fregistrationType; }
				set { fregistrationType = value; }
			}
			ZString fregistrationType;

			#endregion

			void GetRelevantTraderRegistrationDetails(OrgHeader trader)
			{
				RegistrationNumber = ZString.Empty;
				RegistrationType = ZString.Empty;
				if (trader != null)
				{
					var eORICode = trader.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
					if (eORICode.IsEmpty)
					{
						eORICode = trader.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, trader.CountryCode);
					}

					if (!eORICode.IsEmpty)
					{
						var companyPrefix = eORICode.SubstringSafe(0, 2);
						if (!companyPrefix.IsLettersOnlyOrEmpty)
						{
							eORICode = trader.CountryCode + eORICode;
						}

						RegistrationNumber = eORICode;
						RegistrationType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
					}
					else
					{
						var aEOCode = trader.CustomsCodes.GetCustomsRegNo(OrgCusCode.HKCodeTypes.AEO);
						if (aEOCode.IsEmpty)
						{
							aEOCode = trader.CustomsCodes.GetCustomsRegNo(OrgCusCode.HKCodeTypes.AEO, trader.CountryCode);
						}

						if (!aEOCode.IsEmpty)
						{
							RegistrationNumber = aEOCode;
							RegistrationType = OrgCusCode.HKCodeTypes.AEO;
						}
						else if (trader.PrimaryRegistrationNumber != null && !trader.PrimaryRegistrationNumber.Number.IsEmpty)
						{
							RegistrationNumber = trader.PrimaryRegistrationNumber.Number;
							RegistrationType = trader.PrimaryRegistrationNumber.NumberTypeForDisplay;
						}
					}
				}
			}
		}

		FTXSegment PopulateBaseDataOnFtx(SegmentGroup7 group7, TextSubjectQualifierList textSubjectQualifier)
		{
			var group15 = group7.Group15.InstantiateAChildAndAddItToChildrenCollection();
			group15.CST.InstantiateAChildAndAddItToChildrenCollection();

			var ftxSegment = group15.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftxSegment.TextSubjectQualifier = textSubjectQualifier;
			ftxSegment.TextFunctionCoded = TextFunctionCodedList.TextForSubsequentUse;

			return ftxSegment;
		}

		void PopulateOtherCustomsInformation(SegmentGroup7 group7, OtherCustomsInformation info)
		{
			void PopulateFtxSegment(string freeText1, string freeText2, string freeText3, string freeText4)
			{
				if (string.IsNullOrWhiteSpace(freeText1))
				{
					return;
				}
#if NETFRAMEWORK
				var freeText1List = HKDataRegistry.Instance.TruncateOtherCustomsInformation.Value ?
					[freeText1.Length > 35 ? freeText1.Substring(0, 35) : freeText1] : freeText1.Chunk(35).Select(chars => new string(chars.ToArray()));
#else
				var freeText1List = HKDataRegistry.Instance.TruncateOtherCustomsInformation.Value ?
					[freeText1.Length > 35 ? freeText1.Substring(0, 35) : freeText1] : IEnumerableExtensions.Chunk(freeText1, 35).Select(chars => new string(chars.ToArray()));
#endif
				foreach (var ftx1 in freeText1List)
				{
					var segment = PopulateBaseDataOnFtx(group7, TextSubjectQualifierList.CustomsDeclarationInformation);
					segment.TextLiteral.FreeText1 = ftx1;
					segment.TextLiteral.FreeText2 = freeText2;
					segment.TextLiteral.FreeText3 = freeText3;
					segment.TextLiteral.FreeText4 = freeText4;
				}
			}

			void PopulateDetailInfo()
			{
				PopulateFtxSegment(info.ContactPerson, info.CountryCode, info.Identifier, SecurityAndInformationIdentifierList.ContactPerson);

				info.ContactNumber = FormatPhoneNumber(info.ContactNumber);
				if (!info.ContactNumber.IsEmpty)
				{
					if (info.ContactCode.IsEmpty)
					{
						info.ContactCode = Core.Constants.AWB.ContactCodes.TELEPHONE;
					}
					info.ContactNumber = info.ContactCode.PadRight(3, ' ') + info.ContactNumber;
					PopulateFtxSegment(info.ContactNumber, info.CountryCode, info.Identifier, SecurityAndInformationIdentifierList.ContactNumber);
				}

				var emailParts = info.ContactEmail.Split("@");
				if (emailParts.Length > 1)
				{
					var userPart = emailParts[0];
					var domainPart = emailParts[1];
					if (!userPart.IsEmpty && !domainPart.IsEmpty)
					{
						PopulateFtxSegment(userPart, Constants.CountryCodes.UnitedStates, info.Identifier, "MU");
						PopulateFtxSegment(domainPart, Constants.CountryCodes.UnitedStates, info.Identifier, "MD");
					}
				}

				if (!string.IsNullOrWhiteSpace(info.AccountHolder))
				{
					PopulateFtxSegment(info.AccountHolder, info.CountryCode, info.Identifier, "AH");
				}

				if (!string.IsNullOrWhiteSpace(info.AccountName))
				{
					PopulateFtxSegment(info.AccountName, info.CountryCode, info.Identifier, "AN");
				}

				if (!string.IsNullOrWhiteSpace(info.AccountNumber))
				{
					if (!string.IsNullOrWhiteSpace(info.AccountIssuer))
					{
						PopulateFtxSegment(info.AccountIssuer, info.CountryCode, info.Identifier, "AI");
					}
					PopulateFtxSegment(info.AccountNumber, info.CountryCode, info.Identifier, "AR");
				}
			}

			void PopulateRegistryNumber()
			{
				ZString eORICode = info.TraderNo;
				var ftxCodeType = info.TraderNoType + eORICode;
				if (info.TraderNoType == "EORI")
				{
					var countryPrefix = eORICode.SubstringSafe(0, 2);
					if (eORICode.Length <= 2 || !countryPrefix.IsLettersOnlyOrEmpty)
					{
						eORICode = info.CountryCode + eORICode;
					}
					else
					{
						info.CountryCode = countryPrefix;
					}

					ftxCodeType = eORICode;
				}
				PopulateFtxSegment(ftxCodeType, info.CountryCode, info.Identifier, SecurityAndInformationIdentifierList.TraderIdentification);
			}

			info.TraderNoType = GetProperTraderNoType(info.TraderNoType);
			var outputRegistryInfo = !info.TraderNo.IsNullOrEmpty();
			var outputRegistryInfoOnly = outputRegistryInfo && info.TraderNoType == "EORI";

			if (!outputRegistryInfoOnly)
			{
				PopulateDetailInfo();
			}

			if (outputRegistryInfo)
			{
				PopulateRegistryNumber();
			}
		}

		void PopulateHarmonizedCode(SegmentGroup7 group7, string harmonizedTariffCode)
		{
			var segment = PopulateBaseDataOnFtx(group7, TextSubjectQualifierList.GoodsDescription);
			segment.TextLiteral.FreeText1 = harmonizedTariffCode;
		}

		string GetProperTraderNoType(string traderNoType)
		{
			var result = traderNoType.ToUpper(CultureInfo.CurrentCulture);

			switch (result)
			{
				case OrgCusCode.EuropeanUnionSharedCodeTypes.Eori:
				case "EORI NO.":
					{
						result = "EORI";
						break;
					}

				case "PAS":
					{
						result = "PASSPORT";
						break;
					}

				case "888":
					{
						result = "8888";
						break;
					}

				case "999":
					{
						result = "9999";
						break;
					}

				case "USC":
					{
						result = "USCI";
						break;
					}

				case "ID":
				case "OC":
					{
						break;
					}

				default:
					{
						result = traderNoType;
						break;
					}
			}

			return result;
		}

		void UNTSegment()
		{
			UNTSegment uNT = message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.NumberOfSegmentsInTheMessage = message.CountIncludingUNT.ToString();
			uNT.MessageReferenceNumber = ConsolReferenceNumber;
		}

		ZString MarksAndNumbersNoteTextFromShipment(ForwardingShipment shipment)
		{
			StringBuilder resultBuilder = new StringBuilder();
			if (shipment != null)
			{
				StmNote[] notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				if (notes.Length > 0)
				{
					foreach (StmNote note in notes)
					{
						resultBuilder.Append(note.ST_NoteDataAsText);
					}
				}
			}
			return resultBuilder.ToString();
		}

		ZString DetailedGoodsDescriptionFromShipment(ForwardingShipment shipment)
		{
			var goodsDescription = ZString.Empty;

			if (shipment != null)
			{
				var notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
				goodsDescription = notes != null && notes.Any() ? notes.FirstOrDefault().ST_NoteDataAsText : ZString.Empty;
			}

			return goodsDescription;
		}

		void PopulateLabels(MarksLabelsElements labels, ZString marksAndNumbers)
		{
			labels.ShippingMarks1 = marksAndNumbers.Left(35);
			labels.ShippingMarks2 = marksAndNumbers.SubstringSafe(35, 35);
			labels.ShippingMarks3 = marksAndNumbers.SubstringSafe(70, 35);
			labels.ShippingMarks4 = marksAndNumbers.SubstringSafe(105, 35);
			labels.ShippingMarks5 = marksAndNumbers.SubstringSafe(140, 35);
			labels.ShippingMarks6 = marksAndNumbers.SubstringSafe(175, 35);
			labels.ShippingMarks7 = marksAndNumbers.SubstringSafe(210, 35);
			labels.ShippingMarks8 = marksAndNumbers.SubstringSafe(245, 35);
			labels.ShippingMarks9 = marksAndNumbers.SubstringSafe(280, 35);
			labels.ShippingMarks10 = marksAndNumbers.SubstringSafe(315, 35);
		}

		ExportAWBHeader GetAirWaybill(ForwardingShipment shipment)
		{
			return shipment.AWBHeader ?? consol.AWBHeader;
		}

		ZString GetContactDetails(ForwardingShipment shipment, OrganisationTypes organisationType)
		{
			var result = ZString.Empty;

			var airWayBill = GetAirWaybill(shipment);
			if (airWayBill != null)
			{
				ZString contactCode;
				ZString contactDetail;

				switch (organisationType)
				{
					case OrganisationTypes.Consignee:
						contactCode = airWayBill.EH_ConsigneeContactCode;
						contactDetail = airWayBill.EH_ConsigneeContactDetail;
						break;
					case OrganisationTypes.Consignor:
						contactCode = airWayBill.EH_ShipperContactCode;
						contactDetail = airWayBill.EH_ShipperContactDetail;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(organisationType), "Organisation type must be Consignee or Consignor");
				}

				if (!contactCode.IsEmpty && !contactDetail.IsEmpty)
				{
					result = contactCode.PadRight(3, ' ') + FormatPhoneNumber(contactDetail);
				}
			}

			return result;
		}

		ZString FormatPhoneNumber(ZString phoneNumber) => phoneNumber.KeepChars("0123456789").Left(25);

		#endregion
	}
}
