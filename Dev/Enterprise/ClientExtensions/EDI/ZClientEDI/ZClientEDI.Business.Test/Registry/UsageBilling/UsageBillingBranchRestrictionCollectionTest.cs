using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingBranchRestrictionCollection))]
	public class UsageBillingBranchRestrictionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UsageBillingBranchRestrictionCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override UsageBillingBranchRestrictionCollection GetCollectionToTest() => new UsageBillingBranchRestrictionCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new UsageBillingBranchRestriction();

		public void TestIsInvoicingBranchAllowed()
		{
			var collection = new UsageBillingBranchRestrictionCollection();
			AssertEquals(true, collection.IsInvoicingBranchAllowed("ENT", Env.CurrentBranch.PK));
			AssertEquals(true, collection.IsInvoicingBranchAllowed("ENT", ZGuid.NewZGuid()));

			var branchRestriction = collection.AddNew();
			branchRestriction.ProductCode = "ENT";
			branchRestriction.InvoicingBranch = Env.CurrentBranch.PK;

			AssertEquals(true, collection.IsInvoicingBranchAllowed("ENT", Env.CurrentBranch.PK));
			AssertEquals(false, collection.IsInvoicingBranchAllowed("ENT", ZGuid.NewZGuid()));

			AssertEquals(true, collection.IsInvoicingBranchAllowed("CW1", Env.CurrentBranch.PK));
			AssertEquals(true, collection.IsInvoicingBranchAllowed("CW1", ZGuid.NewZGuid()));
		}
	}
}
