using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementFilterBusinessObjectBase))]
	public class JobManagementFilterBusinessObjectBaseTest : AccountingFilterStripBusinessObjectTestCase
	{
		#region Filter Tests

		public void TestForwardingConsolsExcluded()
		{
			ForwardingConsol forwardingConsol = TestObjectCreator.CreateConsol();
			JobHeader forwardingConsolJob = Factory.NewJobForTesting<Job>();
			forwardingConsolJob.Parent = forwardingConsol;

			ForwardingConsol forwardingConsol2 = TestObjectCreator.CreateConsol(consolNum: "C002");
			JobHeader legacyGatewayJob = TestObjectCreator.CreateJobForLegacyGateway(forwardingConsol2);

			CommonConsol cfsConsol = (CommonConsol)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Freight.Integration.CFS.ICFSLoadListConsol)));
			JobHeader cfsConsolJob = new Job.Loader((IJobHeaderParent)cfsConsol).TryLoadOrCreateWithoutMutexForTestOnly();
			cfsConsolJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			cfsConsol.JK_IsCFS = true;

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain cfsConsolJob", FilterCollection.Contains(cfsConsolJob));
			Assert("Expecting collection to contain legacyGatewayJob", FilterCollection.Contains(legacyGatewayJob));
			Assert("Expecting collection not to contain forwardingConsolJob", !FilterCollection.Contains(forwardingConsolJob));
		}

		public void TestSpotQuotesExcluded()
		{
			JobHeader spotQuoteJob = Factory.NewJobForTesting<JobHeader>();
			spotQuoteJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			spotQuoteJob.JH_GB = GlbBranch.CurrentBranch.PK;
			spotQuoteJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			spotQuoteJob.JH_JobNum = "Job1";
			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection to not contain spotQuote", false, FilterCollection.Contains(spotQuoteJob));
		}

		#region TestTransportBookingQuoteJobHeaderExcluded

		public void TestTransportBookingQuoteJobHeaderExcluded()
		{
			// Consolidation "Quote" type
			var consolidationBooking1 = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking1.KB_JobType = TransportConsolidationJobTypes.Codes.QuotedBooking;

			var booking1 = CreateDtbBookingWithConsolidationPK(consolidationBooking1.PK);
			var booking2 = CreateDtbBookingWithConsolidationPK(consolidationBooking1.PK);
			var jobHeader1 = CreateJobHeaderOnDtbBooking(booking1, "Job1");
			var jobHeader2 = CreateJobHeaderOnDtbBooking(booking2, "Job2");

			// Consolidation "Booking" type
			var consolidationBooking2 = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking2.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			var booking3 = CreateDtbBookingWithConsolidationPK(consolidationBooking2.PK);
			var booking4 = CreateDtbBookingWithConsolidationPK(consolidationBooking2.PK);
			var jobHeader3 = CreateJobHeaderOnDtbBooking(booking3, "Job3");
			var jobHeader4 = CreateJobHeaderOnDtbBooking(booking4, "Job4");

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection does not contain Transport Booking quote Job Header", false, FilterCollection.Contains(jobHeader1));
			AssertEquals("Expecting collection does not contain Transport Booking quote Job Header", false, FilterCollection.Contains(jobHeader2));
			AssertEquals("Expecting collection should contain Transport Booking Job Header", true, FilterCollection.Contains(jobHeader3));
			AssertEquals("Expecting collection should contain Transport Booking Job Header", true, FilterCollection.Contains(jobHeader4));
		}

		JobHeader CreateJobHeaderOnDtbBooking(IDtbBooking booking, string jobNum)
		{
			var jobHeader = new Job.Loader((IJobHeaderParent)booking).TryLoadOrCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			jobHeader.JH_JobNum = jobNum;

			return jobHeader;
		}

		IDtbBooking CreateDtbBookingWithConsolidationPK(ZGuid consolidationPK)
		{
			IDtbBooking booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidationPK;

			return booking;
		}

		#endregion

		public void TestGatewayJobIncluded()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "JB007";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();

			Factory.Save();

			Assert("Pre-condition", consol.IsGateway());
			AssertEquals("JH_JobNum", "JB007", consolJob.JH_JobNum);

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain consolJob", FilterCollection.Contains(consolJob));
		}

		public void TestNonCurrentLoginCompanyExcluded()
		{
			Job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Job2.JH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK;
				Factory.Save();
			}

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		public void TestIsActiveStatusFilterAlwaysAppliedOfJobManagementFilter()
		{
			var type = typeof(JobManagementFilterBusinessObjectBase);
			var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

			AssertEquals("IsActiveStatusFilterAlwaysApplied", true, (bool)type.GetMethod("IsActiveStatusFilterAlwaysApplied", flags).Invoke(new JobManagementFilterBusinessObjectBase(), null));
		}

		#endregion

		#region Text Filter Tests

		public void TestJobStatusQuery()
		{
			Job1.JH_Status = JobHeaderStatus.Closed.Code;
			Job2.JH_Status = JobHeaderStatus.Working.Code;
			Job3.JH_Status = JobHeaderStatus.JobInvoiced.Code;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Job Status"];

			filter.Property = "OPN";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));

			filter.Property = "OPN";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = JobHeaderStatus.Complete.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
		}

		public void TestJobStatusHoldReasonFilter()
		{
			Job1.JH_HoldReason = "Reason ABC";
			Job2.JH_HoldReason = "Reason DEF";
			Job4.JH_HoldReason = "";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Job3.JH_HoldReason = "Reason GHI";
				Job3.JH_GC = TestObjectCreator.NonCurrentCompanyBranch.Company.PK; // This Job3 belongs to another Company and should not appear in any of the results.
				Factory.Save();
			}

			var filter = (ModuleTextFilter)FilterBO["Job Status Hold Reason"];

			filter.Property = "Reason";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job4", !FilterCollection.Contains(Job4));

			filter.Property = "Reason DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job4", !FilterCollection.Contains(Job4));

			filter.Property = "DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job4", !FilterCollection.Contains(Job4));
		}

		#endregion

		#region Number Filter Tests

		public void TestFilterMaxLengths()
		{
			AssertEquals("Consol # max length", ModuleNumberFilter.MultiplyMaxLength(JobConsolSchema.JK_UniqueConsignRef.MaxLength), FilterBO["Consol #"].MaxLength);
			AssertEquals("House Bill #", ModuleNumberFilter.MultiplyMaxLength(JobShipmentSchema.JS_HouseBill.MaxLength), FilterBO["House Bill #"].MaxLength);
			AssertEquals("Master Bill # max length", ModuleNumberFilter.MultiplyMaxLength(JobConsolSchema.JK_MasterBillNum.MaxLength), FilterBO["Master Bill #"].MaxLength);
			AssertEquals("Flight/Voyage # and Vessel max length", JobVoyageSchema.JV_VoyageFlight.MaxLength, FilterBO["Flight/Voyage # and Vessel"].MaxLength);
			AssertEquals("Customs Entry # max length", ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength), FilterBO["Customs Entry #"].MaxLength);
			AssertEquals("Order # max length", ModuleNumberFilter.MultiplyMaxLength(JobOrderHeaderSchema.JD_OrderNumber.MaxLength), FilterBO["Order #"].MaxLength);
			AssertEquals("Transport Booking Reference max length", ModuleNumberFilter.MultiplyMaxLength(DtbBookingSchema.KM_TransportReference.MaxLength), FilterBO["Transport Booking Reference"].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.RunSheetNumberFilterName} max length", ModuleNumberFilter.MultiplyMaxLength(DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength), FilterBO[InvoiceBulkOperationFilterHelper.RunSheetNumberFilterName].MaxLength);
			AssertEquals("Container # max length", ModuleNumberFilter.MultiplyMaxLength(JobContainerSchema.JC_ContainerNum.MaxLength), FilterBO["Container #"].MaxLength);
			AssertEquals("Job Local Reference max length", ModuleNumberFilter.MultiplyMaxLength(JobHeaderSchema.JH_JobLocalReference.MaxLength), FilterBO["Job Local Reference"].MaxLength);
		}

		public void TestJobNumberFilter()
		{
			Job1.JH_JobNum = "11111111";
			Job2.JH_JobNum = "22222222";
			Job3.JH_JobNum = "33333333";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["Job #"];

			filter.Property = "11111111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = "";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));

			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
		}

		public void TestJobNumberRangeFilter()
		{
			Job1.JH_JobNum = "11111111";
			Job2.JH_JobNum = "22222222";
			Job3.JH_JobNum = "33333333";

			Factory.Save();

			var filter = (ModuleTextRangeFilter)FilterBO["Job Number Range"];

			filter.Property1 = "";
			filter.Property2 = "11111111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property1 = "22222222";
			filter.Property2 = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property1 = "11111111";
			filter.Property2 = "33333333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));

			filter.Property1 = "444";
			filter.Property2 = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
		}

		public void TestJobNumberFilterIsExclusive()
		{
			var exclusiveFilter = FilterBO.GetModuleFilterThatOverridesAllOtherFilters();

			AssertNotNull("Module should contain an exclusive filter", exclusiveFilter);
			AssertEquals("Module exclusive filter should be a 'Job #' filter", AccountingUtils.NumberFilterTypes.JobNumber, exclusiveFilter.Description);
		}

		public void TestGatewayJobsForShipmentNumberFilter()
		{
			var creator = new TestObjectCreator(Factory);
			//			C0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = creator.CreateGatewayConsolsAndShipments();

			var shipment1 = setup.s0001;
			var shipment2 = setup.s0002;
			var consol1 = setup.gC0001;
			var consol2 = setup.gC0002;

			var shipment1JobHeader = creator.CreateJob(shipment1);
			var shipment2JobHeader = creator.CreateJob(shipment2);
			var consol1JobHeader = creator.CreateJob(consol1);
			var consol2JobHeader = creator.CreateJob(consol2);
			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["Gateway Jobs for Shipment #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "S0001";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain job for shipment1", FilterCollection.Contains(shipment1JobHeader));
			Assert("Expecting collection not to contain job for shipment2", !FilterCollection.Contains(shipment2JobHeader));
			Assert("Expecting collection not to contain job for consol1", !FilterCollection.Contains(consol1JobHeader));
			Assert("Expecting collection to contain job for consol2", FilterCollection.Contains(consol2JobHeader));
		}

		public void TestConsolNumberFilter()
		{
			ForwardingConsol jobConsol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol jobConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			jobConsol1.JK_UniqueConsignRef = "11100011";
			jobConsol2.JK_UniqueConsignRef = "22000222";

			jobConsol1.JK_IsCFS = false;
			jobConsol2.JK_IsCFS = false;

			ForwardingShipment jobShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment jobShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = jobConsol1.PK;
			jobConShipLink2.JN_JK = jobConsol2.PK;

			jobConShipLink1.JN_JS = jobShipment1.PK;
			jobConShipLink2.JN_JS = jobShipment2.PK;

			Job1.JH_ParentID = jobShipment1.PK;
			Job2.JH_ParentID = jobShipment2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Consol #"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#region Operations Filters Test

		public void TestInvoiceJobXMLDataHaveTheSameFieldsAsOperationsFilters()
		{
			var universalTransactionWrapper = new UniversalTransactionWrapper(Factory);
			var invoiceJobXMLDataFieldsList = universalTransactionWrapper.GetJobDataFieldNameList_ForTestOnly();

			var invoiceJobXMLDataFiltersToExcludeList = new[]
				{
						"Job Target #"
					};

			var operationsFilters = FilterBO.ModuleFilters.Where(x => x.Category.Description.ToString() == "Other Operations Filters").Select(x => x.MultilingualDescription.ToString());

			var excludedFiltersList = new[]
				{
						"Gateway Jobs for Shipment #", //Excluded as it's only related to search gateway job related to a shipment and it's only applied to the internal job.
						"Consol #", //Excluded as it’s not a shipment property. It’s a consol property.
						"Master Bill #", //Excluded as it’s not a shipment property. It’s a consol property.
						"Consignment Run-sheet Number", //Can be added later by request. This is for TransportConsignment Data Object with DtbConsignmentDataObjectReader and looks like it’s maybe only imported and not exported by CW. I couldn’t find any references on Run Sheet there.
						"Carrier/Principal", //Excluded as it’s not a shipment property. It’s a consol property.
						"Job Local Reference" //Job Header field - not in UXML
					};

			AssertContainsExactElementsInAnyOrder(
@"If new filter is added please consider to add it in invoice JobXMLData list to help users to have more information to find matched job.
If it's decided to not include it, please add in to excludedFiltersList in this test with a reason in comment.",
				operationsFilters.Except(excludedFiltersList), invoiceJobXMLDataFieldsList.Except(invoiceJobXMLDataFiltersToExcludeList));
		}

		#region Test Job Local Reference

		public void TestLocalJobReferenceFilter()
		{
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Job Local Reference"];
			filter.Property = Job1.JH_JobLocalReference;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.Property = Job2.JH_JobLocalReference;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.Property = "C";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#region Test House Bill Number Filter

		public void TestHouseBillNumberFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_HouseBill = "A12345";
			shipment2.JS_HouseBill = "B98765";

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["House Bill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "B98765";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "C";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#region Test Master Bill Or Ocean Bill Number

		public void TestMasterBillOrOceanBillNumber()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_HouseBill = "D12345";
			shipment2.JS_HouseBill = "C98765";

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			consol1.JK_MasterBillNum = "A12345";
			consol2.JK_MasterBillNum = "B98765";

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Factory.Save();

			BusinessObject declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration1[JobDeclarationSchema.JE_MasterBill] = "E44444";

			Job3.JH_ParentID = (ZGuid)declaration1[JobDeclarationSchema.PK];
			Job3.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Master Bill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = "B";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = "C";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			filter.Property = "E";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
		}

		#endregion

		#region Test Container Number Filter

		public void TestContainerNumberFilter()
		{
			ForwardingShipment forwardingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment.JS_IsForwardRegistered = true;
			ForwardingShipment cFSShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			cFSShipment.JS_IsCFSRegistered = true;
			ForwardingShipment cFSGatePass = Factory.NewWithValidTestData<ForwardingShipment>();
			cFSGatePass.JS_IsCFSRegistered = true;
			cFSGatePass.JS_TranshipToOtherCFS = true;
			ForwardingShipment shippingBooking = Factory.NewWithValidTestData<ForwardingShipment>();
			ContainerDetention contDetention = Factory.NewWithValidTestData<ContainerDetention>();

			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_IsCFS = true;

			Job1.JH_ParentID = forwardingShipment.PK;
			Job2.JH_ParentID = cFSShipment.PK;
			Job3.JH_ParentID = cFSGatePass.PK;
			Job4.JH_ParentID = consol1.PK;
			JobManagement job5 = Factory.NewWithValidTestData<JobManagement>();
			job5.JH_ParentID = shippingBooking.PK;
			JobManagement job6 = Factory.NewWithValidTestData<JobManagement>();
			job6.JH_ParentID = contDetention.PK;

			ForwardingContainer fc1 = Factory.NewWithValidTestData<ForwardingContainer>();
			fc1.JC_ContainerNum = "A123";
			fc1.JC_JK = consol1.PK;
			fc1.JC_JS_FCLBookingOnlyLink = shippingBooking.PK;

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment forwardingShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment1.JS_IsForwardRegistered = true;
			ForwardingShipment forwardingShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			forwardingShipment2.JS_IsForwardRegistered = true;

			JobManagement job7 = Factory.NewWithValidTestData<JobManagement>();
			job7.JH_ParentID = forwardingShipment1.PK;
			JobManagement job8 = Factory.NewWithValidTestData<JobManagement>();
			job8.JH_ParentID = forwardingShipment2.PK;

			ForwardingContainer fc2 = Factory.NewWithValidTestData<ForwardingContainer>();
			fc2.JC_ContainerNum = "B456";
			fc2.JC_JK = consol2.PK;

			RefContainerStock stock = Factory.NewWithValidTestData<RefContainerStock>();
			stock.R6_ContainerNum = fc1.JC_ContainerNum;
			contDetention.Movements.AddNew().E9_R6 = stock.PK;

			consol1.Shipments.AddRange(forwardingShipment, cFSShipment, cFSGatePass);
			consol2.Shipments.AddRange(forwardingShipment1, forwardingShipment2);

			forwardingShipment.OuterPackLines.AddNew().Containers.Add(fc1);
			cFSGatePass.OuterPackLines.AddNew().Containers.Add(fc1);
			forwardingShipment1.OuterPackLines.AddNew().Containers.Add(fc2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Container #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Should be 6 Jobs", 6, FilterCollection.Count);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));
			Assert("Expecting collection to contain Job5", FilterCollection.Contains(job5));
			Assert("Expecting collection to contain Job6", FilterCollection.Contains(job6));

			filter.Property = "B";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Should be 1 Job", 1, FilterCollection.Count);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(job7));

			filter.Property = "C";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Should be 0 Jobs", 0, FilterCollection.Count);
		}

		#endregion

		#region Test Voyage Vessel Filter

		public void TestVoyageVesselFilter()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "tvfvf1";
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			Transport transport1 = consol1.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = vessel.RV_FK;
			transport1.JW_VoyageFlight = "2222";

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "tvfvf2";
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			Transport transport2 = consol2.Transports[0];
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "7654321";

			transport2.JW_Vessel = vessel2.RV_FK;
			transport2.JW_VoyageFlight = "2233";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_IsShipping = true;
			shipment2.JS_IsShipping = true;

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			var vessel3 = Factory.NewWithValidTestData<RefVessel>();
			vessel3.RV_Name = "NkVessel";

			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageFlight = "02";
			voyage.JV_RV_NKVessel = vessel3.RV_FK;
			VoyageAccount voyAccount = Factory.NewWithValidTestData<VoyageAccount>();
			voyAccount.NA_JV = voyage.PK;
			Job3.JH_ParentID = voyAccount.PK;

			Factory.Save();

			ModuleTextAndNkFilter voyageVesselFilter = (ModuleTextAndNkFilter)FilterBO["Flight/Voyage # and Vessel"];

			voyageVesselFilter.Property = "";
			voyageVesselFilter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));

			voyageVesselFilter.Property = "22";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			voyageVesselFilter.Property = "2233";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			voyageVesselFilter.Property = "";
			voyageVesselFilter.NkProperty = vessel.RV_Name;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			voyageVesselFilter.NkProperty = vessel2.RV_Name;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			voyageVesselFilter.NkProperty = "Test";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));

			voyageVesselFilter.NkProperty = "Nk";
			voyageVesselFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));

			voyageVesselFilter.NkProperty = "";
			voyageVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
		}

		#endregion

		#region TestCustomsEntryNumberFilters

		public void TestCustomsEntryNumberFilters()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			CusEntryNumber number1 = shipment1.Numbers.AddNew();
			number1.CE_EntryType = "COC";
			number1.CE_EntryNum = "111";
			number1.CE_Category = "CUS";
			number1.CE_EntryIsSystemGenerated = true;
			number1.CE_IssueDate = new ZDateTime(2007, 1, 1);

			BusinessObject declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			BusinessObject entryHeader1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusEntryHeader>();
			declaration1[JobDeclarationSchema.JE_JS] = shipment1.PK;
			entryHeader1[CusEntryHeaderSchema.CH_JE] = declaration1[JobDeclarationSchema.PK];
			number1[CusEntryNumSchema.CE_ParentID] = entryHeader1[CusEntryHeaderSchema.PK];
			number1.CE_ParentTable = entryHeader1.TableName;

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			CusEntryNumber number2 = shipment2.Numbers.AddNew();
			number2.CE_EntryType = "DLV";
			number2.CE_EntryNum = "122";
			number2.CE_Category = "CUS";
			number2.CE_EntryIsSystemGenerated = true;
			number2.CE_IssueDate = new ZDateTime(2007, 1, 1);
			BusinessObject declaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			BusinessObject entryHeader2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusEntryHeader>();
			declaration2[JobDeclarationSchema.JE_JS] = shipment2.PK;
			entryHeader2[CusEntryHeaderSchema.CH_JE] = declaration2[JobDeclarationSchema.PK];
			number2[CusEntryNumSchema.CE_ParentID] = entryHeader2[CusEntryHeaderSchema.PK];
			number2.CE_ParentTable = entryHeader2.TableName;

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ModuleNumberFilter customsEntryNoFilter = (ModuleNumberFilter)FilterBO["Customs Entry #"];
			customsEntryNoFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			customsEntryNoFilter.Property = "";
			customsEntryNoFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			customsEntryNoFilter.Property = "1";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			customsEntryNoFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			customsEntryNoFilter.Property = "122";
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
		}

		#endregion

		#region Test Order Number Filter

		public void TestOrderNumberFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			Order order1 = Factory.NewWithValidTestData<Order>();
			Order order2 = Factory.NewWithValidTestData<Order>();
			order1.JD_JS = shipment1.PK;
			order2.JD_JS = shipment2.PK;
			order1.JD_OrderNumber = "909090";
			order2.JD_OrderNumber = "909091";

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Order #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "9";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "909091";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "C";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#region TestTransportBookingReference Filter

		public void TestTransportBookingReferenceFilter()
		{
			var shipment1 = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var shipment2 = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			var consolidationBooking1 = Factory.New<IDtbBookingConsolidation>();
			var consolidationBooking2 = Factory.New<IDtbBookingConsolidation>();
			var consolidationBooking3 = Factory.New<IDtbBookingConsolidation>();
			var consolidationBooking4 = Factory.New<IDtbBookingConsolidation>();
			consolidationBooking1.KB_ParentID = shipment1.PK;
			consolidationBooking1.KB_ParentTableCode = shipment1.TablePrefix;
			consolidationBooking2.KB_ParentID = shipment2.PK;
			consolidationBooking2.KB_ParentTableCode = shipment2.TablePrefix;

			var booking1 = Factory.New<IDtbBooking>();
			var booking2 = Factory.New<IDtbBooking>();
			var booking3 = Factory.New<IDtbBooking>();
			var booking4 = Factory.New<IDtbBooking>();
			booking1.KM_KB_Booking = consolidationBooking1.PK;
			booking2.KM_KB_Booking = consolidationBooking2.PK;
			booking3.KM_KB_Booking = consolidationBooking3.PK;
			booking4.KM_KB_Booking = consolidationBooking4.PK;

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;
			Job3.JH_ParentID = booking3.PK;
			Job4.JH_ParentID = booking4.PK;

			booking1.KM_TransportReference = "A1";
			booking2.KM_TransportReference = "A2";
			booking3.KM_TransportReference = "A1i";
			booking4.KM_TransportReference = "A2i";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["Transport Booking Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A1";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job4", !FilterCollection.Contains(Job4));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "B";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection not to contain Job3", !FilterCollection.Contains(Job3));
			Assert("Expecting collection not to contain Job4", !FilterCollection.Contains(Job4));
		}

		#endregion

		#region Test Consignment RunSheet Number Filter

		public void TestConsignmentRunSheetNumberFilter()
		{
			var consignmentHelper = new TransportConsignmentTestHelper(Factory);

			var consignment1 = consignmentHelper.CreateConsignment("LTC001");
			var consignment1PickupAddress = consignmentHelper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var consignment1PickupAction = consignmentHelper.CreateConsignmentAction(consignment1PickupAddress, ActionTypes.Codes.PickUp);
			var consignment1DeliveryAddress = consignmentHelper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.Delivery);
			var consignment1DeliveryAction = consignmentHelper.CreateConsignmentAction(consignment1DeliveryAddress, ActionTypes.Codes.Delivery);
			Job1.JH_ParentID = consignment1.PK;

			var consignment2 = consignmentHelper.CreateConsignment("LTC002");
			var consignment2PickupAddress = consignmentHelper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.PickUp);
			var consignment2PickupAction = consignmentHelper.CreateConsignmentAction(consignment2PickupAddress, ActionTypes.Codes.PickUp);
			var consignment2DeliveryAddress = consignmentHelper.CreateConsignmentAddress(consignment2, ConsignmentAddressTypes.Codes.Delivery);
			var consignment2DeliveryAction = consignmentHelper.CreateConsignmentAction(consignment2DeliveryAddress, ActionTypes.Codes.Delivery);
			Job2.JH_ParentID = consignment2.PK;

			var runSheet1 = consignmentHelper.CreateRunSheet(runSheetNumber: "A1");
			consignmentHelper.CreateRunSheetInstruction(consignment1PickupAction, runSheet1.PK);
			consignmentHelper.CreateRunSheetInstruction(consignment1DeliveryAction, runSheet1.PK);

			var runSheet2 = consignmentHelper.CreateRunSheet(runSheetNumber: "A2");
			consignmentHelper.CreateRunSheetInstruction(consignment2PickupAction, runSheet2.PK);
			consignmentHelper.CreateRunSheetInstruction(consignment2DeliveryAction, runSheet2.PK);

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterBO["RunSheetNumber"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { Job1.PK, Job2.PK }, FilterCollection.GetPKs());

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "A1";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection contain 1 jobs.", 1, FilterCollection.Count);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { Job1.PK }, FilterCollection.GetPKs());

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "B";
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals("Expecting collection empty", 0, FilterCollection.Count);
		}

		#endregion

		#region Test Carrier

		public void TestCarrier()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			Job1.JH_ParentID = shipment1.PK;
			Job2.JH_ParentID = shipment2.PK;

			Factory.Save();

			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			consol1.SetDefaultShippingLineAddress(org1);

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			consol2.SetDefaultShippingLineAddress(org2);

			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();

			JobConShipLink jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			JobConShipLink jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();

			jobConShipLink1.JN_JK = consol1.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			jobConShipLink1.JN_JS = shipment1.PK;
			jobConShipLink2.JN_JS = shipment2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Carrier/Principal"];
			filter.Property = org1.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.Property = org2.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));

			filter.Property = org3.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#endregion

		#region Flags Filter Tests

		public void TestJobNoRevenueRecognitionDateFilter()
		{
			JobChargeRevRecognition revRecog1 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRecog2 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			JobChargeRevRecognition revRecog3 = Factory.NewWithValidTestData<JobChargeRevRecognition>();
			revRecog1.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.JobClosure;
			revRecog2.D3_RecognitionDate = new ZDateTime(2000, 2, 2, 22, 0, 0);
			revRecog3.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.CustomsClearanceDate;
			revRecog1.D3_JH = Job1.PK;
			revRecog2.D3_JH = Job2.PK;
			revRecog3.D3_JH = Job3.PK;

			Factory.Save();

			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterBO["No Recognized Date"];

			filter.Property0 = ZBool.True;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));

			filter.Property0 = ZBool.False;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection to contain Job2", FilterCollection.Contains(Job2));
			Assert("Expecting collection to contain Job3", FilterCollection.Contains(Job3));
			Assert("Expecting collection to contain Job4", FilterCollection.Contains(Job4));
		}

		#endregion

		#region Reference Filter Tests

		public void TestJobLocalClientFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			Job1.LocalChargesPK = org1.PK;
			Job2.LocalChargesPK = org2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Local Client"];
			filter.Property = org1.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.Property = org3.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		public void TestJobOverseasAgent()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org3 = Factory.NewWithValidTestData<OrgHeader>();
			Job1.AgentCollectPK = org1.PK;
			Job2.AgentCollectPK = org2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Overseas Agent"];
			filter.Property = org1.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Job1", FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));

			filter.Property = org3.PK;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Job1", !FilterCollection.Contains(Job1));
			Assert("Expecting collection not to contain Job2", !FilterCollection.Contains(Job2));
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<JobHeader>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.JobManagementCRMSecurity);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobManagementFilterBusinessObjectBase();
		}

		protected JobManagement Job1
		{
			get { return job1 ?? (job1 = CreateJob()); }
		}
		JobManagement job1;

		protected JobManagement Job2
		{
			get { return job2 ?? (job2 = CreateJob()); }
		}
		JobManagement job2;

		protected JobManagement Job3
		{
			get { return job3 ?? (job3 = CreateJob()); }
		}
		JobManagement job3;

		protected JobManagement Job4
		{
			get { return job4 ?? (job4 = CreateJob()); }
		}
		JobManagement job4;

		JobManagement CreateJob()
		{
			JobManagement job = null;
			job = Factory.NewWithValidTestData<JobManagement>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			// JobHeader is not valid with empty JH_Status.
			job.JH_Status = JobHeaderStatus.Working.Code;
			return job;
		}

		protected JobManagementCollection FilterCollection;
		JobManagementFilterBusinessObjectBase FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			var poke1 = Job1;
			var poke2 = Job2;
			var poke3 = Job3;
			var poke4 = Job4;

			FilterCollection = new JobManagementCollection(Factory);
			FilterBO = (JobManagementFilterBusinessObjectBase)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
