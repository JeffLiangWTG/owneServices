using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eHub;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	/// <summary>
	/// Request XML should looks like this:
	/// <Native xmlns='http://www.cargowise.com/Schemas/Native/2011/11' version='1.0'>
	///		<Header>
	///			....
	///		</Header>
	///		<Body>
	///			....
	///		</Body>
	/// </Native>
	/// </summary>
	public class RequestSerializerTest : TestCase
	{
		public void TestRootElement_Name_Should_Be_Native()
		{
			var request = new Request();
			var element = serializer.Serialize(request);

			AssertEquals(ReferenceDataXMLForDeSerialize.RootElementName, element.Name.LocalName);
			AssertEquals(ns, element.Name.Namespace);
		}

		public void TestEmptyReqeuset_Should_Have_Header_Element()
		{
			var request = new Request();
			var element = serializer.Serialize(request);

			Assert(element.HasElements);
			AssertNotNull(element.Element(ns + ReferenceDataXMLForDeSerialize.HeaderElementName));
		}

		public void TestRequest_Should_Have_Header_Element()
		{
			var request = new Request
			{
				EntitySets = new[] { new XElement("Test") },
				Settings = new HeaderData_Versioned_Native()
			};
			var element = serializer.Serialize(request);

			Assert(element.HasElements);
			AssertNotNull(element.Element(ns + ReferenceDataXMLForDeSerialize.HeaderElementName));
		}

		public void TestRequest_Should_Always_Have_Body_Element()
		{
			var request = new Request();
			var element = serializer.Serialize(request);
			AssertNotNull(element.Element(ns + ReferenceDataXMLForDeSerialize.BodyElementName));

			request.EntitySets = new[] { new XElement("Test") };
			element = serializer.Serialize(request);
			AssertNotNull(element.Element(ns + ReferenceDataXMLForDeSerialize.BodyElementName));
		}

		public void TestStream_Is_Still_Open()
		{
			var request = new Request();
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, request);

				Assert(stream.CanRead);
			}
		}

		public void TestStreamCanHandleInvalidChar()
		{
			var request = new Request();
			request.EntitySets = new[] { new XElement("Test", "®") };
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, request);
				Assert("Not exception thrown", true);
			}
		}

		[UseSnapshotProtection]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "we know the expected format")]
		public void TestAddConsolToOrganizationXml()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				OrgHeader orgHeader = CreateOrg(factory);
				var consol = CreateConsol(factory, new ZGuid(), orgHeader.MainAddress.PK, Constants.TransportModes.Sea, "USHOU", "AUBNE");
				consol.Shipments.Add(CreateShipment(factory, 200m, "LB", 3000m, "L"));
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 01, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be greater than Period Start", consol.JK_SystemCreateTimeUtc >= setting.PeriodStart);
				Assert("PRE: Consol Create Time should be lees than Period End", consol.JK_SystemCreateTimeUtc < setting.PeriodEnd);

				var request = new Request();
				request.EntitySets = new[] { new XElement(ns + "Organization", new XElement(ns + "OrgHeader")) };
				var currentTime = DateTime.UtcNow;
				XElement element = serializer.Serialize(request, orgHeader.PK.ToGuid());

				AssertNotEquals("Scavenging Organization XML should include ClientOrgConsol element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClientOrgConsol"));

				string expectedXML = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header />
  <Body>
    <Organization>
      <OrgHeader>
        <ClientOrgConsolCollection>
          <ClientOrgConsol Action=""INSERT"">
            <AgentType>Receiving</AgentType>
            <TransMode>SEA</TransMode>
            <LoadPort>USHOU</LoadPort>
            <DischargePort>AUBNE</DischargePort>
            <TotalWeight>190.718</TotalWeight>
            <TotalVolume>33.000</TotalVolume>
            <ShipCount>2</ShipCount>
            <CreateUTC>2014-01-01 01:00:00.000</CreateUTC>
            <ExportUTC>\d{4}\-\d{2}\-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}</ExportUTC>
          </ClientOrgConsol>
        </ClientOrgConsolCollection>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";
				AssertMatch("Scavenging Organization Xml with Consols", new Regex(expectedXML.Trim()), element.ToString());

				AssertDateTimeWithinOneSecond("Export UTC should be current UTC", currentTime, DateTime.Parse(element.Descendants().First(e => e.Name.LocalName == "ExportUTC").Value, CultureInfo.InvariantCulture));
			}
		}

		[UseSnapshotProtection]
		public void TestAddConsolToOrganizationXml_IfOrgHasNoConsol()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				Guid orgHeaderPK = CreateOrg(factory).PK.ToGuid();
				ForwardingConsol consol = CreateConsol(factory, new ZGuid(), new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 01, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be greater than Period Start", consol.JK_SystemCreateTimeUtc >= setting.PeriodStart);
				Assert("PRE: Consol Create Time should be lees than Period End", consol.JK_SystemCreateTimeUtc < setting.PeriodEnd);

				var request = new Request();
				request.EntitySets = new[] { new XElement(ns + "Organization", new XElement(ns + "OrgHeader")) };
				XElement element = serializer.Serialize(request, orgHeaderPK);

				AssertEquals("Scavenging Organization XML should not include ClientOrgConsol element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClientOrgConsol"));
				AssertMultilineASCIIEquals("Organization Xml without Consol", @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header />
  <Body>
    <Organization>
      <OrgHeader />
    </Organization>
  </Body>
</Native>".Trim(), element.ToString());
			}
		}

		[UseSnapshotProtection]
		public void TestAddConsolToOrganizationXml_IfInvalidSetting()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				Guid orgHeaderPK = CreateOrg(factory).PK.ToGuid();
				factory.Save();

				var request = new Request();
				request.EntitySets = new[] { new XElement(ns + "Organization", new XElement(ns + "OrgHeader")) };
				AssertExceptionThrown<Exception>("OrgCollection in scavenging setting is invalid.", () => serializer.Serialize(request, orgHeaderPK));

				SetupRegistry(new ZDateTime(), new ZDateTime(2014, 01, 19, 01, 00, 00));
				request.EntitySets = new[] { new XElement(ns + "Organization", new XElement(ns + "OrgHeader")) };
				AssertExceptionThrown<Exception>("OrgCollection in scavenging setting is invalid.", () => serializer.Serialize(request, orgHeaderPK));

				SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime());
				request.EntitySets = new[] { new XElement(ns + "Organization", new XElement(ns + "OrgHeader")) };
				AssertExceptionThrown<Exception>("OrgCollection in scavenging setting is invalid.", () => serializer.Serialize(request, orgHeaderPK));
			}
		}

		[UseSnapshotProtection]
		public void TestRetrieveConsolXmlAsString()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				OrgHeader orgHeader = CreateOrg(factory);
				ForwardingConsol consol = CreateConsol(factory, orgHeader.MainAddress.PK, new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 01, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be greater than Period Start", consol.JK_SystemCreateTimeUtc >= setting.PeriodStart);
				Assert("PRE: Consol Create Time should be lees than Period End", consol.JK_SystemCreateTimeUtc < setting.PeriodEnd);

				var consolXml = serializer.RetrieveConsolXmlAsString(orgHeader.PK.ToGuid());

				AssertMatch("Retrieved Consol Xml",
new Regex(@"<ClientOrgConsolCollection><ClientOrgConsol><AgentType>Sending</AgentType><TransMode>AIR</TransMode><LoadPort>AUSYD</LoadPort><DischargePort>USCHI</DischargePort><TotalWeight>100.000</TotalWeight><TotalVolume>30.000</TotalVolume><ShipCount>1</ShipCount><CreateUTC>2014-01-01 01:00:00.000</CreateUTC><ExportUTC>\d{4}\-\d{2}\-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}</ExportUTC></ClientOrgConsol></ClientOrgConsolCollection>"),
consolXml);
			}
		}

		[UseSnapshotProtection]
		public void TestRetrieveConsolXmlAsString_IfOrgProxy()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				OrgHeader orgHeader = GlbCompany.CurrentCompany.OrgProxy;
				ForwardingConsol consol = CreateConsol(factory, orgHeader.MainAddress.PK, new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 01, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be greater than Period Start", consol.JK_SystemCreateTimeUtc >= setting.PeriodStart);
				Assert("PRE: Consol Create Time should be lees than Period End", consol.JK_SystemCreateTimeUtc < setting.PeriodEnd);

				var consolXml = serializer.RetrieveConsolXmlAsString(orgHeader.PK.ToGuid());

				AssertEquals("Retrieved Consol Xml should be empty", string.Empty, consolXml);
			}
		}

		[UseSnapshotProtection]
		public void TestRetrieveConsolXmlAsString_IfConsolCreatedBeforePeriodStart()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				OrgHeader orgHeader = CreateOrg(factory);
				ForwardingConsol consol = CreateConsol(factory, orgHeader.MainAddress.PK, new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2013, 10, 01, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be lees than Period Start", consol.JK_SystemCreateTimeUtc < setting.PeriodStart);

				var consolXml = serializer.RetrieveConsolXmlAsString(orgHeader.PK.ToGuid());

				AssertEquals("Retrieved Consol Xml should be empty", string.Empty, consolXml);
			}
		}

		[UseSnapshotProtection]
		public void TestRetrieveConsolXmlAsString_IfConsolCreatedAfterPeriodStart()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				ScavengingSetting setting = SetupRegistry(new ZDateTime(2013, 10, 19, 01, 00, 00), new ZDateTime(2014, 01, 19, 01, 00, 00));
				var factory = new BusinessObjectFactory(connection);
				OrgHeader orgHeader = CreateOrg(factory);
				ForwardingConsol consol = CreateConsol(factory, orgHeader.MainAddress.PK, new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 22, 01, 00, 00);
				factory.Save();

				Assert("PRE: Consol Create Time should be greater than Period End", consol.JK_SystemCreateTimeUtc >= setting.PeriodEnd);

				var consolXml = serializer.RetrieveConsolXmlAsString(orgHeader.PK.ToGuid());

				AssertEquals("Retrieved Consol Xml should be empty", string.Empty, consolXml);
			}
		}

		public void TestSetupConsolXml()
		{
			#region ConsolXmlAsString

			const string ConsolXmlAsString = @"
<ClientOrgConsolCollection>
  <ClientOrgConsol>
    <AgentType>Receiving</AgentType>
    <TransMode>AIR</TransMode>
    <LoadPort>AUSYD</LoadPort>
    <DischargePort>USCHI</DischargePort>
    <TotalWeight>100.000</TotalWeight>
    <TotalVolume>5.000</TotalVolume>
    <ShipCount>1</ShipCount>
    <ExportUTC>2013-11-05T05:00:00.000</ExportUTC>
  </ClientOrgConsol>
  <ClientOrgConsol>
    <AgentType>Sending</AgentType>
    <TransMode>AIR</TransMode>
    <LoadPort>AUSYD</LoadPort>
    <DischargePort>USLAX</DischargePort>
    <TotalWeight>179.000</TotalWeight>
    <TotalVolume>23.000</TotalVolume>
    <ShipCount>2</ShipCount>
    <ExportUTC>2013-11-05T05:00:00.000</ExportUTC>
  </ClientOrgConsol>
</ClientOrgConsolCollection>";
			#endregion

			var consolXml = XElement.Parse(ConsolXmlAsString);
			serializer.SetupConsolXml(consolXml);
			AssertMultilineASCIIEquals("Exported Data Header", @"
<ClientOrgConsolCollection xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <ClientOrgConsol Action=""INSERT"">
    <AgentType>Receiving</AgentType>
    <TransMode>AIR</TransMode>
    <LoadPort>AUSYD</LoadPort>
    <DischargePort>USCHI</DischargePort>
    <TotalWeight>100.000</TotalWeight>
    <TotalVolume>5.000</TotalVolume>
    <ShipCount>1</ShipCount>
    <ExportUTC>2013-11-05T05:00:00.000</ExportUTC>
  </ClientOrgConsol>
  <ClientOrgConsol Action=""INSERT"">
    <AgentType>Sending</AgentType>
    <TransMode>AIR</TransMode>
    <LoadPort>AUSYD</LoadPort>
    <DischargePort>USLAX</DischargePort>
    <TotalWeight>179.000</TotalWeight>
    <TotalVolume>23.000</TotalVolume>
    <ShipCount>2</ShipCount>
    <ExportUTC>2013-11-05T05:00:00.000</ExportUTC>
  </ClientOrgConsol>
</ClientOrgConsolCollection>".Trim(), consolXml.ToString());
		}

		ScavengingSetting SetupRegistry(ZDateTime periodStart, ZDateTime periodEnd)
		{
			var collection = new ScavengingSettingCollection();
			ScavengingSetting setting = collection.AddNew();
			setting.TaskName = "OrgCollection";
			setting.PeriodStart = periodStart;
			setting.PeriodEnd = periodEnd;
			eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			return setting;
		}

		OrgHeader CreateOrg(BusinessObjectFactory factory)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_IsForwarder = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			return orgHeader;
		}

		ForwardingConsol CreateConsol(BusinessObjectFactory factory, ZGuid sendingAgentAddress, ZGuid receivingAgentAddress, string transportMode, string loadPort, string dischargePort)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_OA_SendingForwarderAddress = sendingAgentAddress;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgentAddress;
			consol.Shipments.Add(CreateShipment(factory, 100m, "KG", 30m, "M3"));

			return consol;
		}

		ForwardingShipment CreateShipment(BusinessObjectFactory factory, decimal weight, string weightUnit, decimal volume, string volumeUnit)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = weightUnit;
			shipment.JS_ActualVolume = volume;
			shipment.JS_UnitOfVolume = volumeUnit;

			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();
			serializer = new RequestSerializer();
			ns = NativeXmlInfo.Namespace_2011_11;
		}
		RequestSerializer serializer;
		XNamespace ns;
	}
}
