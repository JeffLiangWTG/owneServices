using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(MessageCusStatus))]
	public class MessageCusStatusTest : CalculatedCusStatusTest
	{
		public override void TestStatusList()
		{
			AssertNotNull("nullness", Status.StatusList);
			AssertEquals("type", typeof(CMRBaseStatuses), Status.StatusList.GetType());

			Status.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			AssertEquals("description matches set code", CMRBaseStatuses.Descriptions.AmendmentAccepted, Status.Description);
			AssertEquals("status on dummy matches set code", CMRBaseStatuses.Codes.AmendmentAccepted, Dummy.Status);
		}

		public new void TestDefaultValue()
		{
			AssertEquals("default value is", CMRBaseStatuses.Codes.NotSent, Status.Code);
		}

		public void TestIsWaiting()
		{
			Status.Code = "WTO";
			AssertEquals(true, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "WTA";
			AssertEquals(true, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "WTW";
			AssertEquals(true, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);
		}

		public void TestIsAccepted()
		{
			Status.Code = "ACO";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(true, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "ACA";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(true, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "ACW";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(true, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);
		}

		public void TestIsRejected()
		{
			Status.Code = "RJO";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(true, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "RJA";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(true, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);

			Status.Code = "RJW";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(true, Status.IsRejected);
			AssertEquals(false, Status.IsNotSent);
		}

		public void TestIsNotSent()
		{
			Status.Code = "NOT";
			AssertEquals(false, Status.IsWaiting);
			AssertEquals(false, Status.IsAccepted);
			AssertEquals(false, Status.IsRejected);
			AssertEquals(true, Status.IsNotSent);
		}

		#region Implementation

		#region Properties

		MessageCusStatus Status
		{
			get
			{
				if (fStatus == null)
				{
					fStatus = new MessageCusStatus(Dummy.StatusInfo, DummyCalculator);
				}
				return fStatus;
			}
		}
		MessageCusStatus fStatus;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Status;
		}

		#endregion
	}
}
