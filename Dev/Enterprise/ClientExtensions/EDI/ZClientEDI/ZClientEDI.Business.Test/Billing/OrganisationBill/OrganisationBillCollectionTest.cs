using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OrganisationBillCollection))]
	internal sealed class OrganisationBillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrganisationBillCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override OrganisationBillCollection GetCollectionToTest()
		{
			return new OrganisationBillCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrganisationBill(Factory, Env.CurrentBranch.PK, Factory.New<OrgHeader>().PK, "", ZDateTime.Now);
		}

		#endregion
	}
}
