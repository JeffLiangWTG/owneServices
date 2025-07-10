using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CargoAndUnderbondCusStatus))]
	public class CargoAndUnderbondCusStatusTest : CalculatedCusStatusTest
	{
		public override void TestStatusList()
		{
			AssertNotNull("nullness", Status.StatusList);

			Status.Code = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			AssertEquals("description matches set code", CMRConsolidatedCargoStatuses.Descriptions.AcsseizedCargoIsSeizedByCustoms, Status.Description);
			AssertEquals("status on dummy matches set code", CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, Dummy.Status);

			Status.Code = CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived;
			AssertEquals("description matches set code", CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived, Status.Description);
			AssertEquals("status on dummy matches set code", CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived, Dummy.Status);
		}

		public void TestStatusListIsCached()
		{
			Assert("StatusList shoudl be cached", ReferenceEquals(CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(Factory), Status.StatusList));
		}

		#region Implementation

		#region Properties

		CargoAndUnderbondCusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new CargoAndUnderbondCusStatus(Dummy.StatusInfo, DummyCalculator);
				}
				return fStatus;
			}
		}
		CargoAndUnderbondCusStatus fStatus;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion
	}
}
