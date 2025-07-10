using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbILStaffExternalPasswordCollection))]
	sealed class GlbILStaffExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPasswordTypeFilter()
		{
			var collection = (GlbILStaffExternalPasswordCollection)GetCollectionToTest();
			AssertContains("CompleteFilter should contain Signature password type filter", "GP_PasswordType = 'ILS'", collection.CompleteFilter.LiteralTextADOFormatted);
		}

		public void TestMaxCountValidation()
		{
			var collection = (ISupportMaxCountValidation)GetCollectionToTest();
			CombineAssertions(() =>
			{
				var validator = collection.MaxCountValidator;
				var notification = validator.Notification;
				AssertEquals("MaxCount", 1, validator.MaxCount);
				AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
				AssertEquals("Notification Type", NotificationType.Error, notification.Type);
				AssertEquals("Notification Message", "Only one Signature entry is allowed.", notification.Message);
			});
		}

		public void TestErrorReportedIfMoreThanOneElementIsAdded()
		{
			var collection = GetCollectionToTest();
			collection.AddNew();
			AssertEquals("ErrorReporter.LastMessageReported", "", ErrorReporter.LastMessageReported);

			collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("ErrorReporter.LastKeyReported", $"{TestedTypeHelper.GetTestedType(GetType()).FullName} error", ErrorReporter.LastKeyReported);
				AssertEquals("ErrorReporter.LastMessageReported", "Only one Signature entry is allowed.", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public override void TestAdd()
		{
			base.TestAdd();
			ErrorReporter.Clear();
		}

		public override void TestAddNew()
		{
			base.TestAddNew();
			ErrorReporter.Clear();
		}

		public override void TestDelete()
		{
			base.TestDelete();
			ErrorReporter.Clear();
		}

		public override void TestSuspendCountChanged()
		{
			base.TestSuspendCountChanged();
			ErrorReporter.Clear();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var staff = Factory.New<GlbStaff>();
			return new GlbILStaffExternalPasswordCollection(staff);
		}
	}
}
