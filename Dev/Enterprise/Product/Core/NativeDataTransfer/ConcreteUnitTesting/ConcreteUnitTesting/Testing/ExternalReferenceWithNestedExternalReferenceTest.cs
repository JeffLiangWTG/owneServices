using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class ExternalReferenceWithNestedExternalReferenceTest : TestCaseWithFactory
	{
		public void TestGenerateSchemaWithOrgAddressReferenceWithDefinedExternalReference()
		{
			SetupTable_HasExternalReference();

			var locator = new DefinitionAssemblyLoader(new string[] { this.GetType().Assembly.FullName });
			var definitionData = locator.Load("ExternalReference");
			var entitySetDefinition = new EntitySetDefinitionBuilder(definitionData).GetEntitySetDefinition();
			var xsdContent = new NativeXsdGenerator().Generate(entitySetDefinition).ToString();

			AssertMultilineASCIIEquals("Schema Generated for HasExternalReference table", ExpectedHasExternalReferenceSchema, xsdContent);
		}

		public void TestGenerateSchemaWithOrgAddressReferenceWithoutDefinedExternalReference()
		{
			SetupTable_HasExternalReference();

			var definitionData = XElement.Parse(@"
<NativeSchemaSet Name=""ExternalReference"" Root=""HasExternalReference"">
  <Entities>
    <Entity Name=""HasExternalReference""/>
  </Entities>
  <Associations>
  </Associations>
</NativeSchemaSet>
				".Trim());
			var entitySetDefinition = new EntitySetDefinitionBuilder(definitionData).GetEntitySetDefinition();
			var xsdContent = new NativeXsdGenerator().Generate(entitySetDefinition).ToString();

			AssertMultilineASCIIEquals("Schema Generated for HasExternalReference table", ExpectedHasExternalReferenceSchema, xsdContent);
		}

		#region ExpectedHasExternalReferenceSchema

		string ExpectedHasExternalReferenceSchema => string.Format(expectedHasExternalReferenceSchemaSource.Trim(), NativeXmlInfo.Version_2011_11);

		const string expectedHasExternalReferenceSchemaSource = @"<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""Native.xsd"" />
  <xs:element name=""ExternalReference"" type=""ExternalReferenceData"" />
  <xs:complexType name=""ExternalReferenceData"">
    <xs:all>
      <xs:element name=""HasExternalReference"" type=""NativeExternalReference"" />
    </xs:all>
    <xs:attribute name=""version"" type=""xs:token"" />
  </xs:complexType>
  <xs:complexType name=""NativeExternalReference"">
    <xs:all>
      <xs:element name=""PK"" minOccurs=""0"" type=""xs:string"" />
      <xs:element name=""AgentOfficeAddress"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <!-- Candidate Key: Code + OrgHeader.PK -->
            <xs:element name=""Code"" minOccurs=""0"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""25"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""PK"" minOccurs=""0"" type=""xs:string"" />
            <xs:element name=""OrgHeader"" minOccurs=""0"">
              <xs:complexType>
                <xs:all>
                  <!-- Candidate Key: Code -->
                  <xs:element name=""Code"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""12"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""PK"" minOccurs=""0"" type=""xs:string"" />
                </xs:all>
                <xs:attribute name=""Action"" type=""Action"" />
              </xs:complexType>
            </xs:element>
          </xs:all>
          <xs:attribute name=""Action"" type=""Action"" />
          <xs:attribute name=""TableName"" type=""xs:string"" />
        </xs:complexType>
      </xs:element>
    </xs:all>
    <xs:attribute name=""Action"" type=""Action"" />
  </xs:complexType>
</xs:schema>";

		#endregion

		public void TestGenerateOutputWithOrgAddressReference()
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

			SetupTable_HasExternalReference();

			var addressReferencePK = AddRow_HasExternalReference(agentAdd);

			var loader = new DefinitionAssemblyLoader(new string[] { this.GetType().Assembly.FullName });
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader) };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			using (var dataStream = xmlSerializer.SerializeToStream(null, new[] { new RowID(addressReferencePK, "HasExternalReference") }, null))
			using (var reader = new StreamReader(dataStream))
			{
				string actualMessage = reader.ReadToEnd();
				AssertMultilineASCIIEquals("XML from HasExternalReference", string.Format(@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{3}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <ExternalReference version=""{3}"">
      <HasExternalReference Action=""MERGE"">
        <PK>{0}</PK>
        <AgentOfficeAddress TableName=""OrgAddress"">
          <Code>DEHAM - 3489BLAUKASESTRAS</Code>
          <PK>{2}</PK>
          <OrgHeader>
            <Code>RATEARHAM</Code>
            <PK>{1}</PK>
          </OrgHeader>
        </AgentOfficeAddress>
      </HasExternalReference>
    </ExternalReference>
  </Body>
</Native>
						".Trim(), addressReferencePK.ToString(), agentOrg.PK.ToString(), agentAdd.PK.ToString(), NativeXmlInfo.Version_2011_11), actualMessage);
			}
		}

		public void TestImportOrgAddressReferenceWithNonMatchingPKsWillMatchByCode()
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

			SetupTable_HasExternalReference();

			var inboundXML = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <ExternalReference>
      <HasExternalReference Action=""MERGE"">
        <PK>414cde1f-9be9-43a4-b300-64b36e1b5d2e</PK>
        <AgentOfficeAddress TableName=""OrgAddress"">
          <Code>DEHAM - 3489BLAUKASESTRAS</Code>
          <PK>9a614963-17e0-41a8-9961-f2274cd710a6</PK>
          <OrgHeader>
            <Code>RATEARHAM</Code>
            <PK>e586d412-1718-44d2-a7ac-a884e95da7ce</PK>
          </OrgHeader>
        </AgentOfficeAddress>
      </HasExternalReference>
    </ExternalReference>
  </Body>
</ReferenceData>".Trim();

			var loader = new DefinitionAssemblyLoader(new string[] { this.GetType().Assembly.FullName });
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader);

			var encoding = new System.Text.UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(inboundXML)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: ExternalReference
