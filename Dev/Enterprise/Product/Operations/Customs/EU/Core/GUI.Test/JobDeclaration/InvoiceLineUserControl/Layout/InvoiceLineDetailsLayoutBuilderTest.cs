using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>))]
	class InvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>, JobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		public void TestCusNumberCodeFindBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var layout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					AssertEquals("Export and IsUCC6", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, invoiceLine));

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					AssertEquals("Import and IsUCC6", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, invoiceLine));
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					AssertEquals("Export and not IsUCC6", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, invoiceLine));

					declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
					AssertEquals("Import and not IsUCC6", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, invoiceLine));
				}
			});
		}

		protected override InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine> GetColumnLayoutBuilderForTesting()
		{
			return new InvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		}

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;
	}
}
