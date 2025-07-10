using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Grpc.Core;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class CLSMSMessageSenderControllerTest : TestCase
	{
		public class CLSMSMessageSenderControllerForTesting : CLSMSMessageSenderController
		{
			public CLSMSMessageSenderControllerForTesting(CancellationToken cancellationToken) : base(CLSMSSettingManagerForTesting.CorrectMachine, cancellationToken) { }

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				return new CLSMSSettingManagerForTesting(machineName, CLSMSTestHelper.CreateFakeWebClient(CLSMSSettingManagerForTesting.CorrectMachine));
			}

			public new CLSMSSettingManagerForTesting SettingManager => (CLSMSSettingManagerForTesting)base.SettingManager;

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			protected override void Process()
			{
				base.Process();
				ShouldStop = true;
			}

			public void Test_ProcessCore()
			{
				base.ProcessCore(SettingManager.CurrentSetting);
			}
		}

		public void TestProcess()
		{
			var testXml = @"<?xml version=""1.0"" encoding=""ISO-8859-1"" ?>
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
					<valor>
						<![CDATA[ En Participaciones, para [ALM] [nombres] [<IQUIQUE TERMINAL INTERNACIONAL S.A.>] para [valor-id] [<96915330-0>], es distinto al registrado en el sistema]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Advertencia</tipo>
					<valor>
						<![CDATA[ En Participaciones, para [EMI] [nombres] [<INTERLINE LTDA>] para [valor-id] [<77777060-8>], es distinto al registrado en el sistema]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Advertencia</tipo>
					<valor>
						<![CDATA[ En Participaciones, para [REP] [nombres] [<INTERLINE LTDA>] para [valor-id] [<77777060-8>], es distinto al registrado en el sistema]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Advertencia</tipo>
					<valor>
						<![CDATA[En Participacion[EMB] el atributo [telefono] <S/I> debe ser un número válido.]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Advertencia</tipo>
					<valor>
						<![CDATA[En Participacion[CONS] el atributo [telefono] <57 2 523813> debe ser un número válido.]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Advertencia</tipo>
					<valor>
						<![CDATA[En Participacion[NOTI] el atributo [telefono] <S/I> debe ser un número válido.]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Mensaje</tipo>
					<valor>
						<![CDATA[[16/06/2020 02:30:14] Mensaje recibido en servidor]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Mensaje</tipo>
					<valor>
						<![CDATA[[16/06/2020 02:30:15] Inicio recepcion]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Mensaje</tipo>
					<valor>
						<![CDATA[[16-06-2020 02:30:16] Fin recepcion]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Mensaje</tipo>
					<valor>
						<![CDATA[[16-06-2020 02:30:16] Termino procesado en servidor.]]>
					</valor>
				</detalle>
				<detalle>
					<tipo>Mensaje</tipo>
					<valor>
						<![CDATA[Tiempo de procesado: 2481 [ms]]]>
					</valor>
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
			var testPath = TestSenderController.SettingManager.CurrentSetting.AcceptedFolder;
			var messageMetaData = new Dictionary<string, string>();
			messageMetaData["custom.FileName"] = "TestFile.xml";

			var receiveHandler = new CLReceiveHandler(testPath, null);

			var msgIds = new List<MsgIdUri>();
			var msgIdUri = new MsgIdUri { Msgid = 100 };
			msgIds.Add(msgIdUri);

			receiveHandler.HandleReceivedMessageBatch(msgIds,
				msgid => messageMetaData,
				msgid => new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				msgData =>
				{
					var testMsgBody = new MemoryStream();
					var testMsgBytes = Encoding.UTF8.GetBytes(testXml);
					testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
					return testMsgBody;
				});

			var testResult = receiveHandler.MessageProcessingResults;

			AssertEquals(1, testResult.Count);
			AssertEquals(xTMessaging.Shared.Constants.MessageHandlingResultOperation.Success, testResult[msgIds[0]].Operation);
			Assert("Message is wrote down by xml", File.Exists(Path.Combine(testPath, "TestFile.xml")));
		}

		CLSMSMessageSenderControllerForTesting TestSenderController;
		StringBuilder TestLogger;
		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
			TestLogger = new StringBuilder();
			TestSenderController = new CLSMSMessageSenderControllerForTesting(cts.Token);
			TestSenderController.ShowInformation += (s, e) => TestLogger.AppendLine(e.Message);
			CLSMSTestHelper.DeleteTestFolders();
			CLSMSTestHelper.CreateTestFolders();
		}

		protected override void TearDown()
		{
			try
			{
				cts.Cancel();
			}
			finally
			{
				cts.Dispose();
			}
			CLSMSTestHelper.DeleteTestFolders();
			TestLogger.Clear();
			base.TearDown();
		}
	}
}
