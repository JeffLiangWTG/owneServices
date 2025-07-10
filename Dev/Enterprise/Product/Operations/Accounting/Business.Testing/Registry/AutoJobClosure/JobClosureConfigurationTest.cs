namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.Registry.Business;
	using NUnit.Framework;

	[TestedType(typeof(JobClosureConfiguration))]
	public class JobClosureConfigurationTest : ChargeGroupSettingTest
	{
		public void TestDefualtValue()
		{
			var jobClosureConfiguration = new JobClosureConfiguration();

			AssertEquals(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, jobClosureConfiguration.JobType);
			AssertEquals(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, jobClosureConfiguration.JobClosureDateOptionCode);
			AssertEquals(JobConfigurationSelectorHelper.OffsetTypeCodes.Days, jobClosureConfiguration.ReopenRestrictionOffsetType);
			AssertEquals(JobConfigurationSelectorHelper.OffsetTypeCodes.Days, jobClosureConfiguration.OffsetType);
			AssertEquals(1, jobClosureConfiguration.Offset);
			AssertEquals(0, jobClosureConfiguration.ReopenRestrictionOffset);
			AssertEquals(ChargeRecognitionFilterOptionList.Codes.ALL, jobClosureConfiguration.JobChargeRecognitionFilter);
			AssertEquals(JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close, jobClosureConfiguration.ConfigurationType);
		}

		public void TestToJobStatusWhenSetConfigurationType()
		{
			var jobClosureConfiguration = new JobClosureConfiguration();

			AssertEquals("Precondition", JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close, jobClosureConfiguration.ConfigurationType);
			AssertEquals("Precondition", JobHeaderStatus.Closed.Code, jobClosureConfiguration.ToJobStatus);

			jobClosureConfiguration.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;
			AssertEquals(JobHeaderStatus.JobReadyForFinancialClosure.Code, jobClosureConfiguration.ToJobStatus);

			jobClosureConfiguration.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close;
			AssertEquals(JobHeaderStatus.Closed.Code, jobClosureConfiguration.ToJobStatus);
		}

		public void TestReadOnlyOfToJobStatus()
		{
			var jobClosureConfiguration = new JobClosureConfiguration();
			Assert(jobClosureConfiguration.ToJobStatusInfo.ReadOnly);
		}

		public void TestPropertyReadOnlyWhenConfigurationTypeIsUpdate()
		{
			AssertNotEquals("Precondition", JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update, BizObj.ConfigurationType);
			Assert("Precondition", !BizObj.ReopenRestrictionOffsetInfo.ReadOnly);
			Assert("Precondition", !BizObj.CloseJobsWithOpenWipInfo.ReadOnly);
			Assert("Precondition", !BizObj.CloseJobsWithOpenAcrInfo.ReadOnly);
			Assert("Precondition", !BizObj.JobChargeRecognitionFilterInfo.ReadOnly);
			Assert("Precondition", !BizObj.ReopenRestrictionOffsetTypeInfo.ReadOnly);

			BizObj.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;
			Assert(BizObj.ReopenRestrictionOffsetInfo.ReadOnly);
			Assert(BizObj.CloseJobsWithOpenWipInfo.ReadOnly);
			Assert(BizObj.CloseJobsWithOpenAcrInfo.ReadOnly);
			Assert(BizObj.JobChargeRecognitionFilterInfo.ReadOnly);
			Assert(BizObj.ReopenRestrictionOffsetTypeInfo.ReadOnly);
		}

		public void TestIsSameItem()
		{
			var jobClosureConfiguration1 = new JobClosureConfiguration();
			var jobClosureConfiguration2 = new JobClosureConfiguration();
			var jobClosureConfiguration3 = new JobClosureConfiguration();

			var identifier = ZGuid.NewZGuid();
			var identifier2 = ZGuid.NewZGuid();

			jobClosureConfiguration1.Identifier = identifier;
			jobClosureConfiguration2.Identifier = identifier;
			jobClosureConfiguration3.Identifier = identifier2;

			Assert(jobClosureConfiguration1.IsSameItem(jobClosureConfiguration2));
			Assert(!jobClosureConfiguration1.IsSameItem(jobClosureConfiguration3));
		}

		public void TestIsEqual()
		{
			var jobClosureConfiguration1 = new JobClosureConfiguration();
			var jobClosureConfiguration2 = new JobClosureConfiguration();
			var jobClosureConfiguration3 = new JobClosureConfiguration();
			var identifier = ZGuid.NewZGuid();
			jobClosureConfiguration1.Identifier = identifier;
			jobClosureConfiguration2.Identifier = identifier;
			jobClosureConfiguration3.Identifier = identifier;

			AssertEquals("Precondition", JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, jobClosureConfiguration1.JobType);
			AssertEquals("Precondition", JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, jobClosureConfiguration2.JobType);
			AssertEquals("Precondition", JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, jobClosureConfiguration3.JobType);

			jobClosureConfiguration3.JobType = "AAA";

			Assert(jobClosureConfiguration1.IsEqual(jobClosureConfiguration2));
			Assert(!jobClosureConfiguration1.IsEqual(jobClosureConfiguration3));
		}

		public void TestGetLogText()
		{
			var jobClosureConfiguration = new JobClosureConfiguration();

			jobClosureConfiguration.JobType = "JBT";
			jobClosureConfiguration.DirectionCode = "DCC";
			jobClosureConfiguration.Mode = "MDE";
			jobClosureConfiguration.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			jobClosureConfiguration.ConfigurationType = "CLS";
			jobClosureConfiguration.FromJobStatus = "INV";
			jobClosureConfiguration.ToJobStatus = "CLS";
			jobClosureConfiguration.CloseJobsWithOpenAcr = true;
			jobClosureConfiguration.CloseJobsWithOpenWip = false;
			jobClosureConfiguration.JobChargeRecognitionFilter = "JRF";
			jobClosureConfiguration.JobClosureDateOptionCode = "JCD";
			jobClosureConfiguration.Offset = 1;
			jobClosureConfiguration.OffsetType = "OFT";
			jobClosureConfiguration.ReopenRestrictionOffset = 1;
			jobClosureConfiguration.ReopenRestrictionOffsetType = "RFT";

			AssertEquals($@"Configuration Added: Job Type = JBT, Direction = DCC, Mode = MDE, Dept = {GlbDepartment.CurrentDepartment.GE_Code}, Update/Close = Close, From Status = INV, To Status = CLS, Open Accruals = Yes, Open WIPs = No, Recognized Charges = JRF, Relevant Date = JCD, Offset = 1, Offset Type = OFT, Reopen Offset = 1, Reopen Offset Type = RFT", jobClosureConfiguration.GetLogText(RegistryChangeLogger.EventType.Add));
			AssertEquals($@"Configuration Changed: Job Type = JBT, Direction = DCC, Mode = MDE, Dept = {GlbDepartment.CurrentDepartment.GE_Code}, Update/Close = Close, From Status = INV, To Status = CLS, Open Accruals = Yes, Open WIPs = No, Recognized Charges = JRF, Relevant Date = JCD, Offset = 1, Offset Type = OFT, Reopen Offset = 1, Reopen Offset Type = RFT", jobClosureConfiguration.GetLogText(RegistryChangeLogger.EventType.Edit));
			AssertEquals($@"Configuration Deleted: Job Type = JBT, Direction = DCC, Mode = MDE, Dept = {GlbDepartment.CurrentDepartment.GE_Code}, Update/Close = Close, From Status = INV, To Status = CLS, Open Accruals = Yes, Open WIPs = No, Recognized Charges = JRF, Relevant Date = JCD, Offset = 1, Offset Type = OFT, Reopen Offset = 1, Reopen Offset Type = RFT", jobClosureConfiguration.GetLogText(RegistryChangeLogger.EventType.Delete));
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new JobClosureConfiguration();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Core.Constants.TransportModes.Air;
			result.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;

			result.Offset = 2;

			return result;
		}

		protected new JobClosureConfiguration BizObj
		{
			get { return (JobClosureConfiguration)base.BizObj; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}
	}
}
