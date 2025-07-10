using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	public class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().JI_AddInfoInfo);

		public void TestZG_CountryOfSupplyMaxLength() => AssertEquals(2, ((AddInfoJobComInvoiceLine)GetNewBusinessObject()).ZG_CountryOfSupplyInfo.MaxLength);

		public void TestMultipleKeysToUse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var addInfo = (AddInfoJobComInvoiceLine)((IAddInfoManager)invoiceLine).AddInfo;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - Export", new[] { JobDeclaration.CaptionKeyExportUCC6 }, addInfo.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - Import", new[] { JobDeclaration.CaptionKeyImportUCC6 }, addInfo.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - Import", new[] { JobDeclaration.CaptionKeyUCC }, addInfo.MultipleKeysToUse);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertSequencesEqual("MultipleKeysToUse - Non-UCC6 - Export", new[] { JobDeclaration.CaptionKeySAD }, addInfo.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertSequencesEqual("MultipleKeysToUse - Non-UCC6 - Import", new[] { JobDeclaration.CaptionKeySAD }, addInfo.MultipleKeysToUse);
				}
			});
		}

		public void TestZG_RelatedIndicator2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_RelatedIndicator2 does not exist.", null, invoiceLine.GetType().GetProperty("JI_RelatedIndicator2"));

			invoiceLine.RelatedIndicator2 = true;
			AssertEquals("ZG_RelatedIndicator2 has the added string Value", ValuationIndicatorCodeList.Codes.Yes, invoiceLine.ZG_RelatedIndicator2);

			invoiceLine.RelatedIndicator2 = false;
			AssertEquals("ZG_RelatedIndicator2 has a new string Value", ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader, invoiceLine.ZG_RelatedIndicator2);
		}

		public void TestZG_RelatedIndicator3()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_RelatedIndicator3 does not exist.", null, invoiceLine.GetType().GetProperty("JI_RelatedIndicator3"));

			invoiceLine.RelatedIndicator3 = true;
			AssertEquals("ZG_RelatedIndicator3 has the added string Value", ValuationIndicatorCodeList.Codes.Yes, invoiceLine.ZG_RelatedIndicator3);

			invoiceLine.RelatedIndicator3 = false;
			AssertEquals("ZG_RelatedIndicator3 has a new string Value", ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader, invoiceLine.ZG_RelatedIndicator3);
		}

		public void TestZG_RelatedIndicator4()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_RelatedIndicator4 does not exist.", null, invoiceLine.GetType().GetProperty("JI_RelatedIndicator4"));

			invoiceLine.RelatedIndicator4 = true;
			AssertEquals("ZG_RelatedIndicator4 has the added string Value", ValuationIndicatorCodeList.Codes.Yes, invoiceLine.ZG_RelatedIndicator4);

			invoiceLine.RelatedIndicator4 = false;
			AssertEquals("ZG_RelatedIndicator4 has a new string Value", ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader, invoiceLine.ZG_RelatedIndicator4);
		}

		public void TestZG_HadErrorInLastResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_HadErrorInLastResponse does not exist.", null, invoiceLine.GetType().GetProperty("JI_HadErrorInLastResponse"));

			invoiceLine.ZG_HadErrorInLastResponse = true;
			AssertEquals("ZG_HadErrorInLastResponse has the added boolean Value", true, invoiceLine.ZG_HadErrorInLastResponse);

			invoiceLine.ZG_HadErrorInLastResponse = false;
			AssertEquals("ZG_HadErrorInLastResponse has a new boolean Value", false, invoiceLine.ZG_HadErrorInLastResponse);
		}

		public void TestZG_RL_NKPrincipalsRepresentativeCity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_RL_NKPrincipalsRepresentativeCity does not exist.", null, invoiceLine.GetType().GetProperty("JI_RL_NKPrincipalsRepresentativeCity"));

			invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "1";
			AssertEquals("ZG_RL_NKPrincipalsRepresentativeCity has the added string Value", "1", invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity);

			invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "2";
			AssertEquals("ZG_RL_NKPrincipalsRepresentativeCity has a new added string Value", "2", invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity);
		}

		public void TestZG_PrincipalsRepresentativeName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_PrincipalsRepresentativeName does not exist.", null, invoiceLine.GetType().GetProperty("JI_PrincipalsRepresentativeName"));

			invoiceLine.ZG_PrincipalsRepresentativeName = "hello";
			AssertEquals("ZG_PrincipalsRepresentativeName has the added string Value", "hello", invoiceLine.ZG_PrincipalsRepresentativeName);

			invoiceLine.ZG_PrincipalsRepresentativeName = "world";
			AssertEquals("ZG_PrincipalsRepresentativeCity has a new added string Value", "world", invoiceLine.ZG_PrincipalsRepresentativeName);
		}

		public void TestZG_StatisticalValueManualOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_StatisticalValueManualOverride does not exist.", null, invoiceLine.GetType().GetProperty("JI_StatisticalValueManualOverride"));

			invoiceLine.ZG_StatisticalValueManualOverride = true;
			AssertEquals("ZG_StatisticalValueManualOverride has the added boolean Value", true, invoiceLine.ZG_StatisticalValueManualOverride);

			invoiceLine.ZG_StatisticalValueManualOverride = false;
			AssertEquals("ZG_StatisticalValueManualOverride has a new boolean Value", false, invoiceLine.ZG_StatisticalValueManualOverride);
		}

		public void TestZG_StatisticalValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("Invalid AddInfo property JI_StatisticalValue does not exist.", null, invoiceLine.GetType().GetProperty("JI_StatisticalValue"));

			invoiceLine.ZG_StatisticalValueManualOverride = true;
			invoiceLine.ZG_StatisticalValue = 1;
			AssertEquals("ZG_StatisticalValue has the added string Value", decimal.Parse("1"), invoiceLine.ZG_StatisticalValue);

			invoiceLine.ZG_StatisticalValue = 2;
			AssertEquals("ZG_StatisticalValue has a new added string Value", decimal.Parse("2"), invoiceLine.ZG_StatisticalValue);
		}

		public void TestZG_TransNature()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertType<ZString>(invoiceLine.ZG_TransNature);
			AssertEquals(2, invoiceLine.ZG_TransNatureInfo.MaxLength);
		}

		public void TestZG_RegionOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertType<ZString>(invoiceLine.ZG_RegionOfDestination);
			AssertEquals(10, invoiceLine.ZG_RegionOfDestinationInfo.MaxLength);
		}
	}
}
