using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJZ_IncoTermPlace_OnValidUnloco()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_IncoTermPlace = "abc";
				invoice.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
				AssertEquals("AgreedPlaceCode is country", "abc", invoice.JZ_IncoTermPlace);

				invoice.ZG_AgreedPlaceCode = "BEAAA";
				AssertEquals("Invalid Unloco Code", "abc", invoice.JZ_IncoTermPlace);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					invoice.JZ_IncoTermPlace = "abc";
					invoice.ZG_AgreedPlaceCode = "BEBRU";
					AssertEquals("AgreedPlaceCode enabled - Valid Unloco Code", ZString.Empty, invoice.JZ_IncoTermPlace);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, false))
				{
					invoice.JZ_IncoTermPlace = "abc";
					invoice.ZG_AgreedPlaceCode = "BEBRU";
					AssertEquals("AgreedPlaceCode disabled - Valid Unloco Code", "abc", invoice.JZ_IncoTermPlace);
				}
			});
		}

		public void TestZG_RelatedIndicator2_RefreshBinding()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertRelatedIndictorRefreshesLines(invoice, invoice.RelatedIndicator2Info);
		}

		public void TestZG_RelatedIndicator2_ReadOnly()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			CombineAssertions(() =>
			{
				invoice.RelatedIndicator2 = false;
				AssertEquals("No Invoice Lines", false, invoice.RelatedIndicator2Info.ReadOnly);
				invoice.InvoiceLines.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.RelatedIndicator2 = true;
				AssertEquals("Invoice Line with RelatedIndictor", true, invoice.RelatedIndicator2Info.ReadOnly);
				line.RelatedIndicator2 = false;
				AssertEquals("Invoice Line with RelatedIndictor", false, invoice.RelatedIndicator2Info.ReadOnly);
				invoice.RelatedIndicator2 = true;
				AssertEquals("Invoice RelatedIndictor", false, invoice.RelatedIndicator2Info.ReadOnly);
			});
		}

		public void TestZG_RelatedIndicator3_RefreshBinding()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertRelatedIndictorRefreshesLines(invoice, invoice.RelatedIndicator3Info);
		}

		public void TestZG_RelatedIndicator3_ReadOnly()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			CombineAssertions(() =>
			{
				invoice.RelatedIndicator3 = false;
				AssertEquals("No Invoice Lines", false, invoice.RelatedIndicator3Info.ReadOnly);
				invoice.InvoiceLines.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.RelatedIndicator3 = true;
				AssertEquals("Invoice Line with RelatedIndictor", true, invoice.RelatedIndicator3Info.ReadOnly);
				line.RelatedIndicator3 = false;
				AssertEquals("Invoice Line with RelatedIndictor", false, invoice.RelatedIndicator3Info.ReadOnly);
				invoice.RelatedIndicator3 = true;
				AssertEquals("Invoice RelatedIndictor", false, invoice.RelatedIndicator3Info.ReadOnly);
			});
		}

		public void TestZG_RelatedIndicator4_RefreshBinding()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			AssertRelatedIndictorRefreshesLines(invoice, invoice.RelatedIndicator4Info);
		}

		public void TestZG_RelatedIndicator4_ReadOnly()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			CombineAssertions(() =>
			{
				invoice.RelatedIndicator4 = false;
				AssertEquals("No Invoice Lines", false, invoice.RelatedIndicator4Info.ReadOnly);
				var line = invoice.InvoiceLines.AddNew();
				invoice.InvoiceLines.AddNew();
				line.RelatedIndicator4 = true;
				AssertEquals("Invoice Line with RelatedIndictor", true, invoice.RelatedIndicator4Info.ReadOnly);
				line.RelatedIndicator4 = false;
				AssertEquals("Invoice Line with RelatedIndictor", false, invoice.RelatedIndicator4Info.ReadOnly);
				invoice.RelatedIndicator4 = true;
				AssertEquals("Invoice RelatedIndictor", false, invoice.RelatedIndicator4Info.ReadOnly);
			});
		}

		public void TestZG_AgreedPlaceCode_ResourceStringData() => AssertEntity<AddInfoJobComInvoiceHeader>()
			.HasProperty(x => x.ZG_AgreedPlaceCode)
			.WithCaption("Incoterm Place Code");

		void AssertRelatedIndictorRefreshesLines(JobComInvoiceHeader invoice, ZPropertyInfo relatedIndicatorInfo)
		{
			bool line1Refreshed = false, line2Refreshed = false;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			(invoiceLine1 as IBindingList).ListChanged += (o, e) => line1Refreshed = true;
			(invoiceLine2 as IBindingList).ListChanged += (o, e) => line2Refreshed = true;
			relatedIndicatorInfo.Value = ZBool.False;
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", true, line1Refreshed);
				AssertEquals("Line 2", true, line2Refreshed);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo);
	}
}
