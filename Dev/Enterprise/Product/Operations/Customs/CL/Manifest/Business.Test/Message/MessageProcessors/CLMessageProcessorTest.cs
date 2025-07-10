using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	sealed class CLMessageProcessorTest : TestCaseWithFactory
	{
		readonly CLBranchMessageProcessor processor = new CLBranchMessageProcessor { Logger = new LoggingInformation() };

		public void TestProcessMessageEmptyAndInvalidBody()
		{
			var message1 = CreateMessage(string.Empty, MessageTypes.Codes.CHB, ZGuid.Empty);
			var message2 = CreateMessage("BODYTEXT", MessageTypes.Codes.CHB, ZGuid.Empty);

			processor.ExecuteBatch();
			message1.Reload();
			message2.Reload();

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message2.EM_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessBillNotFound()
		{
			CreateHeader();
			var message = CreateMessage(AcceptedResponse, MessageTypes.Codes.CHB, ZGuid.Empty);

			processor.ExecuteBatch();
			message.Reload();

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessInvalidResponseFirstSubmit()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLTAO239432";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "SNT";
			Factory.Save();

			var message = CreateMessage(InvalidResponse, MessageTypes.Codes.CHB, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): (H)ATLTAO239432
Status: Invalid

Datetime sent: 2021-07-20 11:59:31
Datetime response: 2021-07-20 12:01:53

";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLTAO239432 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When processing a response with invalid status, if it is a first submit, the bill status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_BillStatus);
				AssertEquals("When processing a response with invalid status, if it is a first submit, the Message status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessInvalidResponseResend()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLTAO239432";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "ACP";
			Factory.Save();

			var message = CreateMessage(InvalidResponse, MessageTypes.Codes.CHA, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): (H)ATLTAO239432
Status: Invalid

Datetime sent: 2021-07-20 11:59:31
Datetime response: 2021-07-20 12:01:53

";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLTAO239432 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When processing a response with invalid status, if it is a Resend/Cancel, the Bill status must be kept as ACP.", CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("When processing a response with invalid status, if it is a Resend/Cancel, the Message status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessInvalidResponseCancel()
		{
			var header = CreateHeader();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)ATLTAO239432";
			bill.ABL_MessageStatus = "AWA";
			bill.ABL_BillStatus = "ACP";
			Factory.Save();

			var message = CreateMessage(InvalidResponse, MessageTypes.Codes.CHC, bill.PK);

			processor.ExecuteBatch();
			message.Reload();
			header.Reload();
			bill.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response (Failure) for " + header.AMA_JobReference);
			AssertNotNull(email);

			var body = email.Body;
			var recipients = email.Recipients;

			var expectedresult = @"Bill Number (numero-referencia): (H)ATLTAO239432
Status: Invalid

Datetime sent: 2021-07-20 11:59:31
Datetime response: 2021-07-20 12:01:53

";

			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", recipients[0].Email);
				Assert(body.Contains("Manifest Message for job MAN0000001, bill (H)ATLTAO239432 has been rejected. For details please follow the Link to the Manifest"));
				Assert("Contains Column", body.Contains("Column"));
				Assert("Contains Value", body.Contains("Value"));

				AssertEquals(bill.PK, message.EM_LinkUniqueID);
				AssertEquals(AsycudaBillSchema.Constants.TableName, message.EM_LinkTable);

				AssertEquals("When processing a response with invalid status, if it is a Resend/Cancel, the Bill status must be kept as ACP.", CustomsStatusList.Codes.ACP, bill.ABL_BillStatus);
				AssertEquals("When processing a response with invalid status, if it is a Resend/Cancel, the Message status must be ERR.", CustomsStatusList.Codes.ERR, bill.ABL_MessageStatus);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals(expectedresult, message.EM_MessageInterpretation);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageFormatXML()
		{
			var expectedresult = @"<?xml version=""1.0"" encoding=""iso-8859-1""?>
<Documento tipo=""BL"" version=""1.0"">
  <user-host-origen>SOFTCARGO/192.168.1.150</user-host-origen>
  <id-sender>SMS-3.3.20P</id-sender>
  <numero-referencia>(H)ATLUKB239892</numero-referencia>
  <unidad-peso>KGM</unidad-peso>
  <id-documento-servidor>68630844</id-documento-servidor>
  <tipo-accion>I</tipo-accion>
  <service>LINER</service>
  <total-peso>18740.000</total-peso>
  <total-bultos>1</total-bultos>
  <total-volumen>40.00</total-volumen>
  <total-item>1</total-item>
  <tipo-servicio>FCL/FCL</tipo-servicio>
  <unidad-volumen>MTQ</unidad-volumen>
  <Control>
    <item-control>
      <fecha-hora-local>2020-06-16 02:30:01</fecha-hora-local>
      <operacion>Enviado al servidor SOAP</operacion>
    </item-control>
    <item-control>
      <fecha-hora-local>2020-06-16 02:30:32</fecha-hora-local>
      <operacion>Respuesta recibida desde el servidor</operacion>
      <estado>Aprobado</estado>
      <Detalles>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participaciones, para [ALM] [nombres] [<IQUIQUE TERMINAL INTERNACIONAL S.A.>] para [valor-id] [<96915330-0>], es distinto al registrado en el sistema]]></valor>
        </detalle>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participaciones, para [EMI] [nombres] [<INTERLINE LTDA>] para [valor-id] [<77777060-8>], es distinto al registrado en el sistema]]></valor>
        </detalle>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participaciones, para [REP] [nombres] [<INTERLINE LTDA>] para [valor-id] [<77777060-8>], es distinto al registrado en el sistema]]></valor>
        </detalle>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participacion[EMB] el atributo [telefono] <S/I> debe ser un número válido.]]></valor>
        </detalle>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participacion[CONS] el atributo [telefono] <57 2 523813> debe ser un número válido.]]></valor>
        </detalle>
        <detalle>
          <tipo>Advertencia</tipo>
          <valor><![CDATA[En Participacion[NOTI] el atributo [telefono] <S/I> debe ser un número válido.]]></valor>
        </detalle>
        <detalle>
          <tipo>Mensaje</tipo>
          <valor><![CDATA[[16/06/2020 02:30:14] Mensaje recibido en servidor]]></valor>
        </detalle>
        <detalle>
          <tipo>Mensaje</tipo>
          <valor><![CDATA[[16/06/2020 02:30:15] Inicio recepcion]]></valor>
        </detalle>
        <detalle>
          <tipo>Mensaje</tipo>
          <valor><![CDATA[[16-06-2020 02:30:16] Fin recepcion]]></valor>
        </detalle>
        <detalle>
          <tipo>Mensaje</tipo>
          <valor><![CDATA[[16-06-2020 02:30:16] Termino procesado en servidor.]]></valor>
        </detalle>
        <detalle>
          <tipo>Mensaje</tipo>
          <valor><![CDATA[Tiempo de procesado: 2481 [ms]]]></valor>
        </detalle>
      </Detalles>
    </item-control>
  </Control>
  <Transbordos>
    <transbordo>
      <cod-lugar>KRPUS</cod-lugar>
      <descripcion-lugar>BUSAN</descripcion-lugar>
      <fecha-arribo>20-05-2020 00:00</fecha-arribo>
    </transbordo>
  </Transbordos>
  <Locaciones>
    <locacion>
      <descripcion>KOBE</descripcion>
      <nombre>LE</nombre>
      <codigo>JPUKB</codigo>
    </locacion>
    <locacion>
      <descripcion>KOBE</descripcion>
      <nombre>PE</nombre>
      <codigo>JPUKB</codigo>
    </locacion>
    <locacion>
      <descripcion>IQUIQUE</descripcion>
      <nombre>PD</nombre>
      <codigo>CLIQQ</codigo>
    </locacion>
    <locacion>
      <descripcion>IQUIQUE</descripcion>
      <nombre>LD</nombre>
      <codigo>CLIQQ</codigo>
    </locacion>
    <locacion>
      <descripcion>IQUIQUE</descripcion>
      <nombre>LEM</nombre>
      <codigo>CLIQQ</codigo>
    </locacion>
    <locacion>
      <descripcion>KOBE</descripcion>
      <nombre>LRM</nombre>
      <codigo>JPUKB</codigo>
    </locacion>
  </Locaciones>
  <Items>
    <item>
      <marcas>S/I</marcas>
      <observaciones>S/I</observaciones>
      <tipo-bulto>78</tipo-bulto>
      <descripcion>1X40´HC STC.: 766 PAQUETES CONTENIENDO VEHICULOS Y REPUESTOS USADOS</descripcion>
      <carga-peligrosa>N</carga-peligrosa>
      <numero-item>1</numero-item>
      <unidad-peso>KGM</unidad-peso>
      <cantidad>1</cantidad>
      <unidad-volumen>MTQ</unidad-volumen>
      <peso-bruto>18740.000</peso-bruto>
      <volumen>40.00</volumen>
      <Contenedores>
        <contenedor>
          <digito>4</digito>
          <numero>663814</numero>
          <status>FCL/FCL</status>
          <sigla>INKU</sigla>
          <peso>18740.000</peso>
          <tipo-cnt>45G0</tipo-cnt>
          <nombre-operador>MEDITERRANEAN SHIPPING COMPANY CHILE S.A.</nombre-operador>
          <Sellos>
            <sello>
              <numero>FJ08527500</numero>
            </sello>
          </Sellos>
        </contenedor>
      </Contenedores>
    </item>
  </Items>
  <OpTransporte>
    <optransporte>
      <nombre-nave>MSC CLEA</nombre-nave>
      <sentido-operacion>I</sentido-operacion>
    </optransporte>
  </OpTransporte>
  <Referencias>
    <referencia>
      <tipo-referencia>REF</tipo-referencia>
      <numero>194914</numero>
      <fecha>05-06-2020</fecha>
      <tipo-documento>MFTO</tipo-documento>
    </referencia>
    <referencia>
      <tipo-referencia>MADRE</tipo-referencia>
      <numero>MEDUJP845558</numero>
      <fecha>05-06-2020</fecha>
      <tipo-documento>BL</tipo-documento>
    </referencia>
  </Referencias>
  <Fechas>
    <fecha>
      <valor>16-06-2020 02:29</valor>
      <nombre>FPRES</nombre>
    </fecha>
    <fecha>
      <valor>05-06-2020</valor>
      <nombre>FEM</nombre>
    </fecha>
    <fecha>
      <valor>17-05-2020 00:00</valor>
      <nombre>FEMB</nombre>
    </fecha>
    <fecha>
      <valor>17-05-2020 00:00</valor>
      <nombre>FZARPE</nombre>
    </fecha>
  </Fechas>
  <Participaciones>
    <participacion>
      <nacion-id>CL</nacion-id>
      <valor-id>96915330-0</valor-id>
      <nombres>IQUIQUE TERMINAL INTERNACIONAL S.A.</nombres>
      <codigo-almacen>A-39</codigo-almacen>
      <nombre>ALM</nombre>
      <tipo-id>RUT</tipo-id>
    </participacion>
    <participacion>
      <nacion-id>CL</nacion-id>
      <valor-id>77777060-8</valor-id>
      <nombres>INTERLINE LTDA</nombres>
      <nombre>EMI</nombre>
      <tipo-id>RUT</tipo-id>
    </participacion>
    <participacion>
      <nacion-id>CL</nacion-id>
      <valor-id>77777060-8</valor-id>
      <nombres>INTERLINE LTDA</nombres>
      <nombre>REP</nombre>
      <tipo-id>RUT</tipo-id>
    </participacion>
    <participacion>
      <nacion-id>CL</nacion-id>
      <nombres>ATLANTIC FORWARDING GROUP</nombres>
      <nombre>EMIDO</nombre>
    </participacion>
    <participacion>
      <nacion-id>JP</nacion-id>
      <telefono>S/I</telefono>
      <nombres>SAKURAI CORPORATION</nombres>
      <direccion>15-4 NISHIMUKOUIMA-CHO AMAGASAKI-SHI HYOGO 660-085</direccion>
      <comuna>13101</comuna>
      <correo-electronico>deptoimport2015@gmail.com</correo-electronico>
      <nombre>EMB</nombre>
    </participacion>
    <participacion>
      <valor-id>76623447-K</valor-id>
      <nombres>IMPORT Y EXPORT SAKURAI CORP. LIMITADA</nombres>
      <direccion>STA ROSA DE HUARA, MANZANA D, SITIO 28, ZONA FRANC</direccion>
      <comuna>01101</comuna>
      <tipo-id>RUT</tipo-id>
      <nacion-id>CL</nacion-id>
      <telefono>57 2 523813</telefono>
      <nombre>CONS</nombre>
      <correo-electronico>KING.AUTO.PARTS.LTDA@GMAIL.COM</correo-electronico>
    </participacion>
    <participacion>
      <telefono>S/I</telefono>
      <valor-id>S/I</valor-id>
      <nombres>SAME AS CONSIGNEE</nombres>
      <direccion>XXX</direccion>
      <comuna>13101</comuna>
      <correo-electronico>deptoimport2015@gmail.com</correo-electronico>
      <nombre>NOTI</nombre>
      <tipo-id>PAS</tipo-id>
    </participacion>
  </Participaciones>
  <Flete>
    <forma-pago-flete>
      <tipo>COLLECT</tipo>
    </forma-pago-flete>
  </Flete>
