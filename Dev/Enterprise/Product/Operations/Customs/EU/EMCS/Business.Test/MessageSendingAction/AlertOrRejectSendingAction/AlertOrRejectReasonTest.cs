using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(AlertOrRejectReason))]
	sealed class AlertOrRejectReasonTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Alert or Reject Reason", emcsAlertOrRejectReason.HumanReadableName);
		}

		public void TestMaxLength()
		{
			AssertEquals(3, emcsAlertOrRejectReason.ReasonInfo.MaxLength);
			AssertEquals(350, emcsAlertOrRejectReason.InformationInfo.MaxLength);
		}

		public void TestLookups()
		{
			AssertType<AlertOrRejectReasonLookups>(emcsAlertOrRejectReason.Lookups);
		}

		public void TestValidation()
		{
			AssertType<AlertOrRejectReasonValidation>(emcsAlertOrRejectReason.Validation);
		}

		AlertOrRejectReason emcsAlertOrRejectReason;
		protected override void SetUp()
		{
			base.SetUp();
			emcsAlertOrRejectReason = new AlertOrRejectReason(Factory);
		}

		protected override BusinessObject GetNewBusinessObject() => new AlertOrRejectReason(Factory);
	}
}
