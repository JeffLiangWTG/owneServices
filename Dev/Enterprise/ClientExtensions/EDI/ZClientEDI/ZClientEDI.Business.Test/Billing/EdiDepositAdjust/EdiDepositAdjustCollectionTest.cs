using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiDepositAdjustCollection))]
	public class EdiDepositAdjustCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiDepositAdjustCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			var item = Collection.AddNew();
			AssertEquals("Master", Master.PK, item.DEA_OH);

			var list2 = new EdiDepositAdjustCollection(Master, "ODPL");
			item = list2.AddNew();
			AssertEquals("Master", Master.PK, item.DEA_OH);
			AssertEquals("ChargeCode", "ODPL", item.DEA_ChargeCode);
		}

		#region Implementation

		EDIOrgHeader Master;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiDepositAdjustCollection);
		}

		protected override EdiDepositAdjustCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<EDIOrgHeader>();
			return new EdiDepositAdjustCollection(Master);
		}

		#endregion
	}
}
