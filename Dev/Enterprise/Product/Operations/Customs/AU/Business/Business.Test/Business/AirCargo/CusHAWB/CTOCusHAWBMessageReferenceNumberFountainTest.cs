using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusHAWBMessageReferenceNumberFountainTest : NUnit.Framework.TestCase
	{
		[UseSnapshotProtection]
		public void TestSetMessageReference_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;

			var mawb1 = factory.New<CTOCusMAWB>();
			CTOCusHAWB hawb1 = mawb1.ChildBills.AddNew();
			ZString messageReference;

			AssertEquals("NUnit.Framework.TestCase should not be in a transaction", false, dbConnection.IsInTransaction);
			try
			{
				dbConnection.BeginTransaction();
				hawb1.PopulateSendersReferenceIfNeeded();
				messageReference = hawb1.CS_MessageReference;
				AssertStartsWith("Message reference number starts with 'M'", "M", messageReference);
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var otherFactory = new BusinessObjectFactory();
			var mawb2 = otherFactory.New<CTOCusMAWB>();
			CTOCusHAWB hawb2 = mawb2.ChildBills.AddNew();
			hawb2.Factory.Save();
			AssertEquals("MessageReference is re-allocated to next requestor", messageReference, hawb2.CS_MessageReference);

			AssertEquals("Previous requestor has not saved so still holds returned number", messageReference, hawb1.CS_MessageReference);
			hawb1.Factory.Save();
			AssertNotEquals("A new number is allocated", messageReference, hawb1.CS_MessageReference);

			messageReference = hawb1.CS_MessageReference;
			hawb1.PopulateSendersReferenceIfNeeded();
			AssertEquals("Not allocated twice", messageReference, hawb1.CS_MessageReference);

			hawb1.CS_MessageReference = ZString.Empty;
			AssertEquals("HasChanges", true, ((IBusinessObjectState)hawb1).HasChangesNotIncludingChildren);
			hawb1.Factory.Save();
			AssertNotEquals("A new reference is generated", messageReference, hawb1.CS_MessageReference);
			AssertEquals("reference has a value", false, hawb1.CS_MessageReference.IsEmpty);
		}
	}
}
