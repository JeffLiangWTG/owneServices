using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	abstract class ConsolPostedTaxableLine_GetTaxCalculationParametersTest : ConsolRelatedTaxableLine_GetTaxCalculationParametersTest
	{
		protected override ZString GetExpectedJobType()
		{
			return JobInvoicingConsumerTypes.ForwardingConsol.Code;
		}

		protected override CostSell GetExpectedCostOrSell()
		{
			return CostSell.Cost;
		}

		public override void TestGetTaxCalulcationParameters_TestBranchParameterValue()
		{
			var branch1 = Creator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				int attemptNumber = 1;
				var branch2 = Creator.CreateBranch("BBB", GlbCompany.CurrentCompany);
				var line = GetInvoiceLine(attemptNumber, GetJobInvoicingPostingOption(), Creator.AALSHI, null, branch2);
				var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);

				AssertEquals("Precondition: Line Branch value", branch2.PK, line.AL_GB);
				AssertEquals("Precondition: Line Tax Branch value", ZGuid.Empty, line.AL_GB_TaxBranch);
				AssertEquals(branch1.PK, taxCalcParams.Branch.PK);

				//Next iteration
				++attemptNumber;
				AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
				var branch3 = Creator.CreateBranch("CCC", GlbCompany.CurrentCompany);
				line = GetInvoiceLine(attemptNumber, GetJobInvoicingPostingOption(), Creator.AALSHI, null, branch2, branch3);
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);

				AssertEquals("Precondition: Line Branch value", branch2.PK, line.AL_GB);
				AssertEquals("Precondition: Line Tax Branch value", branch3.PK, line.AL_GB_TaxBranch);
				AssertEquals(branch3.PK, taxCalcParams.Branch.PK);
			}
		}

		public override void TestGetTaxCalulcationParameters_TestPlaceOfSupplyParameterValue()
		{
			var allPOSEnabled = GetAllPOSEnabledFixedPlaceOfSupplyConfig();

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allPOSEnabled))
			{
				int attemptNumber = 1;
				var line = GetInvoiceLine(attemptNumber, GetJobInvoicingPostingOption(), Creator.AALSHI, "");
				var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);

				AssertEquals("Precondition: Line Place of Supply value", ZString.Empty, line.AL_PlaceOfSupply);
				AssertNull("FixedPlaceOfSupply in Tax Caluclation Params when CostPlaceOfSupply is not set on apportionment charge", taxCalcParams.FixedPlaceOfSupply);

				//Next iteration
				++attemptNumber;
				line = GetInvoiceLine(attemptNumber, GetJobInvoicingPostingOption(), Creator.AALSHI, "NSW");
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);

				AssertEquals("Precondition: Line Place of Supply value", "NSW", line.AL_PlaceOfSupply);
				AssertEquals("FixedPlaceOfSupply in Tax Caluclation Params when CostPlaceOfSupply is set on apportionment charge", "NSW", taxCalcParams.FixedPlaceOfSupply.Code);
			}
		}

		protected InvoicingLineBase GetInvoiceLine(int attemptNumber, JobInvoicingPostingOption option, OrgHeader invoiceOrg, ZString? costPlaceOfSupply, GlbBranch linebranch = null, GlbBranch lineTaxbranch = null)
		{
			var invoiceFromBillingTab = CreateInvoiceForBillingTab(attemptNumber, option, invoiceOrg, costPlaceOfSupply, linebranch, lineTaxbranch);
			return GetLineFromInvoice(invoiceFromBillingTab);
		}

		InvoicingBase CreateInvoiceForBillingTab(int attemptNumber
			, JobInvoicingPostingOption option
			, OrgHeader senderOrg
			, ZString? costPlaceOfSupply
			, GlbBranch linebranch = null
			, GlbBranch lineTaxbranch = null
			, string origin = "USLGB"
			, string destination = "AUSYD")
		{
			var invoiceDate = ZDateTime.Now.AddDays(3);
			var invoiceNum = "abc123" + attemptNumber.ToString();
			var consolNum = "C001001" + attemptNumber.ToString();
			var shipmentNum = "S001" + attemptNumber.ToString();

			ForwardingConsol consol = CreateConsol(origin, destination, consolNum, senderOrg);
			var shipment = Creator.CreateShipment(shipmentNum, origin, destination, consol);
			ApportionmentListing apportionments = new ApportionmentListing(Factory, consol);
			JobConsolCost consolCost = Creator.CreateConsolCost(consol, Creator.FRT, senderOrg, apportionments);
			SetConsolCostParameters(consolCost, 100m, invoiceNum, invoiceDate, invoiceDate);
			if (linebranch != null)
			{
				consolCost.ApportionmentCharges[0].JR_GB = linebranch.PK;
			}
			if (lineTaxbranch != null)
			{
				consolCost.E6_GB_CostTaxBranch = lineTaxbranch.PK;
			}
			Factory.Save();
			if (costPlaceOfSupply.HasValue)
			{
				consolCost.E6_PlaceOfSupply = costPlaceOfSupply ?? "";
			}

			return GetInvoiceLineFromPostConsolAsBillingTab(option, consol, apportionments);
		}

		InvoicingBase GetInvoiceLineFromPostConsolAsBillingTab(JobInvoicingPostingOption option, ForwardingConsol consol, ApportionmentListing apportionments)
		{
			var jobs = new[] { (Job)consol.Shipments[0].Job };
			var invoices = Creator.PostConsolAsBillingTab(Factory, jobs, consol, apportionments, option);
			AssertEquals("Precondition: Posted AP Invoice Count", 1, invoices.Length);
			return invoices.First();
		}

		void SetConsolCostParameters(JobConsolCost consolCost, ZDecimal oSCostAmount, ZString invoiceNum, ZDateTime invoiceDate, ZDateTime paymentDate)
		{
			consolCost.E6_OSCostAmount = oSCostAmount;
			consolCost.E6_InvoiceNum = invoiceNum;
			consolCost.E6_InvoiceDate = invoiceDate;
		}

		protected abstract InvoicingLineBase GetLineFromInvoice(InvoicingBase invoiceBase);

		protected abstract JobInvoicingPostingOption GetJobInvoicingPostingOption();

		protected abstract ForwardingConsol CreateConsol(string origin, string destination, string consolNum, OrgHeader invoiceOrg = null);
	}
}
