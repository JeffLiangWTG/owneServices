using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	class NativeDataMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCanProcessReallyScrewedUpXML()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = @"<There's no way> 
<That this load of %^$#^%$(&*<<*&$!!!>
<Won't give a narsty error.";

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message)", MessageStatus.Rejected, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Error - The ''' character, hexadecimal value 0x27, cannot be included in a name. Line 1, position 7.
				".Trim(), logger.Logs);
		}

		public void TestCanProcessMessageWithOuterValidationProblem()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
  </Body>
</Native>";

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message)", MessageStatus.Rejected, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Error - Body element must not be empty.
				".Trim(), logger.Logs);
		}

		public void TestCanProcessMessageWithUnknownDataSetType()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <RandomData version=""1.0"">
      <RefRandom Action=""INSERT"">
        <Code>SING</Code>
        <IsActive>true</IsActive>
        <Name>SING-MEISTERS</Name>
        <Prospect>true</Prospect>
        <Language>EN</Language>
        <SongCollection>
          <Song Action=""MERGE"">
            <Code>MYL</Code>
            <Name>My Life</Name>
          </Song>
        </SongCollection>
      </RefRandom>
    </RandomData>
  </Body>
</Native>";

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message)", MessageStatus.Rejected, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Error - Could not load EntitySet with Entity Set Name: RandomData - EntitySet RandomData not defined
Error - EntitySet RandomData not defined
				".Trim(), logger.Logs);
		}

		public void TestCanProcessMessageWithUnexpectedElement()
		{
			#region NativeOrganizationWithUnexpectedElement

			const string NativeOrganizationWithUnexpectedElement = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""INSERT"">
        <Code>BENTOAPER</Code>
        <IsActive>true</IsActive>
        <FullName>BENS TOAST COMPANY</FullName>
        <IsConsignor>true</IsConsignor>
        <Language>EN</Language>
        <OrgNonExistentCollection>
          <OrgNonExistent Action=""MERGE"">
            <IsActive>true</IsActive>
            <Code>MY ADDRESS</Code>
            <Language>EN</Language>
            <Address1>MY ADDRESS</Address1>
            <City>AWESOMNIA</City>
            <State>WA</State>
            <PostCode>7777</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUPER</Code>
            </RelatedPortCode>
          </OrgNonExistent>
        </OrgNonExistentCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUPER</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			#endregion

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganizationWithUnexpectedElement;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine ignoring unrecognised elements", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: BENTOAPER
Information - External Code: BENTOAPER
				".Trim(), logger.Logs);
		}

		public void TestCanProcessValidOrganization()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
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
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			var originalLogsCount = Factory.GetDatabaseCount(typeof(StmALog));

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			MessageStatus result;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				result = processor.Process(message);
			}

			AssertEquals("processor.Process(message) - Should process fine ignoring unrecognised elements", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: FLAASPMEL
				".Trim(), logger.Logs);

			var currentLogsCount = Factory.GetDatabaseCount(typeof(StmALog));

			AssertEquals("logs.Length == 1", 1, currentLogsCount - originalLogsCount);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImportCode);
			var dimEvent = Factory.LoadTop1<IStmALog>(query);

			CombineAssertions("dimEvent", () =>
			{
				AssertEquals("dimEvent.SL_GB_NKBranch", Environment.Env.CurrentBranch.Code, dimEvent.SL_GB_NKBranch);
				AssertEquals("dimEvent.SL_GE_NKDepartment", Environment.Env.CurrentDepartment.Code, dimEvent.SL_GE_NKDepartment);
				AssertEquals("dimEvent.SL_GS_NKUser", User.InterchangeUserCode, dimEvent.SL_GS_NKUser);
			});
		}

		public void TestCanProcessValidOrganizationWithEnableCodeMapping_HasNoOrgCode()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: 
				".Trim(), logger.Logs.Trim());
		}

		public void TestCanProcessValidOrganizationWithDisableCodeMapping_HasNoOrgCode()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: 
				".Trim(), logger.Logs.Trim());
		}

		public void TestCanProcessValidOrganizationWithDisableCodeMapping_HasOrgCode_UserCanEdit()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<Code>JAYS</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			Environment.Env.Registry.CanUserEditOrganisationCode = true;

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: JAYS
Information - External Code: JAYS
				".Trim(), logger.Logs.Trim());
		}

		public void TestCanProcessValidOrganizationWithDisableCodeMapping_HasOrgCode_UserCannotEdit()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>false</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<Code>JAYS</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			Environment.Env.Registry.CanUserEditOrganisationCode = false;

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: JAYS
				".Trim(), logger.Logs.Trim());
		}

		public void TestCanProcessValidOrganizationWithEnableCodeMapping_HasOrgCode_UserCannotEdit()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<Code>JAYS</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			Environment.Env.Registry.CanUserEditOrganisationCode = false;

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: JAYS
				".Trim(), logger.Logs.Trim());
		}

		public void TestCanProcessValidOrganizationWithEnableCodeMapping_HasOrgCode_UserCanEdit()
		{
			#region NativeOrganisationXML

			const string NativeOrganisationXML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
	<Header>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Organization>
			<OrgHeader Action=""INSERT"">
				<Code>JAYS</Code>
				<IsActive>true</IsActive>
				<FullName>FLAT ASP ROADWORKS</FullName>
				<IsForwarder>true</IsForwarder>
				<ClosestPort>
					<Code>AUMEL</Code>
				</ClosestPort>
				<OrgAddressCollection>
					<OrgAddress Action=""INSERT"">
						<IsActive>true</IsActive>
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
							<OrgAddressCapability Action=""INSERT"">
								<AddressType>OFC</AddressType>
								<IsMainAddress>true</IsMainAddress>
							</OrgAddressCapability>
						</OrgAddressCapabilityCollection>
					</OrgAddress>
				</OrgAddressCollection>
			</OrgHeader>
		</Organization>
	</Body>
</Native>";

			#endregion

			Environment.Env.Registry.CanUserEditOrganisationCode = true;

			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = NativeOrganisationXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			var result = processor.Process(message);

			AssertEquals("processor.Process(message) - Should process fine regardless EnableCodeMapping setting", MessageStatus.Processed, result);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Organization
Information - Local Code: FLAASPMEL
Information - External Code: JAYS
				".Trim(), logger.Logs.Trim());
		}

		[TestDate(2020, 1, 1)]
		public void TestSetUsesDepthFirstTraversalForUpdate()
		{
			#region Xml blob
			var nativeXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <WorkflowTemplate version=""2.0"">
      <ProcessTaskTemplate Action=""MERGE"">
        <PK>8c07f0b1-d5e8-4c90-95b5-b0e27fced40c</PK>
        <IsSystem>false</IsSystem>
        <IsActive>true</IsActive>
        <ProcessType>SHP</ProcessType>
        <SubType1></SubType1>
        <SubType2></SubType2>
        <SubType3></SubType3>
        <SubType4></SubType4>
        <SubType5></SubType5>
        <LoadPortCountry></LoadPortCountry>
        <DischargePortCountry></DischargePortCountry>
        <OrgAssessmentOrder></OrgAssessmentOrder>
        <EffectiveStartDateUtc></EffectiveStartDateUtc>
        <RespondToCascadedEvents>false</RespondToCascadedEvents>
        <RecalculateScheduledDate>true</RecalculateScheduledDate>
        <TaskFallbackMethod>EFB</TaskFallbackMethod>
        <MilestoneFallbackMethod>EFB</MilestoneFallbackMethod>
        <TriggerFallbackMethod>EFB</TriggerFallbackMethod>
        <SystemCreateTimeUtc>2020-11-17T03:49:00</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2020-11-17T03:49:00</SystemLastEditTimeUtc>
        <IsPartialTemplate>false</IsPartialTemplate>
        <IsUniversal>false</IsUniversal>
        <EffectiveEndDateUtc></EffectiveEndDateUtc>
        <Name>JAMIE</Name>
        <Description></Description>
        <CustomFieldFallback>NFB</CustomFieldFallback>
        <ReleaseGroupFallbackMethod>EFB</ReleaseGroupFallbackMethod>
        <IsScreenLayoutFallback>false</IsScreenLayoutFallback>
        <ProcessTasksCollection>
          <ProcessTasks Action=""MERGE"">
            <PK>67f91d2a-c3a8-4abc-8202-e3389a366668</PK>
            <IsPublished>true</IsPublished>
            <TaskID>T00008888</TaskID>
            <Sequence>1</Sequence>
            <Type>UDF</Type>
            <IsInterruptable>false</IsInterruptable>
            <Status>OPN</Status>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <ExceptionAddedUtc></ExceptionAddedUtc>
            <IsCalendarItem>false</IsCalendarItem>
            <ScheduledDateUtc></ScheduledDateUtc>
            <EstDuration></EstDuration>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <ActualDateUtc></ActualDateUtc>
            <ActualDuration></ActualDuration>
            <SuspendedAtUtc></SuspendedAtUtc>
            <CompletedTimeUtc></CompletedTimeUtc>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Notes></Notes>
            <CardNote></CardNote>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2></Condition2>
            <TriggerCondition></TriggerCondition>
            <RespondToCascadedEvents>false</RespondToCascadedEvents>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ReferencedID></ReferencedID>
            <ReferencedTableCode></ReferencedTableCode>
            <SuspendedAt></SuspendedAt>
            <ActualDate></ActualDate>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <ScheduledDate></ScheduledDate>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <LineTriggerType></LineTriggerType>
            <Description>TASK</Description>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <TriggerContext>DEF</TriggerContext>
            <IsResetBeingAppliedToThisTask>false</IsResetBeingAppliedToThisTask>
            <Condition2Value></Condition2Value>
            <GlbCapability />
            <GlbStaff />
            <MilestoneEvent TableName=""StmEvent"" />
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <AssignedGroup TableName=""GlbGroup"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <OrgAddress />
            <OrgContact />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
        </ProcessTasksCollection>
        <OrgAddress />
        <OrgHeader />
        <WhsWarehouse />
        <GlbCompany>
          <Code>EDI</Code>
          <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
        </GlbCompany>
        <GlbBranch />
        <GlbDepartment />
        <BufferManagementSystem TableName=""BMSystem"" />
      </ProcessTaskTemplate>
    </WorkflowTemplate>
  </Body>
</Native>";
			#endregion

			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = nativeXML;
			var message2 = Factory.New<IEDIMessage>();
			message2.EM_MessageText = nativeXML.Replace("<Description>TASK</Description>", "<Description>EDITED TASK</Description>");

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);

			var template = new BusinessObjectFactory().Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "JAMIE")).Single();
			AssertEquals("Precondition: No edit events", 0, template.Logs.Find(l => l.SL_SE_NKEvent == "EDT" && l.SL_PostedTimeUtc == ZDateTime.Now).Count());

			TestDateAttribute.AddDays(1);
			processor.Process(message2);

			template = new BusinessObjectFactory().Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name,"JAMIE")).Single();
			AssertEquals("Do not have EDT event", 0, template.Logs.Find(l => l.SL_SE_NKEvent == "EDT" && l.SL_PostedTimeUtc == ZDateTime.Now).Count());
			AssertEquals("Audit Fields Updated", ZDateTime.Now, template.P0_SystemLastEditTimeUtc);
		}

		public void TestCanUpdateBloblColumnsWithNull()
		{
			#region Xml blob
			var nativeXML = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>9999723</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <WorkflowTemplate version=""2.0"">
      <ProcessTaskTemplate Action=""MERGE"">
        <PK>9bf33f72-4ac3-4d2d-a834-ecaf52f84b1c</PK>
        <ProcessType>SHP</ProcessType>
        <SubType1></SubType1>
        <SubType2></SubType2>
        <SubType3></SubType3>
        <LoadPortCountry></LoadPortCountry>
        <DischargePortCountry></DischargePortCountry>
        <SubType4></SubType4>
        <OrgAssessmentOrder></OrgAssessmentOrder>
        <FormState>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8Rm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2UgeG1sbnM6eHNkPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM9Imh0dHA6Ly93d3cuZWRpLmNvbS5hdS9FbnRlcnByaXNlU2VydmljZS8iPg0KICA8VGFiPg0KICAgIDxOYW1lPlNoaXBtZW50RGV0YWlsc1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkJhc2ljIFJlZ2lzdHJhdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkaXRpb25hbFRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkFkZGl0aW9uYWwgRGV0YWlsPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5SZWxhdGVkU2hpcG1lbnRzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UmVsYXRlZCBTaGlwbWVudHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPlJvdXRpbmdUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Sb3V0aW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Db250YWluZXJEZXRhaWxzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGFja2luZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+UGlja3VwVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGlja3VwPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5EZWxpdmVyeVRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkRlbGl2ZXJ5PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Xb3JrZmxvd1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldvcmtmbG93ICZhbXA7IFRyYWNraW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5CaWxsaW5nVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbGluZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkcmVzc2VzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkcmVzc2VzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Ccm9rZXJhZ2VUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ccm9rZXJhZ2U8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPkRvY0RhdGFUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Eb2MgRGF0YTwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+ZURvY3NUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5lRG9jczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+Tm90ZXNUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RlczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+RXZlbnRUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Mb2dzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkFkZGl0aW9uYWxUZXJtczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkaXRpb25hbCBUZXJtczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BaXJ3YXlCaWxsRGltczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWlyIFdheWJpbGwgRGltczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BdmlhdGlvblNlY3VyaXR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5BdmlhdGlvbiBTZWN1cml0eTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5CaWxsRGV0YWlsczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbCBQcmludHMvSXNzdWUgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5DaGFyZ2VhYmxlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VhYmxlPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjU8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q2hhcmdlc0FwcGx5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VzIEFwcGx5PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI0PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbW11bml0eVRyYW5zaXRTdGF0dXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbW11bml0eSBUcmFuc2l0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Db25zb2xzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Db25zb2xpZGF0aW9uIERldGFpbHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbnRhaW5lck1vZGVPdmVycmlkZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q29udGFpbmVyIE1vZGUgT3ZlcnJpZGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjI8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdBZ2VudEFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEFnZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdDdXN0b21lckFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEN1c3RvbWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q3VzdG9tRmllbGRzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DdXN0b20gRmllbGRzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkN1c3RvbXNFbnRyeU51bWJlcjwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q3VzdG9tcyBDbGVhcmFuY2UvUGVybWl0IE5vLjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5FRnJlaWdodFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+ZS1mcmVpZ2h0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Fc3RFeHBvcnRDdXN0b21zQ2xlYXJMYWJlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXN0aW1hdGVkIEV4cG9ydCBDbGVhcmFuY2UgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkV4cG9ydFN0YXRlbWVudDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXhwb3J0ZXIgU3RhdGVtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI1PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkRlc2NyaXB0aW9uPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Hb29kcyBEZXNjcmlwdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj45PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkhvdXNlQmlsbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SG91c2UgYmlsbCBOdW1iZXI8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Ib3VzZWJpbGxUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ib3VzZSBiaWxsIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTk8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGF5bWVudFRlcm08L05hbWU+DQogICAgPERlc2NyaXB0aW9uPklOQ08gLyBQYXltZW50IFRlcm08L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+SVNGQmlsbFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SVNGIEJpbGwgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI5PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkxvYWRpbmdNZXRlcnM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkxvYWRpbmcgTWV0ZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjQ8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+TWFya3NOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5NYXJrcyAmYW1wOyBOdW1iZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjEwPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk5vdGlmeVBhcnR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RpZnkgUGFydHk8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T25Cb2FyZDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+T24gQm9hcmQgRGV0YWlsczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMTwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5PcmRlckxpbmtzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5PcmRlciBNYW5hZ2VtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T3JkZXJVcGRhdGVDdXRPZmY8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yZGVyIFVwZGF0ZSBDdXRvZmYgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk9yaWdpbkRlc3RpbmF0aW9uRGF0ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yaWdpbiwgRGVzdGluYXRpb24gYW5kIERhdGVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGFja3NWYWx1ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPlBhY2thZ2VzIGFuZCBHb29kcyBWYWx1ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj42PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlBoYXNlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5QaGFzZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5SZWZlcmVuY2VOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWZlcmVuY2UgTnVtYmVyczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5BZGRpdGlvbmFsIERldGFpbDwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlJlbGVhc2VUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWxlYXNlIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTc8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2NyZWVuaW5nU3RhdHVzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TY3JlZW5pbmcgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI3PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlNlcnZpY2VMZXZlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZSBMZXZlbDwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5TZXJ2aWNlczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2hpcHBlckNPRDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2hpcHBlciBDT0Q8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjY8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+RnJlaWdodFNwb3RSYXRlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TcG90IFJhdGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+V2VpZ2h0Vm9sdW1lPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5XZWlnaHQvVm9sIENsaWVudC9DYXJyaWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPldlaWdodFZvbHVtZUNoYXJnZWFibGU8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldlaWdodC9Wb2x1bWU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5GcmVpZ2h0UmF0ZXNBbmRHYXRld2F5czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RnJlaWdodCBSYXRlcyBhbmQgR2F0ZXdheXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCjwvRm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2U+</FormState>
        <RespondToCascadedEvents>false</RespondToCascadedEvents>
        <RecalculateScheduledDate>true</RecalculateScheduledDate>
        <EffectiveStartDateUtc></EffectiveStartDateUtc>
        <TaskFallbackMethod>EFB</TaskFallbackMethod>
        <MilestoneFallbackMethod>EFB</MilestoneFallbackMethod>
        <TriggerFallbackMethod>EFB</TriggerFallbackMethod>
        <IsSystem>false</IsSystem>
        <IsActive>false</IsActive>
        <SubType5></SubType5>
        <SystemCreateTimeUtc>2016-01-15T15:09:00</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2020-08-04T13:33:00</SystemLastEditTimeUtc>
        <IsPartialTemplate>false</IsPartialTemplate>
        <IsUniversal>false</IsUniversal>
        <EffectiveEndDateUtc></EffectiveEndDateUtc>
        <CustomFieldFallback>NFB</CustomFieldFallback>
        <Name>SHP, global</Name>
        <Description>Global shipment to various (ODS, uBase, eDC, D-Track)</Description>
        <ReleaseGroupFallbackMethod>EFB</ReleaseGroupFallbackMethod>
        <ProcessTasksCollection>
          <ProcessTasks Action=""MERGE"">
            <PK>a1657888-8844-4f2e-b92e-211ebe87de57</PK>
            <TaskID>T518559990</TaskID>
            <Sequence>2027</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value></Condition2Value>
            <RespondToCascadedEvents>true</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>C2C - Manual Resend</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>-06951432810000000656</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <ProcessTaskNotification_RegularTriggersCollection TableName=""ProcessTaskNotification"">
              <ProcessTaskNotification_RegularTriggers Action=""MERGE"">
                <PK>78e7c86a-c870-4d3b-a42a-334c7aca94d4</PK>
                <TriggerType>XUS</TriggerType>
                <TriggerParty>ORP</TriggerParty>
                <EmailText></EmailText>
                <MessagePurpose>C2C</MessagePurpose>
                <EmailAddr></EmailAddr>
                <SourceTemplateNotification></SourceTemplateNotification>
                <TriggerPartyService></TriggerPartyService>
                <Document TableName=""StmMenuItem"" />
                <StmPrintQueue />
                <Recipient TableName=""OrgHeader"">
                  <Code>ACCGRELAX</Code>
                  <PK>6016fbe9-a8a1-4561-b0a7-fbfcd8b317b4</PK>
                </Recipient>
                <WorkflowTemplate TableName=""ProcessTaskTemplate"" />
                <Trigger TableName=""ProcessTemplateTrigger"" />
              </ProcessTaskNotification_RegularTriggers>
            </ProcessTaskNotification_RegularTriggersCollection>
            <TemplateConditionNoteCollection TableName=""StmNote"">
              <TemplateConditionNote Action=""MERGE"">
                <PK>b62b4a7a-d0f7-41da-9b8b-a89be266a4ff</PK>
                <NoteData></NoteData>
                <NoteText>""&lt;GetDepartureCFSDocAddress.Organisation.OH_Code&gt;""==""6402432266""||""&lt;GetDepartureCFSDocAddress.Organisation.OH_Code&gt;""==""65521006""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432266""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432374""||""&lt;JS_JK_ReceivingAgent&gt;""==""6406259703""||""&lt;JS_JK_ReceivingAgent&gt;""==""6402432470||""&lt;JS_JK_SendingAgent&gt;""==""6402432266""||""&lt;JS_JK_SendingAgent&gt;""==""6402432374""||""&lt;JS_JK_SendingAgent&gt;""==""6406259703""||""&lt;JS_JK_SendingAgent&gt;""==""6402432470
""||""&lt;JS_JK_SendingAgent&gt;""==""6402432375""||""&lt;JS_RL_NKOrigin&gt;""==""USORD""||""&lt;JS_RL_NKOrigin&gt;""==""USCHI""||""&lt;Origin.CountryCode.Code&gt;""==""US""||""&lt;Destination.CountryCode.Code&gt;""==""US""||""&lt;Consols.JK_RL_NKDischargePort&gt;""==""USORD""</NoteText>
                <NoteType>DOC</NoteType>
                <NoteContext>AAA</NoteContext>
                <IsCustomDescription>false</IsCustomDescription>
                <ForceRead>true</ForceRead>
                <Description>User Defined Condition</Description>
                <RelatedCompany TableName=""GlbCompany"" />
              </TemplateConditionNote>
            </TemplateConditionNoteCollection>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>Z47</Code>
              <PK>05487041-bf4d-4d23-aa4b-56c7f8ef5cf3</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
          <ProcessTasks Action=""MERGE"">
            <PK>28f224af-2159-464f-b5ef-9933f859d28b</PK>
            <TaskID>T450903168</TaskID>
            <Sequence>1224</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value></Condition2Value>
            <RespondToCascadedEvents>true</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>GE Power Message Accept</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>11644135080000000044</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <ProcessTaskNotification_RegularTriggersCollection TableName=""ProcessTaskNotification"">
              <ProcessTaskNotification_RegularTriggers Action=""MERGE"">
                <PK>52595058-1edf-410b-8fa1-84d80e5cbc2d</PK>
                <TriggerType>XUE</TriggerType>
                <TriggerParty>ORP</TriggerParty>
                <EmailText></EmailText>
                <MessagePurpose>XG4</MessagePurpose>
                <EmailAddr></EmailAddr>
                <SourceTemplateNotification></SourceTemplateNotification>
                <TriggerPartyService></TriggerPartyService>
                <Document TableName=""StmMenuItem"" />
                <StmPrintQueue />
                <Recipient TableName=""OrgHeader"" />
                <WorkflowTemplate TableName=""ProcessTaskTemplate"" />
                <Trigger TableName=""ProcessTemplateTrigger"" />
              </ProcessTaskNotification_RegularTriggers>
            </ProcessTaskNotification_RegularTriggersCollection>
            <TemplateConditionNoteCollection TableName=""StmNote"">
              <TemplateConditionNote Action=""MERGE"">
                <PK>f35ee213-d4e7-4f5a-a7d7-e15b09027000</PK>
                <NoteData></NoteData>
                <NoteText>""&lt;Numbers[STP].CE_EntryNum&gt;""==""GEPOWEREDIID""</NoteText>
                <NoteType>DOC</NoteType>
                <NoteContext>AAA</NoteContext>
                <IsCustomDescription>false</IsCustomDescription>
                <ForceRead>true</ForceRead>
                <Description>User Defined Condition</Description>
                <RelatedCompany TableName=""GlbCompany"" />
              </TemplateConditionNote>
            </TemplateConditionNoteCollection>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>MAA</Code>
              <PK>dab32f1a-7a32-4466-9d5d-3d3141887e66</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
          <ProcessTasks Action=""MERGE"">
            <PK>09a90fb6-afca-4085-8548-ee3dac01f2f1</PK>
            <TaskID>T450903169</TaskID>
            <Sequence>1225</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value></Condition2Value>
            <RespondToCascadedEvents>false</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>GE Power Message Reject</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>11644135080000000044</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <ProcessTaskNotification_RegularTriggersCollection TableName=""ProcessTaskNotification"">
              <ProcessTaskNotification_RegularTriggers Action=""MERGE"">
                <PK>43b40934-66e0-41ac-9e34-d3d8e226a9fe</PK>
                <TriggerType>XUE</TriggerType>
                <TriggerParty>ORP</TriggerParty>
                <EmailText></EmailText>
                <MessagePurpose>XG4</MessagePurpose>
                <EmailAddr></EmailAddr>
                <SourceTemplateNotification></SourceTemplateNotification>
                <TriggerPartyService></TriggerPartyService>
                <Document TableName=""StmMenuItem"" />
                <StmPrintQueue />
                <Recipient TableName=""OrgHeader"" />
                <WorkflowTemplate TableName=""ProcessTaskTemplate"" />
                <Trigger TableName=""ProcessTemplateTrigger"" />
              </ProcessTaskNotification_RegularTriggers>
            </ProcessTaskNotification_RegularTriggersCollection>
            <TemplateConditionNoteCollection TableName=""StmNote"">
              <TemplateConditionNote Action=""MERGE"">
                <PK>4cb9a680-b052-4ea1-bd4c-134d657ed7b1</PK>
                <NoteData></NoteData>
                <NoteText>""&lt;Numbers[STP].CE_EntryNum&gt;""==""GEPOWEREDIID""</NoteText>
                <NoteType>DOC</NoteType>
                <NoteContext>AAA</NoteContext>
                <IsCustomDescription>false</IsCustomDescription>
                <ForceRead>true</ForceRead>
                <Description>User Defined Condition</Description>
                <RelatedCompany TableName=""GlbCompany"" />
              </TemplateConditionNote>
            </TemplateConditionNoteCollection>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>MRJ</Code>
              <PK>6c26919c-2f7f-4d2a-9d36-bc097304795d</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
          <ProcessTasks Action=""MERGE"">
            <PK>8b9f29bb-bcc5-418c-b07f-bad08111b2f5</PK>
            <TaskID>T450903170</TaskID>
            <Sequence>1226</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value></Condition2Value>
            <RespondToCascadedEvents>false</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>GE PowerActual Pickup</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>11644135080000000044</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <ProcessTaskNotification_RegularTriggersCollection TableName=""ProcessTaskNotification"">
              <ProcessTaskNotification_RegularTriggers Action=""MERGE"">
                <PK>f0b56aa0-6d7c-4aaf-ad58-862a47ef0370</PK>
                <TriggerType>XUE</TriggerType>
                <TriggerParty>ORP</TriggerParty>
                <EmailText></EmailText>
                <MessagePurpose>XG4</MessagePurpose>
                <EmailAddr></EmailAddr>
                <SourceTemplateNotification></SourceTemplateNotification>
                <TriggerPartyService></TriggerPartyService>
                <Document TableName=""StmMenuItem"" />
                <StmPrintQueue />
                <Recipient TableName=""OrgHeader"" />
                <WorkflowTemplate TableName=""ProcessTaskTemplate"" />
                <Trigger TableName=""ProcessTemplateTrigger"" />
              </ProcessTaskNotification_RegularTriggers>
            </ProcessTaskNotification_RegularTriggersCollection>
            <TemplateConditionNoteCollection TableName=""StmNote"">
              <TemplateConditionNote Action=""MERGE"">
                <PK>996c1ce7-73b8-4b58-b004-1ed77a901019</PK>
                <NoteData></NoteData>
                <NoteText>""&lt;Numbers[STP].CE_EntryNum&gt;""==""GEPOWEREDIID""</NoteText>
                <NoteType>DOC</NoteType>
                <NoteContext>AAA</NoteContext>
                <IsCustomDescription>false</IsCustomDescription>
                <ForceRead>true</ForceRead>
                <Description>User Defined Condition</Description>
                <RelatedCompany TableName=""GlbCompany"" />
              </TemplateConditionNote>
            </TemplateConditionNoteCollection>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>PCF</Code>
              <PK>b3e698d2-1298-4172-becd-d15823dd107f</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"" />
          </ProcessTasks>
        </ProcessTasksCollection>
        <OrgAddress />
        <OrgHeader />
        <WhsWarehouse />
        <GlbCompany />
        <GlbBranch />
        <GlbDepartment />
        <BufferManagementSystem TableName=""BMSystem"" />
      </ProcessTaskTemplate>
    </WorkflowTemplate>
  </Body>
</Native>";
			#endregion

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ACCGRELAX";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DAU";
			Factory.Save();

			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = nativeXML;
			var message2 = Factory.New<IEDIMessage>();
			message2.EM_MessageText = nativeXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);
			processor.Process(message2);

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", @"
Information - ProcessTaskTemplate - 1 inserts, 0 updates, 0 deletes
Information - ProcessTasks - 4 inserts, 0 updates, 0 deletes
Information - ProcessTaskNotification_RegularTriggers - 4 inserts, 0 updates, 0 deletes
Information - TemplateConditionNote - 4 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: WorkflowTemplate
Information - ProcessTaskTemplate - 0 inserts, 1 updates, 0 deletes
Information - ProcessTasks - 0 inserts, 0 updates, 0 deletes
Information - ProcessTaskNotification_RegularTriggers - 0 inserts, 0 updates, 0 deletes
Information - TemplateConditionNote - 0 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: WorkflowTemplate
".Trim(), logger.Logs.Trim());
		}

		public void TestImportWorkflowTemplate_TriggerDepartment()
		{
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			#region Xml blob
			var nativeXML = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>9999723</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <WorkflowTemplate version=""2.0"">
      <ProcessTaskTemplate Action=""MERGE"">
        <PK>9bf33f72-4ac3-4d2d-a834-ecaf52f84b1c</PK>
        <ProcessType>SHP</ProcessType>
        <SubType1></SubType1>
        <SubType2></SubType2>
        <SubType3></SubType3>
        <LoadPortCountry></LoadPortCountry>
        <DischargePortCountry></DischargePortCountry>
        <SubType4></SubType4>
        <OrgAssessmentOrder></OrgAssessmentOrder>
        <FormState>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8Rm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2UgeG1sbnM6eHNkPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM9Imh0dHA6Ly93d3cuZWRpLmNvbS5hdS9FbnRlcnByaXNlU2VydmljZS8iPg0KICA8VGFiPg0KICAgIDxOYW1lPlNoaXBtZW50RGV0YWlsc1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkJhc2ljIFJlZ2lzdHJhdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkaXRpb25hbFRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkFkZGl0aW9uYWwgRGV0YWlsPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5SZWxhdGVkU2hpcG1lbnRzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UmVsYXRlZCBTaGlwbWVudHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPlJvdXRpbmdUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Sb3V0aW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Db250YWluZXJEZXRhaWxzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGFja2luZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+UGlja3VwVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGlja3VwPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5EZWxpdmVyeVRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkRlbGl2ZXJ5PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Xb3JrZmxvd1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldvcmtmbG93ICZhbXA7IFRyYWNraW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5CaWxsaW5nVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbGluZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkcmVzc2VzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkcmVzc2VzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Ccm9rZXJhZ2VUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ccm9rZXJhZ2U8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPkRvY0RhdGFUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Eb2MgRGF0YTwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+ZURvY3NUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5lRG9jczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+Tm90ZXNUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RlczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+RXZlbnRUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Mb2dzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkFkZGl0aW9uYWxUZXJtczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkaXRpb25hbCBUZXJtczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BaXJ3YXlCaWxsRGltczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWlyIFdheWJpbGwgRGltczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BdmlhdGlvblNlY3VyaXR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5BdmlhdGlvbiBTZWN1cml0eTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5CaWxsRGV0YWlsczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbCBQcmludHMvSXNzdWUgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5DaGFyZ2VhYmxlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VhYmxlPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjU8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q2hhcmdlc0FwcGx5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VzIEFwcGx5PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI0PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbW11bml0eVRyYW5zaXRTdGF0dXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbW11bml0eSBUcmFuc2l0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Db25zb2xzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Db25zb2xpZGF0aW9uIERldGFpbHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbnRhaW5lck1vZGVPdmVycmlkZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q29udGFpbmVyIE1vZGUgT3ZlcnJpZGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjI8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdBZ2VudEFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEFnZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdDdXN0b21lckFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEN1c3RvbWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q3VzdG9tRmllbGRzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DdXN0b20gRmllbGRzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkN1c3RvbXNFbnRyeU51bWJlcjwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q3VzdG9tcyBDbGVhcmFuY2UvUGVybWl0IE5vLjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5FRnJlaWdodFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+ZS1mcmVpZ2h0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Fc3RFeHBvcnRDdXN0b21zQ2xlYXJMYWJlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXN0aW1hdGVkIEV4cG9ydCBDbGVhcmFuY2UgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkV4cG9ydFN0YXRlbWVudDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXhwb3J0ZXIgU3RhdGVtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI1PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkRlc2NyaXB0aW9uPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Hb29kcyBEZXNjcmlwdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj45PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkhvdXNlQmlsbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SG91c2UgYmlsbCBOdW1iZXI8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Ib3VzZWJpbGxUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ib3VzZSBiaWxsIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTk8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGF5bWVudFRlcm08L05hbWU+DQogICAgPERlc2NyaXB0aW9uPklOQ08gLyBQYXltZW50IFRlcm08L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+SVNGQmlsbFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SVNGIEJpbGwgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI5PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkxvYWRpbmdNZXRlcnM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkxvYWRpbmcgTWV0ZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjQ8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+TWFya3NOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5NYXJrcyAmYW1wOyBOdW1iZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjEwPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk5vdGlmeVBhcnR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RpZnkgUGFydHk8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T25Cb2FyZDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+T24gQm9hcmQgRGV0YWlsczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMTwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5PcmRlckxpbmtzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5PcmRlciBNYW5hZ2VtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T3JkZXJVcGRhdGVDdXRPZmY8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yZGVyIFVwZGF0ZSBDdXRvZmYgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk9yaWdpbkRlc3RpbmF0aW9uRGF0ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yaWdpbiwgRGVzdGluYXRpb24gYW5kIERhdGVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGFja3NWYWx1ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPlBhY2thZ2VzIGFuZCBHb29kcyBWYWx1ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj42PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlBoYXNlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5QaGFzZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5SZWZlcmVuY2VOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWZlcmVuY2UgTnVtYmVyczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5BZGRpdGlvbmFsIERldGFpbDwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlJlbGVhc2VUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWxlYXNlIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTc8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2NyZWVuaW5nU3RhdHVzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TY3JlZW5pbmcgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI3PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlNlcnZpY2VMZXZlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZSBMZXZlbDwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5TZXJ2aWNlczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2hpcHBlckNPRDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2hpcHBlciBDT0Q8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjY8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+RnJlaWdodFNwb3RSYXRlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TcG90IFJhdGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+V2VpZ2h0Vm9sdW1lPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5XZWlnaHQvVm9sIENsaWVudC9DYXJyaWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPldlaWdodFZvbHVtZUNoYXJnZWFibGU8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldlaWdodC9Wb2x1bWU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5GcmVpZ2h0UmF0ZXNBbmRHYXRld2F5czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RnJlaWdodCBSYXRlcyBhbmQgR2F0ZXdheXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCjwvRm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2U+</FormState>
        <RespondToCascadedEvents>false</RespondToCascadedEvents>
        <RecalculateScheduledDate>true</RecalculateScheduledDate>
        <EffectiveStartDateUtc></EffectiveStartDateUtc>
        <TaskFallbackMethod>EFB</TaskFallbackMethod>
        <MilestoneFallbackMethod>EFB</MilestoneFallbackMethod>
        <TriggerFallbackMethod>EFB</TriggerFallbackMethod>
        <IsSystem>false</IsSystem>
        <IsActive>false</IsActive>
        <SubType5></SubType5>
        <SystemCreateTimeUtc>2016-01-15T15:09:00</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2020-08-04T13:33:00</SystemLastEditTimeUtc>
        <IsPartialTemplate>false</IsPartialTemplate>
        <IsUniversal>false</IsUniversal>
        <EffectiveEndDateUtc></EffectiveEndDateUtc>
        <CustomFieldFallback>NFB</CustomFieldFallback>
        <Name>SHP, global</Name>
        <Description>Global shipment to various (ODS, uBase, eDC, D-Track)</Description>
        <ReleaseGroupFallbackMethod>EFB</ReleaseGroupFallbackMethod>
        <ProcessTasksCollection>
          <ProcessTasks Action=""MERGE"">
            <PK>8b9f29bb-bcc5-418c-b07f-bad08111b2f5</PK>
            <TaskID>T450903170</TaskID>
            <Sequence>1226</Sequence>
            <Type>TRG</Type>
            <Status>OPN</Status>
            <ScheduledDate></ScheduledDate>
            <EstDuration></EstDuration>
            <ActualDate></ActualDate>
            <ActualDuration></ActualDuration>
            <Notes></Notes>
            <MilestoneExceptionAdded></MilestoneExceptionAdded>
            <IsCalendarItem>false</IsCalendarItem>
            <SuspendedAt></SuspendedAt>
            <TotalSuspendedDuration></TotalSuspendedDuration>
            <NonWorkHours></NonWorkHours>
            <TaskCannotBeDeleted>false</TaskCannotBeDeleted>
            <Condition1></Condition1>
            <AndOr></AndOr>
            <Condition2>UDF</Condition2>
            <ReferencedID></ReferencedID>
            <IsPublished>true</IsPublished>
            <EstimatedDefaultedFrom></EstimatedDefaultedFrom>
            <EstimatedDefaultTimeDelta></EstimatedDefaultTimeDelta>
            <EstimatedDefaultFromPredecessor>0</EstimatedDefaultFromPredecessor>
            <TriggerField></TriggerField>
            <ParentTemplateID></ParentTemplateID>
            <ExceptionAddedUTC></ExceptionAddedUTC>
            <ScheduledDateUTC></ScheduledDateUTC>
            <ActualDateUTC></ActualDateUTC>
            <SuspendedAtUTC></SuspendedAtUTC>
            <Condition2Value></Condition2Value>
            <RespondToCascadedEvents>false</RespondToCascadedEvents>
            <CascadedEventsContext></CascadedEventsContext>
            <EstimateVariationFactor>2.00</EstimateVariationFactor>
            <EstimatedTimeToComplete></EstimatedTimeToComplete>
            <CompletedTimeUtc></CompletedTimeUtc>
            <OriginalScheduledDateUtc></OriginalScheduledDateUtc>
            <RecalculateScheduledDate>true</RecalculateScheduledDate>
            <IsInterruptable>false</IsInterruptable>
            <Description>GE PowerActual Pickup</Description>
            <ReferencedTableCode></ReferencedTableCode>
            <CardNote>11644135080000000044</CardNote>
            <LineTriggerType></LineTriggerType>
            <TriggerCondition></TriggerCondition>
            <EstimatedHandoverTimeUtc></EstimatedHandoverTimeUtc>
            <TriggerFiredCountdown>100</TriggerFiredCountdown>
            <PenetrationResetDateUtc></PenetrationResetDateUtc>
            <TriggerContext>DEF</TriggerContext>
            <GlbCapability />
            <GlbStaff />
            <AssignedGroup TableName=""GlbGroup"" />
            <OrgAddress />
            <OrgContact />
            <MilestoneEvent TableName=""StmEvent"">
              <Code>PCF</Code>
              <PK>b3e698d2-1298-4172-becd-d15823dd107f</PK>
            </MilestoneEvent>
            <TaskCompletionEvent TableName=""StmEvent"" />
            <ExceptionEvent TableName=""StmEvent"" />
            <OriginCountry TableName=""RefCountry"" />
            <DestinationCountry TableName=""RefCountry"" />
            <GlbCompany />
            <ProcessHeader />
            <TriggerBranch TableName=""GlbBranch"" />
            <TriggerDepartment TableName=""GlbDepartment"">
              <PK>{department.PK}</PK>
            </TriggerDepartment>
          </ProcessTasks>
        </ProcessTasksCollection>
        <OrgAddress />
        <OrgHeader />
        <WhsWarehouse />
        <GlbCompany />
        <GlbBranch />
        <GlbDepartment />
        <BufferManagementSystem TableName=""BMSystem"" />
      </ProcessTaskTemplate>
    </WorkflowTemplate>
  </Body>
</Native>";
			#endregion

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ACCGRELAX";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DAU";
			Factory.Save();

			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = nativeXML;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);

			ProcessTask LoadImportedTrigger() => Factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_GE_TriggerDepartment, department.PK));

			AssertNull("Precondition - Bizo does not exist yet", LoadImportedTrigger());
			processor.Process(message1);
			AssertNotNull("Trigger department should be imported form native xml", LoadImportedTrigger());
		}

		public void TestImportWorkflowTemplate_IgnoreIsSystemField()
		{
			ZGuid testPK = ZGuid.NewZGuid();
			#region Xml blob
			var nativeXML = $@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>9999723</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <WorkflowTemplate version=""2.0"">
      <ProcessTaskTemplate Action=""MERGE"">
		<PK>{testPK}</PK>
		<Name>test</Name>
		<IsSystem>true</IsSystem>
        <ProcessType>SHP</ProcessType>
        <SubType1></SubType1>
        <SubType2></SubType2>
        <SubType3></SubType3>
        <LoadPortCountry></LoadPortCountry>
        <DischargePortCountry></DischargePortCountry>
        <SubType4></SubType4>
        <OrgAssessmentOrder></OrgAssessmentOrder>
        <FormState>PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8Rm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2UgeG1sbnM6eHNkPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYSIgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeG1sbnM9Imh0dHA6Ly93d3cuZWRpLmNvbS5hdS9FbnRlcnByaXNlU2VydmljZS8iPg0KICA8VGFiPg0KICAgIDxOYW1lPlNoaXBtZW50RGV0YWlsc1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkJhc2ljIFJlZ2lzdHJhdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkaXRpb25hbFRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkFkZGl0aW9uYWwgRGV0YWlsPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5SZWxhdGVkU2hpcG1lbnRzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UmVsYXRlZCBTaGlwbWVudHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPlJvdXRpbmdUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Sb3V0aW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Db250YWluZXJEZXRhaWxzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGFja2luZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+UGlja3VwVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+UGlja3VwPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5EZWxpdmVyeVRhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkRlbGl2ZXJ5PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Xb3JrZmxvd1RhYlBhZ2U8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldvcmtmbG93ICZhbXA7IFRyYWNraW5nPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5CaWxsaW5nVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbGluZzwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+QWRkcmVzc2VzVGFiUGFnZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkcmVzc2VzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPFRhYj4NCiAgICA8TmFtZT5Ccm9rZXJhZ2VUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ccm9rZXJhZ2U8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogIDwvVGFiPg0KICA8VGFiPg0KICAgIDxOYW1lPkRvY0RhdGFUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Eb2MgRGF0YTwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+ZURvY3NUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5lRG9jczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+Tm90ZXNUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RlczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgPC9UYWI+DQogIDxUYWI+DQogICAgPE5hbWU+RXZlbnRUYWJQYWdlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Mb2dzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICA8L1RhYj4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkFkZGl0aW9uYWxUZXJtczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWRkaXRpb25hbCBUZXJtczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BaXJ3YXlCaWxsRGltczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QWlyIFdheWJpbGwgRGltczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5BdmlhdGlvblNlY3VyaXR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5BdmlhdGlvbiBTZWN1cml0eTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5CaWxsRGV0YWlsczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+QmlsbCBQcmludHMvSXNzdWUgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5DaGFyZ2VhYmxlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VhYmxlPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjU8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q2hhcmdlc0FwcGx5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DaGFyZ2VzIEFwcGx5PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI0PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbW11bml0eVRyYW5zaXRTdGF0dXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbW11bml0eSBUcmFuc2l0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Db25zb2xzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Db25zb2xpZGF0aW9uIERldGFpbHM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkNvbnRhaW5lck1vZGVPdmVycmlkZTwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q29udGFpbmVyIE1vZGUgT3ZlcnJpZGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjI8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdBZ2VudEFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEFnZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q29udHJvbGxpbmdDdXN0b21lckFkZHJlc3M8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkNvbnRyb2xsaW5nIEN1c3RvbWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+ZmFsc2U8L1Zpc2libGU+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+Q3VzdG9tRmllbGRzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5DdXN0b20gRmllbGRzPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkN1c3RvbXNFbnRyeU51bWJlcjwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+Q3VzdG9tcyBDbGVhcmFuY2UvUGVybWl0IE5vLjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xNDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5FRnJlaWdodFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+ZS1mcmVpZ2h0IFN0YXR1czwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4zMjwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Fc3RFeHBvcnRDdXN0b21zQ2xlYXJMYWJlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXN0aW1hdGVkIEV4cG9ydCBDbGVhcmFuY2UgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkV4cG9ydFN0YXRlbWVudDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RXhwb3J0ZXIgU3RhdGVtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI1PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkRlc2NyaXB0aW9uPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Hb29kcyBEZXNjcmlwdGlvbjwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj45PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkhvdXNlQmlsbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SG91c2UgYmlsbCBOdW1iZXI8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MDwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5Ib3VzZWJpbGxUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ib3VzZSBiaWxsIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTk8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGF5bWVudFRlcm08L05hbWU+DQogICAgPERlc2NyaXB0aW9uPklOQ08gLyBQYXltZW50IFRlcm08L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+SVNGQmlsbFN0YXR1czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+SVNGIEJpbGwgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI5PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPkxvYWRpbmdNZXRlcnM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPkxvYWRpbmcgTWV0ZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjQ8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+TWFya3NOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5NYXJrcyAmYW1wOyBOdW1iZXJzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjEwPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk5vdGlmeVBhcnR5PC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5Ob3RpZnkgUGFydHk8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T25Cb2FyZDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+T24gQm9hcmQgRGV0YWlsczwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yMTwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5PcmRlckxpbmtzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5PcmRlciBNYW5hZ2VtZW50PC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+UmlnaHQgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+T3JkZXJVcGRhdGVDdXRPZmY8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yZGVyIFVwZGF0ZSBDdXRvZmYgRGF0ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPmZhbHNlPC9WaXNpYmxlPg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPk9yaWdpbkRlc3RpbmF0aW9uRGF0ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPk9yaWdpbiwgRGVzdGluYXRpb24gYW5kIERhdGVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+UGFja3NWYWx1ZXM8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPlBhY2thZ2VzIGFuZCBHb29kcyBWYWx1ZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj42PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlBoYXNlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5QaGFzZTwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4yODwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5SZWZlcmVuY2VOdW1iZXJzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWZlcmVuY2UgTnVtYmVyczwvRGVzY3JpcHRpb24+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5BZGRpdGlvbmFsIERldGFpbDwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50PlJpZ2h0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlJlbGVhc2VUeXBlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5SZWxlYXNlIFR5cGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MTc8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2NyZWVuaW5nU3RhdHVzPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TY3JlZW5pbmcgU3RhdHVzPC9EZXNjcmlwdGlvbj4NCiAgICA8R3JvdXA+RGV0YWlsczwvR3JvdXA+DQogICAgPFZpc2libGU+dHJ1ZTwvVmlzaWJsZT4NCiAgICA8VGFiTmFtZT5CYXNpYyBSZWdpc3RyYXRpb248L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgVG9wPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjI3PC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPlNlcnZpY2VMZXZlbDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZSBMZXZlbDwvRGVzY3JpcHRpb24+DQogICAgPEdyb3VwPkRldGFpbHM8L0dyb3VwPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QmFzaWMgUmVnaXN0cmF0aW9uPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4xMzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5TZXJ2aWNlczwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2VydmljZXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5NaWRkbGUgQm90dG9tPC9QbGFjZW1lbnQ+DQogICAgPFBvc2l0aW9uPjA8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+U2hpcHBlckNPRDwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+U2hpcHBlciBDT0Q8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MjY8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+RnJlaWdodFNwb3RSYXRlPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5TcG90IFJhdGU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzE8L1Bvc2l0aW9uPg0KICA8L0ZpZWxkPg0KICA8RmllbGQ+DQogICAgPE5hbWU+V2VpZ2h0Vm9sdW1lPC9OYW1lPg0KICAgIDxEZXNjcmlwdGlvbj5XZWlnaHQvVm9sIENsaWVudC9DYXJyaWVyPC9EZXNjcmlwdGlvbj4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkFkZGl0aW9uYWwgRGV0YWlsPC9UYWJOYW1lPg0KICAgIDxQbGFjZW1lbnQ+TWlkZGxlIFRvcDwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCiAgPEZpZWxkPg0KICAgIDxOYW1lPldlaWdodFZvbHVtZUNoYXJnZWFibGU8L05hbWU+DQogICAgPERlc2NyaXB0aW9uPldlaWdodC9Wb2x1bWU8L0Rlc2NyaXB0aW9uPg0KICAgIDxHcm91cD5EZXRhaWxzPC9Hcm91cD4NCiAgICA8VmlzaWJsZT50cnVlPC9WaXNpYmxlPg0KICAgIDxUYWJOYW1lPkJhc2ljIFJlZ2lzdHJhdGlvbjwvVGFiTmFtZT4NCiAgICA8UGxhY2VtZW50Pk1pZGRsZSBUb3A8L1BsYWNlbWVudD4NCiAgICA8UG9zaXRpb24+MzwvUG9zaXRpb24+DQogIDwvRmllbGQ+DQogIDxGaWVsZD4NCiAgICA8TmFtZT5GcmVpZ2h0UmF0ZXNBbmRHYXRld2F5czwvTmFtZT4NCiAgICA8RGVzY3JpcHRpb24+RnJlaWdodCBSYXRlcyBhbmQgR2F0ZXdheXM8L0Rlc2NyaXB0aW9uPg0KICAgIDxWaXNpYmxlPnRydWU8L1Zpc2libGU+DQogICAgPFRhYk5hbWU+QWRkaXRpb25hbCBEZXRhaWw8L1RhYk5hbWU+DQogICAgPFBsYWNlbWVudD5MZWZ0IEJvdHRvbTwvUGxhY2VtZW50Pg0KICAgIDxQb3NpdGlvbj4wPC9Qb3NpdGlvbj4NCiAgPC9GaWVsZD4NCjwvRm9ybUN1c3RvbWlzYXRpb25TZXR0aW5nc1N0b3JhZ2U+</FormState>
        <RespondToCascadedEvents>false</RespondToCascadedEvents>
        <RecalculateScheduledDate>true</RecalculateScheduledDate>
        <EffectiveStartDateUtc></EffectiveStartDateUtc>
        <TaskFallbackMethod>EFB</TaskFallbackMethod>
        <MilestoneFallbackMethod>EFB</MilestoneFallbackMethod>
        <TriggerFallbackMethod>EFB</TriggerFallbackMethod>
        <IsActive>false</IsActive>
        <SubType5></SubType5>
        <IsPartialTemplate>false</IsPartialTemplate>
        <IsUniversal>false</IsUniversal>
        <EffectiveEndDateUtc></EffectiveEndDateUtc>
        <CustomFieldFallback>NFB</CustomFieldFallback>
        <Description>Global shipment to various (ODS, uBase, eDC, D-Track)</Description>
        <ReleaseGroupFallbackMethod>EFB</ReleaseGroupFallbackMethod>
        <OrgAddress />
        <OrgHeader />
        <WhsWarehouse />
        <GlbCompany />
        <GlbBranch />
        <GlbDepartment />
        <BufferManagementSystem TableName=""BMSystem"" />
      </ProcessTaskTemplate>
    </WorkflowTemplate>
  </Body>
</Native>";
			#endregion

			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = nativeXML;
			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);

			ProcessTaskTemplate import = Factory.LoadTop1<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_Name, "test"));
			Assert(!import.P0_IsSystem);
		}

		public void TestCannotEditSystemTemplates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsSystem = true;
			Factory.Save();

			string export = ExportXMLToString(template);
			export = export.Replace("<ProcessType></ProcessType>", "<ProcessType>SHP</ProcessType>");
			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = export;
			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);

			AssertContains("Warning: Cannot use Native XML to update/delete System ProcessTaskTemplate.", logger.Logs);

			ProcessTaskTemplate reloadTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(template.PK);
			AssertEquals("", reloadTemplate.P0_ProcessType);
		}

		public void TestCannotDeleteSystemTemplates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsSystem = true;
			Factory.Save();

			string export = ExportXMLToString(template);
			export = export.Replace("MERGE", "DELETE");
			var message1 = Factory.New<IEDIMessage>();
			message1.EM_MessageText = export;
			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message1);

			AssertContains("Warning: Cannot use Native XML to update/delete System ProcessTaskTemplate.", logger.Logs);
			ProcessTaskTemplate reloadTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(template.PK);
			AssertNotNull(reloadTemplate);
		}

		#region NativeDataMessageProcessor RatingHeaders

		public void TestNativeDataMessageProcessor_ClientRate_EMLRecipientRole()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var messageText = CreateMessageTextWithRecipientRole(clientRate, "EML");

			var logger = ProcessNativeDataMessage(messageText);

			// breaking change WI00222798: when target company is different from the company exporting rates, we create new rates
			AssertNonQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_ClientRate_CLIRecipientRole()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			clientRate.Header.OH_Code = "CLIENT";

			var messageText = CreateMessageTextWithRecipientRole(clientRate, "CLI");

			var logger = ProcessNativeDataMessage(messageText);

			var extraLogMessage = System.Environment.NewLine + "Information - Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: CLIENT";
			AssertNonQuoteInserted(logger, extraLogMessage);

			var ratingHeaders = new BusinessObjectFactory().Load<RatingHeader>(new ZQuery());
			var costing = ratingHeaders.Single(r => r.TH_RateType == RatingConstants.RatingHeaderTypes.Costing);
			AssertNotNull("Client Rate should be been transformed into a Costing", costing);
			AssertNotEquals("Costing should not be the same as the original Client Rate", costing.PK, clientRate.PK);
		}

		public void TestNativeDataMessageProcessor_ClientRate_CLIRecipientRole_GlobalRateRemainsGlobal()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var client = clientRate.Header;
			client.OH_Code = "CLIENT";

			var globalClientRate = Factory.NewWithValidTestData<ClientRate>();
			globalClientRate.TH_OH = client.PK;
			globalClientRate.TH_GC = ZGuid.Empty;

			Factory.Save();

			AssertNull("Pre-condition: global rates should have no company by default", globalClientRate.Company);
			var messageText = CreateMessageTextWithRecipientRole(globalClientRate, "CLI");

			var logger = ProcessNativeDataMessage(messageText);
			var expectedLogMessage = $@"Information - Publisher set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: CLIENT
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 2 inserts, 0 updates, 0 deletes
Information - RateLines - 2 inserts, 0 updates, 0 deletes
Information - RateLineItems - 2 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertContainsExactLinesInAnyOrder(expectedLogMessage, logger.Logs.Trim());

			var ratingHeaders = new BusinessObjectFactory().Load<RatingHeader>(new ZQuery());
			var costing = ratingHeaders.Single(r => r.TH_RateType == "COS");

			AssertRateLinesExist(costing);
			CombineAssertions("Should be able to process a Global rate via client recipient role without turning it into a Local Rate", () =>
			{
				AssertEquals("Should have added a new costing rather than have updated the existing client rates", 3, ratingHeaders.Length);
				AssertNotEquals("Should be different", costing.PK, globalClientRate.PK);
				AssertNotEquals("Should be different", costing.PK, clientRate.PK);

				AssertNull("Should still be global", globalClientRate.Company);
				AssertNull("Should also be global", costing.Company);

				var lclEntry = costing.LCLRateEntriesForBinding[0];
				var orgEntry = costing.ORGRateEntriesForBinding[0];
				AssertEquals("Publisher should be the target of the EDIMessage", NewBranch.GB_GC, lclEntry.TI_GC_Publisher);
				AssertEquals("Publisher should be the target of the EDIMessage", NewBranch.GB_GC, orgEntry.TI_GC_Publisher);

				Assert("Should use global charge code for global rate", lclEntry.RateLines[0].ChargeCode.IsGlobal);
				Assert("Should use global charge code for global rate", orgEntry.RateLines[0].ChargeCode.IsGlobal);
			});
		}

		public void TestNativeDataMessageProcessor_ClientRate_CLIRecipientRole_OverridesGlbCompanyWithEdiMessageTargetCompany()
		{
			#region NativeRatesXML

			var nativeRatesXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
				<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
					<DataSourceCollection>
						<DataSource>
							<Type>ClientRate</Type>
							<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~SAL</Key>
						</DataSource>
					</DataSourceCollection>
					<ActionPurpose>
						<Code>EVT</Code>
						<Description>Event</Description>
					</ActionPurpose>
					<Company>
						<Code>NUC</Code>
					</Company>
					<EnterpriseID>EDI</EnterpriseID>
					<EventType>
						<Code>EDT</Code>
						<Description>Edited a record</Description>
					</EventType>
					<EventUser>
						<Code>E</Code>
						<Name>CargoWise Support</Name>
					</EventUser>
					<EventBranch>
						<Code>NUB</Code>
					</EventBranch>
					<EventDepartment>
						<Code>BRN</Code>
					</EventDepartment>
					<ServerID>DAT</ServerID>
					<TriggerCount>14</TriggerCount>
					<TriggerDate>2014-11-27T13:17:00+11:00</TriggerDate>
					<TriggerDescription>Send</TriggerDescription>
					<TriggerType>Trigger</TriggerType>
					<RecipientRoleCollection>
						<RecipientRole>
							<Code>CLI</Code>
							<Description>Client</Description>
						</RecipientRole>
					</RecipientRoleCollection>
				</nv:DataContext>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>123.0000</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<AccChargeCode />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AASDRA</Code>=
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
							<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

			#endregion

			var logger = ProcessNativeDataMessage(nativeRatesXML);

			var expectedLogMessages = $@"Warning - Data Context Company 'NUC' does not exist. The default Company for Native XML will be used.
Information - Carrier sell rates have been transformed to client buy rates. Carrier: EDICUS, Client: AASDRA
Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 1 inserts, 0 updates, 0 deletes
Information - RateLines - 1 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			var newFactory = new BusinessObjectFactory();
			var costing = newFactory.LoadTop1<Costing>(new ZQuery());
			AssertEquals("TargetCompany should override 'EDI' company in XML", NewBranch.Company.GC_Code, costing.Company.GC_Code);
		}

		public void TestNativeDataMessageProcessor_ClientRate_ORGRecipientRole()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var messageText = CreateMessageTextWithRecipientRole(clientRate, "ORP");

			var logger = ProcessNativeDataMessage(messageText);

			AssertNonQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_CompanyTariff_EMLRecipientRole()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var messageText = CreateMessageTextWithRecipientRole(companyTariff, "EML");

			var logger = ProcessNativeDataMessage(messageText);

			// breaking change WI00222798: when target company is different from the company exporting rates, we create new rates
			AssertNonQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_CompanyTariff_CLIRecipientRole()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var messageText = CreateMessageTextWithRecipientRole(companyTariff, "CLI");

			var logger = ProcessNativeDataMessage(messageText);

			AssertNonQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_CompanyTariff_ORGRecipientRole()
		{
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var messageText = CreateMessageTextWithRecipientRole(companyTariff, "ORP");

			var logger = ProcessNativeDataMessage(messageText);

			AssertNonQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_Quotation_EMLRecipientRole()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			var messageText = CreateMessageTextWithRecipientRole(quotation, "EML");

			var logger = ProcessNativeDataMessage(messageText);

			// breaking change WI00222798: when target company is different from the company exporting rates, we create new rates
			AssertQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_Quotation_CLIRecipientRole()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			var messageText = CreateMessageTextWithRecipientRole(quotation, "CLI");

			var logger = ProcessNativeDataMessage(messageText);

			AssertQuoteInserted(logger);
		}

		public void TestNativeDataMessageProcessor_Quotation_ORGRecipientRole()
		{
			var quotation = Factory.NewWithValidTestData<Quote>();
			var messageText = CreateMessageTextWithRecipientRole(quotation, "ORP");

			var logger = ProcessNativeDataMessage(messageText);

			AssertQuoteInserted(logger);
		}

		[TestDate(2020, 1, 1)]
		public void TestNativeDataMessageProcessor_AuditUpdatedOnChildUpdate()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "77.1");
			message.EM_GB = NewBranch.PK;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			var expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 1 inserts, 0 updates, 0 deletes
