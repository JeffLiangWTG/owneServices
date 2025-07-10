using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class ChildCollectionsWithoutCandidateKeysTest : TestCaseWithFactory
	{
		public void TestUpdatingOrgAppointedAgentPortsCollectionDoesNotCreateDuplicate()
		{
			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_FullName = "RATHAUS EARVAAG GMBH";
			agentOrg.OH_RL_NKClosestPort = "DEHAM";
			agentOrg.OH_Code = "RATEARHAM";
			var agentAdd = agentOrg.MainAddress;
			agentAdd.OA_Address1 = "3489 BLAU KASE STRASSE";
			agentAdd.OA_City = "HAMBURG";
			agentAdd.OA_PostCode = "12345";
			agentAdd.OA_RL_NKRelatedPortCode = "DEHAM";
			agentAdd.OA_Code = "DEHAM - 3489BLAUKASESTRAS";
			Factory.Save();

			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgAppointedAgentPortsCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgAppointedAgentPorts - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.AppointedAgentPorts.Count", 1, importedOrg.AppointedAgentPorts.Count);
				var importedAgentPort = importedOrg.AppointedAgentPorts[0];
				AssertEquals("importedAgentPort.O5_SeaAirCarrierOrForwarderType", "FWD", importedAgentPort.O5_SeaAirCarrierOrForwarderType);
				AssertEquals("importedAgentPort.O5_AgentDirection", "BTH", importedAgentPort.O5_AgentDirection);
				AssertEquals("importedAgentPort.O5_PortOrCountry", "DEHAM", importedAgentPort.O5_PortOrCountry);
				AssertEquals("importedAgentPort.O5_OA_AgentOfficeAddress", agentAdd.PK, importedAgentPort.O5_OA_AgentOfficeAddress);
			});

			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgAppointedAgentPortsCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgAppointedAgentPorts - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Update", expectedLog, manager.GetLogs());
			}

			importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.AppointedAgentPorts.Count", 1, importedOrg.AppointedAgentPorts.Count);
				var importedAgentPort = importedOrg.AppointedAgentPorts[0];
				AssertEquals("importedAgentPort.O5_SeaAirCarrierOrForwarderType", "FWD", importedAgentPort.O5_SeaAirCarrierOrForwarderType);
				AssertEquals("importedAgentPort.O5_AgentDirection", "BTH", importedAgentPort.O5_AgentDirection);
				AssertEquals("importedAgentPort.O5_PortOrCountry", "DEHAM", importedAgentPort.O5_PortOrCountry);
				AssertEquals("importedAgentPort.O5_OA_AgentOfficeAddress", agentAdd.PK, importedAgentPort.O5_OA_AgentOfficeAddress);
			});
		}

		#region XMLWithOrgAppointedAgentPortsCollection

		const string XMLWithOrgAppointedAgentPortsCollection = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 42SALISBURYLANE</Code>
						<Language>EN</Language>
						<Address1>42 SALISBURY LANE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383361000</Phone>
						<Fax>+61393361001</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
        <OrgAppointedAgentPortsCollection>
          <OrgAppointedAgentPorts Action=""MERGE"">
            <SeaAirCarrierOrForwarderType>FWD</SeaAirCarrierOrForwarderType>
            <AgentDirection>BTH</AgentDirection>
            <PortOrCountry>DEHAM</PortOrCountry>
            <AgentOfficeAddress>
              <Code>DEHAM - 3489BLAUKASESTRAS</Code>
              <OrgHeader>
                <Code>RATEARHAM</Code>
              </OrgHeader>
            </AgentOfficeAddress>
          </OrgAppointedAgentPorts>
        </OrgAppointedAgentPortsCollection>
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		#endregion

		public void TestUpdatingOrgWebURLCollectionDoesNotCreateDuplicate()
		{
			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgWebURLCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgWebURL - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on add", expectedLog, manager.GetLogs());
			}

			var importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.OrgWebURLs.Count", 1, importedOrg.OrgWebURLs.Count);
				var webURL = importedOrg.OrgWebURLs[0];
				AssertEquals("webURL.PU_Type", "MAI", webURL.PU_Type);
				AssertEquals("webURL.PU_URL", "www.accesspacific.co.nz", webURL.PU_URL);
			});

			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgWebURLCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgWebURL - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Objects already exist", expectedLog, manager.GetLogs());
			}

			importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.OrgWebURLs.Count", 1, importedOrg.OrgWebURLs.Count);
				var webURL = importedOrg.OrgWebURLs[0];
				AssertEquals("webURL.PU_Type", "MAI", webURL.PU_Type);
				AssertEquals("webURL.PU_URL", "www.accesspacific.co.nz", webURL.PU_URL);
			});
		}

		#region XMLWithOrgWebURLCollection

		const string XMLWithOrgWebURLCollection = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 42SALISBURYLANE</Code>
						<Language>EN</Language>
						<Address1>42 SALISBURY LANE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383361000</Phone>
						<Fax>+61393361001</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
				<OrgWebURLCollection> 
					<OrgWebURL Action=""MERGE""> 
            <URL>www.accesspacific.co.nz</URL> 
            <Type>MAI</Type> 
            <IsPrimary>true</IsPrimary> 
            <Description>Main Website</Description> 
	        </OrgWebURL> 
				</OrgWebURLCollection> 
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		#endregion

		public void TestUpdatingOrgStaffAssignmentsCollectionDoesNotCreateDuplicate()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName  = "Jenny Coupland";
			staff.GS_LoginName = "jenny.coupland";
			staff.GS_Code = "JC";

			Factory.Save();

			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgStaffAssignmentsCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgStaffAssignments - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Add", expectedLog, manager.GetLogs());
			}

			var importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.StaffAssignments.Count", 1, importedOrg.StaffAssignments.Count);
				var staffAssignment = importedOrg.StaffAssignments[0];
				AssertEquals("staffAssignment.O8_GS_NKPersonResponsible", "JC", staffAssignment.O8_GS_NKPersonResponsible);
				AssertEquals("staffAssignment.O8_Role", "CAG", staffAssignment.O8_Role);
			});

			using (var stream = new MemoryStream(encoding.GetBytes(XMLWithOrgStaffAssignmentsCollection)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgStaffAssignments - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text on Update", expectedLog, manager.GetLogs());
			}

			importedOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FLAASPMEL"));
			AssertNotNull("Imported Org (with code of 'FLAASPMEL')", importedOrg);
			CombineAssertions(delegate
			{
				AssertEquals("importedOrg.OH_FullName", "FLAT ASP ROADWORKS", importedOrg.OH_FullName);
				AssertEquals("importedOrg.StaffAssignments.Count", 1, importedOrg.StaffAssignments.Count);
				var staffAssignment = importedOrg.StaffAssignments[0];
				AssertEquals("staffAssignment.O8_GS_NKPersonResponsible", "JC", staffAssignment.O8_GS_NKPersonResponsible);
				AssertEquals("staffAssignment.O8_Role", "CAG", staffAssignment.O8_Role);
			});
		}

		#region XMLWithOrgStaffAssignmentsCollection

		const string XMLWithOrgStaffAssignmentsCollection = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ReferenceData xsi:schemaLocation=""http://www.cargowise.com/Schemas/Universal"" xmlns=""http://www.cargowise.com/Schemas/Universal"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""MERGE"">
				<Code>FLAASPMEL</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""MERGE"">
						<Code>AUMEL - 42SALISBURYLANE</Code>
						<Language>EN</Language>
						<Address1>42 SALISBURY LANE</Address1>
						<City>TULLAMARINE</City>
						<State>VIC</State>
						<PostCode>3043</PostCode>
						<Phone>+61383361000</Phone>
						<Fax>+61393361001</Fax>
						<FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
						<LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
						<AIREquipmentNeeded>PSL</AIREquipmentNeeded>
						<RelatedPortCode>
							<Code>AUMEL</Code>
						</RelatedPortCode>
						<OrgAddressCapabilityCollection>
							<OrgAddressCapability Action=""MERGE"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
				<OrgStaffAssignmentsCollection> 
					<OrgStaffAssignments Action=""MERGE""> 
						<Role>CAG</Role> 
						<Department>ALL</Department> 
						<GlbCompany> 
							<Code>EDI</Code> 
						</GlbCompany> 
						<PersonResponsible> 
							<LoginName>jenny.coupland</LoginName> 
							<Code>JC</Code> 
						</PersonResponsible> 
					</OrgStaffAssignments> 
				</OrgStaffAssignmentsCollection> 
			</OrgHeader>
		</Organization>
	</Body>
</ReferenceData>";

		#endregion

	}
}
