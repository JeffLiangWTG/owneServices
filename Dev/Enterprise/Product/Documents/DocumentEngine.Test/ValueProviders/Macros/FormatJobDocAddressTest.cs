using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FormatJobDocAddress))]
	sealed class FormatJobDocAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new FormatJobDocAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<FormatJobDocAddres(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< FormatJobDocAddress (  Fld   )   >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Format  JobDocAddress(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatJobDoc Address(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatJobDocAddress(Tbl.Fld, 1==1)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatJobDocAddress(Tbl.Fld, <Value>==1)>", Passes.FirstPass));
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "MURGON LEATHER CO LIMITED"));
			var command = Db.Connection.Command(@"INSERT INTO dbo.JOBDOCADDRESS (E2_PK, E2_OA_ADDRESS, E2_PARENTID, E2_PARENTTABLECODE, E2_CompanyName, E2_Address1, E2_City, E2_Postcode, E2_State, E2_RN_NKCountryCode)"
														+ "VALUES ('" + TestData.JobDocAddressPKString + "', '" + org.MainAddress.PK + "', '" + org.MainAddress.PK + "', 'OA', 'BOB THE BUILDER', '10 WHERE STREET', 'WENDY', '2210', 'AUCKLAND', 'NZ')");
			command.ExecuteNonQuery();

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", 0.000001d));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", 0.000002F));
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			var pass = Report.Renderer != null ? Report.Renderer.CurrentPass : Passes.FirstPass;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", Report.MacroTranslator.GetValue("<FormatJobDocAddress(Header.JobDocAddressID , <TestValue1>==0.000001)>", pass));
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", Report.MacroTranslator.GetValue("<FormatJobDocAddress(Header.JobDocAddressID , <TestValue2>==0.000001)>", pass));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro_WhenCultureChanges_ShouldConvertToStringWithoutScientificNotation()
		{
			using(Culture.SetTemporarily(CultureInfo.GetCultureInfo("pt-BR")))
			{
				var factory = new BusinessObjectFactory();
				var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "MURGON LEATHER CO LIMITED"));
				var command = Db.Connection.Command(@"INSERT INTO dbo.JOBDOCADDRESS (E2_PK, E2_OA_ADDRESS, E2_PARENTID, E2_PARENTTABLECODE, E2_CompanyName, E2_Address1, E2_City, E2_Postcode, E2_State, E2_RN_NKCountryCode)"
															+ "VALUES ('" + TestData.JobDocAddressPKString + "', '" + org.MainAddress.PK + "', '" + org.MainAddress.PK + "', 'OA', 'BOB THE BUILDER', '10 WHERE STREET', 'WENDY', '2210', 'AUCKLAND', 'NZ')");
				command.ExecuteNonQuery();

				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", 765.217d));
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", 865.217F));
				PrepareRenderer();
				Report.Renderer.CurrentAreaToProcess = null;
				var pass = Report.Renderer != null ? Report.Renderer.CurrentPass : Passes.FirstPass;
				AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", Report.MacroTranslator.GetValue("<FormatJobDocAddress(Header.JobDocAddressID , <TestValue1>==765.217)>", pass));
				AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", Report.MacroTranslator.GetValue("<FormatJobDocAddress(Header.JobDocAddressID , <TestValue2>==765.217)>", pass));
				Assert(!Report.ErrorManager.HasErrors);
			}
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Test)>", Report));
		}

		public void TestGuidAsString()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "MURGON LEATHER CO LIMITED"));
			var command = Db.Connection.Command(@"INSERT INTO dbo.JOBDOCADDRESS (E2_PK, E2_OA_ADDRESS, E2_PARENTID, E2_PARENTTABLECODE, E2_CompanyName, E2_Address1, E2_City, E2_Postcode, E2_State, E2_RN_NKCountryCode)"
														+ "VALUES ('" + TestData.JobDocAddressPKString + "', '" + org.MainAddress.PK + "', '" + org.MainAddress.PK + "', 'OA', 'BOB THE BUILDER', '10 WHERE STREET', 'WENDY', '2210', 'AUCKLAND', 'NZ')");
			command.ExecuteNonQuery();

			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Header.JobDocAddressID , 1==1)>", Report));
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Header.JobDocAddressID)>", Report));

			command = Db.Connection.Command("UPDATE dbo.JOBDOCADDRESS SET E2_ADDRESSOVERRIDE = 1, E2_VALIDATIONSTATUS = 'NYV', E2_OA_ADDRESS = NULL WHERE E2_PK = '" + TestData.JobDocAddressPKString + "'");
			command.ExecuteNonQuery();
			AssertEquals("BOB THE BUILDER\n10 WHERE STREET\nWENDY 2210\nNEW ZEALAND", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Header.JobDocAddressID , 1==1)>", Report));
			AssertEquals("BOB THE BUILDER\n10 WHERE STREET\nWENDY 2210\nNEW ZEALAND", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Header.JobDocAddressID)>", Report));
		}

		public void TestDBNullGuid()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("", ValueProviderToTest.GetReplacement("<FormatJobDocAddress(Header.NullOrgID)>", Report));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Address1 = "MURGON LEATHER CO LIMITED";
			mainAddress.Address2 = "23 MACALISTER STREET";
			mainAddress.City = "MURGON";
			mainAddress.Postcode = "4605";
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = mainAddress.PK;
			jobDocAddress.E2_ParentID = mainAddress.PK;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_ParentTableCode = "OA";
			Factory.Save();
			jobDocAddress.E2_RN_NKCountryCode = "AU";
			jobDocAddress.E2_Address1 = "Unit 3, 72 O'Riordan Street";
			jobDocAddress.E2_Address2 = "Alexandria NSW";
			jobDocAddress.E2_City = "SYDNEY";
			jobDocAddress.E2_Postcode = "2015";
			jobDocAddress.E2_State = "NSW";
			Factory.Save();
			Db.Connection.ExecuteNonQuery($"update {TestData.HeaderTempTableName} set JobDocAddressID = '{jobDocAddress.PK.ToString()}' ");
			Report.Renderer.CurrentAreaToProcess = null;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportMode", "SEA"));
		}
	}
}
