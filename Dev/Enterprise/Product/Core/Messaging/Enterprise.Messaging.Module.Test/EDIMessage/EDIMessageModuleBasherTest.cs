using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDIMessageModule))]
	class EDIMessageModuleBasherTest : ZModuleBasherTest
	{
		public void TestAllowNewEdit()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowEdit", false, module.AllowEdit);
			}
		}

		public void TestExportUniversalInterchangeSchema_DeliveryMetadata()
		{
			using (var tempFileDirectory = new TempDirectory())
			{
				var tempDirectoryName = tempFileDirectory.DirectoryName;
				var xsdPath = Path.Combine(tempDirectoryName, "UniversalInterchange.xsd");
				var zipPath = Path.Combine(tempDirectoryName, UniversalZipName);

				using (TestEDIMessageModule module = new TestEDIMessageModule())
				using (ZForm form = new ZForm(module.FilterBusinessObject))
				{
					form.Controls.Add(module.EmbeddedControl);
					module.GridCollection.Load();
					form.Show();

					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var universalXmlSchemasMenuItem = actionMenuItem.MenuItems.FindByText("Save Universal + Native XML Schemas");
					AssertNotNull("UniversalXmlMessageMenuItem", universalXmlSchemasMenuItem);

					module.Directory = tempDirectoryName;
					AssertNoExceptionThrown("UniversalXmlSchemasGeneration", () => universalXmlSchemasMenuItem.PerformClick());

					ZipCompression.Unzip(zipPath, tempDirectoryName);
					AssertEquals("UniversalInterchange.xsd should be exported", true, File.Exists(xsdPath));

					var content = File.ReadAllText(xsdPath);
					AssertContains(DeliveryMetadata, content);
				}
			}
		}

		const string DeliveryMetadata = @"              <xs:element name=""DeliveryMetadata"" minOccurs=""0"">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name=""ValueCollection"" minOccurs=""0"">
                      <xs:complexType>
                        <xs:sequence>
                          <xs:element name=""Value"" maxOccurs=""unbounded"">
                            <xs:complexType>
                              <xs:sequence>
                                <xs:element name=""Name"" type=""xs:string"" />
                                <xs:element name=""Type"">
                                  <xs:simpleType>
                                    <xs:restriction base=""xs:string"">
                                      <xs:enumeration value=""String"" />
                                      <xs:enumeration value=""DateTime"" />
                                      <xs:enumeration value=""Integer"" />
                                      <xs:enumeration value=""Decimal"" />
                                      <xs:enumeration value=""Byte"" />
                                      <xs:enumeration value=""Boolean"" />
                                      <xs:enumeration value=""Short"" />
                                      <xs:enumeration value=""DateTimeOffset"" />
                                      <xs:enumeration value=""Geography"" />
                                      <xs:enumeration value=""Base64Binary"" />
                                    </xs:restriction>
                                  </xs:simpleType>
                                </xs:element>
                                <xs:element name=""Data"" type=""xs:string"" />
                              </xs:sequence>
                            </xs:complexType>
                          </xs:element>
                        </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:sequence>
                </xs:complexType>
              </xs:element>";

		public void TestAcknowledgementTagContent()
		{
			using (var tempFileDirectory = new TempDirectory())
			{
				var tempDirectoryName = tempFileDirectory.DirectoryName;
				var xsdPath = Path.Combine(tempDirectoryName, "UniversalInterchange.xsd");
				var zipPath = Path.Combine(tempDirectoryName, UniversalZipName);

				using (TestEDIMessageModule module = new TestEDIMessageModule())
				using (ZForm form = new ZForm(module.FilterBusinessObject))
				{
					form.Controls.Add(module.EmbeddedControl);
					module.GridCollection.Load();
					form.Show();

					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var universalXmlSchemasMenuItem = actionMenuItem.MenuItems.FindByText("Save Universal + Native XML Schemas");
					AssertNotNull("UniversalXmlMessageMenuItem", universalXmlSchemasMenuItem);

					module.Directory = tempDirectoryName;
					AssertNoExceptionThrown("UniversalXmlSchemasGeneration", () => universalXmlSchemasMenuItem.PerformClick());

					ZipCompression.Unzip(zipPath, tempDirectoryName);
					AssertEquals("UniversalInterchange.xsd should be exported", true, File.Exists(xsdPath));
				}

				var xmlDoc = new XmlDocument();
				xmlDoc.Schemas.Add("http://www.cargowise.com/Schemas/Universal/2011/11", xsdPath);

				// Valid XML UniversalInterchange request document
				xmlDoc.LoadXml(@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Header>
        <SenderID>WMTD1</SenderID>
        <RecipientID>HYEDAUAYA</RecipientID>
        <Acknowledgement>
            <Required>OnAll</Required>
            <Channel>eAdaptor</Channel>
            <RecipientID>EDIEDIDAT</RecipientID>
            <ContextCollection>
                <Context>
                  <Type>Some Type</Type>
                  <Value>Some Value</Value>
                </Context>
                <Context>
                  <Type>Some Other Type</Type>
                  <Value>Some Other Value</Value>
                </Context>
            </ContextCollection>
        </Acknowledgement>
    </Header>
    <Body>
        <!-- Some XML Message Payload -->
    </Body>
</UniversalInterchange>");

				xmlDoc.Validate(
						new ValidationEventHandler(
								(sender, e) => ValidationEventHandler(sender, e, true)
						)
				);

				// Valid XML UniversalInterchange request document
				xmlDoc.LoadXml(@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Header>
        <SenderID>WMTD1</SenderID>
        <RecipientID>HYEDAUAYA</RecipientID>
        <Acknowledgement>
            <Required>OnAll</Required>
            <Channel>eAdaptor</Channel>
            <RecipientID>EDIEDIDAT</RecipientID>
            <!-- No <Context> node -->
        </Acknowledgement>
    </Header>
    <Body>
        <!-- Some XML Message Payload -->
    </Body>
</UniversalInterchange>");

				xmlDoc.Validate(
						new ValidationEventHandler(
								(sender, e) => ValidationEventHandler(sender, e, true)
						)
				);

				// Invalid XML UniversalInterchange request document
				xmlDoc.LoadXml(@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Header>
        <SenderID>WMTD1</SenderID>
        <RecipientID>HYEDAUAYA</RecipientID>
        <Acknowledgement>
            <Required>OnSomethingElse</Required>
            <Channel>eAdaptor</Channel>
            <RecipientID>EDIEDIDAT</RecipientID>
            <ContextCollection>
                <Context>
                  <Type>Some Type</Type>
                  <Value>Some Value</Value>
                </Context>
            </ContextCollection>
        </Acknowledgement>
    </Header>
    <Body>
        <!-- Some XML Message Payload -->
    </Body>
</UniversalInterchange>");

				xmlDoc.Validate(
						new ValidationEventHandler(
								(sender, e) => ValidationEventHandler(sender, e, false)
						)
				);

				// Invalid XML UniversalInterchange request document
				xmlDoc.LoadXml(@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Header>
        <SenderID>WMTD1</SenderID>
        <RecipientID>HYEDAUAYA</RecipientID>
        <Acknowledgement>
            <Required>OnAll</Required>
            <Channel>NotRecoignizedFailFailFail</Channel>
            <RecipientID>EDIEDIDAT</RecipientID>
            <ContextCollection>
                <Context>
                  <Type>Some Type</Type>
                  <Value>Some Value</Value>
                </Context>
            </ContextCollection>
        </Acknowledgement>
    </Header>
    <Body>
        <!-- Some XML Message Payload -->
    </Body>
</UniversalInterchange>");

				xmlDoc.Validate(
						new ValidationEventHandler(
								(sender, e) => ValidationEventHandler(sender, e, false)
						)
				);
			}
		}

		static void ValidationEventHandler(object sender, ValidationEventArgs e, bool expect)
		{
			bool validity = (e.Severity != XmlSeverityType.Error);
			AssertEquals("XSD Validation Result: " + e.Message, expect, validity);
		}

		public void TestResetToQueued()
		{
			Message.Factory.Save();

			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();

				module.Grid.UnSelectAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals(1, module.GridCollection.Count);
				module.ClickReset();
				AssertEquals("FOO", Message.EM_Status);
				AssertEquals(EDIMessageModule.SelectAtLeastOneMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				module.Grid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				module.ClickReset();
				AssertEquals("Message status unchanged if 'No' selected", "FOO", Message.EM_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ClickReset();
				AssertEquals("Message status updated", "QUE", Message.EM_Status);
			}
		}

		public void TestResetToQueuedHttpXmlNotAllowed()
		{
			Message.EM_ApplicationCode = "NDQ";
			Factory.Save();
			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();
				AssertEquals(1, module.GridCollection.Count);

				module.Grid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ClickReset();
				var expected = "Messages of application code NDQ cannot be requeued.";
				var result = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Message warning shown", expected, result);
				AssertEquals("Message status unchanged", "FOO", Message.EM_Status);
			}
		}

		public void TestResetToQueuedHandleConcurrency()
		{
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ApplicationCode = "T1T";
			interchange1.EI_From = "ABC1";
			interchange1.EI_To = "DEF2";
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange1.EI_InterchangeType = "IT1";
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_InterchangeNum = "IT0001";

			var messageMock1 = Factory.NewMoq<EDIMessage>();
			messageMock1.Protected()
						.Setup("PopulateMessageNumber");
			messageMock1.CallBase = true;

			var message1 = messageMock1.Object;
			interchange1.ContainedMessages.Add(message1);
			message1.EM_ApplicationCode = "T1T";
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_MessageType = "MT1";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_MessageNum = "MT0001";

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "T1T";
			interchange2.EI_From = "DEF2";
			interchange2.EI_To = "ABC1";
			interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange2.EI_InterchangeType = "IT2";
			interchange2.EI_Status = EDIInterchange.Status.Queued;
			interchange2.EI_InterchangeNum = "IR0002";

			var messageMock2 = Factory.NewMoq<EDIMessage>();
			messageMock2.Protected()
						.Setup("PopulateMessageNumber");
			messageMock2.CallBase = true;

			var message2 = messageMock2.Object;
			interchange2.ContainedMessages.Add(message2);
			message2.EM_ApplicationCode = "T1T";
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageType = "MT2";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageNum = "MR0002";

			var interchange3 = Factory.New<EDIInterchange>();
			interchange3.EI_ApplicationCode = "T1T";
			interchange3.EI_From = "ABC1";
			interchange3.EI_To = "DEF2";
			interchange3.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange3.EI_InterchangeType = "IT3";
			interchange3.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange3.EI_InterchangeNum = "IT0003";

			var messageMock3 = Factory.NewMoq<EDIMessage>();
			messageMock3.Protected()
						.Setup("PopulateMessageNumber");
			messageMock3.CallBase = true;

			var message3 = messageMock3.Object;
			interchange3.ContainedMessages.Add(message3);
			message3.EM_ApplicationCode = "T1T";
			message3.EM_Status = EDIMessage.Status.Queued;
			message3.EM_MessageType = "MT3";
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageNum = "MT0003";

			var interchange4 = Factory.New<EDIInterchange>();
			interchange4.EI_ApplicationCode = "T1T";
			interchange4.EI_From = "DEF2";
			interchange4.EI_To = "ABC1";
			interchange4.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange4.EI_Status = EDIInterchange.Status.Queued;
			interchange4.EI_InterchangeNum = "IR0004";

			var messageMock4 = Factory.NewMoq<EDIMessage>();
			messageMock4.Protected()
						.Setup("PopulateMessageNumber");
			messageMock4.CallBase = true;

			var message4 = messageMock4.Object;
			interchange4.ContainedMessages.Add(message4);
			message4.EM_ApplicationCode = "T1T";
			message4.EM_Status = EDIMessage.Status.Queued;
			message4.EM_MessageType = "MT4";
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_MessageNum = "MR0004";

			Factory.Save();

			interchange1.EI_Status = EDIInterchange.Status.SendPending;
			interchange2.EI_Status = EDIInterchange.Status.SendPending;
			interchange3.EI_Status = EDIInterchange.Status.eHubPending;
			interchange4.EI_Status = EDIInterchange.Status.SendPending;
			message1.EM_Status = EDIMessage.Status.Pending;
			message2.EM_Status = EDIMessage.Status.Pending;
			message3.EM_Status = EDIMessage.Status.Sent;
			message4.EM_Status = EDIMessage.Status.Pending;
			Factory.Save();

			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				var messageDummy1 = (EDIMessage)module.GridCollection.FindByPK(message1.PK);
				var messageDummy2 = (EDIMessage)module.GridCollection.FindByPK(message2.PK);
				var messageDummy3 = (EDIMessage)module.GridCollection.FindByPK(message3.PK);
				var messageDummy4 = (EDIMessage)module.GridCollection.FindByPK(message4.PK);
				interchange1 = messageDummy1.Interchange;
				interchange2 = messageDummy2.Interchange;
				interchange3 = messageDummy3.Interchange;
				interchange4 = messageDummy4.Interchange;
				form.Show();
				module.Grid.SelectAllElements();

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				newFactory.Load<EDIInterchange>(interchange1.PK).EI_Status = EDIInterchange.Status.Sent;
				newFactory.Load<EDIInterchange>(interchange2.PK).EI_Status = EDIInterchange.Status.Received;
				newFactory.Load<EDIInterchange>(interchange3.PK).EI_Status = EDIInterchange.Status.Sent;
				newFactory.Load<EDIInterchange>(interchange4.PK).EI_Status = EDIInterchange.Status.Received;
				newFactory.Load<EDIMessage>(messageDummy1.PK).EM_Status = EDIMessage.Status.Sent;
				newFactory.Load<EDIMessage>(messageDummy2.PK).EM_Status = EDIMessage.Status.Received;
				newFactory.Load<EDIMessage>(messageDummy3.PK).EM_Status = EDIMessage.Status.Sent;
				newFactory.Load<EDIMessage>(messageDummy4.PK).EM_Status = EDIMessage.Status.Received;
				newFactory.Save();

				CombineAssertions(() =>
				{
					AssertEquals("Pre: interchange1.EI_Status", EDIInterchange.Status.SendPending, interchange1.EI_Status);
					AssertEquals("Pre: interchange1.HasChanges", false, interchange1.HasChanges);
					AssertEquals("Pre: interchange2.EI_Status", EDIInterchange.Status.SendPending, interchange2.EI_Status);
					AssertEquals("Pre: interchange2.HasChanges", false, interchange2.HasChanges);
					AssertEquals("Pre: interchange3.EI_Status", EDIInterchange.Status.eHubPending, interchange3.EI_Status);
					AssertEquals("Pre: interchange3.HasChanges", false, interchange3.HasChanges);
					AssertEquals("Pre: interchange4.EI_Status", EDIInterchange.Status.SendPending, interchange4.EI_Status);
					AssertEquals("Pre: interchange4.HasChanges", false, interchange4.HasChanges);
					AssertEquals("Pre: message1.EM_Status", EDIMessage.Status.Pending, messageDummy1.EM_Status);
					AssertEquals("Pre: message1.HasChanges", false, messageDummy1.HasChanges);
					AssertEquals("Pre: message2.EM_Status", EDIMessage.Status.Pending, messageDummy2.EM_Status);
					AssertEquals("Pre: message2.HasChanges", false, messageDummy2.HasChanges);
					AssertEquals("Pre: message3.EM_Status", EDIMessage.Status.Sent, messageDummy3.EM_Status);
					AssertEquals("Pre: message3.HasChanges", false, message3.HasChanges);
					AssertEquals("Pre: message4.EM_Status", EDIMessage.Status.Pending, messageDummy4.EM_Status);
					AssertEquals("Pre: message4.HasChanges", false, messageDummy4.HasChanges);

					ErrorReporter.Clear();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.ClickReset();
					AssertEquals("interchange1.EI_Status", EDIInterchange.Status.Sent, interchange1.EI_Status);
					AssertEquals("interchange1.HasChanges", true, interchange1.HasChanges);
					AssertEquals("interchange2.EI_Status", EDIInterchange.Status.SendPending, interchange2.EI_Status);
					AssertEquals("interchange2.HasChanges", false, interchange2.HasChanges);
					AssertEquals("interchange3.EI_Status", EDIInterchange.Status.Sent, interchange3.EI_Status);
					AssertEquals("interchange3.HasChanges", true, interchange3.HasChanges);
					AssertEquals("interchange4.EI_Status", EDIInterchange.Status.SendPending, interchange4.EI_Status);
					AssertEquals("interchange4.HasChanges", false, interchange4.HasChanges);
					AssertEquals("message1.EM_Status", EDIMessage.Status.Sent, messageDummy1.EM_Status);
					AssertEquals("message1.HasChanges", true, messageDummy1.HasChanges);
					AssertEquals("message2.EM_Status", EDIMessage.Status.Received, messageDummy2.EM_Status);
					AssertEquals("message2.HasChanges", true, messageDummy2.HasChanges);
					AssertEquals("message3.EM_Status", EDIMessage.Status.Sent, messageDummy3.EM_Status);
					AssertEquals("message3.HasChanges", false, messageDummy3.HasChanges);
					AssertEquals("message4.EM_Status", EDIMessage.Status.Received, messageDummy4.EM_Status);
					AssertEquals("message4.HasChanges", true, messageDummy4.HasChanges);
					AssertEquals("Caption", "WARNING", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertAllLinesStartWith("Text", @"Another user has made changes; system cannot reset the data to queue.

Please use the 'Find' button to reload the data and try again.

The following objects have changes and will be merged:
EDI Message (CargoWise Support @
	Status
EDI Message (CargoWise Support @
	Status
EDI Message (CargoWise Support @
	Status
Interchange IT0001 (CargoWise Support @
	Status
Interchange IT0003 (CargoWise Support @
	Status
", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestMessageInterpreter()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_LoginName = "edi.support";
			staff.GS_FullName = "bob";
			Factory.Save();

			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
				{
					Env.Registry.EnableEDIMessageInterpreter = true;
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var messageInterpreterMenuItem = actionMenuItem.MenuItems.FindByText(TestEDIMessageModule.MessageInterpreterMenuName);
					AssertNotNull("messageInterpreterMenuItem", messageInterpreterMenuItem);
				}
			}
			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
				{
					Env.Registry.EnableEDIMessageInterpreter = false;
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var messageInterpreterMenuItem = actionMenuItem.MenuItems.FindByText(TestEDIMessageModule.MessageInterpreterMenuName);
					AssertNull("messageInterpreterMenuItem", messageInterpreterMenuItem);
				}
			}
			using (var module = new TestEDIMessageModule())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, Guid.Empty, Guid.Empty))
				{
					Env.Registry.EnableEDIMessageInterpreter = true;
					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var messageInterpreterMenuItem = actionMenuItem.MenuItems.FindByText(TestEDIMessageModule.MessageInterpreterMenuName);
					AssertNull("messageInterpreterMenuItem", messageInterpreterMenuItem);
				}
			}
		}

		public void TestUniversalXmlSchemasItemAlwaysAppears()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlSchemasMenuItem = actionMenuItem.MenuItems.FindByText("Save Universal + Native XML Schemas");

				AssertNotNull("universalXmlSchemasMenuItem", universalXmlSchemasMenuItem);
			}
		}

		public void TestUniversalXmlSchemasGeneration()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertUniversalXmlSchemasGeneration(false);

			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertUniversalXmlSchemasGeneration(true);
		}

		public void AssertUniversalXmlSchemasGeneration(bool expectDocData)
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlSchemasMenuItem = actionMenuItem.MenuItems.FindByText("Save Universal + Native XML Schemas");

				AssertNotNull("UniversalXmlMessageMenuItem", universalXmlSchemasMenuItem);

				module.Directory = null;
				AssertNoExceptionThrown("UniversalXmlSchemasGeneration", () => universalXmlSchemasMenuItem.PerformClick());
				Assert("File \\Universal XML.zip should not be created when user clicks cancel", !File.Exists("\\" + UniversalZipName));

				var unitTestNotification = UnitTestUserNotification.Instance;
				module.Directory = @"~!@#$%^&*()_+-={}[]/|?\><,.:;'";
				universalXmlSchemasMenuItem.PerformClick();
				AssertStartsWith("Error Dialog did not display the correct message", @"Fail creating Universal or Native XML", unitTestNotification.PreviousMessages[0].Text);

				unitTestNotification.ClearMessagesAndAnswers();

				using (var tempFileDirectory = new TempDirectory())
				{
					var tempDirectoryName = tempFileDirectory.DirectoryName;

					module.Directory = tempDirectoryName;
					AssertNoExceptionThrown("UniversalXmlSchemasGeneration", () => universalXmlSchemasMenuItem.PerformClick());

					foreach (UnitTestUserNotification.PreviousMessage message in unitTestNotification.PreviousMessages)
					{
						AssertEquals("Expected no errors but found:\r\n" + message.Text, false, message.WasError);
					}

					var successMessageText = unitTestNotification.PreviousMessages[0].Text;

					AssertMatch("successMessageText", new Regex(@"^[0-9]+ Universal XML \+ [0-9]+ Native XML schema files exported to \[.+\].$"), successMessageText);
					AssertContains("successMessageText", tempDirectoryName, successMessageText);

					ZipCompression.Unzip(Path.Combine(tempDirectoryName, UniversalZipName), tempDirectoryName);
					ZipCompression.Unzip(Path.Combine(tempDirectoryName, NativeZipName), tempDirectoryName);

					AssertEquals("UniversalInterchange.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalInterchange.xsd")));
					AssertEquals("UniversalEvent.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalEvent.xsd")));
					AssertEquals("UniversalDocumentRequest.xsd should be exported", true, File.Exists(Path.Combine(tempDirectoryName, "UniversalDocumentRequest.xsd")));

					CombineAssertions(delegate
					{
						AssertContains("Check shipment xsd contents", @"<xs:element name=""UniversalShipment"" type=""UniversalShipmentData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalShipment.xsd"));

						AssertContains("Check activity xsd contents", @"<xs:element name=""UniversalActivity"" type=""UniversalActivityData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalActivity.xsd"));
						AssertContains("Check event xsd contents", @"<xs:element name=""UniversalEvent"" type=""UniversalEventData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalEvent.xsd"));
						AssertContains("Check transaction xsd contents", @"<xs:element name=""UniversalTransaction"" type=""UniversalTransactionData"" />", File.ReadAllText(tempDirectoryName + "\\UniversalTransaction.xsd"));
						AssertContains("Check native xsd contents", @"<xs:element name=""Native"">", File.ReadAllText(tempDirectoryName + "\\Native.xsd"));
						AssertContains("Check organisation xsd contents", @"<xs:complexType name=""NativeOrganization"">", File.ReadAllText(tempDirectoryName + "\\NativeOrganization.xsd"));
						AssertContains("Check interchange xsd contents", @"<xs:element name=""UniversalInterchange"">", File.ReadAllText(tempDirectoryName + "\\UniversalInterchange.xsd"));

						var commonXsd = File.ReadAllText(tempDirectoryName + "\\UniversalCommon.xsd");
						AssertContains("Check common xsd contents", @"<xs:complexType name=""DataContext"">", commonXsd);
						AssertEquals($"Check common xsd should {(expectDocData ? "" : "not ")}contain DocData element", expectDocData, commonXsd.Contains(@"<xs:element name=""DocData"""));
						AssertContains("Check attached doc in common xsd", @"<xs:complexType name=""AttachedDocument"">", commonXsd);
						AssertContains("Check attached doc collection in common xsd", @"<xs:element name=""AttachedDocumentCollection"" minOccurs=""0"">", commonXsd);
					});
				}
			}
		}

		public void TestUniversalXMLActionItemAppears()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlMessageMenuItem = actionMenuItem.MenuItems.FindByText("Add Inbound Universal XML Message");

				AssertNotNull("UniversalXmlMessageMenuItem", universalXmlMessageMenuItem);
			}
		}

		public void TestUniversalXmlMessageImport()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlMessageMenuItem = actionMenuItem.MenuItems.FindByText("Add Inbound Universal XML Message");

				AssertNotNull("UniversalXmlMessageMenuItem", universalXmlMessageMenuItem);

				module.FileText = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Event>
    <DataSource>
      <ReferenceCollection>
        <Reference>
          <Type>ForwardingConsol</Type>
          <Key>C00001040</Key>
        </Reference>
      </ReferenceCollection>

      <Company>
        <Code>DNZ</Code>
        <Name>NZ Demo Company</Name>
      </Company>
      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <EnterpriseID>HYE</EnterpriseID>
      <EventType>
        <Code>CCC</Code>
        <Description>Customs Clearance Komenced</Description>
      </EventType>
      <ServerID>DAT</ServerID>
      <TriggerDescription>Fred</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataSource>

    <EventTime>2011-04-11T13:19:12.343</EventTime>
    <EventType>CCC</EventType>
    <DataProvider>CargoWise One</DataProvider>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MIGHTYDUCK</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
