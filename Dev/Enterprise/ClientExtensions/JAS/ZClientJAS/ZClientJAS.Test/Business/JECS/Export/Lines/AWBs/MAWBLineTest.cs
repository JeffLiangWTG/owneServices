using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class MAWBLineTest : AWBLineTestCase
	{
		public void TestSetFieldValues_HandlesExportAWBHeaderRowDeleted()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var consol = shipment.Consols.AddNew();
			consol.IsAWBValuesOverriddenProperty = true;
			Factory.Save();
			var awbHeader = consol.AWBHeader;
			awbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
			awbHeader.EH_AgentName = "FOO";
			var awbLine = new MAWBLineTestClass((ConsolExportAWBHeader)awbHeader);
			consol.AWBHeader.Delete();
			Factory.Save();
			var dataRow = new JXCFlatFileDataRow(120);
			awbLine.SetFieldValuesTest(dataRow);
			AssertEquals("Value should not be set as AWBHeader is deleted.", string.Empty, dataRow.GetField(awbLine.GetFieldPositions().AgentName));
			AssertNotContains("Should not be accessing a property on a deleted ExportAWBHeader row.", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			var newAWBHeader = consol.AWBHeader;
			newAWBHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
			newAWBHeader.EH_AgentName = "BAR";
			Factory.Save();
			dataRow = new JXCFlatFileDataRow(120);
			awbLine.SetFieldValuesTest(dataRow);
			AssertEquals("Value should be set.", "BAR", dataRow.GetField(awbLine.GetFieldPositions().AgentName));
			AssertNotContains("Should not be accessing a property on a deleted ExportAWBHeader row.", "Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
		}

		class MAWBLineTestClass : MAWBLine
		{
			public MAWBLineTestClass(ConsolExportAWBHeader awbHeader) : base(awbHeader)
			{
			}

			internal void SetFieldValuesTest(JXCFlatFileDataRow dataRow)
			{
				base.SetFieldValues(dataRow);
			}

			internal JXCConstants.AWBFieldPositions GetFieldPositions()
			{
				if (fFieldPositions == null)
				{
					fFieldPositions = NewFieldPositions();
				}

				return fFieldPositions;
			}

			JXCConstants.AWBFieldPositions fFieldPositions;
		}

		public void TestConsolAgentType()
		{
			SetConsol();
			AssertEquals("Default consol agent type should be agent", ExpectedLineAsString, Line.LineAsString);
			SetConsolAgentType(Core.Constants.AgentType.Agent);
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
			SetConsolAgentType(Core.Constants.AgentType.Direct);
			fExpectedLineType = "DAWB";
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
			SetConsolAgentType(Core.Constants.AgentType.Charter);
			fExpectedLineType = "MAWB";
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
		}

		[ExpectNoExceptions]
		public override void TestLineAsString()
		{
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
			AWBHeader.AWBRateLines.RemoveAll();
			AssertEquals("Should not blow up when there are no AWBRateLines", ExpectedLineAsString.Replace(";#;", ";;"), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestShipperAddressIsTrimmed()
		{
			AWBHeaderMock.Setup(m => m.EH_ShipperAddress).Returns(new ZString('X', 100));
			AWBHeaderMock.Setup(m => m.EH_ShipperAddress2).Returns(new ZString('Y', 100));
			AssertEquals(GetExpectedLineAsStringForTestShipperAddressIsTrimmed(new ZString('X', 100), new ZString('Y', 100)), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestConsigneeAddressIsTrimmed()
		{
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAddress).Returns(new ZString('_', 100));
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAddress2).Returns(new ZString('-', 100));
			AssertEquals(GetExpectedLineAsStringForTestConsigneeAddressIsTrimmed(new ZString('_', 100), new ZString('-', 100)), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestCarrierAddressIsTrimmed()
		{
			string expectedCarrierAddressString = ";" + new ZString('*', ExpectedCarrierAddressMaxLength) + ";" + new ZString('|', ExpectedCarrierAddressMaxLength) + ";";
			AWBHeaderMock.Setup(m => m.EH_IssuingAgentAddress1).Returns(new ZString('*', 100));
			AWBHeaderMock.Setup(m => m.EH_IssuingAgentAddress2).Returns(new ZString('|', 100));
			AssertEquals(ExpectedLineAsString.Replace(";CAR Add1;Car Add2;", expectedCarrierAddressString), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestAgentPlaceIsTrimmed()
		{
			string excessivelyLongString = new string('X', 1000);
			string expectedAgentPlaceString = string.Format(";{0};{0};{1};", new string('X', JXCConstants.AWBFieldBoundaries.AgentAddressMaxLength), new string('X', JXCConstants.AWBFieldBoundaries.AgentPlaceMaxLength));
			AWBHeaderMock.Protected().Setup<ZString>("AgentPlace").Returns((ZString)excessivelyLongString);
			AssertEquals(ExpectedLineAsString.Replace(";agtplc;agtplc;agtplc;", expectedAgentPlaceString), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestShipperAccountNoIsNeverEmpty()
		{
			AWBHeaderMock.Setup(m => m.EH_ShipperAccount).Returns(ZString.Empty);
			AssertEquals(ExpectedLineAsString.Replace(";ACC 101;", ";" + JXCConstants.NotAvailable + ";"), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestConsigneeAccountNoIsNeverEmpty()
		{
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAccount).Returns(ZString.Empty);
			AssertEquals(ExpectedLineAsString.Replace(";CACC 101;", ";" + JXCConstants.NotAvailable + ";"), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestAccountingInfo()
		{
			AddNewAccountingInfo("acc 8");
			AssertEquals("Pre-condition, make sure that there are 8 items in the Accounting info collection", 8, AWBHeader.AWBAccountingInformations.Count);
			AssertEquals("Accounting info fields should not change, there can only be a max of 7 accounting info in the line string", ExpectedLineAsString, Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[7]);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[6]);
			AssertEquals(ExpectedLineAsString.Replace("acc 7;", ";"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[5]);
			AssertEquals(ExpectedLineAsString.Replace("acc 6;acc 7;", ";;"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[4]);
			AssertEquals(ExpectedLineAsString.Replace("acc 5;acc 6;acc 7;", ";;;"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[3]);
			AssertEquals(ExpectedLineAsString.Replace("acc 4;acc 5;acc 6;acc 7;", ";;;;"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[2]);
			AssertEquals(ExpectedLineAsString.Replace("acc 3;acc 4;acc 5;acc 6;acc 7;", ";;;;;"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[1]);
			AssertEquals(ExpectedLineAsString.Replace("acc 2;acc 3;acc 4;acc 5;acc 6;acc 7;", ";;;;;;"), Line.LineAsString);
			AWBHeader.AWBAccountingInformations.Remove(AWBHeader.AWBAccountingInformations[0]);
			AssertEquals(ExpectedLineAsString.Replace("acc 1;acc 2;acc 3;acc 4;acc 5;acc 6;acc 7;", ";;;;;;;"), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestAccountingInfoIsTrimmed()
		{
			AWBHeader.AWBAccountingInformations.RemoveAndDeleteAll();
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			AddNewAccountingInfo(new ZString('A', 100));
			string expectedAccountingInfoString = string.Format(";{0};{0};{0};{0};{0};{0};{0};", new ZString('A', JXCConstants.AWBFieldBoundaries.AccountingInfoMaxLength));
			AssertEquals(ExpectedLineAsString.Replace(";acc 1;acc 2;acc 3;acc 4;acc 5;acc 6;acc 7;", expectedAccountingInfoString), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		public override void TestHandlingInfo()
		{
			AWBHeaderMock.Setup(m => m.EH_HandlingInformation).Returns(new ZString('+', 121));
			ZString oldHandlingInfoText = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~;~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~;~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";
			ZString newHandlingInfoText = "++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++;++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++;+";
			AssertEquals(ExpectedLineAsString.Replace(oldHandlingInfoText, newHandlingInfoText), Line.LineAsString);
			AWBHeaderMock.Setup(m => m.EH_HandlingInformation).Returns(new ZString('+', 61));
			newHandlingInfoText = "++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++;+;";
			AssertEquals(ExpectedLineAsString.Replace(oldHandlingInfoText, newHandlingInfoText), Line.LineAsString);
			AWBHeaderMock.Setup(m => m.EH_HandlingInformation).Returns(new ZString('9', 10));
			newHandlingInfoText = "9999999999;;";
			AssertEquals(ExpectedLineAsString.Replace(oldHandlingInfoText, newHandlingInfoText), Line.LineAsString);
			AWBHeaderMock.VerifyAll();
		}

		#region Overrides
		protected override ExportAWBHeader AWBHeader
		{
			get
			{
				return AWBHeaderMock.Object;
			}
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 121;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return fExpectedLineType;
			}
		}

		protected override AWBLine GetAWBLine()
		{
			return new MAWBLine(ConsolAWBHeader);
		}

		protected override string ExpectedContentLineAsString
		{
			get
			{
				return "N;Y;HKG;081;33512264;Kleenmaid;Stairway;to heaven;;Belleuve;ACC 101;CKleenmaid;CStairway;Cto heaven;;CBelleuve;CACC 101;CAR101;CAR Add1;Car Add2;;;AGT101;agtplc;agtplc;agtplc;12-3 4567/0000;agtacc;Dublin;acc 1;acc 2;acc 3;acc 4;acc 5;acc 6;acc 7;BNE;QF;LAX;SQ;CGK;GA;SGD;LLP;X;P;88.12;SGD;77.9;SGD;Mallorca;DA089;17/04/2005;GZ999;18/05/2005;27.89;~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~;~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~;~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~;;;;;;;ZZ;76;46;#;38.9;1;2;3;4;5;6;7;8;9;10;11;12;;;;;;;;;;;;;;;;Kachem;24/12/2001;Valencia;blabla;;;;;;;;;ID;CI;Stairway;Belleuve;DDG;POCODE;CStairway;CBelleuve;CDDG;CPOCODE;;";
			}
		}

		protected override int ExpectedCarrierAddressMaxLength
		{
			get
			{
				return JXCConstants.MAWBFieldBoundaries.CarrierAddressMaxLength;
			}
		}

		protected override int ExpectedConsigneeAddressMaxLength
		{
			get
			{
				return JXCConstants.MAWBFieldBoundaries.ConsigneeAddressMaxLength;
			}
		}

		protected override int ExpectedShipperAddressMaxLength
		{
			get
			{
				return JXCConstants.MAWBFieldBoundaries.ShipperAddressMaxLength;
			}
		}

		protected override Type TypeOfExportAWBHeader
		{
			get
			{
				return typeof(JASConsolExportAWBHeader);
			}
		}

		protected override ZString GetExpectedLineAsStringForTestConsigneeAddressIsTrimmed(ZString consigneeAddress1, ZString consigneeAddress2)
		{
			ZString result = base.GetExpectedLineAsStringForTestConsigneeAddressIsTrimmed(consigneeAddress1, consigneeAddress2);
			return result.Replace(";CStairway;", ";" + consigneeAddress1.Left(ExpectedConsigneeAddressMaxLength) + ";");
		}

		protected override ZString GetExpectedLineAsStringForTestShipperAddressIsTrimmed(ZString shipperAddress1, ZString shipperAddress2)
		{
			ZString result = base.GetExpectedLineAsStringForTestShipperAddressIsTrimmed(shipperAddress1, shipperAddress2);
			return result.Replace(";Stairway;", ";" + shipperAddress1.Left(ExpectedShipperAddressMaxLength) + ";");
		}

		#endregion
		#region Implementation
		protected Mock<JASConsolExportAWBHeader> AWBHeaderMock
		{
			get
			{
				if (fAWBHeaderMock == null)
				{
					fAWBHeaderMock = Factory.NewMoq<JASConsolExportAWBHeader>();
					PopulateMockAWBHeader();
				}
				return (Mock<JASConsolExportAWBHeader>)fAWBHeaderMock;
			}
		}

		JASConsolExportAWBHeader ConsolAWBHeader
		{
			get
			{
				return (JASConsolExportAWBHeader)AWBHeader;
			}
		}

		#region Consol
		Mock<JASForwardingConsol> ConsolMock
		{
			get
			{
				if (fConsolMock == null)
				{
					fConsolMock = Factory.NewMoq<JASForwardingConsol>();
				}

				return (Mock<JASForwardingConsol>)fConsolMock;
			}
		}

		Mock fConsolMock;
		void SetConsolAgentType(ZString agentType)
		{
			ConsolMock.Setup(m => m.JK_AgentType).Returns(agentType);
		}

		void SetConsol()
		{
			AWBHeaderMock.Setup(m => m.Consol).Returns(ConsolMock.Object);
		}

		#endregion
		ZString fExpectedLineType = "MAWB";
		Mock fAWBHeaderMock;
		protected virtual void PopulateMockAWBHeader()
		{
			PopulateHeaderData();
			PopulateShipperData();
			PopulateConsigneeData();
			PopulateCarrierData();
			PopulateAgentData();
			PopulateAccountingInfoData();
			PopulateRoutingData();
			PopulateFreightDeclarationsData();
			PopulateFlightInfoData();
			PopulateHandlingInfoData();
			PopulateFreightInfoData();
			PopulateChargesData();
			PopulateFooterData();
		}

		#region Header
		protected virtual void PopulateHeaderData()
		{
			AWBHeaderMock.Setup(m => m.EH_AWBOriginCode).Returns((ZString)"HKG");
			AWBHeaderMock.Protected().Setup<ZString>("MasterBill").Returns((ZString)"08133512264");
		}

		#endregion
		#region Shipper
		protected virtual void PopulateShipperData()
		{
			AWBHeaderMock.Setup(m => m.EH_ShipperName).Returns((ZString)"Kleenmaid");
			AWBHeaderMock.Setup(m => m.EH_ShipperAddress).Returns((ZString)"Stairway");
			AWBHeaderMock.Setup(m => m.EH_ShipperAddress2).Returns((ZString)"to heaven");
			AWBHeaderMock.Setup(m => m.EH_ShipperPlace).Returns((ZString)"Belleuve");
			AWBHeaderMock.Setup(m => m.EH_ShipperPostCode).Returns((ZString)"POCODE");
			AWBHeaderMock.Setup(m => m.EH_ShipperState).Returns((ZString)"DDG");
			AWBHeaderMock.Setup(m => m.EH_ShipperCountryCode).Returns((ZString)"ID");
			AWBHeaderMock.Setup(m => m.EH_ShipperAccount).Returns((ZString)"ACC 101");
		}

		#endregion
		#region Consignee
		protected virtual void PopulateConsigneeData()
		{
			AWBHeaderMock.Setup(m => m.EH_ConsigneeName).Returns((ZString)"CKleenmaid");
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAddress).Returns((ZString)"CStairway");
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAddress2).Returns((ZString)"Cto heaven");
			AWBHeaderMock.Setup(m => m.EH_ConsigneePlace).Returns((ZString)"CBelleuve");
			AWBHeaderMock.Setup(m => m.EH_ConsigneePostCode).Returns((ZString)"CPOCODE");
			AWBHeaderMock.Setup(m => m.EH_ConsigneeState).Returns((ZString)"CDDG");
			AWBHeaderMock.Setup(m => m.EH_ConsigneeCountryCode).Returns((ZString)"CI");
			AWBHeaderMock.Setup(m => m.EH_ConsigneeAccount).Returns((ZString)"CACC 101");
		}

		#endregion
		#region Carrier
		void PopulateCarrierData()
		{
			AWBHeaderMock.Setup(m => m.EH_IssuingAgentName).Returns((ZString)"CAR101");
			AWBHeaderMock.Setup(m => m.EH_IssuingAgentAddress1).Returns((ZString)"CAR Add1");
			AWBHeaderMock.Setup(m => m.EH_IssuingAgentAddress2).Returns((ZString)"Car Add2");
		}

		#endregion
		#region Agent
		void PopulateAgentData()
		{
			AWBHeaderMock.Protected().Setup<ZString>("AgentName").Returns((ZString)"AGT101");
			AWBHeaderMock.Protected().Setup<ZString>("AgentPlace").Returns((ZString)"agtplc");
			AWBHeaderMock.Protected().Setup<ZString>("AgentIATACode").Returns((ZString)"12345670000");
			AWBHeaderMock.Protected().Setup<ZString>("AgentAccountNo").Returns((ZString)"agtacc");
		}

		#endregion
		#region Accounting Info
		void PopulateAccountingInfoData()
		{
			AWBHeader.AWBAccountingInformations.RemoveAll();
			for (int i = 1; i <= 7; i++)
			{
				AddNewAccountingInfo("acc " + i.ToString());
			}
		}

		void AddNewAccountingInfo(ZString info)
		{
			var mock = Factory.NewMoq<ExportAWBAccountingInformation>();
			mock.Setup(m => m.EA_Information).Returns(info);
			AWBHeader.AWBAccountingInformations.Add(mock.Object);
		}

		#endregion
		#region Routing
		void PopulateRoutingData()
		{
			AWBHeaderMock.Setup(m => m.EH_To1st).Returns((ZString)"BNE");
			AWBHeaderMock.Setup(m => m.EH_To2nd).Returns((ZString)"LAX");
			AWBHeaderMock.Setup(m => m.EH_To3rd).Returns((ZString)"CGK");
			AWBHeaderMock.Setup(m => m.EH_By1st).Returns((ZString)"QF");
			AWBHeaderMock.Setup(m => m.EH_By2nd).Returns((ZString)"SQ");
			AWBHeaderMock.Setup(m => m.EH_By3rd).Returns((ZString)"GA");
		}

		#endregion
		#region Freight Declarations
		void PopulateFreightDeclarationsData()
		{
			AWBHeaderMock.Setup(m => m.EH_ChargesCode).Returns((ZString)"LLP");
			AWBHeaderMock.Setup(m => m.EH_WeightVPPDCOL).Returns((ZString)"XCO");
			AWBHeaderMock.Setup(m => m.EH_OtherPPDCOL).Returns((ZString)"PPD");
			AWBHeaderMock.Setup(m => m.EH_DeclaredValue).Returns((ZDecimal)88.12M);
			AWBHeaderMock.Setup(m => m.EH_Currency).Returns((ZString)"SGD");
			AWBHeaderMock.Setup(m => m.EH_CustomsValue).Returns((ZDecimal)77.8999M);
		}

		#endregion
		#region Flight Info
		void PopulateFlightInfoData()
		{
			AWBHeaderMock.Setup(m => m.EH_AirportOfDepartureAndRequestRouteText).Returns((ZString)"Dublin");
			AWBHeaderMock.Setup(m => m.EH_AirportOfDestinationText).Returns((ZString)"Mallorca");
			AWBHeaderMock.Setup(m => m.EH_Booking1stCarrier).Returns((ZString)"DA");
			AWBHeaderMock.Setup(m => m.EH_Booking1stFlight).Returns((ZString)"089");
			AWBHeaderMock.Setup(m => m.Booking1stFlightDate).Returns(new ZDateTime(2005, 4, 17));
			AWBHeaderMock.Setup(m => m.EH_Booking2ndCarrier).Returns((ZString)"GZ");
			AWBHeaderMock.Setup(m => m.EH_Booking2ndFlight).Returns((ZString)"999");
			AWBHeaderMock.Setup(m => m.Booking2ndFlightDate).Returns(new ZDateTime(2005, 5, 18));
			AWBHeaderMock.Setup(m => m.EH_InsuranceValue).Returns((ZDecimal)27.89M);
		}

		#endregion
		#region Handling Info
		void PopulateHandlingInfoData()
		{
			AWBHeaderMock.Setup(m => m.EH_HandlingInformation).Returns(new ZString('~', 180));
		}

		#endregion
		#region Freight Info
		void PopulateFreightInfoData()
		{
			AWBHeaderMock.Setup(m => m.EH_TotalNoOfPieces).Returns((ZInt)76);
			AWBHeaderMock.Setup(m => m.EH_TotalGrossWeight).Returns((ZDecimal)46);
			AWBHeaderMock.Setup(m => m.EH_TotalLineTotals).Returns((ZDecimal)38.9M);
			AWBHeader.AWBRateLines.RemoveAll();
			AWBHeader.AWBRateLines.Add(CreateMockAWBRateLine());
		}

		ExportAWBRateLine CreateMockAWBRateLine()
		{
			var rateLineMock = Factory.NewMoq<ExportAWBRateLine>();
			rateLineMock.Setup(m => m.ER_WeightInLBsOrKGs).Returns((ZString)"#");
			rateLineMock.Setup(m => m.ER_LineCount).Returns((ZByte)1);
			return rateLineMock.Object;
		}

		#endregion
		#region Charges
		void PopulateChargesData()
		{
			AWBHeaderMock.Setup(m => m.EH_TotalWeightPPD).Returns((ZDecimal)1);
			AWBHeaderMock.Setup(m => m.EH_TotalWeightCOL).Returns((ZDecimal)2);
			AWBHeaderMock.Setup(m => m.EH_ValuationPPD).Returns((ZDecimal)3);
			AWBHeaderMock.Setup(m => m.EH_ValuationCOL).Returns((ZDecimal)4);
			AWBHeaderMock.Setup(m => m.EH_TaxesPPD).Returns((ZDecimal)5);
			AWBHeaderMock.Setup(m => m.EH_TaxesCOL).Returns((ZDecimal)6);
			AWBHeaderMock.Setup(m => m.EH_OtherChargesDueAgentPPD).Returns((ZDecimal)7);
			AWBHeaderMock.Setup(m => m.EH_OtherChargesDueAgentCOL).Returns((ZDecimal)8);
			AWBHeaderMock.Setup(m => m.EH_OtherChargesDueCarrierPPD).Returns((ZDecimal)9);
			AWBHeaderMock.Setup(m => m.EH_OtherChargesDueCarrierCOL).Returns((ZDecimal)10);
			AWBHeaderMock.Setup(m => m.EH_TotalPPD).Returns((ZDecimal)11);
			AWBHeaderMock.Setup(m => m.EH_TotalCOL).Returns((ZDecimal)12);
		}

		#endregion
		#region Footer
		void PopulateFooterData()
		{
			AWBHeaderMock.Setup(m => m.EH_ShippersSignature).Returns((ZString)"Kachem");
			AWBHeaderMock.Setup(m => m.EH_AWBIssueDate).Returns(new ZDateTime(2001, 12, 24));
			AWBHeaderMock.Setup(m => m.EH_AWBIssuePlace).Returns((ZString)"Valencia");
			AWBHeaderMock.Setup(m => m.EH_AWBAgentsSignature).Returns((ZString)"blabla");
		}
		#endregion
		#endregion
	}
}
