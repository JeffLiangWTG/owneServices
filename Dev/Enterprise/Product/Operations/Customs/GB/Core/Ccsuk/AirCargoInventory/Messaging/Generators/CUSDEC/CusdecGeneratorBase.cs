using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSDEC;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSDEC_2_912
{
	public abstract class CusdecGeneratorBase : CusAwbToInventoryMessageGenerator
	{
		public CusdecGeneratorBase(ICcsukCusAwb awb, ErrorCollector ec, CusUnderbond underbond)
			: base(awb)
		{
			this.underbond = underbond;
			errorCollector = ec;
			iCuscar = awb.GetCuscarWrapper();
			if (awb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights && !IsConsignmentManifestedOnMultipleFlightsAllowed(awb))
			{
				errorCollector.AddError(string.Format("{0} {1} - The message cannot be created beause this bill has SDC=M but lacks status 1 and lacks splits.", awb.ReferenceNumber, (underbond == null ? ZString.Empty : underbond.SplitReferenceToWhichThisRemovalPertains)), null);
			}
		}

		bool IsConsignmentManifestedOnMultipleFlightsAllowed(ICcsukCusAwb awb)
		{
			return !awb.Status1Date.IsEmpty || (awb.HasSplits && !underbond.SplitReferenceToWhichThisRemovalPertains.IsEmpty);
		}

		public override string MakeMessageText()
		{
			if (ThisProfileCanMakeThisMessage)
			{
				result = new CUSDECMessage();
				MakeUNH();
				MakeBGM();
				MakeGroup1References();
				MakeLOC();
				MakeTDT();
				MakesGISforLicenceRestricted();
				MakeGroup6(); // ie. MOA
				MakeUnsAndCnt();
				MakeUNT();
				var charSet = new UkCharSet();
				return CUSCARGeneratorBase.ReplaceFakeSegmentNames(charSet, result.ToString(charSet));
			}
			else
			{
				errorCollector.AddError(string.Format("This profile/PIMA is not allowed to make {0} requests for {1}", underbond.RemovalTypeHuman, underbond.C4_SendersMessageReference), null);
				return "";
			}
		}

		protected virtual bool ThisProfileCanMakeThisMessage
		{
			get { return LicenceAndPimaHelper.IsSimpleAgentProfile(Awb); }
		}

		void MakeUnsAndCnt()
		{
			result.UNS1.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "D";
			result.UNS2.InstantiateAChildAndAddItToChildrenCollection().SectionIdentification = "S";
			var cnt = result.CNT.InstantiateAChildAndAddItToChildrenCollection();
			cnt.Control.ControlTotalTypeCodeQualifier = ControlTotalTypeCodeQualifierList.GetFromString("11");
			cnt.Control.ControlTotalValue = underbond.NoPackagesExpected.IsEmpty ? iCuscar.NumberOfPiecesExpected.ToString() : underbond.NoPackagesExpected.ToString();
		}

		protected virtual void MakeGroup6()
		{
		}

		void MakeGroup1References()
		{
			var grp1 = result.Group1.InstantiateAChildAndAddItToChildrenCollection();
			MakeGroup1ReferenceOnwardAwb(grp1);
			MakeGroup1ReferenceDeclarantsReference(grp1);
		}

		protected virtual void MakeGroup1ReferenceOnwardAwb(SegmentGroup1 grp1)
		{
		}

		protected virtual void MakeGroup1ReferenceDeclarantsReference(SegmentGroup1 grp1)
		{
			var referenceValue = underbond.AgentsReference;
			if (!referenceValue.IsEmpty)
			{
				var rffDeclarationDef = grp1.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rffDeclarationDef.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("ABE");
				rffDeclarationDef.Reference.ReferenceIdentifier = referenceValue;
			}
		}

		protected virtual void MakeUNH()
		{
			var unh = result.UNH.InstantiateAChildAndAddItToChildrenCollection();
			unh.MessageReferenceNumber = "<<MSGNO PLACEHOLDER>>";
			unh.MessageIdentifier.MessageType = "CUSDEC";
			unh.MessageIdentifier.MessageVersionNumber = "2";
			unh.MessageIdentifier.MessageReleaseNumber = "912";
			unh.MessageIdentifier.ControllingAgency = "UN";
			unh.MessageIdentifier.AssociationAssignedCode = AssociationAssignedCode;
			unh.CommonAccessReference = GbTransmissionMessageGenerator.SysCarPlaceHolder;
		}

		protected virtual void MakeBGM()
		{
			var bgm = result.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bgm.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(BgmDocumentName);
			bgm.DocumentMessageIdentification.DocumentIdentifier = iCuscar.AirlinePrefix + iCuscar.AirWaybillSerialNumber;
			if (!underbond.SplitReferenceToWhichThisRemovalPertains.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("ACD");
				bgm.ReferenceC506.ReferenceIdentifier = iCuscar.HouseAirWaybillNumber;
				bgm.ReferenceC506.DocumentLineIdentifier = underbond.SplitReferenceToWhichThisRemovalPertains;
			}
			if (!iCuscar.HouseAirWaybillNumber.IsEmpty)
			{
				bgm.ReferenceC506.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.GetFromString("HWB");
				bgm.ReferenceC506.ReferenceIdentifier = iCuscar.HouseAirWaybillNumber;
			}
		}

		protected virtual void MakesGISforLicenceRestricted()
		{ }

		protected void MakesGISforLicenceRestrictedCore(string indicatorValue)
		{
			switch (indicatorValue)
			{
				case Enterprise.Customs.Business.YesNoList.Codes.No:
					break;

				case Enterprise.Customs.Business.YesNoList.Codes.Yes:
					{
						var gisGeneralProcessor = result.GIS.InstantiateAChildAndAddItToChildrenCollection();
						gisGeneralProcessor.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("LIC");
						gisGeneralProcessor.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.GetFromString("ZZZ");
						gisGeneralProcessor.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.GetFromString("109");
						break;
					}

				default:
					errorCollector.AddError("LRI", new ErrorInfo("", "Mandatory"));
					break;
			}
		}

		protected virtual void MakeTDT()
		{
		}

		void MakeLOC()
		{
			var locationsToAdd = new List<LocationAndRelationsAsASingleElement>();

			if (!iCuscar.AirportOfArrival.IsEmpty)
			{
				var location11 = new LocationAndRelationsAsASingleElement();
				location11.PlaceLocationQualifier3227 = LocationTypes.Codes.AOA; //11
				location11.PlaceLocationIdentification3225 = iCuscar.AirportOfArrival;
				location11.CodeListQualifier1131_1 = "145";
				location11.CodeListResponsibleAgencyCoded3055_1 = "3";
				location11.SubLocationIdentification3439 = iCuscar.CargoTerminalOperator;
				location11.CodeListQualifier1131_2 = "129";
				location11.CodeListResponsibleAgencyCoded3055_2 = "ZZZ";
				locationsToAdd.Add(location11);
			}

			if (underbond.AirportOrCountryOfDestination.Length == 3 || underbond.AirportOrCountryOfDestination.Length == 5)
			{
				var location85 = MakeLOC85DestinationAirport();
				if (location85 != null)
				{
					locationsToAdd.Add(location85);
				}
			}

			var location28 = MakeLOC28DestinationCountry();
			if (location28 != null)
			{
				locationsToAdd.Add(location28);
			}

			var location5 = MakeLOC5PortOfShipment();
			if (location5 != null)
			{
				locationsToAdd.Add(location5);
			}

			if (locationsToAdd.Count > 0)
			{
				var loc = result.LOC.InstantiateAChildAndAddItToChildrenCollection();
				int i = 0;
				foreach (var location in locationsToAdd)
				{
					i++;
					switch (i)
					{
						case 1:
							loc.Location1 = location;
							break;
						case 2:
							loc.Location2 = location;
							break;
						case 3:
							loc.Location3 = location;
							break;
						case 4:
							loc.Location4 = location;
							break;
					}
				}
			}
		}

		protected virtual LocationAndRelationsAsASingleElement MakeLOC5PortOfShipment()
		{
			return null;
		}

		protected virtual LocationAndRelationsAsASingleElement MakeLOC28DestinationCountry()
		{
			return null;
		}

		protected virtual LocationAndRelationsAsASingleElement MakeLOC85DestinationAirport()
		{
			if (underbond.AirportOrCountryOfDestination.IsEmpty)
			{
				errorCollector.AddError("Destination airport required", new ErrorInfo("", "mandatory"));
				return null;
			}
			else
			{
				var location = new LocationAndRelationsAsASingleElement();
				location.PlaceLocationQualifier3227 = LocationTypes.Codes.AOD;  //85
				location.PlaceLocationIdentification3225 = underbond.AirportOrCountryOfDestination_IATA;
				location.CodeListQualifier1131_1 = "145";
				location.CodeListResponsibleAgencyCoded3055_1 = "3";
				AddShedToLOC85DestinationAirport(location);
				return location;
			}
		}

		protected virtual void AddShedToLOC85DestinationAirport(LocationAndRelationsAsASingleElement parentLocation)
		{
		}

		protected void MakeUNT()
		{
			var unt = result.UNT.InstantiateAChildAndAddItToChildrenCollection();
			unt.NumberOfSegmentsInTheMessage = result.CountIncludingUNT.ToString();
			unt.MessageReferenceNumber = "<<MSGNO PLACEHOLDER>>";
		}

		/// <summary>
		///  e.g. 109503
		/// </summary>
		protected abstract string AssociationAssignedCode { get; }

		/// <summary>
		/// e.g. FRI, FRX, etc
		/// </summary>
		protected abstract string BgmDocumentName { get; }

		public override ZString MessageInterpretation
		{
			get
			{
				return string.Format(
						@"{0}
						<h3>{1} Request ({2})</h3>							
							<h4>{3}</h4>
							<p>
								{4}
							</p>
						",
						 MessagePrettierCss.CSS,
						 underbond.RemovalTypeHuman, BgmDocumentName,
						 underbond.Awb.ReferenceNumber,
						 underbond.ToString()
						 );
			}
		}

		protected ErrorCollector errorCollector;
		protected CUSDECMessage result;
		protected readonly ICuscar iCuscar;
		protected CusUnderbond underbond;
	}
}