".Trim();
				AssertNoExceptionThrown("UniversalXmlMessageImport", () => universalXmlMessageMenuItem.PerformClick());

				var unitTestNotification = UnitTestUserNotification.Instance;
				foreach (UnitTestUserNotification.PreviousMessage message in unitTestNotification.PreviousMessages)
				{
					AssertEquals("Expected no errors but found:\r\n" + message.Text, false, message.WasError);
				}

				AssertStartsWith("Successful Save Message", "XUE - XML Universal Event message was saved with message number [", unitTestNotification.LastMessage.Text);

				var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging));

				AssertNotNull("ediMessage", ediMessage);

				AssertEquals("ediMessage.EM_MessageType", EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
				AssertEquals("ediMessage.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, ediMessage.EM_MessageSubType);
				AssertEquals("ediMessage.EM_ReceiveTransmit", EDIMessage.Direction.Receive, ediMessage.EM_ReceiveTransmit);
				AssertNotNullOrEmpty("ediMessage.EM_MessageNum", ediMessage.EM_MessageNum);
				AssertEquals("ediMessage.EM_Status", EDIMessage.Status.Queued, ediMessage.EM_Status);
				AssertEquals("ediMessage.EM_GB", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
				AssertEquals("ediMessage.EM_GE", GlbDepartment.CurrentDepartment.PK, ediMessage.EM_GE);
				AssertEquals("ediMessage.EM_MessageText", module.FileText, ediMessage.EM_MessageText);
			}
		}

		public void TestUniversalXmlMessageImportHandlesUnmatchingTags()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlMessageMenuItem = actionMenuItem.MenuItems.FindByText("Add Inbound Universal XML Message");

				AssertNotNull("UniversalXmlMessageMenuItem", universalXmlMessageMenuItem);

				module.FileText = ExpectedFileText;
				AssertNoExceptionThrown("UniversalXmlMessageImport", () => universalXmlMessageMenuItem.PerformClick());

				var unitTestNotification = UnitTestUserNotification.Instance;
				AssertEquals("unitTestNotification.LastMessage", "The 'UnMatchedOpeningTag' start tag on line 1 position 2 does not match the end tag of 'TheCrazyTag'. Line 1, position 24.", unitTestNotification.LastMessage.Text);
			}
		}

		public void TestUniversalXmlMessageImportShouldHaveFirstTagAsUniversalShipmentOrEvent()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();

				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");

				AssertNotNull("actionMenuItem", actionMenuItem);

				var universalXmlMessageMenuItem = actionMenuItem.MenuItems.FindByText("Add Inbound Universal XML Message");

				AssertNotNull("UniversalXmlMessageMenuItem", universalXmlMessageMenuItem);

				module.FileText = @"
