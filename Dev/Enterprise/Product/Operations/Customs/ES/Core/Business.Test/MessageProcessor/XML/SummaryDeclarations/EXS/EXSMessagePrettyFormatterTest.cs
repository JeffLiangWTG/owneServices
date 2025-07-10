using System;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE616V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE919V4Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class EXSMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new EXSMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var declarationResponse = SetAcceptedResponseData("XMDRPDDM3KTUA74Z", "9925F59996F50F99", "A1", "V", "21ES00999960000023", "202101051309");
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			AssertEquals("<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>XMDRPDDM3KTUA74Z</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>05-01-2021, 13:09:00</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>21ES00999960000023</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>9925F59996F50F99</td></tr></table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected_616()
		{
			var declarationResponse = new Cc616AType()
			{
				Heahea = new HeaheaType()
				{
					RefNumHea4 = "EXS00011",
					DecRejReaHea252 = "Razon"
				},
				Funerrer1 = new System.Collections.ObjectModel.Collection<Funerrer1Type>()
				{
					new Funerrer1Type()
					{
						ErrTypEr11 = "83",
						ErrPoiEr12 = "MES.HEA.GDS(1).DocRefPD12",
						OriAttValEr14 = "9999012345600001. DECLARACION SUMARIA INEXISTENTE Sumaria: 20ES00999981234560"
					},
					new Funerrer1Type()
					{
						ErrTypEr11 = "84",
						ErrPoiEr12 = "MES.HEA.GDS(1).PACGS2",
						ErrReaEr13 = "Razon error",
						OriAttValEr14 = "Valor original error"
					}
				}
			};
			var messageInterpretationText = GetRejectedInterpretationText(declarationResponse);

			AssertEquals("<H3>Rejected Declaration</H3>" +
				"<table border=\"0\"><tr><td>Reason:</td><td>&nbsp;&nbsp;</td><td>EXS00011 Razon</td></tr></table>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>83</td><td>MES.HEA.GDS(1).DocRefPD12</td><td>&nbsp;</td><td>9999012345600001. DECLARACION SUMARIA INEXISTENTE Sumaria: 20ES00999981234560</td></tr>" +
				"<tr><td>84</td><td>MES.HEA.GDS(1).PACGS2</td><td>Razon error</td><td>Valor original error</td></tr>" +
				"</table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected_919()
		{
			var declarationResponse = new Cd919B()
			{
				Xmlerr805 = new System.Collections.ObjectModel.Collection<Xmlerr805>
				{
					new Xmlerr805()
					{
						ErrLinNumXmler800 = "1",
						ErrColNumXmler801 = "1",
						ErrReaXmler802 = "Too many repetitions",
						ErrLocXmler803 = "MES.HEA.GOOITEGDS",
						ErrCodXmler806 = "35",
						OriAttValXmler804 = "501"
					},
					new Xmlerr805()
					{
						ErrLinNumXmler800 = "1",
						ErrColNumXmler801 = "1",
						ErrReaXmler802 = "Too many repetitions",
						ErrLocXmler803 = "MES.HEA.ITI",
						ErrCodXmler806 = "35",
						OriAttValXmler804 = "601"
					}
				}
			};
			var messageInterpretationText = GetRejectedInterpretationText(declarationResponse);

			AssertEquals("<H3>Rejected Declaration (XML Error)</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td><td><strong>Location</strong></td></tr>" +
				"<tr><td>35</td><td>1</td><td>Too many repetitions</td><td>501</td><td>MES.HEA.GOOITEGDS</td></tr>" +
				"<tr><td>35</td><td>1</td><td>Too many repetitions</td><td>601</td><td>MES.HEA.ITI</td></tr>" +
				"</table>", messageInterpretationText);
		}

		public void TestCSVElectronicDeclarationData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>XMDRPDDM3KTUA74Z</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included CSV Electronic Declaration data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData("XMDRPDDM3KTUA74Z", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Electronic Declaration data", expectedText, messageInterpretationText);
			});
		}

		public void TestTypeData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedA1Text = "<table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration</td></tr></table>";
			var expectedA2Text = "<table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Exit Summary Declaration Express Consignment</td></tr></table>";
			var expectedA3Text = "<table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Re-Export Notification</td></tr></table>";
			var expectedNRText = "<table border=\"0\"><tr><td>Type:</td><td>&nbsp;&nbsp;</td><td>Reshipment Notification</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Type if it's not in the response", expectedA1Text, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, "A1", ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A1 type", expectedA1Text, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, "A2", ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A2 type", expectedA2Text, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, "A3", ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test A3 type", expectedA3Text, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, "NR", ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test NR type", expectedNRText, messageInterpretationText);
			});
		}

		public void TestAcceptanceData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>05-01-2021, 13:09:00</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included Acceptance data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "202101051309");
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Acceptance data", expectedText, messageInterpretationText);
			});
		}

		public void TestMRNData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>21ES00999960000023</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included MRN data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "21ES00999960000023", ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test MRN data", expectedText, messageInterpretationText);
			});
		}

		public void TestCircuitData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test No include Circuit if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "V", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "N", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, "R", ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestClearanceData()
		{
			var declarationResponse = SetAcceptedResponseData(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>9925F59996F50F99</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Not included Clearance data if it's not in the response", expectedText, messageInterpretationText);

				declarationResponse = SetAcceptedResponseData(ZString.Empty, "9925F59996F50F99", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Clearance data", expectedText, messageInterpretationText);
			});
		}

		ZString GetAcceptedInterpretationText(EXSResponse response) => new EXSMessagePrettyFormatter(response).CreateMessageDetailsAccepted();
		ZString GetRejectedInterpretationText(EXSResponse response) => new EXSMessagePrettyFormatter(response).CreateMessageDetailsRejected();

		Cc628A SetAcceptedResponseData(ZString csvElectronicDeclaration, ZString csvClearance, ZString type, ZString circuit, ZString mrn, ZString acceptanceDate)
		{
			return new Cc628A()
			{
				Heahea = new CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal.Heahea()
				{
					DecCsvHea = csvElectronicDeclaration,
					RelCsvHea = csvClearance,
					DecTypeHea = type,
					CusChanHea = circuit,
					DocNumHea5 = mrn,
					DecRegDatTimHea115 = acceptanceDate
				}
			};
		}
	}
}
