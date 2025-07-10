using System.Globalization;
using CargoWise.Data;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FormatOrgAddress))]
	sealed class FormatOrgAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new FormatOrgAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<FormatOrgAddres(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< FormatOrgAddress (  Fld   )   >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Format  OrgAddress(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatOrg Address(Tbl.Fld)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatOrgAddress(Tbl.Fld, 1==1)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<FormatOrgAddress(Tbl.Fld, <Value>==1)>", Passes.FirstPass));
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", 0.000001d));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", 0.000002F));
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			var pass = Report.Renderer != null ? Report.Renderer.CurrentPass : Passes.FirstPass;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", Report.MacroTranslator.GetValue("<FormatOrgAddress(Header.OrgAddressID , <TestValue1>==0.000001)>", pass));
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", Report.MacroTranslator.GetValue("<FormatOrgAddress(Header.OrgAddressID , <TestValue2>==0.000001)>", pass));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro_WhenCultureChanges_ShouldConvertToStringWithoutScientificNotation()
		{
			using(Culture.SetTemporarily(CultureInfo.GetCultureInfo("pt-BR")))
			{
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", 765.217D));
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", 865.217F));
				PrepareRenderer();
				Report.Renderer.CurrentAreaToProcess = null;
				var pass = Report.Renderer != null ? Report.Renderer.CurrentPass : Passes.FirstPass;
				AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", Report.MacroTranslator.GetValue("<FormatOrgAddress(Header.OrgAddressID , <TestValue1>==765.217)>", pass));
				AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", Report.MacroTranslator.GetValue("<FormatOrgAddress(Header.OrgAddressID , <TestValue2>==765.217)>", pass));
				Assert(!Report.ErrorManager.HasErrors);
			}
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("<FormatOrgAddress(Test)>", Report));
		}

		public void TestGuidAsString()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", ValueProviderToTest.GetReplacement("<FormatOrgAddress(Header.OrgAddressID , 1==1)>", Report));
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605\nAUSTRALIA", ValueProviderToTest.GetReplacement("<FormatOrgAddress(Header.OrgAddressID)>", Report));
		}

		public void TestDBNullGuid()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("", ValueProviderToTest.GetReplacement("<FormatOrgAddress(Header.NullOrgID)>", Report));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.Address1 = "Unit 3, 72 O'Riordan Street";
			mainAddress.Address2 = "Alexandria NSW";
			mainAddress.City = "SYDNEY";
			mainAddress.Postcode = "2015";
			mainAddress.StateCode = "NSW";
			mainAddress.OA_RL_NKRelatedPortCode = unloco.RL_Code;
			Factory.Save();
			Db.Connection.ExecuteNonQuery($"update {TestData.HeaderTempTableName} set OrgAddressID = '{mainAddress.PK.ToString()}' ");
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TransportMode", "SEA"));
		}
	}
}
