using System.Collections.Generic;
using System.Reflection;
using CargoWise.Data;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(PostalAddress))]
	sealed class PostalAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new PostalAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("< PostalAddress (  Fld   )   >", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<PostalAddress(Tbl.Fld)>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<PostalAddress(Tbl.Fld)>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<PostalAddres(Tbl.Fld)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("<PostalAddress(Test)>", Report));
		}

		public void TestValidGuid()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", ValueProviderToTest.GetReplacement("<PostalAddress(Header.BillToID)>", Report));
		}

		public void TestGuidAsString()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("MURGON LEATHER CO LIMITED\n23 MACALISTER STREET\nMURGON QLD 4605", ValueProviderToTest.GetReplacement("<PostalAddress(Header.OrgID)>", Report));
		}

		public void TestCOMPANYCODE()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("EDI CUSTOMS BROKERS\n10 HUTCHESON STREET\nALBION QLD\n4010", ValueProviderToTest.GetReplacement("<PostalAddress(COMPANYCODE)>", Report));
		}

		public void TestDBNullGuid()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("", ValueProviderToTest.GetReplacement("<PostalAddress(Header.NullOrgID)>", Report));
		}

		public void TestNoExceptionThrownWhenUsedOnNonOrgHeaderObject()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertNoExceptionThrown(() => { ValueProviderToTest.GetReplacement("<PostalAddress(LoginPK)>", Report); });
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>() { typeof(PostalAddress).GetField("factory", BindingFlags.Instance | BindingFlags.NonPublic) };
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = org.MainAddress;
			mainAddress.Address1 = "Unit 3, 72 O'Riordan Street";
			mainAddress.Address2 = "Alexandria NSW";
			mainAddress.City = "SYDNEY";
			mainAddress.Postcode = "2015";
			mainAddress.StateCode = "NSW";
			Factory.Save();

			Db.Connection.ExecuteNonQuery($"update {TestData.HeaderTempTableName} set BillToID = '{org.PK.ToString()}' ");
		}
	}
}
