using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBPartShipConsignmentReferenceNumberFountainTest : TestCase
	{
		[TestDate(2013, 02, 01, 01, 01, 01)]
		[UseSnapshotProtection]
		public void TestSetPartShipConsignmentReference_NoDuplicateReferenceException()
		{
			using (AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var factory = new BusinessObjectFactory();
				var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;

				var mawb1 = factory.New<CusMAWB>();
				CusHAWB hawb1 = mawb1.ChildBills.AddNew();
				hawb1.CS_HAWB = "HB001";
				ZString reference;

				AssertEquals("NUnit.Framework.TestCase should not be in a transaction", false, dbConnection.IsInTransaction);
				try
				{
					dbConnection.BeginTransaction();
					hawb1.SetConsignmentReferenceIfNecessary();
					reference = hawb1.CS_fPartShipConsignmentReference;
					AssertStartsWith("Reference is allocated", "EDI20130201", reference);
				}
				finally
				{
					dbConnection.RollbackTransaction();
				}

				var otherFactory = new BusinessObjectFactory();
				var mawb2 = otherFactory.New<CusMAWB>();
				CusHAWB hawb2 = mawb2.ChildBills.AddNew();
				hawb2.CS_HAWB = "HB002";
				hawb2.Factory.Save();
				AssertEquals("Reference is re-allocated to next requestor", reference, hawb2.CS_fPartShipConsignmentReference);

				AssertEquals("Previous requestor has not saved so still holds returned number", reference, hawb1.CS_fPartShipConsignmentReference);
				hawb1.Factory.Save();
				AssertNotEquals("A new number is allocated", reference, hawb1.CS_fPartShipConsignmentReference);

				reference = hawb1.CS_fPartShipConsignmentReference;
				hawb1.SetConsignmentReferenceIfNecessary();
				AssertEquals("Not allocated twice", reference, hawb1.CS_fPartShipConsignmentReference);

				hawb1.CS_fPartShipConsignmentReference = ZString.Empty;
				hawb1.CS_RL_NKOrigin = "NZAKL";  // hawb needs a local change to trigger OnSaving.  Detects reference is empty and re-populates.
				AssertEquals("HasChanges", true, ((IBusinessObjectState)hawb1).HasChangesNotIncludingChildren);
				hawb1.Factory.Save();
				AssertNotEquals("A new reference is generated", reference, hawb1.CS_fPartShipConsignmentReference);
				AssertEquals("reference has a value", false, hawb1.CS_fPartShipConsignmentReference.IsEmpty);
			}
		}
	}
}
