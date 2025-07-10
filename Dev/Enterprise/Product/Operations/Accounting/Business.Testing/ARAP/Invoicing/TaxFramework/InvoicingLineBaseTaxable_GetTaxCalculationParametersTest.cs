using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.MasterFiles.Business.AccChargeTaxOverrideMatcher;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	abstract class InvoicingLineBaseTaxable_GetTaxCalculationParametersTest : TestCaseWithFactory
	{
		public void TestGetTaxCalulcationParameters_OrganizationInTaxCalcParamsIsSameAsOnInvoice()
		{
			var line1 = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
			var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line1);
			AssertEquals(Creator.AALSHI.PK, taxCalcParams.Organisation.PK);

			var line2 = CreateInvoiceLine(attemptNumber: 2, Creator.ABIGAS);
			taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line2);
			AssertEquals(Creator.ABIGAS.PK, taxCalcParams.Organisation.PK);
		}

		public void TestGetTaxCalulcationParameters_TestSupplyTypeParameterValue()
		{
			var line = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
			line.AL_SupplyType = "DDD";
			var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
			AssertEquals("DDD", taxCalcParams.SupplyType);

			line.AL_SupplyType = ZString.Empty;
			taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
			AssertEquals(ZString.Empty, taxCalcParams.SupplyType);
		}

		public virtual void TestGetTaxCalulcationParameters_TestBranchParameterValue()
		{
			var branch1 = Creator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var line = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
				var branch2 = Creator.CreateBranch("BBB", GlbCompany.CurrentCompany);
				line.AL_GB = branch2.PK;
				AssertEquals("Precondition: ", ZGuid.Empty, line.AL_GB_TaxBranch);
				var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertEquals(branch2.PK, taxCalcParams.Branch.PK);

				var branch3 = Creator.CreateBranch("CCC", GlbCompany.CurrentCompany);
				line.AL_GB_TaxBranch = branch3.PK;
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertEquals(branch3.PK, taxCalcParams.Branch.PK);
			}
		}

		public virtual void TestGetTaxCalulcationParameters_TestPlaceOfSupplyParameterValue()
		{
			var allPOSEnabled = GetAllPOSEnabledFixedPlaceOfSupplyConfig();

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allPOSEnabled))
			{
				var line = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
				var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertNull("Place of Supply is not set on job or line", taxCalcParams.FixedPlaceOfSupply);

				line.AL_PlaceOfSupply = "NSW";
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertEquals("Place of supply is set on line", "NSW", taxCalcParams.FixedPlaceOfSupply.Code);
			}
		}

		public void TestGetTaxCalculationParameters_TestJobTypeCostSellTransportModeValues()
		{
			var line = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
			var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
			AssertEquals(GetExpectedJobType(), taxCalcParams.JobType);
			AssertEquals(GetExpectedCostOrSell(), taxCalcParams.CostOrSell);
			AssertEquals(GetExpectedTransportMode(), taxCalcParams.TransportMode);
		}

		protected TaxCalculationParameters GetTaxCalculationParametersFromInvoiceLine(InvoicingLineBase line)
		{
			var taxableLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line) as ITaxableTransactionLine;
			return taxableLine.GetTaxCalculationParameters();
		}

		protected CodeDescriptionBoolCollection GetAllPOSEnabledFixedPlaceOfSupplyConfig()
		{
			var allPOSEnabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in allPOSEnabled)
			{
				item.Bool = true;
			}

			return allPOSEnabled;
		}

		protected abstract ZString GetExpectedJobType();

		protected abstract CostSell GetExpectedCostOrSell();

		protected abstract ZString GetExpectedTransportMode();

		protected abstract InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg);

		protected TestObjectCreator Creator => (creator = creator ?? new TestObjectCreator(Factory));

		TestObjectCreator creator;
	}
}
