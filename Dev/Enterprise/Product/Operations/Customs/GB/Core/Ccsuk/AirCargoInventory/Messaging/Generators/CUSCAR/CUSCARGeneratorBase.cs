using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	abstract class CUSCARGeneratorBase : CusAwbToInventoryMessageGenerator
	{
		public CUSCARGeneratorBase(ICcsukCusAwb awb, ErrorCollector ec)
			: base(awb)
		{
			errorCollector = ec;
			awb.CalculateNprFromReceiptsIfNecessary();
			iCuscar = awb.GetCuscarWrapper();
			result = new CUSCARMessage();
			awbReferenceNumber = awb.ReferenceNumber;
			allowVersion3CusCar = GBCustomsDataRegistry.Instance.CcsukAllowVersionThreeCuscarForSpecialHandling.Value;
		}

		public override string MakeMessageText()
		{
			MakeUNH();
			MakeBGM();
			result.Group1.InstantiateAChildAndAddItToChildrenCollection();
			MakesGISes();
			MakeGroup2TDTandLOC();
			MakeGroup3NAD();
			MakeGroup5Lines();
			MakeUNT();

			var charSet = new UkCharSet();
			return ReplaceFakeSegmentNames(charSet, result.ToString(charSet));
		}

		protected void DemandFieldsNotEmpty(params ZString[] parameters)
		{
			for (var i = 0; i < parameters.Length; i += 2)
			{
				if (parameters[i + 1].IsEmpty)
				{
					errorCollector.AddError(parameters[i], new ErrorInfo("", "mandatory"));
				}
			}
		}

		public static string ReplaceFakeSegmentNames(UNCharacterSet charSet, string edifact)
		{
			foreach (var fakey in fakeSegmentsToReplace)
			{
				edifact = edifact.Replace(
											charSet.SegmentDelimiter + fakey + charSet.ElementDelimiter,                        // e.g. 'BGM-CCSUK+
											charSet.SegmentDelimiter + fakey.Replace("-CCSUK", "") + charSet.ElementDelimiter   // e.g. 'BGM+
											);
			}
			return edifact;
		}

		public static string RestoreFakeSegmentNames(UNCharacterSet charSet, string edifact)
		{
			foreach (var fakey in fakeSegmentsToReplace)
			{
				edifact = edifact.Replace(
											charSet.SegmentDelimiter + fakey.Replace("-CCSUK", "") + charSet.ElementDelimiter,                        // e.g. 'BGM+
											charSet.SegmentDelimiter + fakey + charSet.ElementDelimiter   // e.g. 'BGM-CCSUK+
											);
			}
			return edifact;
		}
		static readonly List<string> fakeSegmentsToReplace = new List<string> { BGMSegmentWithReferenceAndDates.SegmentFakeName, TDTSegmentWithDatetime.SegmentFakeName, LOCSegmentWithLotsOfLocations.SegmentFakeName, DOCSegmentWith8Elements.SegmentFakeName, RFFSegmentWithDatetime.SegmentFakeName, MOASegmentWithFunction.SegmentFakeName };

		protected virtual void MakeUNH()
		{
			var unh = result.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = "<<MSGNO PLACEHOLDER>>";
			unh.MessageIdentifier.MessageType = "CUSCAR";
			unh.MessageIdentifier.MessageVersionNumber = GetCuscarMessageVersionTwoOrThree();
			unh.MessageIdentifier.MessageReleaseNumber = "912";
			unh.MessageIdentifier.ControllingAgency = "UN";
			unh.MessageIdentifier.AssociationAssignedCode = AssociationAssignedCode;
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;
		}

		protected abstract string GetCuscarMessageVersionTwoOrThree();

		protected virtual void MakeBGM()
		{
			var bgm = result.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentName = BgmDocumentName;
			bgm.DocumentMessageIdentification.DocumentIdentifier = iCuscar.AirlinePrefix + iCuscar.AirWaybillSerialNumber;
			if (!iCuscar.SplitReference.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("ACD");
				bgm.ReferenceC506.ReferenceIdentifier = iCuscar.HouseAirWaybillNumber;
				bgm.ReferenceC506.DocumentLineIdentifier = iCuscar.SplitReference;
			}
			if (!iCuscar.HouseAirWaybillNumber.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("HWB");
				bgm.ReferenceC506.ReferenceIdentifier = iCuscar.HouseAirWaybillNumber;
			}
			if (!iCuscar.Status1Date.IsEmpty && GBCustomsDataRegistry.Instance.CcsukAllowStatus1DateInFrc.Value)
			{
				bgm.DateTimeC507_2.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("50");
				bgm.DateTimeC507_2.DateOrTimeOrPeriodValue = iCuscar.Status1Date.ToString("yyMMddHHmm");
				bgm.DateTimeC507_2.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("201");
			}
		}

		protected virtual void MakesGISes()
		{
			MakeGISStatusTwo();
			MakeGISShipmentDescription(iCuscar.ShipmentDescriptionCode);
			if (allowVersion3CusCar)
			{
				MakeCommunityHandlingCodes();
			}
		}

		protected virtual void MakeCommunityHandlingCodes()
		{
		}

		protected virtual void MakeGISStatusTwo()
		{
			var gisShipDesc = result.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gisShipDesc.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("S2" + (iCuscar.Status2Indicator ? "Y" : "N"));
		}

		protected virtual void MakeGISShipmentDescription(ZString shipmentDescriptionCode)
		{
			var gisStatusTwo = result.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gisStatusTwo.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(shipmentDescriptionCode);  // Shipment description code
			gisStatusTwo.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.GetFromString("121");
		}

		protected virtual void MakeGroup2TDTandLOC()
		{
			var grp2 = result.Group2.InstantiateAChildAndAddItToChildrenCollection();
			var tdt = grp2.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdt.TransportStageQualifier = TransportStageCodeQualifierList.GetFromString("20");
			Group2Tdt20PutInFullTransportDetails(tdt);
			MakeLOCForAirports(grp2);
		}

		protected virtual void MakeLOCForAirports(CUSCARSegmentGroup2 grp2)
		{
			MakeLOC(grp2.LOC.InstantiateAChildAndAddItToChildrenCollection(), iCuscar.AirportOfArrival, iCuscar.CargoTerminalOperator, iCuscar.AirportOfOrigin, iCuscar.AirportOfDestination);
		}

		protected virtual void Group2Tdt20PutInFullTransportDetails(TDTSegmentWithDatetime tdt)
		{
			tdt.ConveyanceReferenceNumber = iCuscar.FlightNumber;
			tdt.Carrier.CarrierIdentifier = iCuscar.CarrierCode;
			tdt.Carrier.CodeListIdentificationCode = CodeListIdentificationCodeList.Carriers;  // 172
			tdt.Carrier.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;  // IATA=3
			if (!iCuscar.DateOfFlightArrival.IsEmpty)
			{
				tdt.DateTimeC507.DateOrTimeOrPeriodValue = iCuscar.DateOfFlightArrival.ToString("yyMMdd");
				tdt.DateTimeC507.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString("178");
				tdt.DateTimeC507.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString("101");
			}
		}

		protected virtual void MakeLOC(LOCSegmentWithLotsOfLocations lOCSegmentWithLotsOfLocations, string arrivalAirport, string shed, ZString originAirport, ZString destinationAirport)
		{
			MakeLOCAndAddType11ForAOA(lOCSegmentWithLotsOfLocations, arrivalAirport, shed);
			MakeLoc84ForAOO(lOCSegmentWithLotsOfLocations, originAirport);
			MakeLoc85ForAOD(lOCSegmentWithLotsOfLocations, destinationAirport);
		}

		public static void MakeLOCAndAddType11ForAOA(LOCSegmentWithLotsOfLocations lOCSegmentWithLotsOfLocations, string arrivalAirport, string shed)
		{
			lOCSegmentWithLotsOfLocations.Location1.PlaceLocationQualifier3227 = LocationTypes.Codes.AOA;
			lOCSegmentWithLotsOfLocations.Location1.PlaceLocationIdentification3225 = arrivalAirport;
			lOCSegmentWithLotsOfLocations.Location1.CodeListQualifier1131_1 = "145";
			lOCSegmentWithLotsOfLocations.Location1.CodeListResponsibleAgencyCoded3055_1 = "3";
			lOCSegmentWithLotsOfLocations.Location1.SubLocationIdentification3439 = shed;
			lOCSegmentWithLotsOfLocations.Location1.CodeListQualifier1131_2 = "129";
			lOCSegmentWithLotsOfLocations.Location1.CodeListResponsibleAgencyCoded3055_2 = "ZZZ";
		}

		protected virtual void MakeLoc84ForAOO(LOCSegmentWithLotsOfLocations lOCSegmentWithLotsOfLocations, ZString originAirport)
		{
			if (!originAirport.IsEmpty)
			{
				lOCSegmentWithLotsOfLocations.Location2.PlaceLocationQualifier3227 = LocationTypes.Codes.AOO;
				lOCSegmentWithLotsOfLocations.Location2.PlaceLocationIdentification3225 = originAirport;
				lOCSegmentWithLotsOfLocations.Location2.CodeListQualifier1131_1 = "145";
				lOCSegmentWithLotsOfLocations.Location2.CodeListResponsibleAgencyCoded3055_1 = "3";
			}
		}

		protected virtual void MakeLoc85ForAOD(LOCSegmentWithLotsOfLocations lOCSegmentWithLotsOfLocations, ZString destinationAirport)
		{
			if (!destinationAirport.IsEmpty)
			{
				lOCSegmentWithLotsOfLocations.Location3.PlaceLocationQualifier3227 = LocationTypes.Codes.AOD;
				lOCSegmentWithLotsOfLocations.Location3.PlaceLocationIdentification3225 = destinationAirport;
				lOCSegmentWithLotsOfLocations.Location3.CodeListQualifier1131_1 = "145";
				lOCSegmentWithLotsOfLocations.Location3.CodeListResponsibleAgencyCoded3055_1 = "3";
			}
		}

		protected virtual void MakeGroup3NAD()
		{
			var grp3 = result.Group3.InstantiateAChildAndAddItToChildrenCollection();
			MakeGroup3NAD_CustomsBroker(grp3);
		}

		protected virtual void MakeGroup5Lines()
		{
			// NB Air Cargo House doesn't support multi-line houses.  So we will have just one group 5. 
			var grp5 = result.Group5.InstantiateAChildAndAddItToChildrenCollection();
			MakeGroup5GID("0", grp5);
			MakeGroup5FTXDescOfGoods(grp5);
			MakeGroup6QTYNumberOfPieces(grp5, iCuscar.NumberOfPiecesExpected, iCuscar.NumberOfPiecesReceived);
			MakeGroup5MEAWeight(grp5, iCuscar.WeightCode, iCuscar.Weight);
		}

		protected void MakeUNT()
		{
			var unt = result.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = result.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = "<<MSGNO PLACEHOLDER>>";
		}

		protected virtual void MakeGroup5MEAWeight(CUSCARSegmentGroup5 grp5, string weightCode, ZDecimal weight)
		{
			var mea = grp5.MEA.InstantiateAChildAndAddItToChildrenCollection();
			mea.MeasurementAttributeCode = MeasurementAttributeCodeList.GetFromString("WT");
			var zWeight = new ZWeight(weight, weightCode.StartsWith("K") ? Constants.Weight.Kilograms : weightCode);
			mea.ValueRange.MeasurementUnitCode = "KGM";
			if (zWeight.IsValid)
			{
				mea.ValueRange.MeasurementValue = zWeight.InKilograms > 9999m ? zWeight.InKilograms.ToStringTrimZeros("######0") : zWeight.InKilograms.ToStringTrimZeros("###0.##");
			}
			else
			{
				errorCollector.AddError("weight unit", new ErrorInfo("", "mandatory"));
			}
		}

		protected void MakeGroup6QTYNumberOfPieces(CUSCARSegmentGroup5 grp5, ZInt numberOfPiecesExpected, ZInt numberOfPiecesReceived)
		{
			if (numberOfPiecesExpected + numberOfPiecesReceived > 0)
			{
				var grp6 = grp5.Group6.InstantiateAChildAndAddItToChildrenCollection();
				if (numberOfPiecesExpected > 0)
				{
					MakeGroup5Qty(grp6, "118", numberOfPiecesExpected);
				}
				if (AlwaysSendNprEvenIfZero || numberOfPiecesReceived > 0)
				{
					MakeGroup5Qty(grp6, "48", numberOfPiecesReceived);
				}
			}
			else
			{
				errorCollector.AddError("At least one of NPX and NPR must be present", null);
			}
		}

		protected virtual bool AlwaysSendNprEvenIfZero
		{
			get { return false; }
		}

		protected void MakeGroup6QTYNumberOfPieces(CUSCARSegmentGroup5 grp5, ZInt numberOfPiecesExpected)
		{
			var grp6 = grp5.Group6.InstantiateAChildAndAddItToChildrenCollection();
			MakeGroup5Qty(grp6, "118", numberOfPiecesExpected);
		}

		void MakeGroup5Qty(CUSCARSegmentGroup6 grp6, string type, int numberOfPieces)
		{
			var qty = grp6.QTY.InstantiateAChildAndAddItToChildrenCollection();
			qty.QuantityDetails.QuantityTypeCodeQualifier = QuantityTypeCodeQualifierList.GetFromString(type);
			qty.QuantityDetails.Quantity = numberOfPieces.ToString();
		}

		protected virtual void MakeGroup5FTXDescOfGoods(CUSCARSegmentGroup5 grp5)
		{
			var ftx = grp5.FTX.InstantiateAChildAndAddItToChildrenCollection();
			ftx.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString("AAA");
			ftx.TextLiteral.FreeTextValue1 = iCuscar.DescriptionOfGoods.Left(70);
		}

		protected void MakeGroup5GID(ZString lineNumber, CUSCARSegmentGroup5 grp5)
		{
			var gid = grp5.GID.InstantiateAChildAndAddItToChildrenCollection();
			gid.GoodsItemNumber = lineNumber;
		}

		void MakeGroup3NAD_CustomsBroker(CUSCARSegmentGroup3 grp3)
		{
			if (!iCuscar.AgentBrokerConsolidatorCode.IsEmpty)
			{
				var nadCB = grp3.NAD.InstantiateAChildAndAddItToChildrenCollection();
				nadCB.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString("CB");
				nadCB.PartyIdentificationDetails.PartyIdentifier = iCuscar.AgentBrokerConsolidatorCode;
			}
		}

		/// <summary>
		///  e.g. 109503
		/// </summary>
		protected abstract string AssociationAssignedCode { get; }

		/// <summary>
		/// e.g. FRI, FRX, etc
		/// </summary>
		protected abstract string BgmDocumentName { get; }

		protected abstract string BgmDocumentNameHuman { get; }

		protected virtual ZString GetHtmlInterpretationForFieldsFromCusCarEdifact()
		{
			var flagableCuscarFromOutboundMessage = new CuscarInboundParser(result).ParseCuscarForListOfUpdatedFields();
			Dictionary<ZPropertyInfo, IZType> listOfFieldsInMessage = null;
			if (Awb is CusHAWB)
			{
				listOfFieldsInMessage = CuscarFrcConsignmentUpdater.GetHawbMembers((CusHAWB)Awb, flagableCuscarFromOutboundMessage, true);
			}
			else if (Awb is CusMAWB)
			{
				listOfFieldsInMessage = CuscarFrcConsignmentUpdater.GetMawbMembers((CusMAWB)Awb, flagableCuscarFromOutboundMessage, true);
			}
			else if (Awb is SplitHouse)
			{
				var s = Awb as SplitHouse;
				listOfFieldsInMessage = CuscarFrcConsignmentUpdater.GetHawbMembers(s.HAWB, flagableCuscarFromOutboundMessage, true);
			}
			else if (Awb is SplitBasic)
			{
				var s = Awb as SplitBasic;
				listOfFieldsInMessage = CuscarFrcConsignmentUpdater.GetMawbMembers(s.Basic, flagableCuscarFromOutboundMessage, true);
			}

			var htmlValuesInterpretation = ZString.Empty;
			if (listOfFieldsInMessage != null)
			{
				htmlValuesInterpretation = CuscarFrcConsignmentUpdater.MakeHtmlTableFromListOfProperties(listOfFieldsInMessage, ColumnsToShowInInterpretation.FieldNameAndProcessingValue, flagableCuscarFromOutboundMessage.CommunityHandlingCodes);
			}
			return htmlValuesInterpretation;
		}

		public override ZString MessageInterpretation
		{
			get
			{
				var htmlValuesInterpretation = GetHtmlInterpretationForFieldsFromCusCarEdifact();

				return string.Format(
						@"{0}
						<h3>{1} {2}</h3>
							<p>
								Operation: {3}
							</p> 
							{4}
						", MessagePrettierCss.CSS, BgmDocumentNameHuman, awbReferenceNumber, BgmDocumentName, htmlValuesInterpretation);
			}
		}

		protected ErrorCollector errorCollector;
		protected readonly CUSCARMessage result;
		protected readonly ICuscar iCuscar;
		readonly ZString awbReferenceNumber;
		protected readonly bool allowVersion3CusCar;
	}
}
