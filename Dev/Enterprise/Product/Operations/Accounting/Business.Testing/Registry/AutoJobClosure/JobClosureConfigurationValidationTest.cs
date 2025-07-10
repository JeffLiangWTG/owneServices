namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.ComponentModel;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture.Core;

	public class JobClosureConfigurationValidationTest : JobConfigurationSelectorValidationTest
	{
		public override void TestValidateJobType()
		{
			AssertNoErrors("Precondition: JobType should not have errors.", BizObj.JobTypeInfo);

			BizObj.JobType = "";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "ABC";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			var setting1 = (JobClosureConfiguration)BizObjCollection.AddNew();
			var setting2 = (JobClosureConfiguration)BizObjCollection.AddNew();
			var setting3 = (JobClosureConfiguration)BizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);
			AssertNoErrors("Precondition: setting3.JobTypeInfo should not have errors.", setting3.JobTypeInfo);

			setting1.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting1.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			setting2.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting2.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			setting3.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting3.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;

			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting1.JobType = "SHP";
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Sea;

			setting2.JobType = "SHP";
			setting2.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting2.Mode = Core.Constants.TransportModes.Sea;

			setting3.JobType = "SHP";
			setting3.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting3.Mode = Core.Constants.TransportModes.Sea;

			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting1.JobType = "AWB";
			BizObjCollection.RunPreSaveValidation();
			AssertNoError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting2.JobType = "AGB";
			BizObjCollection.RunPreSaveValidation();
			AssertNoError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting1.JobType = "SHP";
			setting2.JobType = "SHP";
			setting3.JobType = "SHP";
			setting1.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;
			setting2.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;
			setting3.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close;

			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);
		}

		public override void TestValidateDirection()
		{
			base.TestValidateDirection();

			var setting1 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting1.JobType = "SHP";
			setting1.Mode = "ALL";
			setting1.DepartmentPK = ZGuid.Empty;
			setting1.DirectionCode = "ALL";

			var setting2 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting2.JobType = "SHP";
			setting2.Mode = "ALL";
			setting2.DepartmentPK = ZGuid.Empty;
			setting2.DirectionCode = "ALL";
			AssertHasError(setting2.DirectionCodeInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");

			setting2.DirectionCode = "EXP";
			AssertNoError(setting2.DirectionCodeInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");
		}

		public override void TestValidateMode()
		{
			base.TestValidateMode();

			var setting1 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting1.JobType = "SHP";
			setting1.DirectionCode = "ALL";
			setting1.DepartmentPK = ZGuid.Empty;
			setting1.Mode = "AIR";

			var setting2 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting2.JobType = "SHP";
			setting2.DirectionCode = "ALL";
			setting2.DepartmentPK = ZGuid.Empty;
			setting2.Mode = "AIR";
			AssertHasError(setting2.ModeInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");

			setting2.Mode = "SEA";
			AssertNoError(setting2.ModeInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");
		}

		public void TestDepartmentPK()
		{
			BizObj.DepartmentPK = ZGuid.Empty;
			AssertNoErrors(BizObj.DepartmentPKInfo);

			BizObj.DepartmentPK = ZGuid.NewZGuid();
			AssertHasError(BizObj.DepartmentPKInfo, "Enter a valid Department.");

			var objectCreator = new TestObjectCreator(Factory);
			BizObj.DepartmentPK = objectCreator.FEADepartment.PK;
			AssertNoError(BizObj.DepartmentPKInfo, "Enter a valid Department.");

			var setting1 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting1.JobType = "SHP";
			setting1.DirectionCode = "ALL";
			setting1.Mode = "AIR";
			setting1.DepartmentPK = objectCreator.FEADepartment.PK;

			var setting2 = (JobClosureConfiguration)BizObjCollection.AddNew();
			setting2.JobType = "SHP";
			setting2.DirectionCode = "ALL";
			setting2.Mode = "AIR";
			setting2.DepartmentPK = objectCreator.FEADepartment.PK;
			AssertHasError(setting2.DepartmentPKInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");

			setting2.DepartmentPK = ZGuid.Empty;
			AssertNoError(setting2.DepartmentPKInfo, "At least one more record already sets Job Closure Configuration for the same Job parameters.");
		}

		public void TestValidateJobClosureDateOption()
		{
			AssertNoErrors("Precondition: Mode should not have errors.", BizObj.JobClosureDateOptionCodeInfo);

			BizObj.JobClosureDateOptionCode = "ABC";
			AssertHasErrors(BizObj.JobClosureDateOptionCodeInfo);

			BizObj.JobClosureDateOptionCode = "";
			AssertHasErrors(BizObj.JobClosureDateOptionCodeInfo);

			BizObj.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AssertHasErrors(BizObj.JobClosureDateOptionCodeInfo);

			// Permitted date options for various job types
			AssertIsPermittedJobClosureDateOption("", CompleteJobClosureDateOptionList);
			AssertIsPermittedJobClosureDateOption("ALL", JobClosureDateOptionPermittedForOthersList);
			AssertIsPermittedJobClosureDateOption("SHP", JobClosureDateOptionPermittedForShipmentsList);
			AssertIsPermittedJobClosureDateOption("FCN", JobClosureDateOptionPermittedFoGatewayJobList);
			AssertIsPermittedJobClosureDateOption("GCN", JobClosureDateOptionPermittedFoGatewayJobList);
			AssertIsPermittedJobClosureDateOption("TCW", JobClosureDateOptionPermittedForTransportConsignmentJobList);
			AssertIsPermittedJobClosureDateOption("LTC", JobClosureDateOptionPermittedForTransportConsignmentJobList);
			AssertIsPermittedJobClosureDateOption("BRK", JobClosureDateOptionPermittedForDeclarationsList);
			AssertIsPermittedJobClosureDateOption("AGS", JobClosureDateOptionPermittedForShippingManagerList);
			AssertIsPermittedJobClosureDateOption("AGB", JobClosureDateOptionPermittedForShippingManagerList);

			foreach (ICodeDescription jobType in JobTypeList)
			{
				if (jobType.Code != "ALL" && jobType.Code != "SHP" && jobType.Code != "FCN" && jobType.Code != "GCN" && jobType.Code != "BRK" && jobType.Code != "AGS" && jobType.Code != "AGB" && jobType.Code != "QSH" && jobType.Code != "TCW" && jobType.Code != "LTC")
				{
					AssertIsPermittedJobClosureDateOption(jobType.Code, JobClosureDateOptionPermittedForOthersList);
				}
			}
		}

		public void TestValidateOffset()
		{
			BizObj.Offset = -1;
			AssertHasError(BizObj.OffsetInfo, "Days Offset must be a non zero positive number");

			BizObj.Offset = 0;
			AssertHasError(BizObj.OffsetInfo, "Days Offset must be a non zero positive number");

			BizObj.Offset = 3;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 10;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 100;
			AssertNoErrors(BizObj.OffsetInfo);
		}

		public void TestValidateReopenRestrictionOffset()
		{
			BizObj.ReopenRestrictionOffset = -1;
			AssertHasError(BizObj.ReopenRestrictionOffsetInfo, "Reopen Restriction Days Offset must be a non positive number");

			BizObj.ReopenRestrictionOffset = 0;
			AssertNoErrors(BizObj.ReopenRestrictionOffsetInfo);

			BizObj.ReopenRestrictionOffset = 1;
			AssertNoErrors(BizObj.ReopenRestrictionOffsetInfo);

			BizObj.ReopenRestrictionOffset = 100;
			AssertNoErrors(BizObj.ReopenRestrictionOffsetInfo);
		}

		public void TestValidateFromJobStatus()
		{
			BizObj.FromJobStatus = string.Empty;
			AssertNoErrors(BizObj.FromJobStatusInfo);

			BizObj.FromJobStatus = "@#$, WRK, &&& , CLS";
			AssertHasError(BizObj.FromJobStatusInfo, "Invalid status code : @#$, &&&, CLS");
		}

		public void TestValidateOffsetTypes()
		{
			BizObj.OffsetType = "***";
			AssertHasError(BizObj.OffsetTypeInfo, "Enter a valid Offset Type.");

			BizObj.OffsetType = "DAY";
			AssertNoError(BizObj.OffsetTypeInfo, "Enter a valid Offset Type.");
		}

		public void TestValidateReopenRestrictionOffsetType()
		{
			BizObj.ReopenRestrictionOffsetType = "***";
			AssertHasError(BizObj.ReopenRestrictionOffsetTypeInfo, "Enter a valid Re-Open Restriction Offset Type.");

			BizObj.ReopenRestrictionOffsetType = "DAY";
			AssertNoError(BizObj.ReopenRestrictionOffsetTypeInfo, "Enter a valid Re-Open Restriction Offset Type.");
		}

		public void TestValidateFromJobStatusWhenConfigurationTypeIsUpdate()
		{
			BizObj.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;
			BizObj.FromJobStatus = string.Empty;
			AssertNoErrors(BizObj.FromJobStatusInfo);

			BizObj.FromJobStatus = "@#$, WRK, &&&, CLS, JFC";
			AssertHasError(BizObj.FromJobStatusInfo, "Invalid status code : @#$, &&&, CLS, JFC");
		}

		public void TestValidateFromJobStatusWhenConfigurationTypeIsClose()
		{
			var configurationForUpdate = (JobClosureConfiguration)BizObjCollection.AddNew();
			configurationForUpdate.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;

			var configurationForClose = (JobClosureConfiguration)BizObjCollection.AddNew();
			configurationForClose.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close;
			configurationForClose.FromJobStatus = string.Empty;
			AssertHasError(configurationForClose.FromJobStatusInfo, "For combination (Job Type + Direction + Mode + Department), the 'To Status'(i.e. JFC) of Update Row Type does not match the 'From Status'(i.e. JFC) of the Close Row Type.");

			configurationForClose.FromJobStatus = "JFC";
			AssertNoErrors(BizObj.FromJobStatusInfo);
		}

		public void TestValidateConfigurationType()
		{
			BizObj.ConfigurationType = "";
			AssertHasError(BizObj.ConfigurationTypeInfo, "Please enter an Update/Close.");

			BizObj.ConfigurationType = "***";
			AssertHasError(BizObj.ConfigurationTypeInfo, "Enter a valid Update/Close.");

			BizObj.ConfigurationType = "UPD";
			AssertHasError(BizObj.ConfigurationTypeInfo, "For each combination (Job Type + Direction + Mode + Department), there must always be a 'Close' row type.");

			var configurationForUpdate = (JobClosureConfiguration)BizObjCollection.AddNew();
			configurationForUpdate.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update;

			AssertHasError(configurationForUpdate.ConfigurationTypeInfo, "For each combination (Job Type + Direction + Mode + Department), there must always be a 'Close' row type.");

			var configurationForClose = (JobClosureConfiguration)BizObjCollection.AddNew();
			configurationForClose.ConfigurationType = JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close;

			configurationForUpdate.ValidateConfigurationType();
			AssertNoError(configurationForUpdate.ConfigurationTypeInfo, "For each combination (Job Type + Direction + Mode + Department), there must always be a 'Close' row type.");
		}

		#region Implementation

		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets Job Closure Configuration for the same Job parameters."; }
		}

		protected new JobClosureConfiguration BizObj
		{
			get { return (JobClosureConfiguration)base.BizObj; }
			set { base.BizObj = value; }
		}

		CodeDescriptionPairList fCompleteJobClosureDateOptionList;
		CodeDescriptionPairList CompleteJobClosureDateOptionList
		{
			get { return fCompleteJobClosureDateOptionList ?? (fCompleteJobClosureDateOptionList = JobClosureConfigurationLookups.CompleteJobClosureDateOptionList); }
		}

		void AssertIsPermittedJobClosureDateOption(string jobType, CodeDescriptionPairList permittedOptionsList)
		{
			BizObj.JobType = jobType;
			foreach (ICodeDescription availableJobClosureDateOption in CompleteJobClosureDateOptionList)
			{
				BizObj.JobClosureDateOptionCode = availableJobClosureDateOption.Code;
				Assert(string.Format("JobClosure Date Option '{0}' is not permitted for job type {1}", availableJobClosureDateOption.Code, jobType),
					permittedOptionsList.Contains(availableJobClosureDateOption) ^ BizObj.JobClosureDateOptionCodeInfo.HasErrors());
			}
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForOthersList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForOthersList
		{
			get { return fJobClosureDateOptionPermittedForOthersList ?? (fJobClosureDateOptionPermittedForOthersList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForOthersList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForShipmentsList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForShipmentsList
		{
			get { return fJobClosureDateOptionPermittedForShipmentsList ?? (fJobClosureDateOptionPermittedForShipmentsList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForShipmentsList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForGatewayJobList;
		CodeDescriptionPairList JobClosureDateOptionPermittedFoGatewayJobList
		{
			get { return fJobClosureDateOptionPermittedForGatewayJobList ?? (fJobClosureDateOptionPermittedForGatewayJobList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForGatewayJobList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForTransportConsignmentJobList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForTransportConsignmentJobList
		{
			get { return fJobClosureDateOptionPermittedForTransportConsignmentJobList ?? (fJobClosureDateOptionPermittedForTransportConsignmentJobList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForTransportConsignmentList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForDeclarationsList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForDeclarationsList
		{
			get { return fJobClosureDateOptionPermittedForDeclarationsList ?? (fJobClosureDateOptionPermittedForDeclarationsList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForDeclarationsList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForShippingManagerList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForShippingManagerList
		{
			get { return fJobClosureDateOptionPermittedForShippingManagerList ?? (fJobClosureDateOptionPermittedForShippingManagerList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForShippingManagerList); }
		}

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				var result = new JobClosureConfiguration();

				result.JobType = "SHP";
				result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
				result.Mode = Core.Constants.TransportModes.Air;
				result.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;

				result.Offset = 2;

				return result;
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				return new JobClosureConfigurationCollection();
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.DepartmentPK = new ZGuid(System.Guid.NewGuid());
			BizObj.JobClosureDateOptionCode = "ALL";
			BizObj.FromJobStatus = "###";
			BizObj.OffsetType = "###";
			BizObj.Offset = -5;
			BizObj.ReopenRestrictionOffsetType = "***";
			BizObj.ReopenRestrictionOffset = -1;
			BizObj.ConfigurationType = "###";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.DepartmentPKInfo);
			AssertHasErrors(BizObj.JobClosureDateOptionCodeInfo);
			AssertHasErrors(BizObj.FromJobStatusInfo);
			AssertHasErrors(BizObj.OffsetInfo);
			AssertHasErrors(BizObj.OffsetTypeInfo);
			AssertHasErrors(BizObj.ReopenRestrictionOffsetInfo);
			AssertHasErrors(BizObj.ReopenRestrictionOffsetTypeInfo);
			AssertHasErrors(BizObj.ConfigurationTypeInfo);
		}

		#endregion
	}
}
