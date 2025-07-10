using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using TdRespuesta = CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Sal.TdRespuesta;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class QueryDVDMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new QueryDVDMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclaration()
		{
			var guarantee1 = SetGuarantee("ref1", 0.20m);
			var guarantee2 = SetGuarantee("ref2", 0.11m);
			var guarantees = new Collection<TdGarantiaGrNutilizada> { guarantee1, guarantee2 };

			var goodsItem1 = SetGoodsItem("001", 0.20m, 0.1m, 0.0m, 0.11m);
			var goodsItem2 = SetGoodsItem("002", 0.11m, 0.0m, 0.1m, 0.20m);
			var goodsItems = new Collection<TdPartida> { goodsItem1, goodsItem2 };

			var declarationResponse = SetResponseData(TdAdministracion.Aeat, "DVD", "22ES009999D04136R3", TdCircuito.V, "MYT5CUUEVP4QF7CJ", "20221002", "131240", "PNS6NA3WMAUC4J8W", 0.31m, guarantees, goodsItems);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04136R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>02-10-2022, 13:12:40</td></tr></table>" +
				"<br><H2>Taxes and fees data</H2>" +
				"<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>" +
				"<br><H2>Guarantees</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
				"<tr><td>AEAT</td><td>ref1</td><td>0.20</td></tr>" +
				"<tr><td>AEAT</td><td>ref2</td><td>0.11</td></tr></table>" +
				"<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
				"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
				"<tr><td>001</td><td>0.20</td><td>0.1</td><td>0.0</td><td>0.11</td></tr>" +
				"<tr><td>002</td><td>0.11</td><td>0.0</td><td>0.1</td><td>0.20</td></tr></table>";

			AssertEquals("Expected Accepted query message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var declarationResponse = new ConsultaDvdh2V1Sal();

			var error1 = SetError("6322", "El identificador del operador no es un EORI valido ", "CABECERA.ED316NumIdentifDepositante", ZString.Empty, ZString.Empty);
			var error2 = SetError("1234", "Error Description", "ErrorTag", "1", "Wrong Value");
			declarationResponse.Errores = new Collection<TdError> { error1, error2 };

			var messagePrettyFormatter = new QueryDVDMessagePrettyFormatter(declarationResponse);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertContains("<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>6322</td><td>El identificador del operador no es un EORI valido </td><td>CABECERA.ED316NumIdentifDepositante</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr></table>", messageInterpretationText);
		}

		public void TestCircuitWithoutAdministration()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, TdCircuito.V, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, TdCircuito.N, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, TdCircuito.R, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCircuitAEATData()
		{
			var declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, TdCircuito.V, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, TdCircuito.N, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, TdCircuito.R, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestCircuitATCData()
		{
			var declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGreenCircuitText = "<br><table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>";
			var expectedOrangeCircuitText = "<br><table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>";
			var expectedRedCircuitText = "<br><table border=\"0\"><tr><td>ATC Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Circuit NOT included if it's not in the response", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, TdCircuito.V, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Green Circuit", expectedGreenCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, TdCircuito.N, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Orange Circuit", expectedOrangeCircuitText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, TdCircuito.R, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Red Circuit", expectedRedCircuitText, messageInterpretationText);
			});
		}

		public void TestMRNData()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES009999D04170R7</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test MRN data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, "22ES009999D04170R7", null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test MRN data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestCSVClearanceData()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>MYT5CUUEVP4QF7CJ</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test CSV Clearance data NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, "MYT5CUUEVP4QF7CJ", ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);

				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test CSV Clearance data", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestReleaseDate()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedAcceptanceText = "<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-02-2021, 05:50:30</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Release date NOT included if it's not in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, "MYT5CUUEVP4QF7CJ", "20210220", "055030", ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Release date when date and csvCode are in the response", expectedAcceptanceText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, "20210220", "055030", ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertNotContains("Test Release date NOT included when date is in the response but csvCode is not", expectedAcceptanceText, messageInterpretationText);
			});
		}

		public void TestAEATGuaranteedTotalData()
		{
			var declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGuaranteedTotalText = "<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Guaranteed Total data NOT included if it's not in the response", expectedGuaranteedTotalText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 0.31m, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Guaranteed Total data", expectedGuaranteedTotalText, messageInterpretationText);
			});
		}

		public void TestGuaranteedTotalDataWithoutAdministation()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGuaranteedTotalText = "<table border=\"0\"><tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Guaranteed Total data NOT included if it's not in the response", expectedGuaranteedTotalText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 0.31m, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test Guaranteed Total data", expectedGuaranteedTotalText, messageInterpretationText);
			});
		}

		public void TestATCGuaranteedTotalData()
		{
			var declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedATCGuaranteedTotalText = "<table border=\"0\"><tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>0.31</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test ATC Guaranteed Total data NOT included if it's not in the response", expectedATCGuaranteedTotalText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 0.31m, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test ATC Guaranteed Total data", expectedATCGuaranteedTotalText, messageInterpretationText);
			});
		}

		public void TestDeclarationTypeData()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedDVDTypeText = "<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>DVD - Into Warehouse Declaration</td></tr></table>";
			var expectedIDATypeText = "<table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>IDA - Into Declarant's Records Warehouse Declaration</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertNotContains("Test Declaration Type NOT included if it's not in the response", expectedDVDTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(null, "DVD", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test DVD Declaration Type", expectedDVDTypeText, messageInterpretationText);

				declarationResponse = SetResponseData(null, "IDA", ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test IDA Declaration Type", expectedIDATypeText, messageInterpretationText);
			});
		}

		public void TestGuaranteesData()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedGuaranteesAEATText = "<br><H2>Guarantees</H2>" +
										"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
										"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
										"<tr><td>AEAT</td><td>ref1</td><td>0.20</td></tr>" +
										"<tr><td>AEAT</td><td>ref2</td><td>0.11</td></tr></table>";

			var expectedGuaranteesATCText = "<H3>Accepted Declaration</H3><br><br><br><H2>Guarantees</H2>" +
										"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
										"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Potential Debt</th></tr></thead>" +
										"<tr><td>ATC</td><td>ref1</td><td>0.20</td></tr>" +
										"<tr><td>ATC</td><td>ref2</td><td>0.11</td></tr></table>";

			var guarantee1 = SetGuarantee("ref1", 0.20m);
			var guarantee2 = SetGuarantee("ref2", 0.11m);
			var guarantees = new Collection<TdGarantiaGrNutilizada> { guarantee1, guarantee2 };

			CombineAssertions(() =>
			{
				AssertNotContains("Test GuaranteesAEAT data NOT included if it's not in the response", expectedGuaranteesAEATText, messageInterpretationText);
				AssertNotContains("Test GuaranteesATC data NOT included if it's not in the response", expectedGuaranteesATCText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Aeat, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, guarantees, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GuaranteesAEAT data", expectedGuaranteesAEATText, messageInterpretationText);

				declarationResponse = SetResponseData(TdAdministracion.Atc, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, guarantees, null);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test GuaranteesATC data", expectedGuaranteesATCText, messageInterpretationText);
			});
		}

		public void TestTaxesAndFeesResponseData()
		{
			var declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, null);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			var expectedTaxesAndFeesResponseText = "<br><H2>Taxes and fees response (Spanish Customs)</H2>" +
												"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
												"<thead><tr class=\"tableheadings\"><th>Item</th><th>VAT</th><th>Duty</th><th>Excise</th><th>Guaranteed Amount</th></tr></thead>" +
												"<tr><td>001</td><td>0.20</td><td>0.1</td><td>0.0</td><td>0.11</td></tr>" +
												"<tr><td>002</td><td>0.11</td><td>0.0</td><td>0.1</td><td>0.20</td></tr></table>";

			var goodsItem1 = SetGoodsItem("001", 0.20m, 0.1m, 0.0m, 0.11m);
			var goodsItem2 = SetGoodsItem("002", 0.11m, 0.0m, 0.1m, 0.20m);
			var goodsItems = new Collection<TdPartida> { goodsItem1, goodsItem2 };

			CombineAssertions(() =>
			{
				AssertNotContains("Test TaxesAndFeesResponse data NOT included if it's not in the response", expectedTaxesAndFeesResponseText, messageInterpretationText);

				declarationResponse = SetResponseData(null, ZString.Empty, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, goodsItems);
				messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);
				AssertContains("Test TaxesAndFeesResponse data", expectedTaxesAndFeesResponseText, messageInterpretationText);
			});
		}

		ZString GetAcceptedInterpretationText(ConsultaDvdh2V1Sal response)
		{
			var messagePrettyFormatter = new QueryDVDMessagePrettyFormatter(response);
			return messagePrettyFormatter.CreateMessageDetailsAccepted();
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

		ConsultaDvdh2V1Sal SetResponseData(TdAdministracion? administration, ZString declarationType, ZString mrn, TdCircuito? circuitAEAT, ZString csvClearance, ZString csvClearanceDate, ZString csvClearanceTime, ZString declarationCSV,
							decimal? guaranteedTotalAEAT, Collection<TdGarantiaGrNutilizada> guaranteesAEAT, Collection<TdPartida> goodsItems)
		{
			var response = new ConsultaDvdh2V1Sal();
			response.Mensaje = new TdMensaje() { MrnOperacion = mrn };
			response.Respuesta = new TdRespuesta()
			{
				Administracion = administration,
				TipoDeDeclaracion = declarationType,
				Circuito = circuitAEAT,
				CsvLevante = csvClearance,
				FechaLevante = csvClearanceDate,
				HoraLevante = csvClearanceTime,
				TotalAgarantizar = guaranteedTotalAEAT,
				GarantiaGrNutilizada = guaranteesAEAT,
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

		TdPartida SetGoodsItem(ZString itemNumber, decimal? vatAmount, decimal? dutyAmount, decimal? exciseAmount, decimal? guaranteedAmount)
		{
			var goodsItem = new TdPartida();
			goodsItem.Ed16NumeroPartida = itemNumber;
			goodsItem.CBimporteIvaCalculado = vatAmount;
			goodsItem.CBimporteArancelCalculado = dutyAmount;
			goodsItem.CBimporteIieeCalculado = exciseAmount;
			goodsItem.CBimporteAGarantizarCalculado = guaranteedAmount;
			return goodsItem;
		}
	}
}
