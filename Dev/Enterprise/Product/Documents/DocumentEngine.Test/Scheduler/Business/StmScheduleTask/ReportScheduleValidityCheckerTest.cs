using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ReportScheduleValidityCheckerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown("Test Constructor", () => new ReportScheduleValidityChecker(null, null, null));
		}

		public void TestCheckDeserializedReportFiltersSettingIsCompatibleWithTemplateFilters()
		{
			SetUpPostMasterGroup();
			var excelTemplate = MultipleTemplatesWithOptionalColumns;
			using (var deserializedReport = new Report(new DocumentPack(), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				deserializedReport.PrepareForRender();

				using (var reportFromTemplate = new Report(new DocumentPack(), excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
				{
					reportFromTemplate.PrepareForRender();

					ReportScheduleValidityChecker checker = new ReportScheduleValidityChecker(null, deserializedReport, reportFromTemplate);

					AssertEquals("Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);

					AssertEquals(true, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());

					deserializedReport.SortOrderCollection.Add(new RuntimeOptions.SortOrder("AUS", "Australia"));
					AssertEquals(false, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());

					deserializedReport.SortOrderCollection.Clear();
					checker.ClearErrorManager();
					AssertEquals(true, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());
					deserializedReport.GroupByCollection.Add(new GroupBy("NZ", "New Zealand"));
					AssertEquals(false, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());

					deserializedReport.GroupByCollection.Clear();
					checker.ClearErrorManager();
					AssertEquals(true, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());
					deserializedReport.OptionalTemplateSheetCollection.Add("extra");
					AssertEquals(false, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());

					deserializedReport.OptionalTemplateSheetCollection.Clear();
					checker.ClearErrorManager();
					AssertEquals(true, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());
					DateField dateField = new DateField(new BusinessObjectFactory());
					dateField.DisplayName = "Date";
					deserializedReport.FilterCollection.Add(dateField);
					AssertEquals(false, checker.IsDeserializedReportFiltersMatchingWithTemplateFilters());

					AssertEquals("Emails to Client", 4, Env.OutgoingMailManager.EmailsCreated.Count);
					Env.OutgoingMailManager.EmailsCreated.Clear();
				}
			}
		}

		public void TestEmailErrorMessageToLocalUserToDealWithTheIssue_CatchesEmailHasNoFromAddressException()
		{
			RawDataRegistry.Instance.MailboxEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "zac@bat.com";
			staff.GS_Code = "ZAC";

			var email = new EmailDef();
			email.Subject = "Test Subject";
			email.Body = "Test Body";

			Factory.Save();

			using (var dummyReport = new Report(new DocumentPack(), MultipleTemplatesWithOptionalColumns, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				dummyReport.PrepareForRender();

				var checker = new ReportScheduleValidityChecker(null, dummyReport, dummyReport);

				AssertExceptionThrown<EmailHasNoFromAddressException>(() => { checker.EmailErrorMessageToLocalUserToDealWithTheIssue(); });
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting multipleTemplatesWithOptionalColumns;
		ExcelTemplateForUnitTesting MultipleTemplatesWithOptionalColumns
		{
			get
			{
				if (multipleTemplatesWithOptionalColumns == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
					multipleTemplatesWithOptionalColumns = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));
				}
				return multipleTemplatesWithOptionalColumns;
			}
		}

		void SetUpPostMasterGroup()
		{
			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "staff@group.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
		}
	}
}
