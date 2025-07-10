using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusPartShipMessageReferenceNumberFountainTest : NUnit.Framework.TestCase
	{
		[UseSnapshotProtection]
		public void TestSetMessageReference_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;

			var mawb1 = factory.New<CTOCusMAWB>();
			CTOCusHAWB hawb1 = mawb1.ChildBills.AddNew();
			CusPartShip partShip1 = hawb1.PartShips.AddNew();
			ZString messageReference;

			AssertEquals("NUnit.Framework.TestCase should not be in a transaction", false, dbConnection.IsInTransaction);
			try
			{
				dbConnection.BeginTransaction();
				partShip1.SetMessageReference();
				messageReference = partShip1.CG_MessageReference;
				AssertStartsWith("Message reference number is '0'", "0", messageReference);
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var otherFactory = new BusinessObjectFactory();
			var mawb2 = otherFactory.New<CTOCusMAWB>();
			CTOCusHAWB hawb2 = mawb2.ChildBills.AddNew();
			CusPartShip partShip2 = hawb2.PartShips.AddNew();
			partShip2.Factory.Save();
			AssertEquals("MessageReference is re-allocated to next requestor", messageReference, partShip2.CG_MessageReference);

			AssertEquals("Previous requestor has not saved so still holds returned number", messageReference, partShip1.CG_MessageReference);
			partShip1.Factory.Save();
			AssertNotEquals("A new number is allocated", messageReference, partShip1.CG_MessageReference);

			messageReference = partShip1.CG_MessageReference;
			partShip1.SetMessageReference();
			AssertEquals("Not allocated twice", messageReference, partShip1.CG_MessageReference);
		}
	}
}
