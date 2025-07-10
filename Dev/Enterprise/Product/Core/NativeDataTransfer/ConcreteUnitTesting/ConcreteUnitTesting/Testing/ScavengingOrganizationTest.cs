using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eHub;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class ScavengingOrganizationTest : TestCaseWithFactory
	{
		public void TestScavengingOrganizationExportXml()
		{
			SetupRegistry();
			var orgHeaderPK = CreateOrg(Factory).PK;

			Factory.Save();

			XElement element = SerializeToOrganizationXml(orgHeaderPK);

			CombineAssertions(delegate
			{
				AssertNotEquals("Scavenging Organization XML should include OrgMiscServ element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgMiscServ"));
				AssertNotEquals("Scavenging Organization XML should include OrgContactCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgContactCollection"));
				AssertNotEquals("Scavenging Organization XML should include OrgAddressCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgAddressCollection"));
				AssertNotEquals("Scavenging Organization XML should include OrgCompanyDataCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgCompanyDataCollection"));
				AssertNotEquals("Scavenging Organization XML should include OrgBrandOrRelatedNameCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgBrandOrRelatedNameCollection"));
				AssertNotEquals("Scavenging Organization XML should include OrgRateTariffLevelCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgRateTariffLevelCollection"));
				AssertNotEquals("Scavenging Organization XML should include ClosestPort element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClosestPort"));

				AssertNotEquals("Scavenging Organization XML should include IsShippingLine empty bool element.", 0, element.Descendants().Count(e => e.Name.LocalName == "IsShippingLine"));
				AssertNotEquals("Scavenging Organization XML should include Mobile empty string element.", 0, element.Descendants().Count(e => e.Name.LocalName == "Mobile"));
				AssertNotEquals("Scavenging Organization XML should include DeliverFromTimeOnly empty datetime element.", 0, element.Descendants().Count(e => e.Name.LocalName == "DeliverFromTimeOnly"));
				AssertNotEquals("Scavenging Organization XML should include PK element.", 0, element.Descendants().Count(e => e.Name.LocalName == "PK"));

				AssertEquals("Scavenging Organization XML should not include empty ClientOrgConsol element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClientOrgConsol"));
			});
		}

		[UseSnapshotProtection]
		public void TestScavengingOrganizationWithConsolExportXml()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				SetupRegistry();
				var factory = new BusinessObjectFactory(connection);

				OrgHeader orgHeader = CreateOrg(factory);
				ZGuid orgHeaderPK = orgHeader.PK;
				ForwardingConsol consol = CreateConsol(factory, orgHeader.MainAddress.PK, new ZGuid(), Constants.TransportModes.Air, "AUSYD", "USCHI");
				consol.JK_SystemCreateTimeUtc = new DateTime(2014, 01, 01, 01, 00, 00);

				factory.Save();

				XElement element = SerializeToOrganizationXml(orgHeaderPK);

				CombineAssertions(delegate
				{
					AssertNotEquals("Scavenging Organization XML should include OrgMiscServ element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgMiscServ"));
					AssertNotEquals("Scavenging Organization XML should include OrgContactCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgContactCollection"));
					AssertNotEquals("Scavenging Organization XML should include OrgAddressCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgAddressCollection"));
					AssertNotEquals("Scavenging Organization XML should include OrgCompanyDataCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgCompanyDataCollection"));
					AssertNotEquals("Scavenging Organization XML should include OrgBrandOrRelatedNameCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgBrandOrRelatedNameCollection"));
					AssertNotEquals("Scavenging Organization XML should include OrgRateTariffLevelCollection element.", 0, element.Descendants().Count(e => e.Name.LocalName == "OrgRateTariffLevelCollection"));
					AssertNotEquals("Scavenging Organization XML should include ClosestPort element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClosestPort"));

					AssertNotEquals("Scavenging Organization XML should include IsShippingLine empty bool element.", 0, element.Descendants().Count(e => e.Name.LocalName == "IsShippingLine"));
					AssertNotEquals("Scavenging Organization XML should include Mobile empty string element.", 0, element.Descendants().Count(e => e.Name.LocalName == "Mobile"));
					AssertNotEquals("Scavenging Organization XML should include DeliverFromTimeOnly empty datetime element.", 0, element.Descendants().Count(e => e.Name.LocalName == "DeliverFromTimeOnly"));
					AssertNotEquals("Scavenging Organization XML should include PK element.", 0, element.Descendants().Count(e => e.Name.LocalName == "PK"));

					AssertNotEquals("Scavenging Organization XML should include ClientOrgConsol element.", 0, element.Descendants().Count(e => e.Name.LocalName == "ClientOrgConsol"));
				});
			}
		}

		void SetupRegistry()
		{
			var collection = new ScavengingSettingCollection();
			var setting = collection.AddNew();
			setting.TaskName = "OrgCollection";
			setting.PeriodStart = new ZDateTime(2013, 10, 19, 01, 00, 00);
			setting.PeriodEnd = new ZDateTime(2014, 01, 19, 01, 00, 00);
			eHubMessagingRegistry.Instance.ScavengingTaskSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		OrgHeader CreateOrg(BusinessObjectFactory factory)
		{
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var address1 = orgHeader.MainAddress;
			address1.OA_Address1 = "1 ADDRESS RD";
			address1.OA_City = "FIRSTVILLE";
			address1.OA_State = "CA";

			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_Address1 = "2 ADDRESS ST";
			address2.OA_City = "SECONDVILLE";
			address2.OA_State = "VA";

			var brand = orgHeader.BrandsOrRelatedNames.AddNew();
			brand.P1_RelatedName = "MR WHIPPED";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "FRED SIMPSON";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "JOHN WALLACE";

			var airline = factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "QA1";
			orgHeader.MiscServ.OM_RM_Airline = airline.PK;
			orgHeader.CompanyData.OB_IsDebtor = true;

			orgHeader.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
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

		XElement SerializeToOrganizationXml(ZGuid orgHeaderPK)
		{
			string actualMessage = "";
			var xmlSerializer = new NativeObjectSerializer();

			using (var dataStream = xmlSerializer.SerializeToScavengingOrganization(orgHeaderPK.ToGuid(), "OrgHeader"))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			return XElement.Load(new StringReader(actualMessage));
		}
	}
}

