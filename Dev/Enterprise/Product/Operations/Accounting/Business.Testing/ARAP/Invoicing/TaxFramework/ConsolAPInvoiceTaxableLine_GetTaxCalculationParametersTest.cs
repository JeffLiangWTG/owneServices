using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	class ConsolAPInvoiceTaxableLine_GetTaxCalculationParametersTest : ConsolRelatedTaxableLine_GetTaxCalculationParametersTest
	{
		public override void TestGetTaxCalulcationParameters_TestBranchParameterValue()
		{
			var branch1 = Creator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Creator.CreateInvoice(typeof(APInvoice), organisation: Creator.AALSHI);
				var consol = Creator.CreateConsol();
				var shipment = Creator.CreateShipment("S001", consol);
				Creator.CreateJob(shipment, createWithMutex: false);

				var consolCost = Creator.CreateConsolCost(invoice, consol, Creator.CC1, 100M);
				var branch2 = Creator.CreateBranch("BBB", GlbCompany.CurrentCompany);
				consolCost.ApportionmentCharges[0].JR_GB = branch2.PK;

				var taxableLine = ImportApportionmentsAndGetTaxacbleLine(invoice);
				var taxCalcParams = taxableLine.GetTaxCalculationParameters();
				AssertEquals("Precondition: Line Branch value", branch2.PK, invoice.Lines[0].AL_GB);
				AssertEquals("Precondition: Line Tax Branch value", ZGuid.Empty, invoice.Lines[0].AL_GB_TaxBranch);
				AssertEquals(branch1.PK, taxCalcParams.Branch.PK);

				var branch3 = Creator.CreateBranch("CCC", GlbCompany.CurrentCompany);
				invoice.AH_GB_TaxBranch = branch3.PK;
				taxableLine = ImportApportionmentsAndGetTaxacbleLine(invoice);
				taxCalcParams = taxableLine.GetTaxCalculationParameters();
				AssertEquals("Precondition: Line Branch value", branch2.PK, invoice.Lines[0].AL_GB);
				AssertEquals("Precondition: Line Tax Branch value", branch3.PK, invoice.Lines[0].AL_GB_TaxBranch);
				AssertEquals(branch3.PK, taxCalcParams.Branch.PK);
			}
		}

		public override void TestGetTaxCalulcationParameters_TestPlaceOfSupplyParameterValue()
		{
			var allPOSEnabled = GetAllPOSEnabledFixedPlaceOfSupplyConfig();

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allPOSEnabled))
			{
				var invoice = Creator.CreateInvoice(typeof(APInvoice), organisation: Creator.AALSHI);
				var consol = Creator.CreateConsol();
				var shipment = Creator.CreateShipment("S001", consol);
				Creator.CreateJob(shipment, createWithMutex: false);

				var consolCost = Creator.CreateConsolCost(invoice, consol, Creator.CC1, 100M);
				var taxableLine = ImportApportionmentsAndGetTaxacbleLine(invoice);
				var taxCalcParams = taxableLine.GetTaxCalculationParameters();
				AssertEquals("Precondition: Line Place of Supply value", ZString.Empty, invoice.Lines[0].AL_PlaceOfSupply);
				AssertNull("FixedPlaceOfSupply in Tax Caluclation Params when CostPlaceOfSupply is not set on apportionment charge", taxCalcParams.FixedPlaceOfSupply);

				consolCost.ApportionmentCharges[0].JR_CostPlaceOfSupply = "NSW";
				taxableLine = ImportApportionmentsAndGetTaxacbleLine(invoice);
				taxCalcParams = taxableLine.GetTaxCalculationParameters();
				AssertEquals("Precondition: Line Place of Supply value", "NSW", invoice.Lines[0].AL_PlaceOfSupply);
				AssertEquals("FixedPlaceOfSupply in Tax Caluclation Params when CostPlaceOfSupply is set on apportionment charge", "NSW", taxCalcParams.FixedPlaceOfSupply.Code);
			}
		}

		public ITaxableTransactionLine ImportApportionmentsAndGetTaxacbleLine(InvoicingBase invoice)
		{
			invoice.ImportAllApportionmentsFromCosting();
			AssertEquals(1, invoice.Lines.Count);
			var taxableLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoice.Lines[0]) as ITaxableTransactionLine;
			return taxableLine;
		}

		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			var invoice = Creator.CreateInvoice(typeof(APInvoice), organisation: invoiceOrg);
			var consol = Creator.CreateConsol(consolNum: "C00" + attemptNumber, transportMode: TransportModes.Air);
			var shipment = Creator.CreateShipment("S00" + attemptNumber, consol);
			Creator.CreateJob(shipment, createWithMutex: false);

			Creator.SetupConsolRelatedAPInvoice(invoice, consol, Creator.CC1, 100M);

			AssertEquals(1, invoice.Lines.Count);
			return invoice.Lines[0];
		}

		protected override ZString GetExpectedJobType()
		{
			return JobInvoicingConsumerTypes.ForwardingConsol.Code;
		}

		protected override CostSell GetExpectedCostOrSell()
		{
			return CostSell.Cost;
		}
	}
}
