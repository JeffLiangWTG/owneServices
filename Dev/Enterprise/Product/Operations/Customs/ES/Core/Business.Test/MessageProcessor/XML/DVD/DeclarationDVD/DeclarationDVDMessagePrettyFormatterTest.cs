using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using TdRespuesta = CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDH2V1Sal.TdRespuesta;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new DeclarationDVDMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var guaranteeAEAT1 = SetGuarantee("ref1", 0.20m);
			var guaranteeAEAT2 = SetGuarantee("ref2", 0.11m);
			var guaranteesAEAT = new Collection<TdGarantiaGrNutilizada> { guaranteeAEAT1, guaranteeAEAT2 };

			var guaranteeATC1 = SetGuarantee("ref3", 0.13m);
			var guaranteesATC = new Collection<TdGarantiaGrNutilizada> { guaranteeATC1 };

			var goodsItem1 = SetGoodsItem("001", 0.20m, 0.1m, 0.0m, 0.11m);
			var goodsItem2 = SetGoodsItem("002", 0.11m, 0.0m, 0.1m, 0.20m);
			var goodsItems = new Collection<TdRespuestaPartida> { goodsItem1, goodsItem2 };

			var declarationResponse = SetResponseData("DVD", "0", "22ES009999D04136R3", TdCircuito.V, TdCircuito.R, "MYT5CUUEVP4QF7CJ", "20221002", "131240", "PNS6NA3WMAUC4J8W", 0.31m, 0.13m, guaranteesAEAT, guaranteesATC, goodsItems);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr>" +
				"<tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr>" +
				"<tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>02-10-2022, 13:12:40</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>PNS6NA3WMAUC4J8W</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr>" +
				"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.13</td></tr></table>" +
				"<br><H2>Guarantees</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>ref1</td><td>0.20</td></tr>" +
				"<tr><td>AEAT</td><td>ref2</td><td>0.11</td></tr>" +
				"<tr><td>ATC</td><td>ref3</td><td>0.13</td></tr></table>" +
				"<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
				"<tr><td>001</td><td>0.20</td><td>0.1</td><td>0.0</td><td>0.11</td></tr>" +
				"<tr><td>002</td><td>0.11</td><td>0.0</td><td>0.1</td><td>0.20</td></tr></table>";

			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new Dvdh2V1Sal();

			var error1 = SetError("6322", "El identificador del operador no es un EORI valido ", "CABECERA.ED316NumIdentifDepositante", ZString.Empty, ZString.Empty);
			var error2 = SetError("1234", "Error Description", "ErrorTag", "1", "Wrong Value");
			declarationResponse.Errores = new Collection<TdError> { error1, error2 };

			var messagePrettyFormatter = new DeclarationDVDMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>6322</td><td>El identificador del operador no es un EORI valido </td><td>CABECERA.ED316NumIdentifDepositante</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr></table>", messageInterpretationText);
		}

		public void TestDeclarationTypeData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDVDTypeText = "<H3>Accepted Declaration</H3><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr></table><br><br><br>";
			var expectedIDATypeText = "<H3>Accepted Declaration</H3><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>IDA - Into Declarant's Records Warehouse Declaration</td></tr></table><br><br><br>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Declaration Type NOT included if it's not in the response", expectedDVDTypeText, messageInterpretationText);

				declarationResponse = SetResponseData("DVD", ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DVD Declaration Type", expectedDVDTypeText, messageInterpretationText);

				declarationResponse = SetResponseData("IDA", ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IDA Declaration Type", expectedIDATypeText, messageInterpretationText);
			});
		}

		public void TestOperationData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expected0OperationText = "<table border=\"0\"><tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(0) DVD Accepted</td></tr></table>";
			var expected1OperationText = "<table border=\"0\"><tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(1) Pre-Declaration Accepted</td></tr></table>";
			var expected2OperationText = "<table border=\"0\"><tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(2) Pre-Declaration Modification</td></tr></table>";
			var expected3OperationText = "<table border=\"0\"><tr><td>Operation:</td><td>&nbsp;&nbsp;</td><td>(3) DVD Accepted by Pre-Declaration Modification</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Operation NOT included if it's not in the response", expected0OperationText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "0", ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 0 Operation", expected0OperationText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "1", ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 1 Operation", expected1OperationText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "2", ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 2 Operation", expected2OperationText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, "3", ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test 3 Operation", expected3OperationText, messageInterpretationText);
			});
		}

		public void TestCircuitAEATData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, TdCircuito.V, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, TdCircuito.N, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, TdCircuito.R, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCircuitATCData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit ATC NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, TdCircuito.V, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit ATC", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, TdCircuito.N, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit ATC", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, TdCircuito.R, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit ATC", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>3AG5G6SSCJJ93NML</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "3AG5G6SSCJJ93NML", ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestReleaseDate()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021, 05:50:30</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Release date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, "3AG5G6SSCJJ93NML", "20210220", "055030", ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, "20210220", "055030", ZString.Empty, null, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestGuaranteedTotalData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGuaranteedTotalText = "<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Guaranteed Total data NOT included if it's not in the response", expectedGuaranteedTotalText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 0.31m, null, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Guaranteed Total data", expectedGuaranteedTotalText, messageInterpretationText);
			});
		}

		public void TestATCGuaranteedTotalData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedATCGuaranteedTotalText = "<table border=\"0\"><tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test ATC Guaranteed Total data NOT included if it's not in the response", expectedATCGuaranteedTotalText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, 0.31m, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test ATC Guaranteed Total data", expectedATCGuaranteedTotalText, messageInterpretationText);
			});
		}

		public void TestGuaranteesData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGuaranteesAEATText = "<br><H2>Guarantees</H2>" +
										"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
										"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
										"<tr><td>AEAT</td><td>ref1</td><td>0.20</td></tr>" +
										"<tr><td>AEAT</td><td>ref2</td><td>0.11</td></tr></table>";

			var expectedGuaranteesATCText = "<br><H2>Guarantees</H2>" +
										"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
										"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
										"<tr><td>ATC</td><td>ref3</td><td>0.10</td></tr>" +
										"<tr><td>ATC</td><td>ref4</td><td>0.23</td></tr></table>";

			var expectedGuaranteesAllText = "<br><H2>Guarantees</H2>" +
										"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
										"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
										"<tr><td>AEAT</td><td>ref1</td><td>0.20</td></tr>" +
										"<tr><td>AEAT</td><td>ref2</td><td>0.11</td></tr>" +
										"<tr><td>ATC</td><td>ref3</td><td>0.10</td></tr>" +
										"<tr><td>ATC</td><td>ref4</td><td>0.23</td></tr></table>";

			var guaranteeAEAT1 = SetGuarantee("ref1", 0.20m);
			var guaranteeAEAT2 = SetGuarantee("ref2", 0.11m);
			var guaranteesAEAT = new Collection<TdGarantiaGrNutilizada> { guaranteeAEAT1, guaranteeAEAT2 };

			var guaranteeATC1 = SetGuarantee("ref3", 0.10m);
			var guaranteeATC2 = SetGuarantee("ref4", 0.23m);
			var guaranteesATC = new Collection<TdGarantiaGrNutilizada> { guaranteeATC1, guaranteeATC2 };

			CombineAssertions(() =>
			{
				AssertNotContains("Test GuaranteesAEAT data NOT included if it's not in the response", expectedGuaranteesAEATText, messageInterpretationText);
				AssertNotContains("Test GuaranteesATC data NOT included if it's not in the response", expectedGuaranteesATCText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, guaranteesAEAT, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GuaranteesAEAT data", expectedGuaranteesAEATText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, guaranteesATC, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GuaranteesATC data", expectedGuaranteesATCText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, guaranteesAEAT, guaranteesATC, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GuaranteesAEAT & GuaranteesATC data when both are in the response", expectedGuaranteesAllText, messageInterpretationText);
			});
		}

		public void TestTaxesAndFeesResponseData()
		{
			var declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedTaxesAndFeesResponseText = "<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
												"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
												"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
												"<tr><td>001</td><td>0.20</td><td>0.1</td><td>0.0</td><td>0.11</td></tr>" +
												"<tr><td>002</td><td>0.11</td><td>0.0</td><td>0.1</td><td>0.20</td></tr></table>";

			var goodsItem1 = SetGoodsItem("001", 0.20m, 0.1m, 0.0m, 0.11m);
			var goodsItem2 = SetGoodsItem("002", 0.11m, 0.0m, 0.1m, 0.20m);
			var goodsItems = new Collection<TdRespuestaPartida> { goodsItem1, goodsItem2 };

			CombineAssertions(() =>
			{
				AssertNotContains("Test TaxesAndFeesResponse data NOT included if it's not in the response", expectedTaxesAndFeesResponseText, messageInterpretationText);

				declarationResponse = SetResponseData(ZString.Empty, ZString.Empty, ZString.Empty, null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null, null, goodsItems);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test TaxesAndFeesResponse data", expectedTaxesAndFeesResponseText, messageInterpretationText);
			});
		}

		ZString GetAcceptedInterpretationText(Dvdh2V1Sal response)
		{
			var messagePrettyFormatter = new DeclarationDVDMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
		}

		Dvdh2V1Sal SetResponseData(ZString declarationType, ZString operationCode, ZString mrn, TdCircuito? circuitAEAT, TdCircuito? circuitATC, ZString csvClearance, ZString csvClearanceDate, ZString csvClearanceTime, ZString declarationCSV,
									decimal? guaranteedTotalAEAT, decimal? guaranteedTotalATC, Collection<TdGarantiaGrNutilizada> guaranteesAEAT, Collection<TdGarantiaGrNutilizada> guaranteesATC, Collection<TdRespuestaPartida> goodsItems)
		{
			var response = new Dvdh2V1Sal();
			response.Respuesta = new TdRespuesta()
			{
				TipoDeDeclaracion = declarationType,
				CodigoOperacion = operationCode,
				Mrn = mrn,
				Circuito = circuitAEAT,
				CircuitoAtc = circuitATC,
				CsvLevante = csvClearance,
				FechaLevante = csvClearanceDate,
				HoraLevante = csvClearanceTime,
				CsvDeclaracionElectronica = declarationCSV,
				TotalAgarantizar = guaranteedTotalAEAT,
				TotalAgarantizarAtc = guaranteedTotalATC,
				GarantiaGrNutilizada = guaranteesAEAT,
				GarantiaGrNutilizadaAtc = guaranteesATC,
				Partida = goodsItems
			};
			return response;
		}

		TdGarantiaGrNutilizada SetGuarantee(ZString reference, decimal? potentialDebt)
		{
			var guarantee = new TdGarantiaGrNutilizada();
			guarantee.CBgarantiaGrn = reference;
			guarantee.CBimportePotencial = potentialDebt;
			return guarantee;
		}

		TdRespuestaPartida SetGoodsItem(ZString itemNumber, decimal? vatAmount, decimal? dutyAmount, decimal? exciseAmount, decimal? guaranteedAmount)
		{
			var goodsItem = new TdRespuestaPartida();
			goodsItem.Ed16NumeroPartida = itemNumber;
			goodsItem.CBimporteIvaCalculado = vatAmount;
			goodsItem.CBimporteArancelCalculado = dutyAmount;
			goodsItem.CBimporteIieeCalculado = exciseAmount;
			goodsItem.CBimporteAGarantizarCalculado = guaranteedAmount;
			return goodsItem;
		}

		TdError SetError(ZString errorCode, ZString errorDescription, ZString errorLocation, ZString goodsItem, ZString wrongValue)
		{
			var error = new TdError();
			error.CodigoError = errorCode;
			error.DescripcionError = errorDescription;
			error.EtiquetaConError = errorLocation;
			error.NumeroOrdenPartidaConError = goodsItem;
			error.ValorErroneo = wrongValue;
			return error;
		}
	}
}