Information - RateLines - 1 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			var newFactory = new BusinessObjectFactory();
			var costing = newFactory.LoadTop1<Costing>(new ZQuery());
			AssertEquals("ADD Log should NOT be added", 0, costing.Logs.Find(l => l.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, costing.TH_SystemLastEditTimeUtc);

			TestDateAttribute.AddDays(1);

			message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "91.8");
			message.EM_GB = NewBranch.PK;

			logger = new TestErrorLogger();
			processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 0 inserts, 0 updates, 0 deletes
Information - RateEntry - 0 inserts, 0 updates, 0 deletes
Information - RateLines - 0 inserts, 0 updates, 0 deletes
Information - RateLineItems - 0 inserts, 1 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			newFactory = new BusinessObjectFactory();
			var reloadCosting = newFactory.LoadTop1<Costing>(new ZQuery());

			AssertEquals("EDT Log should NOT be added", 0, reloadCosting.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && l.SL_PostedTimeUtc == ZDateTime.Now).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, reloadCosting.TH_SystemLastEditTimeUtc);
		}

		[TestDate(2020, 1, 1)]
		public void TestNativeDataMessageProcessor_AuditUpdatedOnChildInsert()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "77.1");
			message.EM_GB = NewBranch.PK;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			var expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 1 inserts, 0 updates, 0 deletes
