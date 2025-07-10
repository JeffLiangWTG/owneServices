using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class DangerousGoodsCountryReferenceMappingTest : TestCaseWithFactory
	{
		public void TestImport_ValidCountryReference()
		{
			CreateCountryReference("9999");

			Factory.Save();

			var manager = new ImportServiceManagerForTesting();
			using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XML_DangerousGoodsCountryReferencePivotInsert_Single)))
			{
				manager.ImportService.Import(stream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: DangerousGoodsCountryReferenceMapping
--- Import Process Finished -----------------------------------------------------------
UNDGCountryReferencePivot - 1 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestImport_InvalidCountryReference()
		{
			var manager = new ImportServiceManagerForTesting();
			using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XML_DangerousGoodsCountryReferencePivotInsert_Single)))
			{
				manager.ImportService.Import(stream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Record: DangerousGoodsCountryReferenceMapping failed to Import:
Could not insert/update the UNDGCountryReferencePivot (UNDGCountryReferencePivot) as it had an invalid reference to a UNDGCountryReference (UNDGCountryReference). There is no UNDGCountryReference with the following values: [RN_NKCountry:FR][Type:ICPE][Code:9999][HasFlashPointLower:False][FlashPointLowerCentigrade:0][HasFlashPointUpper:False][FlashPointUpperCentigrade:0].
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestImport_MultiplePivots()
		{
			CreateCountryReference("1234");
			CreateCountryReference("9999");
			CreateCountryReference("4312");

			Factory.Save();

			var manager = new ImportServiceManagerForTesting();
			using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XML_DangerousGoodsCountryReferencePivotInsert_Multiple)))
			{
				manager.ImportService.Import(stream);
			}

			string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: DangerousGoodsCountryReferenceMapping
Processed: DangerousGoodsCountryReferenceMapping
Processed: DangerousGoodsCountryReferenceMapping
--- Import Process Finished -----------------------------------------------------------
UNDGCountryReferencePivot - 3 inserts, 0 updates, 0 deletes";

			var logs = manager.GetLogs();
			AssertMultilineASCIIEquals(expectedLog, logs);
		}

		public void TestImport_WithStorageInstruction()
		{
			var isAllowed = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed;
			try
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
				var (_, existingCountryReference, pivot) = CreateSubstanceWithCountryReference("1234", "A", "IAT", "9999");

				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XML_DangerousGoodsCountryReferencePivotUpdate)))
				{
					manager.ImportService.Import(stream);
				}

				string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Processed: DangerousGoodsCountryReferenceMapping
--- Import Process Finished -----------------------------------------------------------
UNDGCountryReferencePivot - 0 inserts, 1 updates, 0 deletes";

				var logs = manager.GetLogs();
				AssertMultilineASCIIEquals(expectedLog, logs);

				var newFactory = new BusinessObjectFactory();
				var updatedCountryReferencePivot = newFactory.Load<UNDGCountryReferencePivot>(pivot.PK);
				CombineAssertions("UNDG Country Reference Pivot is updated", () =>
				{
					AssertEquals(true, updatedCountryReferencePivot.DCP_TankStorageInstructionRetentionTray);
					AssertEquals("TBC", updatedCountryReferencePivot.DCP_StorageInstruction);
				});
			}
			finally
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = isAllowed;
			}
		}

		public void TestImport_InvalidStorageInstruction()
		{
			var isAllowed = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed;
			try
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
				var (_, existingCountryReference, pivot) = CreateSubstanceWithCountryReference("1234", "A", "IAT", "9999");

				Factory.Save();

				var manager = new ImportServiceManagerForTesting();
				using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(XML_DangerousGoodsCountryReferencePivotUpdate_InvalidStorageInstruction)))
				{
					manager.ImportService.Import(stream);
				}

				string expectedLog = @"--- Start Import Process --------------------------------------------------------------
