using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class AWBRecordTestCase : JXCRecordTestCase
	{
		#region TestLoadOrCreateConsol
		public void TestLoadOrCreateConsol()
		{
			string masterBillNum = "0811234123";
			string consolFlightNo = "QF001";
			ZDateTime consolETD = new ZDateTime(2005, 11, 1);
			SetFieldValuesForTestLoadOrCreateConsol("081", "1234123", "QF001", consolETD);
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consolFromRecord = record.LoadOrCreateConsol(FactoryProvider);
			Assert("Consol could not be matched, should be creating a new one", consolFromRecord.JK_UniqueConsignRef != "101");
			AssertDefaultConsolValues(consolFromRecord);
			JASForwardingConsol consol = CreateConsolForTestLoadOrCreateConsol(masterBillNum, consolFlightNo, consolETD);
			consolFromRecord = record.LoadOrCreateConsol(FactoryProvider);
			AssertEquals("Consol should be matched", consol.PK, consolFromRecord.PK);
		}

		JASForwardingConsol CreateConsolForTestLoadOrCreateConsol(ZString masterBillNum, ZString flightNo, ZDateTime eTD)
		{
			JASForwardingConsol result = Factory.New<JASForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Air;
			result.JK_UniqueConsignRef = "101";
			result.JK_MasterBillNum = masterBillNum;
			Transport transport = result.Transports[0];
			transport.JW_VoyageFlight = flightNo;
			transport.JW_ETD = eTD;
			return result;
		}

		void SetFieldValuesForTestLoadOrCreateConsol(ZString airlinePrefix, ZString mAWBSerialNo, ZString flightNo, ZDateTime eTD)
		{
			Fields.SetFieldValue(ExpectedFieldPositions.AirlinePrefix, airlinePrefix);
			Fields.SetFieldValue(ExpectedFieldPositions.MAWBSerialNo, mAWBSerialNo);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, flightNo);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, eTD);
		}

		void AssertDefaultConsolValues(JASForwardingConsol consol)
		{
			Assert(!consol.JK_IsNeutralMaster);
			AssertEquals(ExpectedConsolAgentType, consol.JK_AgentType);
			AssertEquals(Core.Constants.TransportModes.Air, consol.JK_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Loose, consol.JK_ConsolMode);
			AssertEquals(Core.Constants.PaymentType.Prepaid, consol.JK_PrepaidCollect);
			Assert(consol.JK_OverrideWaybillDefaults);
		}

		#endregion
		#region TestUpdateConsol
		public void TestUpdateConsol()
		{
			SetFieldValuesForTestUpdateConsol();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			DataImportFlagChanger.LastBizO = null;
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertConsolPropertiesAfterUpdateConsol(record, consol);
			AssertEquals("Should use DataImportFlagChanger", consol, DataImportFlagChanger.LastBizO);
		}

		public void TestUpdateConsol_TransportETAsAreDeterminedByProximity()
		{
			SetFieldValuesForTestUpdateConsol_TransportETAsAreDeterminedByProximity();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertConsolProperties_TransportETAsAreDeterminedByProximity(consol);
		}

		public void TestUpdateConsol_TransportShouldBeLinkedIfThereIsOnlyOneLeg()
		{
			SetFieldValuesForTestUpdateConsol_TransportShouldBeLinkedIfThereIsOnlyOneLeg();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertConsolPropertiesAfterUpdateConsol_TransportShouldBeLinkedIfThereIsOnlyOneLeg(consol);
		}

		[ExpectNoExceptions]
		public void TestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
		}

		protected virtual void SetFieldValuesForTestUpdateConsol()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.AirlinePrefix, "081");
			Fields.SetFieldValue(ExpectedFieldPositions.MAWBSerialNo, "87654321");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, "QF001");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo2, "QF002");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "01/10/1999");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, "03/10/1999");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.To2nd, "MEL");
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, "BNE");
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, "SEA");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		void SetFieldValuesForTestUpdateConsol_TransportETAsAreDeterminedByProximity()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, "QF001");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo2, "QF002");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "02/10/1999");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, "03/10/1999");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "SEA");
			Fields.SetFieldValue(ExpectedFieldPositions.To2nd, "JOH");
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, "MIL");
		}

		void SetFieldValuesForTestUpdateConsol_TransportShouldBeLinkedIfThereIsOnlyOneLeg()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.AirlinePrefix, "081");
			Fields.SetFieldValue(ExpectedFieldPositions.MAWBSerialNo, "87654321");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, "QF001");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "02/10/1999");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, "SEA");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		protected virtual void SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('*', 1000);
			Fields.SetFieldValue(ExpectedFieldPositions.AirlinePrefix, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.MAWBSerialNo, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To2nd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, excessivelyLongString);
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, excessivelyLongString);
		}

		protected virtual void AssertConsolPropertiesAfterUpdateConsol(AWBRecord record, JASForwardingConsol consol)
		{
			Transport departureTransport = consol.Transports.DepartureTransport;
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			Transport flight1 = null;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					flight1 = transport;
					break;
				}
			}

			AssertEquals("08187654321", consol.JK_MasterBillNum);
			AssertEquals("USSEA", consol.JK_RL_NKLoadPort);
			AssertEquals("AUBNE", consol.JK_RL_NKDischargePort);
			AssertEquals("USSEA", departureTransport.JW_RL_NKLoadPort);
			AssertEquals("AUBNE", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("QF001", flight1.JW_VoyageFlight);
			AssertEquals(new ZDateTime(1999, 10, 1), departureTransport.JW_ETD);
			AssertEquals(new ZDateTime(1999, 10, 3), arrivalTransport.JW_ETA);
			AssertEquals(3, consol.Transports.Count);
			AssertConsolTransport(consol.Transports[0], true, Core.Constants.TransportPlanningType.Flight1, "USSEA", "AUSYD", "QF001", new ZDateTime(1999, 10, 1), new ZDateTime(1999, 10, 3));
			AssertConsolTransport(consol.Transports[1], false, Core.Constants.TransportPlanningType.Flight2, "AUSYD", "AUMEL", "QF002", new ZDateTime(1999, 10, 3), new ZDateTime(1999, 10, 3));
			AssertConsolTransport(consol.Transports[2], false, Core.Constants.TransportPlanningType.Flight3, "AUMEL", "AUBNE", "", new ZDateTime(1999, 10, 3), new ZDateTime(1999, 10, 3));
		}

		void AssertConsolProperties_TransportETAsAreDeterminedByProximity(JASForwardingConsol consol)
		{
			Transport departureTransport = consol.Transports.DepartureTransport;
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			AssertEquals(3, consol.Transports.Count);
			AssertEquals("ITMIL", consol.JK_RL_NKLoadPort);
			AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);
			AssertEquals("ITMIL", departureTransport.JW_RL_NKLoadPort);
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(1999, 10, 2), departureTransport.JW_ETD);
			AssertEquals(new ZDateTime(1999, 10, 7), arrivalTransport.JW_ETA);
			AssertConsolTransport(consol.Transports[0], true, Core.Constants.TransportPlanningType.Flight1, "ITMIL", "USSEA", "QF001", new ZDateTime(1999, 10, 2), new ZDateTime(1999, 10, 3));
			AssertConsolTransport(consol.Transports[1], false, Core.Constants.TransportPlanningType.Flight2, "USSEA", "ZAJOH", "QF002", new ZDateTime(1999, 10, 3), new ZDateTime(1999, 10, 5));
			AssertConsolTransport(consol.Transports[2], false, Core.Constants.TransportPlanningType.Flight3, "ZAJOH", "AUSYD", "", new ZDateTime(1999, 10, 5), new ZDateTime(1999, 10, 7));
		}

		void AssertConsolPropertiesAfterUpdateConsol_TransportShouldBeLinkedIfThereIsOnlyOneLeg(JASForwardingConsol consol)
		{
			Transport departureTransport = consol.Transports.DepartureTransport;
			Transport arrivalTransport = consol.Transports.ArrivalTransport;
			Transport flight1 = null;
			foreach (Transport transport in consol.Transports)
			{
				if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
				{
					flight1 = transport;
					break;
				}
			}

			AssertEquals("08187654321", consol.JK_MasterBillNum);
			AssertEquals("USSEA", consol.JK_RL_NKLoadPort);
			AssertEquals("AUSYD", consol.JK_RL_NKDischargePort);
			AssertEquals("USSEA", departureTransport.JW_RL_NKLoadPort);
			AssertEquals("AUSYD", arrivalTransport.JW_RL_NKDiscPort);
			AssertEquals("QF001", flight1.JW_VoyageFlight);
			AssertEquals(new ZDateTime(1999, 10, 2), departureTransport.JW_ETD);
			AssertEquals(new ZDateTime(1999, 10, 4), arrivalTransport.JW_ETA);
			AssertEquals(1, consol.Transports.Count);
			AssertConsolTransport(consol.Transports[0], true, Core.Constants.TransportPlanningType.Flight1, "USSEA", "AUSYD", "QF001", new ZDateTime(1999, 10, 2), new ZDateTime(1999, 10, 4));
		}

		void AssertConsolTransport(Transport transport, bool expectedIsLinked, ZString expectedTransportType, ZString expectedLoadPort, ZString expectedDiscPort, ZString expectedFlightNo, ZDateTime expectedETD, ZDateTime expectedETA)
		{
			AssertEquals(expectedIsLinked, transport.JW_IsLinked);
			AssertEquals(expectedTransportType, transport.JW_TransportType);
			AssertEquals(expectedLoadPort, transport.JW_RL_NKLoadPort);
			AssertEquals(expectedDiscPort, transport.JW_RL_NKDiscPort);
			AssertEquals(expectedFlightNo, transport.JW_VoyageFlight);
			AssertEquals(expectedETD, transport.JW_ETD);
			AssertEquals(expectedETA, transport.JW_ETA);
			AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);
		}

		#endregion
		#region TestUpdateAWBHeader
		public void TestUpdateAWBHeader()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			SetFieldValuesForTestUpdateAWBHeader();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
			AssertAWBHeaderPropertiesAfterUpdateAWBHeader(consol.AWBHeader);
		}

		[ExpectNoExceptions]
		public void TestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed();
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
		}

		protected virtual void SetFieldValuesForTestUpdateAWBHeader()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, "SEA");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, "SHIPPER");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, "SHP Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, "SHP Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, "SEATTLE");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, "WA");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, "90102");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, "US");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, "SHP ACC");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, "CONSIGNEE");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, "CNE Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, "CNE Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, "SYDNEY");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, "NSW");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, "2000");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, "AU");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, "CNE ACC");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1, "ACC 1");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 1, "ACC 2");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 2, "ACC 3");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 3, "ACC 4");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 4, "ACC 5");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 5, "ACC 6");
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 6, "ACC 7");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "MEL");
			Fields.SetFieldValue(ExpectedFieldPositions.To2nd, "BNE");
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.By1st, "QF");
			Fields.SetFieldValue(ExpectedFieldPositions.By2nd, "GA");
			Fields.SetFieldValue(ExpectedFieldPositions.By3rd, "SQ");
			Fields.SetFieldValue(ExpectedFieldPositions.ChargeCode, "CC");
			Fields.SetFieldValue(ExpectedFieldPositions.WeightPPDorCOL, "p");
			Fields.SetFieldValue(ExpectedFieldPositions.OtherPPDorCOL, "C");
			Fields.SetFieldValue(ExpectedFieldPositions.DeclaredValue, "30.4");
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForDeclaredValue, "USD");
			Fields.SetFieldValue(ExpectedFieldPositions.CustomsValue, "111");
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForCustomsValue, "AUD");
			Fields.SetFieldValue(ExpectedFieldPositions.Currency, "IDR");
			Fields.SetFieldValue(ExpectedFieldPositions.AirportOfDeparture, "Seattle International");
			Fields.SetFieldValue(ExpectedFieldPositions.AirportOfDestination, "Sydney International");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, "QF0003");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo2, "GA291");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "01/10/2002");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, "02/10/2002");
			Fields.SetFieldValue(ExpectedFieldPositions.Insurance, "200.45");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, "handling 1");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo2, "handling2");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo3, "handling 3");
			Fields.SetFieldValue(ExpectedFieldPositions.PrepaidValuationCharge, "150.12");
			Fields.SetFieldValue(ExpectedFieldPositions.CollectValuationCharge, "23.23");
			Fields.SetFieldValue(ExpectedFieldPositions.PrepaidTax, "333.12");
			Fields.SetFieldValue(ExpectedFieldPositions.CollectTax, "222.22");
			Fields.SetFieldValue(ExpectedFieldPositions.SignatureOfShipperOrAgent, "SHIPPERORAGENT");
			Fields.SetFieldValue(ExpectedFieldPositions.DateOfIssue, "01/05/2004");
			Fields.SetFieldValue(ExpectedFieldPositions.PlaceOfIssue, "AMSTERDAM");
			Fields.SetFieldValue(ExpectedFieldPositions.SignatureOfCarrierOrAgent, "CARRIERORAGENT");
		}

		protected virtual void AssertAWBHeaderPropertiesAfterUpdateAWBHeader(ExportAWBHeader aWBHeader)
		{
			AssertEquals("SEA", aWBHeader.EH_AWBOriginCode);
			AssertEquals("SHIPPER", aWBHeader.EH_ShipperName);
			AssertEquals("SHP Address1", aWBHeader.EH_ShipperAddress);
			AssertEquals("SHP Address2", aWBHeader.EH_ShipperAddress2);
			AssertEquals("SEATTLE", aWBHeader.EH_ShipperPlace);
			AssertEquals("WA", aWBHeader.EH_ShipperState);
			AssertEquals("90102", aWBHeader.EH_ShipperPostCode);
			AssertEquals("US", aWBHeader.EH_ShipperCountryCode);
			AssertEquals("SHP ACC", aWBHeader.EH_ShipperAccount);
			AssertEquals("CONSIGNEE", aWBHeader.EH_ConsigneeName);
			AssertEquals("CNE Address1", aWBHeader.EH_ConsigneeAddress);
			AssertEquals("CNE Address2", aWBHeader.EH_ConsigneeAddress2);
			AssertEquals("SYDNEY", aWBHeader.EH_ConsigneePlace);
			AssertEquals("NSW", aWBHeader.EH_ConsigneeState);
			AssertEquals("2000", aWBHeader.EH_ConsigneePostCode);
			AssertEquals("AU", aWBHeader.EH_ConsigneeCountryCode);
			AssertEquals("CNE ACC", aWBHeader.EH_ConsigneeAccount);
			AssertEquals(7, aWBHeader.AWBAccountingInformations.Count);
			AssertEquals("ACC 1", aWBHeader.AWBAccountingInformations[0].EA_Information);
			AssertEquals("ACC 2", aWBHeader.AWBAccountingInformations[1].EA_Information);
			AssertEquals("ACC 3", aWBHeader.AWBAccountingInformations[2].EA_Information);
			AssertEquals("ACC 4", aWBHeader.AWBAccountingInformations[3].EA_Information);
			AssertEquals("ACC 5", aWBHeader.AWBAccountingInformations[4].EA_Information);
			AssertEquals("ACC 6", aWBHeader.AWBAccountingInformations[5].EA_Information);
			AssertEquals("ACC 7", aWBHeader.AWBAccountingInformations[6].EA_Information);
			AssertEquals("MEL", aWBHeader.EH_To1st);
			AssertEquals("BNE", aWBHeader.EH_To2nd);
			AssertEquals("SYD", aWBHeader.EH_To3rd);
			AssertEquals("QF", aWBHeader.EH_By1st);
			AssertEquals("GA", aWBHeader.EH_By2nd);
			AssertEquals("SQ", aWBHeader.EH_By3rd);
			AssertEquals("CC", aWBHeader.EH_ChargesCode);
			AssertEquals("PPD", aWBHeader.EH_WeightVPPDCOL);
			AssertEquals("COL", aWBHeader.EH_OtherPPDCOL);
			AssertEquals(30.4m, aWBHeader.EH_DeclaredValue);
			AssertEquals("USD", aWBHeader.EH_HouseDeclaredValueCurrency);
			AssertEquals(111m, aWBHeader.EH_CustomsValue);
			AssertEquals("AUD", aWBHeader.EH_HouseCustomsValueCurrency);
			AssertEquals("IDR", aWBHeader.EH_Currency);
			AssertEquals("Seattle International", aWBHeader.EH_AirportOfDepartureAndRequestRouteText);
			AssertEquals("Sydney International", aWBHeader.EH_AirportOfDestinationText);
			AssertEquals("QF", aWBHeader.EH_Booking1stCarrier);
			AssertEquals("0003", aWBHeader.EH_Booking1stFlight);
			AssertEquals("GA", aWBHeader.EH_Booking2ndCarrier);
			AssertEquals("291", aWBHeader.EH_Booking2ndFlight);
			AssertEquals("01", aWBHeader.EH_Booking1stFlightDate);
			AssertEquals("02", aWBHeader.EH_Booking2ndFlightDate);
			AssertEquals(200.45m, aWBHeader.EH_InsuranceValue);
			AssertEquals("handling 1 handling2 handling 3", aWBHeader.EH_HandlingInformation);
			AssertEquals(150.12m, aWBHeader.EH_ValuationPPD);
			AssertEquals(23.23m, aWBHeader.EH_ValuationCOL);
			AssertEquals(333.12m, aWBHeader.EH_TaxesPPD);
			AssertEquals(222.22m, aWBHeader.EH_TaxesCOL);
			AssertEquals("SHIPPERORAGENT", aWBHeader.EH_ShippersSignature);
			AssertEquals(new ZDateTime(2004, 5, 1), aWBHeader.EH_AWBIssueDate);
			AssertEquals("AMSTERDAM", aWBHeader.EH_AWBIssuePlace);
			AssertEquals("CARRIERORAGENT", aWBHeader.EH_AWBAgentsSignature);
		}

		protected virtual void SetFieldValuesForTestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('*', 1000);
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 3, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 4, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 5, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AccountingInfo1 + 6, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To2nd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.By1st, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.By2nd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.By3rd, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ChargeCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.WeightPPDorCOL, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.OtherPPDorCOL, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.DeclaredValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForDeclaredValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CustomsValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForCustomsValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.Currency, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AirportOfDeparture, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.AirportOfDestination, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightNo2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.Insurance, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo3, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.PrepaidValuationCharge, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CollectValuationCharge, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.PrepaidTax, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CollectTax, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.SignatureOfShipperOrAgent, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.DateOfIssue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.PlaceOfIssue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.SignatureOfCarrierOrAgent, excessivelyLongString);
		}

		#endregion
		#region TestGetArrivalDateByProximity
		public void TestGetArrivalDateByProximity_OCEOrigin()
		{
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			ZDateTime departureDate = new ZDateTime(2006, 1, 29);
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "AUSYD", "USATL"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "NZAKL", "GBLON"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "AUBNE", "CNSHA"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "AUMEL", "ZAJOH"));
			AssertEquals(departureDate, record.GetArrivalDateByProximity(Factory, departureDate, "AUADL", "AUPER"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "SBHIR", "DEFRA"));
		}

		public void TestGetArrivalDateByProximity_EUROrigin()
		{
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			ZDateTime departureDate = new ZDateTime(2006, 1, 29);
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "DEFRA", "USATL"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "GBLON", "JPTYO"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "FRELY", "ZWUTA"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "PLCZW", "AUSYD"));
			AssertEquals(departureDate, record.GetArrivalDateByProximity(Factory, departureDate, "ITMIL", "PTVGA"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "ESMAD", "USBOS"));
		}

		public void TestGetArrivalDateByProximity_AMEOrigin()
		{
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			ZDateTime departureDate = new ZDateTime(2006, 1, 29);
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "CATOR", "GBLON"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "USSEA", "HKHKG"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "BRSAO", "AUMEL"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "MXGDL", "BWFRW"));
			AssertEquals(departureDate, record.GetArrivalDateByProximity(Factory, departureDate, "USATL", "UYMVD"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "ARBUE", "ITMIL"));
		}

		public void TestGetArrivalDateByProximity_ASIOrigin()
		{
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			ZDateTime departureDate = new ZDateTime(2006, 1, 29);
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "JPTYO", "AUADL"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "IDJKT", "ZAJNB"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "SGSIN", "USSEA"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "HKHKG", "ITMIL"));
			AssertEquals(departureDate, record.GetArrivalDateByProximity(Factory, departureDate, "THBKK", "CNSHA"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "IDCGK", "ZACPT"));
		}

		public void TestGetArrivalDateByProximity_AFROrigin()
		{
			AWBRecord record = (AWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			ZDateTime departureDate = new ZDateTime(2006, 1, 29);
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "ZACPT", "GBLON"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "ZAJNB", "HKHKG"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "BWFRW", "AUMEL"));
			AssertEquals(departureDate.AddDays(2), record.GetArrivalDateByProximity(Factory, departureDate, "ZWUTA", "USATL"));
			AssertEquals(departureDate, record.GetArrivalDateByProximity(Factory, departureDate, "ZAJOH", "NGSKO"));
			AssertEquals(departureDate.AddDays(1), record.GetArrivalDateByProximity(Factory, departureDate, "NGBNI", "IDJKT"));
		}

		#endregion
		#region Implementation
		protected HEADRecord HEADRecord
		{
			get
			{
				return new HEADRecord(JXCConstants.LineTypes.HEAD, HEADRecordFields.ConvertToJXCLine());
			}
		}

		protected JASCsvLineForTest HEADRecordFields
		{
			get
			{
				if (fHEADRecordFields == null)
				{
					fHEADRecordFields = new JASCsvLineForTest(JXCConstants.HEADFieldCount);
				}

				return fHEADRecordFields;
			}
		}

		protected JASCsvLineForTest Fields
		{
			get
			{
				if (fFields == null)
				{
					fFields = new JASCsvLineForTest(ExpectedNumberOfFields);
				}

				return fFields;
			}
		}

		protected NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (fFactoryProvider == null)
				{
					fFactoryProvider = new BusinessObjectFactoryProvider(Factory);
				}

				return fFactoryProvider;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UseUnmatchedOrganisationForMatchingEnabled = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled = UseUnmatchedOrganisationForMatchingEnabled;
		}

		protected abstract ZString ExpectedLineType { get; }

		protected abstract ZString ExpectedConsolAgentType { get; }

		protected abstract int ExpectedNumberOfFields { get; }

		protected abstract JXCConstants.AWBFieldPositions ExpectedFieldPositions { get; }

		JASCsvLineForTest fHEADRecordFields;
		JASCsvLineForTest fFields;
		NotificationBuffer fNotificationBuffer;
		BusinessObjectFactoryProvider fFactoryProvider;
		bool UseUnmatchedOrganisationForMatchingEnabled;
		#endregion
	}
}