</Documento>";

			AssertMultilineASCIIEquals("Text from  CL Messages should be formatted like an xml message", expectedresult, AcceptedResponse);
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_JobReference = "MAN0000001";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Chile;

			Factory.Save();
			return header;
		}

		CLMessage CreateMessage(ZString messageText, ZString messageType, ZGuid billPK)
		{
			var message = Factory.New<CLMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_ApplicationReference = "0000000001";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = "0000000001";
			message.EM_MessageText = messageText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkTable = (billPK == ZGuid.Empty ? string.Empty : AsycudaBillSchema.Constants.TableName);
			message.EM_LinkUniqueID = billPK;
			message.EM_SystemCreateUser = Staff.GS_Code;

			Factory.Save();
			return message;
		}

		GlbStaff Staff
		{
			get { return staff ?? (staff = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com")); }
		}
		GlbStaff staff;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;

			Factory.Save();
			return staff;
		}

		string AcceptedResponse => acceptedResponse ?? (acceptedResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaAcceptedResponse)));
		string acceptedResponse;

		string InvalidResponse => invalidResponse ?? (invalidResponse = CLInboundInterchangeProcessorTest.GetExpectedMessageXML(Path.Combine(BaseSourcePath, CLMessagingConstants.SeaInvalidResponse)));
		string invalidResponse;
	}
}
