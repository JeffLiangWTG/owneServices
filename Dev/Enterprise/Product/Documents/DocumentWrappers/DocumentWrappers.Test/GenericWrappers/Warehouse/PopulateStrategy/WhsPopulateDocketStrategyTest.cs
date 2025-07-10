using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class WhsPopulateDocketStrategyTest<TPopulateStrategy, TBusinessObject> : WhsPopulateStrategyTest<TPopulateStrategy, TBusinessObject>
			where TBusinessObject : WhsDocket
			where TPopulateStrategy : WhsPopulateDocketStrategy
	{
		#region Test Properties

		#region TestJobNumber

		public void TestJobNumber()
		{
			AssertEquals(WrappedBO.WD_DocketID, Strategy.JobNumber);

			WrappedBO.WD_DocketID = "W00023123";
			AssertEquals("W00023123", Strategy.JobNumber);
		}

		#endregion

		#region TestJobNumberHeading

		public void TestJobNumberHeading()
		{
			AssertEquals("Job Number", Strategy.JobNumberHeading);
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading()
		{
			if (WrappedBO.WD_DocketSubType.IsEmpty)
			{
				AssertEquals(WrappedBO.Description + " Details", Strategy.SecondaryHeading);
			}
			else
			{
				AssertEquals(WrappedBO.SubTypeDesc + " Details", Strategy.SecondaryHeading);

				WrappedBO.WD_DocketSubType = "";
				AssertEquals(WrappedBO.Description + " Details", Strategy.SecondaryHeading);
			}
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			AssertEquals("", Strategy.ServiceLevel.Code);

			WrappedBO.WD_PL_NKCarrierServiceLevel = "D2D";
			AssertEquals("D2D", Strategy.ServiceLevel.Code);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			AssertEquals("Transport Ref", Strategy.TransportReference.Label);
			AssertEquals("", Strategy.TransportReference.Value);

			WrappedBO.WD_TransportReference = "REF0234";
			AssertEquals("Transport Ref", Strategy.TransportReference.Label);
			AssertEquals("REF0234", Strategy.TransportReference.Value);
		}

		#endregion

		#endregion
	}
}
