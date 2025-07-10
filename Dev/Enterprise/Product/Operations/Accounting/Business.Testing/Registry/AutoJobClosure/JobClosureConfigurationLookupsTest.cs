namespace Enterprise.Accounting.Business.Testing
{
	using System.Collections.Generic;
	using System.Linq;
	using Enterprise.Accounting.Business.JobInvoicing.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	public class JobClosureConfigurationLookupsTest : JobConfigurationSelectorLookupsTest
	{
		public void TestJobClosureDateOptionList()
		{
			AssertEquals("JobClosureDateOption.Count", 2, BizObj.JobClosureDateOptionList.Count);
			AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, BizObj.JobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, BizObj.JobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
		}

		public void TestCompleteJobClosureDateOptionList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 10, CompleteJobClosureDateOptionList.Count);
			AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("JobClosureDateOption should contain 'Actual Arrival Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("JobClosureDateOption should contain 'Actual Departure Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			AssertEquals("JobClosureDateOption should contain 'Pickup date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate));
			AssertEquals("JobClosureDateOption should contain 'Delivery Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate));
			AssertEquals("JobClosureDateOption should contain 'Customs Clearance Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("JobClosureDateOption should contain 'Vessel Arrival Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate));
			AssertEquals("JobClosureDateOption should contain 'Vessel Departure Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate));
			AssertEquals("JobClosureDateOption should contain 'AWB Issue Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate));
			AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, CompleteJobClosureDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
		}

		public void TestRecognitionDateOptionPermittedForOthersList()
		{
			AssertEquals("JobClosureDateOption.Count", 2, JobClosureDateOptionPermittedForOthersList.Count);
			AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
		}

		public void TestJobClosureDateOptionPermittedForShipmentsList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 8, JobClosureDateOptionPermittedForShipmentsList.Count);
			AssertEquals("JobClosureDateOption should contain 'AWB Issue date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate));
			AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("JobClosureDateOption should contain 'Actual Arrival Date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
			AssertEquals("JobClosureDateOption should contain 'Actual Departure Date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			AssertEquals("JobClosureDateOption should contain 'Customs Clearance Date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("JobClosureDateOption should contain 'Pickup date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate));
			AssertEquals("JobClosureDateOption should contain 'Delivery Date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate));
			AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
		}

		public void TestJobClosureDateOptionPermittedForDeclarationsList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("JobClosureDateOption.Count", 3, JobClosureDateOptionPermittedForDeclarationsList.Count);
				AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
				AssertEquals("JobClosureDateOption should contain 'Customs Clearance Date'", true, JobClosureDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			});
		}

		public void TestJobClosureDateOptionPermittedForShippingManagerList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("JobClosureDateOption.Count", 4, JobClosureDateOptionPermittedForShippingManagerList.Count);
				AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
				AssertEquals("JobClosureDateOption should contain 'Vessel Arrival Date'", true, JobClosureDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate));
				AssertEquals("JobClosureDateOption should contain 'Vessel Departure Date'", true, JobClosureDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate));
			});
		}

		public void TestJobClosureDateOptionPermittedForGatewayList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("JobClosureDateOption.Count", 4, JobClosureDateOptionPermittedForGatewayJobList.Count);
				AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForGatewayJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForGatewayJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
				AssertEquals("JobClosureDateOption should contain 'Actual Arrival Date'", true, JobClosureDateOptionPermittedForGatewayJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate));
				AssertEquals("JobClosureDateOption should contain 'Actual Departure Date'", true, JobClosureDateOptionPermittedForGatewayJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			});
		}

		public void TestJobClosureDateOptionPermittedForConsignmentList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("JobClosureDateOption.Count", 4, JobClosureDateOptionPermittedForTransportConsignmentJobList.Count);
				AssertEquals("JobClosureDateOption should contain 'Job Open Date'", true, JobClosureDateOptionPermittedForTransportConsignmentJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("JobClosureDateOption should contain 'Post Date of First AR Transaction'", true, JobClosureDateOptionPermittedForTransportConsignmentJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
				AssertEquals("JobClosureDateOption should contain 'Pickup Date'", true, JobClosureDateOptionPermittedForTransportConsignmentJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate));
				AssertEquals("JobClosureDateOption should contain 'Delivery Date'", true, JobClosureDateOptionPermittedForTransportConsignmentJobList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate));
			});
		}

		public void TestCorrectJobClosureDateOptionListUsed()
		{
			BizObj.JobType = string.Empty;
			AssertListsAreSame(BizObj.JobClosureDateOptionList, CompleteJobClosureDateOptionList);

			BizObj.JobType = "ALL";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForOthersList);

			BizObj.JobType = "SHP";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForShipmentsList);

			BizObj.JobType = "BRK";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForDeclarationsList);

			BizObj.JobType = "AGS";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForShippingManagerList);

			BizObj.JobType = "AGB";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForShippingManagerList);

			BizObj.JobType = "TCW";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForTransportConsignmentJobList);

			BizObj.JobType = "LTC";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForTransportConsignmentJobList);

			BizObj.JobType = "FCN";
			AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForGatewayJobList);

			foreach (string jobTypeCode in JobTypeList.GetAllCodes().Except(new[] { "ALL", "SHP", "BRK", "AGS", "AGB", "FCN", "GCN", "TCW", "LTC" }))
			{
				BizObj.JobType = jobTypeCode;
				AssertListsAreSame(BizObj.JobClosureDateOptionList, JobClosureDateOptionPermittedForOthersList);
			}
		}

		public void TestOffsetTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "DAY", "MONTH" }, BizObj.OffSetTypes.GetAllCodes());
		}

		public void TestJobChargeRecognitionFilterList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "ALL", "REC", "NRC" }, BizObj.JobChargeRecognitionFilterOptions.GetAllCodes());
		}

		public void TestDepartmentLookupIsCached()
		{
			var departments1 = BizObj.Departments;
			var anotherBizoInSameFactory = new JobClosureConfiguration();
			var departments2 = anotherBizoInSameFactory.Departments;
			AssertNotNull(departments1);
			AssertEquals(departments1, departments2);
		}

		public void TestConfigurationTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "CLS", "UPD" }, BizObj.ConfigurationTypeList.GetAllCodes());
		}

		public new void TestJobTypeList()
		{
			AssertJobTypeList(BizObj.JobTypeList, true);
		}

		public void TestGetAllJobTypesThatCanBeProcessedByJCS()
		{
			AssertJobTypeList(JobClosureConfigurationLookups.GetAllJobTypesThatCanBeProcessedByJCS(), false);
		}

		void AssertJobTypeList(CodeDescriptionPairList jobTypeList, bool expectJobTypeALL)
		{
			var expectedJobTypes = expectJobTypeALL ?  new List<string>() { "ALL" } : new List<string>();
			expectedJobTypes.AddRange(JobClosureProcessorTestHelper.AllowedJobTypeList);

			if (expectJobTypeALL)
			{
				AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", jobTypeList[0].Code);
				Assert("The JobType 'ALL' should be JobInvoicingConsumerType", jobTypeList[0] is JobInvoicingConsumerType);
			}

			AssertContainsExactElementsInAnyOrder(expectedJobTypes, jobTypeList.GetAllCodes());
		}

		#region Implementation

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

		CodeDescriptionPairList fJobClosureDateOptionPermittedForGatewayList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForGatewayJobList
		{
			get { return fJobClosureDateOptionPermittedForGatewayList ?? (fJobClosureDateOptionPermittedForGatewayList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForGatewayJobList); }
		}

		CodeDescriptionPairList fJobClosureDateOptionPermittedForTransportConsignmentList;
		CodeDescriptionPairList JobClosureDateOptionPermittedForTransportConsignmentJobList
		{
			get { return fJobClosureDateOptionPermittedForTransportConsignmentList ?? (fJobClosureDateOptionPermittedForTransportConsignmentList = BizObj.JobClosureConfigurationLookup.JobClosureDateOptionPermittedForTransportConsignmentList); }
		}

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new JobClosureConfiguration();
			}
		}

		#endregion

	}
}