Information - RateLines - 1 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			var newFactory = new BusinessObjectFactory();
			var costing = newFactory.LoadTop1<Costing>(new ZQuery());
			AssertEquals("ADD Log should NOT be added", 0, costing.Logs.Find(l => l.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, costing.TH_SystemLastEditTimeUtc);

			TestDateAttribute.AddDays(1);

			message = Factory.New<IEDIMessage>();
			message.EM_GB = NewBranch.PK;
			message.EM_MessageText = nativeRatesXML.Replace(@"<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>RateLineItemValue</Value>", @"<RateLineItemsCollection>
											<RateLineItems Action=""INSERT"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93b</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>100</Value>");

			logger = new TestErrorLogger();
			processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 0 inserts, 0 updates, 0 deletes
Information - RateEntry - 0 inserts, 0 updates, 0 deletes
Information - RateLines - 0 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			newFactory = new BusinessObjectFactory();
			var reloadCosting = newFactory.LoadTop1<Costing>(new ZQuery());

			AssertEquals("EDT Log should not be added", 0, reloadCosting.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && l.SL_PostedTimeUtc == ZDateTime.Now).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, reloadCosting.TH_SystemLastEditTimeUtc);
		}

		[TestDate(2020, 1, 1)]
		public void TestNativeDataMessageProcessor_AuditUpdatedOnChildDelete()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "77.1");
			message.EM_GB = NewBranch.PK;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			var expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 1 inserts, 0 updates, 0 deletes