<UniversalSerialBus>
</UniversalSerialBus>
				".Trim();

				AssertNoExceptionThrown("UniversalXmlMessageImport", () => universalXmlMessageMenuItem.PerformClick());

				var unitTestNotification = UnitTestUserNotification.Instance;
				AssertEquals("unitTestNotification.LastMessage", "Expected Root Element Tag to be 'UniversalShipment', 'UniversalEvent', 'UniversalActivity', 'UniversalTransaction', 'UniversalTransactionBatch', or 'UniversalSchedule', but found <UniversalSerialBus> instead.", unitTestNotification.LastMessage.Text);
			}
		}

		public void TestUniversalXmlMessageImport_ForUniversalActivity_ShouldNotShowRootElementError()
		{
			using (var module = new TestEDIMessageModule())
			using (module.ShowPopup())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var universalXmlMessageMenuItem = actionMenuItem.MenuItems.FindByText("Add Inbound Universal XML Message");

				module.FileText = @"
<UniversalActivity>
</UniversalActivity>";

				AssertNoExceptionThrown("Universal Activities are an acceptable type to import, so no exception should be thrown. SAD!", () => universalXmlMessageMenuItem.PerformClick());
				AssertStartsWith("Add Inbound Universal XML Message", "XUA - XML Universal Activity message was saved with message number [", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniversalXmlMessageImport_SupportsUniversalScheduleForEDISupportAndDevelopersOnly()
		{
			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "alina.van";
			Factory.Save();

			AssertUniversalXmlMessageImport_UniversalSchedule(Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			user.GS_IsDeveloper = true;
			Factory.Save();

			AssertUniversalXmlMessageImport_UniversalSchedule(Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			AssertEquals(true, GlbStaff.CurrentUser.IsSupportUser);
			AssertUniversalXmlMessageImport_UniversalSchedule(Env.SetTemporaryUserContext(Env.CurrentUserContext));
		}

		void AssertUniversalXmlMessageImport_UniversalSchedule(IDisposable userContext)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (userContext)
			{
				bool isImportAllowed = GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper;

				var expectedMessage = (isImportAllowed)
				? "XSC - XML Universal Schedule message was saved"
				: "Expected Root Element Tag to be 'UniversalShipment', 'UniversalEvent', 'UniversalActivity', 'UniversalTransaction', or 'UniversalTransactionBatch', but found <UniversalSchedule> instead.";

				using (var module = new TestEDIMessageModule())
				using (var form = new ZForm(module.FilterBusinessObject))
				{
					form.Controls.Add(module.EmbeddedControl);
					module.GridCollection.Load();
					form.Show();

					var actionMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
					var universalXmlMessageMenuItem = MenuAssertion.AssertHasMenu(actionMenuItem, "Add Inbound Universal XML Message");

					module.FileText = @"
<UniversalSchedule xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Schedule>
<DataProvider>1ST</DataProvider>
<IsCancellation>false</IsCancellation>
<Carrier>
	<AddressType>Carrier</AddressType>
	<OrganizationCode>COS</OrganizationCode>
	<CompanyName>CHINA OCEAN SHIPPING</CompanyName>
</Carrier>
<Transport>
<Sea>
<Vessel>
	<VesselName>LONDON EXPRESS</VesselName>
	<LloydsNumber>9143568</LloydsNumber>
</Vessel>
<VoyageNumber>TEST</VoyageNumber>
</Sea>
</Transport>
<DischargeCollection>
	<Discharge>
	<Port><Code>USLAX</Code></Port>
	<EstimatedArrival>2013-12-19T08:00:45</EstimatedArrival>
	</Discharge>
</DischargeCollection>
<LoadingCollection>
	<Loading>
	<Port><Code>AUSYD</Code></Port>
	<EstimatedArrival>2013-12-23T15:00:30</EstimatedArrival>
	<EstimatedDeparture>2013-12-24T11:00:30</EstimatedDeparture>
	<TerminalCode>00350607</TerminalCode>
	</Loading>
</LoadingCollection>
</Schedule>
</UniversalSchedule>
				".Trim();

					AssertNoExceptionThrown(() => universalXmlMessageMenuItem.PerformClick());
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					if (isImportAllowed)
					{
						var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging));

						AssertNotNull(ediMessage);

						AssertEquals(EDIMessageTypeList.Codes.XDC, ediMessage.EM_MessageType);
						AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalSchedule, ediMessage.EM_MessageSubType);
						AssertEquals(EDIMessage.Direction.Receive, ediMessage.EM_ReceiveTransmit);
						AssertEquals(EDIMessage.Status.Queued, ediMessage.EM_Status);
						AssertEquals(GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
						AssertEquals(GlbDepartment.CurrentDepartment.PK, ediMessage.EM_GE);
						AssertEquals(module.FileText, ediMessage.EM_MessageText);
					}
				}
			}
		}

		public void TestImportMessageUsingEAdaptor_DoesNotFailIfNoFileSelected()
		{
			using (var module = new TestEDIMessageModule())
			using (module.ShowPopup())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var menuItem = actionMenuItem.MenuItems.FindByText(EDIMessageModule.EAdaptorImportMessageMenuName);

				AssertNoExceptionThrown("Canceling Open File Dialog must not raise an exception.", () => menuItem.PerformClick());
			}
		}

		public void TestSimulateMessageImportWithNoCommit()
		{
			using (var module = new TestEDIMessageModule())
			using (module.ShowPopup())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var menuItem = actionMenuItem.MenuItems.FindByText(EDIMessageModule.SimulateMessageImportNoCommitMenuName);

				module.FileText = @"
<UniversalSerialBus>
</UniversalSerialBus>
				".Trim();

				AssertNoExceptionThrown(menuItem.PerformClick);
				Assert("File Should have been created", File.Exists(module.OutputFileName));
				AssertEquals("Output file saved successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				DeleteIfExists(module.OutputFileName);
			}
		}

		public void TestEdiClientColumnVisibility()
		{
			AssertEdiClientColumnVisibility(true);
		}

		public void TestExternalReferenceNumberColumnVisibility()
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();

				var externalReferenceNumberColumn = module.Grid.GetColumnStyle("EM_ExternalReferenceNumber");
				AssertNotNull(externalReferenceNumberColumn);
				Assert("External Reference Num. column should not be visible", !externalReferenceNumberColumn.IsVisible);
			}
		}

		public void TestUniversalAndNativeZipCreated()
		{
			using (var tempFileDirectory = new TempDirectory())
			{
				var tempDirectoryName = tempFileDirectory.DirectoryName;

				using (TestEDIMessageModule module = new TestEDIMessageModule())
				using (ZForm form = new ZForm(module.FilterBusinessObject))
				{
					form.Controls.Add(module.EmbeddedControl);
					module.GridCollection.Load();
					form.Show();

					var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
					AssertNotNull("actionMenuItem", actionMenuItem);
					var universalXmlSchemasMenuItem = actionMenuItem.MenuItems.FindByText("Save Universal + Native XML Schemas");
					AssertNotNull("UniversalXmlMessageMenuItem", universalXmlSchemasMenuItem);

					module.Directory = tempDirectoryName;
					AssertNoExceptionThrown("UniversalXmlSchemasGeneration", () => universalXmlSchemasMenuItem.PerformClick());

					AssertEquals("Universal XML zip should be exported", true, File.Exists(Path.Combine(tempDirectoryName, UniversalZipName)));
					AssertEquals("Native XML zip should be exported", true, File.Exists(Path.Combine(tempDirectoryName, NativeZipName)));
				}
			}
		}

		#region Test Classes

		const string ExpectedFileText = @"<UnMatchedOpeningTag></TheCrazyTag>";

		readonly string UniversalZipName = $"{UniversalXmlInfo.ZipFileName}.zip";

		readonly string NativeZipName = $"{NativeXmlInfo.ZipFileName}.zip";

		class TestEDIMessageModule : EDIMessageModule
		{
			public void ClickReset()
			{
				ResetToQueued_Click(null, null);
			}

			public string FileText { get; set; }
			public string OutputFileName { get { return Path.Combine(Temp.TempPath, "test.txt"); } }

			public string Directory { get; set; }

			protected override string GetDirectory()
			{
				return Directory;
			}

			protected override Stream GetFileStream()
			{
				if (FileText == null)
				{
					return null;
				}
				return new MemoryStream(System.Text.UTF8Encoding.ASCII.GetBytes(FileText));
			}

			protected override Stream TryOpenSaveFileStream()
			{
				return File.OpenWrite(OutputFileName);
			}

			public new BusinessObjectCollection GridCollection
			{
				get { return (BusinessObjectCollection)base.GridCollection; }
			}

			public new ZDisplayGrid Grid
			{
				get { return base.Grid; }
			}

			public new BusinessObject FilterBusinessObject
			{
				get { return base.FilterBusinessObject; }
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Messaging.EDIMessage;
		}

		EDIMessage Message
		{
			get
			{
				if (fMessage == null)
				{
					fMessage = Factory.NewWithValidTestData<EDIMessage>();
					fMessage.EM_Status = "FOO";
					fMessage.EM_MessageType = "BAR";
					fMessage.EM_ReceiveTransmit = "RCV";
				}
				return fMessage;
			}
		}
		EDIMessage fMessage;

		void AssertEdiClientColumnVisibility(bool shouldShow)
		{
			using (TestEDIMessageModule module = new TestEDIMessageModule())
			using (ZForm form = new ZForm(module.FilterBusinessObject))
			{
				form.Controls.Add(module.EmbeddedControl);
				module.GridCollection.Load();
				form.Show();
				Application.DoEvents();

				AssertEquals("Correct EDI client column visibility", shouldShow, !module.Grid.GetColumnStyle("CommunicationPartyConfig+Party+Name").IsUnavailable);
			}
		}

		#endregion
	}
}
