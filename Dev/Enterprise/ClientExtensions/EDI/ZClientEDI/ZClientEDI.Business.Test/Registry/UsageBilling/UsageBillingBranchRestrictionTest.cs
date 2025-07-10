using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingBranchRestriction))]
	internal class UsageBillingBranchRestrictionTest : RegistryBusinessObjectTemplateTestCase<UsageBillingBranchRestriction>
	{
		public void TestGetClone()
		{
			var branchRestrict = NewPopulatedBusinessObject();
			branchRestrict.ProductCode = "E2E";
			branchRestrict.InvoicingBranch = Env.CurrentBranchPK;

			var clone = (UsageBillingBranchRestriction)branchRestrict.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals("E2E", clone.ProductCode);
			AssertEquals(Env.CurrentBranchPK, clone.InvoicingBranch);
		}

		public void TestValidation()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var collection = new UsageBillingBranchRestrictionCollection();
			var restriction1 = collection.AddNew();
			restriction1.RunPreSaveValidation();
			AssertHasError(restriction1.ProductCodeInfo, "Please enter a Product Code.");
			restriction1.ProductCode = "XXX";
			AssertHasError(restriction1.ProductCodeInfo, "Enter a valid Product Code.");
			restriction1.ProductCode = "ABC";
			AssertNoErrors(restriction1.ProductCodeInfo);

			restriction1.InvoicingBranch = Env.CurrentBranchPK;
			AssertNoErrors(restriction1.InvoicingBranchInfo);
			restriction1.InvoicingBranch = ZGuid.Empty;
			AssertHasError(restriction1.InvoicingBranchInfo, "Please enter an Invoicing Branch.");
			restriction1.InvoicingBranch = Env.CurrentBranchPK;

			var restriction2 = collection.AddNew();
			restriction2.ProductCode = "ABC";
			restriction2.InvoicingBranch = Env.CurrentBranchPK;
			AssertHasError(restriction2.InvoicingBranchInfo, "The Invoicing Branch must be unique.");
			restriction2.InvoicingBranch = branch1.PK;
			AssertNoErrors(restriction2.InvoicingBranchInfo);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override UsageBillingBranchRestriction GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override UsageBillingBranchRestriction GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		UsageBillingBranchRestriction NewPopulatedBusinessObject() => new UsageBillingBranchRestriction(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