Information - RateLines - 1 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			var newFactory = new BusinessObjectFactory();
			var costing = newFactory.LoadTop1<Costing>(new ZQuery());
			AssertEquals("ADD Log should NOT be added", 0, costing.Logs.Find(l => l.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, costing.TH_SystemLastEditTimeUtc);

			TestDateAttribute.AddDays(1);

			message = Factory.New<IEDIMessage>();
			message.EM_GB = NewBranch.PK;
			message.EM_MessageText = nativeRatesXML.Replace(@"<RateLineItems Action=""MERGE"">", @"<RateLineItems Action=""DELETE"">");
			message.EM_MessageText = message.EM_MessageText.Replace("RateLineItemValue", "77.1");

			logger = new TestErrorLogger();
			processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 0 inserts, 0 updates, 0 deletes
Information - RateEntry - 0 inserts, 0 updates, 0 deletes
Information - RateLines - 0 inserts, 0 updates, 0 deletes
Information - RateLineItems - 0 inserts, 0 updates, 1 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			newFactory = new BusinessObjectFactory();
			var reloadCosting = newFactory.LoadTop1<Costing>(new ZQuery());

			AssertEquals("EDT Log should not be added", 0, reloadCosting.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && l.SL_PostedTimeUtc == ZDateTime.Now).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, reloadCosting.TH_SystemLastEditTimeUtc);
		}

		[TestDate(2020, 1, 1)]
		public void TestNativeDataMessageProcessor_NoAuditUpdateWhenNoChanges()
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "77.1");
			message.EM_GB = NewBranch.PK;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			var expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 1 inserts, 0 updates, 0 deletes
