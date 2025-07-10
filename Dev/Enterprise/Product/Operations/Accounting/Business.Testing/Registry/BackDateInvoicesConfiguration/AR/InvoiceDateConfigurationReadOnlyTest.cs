using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public abstract class InvoiceDateConfigurationReadOnlyTest : TestCaseWithFactory
	{
		public void TestDirectionReadOnly()
		{
			BizObj.DirectionCode = "ALL";
			BizObj.JobType = "BRK";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is not Empty", true, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is not Empty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "CLL";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is not Empty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "CSH";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is not Empty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "FCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is Empty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "GCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is Empty", false, BizObj.DirectionCode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
				AssertEquals("DirectionInfo is not Empty", true, BizObj.DirectionCode.IsEmpty);
			}

			BizObj.JobType = "";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is not Empty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "AWB";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo is Empty", true, BizObj.DirectionCode.IsEmpty);
		}

		public void TestModeReadOnly()
		{
			BizObj.Mode = "AIR";
			BizObj.JobType = "BRK";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is Empty", true, BizObj.Mode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "CLL";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "CSH";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "FCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "GCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is not Empty", false, BizObj.Mode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
				AssertEquals("ModeInfo is not Empty", true, BizObj.Mode.IsEmpty);
			}

			BizObj.JobType = "AWB";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo is Empty", true, BizObj.Mode.IsEmpty);
		}

		public void TestBrokerReadOnly()
		{
			BizObj.BrokerCode = InvoiceDateConfigurationLookups.BrokerCodes.All;
			BizObj.JobType = "BRK";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is not Empty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is not Empty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			AssertEquals("BrokerInfo.ReadOnly", false, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is not Empty", false, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is Empty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			AssertEquals("BrokerInfo.ReadOnly", false, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is not Empty", false, BizObj.BrokerCode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
				AssertEquals("BrokerInfo is not Empty", true, BizObj.BrokerCode.IsEmpty);

				BizObj.JobType = "";
				AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
				AssertEquals("BrokerInfo is not Empty", true, BizObj.BrokerCode.IsEmpty);
			}

			BizObj.JobType = "AWB";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo is Empty", true, BizObj.BrokerCode.IsEmpty);
		}

		#region Implementation

		protected abstract IInvoiceDateConfiguration GetNewBizObj { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
		}

		IInvoiceDateConfiguration BizObj;

		#endregion
	}
}