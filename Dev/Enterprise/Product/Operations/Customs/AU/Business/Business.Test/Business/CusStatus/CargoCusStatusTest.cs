using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CargoCusStatus))]
	public class CargoCusStatusTest : CalculatedCusStatusTest
	{
		public override void TestStatusList()
		{
			AssertNotNull("nullness", Status.StatusList);
			AssertEquals("type", typeof(CMRConsolidatedCargoStatuses), Status.StatusList.GetType());

			Status.Code = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			AssertEquals("description matches set code", CMRConsolidatedCargoStatuses.Descriptions.AcsseizedCargoIsSeizedByCustoms, Status.Description);
			AssertEquals("status on dummy matches set code", CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, Dummy.Status);
		}

		public void TestStatusListIsCached()
		{
			Assert("StatusList shoudl be cached", ReferenceEquals(Factory.GetCachedValue<CMRConsolidatedCargoStatuses>(), Status.StatusList));
		}

		#region Implementation

		#region Properties

		CargoCusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new CargoCusStatus(Dummy.StatusInfo, DummyCalculator);
				}
				return fStatus;
			}
		}
		CargoCusStatus fStatus;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion
	}
}
