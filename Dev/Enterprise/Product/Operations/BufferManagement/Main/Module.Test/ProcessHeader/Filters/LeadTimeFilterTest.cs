using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(LeadTimeFilter))]
	[TestUtcOffset(10, 0, 0)]
	[TestDate(2015, 7, 14)]
	class LeadTimeFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Validation

		public void TestValidation()
		{
			filter.LeadTimeEstimateFactor = -1;
			filter.BufferPK = ZGuid.Empty;

			filter.Validation.ValidateAll();

			AssertHasError(filter.BufferPKInfo, "Please enter a Buffer.");
			AssertHasError(filter.LeadTimeRangeCodeInfo, "Please enter a Range.");
			AssertHasError(filter.LeadTimeEstimateFactorInfo, "Please enter a 'Lead Time Estimate Factor' greater than or equal to 0.");

			filter.BufferPK = config.Bucket.PK;
			filter.LeadTimeRangeCode = "AAA";

			AssertHasError(filter.BufferPKInfo, "Enter a valid Buffer.");
			AssertHasError(filter.LeadTimeRangeCodeInfo, "Enter a valid Range.");

			filter.BufferPK = config.Buffer.PK;
			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			filter.LeadTimeEstimateFactor = 0m;

			AssertNoErrors(filter);
		}

		#endregion

		#region Serialisation

		public void TestSerialisation()
		{
			filter.BufferPK = config.Buffer.PK;
			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			filter.LeadTimeEstimateFactor = 2.5m;

			var newFilter = new LeadTimeFilter(Factory);

			BMSTestHelper.SerialiseAndDeSerialise(filter, newFilter);

			AssertEquals(config.Buffer.PK, newFilter.BufferPK);
			AssertEquals(LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime, newFilter.LeadTimeRangeCode);
			AssertEquals(2.5m, newFilter.LeadTimeEstimateFactor);
		}

		#endregion

		#region Filter Query

		[TestDate(2015, 7, 14)]
		public void TestFilter_NoAgreedDeliveryDate()
		{
			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			AssertResults("Should not return records with no agreed delivery date");

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			AssertResults("Should not return records with no agreed delivery date");
		}

		[TestDate(2015, 7, 14)]
		public void TestFilter_WithAgreedDeliveryDate()
		{
			filter.LeadTimeEstimateFactor = 1.5m;

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(1);
			Factory.Save();

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			AssertResults("Job and workflow are within profitable lead time", jobHeader, workflow);

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			AssertResults("Job and workflow are not outside profitable lead time");

			// Advance to the future. Job is now due to complete in the past. This is still within the profitable lead time.
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1);

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			AssertResults("Job and workflow are within profitable lead time (due in the past)", jobHeader, workflow);

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			AssertResults("Job and workflow are not outside profitable lead time (due in the past)");

			// Travel back to the past. Job is now outside the profitable lead time up to the agreed delivery date.
			TestDateAttribute.Date = jobHeader.FH_AgreedDeliveryDate
				.AddMinutes(-(config.Buffer.FC_BufferTimespanInMinutes / 2))
				.AddMinutes((int)-(jobHeader.FH_PlannedDurationInMinutes * 1.5m))
				.AddMinutes(-1)
				.ToDateTime();

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			AssertResults("Job and workflow are not within profitable lead time");

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			AssertResults("Job and workflow are outside profitable lead time", jobHeader, workflow);

			// Travel one minute into the future. We're now just inside the profitable lead time.
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.WithinProfitableLeadTime;
			AssertResults("Job and workflow are within profitable lead time", jobHeader, workflow);

			filter.LeadTimeRangeCode = LeadTimeRangeTypeList.Codes.OutsideProfitableLeadTime;
			AssertResults("Job and workflow are not outside profitable lead time");
		}

		#endregion

		#region Implementation

		void AssertResults(string message, params ProcessHeader[] expectedResults)
		{
			AssertContainsExactElementsInAnyOrder(message, expectedResults, Factory.Load<ProcessHeader>(filter.Query));
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			filter = new LeadTimeFilter(Factory)
			{
				IsActive = true,
				BufferPK = config.Buffer.PK
			};
			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Pantomime Horses");
			workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Enraged Pantomime Princess Margaret");
			BMSTestHelper.CreateTask(workflow, description: "Life or Death Struggrl", lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();
		}

		SchematicTestConfig config;
		LeadTimeFilter filter;
		ProcessJobHeader jobHeader;
		ProcessHeader workflow;

		#endregion
	}
}
