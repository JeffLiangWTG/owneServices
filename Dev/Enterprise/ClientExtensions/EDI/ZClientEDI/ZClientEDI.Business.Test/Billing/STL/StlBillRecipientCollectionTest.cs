using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBillCollection))]
	internal sealed class StlBillRecipientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlBillCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#region Implementation

		protected override StlBillCollection GetCollectionToTest()
		{
			return new StlBillCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlBill(Factory, GlbBranch.CurrentBranch, Factory.New<EDIOrgHeader>(), "", new ZDateTime(2015, 7, 1), ZDateTime.Today);
		}

		#endregion
	}
}