--- Import Process Finished -----------------------------------------------------------
HasExternalReference - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());

				using (var cmd = GetDBCommand("select X9_OA_AgentOfficeAddress from HasExternalReference"))
				{
					var result = cmd.ExecuteScalar();
					AssertEquals("HasExternalReference.X9_OA_AgentOfficeAddress", agentAdd.PK, result);
				}
			}
		}

		#region Implementation

		static Guid AddRow_HasExternalReference(OrgAddress agentOfficeAddress)
		{
			var addressReferencePK = Guid.NewGuid();
			using (var cmd = GetDBCommand(string.Format(
				"insert into HasExternalReference (X9_PK, X9_OA_AgentOfficeAddress) values ('{0}', '{1}')"
				, addressReferencePK.ToString()
				, agentOfficeAddress.PK.ToString()
			)))
			{
				cmd.ExecuteNonQuery();
			}
			return addressReferencePK;
		}

		static void SetupTable_HasExternalReference()
		{
			using (var cmd = GetDBCommand(@"
CREATE TABLE HasExternalReference (X9_PK [uniqueidentifier] NOT NULL, X9_OA_AgentOfficeAddress [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_UX__X9_PK] PRIMARY KEY NONCLUSTERED ([X9_PK] ASC)
 WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
 ON [PRIMARY]
) ON [PRIMARY]"))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = GetDBCommand(@"
ALTER TABLE HasExternalReference WITH CHECK ADD CONSTRAINT [HasExternalReference_X9_OA_AgentOfficeAddress_FK2_OrgAddress_RRR_120N]
FOREIGN KEY(X9_OA_AgentOfficeAddress)
REFERENCES OrgAddress (OA_PK)"))
			{
				cmd.ExecuteNonQuery();
			}

			using (var cmd = GetDBCommand(@"
ALTER TABLE HasExternalReference CHECK CONSTRAINT [HasExternalReference_X9_OA_AgentOfficeAddress_FK2_OrgAddress_RRR_120N]")
			)
			{
				cmd.ExecuteNonQuery();
			}

			GlobalDefinition.Instance.AddToTableMappingsForTesting("HasExternalReference", "ExternalReference");
		}

		protected override void SetUp()
		{
			overridenClientHook = ClientHookLoader.Instance.OverrideClientHookForTest(new ClientHookWithHasExternalReferenceTable());
			base.SetUp();
		}

		IDisposable overridenClientHook;

		protected override void TearDown()
		{
			base.TearDown();
			overridenClientHook.Dispose();
		}

		static DbCommand GetDBCommand(string commandText)
		{
			return Db.Connection.Command(commandText);
		}

		class ClientHookWithHasExternalReferenceTable : ClientHook
		{
			public override Clients Client => Clients.None;

			public override string ClientDisplayName => "TEST with HasExternalReference Table";

			protected override ITableSchema[] GetTableSchemas()
			{
				return new ITableSchema[] { HasExternalReferenceSchema.Instance };
			}

			[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
			class HasExternalReferenceSchema : Schema, ITableSchema
			{
				public static readonly HasExternalReferenceSchema Instance = new HasExternalReferenceSchema();
				static readonly SchemaPKColumn PK = new SchemaPKColumn(Instance, Constants.PK);
				static readonly SchemaGuidColumn X9_OA_AgentOfficeAddress = new SchemaGuidColumn(Instance, Constants.X9_OA_AgentOfficeAddress, 0, DBNull.Value, IsNullable);

				[SuppressMessage("Microsoft.Performance", "CA1810", Justification = "Empty static constructor to prevent initialisation until first member access")]
				static HasExternalReferenceSchema()
				{ }

				static class Constants
				{
					public const string SqlSchemaName = "dbo";
					public const string TableName = "HasExternalReference";
					public const string DatabaseName = "";
					public const string Prefix = "X9";
					public const string PK = "X9_PK";
					public const string X9_OA_AgentOfficeAddress = "X9_OA_AgentOfficeAddress";
					public const string PkIndex = null;
				}

				static SchemaColumnCollection All => AllHolder.All;

				static class AllHolder
				{
					[SuppressMessage("Microsoft.Performance", "CA1810", Justification = "Empty static constructor to prevent initialisation until first member access")]
					static AllHolder()
					{ }

					public static readonly SchemaColumnCollection All = new SchemaColumnCollection(PK, new SchemaColumn[] { X9_OA_AgentOfficeAddress });
				}

				static SchemaColumn GetSchemaColumn(string columnName)
				{
					switch (columnName)
					{
						case Constants.PK:
							return PK;

						case Constants.X9_OA_AgentOfficeAddress:
							return X9_OA_AgentOfficeAddress;

						default:
							return null;
					}
				}

				string ITableSchema.SqlSchemaName => Constants.SqlSchemaName;
				string ITableSchema.TableName => Constants.TableName;
				string ITableSchema.PkIndexName => Constants.PkIndex;
				SchemaPKColumn ITableSchema.PK => PK;
				SchemaColumnCollection ITableSchema.All => All;

				SchemaColumn ITableSchema.GetSchemaColumn(string columnName)
				{
					return GetSchemaColumn(columnName);
				}
			}
		}

		#endregion
	}
}
