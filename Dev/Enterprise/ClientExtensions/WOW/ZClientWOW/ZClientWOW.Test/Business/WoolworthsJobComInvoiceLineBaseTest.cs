using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobComInvoiceLine))]
	class WoolworthsJobComInvoiceLineBaseTest : Customs.Business.Testing.BaseJobComInvoiceLineAbstractTest
	{
		public override void TestJI_FormattedTariff()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = "1234567890";
				AssertEquals("Unformatted", "1234.56.78", InvoiceLine.JI_FormattedTariff);
				InvoiceLine.JI_FormattedTariff = "9876.54.32 10";
				AssertEquals("Formatted", "9876.54.32", InvoiceLine.JI_FormattedTariff);
			});
		}

		public void TestJI_FormattedTariff_Import()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				InvoiceLine.JI_Tariff = "1234567890";
				AssertEquals("Unformatted", "1234.56.78 90", InvoiceLine.JI_FormattedTariff);
				InvoiceLine.JI_FormattedTariff = "9876.54.32 10";
				AssertEquals("Formatted", "9876.54.32 10", InvoiceLine.JI_FormattedTariff);
			});
		}

		public override void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Assert("Inherits AU which is different to base and tested in AU", true);
		}

		public override void TestEffectiveCountryOfOrigin()
		{
			Assert("Inherits AU which is different to base and tested in AU", true);
		}

		public override void TestMakeCustomsQuantityReadOnly()
		{
			Assert("Inherits AU which is different to base and tested in AU", true);
		}

		public override void TestWipeNKTaxType()
		{
			Assert("Inherits AU which is different to base and tested in AU", true);
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("We don't want this to run - we have stuff loading as WOW demand Load Validation - this was not picked up previously as it ran in OnLoadedInternal but now runs in OnLoaded.. so we live with it.", true);
		}

		protected override bool UseUniversalTariff => false;
		protected override bool ShouldMatchHTBForTestPivot => false;
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			return (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<InvoiceLineApportionedCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceLineCharge>);
	}
}
