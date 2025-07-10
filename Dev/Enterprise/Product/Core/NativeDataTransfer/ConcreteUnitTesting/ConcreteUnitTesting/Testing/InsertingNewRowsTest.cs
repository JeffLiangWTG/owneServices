using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class InsertingNewRowsTest : TestCaseWithFactory
	{
		#region OrgHeaderXml

		const string OrgHeaderXml = @"<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header>
    <EnableCodeMapping>false</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""Merge"">
        <Code>6401406481</Code>
        <FullName>DSV AIR &amp; SEA LTD I352</FullName>
        <Language>EN</Language>
        <OrgAddressCollection>
          <OrgAddress Action=""Merge"">
            <PK>f6c10d59-7cdf-4ec3-a296-1197164ef761</PK>
            <Address1>UNIT 2 SWORDS BUSINESS PARK</Address1>
            <Address2>SWORDS</Address2>
            <City>DUBLIN</City>
            <PostCode>IE</PostCode>
            <State></State>
            <Phone>+18955555</Phone>
            <Mobile></Mobile>
            <Fax>+18955589</Fax>
            <Email>loretta.neary@ie.dsv.com</Email>
            <Code>[1] UNIT 2 SWORDS BUSINES</Code>
            <Language>EN</Language>           
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""Merge"">
            <ARCreditLimit>000000999999999</ARCreditLimit>
            <ARInvoiceTerms>MTH</ARInvoiceTerms>
            <ARInvoiceTermDays>20</ARInvoiceTermDays>
            <ARDisbursementInvoiceTerms>INV</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>00</ARDisbursementInvoiceTermDays>
            <GlbCompany>
              <Code>{0}</Code>
            </GlbCompany>         
          </OrgCompanyData>
        </OrgCompanyDataCollection>
      </OrgHeader>
    </Organization>
  </Body>
</ReferenceData>";

		#endregion

		public void TestImportInsertsNewEntityWhenReferenceIsNotPresentInTheDatabaseAndShouldBeInserted()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(OrgHeaderXml, "EDI"))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(OrgHeaderXml, "EDI"))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Processed: Organization
--- Import Process Finished -----------------------------------------------------------
OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		public void TestImportWithInvalidGlbCompanyFails()
		{
			DataRegistry.Instance.CanUserEditOrganisationCode = true;

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(OrgHeaderXml, "---"))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Organization failed to Import:
Could not insert/update the AR/AP Details (OrgCompanyData) as it had an invalid reference to a Company (GlbCompany). There is no Company with the following values: [Code:---].
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
				".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}

		#region DummyBOXml

		const string DummyBOXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData xmlns=""http://www.cargowise.com/Schemas/Universal"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Dummy>
      <DummyBizo Action=""MERGE"">
        <PK>{0}</PK>
        <Number>1</Number>
        <Description>FOO</Description>
        <ByteArray></ByteArray>
        <Text></Text>
        <NText></NText>
      </DummyBizo>
    </Dummy>
  </Body>
</ReferenceData>";

		#endregion

		public void TestPKMatchingOnImportHasPrecedenceOverNaturalKeys()
		{
			TestUtil.AddDummyBizoToGlobalDefinitions();
			TestUtil.Connection.ExecuteNonQuery(@"Create UNIQUE INDEX NR_UX__Z0_Number ON DummyBizo(Z0_Number)");

			var loader = new DefinitionAssemblyLoader(new[]
			{
				typeof(InsertingNewRowsTest).Assembly.FullName,
				typeof(DefinitionAssemblyLoader).Assembly.FullName
			});
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(loader);

			var dummyBO1 = Factory.New<DummyBusinessObject>();
			dummyBO1.Z0_Number = 1;

			var dummyBO2 = Factory.New<DummyBusinessObject>();
			dummyBO2.Z0_Number = 2;

			Factory.Save();

			using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(DummyBOXml, dummyBO2.PK.ToString()))))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);

				string expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Record: Dummy failed to Import:
The value of Number must be unique on DummyBizo. The duplicate value(s) are: (1).
Error occurred trying to import file. Please fix the error and try importing the file again.
--- Import Process Finished -----------------------------------------------------------
No insert/update action performed.
			".Trim();

				AssertMultilineASCIIEquals("Log Text", expectedLog, manager.GetLogs());
			}
		}
	}
}
