using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class LegacyNativeNamespaceTest : TestCaseWithFactory
	{
		public void TestNativeNamespaceIsUsedOnExport() //WhenLegacyRegistryItemIsTurnedOn()
		{
			ReferenceDataXMLForSerialize.ResetInstanceForTesting();
			try
			{
				TestUtil.AddDummyBizoToGlobalDefinitions();
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Code = "FOO";
				dummy.Z0_Description = "Wallah wallah wallah";
				Factory.Save();

				var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
				var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
				var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
				using (var dataStream = xmlSerializer.SerializeToStream(dummy))
				using (var reader = new StreamReader(dataStream))
				{
					string actualMessage = reader.ReadToEnd();

					CombineAssertions(delegate
					{
						AssertStartsWith("Serialized DummyBusinessObject", string.Format(@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy version=""{0}"">
      <DummyBizo Action=""MERGE"">
        <PK>".Trim(), NativeXmlInfo.Version_2011_11), actualMessage);
							AssertEndsWith("Serialized DummyBusinessObject", @"
	     </DummyBizo>
    </Dummy>
  </Body>
</Native>
".Trim(), actualMessage);
					});
				}
			}
			finally
			{
				ReferenceDataXMLForSerialize.ResetInstanceForTesting();
			}
		}

		public void TestNative_RootElementCanBeImported()
		{
			const string sourceXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native"">
    <OwnerCode>BENGOVSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>BENGOVSYD</Code>
        <FullName>Ben Givett</FullName>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUMEL</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			var session = new AncillaryImportServices();
			var handler = new ImportHandler(session);

			using (var inputStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(sourceXML)))
			{
				handler.Import(inputStream);
			}

			var loggerInfos = new List<string>(((MemoryLogger)session.Logger).Buffer.Infos);
			AssertEquals("Log Text", @"
OrgHeader - 1 inserts, 0 updates, 0 deletes
".Trim(), string.Join("\r\n", loggerInfos.ToArray()));
		}
	}
}
