
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(AlternativeEvidenceCollection<AlternativeEvidence>))]
	class AlternativeEvidenceCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AlternativeEvidence>
	{
		public void TexMaxCount()
		{
			var collection = (ISupportMaxCountValidation)GetCusCodeDataCollection();
			var validator = collection.MaxCountValidator;
			var notification = validator.Notification;
			AssertEquals("MaxCount", 9, validator.MaxCount);
			AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
			AssertEquals("Notification Type", NotificationType.Error, notification.Type);
			AssertEquals("Notification Message", "Maximum number of items is 9.", notification.Message);
		}

		public void TestAllowNew()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var report = header.CusExitReports.AddNew();
			CombineAssertions(() =>
			{
				for (int i = 0; i < 8; i++)
				{
					report.AlternativeEvidences.AddNew();
				}
				AssertEquals("Allow new when count is 8", true, report.AlternativeEvidences.AllowNew);

				report.AlternativeEvidences.AddNew();
				AssertEquals("Maximum count is 9", false, report.AlternativeEvidences.AllowNew);
			});
		}

		protected override CusCodeDataCollection<AlternativeEvidence> GetCusCodeDataCollection()
		{
			var header = Factory.NewWithValidTestData<CusExitHeader>();
			var report = header.CusExitReports.AddNew();
			return new AlternativeEvidenceCollection<AlternativeEvidence>(report);
		}
	}
}
