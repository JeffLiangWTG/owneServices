using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseAcceptResponseObjectParent))]
	class ImportLicenseAcceptResponseObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportLicenseAcceptResponseObjectParent(Factory.New<JobDeclaration>());
		}

		#endregion

		public void TestHumanReadableName()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseAcceptResponseObjectParent;
			AssertEquals("Load Response from Customs", importLicenseParent.HumanReadableName);
		}

		public void TestResponseHasDiagnosis()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseAcceptResponseObjectParent;
			AssertEquals(true, importLicenseParent.ResponseHasDiagnosis);
		}

		public void TestResponseHasReferenceNumber()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseAcceptResponseObjectParent;
			AssertEquals(true, importLicenseParent.ResponseHasReferenceNumber);
		}

		public void TestResponseHasStatus()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseAcceptResponseObjectParent;
			AssertEquals(false, importLicenseParent.ResponseHasStatus);
		}

		public void TestImportLicenseResponseObjects()
		{
			var importLicenseParent = GetNewBusinessObject() as ImportLicenseAcceptResponseObjectParent;
			AssertNotNull("ImportLicenseResponseObjects should not be null.", importLicenseParent.Collection);
		}

		public void TestLoadResponseXML()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			declaration.JE_OH_Importer = importer.PK;

			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message.EM_ApplicationReference = "000001";

			var message2 = Factory.NewWithValidTestData<BREDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message2.EM_ApplicationReference = "000001";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			Factory.Save();

			entry.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010021";
			entry2.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010022";

			message.EM_LinkedObject = entry;
			message2.EM_LinkedObject = entry2;

			Factory.Save();

			entry.Messages.Add(message);
			entry2.Messages.Add(message2);

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				var messageBuilder = new ZStringBuilder();
				var importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertEquals("(100/100) File reading complete!\r\n", messageBuilder.ToString());

				AssertEquals("ImportLicenseResponseObjectCollection count should be", 2, importLicenseParent.Collection.Count);
				var response1 = importLicenseParent.Collection[0];
				AssertEquals("response1.EntryNumber should be", "BXI000010021", response1.ReferenceNumber);
				AssertEquals("response1.EntryNumber should be", "2200000000", response1.EntryNumber);
				AssertEquals("response1.EntryNumber should be", "07/04/2022", response1.RegistrationDate);
				AssertEquals("response1.EntryNumber should be", "PENDENCIA DE AUTORIZACAO ANVISA; TEST", response1.Diagnosis);

				var response2 = importLicenseParent.Collection[1];
				AssertEquals("response2.EntryNumber should be", "BXI000010022", response2.ReferenceNumber);
				AssertEquals("response2.EntryNumber should be", ZString.Empty, response2.EntryNumber);
				AssertEquals("response2.EntryNumber should be", "08/04/2022", response2.RegistrationDate);
				AssertEquals("response2.EntryNumber should be", "Licenciamento de Importação foi registrado", response2.Diagnosis);
			}
		}

		public void TestLoadResponseXMLWrongFile()
		{
			var messageBuilder = new ZStringBuilder();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(wrongResponseXML)))
			{
				var importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
				importLicenseParent.AddLog = AppendLog(messageBuilder);
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertEquals("Load result should be 'Unable to read Response.xml'", "(100/100) Unable to read Response.xml\r\n", messageBuilder.ToString());
			}
		}

		public void TestLoadResponseXMLError()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var message = Factory.NewWithValidTestData<BREDIMessage>();
			message.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message.EM_ApplicationReference = "RLI00000000001000002";

			var message2 = Factory.NewWithValidTestData<BREDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			message2.EM_ApplicationReference = "RLI00000000001000002";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			Factory.Save();
			entry2.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010022";

			var messageBuilder = new ZStringBuilder();
			var importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
			importLicenseParent.AddLog = AppendLog(messageBuilder);

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertEquals("EM_ApplicationReference mismatch with <idlote>", "(100/100) The Batch Identifier contained in the XML file (RLI00000000001000001) does not match any Entry of this Job.\r\n", messageBuilder.ToString());
			}

			message.EM_ApplicationReference = "000001";
			message2.EM_ApplicationReference = "000001";

			message.EM_LinkedObject = entry;
			message2.EM_LinkedObject = entry2;
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			Factory.Save();

			entry.Messages.Add(message);
			entry2.Messages.Add(message2);

			messageBuilder.Clear();
			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertContains("System should not locate any CusEntryheader with the reference (BXI000010021) ", "The Import License Identifier contained in the XML file (BXI000010021) does not match any Entry of this Job.", messageBuilder.ToString());
			}

			entry.ImportLicenseIdentifierNumber.CE_EntryNum = "BXI000010021";
			importer.PrimaryRegistrationNumber.Number = "00000000000002";
			Factory.Save();

			messageBuilder.Clear();
			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertContains("System should not match the importer number", "The Importer Registration Number contained in the XML file(00000000000001) does not match the Importer of this Job. The Import License Identifier is BXI000010021.", messageBuilder.ToString());
			}

			entry.MovementReferenceNumberSetter("2200000000");
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			Factory.Save();

			messageBuilder.Clear();
			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertContains("System should warm this message", "An XML file containing the following(s) Import License(s) was already loaded. Import License Number: 2200000000.", messageBuilder.ToString());
			}

			entry.MovementReferenceNumberSetter("2200000002");
			Factory.Save();

			messageBuilder.Clear();
			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertContains("System should warm this message", "The Entry Header B00001000-1 Import License Number (2200000002) differs from the Import License Number contained in the XML file (2200000000).", messageBuilder.ToString());
			}

			entry2.CH_Status = BRMessageStatusList.Codes.Rejected;
			entry2.Logs.AddNew(Events.MessageRejected, "|CRF=RLI00000000001000001");
			Factory.Save();
			messageBuilder.Clear();
			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				AssertContains("System should warm this message", "An XML file was already loaded for the Batch Identifier <RLI00000000001000001> and Import License Identifier <BXI000010022>.", messageBuilder.ToString());
			}
		}

		public void TestCreateEDIInterchange()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.PrimaryRegistrationNumber.Number = "00000000000001";
			declaration.JE_OH_Importer = importer.PK;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			entry1.ImportLicenseIdentifier = "BXI000010021";
			entry2.ImportLicenseIdentifier = "BXI000010022";

			var outgoingMessage1 = Factory.NewWithValidTestData<BREDIMessage>();
			outgoingMessage1.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			outgoingMessage1.EM_ApplicationReference = "000001";
			entry1.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.NewWithValidTestData<BREDIMessage>();
			outgoingMessage2.EM_ApplicationCode = ApplicationCodes.BRCustoms;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			outgoingMessage2.EM_ApplicationReference = "000001";
			outgoingMessage2.EM_LinkedObject = entry2;
			entry2.Messages.Add(outgoingMessage2);

			Factory.Save();

			var messageBuilder = new ZStringBuilder();
			var importLicenseParent = new ImportLicenseAcceptResponseObjectParent(declaration);
			importLicenseParent.AddLog = AppendLog(messageBuilder);

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(wrongResponseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
				messageBuilder.Clear();
			}
			var succeed = importLicenseParent.CreateDataFromXml();
			AssertEquals("Create EDIInterchange result should be", false, succeed);
			AssertEquals("Log", "(100/100) Valid response file has not been loaded.\r\n", messageBuilder.ToString());

			using (var response = new MemoryStream(Encoding.UTF8.GetBytes(responseXML)))
			{
				importLicenseParent.LoadAndValidateXML("Response.xml", response);
			}
			messageBuilder.Clear();
			succeed = importLicenseParent.CreateDataFromXml();
			AssertEquals("Create EDIInterchange result should be", true, succeed);
			AssertEquals("Log", "(0/2) Response Message 1 processed.\r\n(1/2) Response Message 2 processed.\r\n", messageBuilder.ToString());

			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, MessageTypeList.Codes.LIC));

			CombineAssertions(() =>
			{
				AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.BRCustoms, interchange.EI_ApplicationCode);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_From", BREDIInterchange.BRCustoms, interchange.EI_From);
				AssertEquals("EI_To", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
				AssertEquals("EI_Status", EDIMessageStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, interchange.EI_GB);
				AssertEquals("EI_BodyText", responseXML, interchange.EI_BodyText);

				AssertEquals("EDIMessages created", 2, interchange.ContainedMessages.Count);

				foreach (EDIMessage message in interchange.ContainedMessages)
				{
					AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
					AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
					AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
					AssertEquals("EM_MessageType", MessageTypeList.Codes.LIC, message.EM_MessageType);
					AssertEquals("EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
					AssertEquals("EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
					Assert("EM_MessageText", !message.EM_MessageText.IsEmpty);
				}

				AssertEquals("CH_Status for BXI000010021", BRMessageStatusList.Codes.Accepted, entry1.CH_Status);
				AssertEquals("MRN for BXI000010021", "2200000000", entry1.MovementReferenceNumber);
				AssertEquals("CH_Status for BXI000010022", BRMessageStatusList.Codes.Rejected, entry2.CH_Status);
				AssertEquals("MRN for BXI000010022", "", entry2.MovementReferenceNumber);
			});
		}

		Action<int, int, string> AppendLog(ZStringBuilder messageBuilder)
		{
			return (completedCount, totalCount, messageText) => messageBuilder.AppendLine($"({completedCount}/{totalCount}) {messageText}");
		}

		readonly string responseXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<lote-li>
	<cpfUsuario>000.000.000-01</cpfUsuario>
	<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
	<idLote>RLI00000000001000001</idLote>
	<listaLIVORetorno>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010021</idSolicitacao>
			<importador>
				<numero>00000000000001</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>PENDENCIA DE AUTORIZACAO ANVISA</mensagemDiagnostico>
				<mensagemDiagnostico>TEST</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI>2200000000</numeroLI>
		</li>
		<li>
			<dtRegistro>08/04/2022</dtRegistro>
			<idSolicitacao>BXI000010022</idSolicitacao>
			<importador>
				<numero>00000000000001</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>Licenciamento de Importação foi registrado</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI/>
		</li>
	</listaLIVORetorno>
	<versao/>
	<versaoValida>true</versaoValida>
</lote-li>";

		readonly string wrongResponseXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
	<cpfUsuario>000.000.000-01</cpfUsuario>
	<dataHoraEnvioFormatada>07/04/2022 09:00:00</dataHoraEnvioFormatada>
	<idLote>RLI00000000001000001</idLote>
	<listaLIVORetorno>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010021</idSolicitacao>
			<importador>
				<numero>00000000000001</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>ERRO NA COMUNICAÇÃO COM O DRAWBACK ISENÇÃO</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI/>
		</li>
		<li>
			<dtRegistro>07/04/2022</dtRegistro>
			<idSolicitacao>BXI000010022</idSolicitacao>
			<importador>
				<numero>00000000000001</numero>
				<tipoImportador>J</tipoImportador>
			</importador>
			<mensagemDiagnostico>
				<mensagemDiagnostico>Licenciamento de Importação foi registrado</mensagemDiagnostico>
			</mensagemDiagnostico>
			<numeroLI>2200000001</numeroLI>
		</li>
	</listaLIVORetorno>
	<versao/>
	<versaoValida>true</versaoValida>";
	}
}