Record: DangerousGoodsCountryReferenceMapping failed to Import:
StorageInstruction value XXX is invalid. Only one of the following are allowed: TBC - To be confirmed by Safety Data Sheet, ACD - Acid, GAS - Gas, BAS - Base, OXS - Oxidizing, MSC - Miscellaneous Dangerous, WAT - Water Reactive, FLL - Flammable Liquid, LIT - Lithium, EXP - Explosive, RAD - Radioactive, ORG - Organic peroxide, FLS - Flammable solid, TOX - Toxic
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.";

				var logs = manager.GetLogs();
				AssertMultilineASCIIEquals(expectedLog, logs);

				var newFactory = new BusinessObjectFactory();
				var updatedCountryReferencePivot = newFactory.Load<UNDGCountryReferencePivot>(pivot.PK);
				CombineAssertions("UNDG Country Reference is updated", () =>
				{
					AssertEquals(false, updatedCountryReferencePivot.DCP_TankStorageInstructionRetentionTray);
					AssertEquals("", updatedCountryReferencePivot.DCP_StorageInstruction);
				});
			}
			finally
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = isAllowed;
			}
		}

		public void TestExport()
		{
			var (_, _, pivot) = CreateSubstanceWithCountryReference("1234", "A", "IAT", "9999");
			pivot.DCP_StorageInstruction = "GAS";
			pivot.DCP_TankStorageInstructionRetentionTray = true;

			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			string pivotXML = "";
			using (var dataStream = xmlSerializer.SerializeToStream(pivot))
			using (var reader = new StreamReader(dataStream))
			{
				pivotXML = reader.ReadToEnd();
			}

			AssertNotNullOrEmpty("DangerousGoodsCountryReferenceMapping xml was generated.", pivotXML);
			AssertContains("Pivot details are correct.", "<UNDGCountryReferencePivot Action=\"MERGE\">", pivotXML);
			AssertContains("Country Reference detail is populated.", "<Code>9999</Code>", pivotXML);
			AssertContains("Pivot UNNO detail is populated.", "<UNNO>1234</UNNO>", pivotXML);
			AssertContains("Pivot Variant detail is populated.", "<Variant>A</Variant>", pivotXML);
			AssertContains("Storage Instruction is populated.", "<StorageInstruction>GAS</StorageInstruction>", pivotXML);
			AssertContains("Tank Storage Instruction Retention Tray is populated.", "<TankStorageInstructionRetentionTray>true</TankStorageInstructionRetentionTray>", pivotXML);
		}

		(UNDGSubstance, UNDGCountryReference, UNDGCountryReferencePivot) CreateSubstanceWithCountryReference(string unno, string variant, string standard, string referenceCode)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1234";
			substance.DG_Variant = "A";
			substance.DG_Standard = "IAT";

			var reference = CreateCountryReference(referenceCode);
			var pivot = AttachCountryReferenceToSubstance(reference, substance);
			return (substance, reference, pivot);
		}

		UNDGCountryReference CreateCountryReference(ZString code)
		{
			var reference = Factory.New<UNDGCountryReference>();
			reference.DCR_Code = code;
			reference.DCR_Description = "Test description";
			reference.DCR_Type = "ICPE";
			reference.DCR_RN_NKCountry = "FR";

			return reference;
		}

		UNDGCountryReferencePivot AttachCountryReferenceToSubstance(UNDGCountryReference countryReference, UNDGSubstance substance)
		{
			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = countryReference.PK;
			pivot.DCP_UNNO = substance.DG_UNNO;
			pivot.DCP_Variant = substance.DG_Variant;
			pivot.DCP_Standard = substance.DG_Standard;
			return pivot;
		}

		const string XML_DangerousGoodsCountryReferencePivotInsert_Single = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""MERGE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>9999</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
  </Body>
</Native>";

		const string XML_DangerousGoodsCountryReferencePivotInsert_Multiple = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""MERGE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>1234</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""MERGE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>9999</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""MERGE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>4312</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
  </Body>
</Native>";

		const string XML_DangerousGoodsCountryReferencePivotUpdate = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""UPDATE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <TankStorageInstructionRetentionTray>true</TankStorageInstructionRetentionTray>
        <StorageInstruction>TBC</StorageInstruction>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>9999</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
  </Body>
</Native>";

		const string XML_DangerousGoodsCountryReferencePivotUpdate_InvalidStorageInstruction = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Body>
    <DangerousGoodsCountryReferenceMapping version=""2.0"">
      <UNDGCountryReferencePivot Action=""UPDATE"">
        <UNNO>1234</UNNO>
        <Variant>A</Variant>
        <Standard>IAT</Standard>
        <TankStorageInstructionRetentionTray>true</TankStorageInstructionRetentionTray>
        <StorageInstruction>XXX</StorageInstruction>
        <UNDGCountryReference>
          <Type>ICPE</Type>
          <Code>9999</Code>
          <HasFlashPointLower>false</HasFlashPointLower>
          <FlashPointLowerCentigrade>0.0</FlashPointLowerCentigrade>
          <HasFlashPointUpper>false</HasFlashPointUpper>
          <FlashPointUpperCentigrade>0.0</FlashPointUpperCentigrade>
          <Country TableName=""RefCountry"">
            <Code>FR</Code>
          </Country>
        </UNDGCountryReference>
      </UNDGCountryReferencePivot>
    </DangerousGoodsCountryReferenceMapping>
  </Body>
</Native>";
	}
}
