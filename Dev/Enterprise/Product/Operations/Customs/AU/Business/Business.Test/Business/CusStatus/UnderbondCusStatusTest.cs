using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(UnderbondCusStatus))]
	public class UnderbondCusStatusTest : CalculatedCusStatusTest
	{
		public override void TestStatusList()
		{
			AssertNotNull("nullness", Status.StatusList);
			AssertEquals("type", typeof(CMRUnderbondStatuses), Status.StatusList.GetType());

			Status.Code = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			AssertEquals("description matches set code", CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived, Status.Description);
			AssertEquals("status on dummy matches set code", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived, Dummy.Status);
		}

		public void TestStatusListIsCached()
		{
			Assert("StatusList shoudl be cached", ReferenceEquals(Factory.GetCachedValue<CMRUnderbondStatuses>(), Status.StatusList));
		}

		#region Implementation

		#region Properties

		UnderbondCusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new UnderbondCusStatus(Dummy.StatusInfo, DummyCalculator);
				}
				return fStatus;
			}
		}
		UnderbondCusStatus fStatus;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion
	}
}
