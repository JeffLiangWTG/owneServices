using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportDocCas44PendV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class Box44ImportMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new Box44ImportMessagePrettyFormatter(null, Factory.New<CusEntryHeader>()));
			AssertExceptionThrown<ArgumentNullException>("Null entry header", () => new Box44ImportMessagePrettyFormatter(new ImportDocCas44PendV1Sal(), null));
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclarationNotDeferred()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "KN", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), CountryCodes.Spain);
			Factory.Save();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Description");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList("ES", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "N853", "DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715]", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();

			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;

			entryHeader.TotalAmount = 1.15;
			var declarationResponse = new ImportDocCas44PendV1Sal();
			declarationResponse.EstadoDelDespacho = "L";
			declarationResponse.DescripEstadoDelDespacho = "Accepted with Customs Clearance";
			declarationResponse.EstadoDelDespachoAtc = "L";
			declarationResponse.DescripEstadoDelDespachoAtc = "Accepted with Customs Clearance";
			declarationResponse.EstadoContable = "CO";
			declarationResponse.DescripEstadoContable = "Contracted";
			declarationResponse.EstadoContableAtc = "CO";
			declarationResponse.DescripEstadoContableAtc = "Contracted";
			declarationResponse.TienePendenciasSinUltimar = "N";
			declarationResponse.TienePendenciasSinUltimarAtc = "N";

			declarationResponse.TotalApagar = 1.15M;
			declarationResponse.ExencionIvaGarantiaLevante = 1.05M;
			declarationResponse.ExencionIvaGarantiaPendencia = 0.15M;
			declarationResponse.TotalAgarantizar = 1.15M;
			declarationResponse.TotalApagarAtc = 1.31M;
			declarationResponse.TotalAgarantizarAtc = 1.31M;
			declarationResponse.TotalIvAdiferido = 1.15M;

			declarationResponse.FechaLimitePago = "20221025";
			declarationResponse.FechaLimitePagoAtc = "20221026";

			var grnGuarantee1 = SetGRNGuarantee("16ESAGL9990000096", 45.13, 0, 0);
			var grnGuarantee2 = SetGRNGuarantee("16ESAGL9990000095", 1.15, 1.15, 1.15);
			declarationResponse.GarantiaGrNutilizada = new Collection<GarantiaGrNutilizadaTd> { grnGuarantee1, grnGuarantee2 };

			var grnGuaranteeCan1 = SetGRNGuarantee("18ESCGL9980000060", 1.31, 1.31, 1.31);
			declarationResponse.GarantiaGrNutilizadaAtc = new Collection<GarantiaGrNutilizadaTd> { grnGuaranteeCan1 };

			declarationResponse.InformacionDePartida = new Collection<PartidaTd> { };
			var line1 = new PartidaTd();
			line1.C32NumeroDePartida = 1;

			var certificate1 = SetCertificate("SNM", "SIF05", " Sanidad Exterior - Mº Sanidad", new Collection<string> { "N853", "C657", "C678", "1405", "1413", "C640" });
			var certificate2 = SetCertificate("VIM", "SIF02", " Sanidad Animal, M Agricultura", new Collection<string> { "N853", "C657", "C678", "C640" });
			line1.C44CertificadoRequerido = new Collection<CertiControlTd> { certificate1, certificate2 };

			var tribute1 = SetTribute("A00", 42.560M, 2.700000M, "MA", "%", 1.15M, 1.15M);
			var tribute2 = SetTribute("3IG", 43.710M, 3.000000M, ZString.Empty, "KN", 1.31M, 1.31M);
			line1.C47TributoLiquidado = new Collection<Cas47TributoLiquidadoTd> { tribute1, tribute2 };

			var line2 = new PartidaTd();
			line2.C32NumeroDePartida = 2;

			var certificate3 = SetCertificate("SNM", "SIF06", " Sanidad Interior - Mº Sanidad", new Collection<string> { "N853", "C657", "C678", "1405", "1413", "C640" });
			line2.C44CertificadoRequerido = new Collection<CertiControlTd> { certificate3 };

			var tribute3 = SetTribute("A00", 1219.350M, 17.600000M, ZString.Empty, "%", 214.61M, 214.61M);
			var tribute4 = SetTribute("B00", 1481.380M, 10.000000M, ZString.Empty, "%", 148.14M, 148.14M);
			line2.C47TributoLiquidado = new Collection<Cas47TributoLiquidadoTd> { tribute3, tribute4 };

			declarationResponse.InformacionDePartida = new Collection<PartidaTd> { line1, line2 };

			var messagePrettyFormatter = new Box44ImportMessagePrettyFormatter(declarationResponse, entryHeader);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			var expectedMessageInterpretation =
					"<H3>Accepted Declaration</H3>" +
					"<br><table border=\"0\"></table><table border=\"0\"></table><br><table border=\"0\"></table>" +
					"<br><H2>Taxes and fees data</H2>" +
					"<table border=\"0\"><tr><td>AEAT Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
					"<tr><td>ATC Dispatch Status:</td><td>&nbsp;&nbsp;</td><td>L - Accepted with Customs Clearance</td></tr>" +
					"<tr><td>AEAT Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contracted</td></tr>" +
					"<tr><td>ATC Accounting Status:</td><td>&nbsp;&nbsp;</td><td>CO - Contracted</td></tr>" +
					"<tr><td>AEAT Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr>" +
					"<tr><td>ATC Unfinished Pendencies:</td><td>&nbsp;&nbsp;</td><td>N</td></tr></table><br><br>" +
					"<table border=\"0\"><tr><td>Total:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
					"<tr><td>Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>-0.05</td></tr>" +
					"<tr><td>ATC Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
					"<tr><td>ATC Guaranteed Total:</td><td>&nbsp;&nbsp;</td><td>1.31</td></tr>" +
					"<tr><td>Total Deferred VAT:</td><td>&nbsp;&nbsp;</td><td>1.15</td></tr>" +
					"<tr><td>Clearance Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>1.05</td></tr>" +
					"<tr><td>Real Clearance Guarantee:</td><td>&nbsp;&nbsp;</td><td>0.10</td></tr>" +
					"<tr><td>Pendency Guarantee VAT Exemption:</td><td>&nbsp;&nbsp;</td><td>0.15</td></tr>" +
					"<tr><td>Real Pendency Guarantee:</td><td>&nbsp;&nbsp;</td><td>-0.15</td></tr></table>" +
					"<br><H2>Payment information</H2><br>" +
					"<table border=\"0\"><tr><td>Payment date limit:</td><td>&nbsp;&nbsp;</td><td>25-10-2022</td></tr>" +
					"<tr><td>ATC payment date limit:</td><td>&nbsp;&nbsp;</td><td>26-10-2022</td></tr></table><br>" +
					"<br><H2>Guarantees</H2><br>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
					"<thead><tr class=\"tableheadings\"><th>Customs</th><th>GRN</th><th>Real Debt</th><th>Potential Debt</th><th>Undetermined Real Debt</th></tr></thead>" +
					"<tr><td>AEAT</td><td>16ESAGL9990000096</td><td>45.13</td><td>0</td><td>0</td></tr>" +
					"<tr><td>AEAT</td><td>16ESAGL9990000095</td><td>1.15</td><td>1.15</td><td>1.15</td></tr>" +
					"<tr><td>ATC</td><td>18ESCGL9980000060</td><td>1.31</td><td>1.31</td><td>1.31</td></tr></table>" +
					"<br><br><H2>Taxes and fees response (Spanish Customs)</H2><br>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
					"<thead><tr class=\"tableheadings\"><th>Item</th><th>Type</th><th>MAX/MIN Rate</th><th>Base Amount</th><th>Tax Rate</th><th>Total Amount</th><th>Total Guaranteed Amount</th></tr></thead>" +
					"<tr><td>1</td><td>A00</td><td>MA</td><td>42.560</td><td>2.700000 %</td><td>1.15</td><td>1.15</td></tr>" +
					"<tr><td>2</td><td>A00</td><td>&nbsp;</td><td>1219.350</td><td>17.600000 %</td><td>214.61</td><td>214.61</td></tr>" +
					"<tr><td>2</td><td>B00</td><td>&nbsp;</td><td>1481.380</td><td>10.000000 %</td><td>148.14</td><td>148.14</td></tr>" +
					"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>363.90</td><td>363.90</td></tr>" +
					"<tr><td>1</td><td>3IG</td><td>&nbsp;</td><td>43.710</td><td>3.000000 €/KN</td><td>1.31</td><td>1.31</td></tr>" +
					"<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Total: </td><td>1.31</td><td>1.31</td></tr></table><br>" +
					"<br><H2>Required Certificates</H2><br>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
					"<thead><tr class=\"tableheadings\"><th>Item</th><th>Measure</th><th>Agency</th><th>Documents</th></tr></thead>" +
					"<tr><td>1</td><td>SNM</td><td>SIF05 -  Sanidad Exterior - M&#186; Sanidad</td><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715], " +
					"C657, C678, 1405, 1413, C640</td></tr>" +
					"<tr><td>1</td><td>VIM</td><td>SIF02 -  Sanidad Animal, M Agricultura</td><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715], " +
					"C657, C678, C640</td></tr>" +
					"<tr><td>2</td><td>SNM</td><td>SIF06 -  Sanidad Interior - M&#186; Sanidad</td><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS (DSCE-P)[anex II-2, secc B, R/UE 2019/1715], " +
					"C657, C678, 1405, 1413, C640</td></tr>" +
					"</table>";
			AssertEquals("Expected Accepted declaration message interpretation text", expectedMessageInterpretation, messageInterpretationText);
		}

		public void TestCreateMessageDetailsAcceptedCompleteDeclarationDeferred()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

			entryHeader.TotalAmount = 2.15;
			var declarationResponse = new ImportDocCas44PendV1Sal();
			declarationResponse.TotalApagar = 1.15M;

			var messagePrettyFormatter = new Box44ImportMessagePrettyFormatter(declarationResponse, entryHeader);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertContains("<H3>Warning: Taxes and fees data received differ from sent data</H3>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var declarationResponse = new ImportDocCas44PendV1Sal();

			var error1 = SetError("1001", "Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS", 001, 401, "C40ClaseDocumento", "KK");
			var error2 = SetError("1001", "Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS", 002, 401, "C40ClaseDocumento", "KK");
			declarationResponse.Error = new Collection<ErrorTd> { error1, error2 };

			var messagePrettyFormatter = new Box44ImportMessagePrettyFormatter(declarationResponse, entryHeader);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
				"<tr><td>1001</td><td>1.401<br>Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS.C40ClaseDocumento.KK</td></tr>" +
				"<tr><td>1001</td><td>2.401<br>Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS.C40ClaseDocumento.KK</td></tr></table>", messageInterpretationText);
		}

		Cas47TributoLiquidadoTd SetTribute(ZString chargeType, ZDecimal baseValue, ZDecimal rate, ZString maxMin, ZString rateDuty, ZDecimal chargeAmount, ZDecimal chargeAmountGuarantee)
		{
			var tribute = new Cas47TributoLiquidadoTd();
			tribute.C47TributoClase = chargeType;
			tribute.C47TributoBaseImponible = baseValue;
			tribute.C47TributoTipoImpositivo = rate;
			tribute.C47TributoIndicadorMaxMinNor = maxMin;
			tribute.C47TributoUnidadFiscal = rateDuty;
			tribute.C47TributoCuotaPaga = chargeAmount;
			tribute.C47TributoCuotaGarantiza = chargeAmountGuarantee;
			return tribute;
		}

		GarantiaGrNutilizadaTd SetGRNGuarantee(ZString grn, ZDecimal realDebt, ZDecimal potentialDebt, ZDecimal realUndeterminedDebt)
		{
			var guarantee = new GarantiaGrNutilizadaTd();
			guarantee.CBgarantiaGrn = grn;
			guarantee.CBimporteReal = realDebt;
			guarantee.CBimportePotencial = potentialDebt;
			guarantee.CBimporteRealSinDeterminar = realUndeterminedDebt;
			return guarantee;
		}

		CertiControlTd SetCertificate(ZString measure, ZString organisation, ZString organisationName, Collection<string> certificateTypes)
		{
			var certificate = new CertiControlTd();
			certificate.Medida = measure;
			certificate.Organismo = organisation;
			certificate.NombreOrganismo = organisationName;
			certificate.TipoCertificado = certificateTypes;
			return certificate;
		}

		ErrorTd SetError(ZString errorCode, ZString errorDescription, ZInt errorLineNumber, ZInt errorElementNumber, ZString errorTag, ZString wrongValue)
		{
			var error = new ErrorTd();
			error.CodigoError = errorCode;
			error.DescripcionError = errorDescription;
			error.NumeroOrdenPartidaConError = errorLineNumber;
			error.NumeroOrdenElementoErroneo = errorElementNumber;
			error.EtiquetaConError = errorTag;
			error.ValorErroneo = wrongValue;
			return error;
		}
	}
}
