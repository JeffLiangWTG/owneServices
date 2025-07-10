using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobComInvoiceLinePreviousDocumentCleanupStrategyTest : TestCaseWithFactory
{
	public void TestCleanup_WhenUcc6Export()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		previousDocument.CSI_Procedure = "PROC";
		previousDocument.CSI_SubType = "T";
		previousDocument.CSI_UnitOfQuantity3 = "KG";

		ICleanUpStrategy strategy = new JobComInvoiceLinePreviousDocumentCleanupStrategy(previousDocument);

		using (TemporarilySetUcc6Configuration(isActive: false))
		{
			strategy.CleanUp();
			CombineAssertions("When Non-UCC6 Declaration", () =>
			{
				AssertEquals("CSI_Procedure", "PROC", previousDocument.CSI_Procedure);
				AssertEquals("CSI_SubType", "T", previousDocument.CSI_SubType);
				AssertEquals("CSI_UnitOfQuantity3", "KG", previousDocument.CSI_UnitOfQuantity3);
			});
		}

		using (TemporarilySetUcc6Configuration(isActive: true))
		{
			strategy.CleanUp();
			CombineAssertions("When UCC6 Declaration", () =>
			{
				AssertEquals("CSI_Procedure", "", previousDocument.CSI_Procedure);
				AssertEquals("CSI_SubType", "", previousDocument.CSI_SubType);
				AssertEquals("CSI_UnitOfQuantity3", "", previousDocument.CSI_UnitOfQuantity3);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		previousDocument = invoiceLine.PreviousDocuments.AddNew();
	}

	IDisposable TemporarilySetUcc6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	PreviousDocument previousDocument;
}