Information - RateLines - 1 inserts, 0 updates, 0 deletes
Information - RateLineItems - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			var newFactory = new BusinessObjectFactory();
			var costing = newFactory.LoadTop1<Costing>(new ZQuery());
			AssertEquals("ADD Log should NOT be added", 0, costing.Logs.Find(l => l.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).ToList().Count);
			AssertEquals("Audit time is updated", ZDateTime.UtcNow, costing.TH_SystemLastEditTimeUtc);

			TestDateAttribute.AddDays(1);

			message = Factory.New<IEDIMessage>();
			message.EM_MessageText = nativeRatesXML.Replace("RateLineItemValue", "77.1");
			message.EM_GB = NewBranch.PK;

			logger = new TestErrorLogger();
			processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			expectedLogMessages = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 0 inserts, 0 updates, 0 deletes
Information - RateEntry - 0 inserts, 0 updates, 0 deletes
Information - RateLines - 0 inserts, 0 updates, 0 deletes
Information - RateLineItems - 0 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertMultilineASCIIEquals("Log Messages after processor.Process(message)", expectedLogMessages, logger.Logs.Trim());

			newFactory = new BusinessObjectFactory();
			var reloadCosting = newFactory.LoadTop1<Costing>(new ZQuery());

			AssertEquals("expect no EDT event", 0, reloadCosting.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecordCode && l.SL_PostedTimeUtc == ZDateTime.Now).ToList().Count);
			AssertEquals("No changes so audit time should not be changed", ZDateTime.UtcNow.AddDays(-1), reloadCosting.TH_SystemLastEditTimeUtc);
		}

		#region NativeRatesXML

		const string nativeRatesXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
			<Header>
				<OwnerCode>EDICUS</OwnerCode>
				<EnableCodeMapping>true</EnableCodeMapping>
			</Header>
			<Body>
				<Rate version=""2.0"">
					<RatingHeader Action=""MERGE"">
						<PK>e583b7a9-014a-4c53-ab9b-bf4aaefbe638</PK>
						<QuoteNumber></QuoteNumber>
						<QuoteDateTime></QuoteDateTime>
						<QuoteEndDate></QuoteEndDate>
						<FollowUpDate></FollowUpDate>
						<Accepted>2014-11-27T00:00:00</Accepted>
						<RateType>SAL</RateType>
						<GlobalRateLevel>0</GlobalRateLevel>
						<GlobalRateDescription></GlobalRateDescription>
						<AirCFX>0.00</AirCFX>
						<SeaCFX>0.00</SeaCFX>
						<ExportAirCFX>0.00</ExportAirCFX>
						<ExportSeaCFX>0.00</ExportSeaCFX>
						<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
						<SystemCreateTimeUtc>2014-09-17T01:14:00</SystemCreateTimeUtc>
						<QuoteCancellationReason></QuoteCancellationReason>
						<IsCancelled>false</IsCancelled>
						<OneTimeQuote>false</OneTimeQuote>
						<IsLocked>false</IsLocked>
						<IsOneOffQuoteConsumed>false</IsOneOffQuoteConsumed>
						<PrintRateLevelOriginCharges>true</PrintRateLevelOriginCharges>
						<PrintRateLevelDestinationCharges>true</PrintRateLevelDestinationCharges>
						<PrintInheritedOriginCharges>true</PrintInheritedOriginCharges>
						<PrintInheritedDestinationCharges>true</PrintInheritedDestinationCharges>
						<RateEntryCollection>
							<RateEntry Action=""MERGE"">
								<PK>f3d74720-1b41-4d4c-b138-d352b0bb05e5</PK>
								<LineOrder>0</LineOrder>
								<RateStartDate>2014-11-01T00:00:00</RateStartDate>
								<RateEndDate>2015-03-01T00:00:00</RateEndDate>
								<Frequency>0</Frequency>
								<CartagePickupAddressPostCode></CartagePickupAddressPostCode>
								<CartageDeliveryAddressPostCode></CartageDeliveryAddressPostCode>
								<OriginLRC Relationship=""COU"">AU</OriginLRC>
								<DestinationLRC Relationship=""COU"">DE</DestinationLRC>
								<PageHeading></PageHeading>
								<PageOpeningText></PageOpeningText>
								<PageClosingText></PageClosingText>
								<QuotePageIncoTerm></QuotePageIncoTerm>
								<BuyersConsolRateMode></BuyersConsolRateMode>
								<SystemCreateTimeUtc>2014-11-24T13:42:00</SystemCreateTimeUtc>
								<SystemLastEditTimeUtc>2014-11-27T03:17:00</SystemLastEditTimeUtc>
								<ContractNumber></ContractNumber>
								<IsCrossTrade>false</IsCrossTrade>
								<MatchContainerRateClass>false</MatchContainerRateClass>
								<DataChecked>false</DataChecked>
								<RateCategory>AIR</RateCategory>
								<TransitTime></TransitTime>
								<Mode>LSE</Mode>
								<FrequencyUnit></FrequencyUnit>
								<FromSuburb TableName=""RefCityTown"" />
								<ToSuburb TableName=""RefCityTown"" />
								<RateLinesCollection>
									<RateLines Action=""MERGE"">
										<PK>343226ff-8cf4-4ec5-a91f-173269b0acd4</PK>
										<LineOrder>0</LineOrder>
										<RateDesc></RateDesc>
										<ConversionFactor>0.000</ConversionFactor>
										<WeightVolume></WeightVolume>
										<WeightVolumeMultiple>0.0</WeightVolumeMultiple>
										<RateCalculator>FLT</RateCalculator>
										<CompanyTariffLevel>0</CompanyTariffLevel>
										<Rounding>DEF</Rounding>
										<RoundingFactor>0.000</RoundingFactor>
										<ActualPercentage>0</ActualPercentage>
										<IsOnPallets>false</IsOnPallets>
										<IsWhsJobLevelCharge>false</IsWhsJobLevelCharge>
										<Condition></Condition>
										<RateDescLocal></RateDescLocal>
										<FeeChargeLevel></FeeChargeLevel>
										<FeeChargeType></FeeChargeType>
										<RateLineItemsCollection>
											<RateLineItems Action=""MERGE"">
												<PK>d26f10fe-597e-46ff-9875-025d8063a93a</PK>
												<LineOrder>1</LineOrder>
												<Type>BAS</Type>
												<BreakMinimum>0.000</BreakMinimum>
												<Break>0.000</Break>
												<BreakWeightVolume></BreakWeightVolume>
												<Value>RateLineItemValue</Value>
												<AgentDeclaredRate>0.0000</AgentDeclaredRate>
												<FlatAmount>0.0000</FlatAmount>
												<Text></Text>
												<CallForPricing>false</CallForPricing>
												<UnitMultiple>1</UnitMultiple>
												<AccChargeCode />
												<DomesticZone TableName=""RateTransportZones"" />
											</RateLineItems>
										</RateLineItemsCollection>
										<Currency TableName=""RefCurrency"">
											<Code>AUD</Code>
											<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
										</Currency>
										<AccChargeCode>
											<Code>FRT</Code>
											<PK>8319278c-e149-4895-bc52-114e69e069d9</PK>
											<GlbCompany>
												<Code>EDI</Code>
												<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
											</GlbCompany>
										</AccChargeCode>
										<ProductNumber TableName=""OrgSupplierPart"" />
									</RateLines>
								</RateLinesCollection>
								<Currency TableName=""RefCurrency"">
									<Code>AUD</Code>
									<PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
								</Currency>
								<Supplier TableName=""OrgHeader"" />
								<TransportProvider TableName=""OrgHeader"" />
								<Consignor TableName=""OrgHeader"">
									<Code>AASDRA</Code>
									<PK>935333a9-ced8-4af5-845d-65150e5c5346</PK>
								</Consignor>
								<Consignee TableName=""OrgHeader"" />
								<CartagePickupAddressOverride TableName=""OrgAddress"" />
								<CartageDeliveryAddressOverride TableName=""OrgAddress"" />
								<ServiceLevel_NI TableName=""RefServiceLevel"" />
								<ViaLRC />
								<AgentOverride TableName=""OrgHeader"" />
								<Warehouse TableName=""WhsWarehouse"" />
								<RefContainer />
								<CarrierServiceLevel TableName=""OrgCarrierServiceLevel"" />
								<CommodityCode TableName=""RefCommodityCode"" />
								<OriginZone TableName=""RateTransportZones"" />
								<DestinationZone TableName=""RateTransportZones"" />
								<Publisher TableName=""GlbCompany"">
									<Code>EDI</Code>
									<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
								</Publisher>
							</RateEntry>
						</RateEntryCollection>
						<FirstSignatory TableName=""GlbStaff"" />
						<SecondSignatory TableName=""GlbStaff"" />
						<OrgHeader>
							<Code>AASDRA</Code>=
						</OrgHeader>
						<GlbCompany>
							<Code>EDI</Code>
							<PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
						</GlbCompany>
					</RatingHeader>
				</Rate>
			</Body>
		</Native>";

		#endregion

		string CreateMessageTextWithRecipientRole(RatingHeader originalRatingHeader, string recipientRoleCode)
		{
			if (originalRatingHeader.IsGlobal())
			{
				var globalFRTRate = Factory.NewWithValidTestData<AccChargeCode>();
				globalFRTRate.AC_GC = ZGuid.Empty;
				globalFRTRate.AC_Code = "FRT";
				globalFRTRate.AC_Desc = "Global FRT";
				globalFRTRate.AC_ChargeType = "MRG";
				var globalOCARTRate = Factory.NewWithValidTestData<AccChargeCode>();
				globalOCARTRate.AC_GC = ZGuid.Empty;
				globalOCARTRate.AC_Code = "OCART";
				globalOCARTRate.AC_Desc = "Global OCART";
				globalOCARTRate.AC_ChargeType = "MRG";
				Factory.Save();
			}

			var rateEntry1 = originalRatingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
			var rateLine1 = rateEntry1.AddRateLine("OCART", UnitCalculator.Code, "CN");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 45m;

			var rateEntry2 = originalRatingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine("FRT", UnitCalculator.Code, "CN");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 205m;
			rateLine2.TL_Condition = RateLineConditions.UserDefined;
			rateLine2.TL_ConditionalExpression = "MOD=SEA";

			Factory.Save();

			#region DataContext

			var rateType = originalRatingHeader.IsClientRate()
				? nameof(DataContextType.ClientRate)
				: originalRatingHeader.IsQuote()
					? nameof(DataContextType.Quotation)
					: originalRatingHeader.IsTariff()
						? nameof(DataContextType.GlobalRate)
						: "";

			var recipientRoleText = $@"
	<nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
		<DataSourceCollection>
			<DataSource>
				<Type>{rateType}</Type>
				<Key>24181eea-d3e5-4afe-8892-8684ab555879~878d7aca-ffc3-49fc-9710-969ca0c0f2ac~{originalRatingHeader.TH_RateType}</Key>
			</DataSource>
		</DataSourceCollection>
		<ActionPurpose>
			<Code>EVT</Code>
			<Description>Event</Description>
		</ActionPurpose>
		<Company>
			<Code>EDI</Code>
		</Company>
		<EnterpriseID>EDI</EnterpriseID>
		<EventType>
			<Code>EDT</Code>
			<Description>Edited a record</Description>
		</EventType>
		<EventUser>
			<Code>E</Code>
			<Name>CargoWise Support</Name>
		</EventUser>
		<EventBranch>
			<Code>BNE</Code>
		</EventBranch>
		<EventDepartment>
			<Code>BRN</Code>
		</EventDepartment>
		<ServerID>DAT</ServerID>
		<TriggerCount>14</TriggerCount>
		<TriggerDate>2017-11-27T13:17:00+11:00</TriggerDate>
		<TriggerDescription>Send</TriggerDescription>
		<TriggerType>Trigger</TriggerType>
		<RecipientRoleCollection>
			<RecipientRole>
				<Code>{recipientRoleCode}</Code>
			</RecipientRole>
		</RecipientRoleCollection>
	</nv:DataContext>
";

			#endregion

			var messageText = ExportXMLToString(originalRatingHeader);
			messageText = messageText.Replace("</EnableCodeMapping>", "</EnableCodeMapping>" + recipientRoleText);

			return messageText;
		}

		TestErrorLogger ProcessNativeDataMessage(string messageText)
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_MessageText = messageText;
			message.EM_GB = NewBranch.PK;

			var logger = new TestErrorLogger();
			var processor = new NativeDataMessageProcessor(logger);
			processor.Process(message);

			return logger;
		}

		void AssertQuoteInserted(TestErrorLogger logger, string extraLogMessage = "")
		{
			var expectedLogMessage = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 2 inserts, 0 updates, 0 deletes
Information - RateLines - 2 inserts, 0 updates, 0 deletes
Information - RateLineItems - 2 inserts, 0 updates, 0 deletes
Information - JobDocAddress - 1 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertRatingHeaderInsertedCore(logger, expectedLogMessage, extraLogMessage);
		}

		void AssertNonQuoteInserted(TestErrorLogger logger, string extraLogMessage = "")
		{
			var expectedLogMessage = $@"Information - GlbCompany set by TargetCompanyPK ({NewBranch.GB_GC}) from ediMessage.
Information - RatingHeader - 1 inserts, 0 updates, 0 deletes
Information - RateEntry - 2 inserts, 0 updates, 0 deletes
Information - RateLines - 2 inserts, 0 updates, 0 deletes
Information - RateLineItems - 2 inserts, 0 updates, 0 deletes
Information - --------------------------------------------------------------------------------
Information - Imported: Rate";

			AssertRatingHeaderInsertedCore(logger, expectedLogMessage, extraLogMessage);
		}

		void AssertRatingHeaderInsertedCore(TestErrorLogger logger, string expectedLogMessage, string extraLogMessage = "")
		{
			AssertContainsExactLinesInAnyOrder(expectedLogMessage + extraLogMessage, logger.Logs.Trim());

			var ratingHeaders = new BusinessObjectFactory().Load<RatingHeader>(new ZQuery());
			var ediRatingHeader = ratingHeaders.Single(r => r.Company.GC_Code == "EDI");
			var newRatingHeader = ratingHeaders.Single(r => r.Company.GC_Code == "NEW");
			AssertEquals("Should not have updated the existing rate", 2, ratingHeaders.Length);
			AssertNotEquals("Should be different", ediRatingHeader.PK, newRatingHeader.PK);
			AssertRateLinesExist(ediRatingHeader);
			AssertRateLinesExist(newRatingHeader);
		}

		static void AssertRateLinesExist(RatingHeader ratingHeader)
		{
			CombineAssertions(() =>
			{
				var orgRateLine = ratingHeader.ORGRateEntriesForBinding.Cast<RateEntry>().Single().RateLines.Cast<RateLine>().Single();
				AssertNotNull(orgRateLine);
				AssertEquals("CN", orgRateLine.TL_WeightVolume);
				AssertEquals("UNT", orgRateLine.TL_RateCalculator);
				AssertEquals("OCART", orgRateLine.ChargeCode.AC_Code);

				var lclRateLine = ratingHeader.LCLRateEntriesForBinding.Cast<RateEntry>().Single().RateLines.Cast<RateLine>().Single();
				AssertNotNull(lclRateLine);
				AssertEquals("CN", lclRateLine.TL_WeightVolume);
				AssertEquals("UNT", lclRateLine.TL_RateCalculator);
				AssertEquals("FRT", lclRateLine.ChargeCode.AC_Code);
				AssertEquals("USR", lclRateLine.TL_Condition);
				AssertEquals("MOD=SEA", lclRateLine.TL_ConditionalExpression);
			});
		}

		static string ExportXMLToString(IBusiness obj)
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer() { Converter = converter };
			var exporter = new NativeXmlExportService() { Serializer = serializer };
			var tempFile = TempForTest.GetTempFileName();

			using (var stream = new FileStream(tempFile, FileMode.Open))
			{
				exporter.Export(new[] { obj }, stream);
			}
			var result = File.ReadAllText(tempFile);
			File.Delete(tempFile);

			return result;
		}

		GlbBranch newBranch;
		GlbBranch NewBranch
		{
			get
			{
				if (newBranch == null)
				{
					var newFactory = new BusinessObjectFactory();
					var company = newFactory.NewWithValidTestData<GlbCompany>();
					company.GC_Code = "NEW";
					newBranch = company.Branches.AddNew();
					newBranch.GB_Code = "NEW";
					newFactory.Save();
				}

				return newBranch;
			}
		}

		#endregion
	}
}
