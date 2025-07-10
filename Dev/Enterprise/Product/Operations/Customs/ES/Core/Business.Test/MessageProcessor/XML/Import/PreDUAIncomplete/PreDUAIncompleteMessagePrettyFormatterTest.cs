using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PreDUAIncompleteMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestCreateMessageDetailsAccepted()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var esCode = CountryCodes.Spain;
			var supportingDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			helper.CreateNewOrGetExistingCusCodeType(supportingDocumentType, "Supporting Documents for Import");
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "N851", "CERTIFICADO FITOSANITARIO", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C085", "DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "N853", "DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C657", "CERTIFICADO DE SANIDAD", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "C678", "DOC.SANIT.COMUN PIENSOS+ALIMENTOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "1405", "INSPECCION SANIDAD EXTERIOR. NO PROCEDE", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "1413", "INSPECCION SANIDAD EXTERIOR-NO AFECTADOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(esCode, supportingDocumentType, "N003", "CERTIFICADO DE CALIDAD-R/UE", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateNewOrGetExistingDataGrouping(esCode, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

			var declarationResponse = new PreDeclaIncompletaV1Sal();

			declarationResponse.Partida = new Collection<PartidaTd> { };
			declarationResponse.CsVdeDeclaracionElectronica = "SZL6GY3FP7WLB5NP";
			var line1 = new PartidaTd();
			line1.C32NumeroDePartida = 1;

			var certificate1 = SetCertificate("SNM", "SIF05", "Sanidad Exterior - Mº Sanidad", new Collection<string> { "N853", "N003", "C678", "1405", "1413", "C085" });
			var certificate2 = SetCertificate("VIM", "SIF02", "Sanidad Animal, M Agricultura", new Collection<string> { "N851", "C657", "C678", "C085" });
			line1.CertificadosRequeridos = new Collection<CertiControlTd> { certificate1, certificate2 };

			var line2 = new PartidaTd();
			line2.C32NumeroDePartida = 2;

			var certificate3 = SetCertificate("SVI", "SIF06", " Sanidad Interior - Mº Sanidad", new Collection<string> { "N853", "C657", "C678", "1405", "1413", "C085" });
			line2.CertificadosRequeridos = new Collection<CertiControlTd> { certificate3 };

			declarationResponse.Partida = new Collection<PartidaTd> { line1, line2 };

			var messagePrettyFormatter = new PreDUAIncompleteMessagePrettyFormatter(declarationResponse, entryHeader);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsAccepted();

			AssertEquals("<H3>Accepted Declaration</H3><br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>SZL6GY3FP7WLB5NP</td></tr></table>" +
						"<br><br><H3>Required Certificates</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Item</strong></td><td><strong>Measure</strong></td><td><strong>Agency</strong></td><td><strong>Documents</strong></td></tr>" +
						"<tr><td>1</td><td>SNM</td><td>SIF05 Sanidad Exterior - Mº Sanidad</td>" +
							"<td><table width=\"100%\">" +
								"<tr><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)</td></tr>" +
								"<tr><td>N003 - CERTIFICADO DE CALIDAD-R/UE</td></tr>" +
								"<tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr>" +
								"<tr><td>1405 - INSPECCION SANIDAD EXTERIOR. NO PROCEDE</td></tr>" +
								"<tr><td>1413 - INSPECCION SANIDAD EXTERIOR-NO AFECTADOS</td></tr>" +
								"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
							"</table></td></tr>" +
						"<tr><td>1</td><td>VIM</td><td>SIF02 Sanidad Animal, M Agricultura</td>" +
							"<td><table width=\"100%\">" +
								"<tr><td>N851 - CERTIFICADO FITOSANITARIO</td></tr>" +
								"<tr><td>C657 - CERTIFICADO DE SANIDAD</td></tr>" +
								"<tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr>" +
								"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
							"</table></td></tr>" +
						"<tr><td>2</td><td>SVI</td><td>SIF06  Sanidad Interior - Mº Sanidad</td>" +
							"<td><table width=\"100%\">" +
								"<tr><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)</td></tr>" +
								"<tr><td>C657 - CERTIFICADO DE SANIDAD</td></tr>" +
								"<tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr>" +
								"<tr><td>1405 - INSPECCION SANIDAD EXTERIOR. NO PROCEDE</td></tr>" +
								"<tr><td>1413 - INSPECCION SANIDAD EXTERIOR-NO AFECTADOS</td></tr>" +
								"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
							"</table></td></tr>" +
						"</table>", messageInterpretationText);
		}

		public void TestCreateMessageDetailsRejected()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var declarationResponse = new PreDeclaIncompletaV1Sal();

			declarationResponse.CodigoRespuesta = "1001";
			declarationResponse.DescripcionRespuesta = "Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS";
			declarationResponse.NumeroOrdenPartidaConError = 001;

			var messagePrettyFormatter = new PreDUAIncompleteMessagePrettyFormatter(declarationResponse, entryHeader);

			var messageInterpretationText = messagePrettyFormatter.CreateMessageDetailsRejected();

			AssertEquals("<H4>Error = 1001 - Partida(1).CASILLA 40.CODIGO DOCUMENTO (CAS 40). LOS BULTOS A DATAR EXCEDEN EL SALDO DE LOS BULTOS DISPONIBLESERROR EN DATADO EN SUMARIAS</H4><H4>Item = 1</H4>", messageInterpretationText);
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
	}
}
