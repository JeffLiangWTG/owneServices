namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Accounting.Utility.Testing;
	using Enterprise.Core;
	using Enterprise.Customs.Business;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Freight.QuotedBookings.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.TransportConsignment.Business.Testing;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	public class JobClosureProcessorTestHelper : TestCaseWithFactory
	{
		protected Job CreateJobForAWB(string shipmentNo, string consolNo, ZGuid companyPK, ZGuid branchPK, ZDateTime aWB)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", consolNo);
			var shipment = TestObjectCreator.CreateShipment(shipmentNo, consol);
			consol.JK_MasterBillIssueDate = aWB;

			Job testJob = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = shipment;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateJobForCUS(string shipmentNo, ZGuid companyPK, ZGuid branchPK, ZDateTimeOffset cUS)
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNo, "AUSYD", "USLAX");
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.Logs.AddNew(Events.ExportCustomsCleared, cUS.AddDays(-1));
			shipment.Logs.AddNew(Events.CustomsCleared, cUS);
			var testJob = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = shipment;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateBRKJobForCUS(string jobNo, ZGuid companyPK, ZGuid branchPK, ZDateTimeOffset cUS)
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.Logs.AddNew(Events.ExportCustomsCleared, cUS.AddDays(-1));
			dec.Logs.AddNew(Events.CustomsCleared, cUS);

			var testJob = TestObjectCreator.CreateJob(dec, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = dec;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateSHPJobForPICAndDEL(string shipmentNo, ZGuid companyPK, ZGuid branchPK, ZDateTime e_PIC, ZDateTime e_DEL, ZDateTime pIC, ZDateTime dEL, string transportMode = "AIR")
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNo, "AUSYD", "USLAX");
			shipment.JS_TransportMode = transportMode;
			var testJob = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = shipment;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;
			shipment.DocsAndCartage.JP_EstimatedPickup = e_PIC;
			shipment.DocsAndCartage.JP_EstimatedDelivery = e_DEL;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = pIC;
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = dEL;

			return testJob;
		}

		protected Job CreateTCWJobForPICAndDEL(string consignmentNo, ZGuid companyPK, ZGuid branchPK, ZDateTimeOffset e_PIC, ZDateTimeOffset e_DEL, ZDateTimeOffset pIC, ZDateTimeOffset dEL)
		{
			var helper = new TransportBookingConsignmentTestHelper(Factory);
			var consignment = helper.CreateBookingConsignment();
			consignment.KM_JobID = consignmentNo;
			consignment.Logs.AddNew(Events.PickedUp, e_PIC, true);
			consignment.Logs.AddNew(Events.PickedUp, pIC, false);
			consignment.Logs.AddNew(Events.Delivered, e_DEL, true);
			consignment.Logs.AddNew(Events.Delivered, dEL, false);

			var testJob = TestObjectCreator.CreateJob(consignment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = consignment;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateLTCJobForPICAndDEL(string consignmentNo, ZGuid companyPK, ZGuid branchPK, ZDateTimeOffset e_PIC, ZDateTimeOffset e_DEL, ZDateTimeOffset pIC, ZDateTimeOffset dEL)
		{
			var helper = new TransportConsignmentTestHelper(Factory);
			var consignment = helper.CreateConsignment(consignmentNo);
			consignment.LTC_JobID = consignmentNo;
			consignment.Logs.AddNew(Events.PickedUp, e_PIC, true);
			consignment.Logs.AddNew(Events.PickedUp, pIC, false);
			consignment.Logs.AddNew(Events.Delivered, e_DEL, true);
			consignment.Logs.AddNew(Events.Delivered, dEL, false);

			var testJob = TestObjectCreator.CreateJob(consignment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.Parent = consignment;
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateJobForVADAndVDD(ZGuid companyPK, ZGuid branchPK, ZDateTime vDD, ZDateTime vAD)
		{
			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USLAX";
			var origin = Factory.NewWithValidTestData<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destn = Factory.NewWithValidTestData<VoyageDestination>();
			destn.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = billOfLading.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destn.PK;
			billOfLading.JS_JX = sailing.PK;

			Job testJob = TestObjectCreator.CreateJob(billOfLading, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;
			destn.JB_A_ARV = vAD;
			origin.JA_A_DEP = vDD;

			return testJob;
		}

		protected Job CreateJobForARVAndDEP(bool isFCNJobType, string shipmentNo, string consolNo, ZGuid companyPK, ZGuid branchPK, ZDateTime eTD, ZDateTime eTA, ZDateTime aTD, ZDateTime aTA)
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", consolNo);
			ForwardingShipment shipment = null;
			if (!isFCNJobType)
			{
				shipment = TestObjectCreator.CreateShipment(shipmentNo, consol);
			}
			var transport = consol.Transports.AddNew();
			consol.JK_IsCFS = !isFCNJobType;
			SetTransportValue(transport, "AUSYD", "USLAX", eTD, eTA, aTD, aTA);

			var testJob = isFCNJobType ? TestObjectCreator.CreateJobForLegacyGateway(consol) : TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			return testJob;
		}

		protected Job CreateJobForFAR(JobInvoicingConsumerType jobType, ZGuid companyPK, ZGuid branchPK, ZDateTime fAR)
		{
			var plugin = TestObjectCreator.CreateJobPlugIn(jobType);
			var testJob = TestObjectCreator.CreateJob(plugin, TestObjectCreator.LocalClient, 5M, TestObjectCreator.Agent, 10M);
			testJob.JH_GC = companyPK;
			testJob.JH_GB = branchPK;

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			invoice.AH_PostDate = fAR;
			invoice.AH_GC = companyPK;
			invoice.AH_GB = branchPK;

			var invoiceLine = TestObjectCreator.CreateARInvoiceLine(invoice, testJob, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Charge Desc. 1", 123.0m);
			invoiceLine.AL_PostDate = fAR;
			invoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			invoiceLine.AL_GC = companyPK;
			invoiceLine.AL_GB = branchPK;

			TestObjectCreator.CreateJobCharge(invoiceLine, testJob, TestObjectCreator.CC1, TestObjectCreator.AUD);
			testJob.Charges.Load();

			return testJob;
		}

		protected Job CreateJob(JobInvoicingConsumerType jobType, ZGuid companyPK, ZGuid branchPK, ZDateTime jOP)
		{
			IJobInvoicingPlugIn plugin = null;
			switch (jobType.Code)
			{
				case "QSH":
					plugin = QuotedBooking.CreateNewBooking(Factory);
					break;
				case "FCN":
					plugin = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", TestObjectCreator.GetRandomString(5));
					break;
				case "GCN":
				case "YRA":
				case "YRE":
				case "YTU":
				case "YAO":
				case "MWO":
				case "YPI":
					plugin = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", TestObjectCreator.GetRandomString(5), receivingGatewayCompany: GlbCompany.CurrentCompany);
					break;
				case "ORG":
					plugin = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", TestObjectCreator.GetRandomString(5), receivingGatewayCompany: GlbCompany.CurrentCompany);
					break;
				default:
					plugin = TestObjectCreator.CreateJobPlugIn(jobType);
					break;
			}

			if (plugin != null)
			{
				Job job;

				switch (jobType.Code)
				{
					case "FCN":
						job = TestObjectCreator.CreateJobForLegacyGateway((ForwardingConsol)plugin);
						break;
					default:
						job = TestObjectCreator.CreateJob(plugin, false);
						break;
				}

				job.JH_GC = companyPK;
				job.JH_GB = branchPK;
				job.JH_A_JOP = jOP;

				return job;
			}

			return null;
		}

		protected Job CreateConsolJob(bool isCLLJobType, string consolNo, ZGuid companyPK, ZGuid branchPK, ZDateTime jOP, string transportMode = "AIR")
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", consolNo);
			consol.JK_TransportMode = transportMode;
			consol.JK_IsCFS = isCLLJobType;
			var job = TestObjectCreator.CreateJobForLegacyGateway(consol);
			job.JH_GC = companyPK;
			job.JH_GB = branchPK;
			job.JH_A_JOP = jOP;
			return job;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor, RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, string invoiceType, ZGuid branchPK)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			if (desc != null)
			{
				charge.JR_Desc = desc;
			}

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency != null ? costCurrency.RX_Code : ZString.Empty;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_InvoiceType = invoiceType;

			charge.JR_OSSellAmt = oSSellAmt;
			charge.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge.JR_GB = branchPK;
			return charge;
		}

		protected void SetTransportValue(Transport transport, ZString homePort, ZString overseasPort, ZDateTime eTD, ZDateTime eTA, ZDateTime aTD, ZDateTime aTA)
		{
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsCharter = true;
			transport.JW_RL_NKLoadPort = homePort;
			transport.JW_RL_NKDiscPort = overseasPort;
			transport.JW_ETD = eTD;
			transport.JW_ETA = eTA;
			transport.JW_ATD = aTD;
			transport.JW_ATA = aTA;
			transport.JW_VoyageFlight = "1235222";
			transport.JW_JX_JV_RegistrationNo = "999AI88";
		}

		protected void SetRegistryValue(Guid companyPK, params JobClosureConfiguration[] config)
		{
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(companyPK, Guid.Empty, Guid.Empty, regValue);
		}

		protected GlbCompany[] CreateCompany(int noOfCompany)
		{
			var companies = new GlbCompany[noOfCompany];
			var proxies = new OrgHeader[noOfCompany];

			for (int i = 0; i < noOfCompany; i++)
			{
				proxies[i] = TestObjectCreator.CreateOrgHeader("PRX" + i.ToString(), true, true);
			}
			Factory.Save();

			for (int i = 0; i < noOfCompany; i++)
			{
				companies[i] = TestObjectCreator.CreateNewCompany("CO" + i.ToString(), orgProxy: proxies[i]);
			}
			return companies;
		}

		protected GlbBranch[] CreateBranch(GlbCompany[] companies)
		{
			GlbBranch[] branches = new GlbBranch[companies.Length];
			for (int i = 0; i < companies.Length; i++)
			{
				branches[i] = TestObjectCreator.CreateBranch("B" + i.ToString(), companies[i]);
			}
			return branches;
		}

		protected ForwardingConsol SetUpConsol(ForwardingShipment shipment1, ForwardingShipment shipment2, AccChargeCode chargeCode)
		{
			//Creating Consol Test
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);

			//ConsolCost1
			JobConsolCost costCC1 = listing.CostsCollection.TryAddNew();
			costCC1.E6_AC_ChargeCode = chargeCode.PK;
			costCC1.E6_OSCostAmount = 50m;

			//s1
			costCC1.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[0].JR_E6 = costCC1.PK;
			AddJobCharge((shipment1.Job as Job), costCC1);

			//s2
			costCC1.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			costCC1.ApportionmentCharges[1].JR_E6 = costCC1.PK;
			AddJobCharge((shipment2.Job as Job), costCC1);

			return consol;
		}

		protected ForwardingConsol SetUpConsol()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			shipment1.JS_ActualWeight = 151.73m;
			shipment1.JS_ActualChargeable = 13m;
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job1.Charges.RemoveAll();

			var shipment2 = TestObjectCreator.CreateShipment("S0002", "AUSYD", "USLAX");
			shipment2.JS_ActualWeight = 251.73m;
			shipment2.JS_ActualChargeable = 23m;
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job2.Charges.RemoveAll();

			return SetUpConsol(shipment1, shipment2, TestObjectCreator.CC1);
		}

		protected void AssertNumbeOfRows(string sqlText, string message, int expectedNumberOfRows)
		{
			using (var cmd = Db.Connection.Command(sqlText))
			{
				var ob = cmd.ExecuteScalar();
				AssertEquals(message, expectedNumberOfRows, Convert.ToInt32(ob));
			}
		}

		protected void AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(LoggerForTesting logger, Job job)
		{
			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(new[] { logger }, new[] { job });
		}

		protected void AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(LoggerForTesting logger, IEnumerable<Job> jobs, IEnumerable<ZGuid> expectedJobPKs = null)
		{
			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(new[] { logger }, jobs, expectedJobPKs);
		}

		protected void AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(IEnumerable<LoggerForTesting> loggers, Job job)
		{
			AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(loggers, new[] { job });
		}

		protected void AssertJobAutoClosuresMatchedBetweenLoggersAndJobs(IEnumerable<LoggerForTesting> loggers, IEnumerable<Job> jobs, IEnumerable<ZGuid> expectedJobPKs = null)
		{
			var messagesFromLogs = GetSuccessfulJobAutoClosureMessagesFromLoggers(loggers).OrderBy(x => x).ToArray();
			var messagesFromJobs = GetJobAutoClosureMessagesFromJobs(jobs, expectedJobPKs).OrderBy(x => x.LogMessage).ToArray();

			AssertEquals(messagesFromLogs.Length, messagesFromJobs.Length);  // ensure that the collection are the same length before zipping them
			foreach (var message in messagesFromJobs.Zip(messagesFromLogs, (fromJob, fromLog) => new { fromJob, fromLog }))
			{
				AssertEndsWith("Job.ReferenceFreeText does not end with the close reason from the log.", ". " + message.fromLog, message.fromJob.LogMessage);  // the Job.ReferenceFreeText it the concatenation of the original message and the new log derived message
			}
		}

		protected void AssertJobs(HashSet<ZGuid> expectedJobs, bool assertWhetherLogExist = false)
		{
			var jobs = new BusinessObjectFactory().Load<Job>(new ZQuery(JobHeaderSchema.JH_Status, "CLS"));
			AssertJobs(expectedJobs, jobs, assertWhetherLogExist);
		}

		protected void AssertJobs(HashSet<ZGuid> expectedJobs, IEnumerable<Job> jobs, bool assertWhetherLogExist = false, string message = "")
		{
			var jobPKs = jobs.Select(j => j.PK);
			AssertContainsExactElementsInAnyOrder(message, expectedJobs, jobPKs);
			if (assertWhetherLogExist)
			{
				AssertWhetherLogExist(expectedJobs, true);
			}
		}

		protected void AssertJobCloseDate(Job job, ZDateTime expectedCloseDate)
		{
			job.Reload();
			AssertEquals("Job Close Date", expectedCloseDate, job.JH_A_JCL);
		}

		protected void AssertLogExist(HashSet<ZGuid> jobs)
		{
			AssertWhetherLogExist(jobs, true);
		}

		protected void AssertLogDoesNotExist(HashSet<ZGuid> jobs)
		{
			AssertWhetherLogExist(jobs, false);
		}

		void AssertWhetherLogExist(HashSet<ZGuid> jobs, bool isExist)
		{
			using (var cmd = Db.Connection.Command(string.Format(@"SELECT COUNT(*) FROM dbo.stmALog WHERE	SL_Parent IN ('{0}') 
																										AND SL_SE_NKEvent = 'JCL'
																										AND SL_Table = 'JobHeader'
																										AND SL_Reference Like '%to CLS via the ‘Bulk Job Close’ menu Option'", string.Join("', '", jobs.Select(x => x.ToString()).ToArray()))))
			{
				int rowCount;
				Assert("Log Exist", int.TryParse(cmd.ExecuteScalar().ToString(), out rowCount));
				AssertEquals("Log Count", isExist ? jobs.Count : 0, rowCount);
			}
		}

		protected void CreateAndPostCharge(Job job, AccChargeCode code, string transactionNo, string description, ZGuid companyPK, ZGuid branchPK)
		{
			var charge = CreateCharge(job, code, description, TestObjectCreator.AUD, 16.0m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 16.0m, TestObjectCreator.Debtor, InvoiceTypesList.Codes.FinalInvoice, branchPK);
			charge.ChargeCode.AC_GC = companyPK;
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), transactionNo, TestObjectCreator.AUD, 1.0m);
			var apline = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, description, 16.0m);

			apline.AL_AT = charge.JR_AT_CostGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_CostVATClass = ZGuid.Empty;
			charge.JR_AL_APLine = apline.PK;

			var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNo + "_AR", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			var arline = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, description, 16.0m);

			arline.AL_AT = charge.JR_AT_SellGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_SellVATClass = ZGuid.Empty;
			charge.JR_AL_ARLine = arline.PK;
		}

		protected void AddJobCharge(Job job, JobConsolCost cost)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_AC = cost.ChargeCode.PK;
		}

		public TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		public static string[] AllowedJobTypeList
		{
			get
			{
				return new string[]
					{
						JobInvoicingConsumerTypes.Shipment.Code,
						JobInvoicingConsumerTypes.Brokerage.Code,
						JobInvoicingConsumerTypes.PostClearanceBrokerage.Code,
						JobInvoicingConsumerTypes.MasterAWB.Code,
						JobInvoicingConsumerTypes.CFSShipment.Code,
						JobInvoicingConsumerTypes.CFSLoadList.Code,
						JobInvoicingConsumerTypes.FCLStorage.Code,
						JobInvoicingConsumerTypes.LocalCartage.Code,
						JobInvoicingConsumerTypes.AgentBooking.Code,
						JobInvoicingConsumerTypes.TransportBooking.Code,
						JobInvoicingConsumerTypes.TransportBookingWithAgent.Code,
						JobInvoicingConsumerTypes.WarehouseInwards.Code,
						JobInvoicingConsumerTypes.WarehouseOutwards.Code,
						JobInvoicingConsumerTypes.WarehouseStorage.Code,
						JobInvoicingConsumerTypes.WarehouseStocktake.Code,
						JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code,
						JobInvoicingConsumerTypes.WarehouseVASOrder.Code,
						JobInvoicingConsumerTypes.CusMAWB.Code,
						JobInvoicingConsumerTypes.CusUnderbond.Code,
						JobInvoicingConsumerTypes.CTOCusMAWB.Code,
						JobInvoicingConsumerTypes.CTOCusImportHAWB.Code,
						JobInvoicingConsumerTypes.CTOCusExportHAWB.Code,
						JobInvoicingConsumerTypes.AgencyBillOfLading.Code,
						JobInvoicingConsumerTypes.AgencyBooking.Code,
						JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code,
						JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code,
						JobInvoicingConsumerTypes.AgencySundryCharges.Code,
						JobInvoicingConsumerTypes.ImporterSecurityFiling.Code,
						JobInvoicingConsumerTypes.eManifest.Code,
						JobInvoicingConsumerTypes.CAeManifest.Code,
						JobInvoicingConsumerTypes.TransportBookingConsignment.Code,
						JobInvoicingConsumerTypes.TransportConsignment.Code,
						JobInvoicingConsumerTypes.TransitReceive.Code,
						JobInvoicingConsumerTypes.TransitDispatch.Code,
						JobInvoicingConsumerTypes.TransitDispatchLoadList.Code,
						JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code,
						JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code,
						JobInvoicingConsumerTypes.ForwardingConsol.Code,
						JobInvoicingConsumerTypes.QuotedBooking.Code,
						JobInvoicingConsumerTypes.GatewayConsol.Code,
						JobInvoicingConsumerTypes.CYDReceiveAdviceJobCode,
						JobInvoicingConsumerTypes.CYDReleaseAdviceJobCode,
						JobInvoicingConsumerTypes.CYDTransportationUnitJobCode,
						JobInvoicingConsumerTypes.CYDAdHocServiceOrderJobCode,
						JobInvoicingConsumerTypes.MNRWorkOrderHeaderJobCode,
						JobInvoicingConsumerTypes.WorkItemCode,
						JobInvoicingConsumerTypes.CustomsTransitNCTSCode,
						JobInvoicingConsumerTypes.BRLPCOCode,
						JobInvoicingConsumerTypes.CustomsTemporaryStorageCode,
						JobInvoicingConsumerTypes.CYDPeriodicInvoicingJobCode,
					};
			}
		}

		const string autoJobClosureEligibilityVerificationDetails = "Auto Job Closure eligibility verification details -";
		const string autoUpdateJobStatusToJFCEligibilityVerificationDetails = "Auto Update Job Status To JFC eligibility verification details -";
		static readonly Regex jobPrefixRegex = new Regex(@"^\[(?<companyCode>[^]]*)\]\[(?<jobCode>[^]]*)\]\[(?<updaterCode>[^]]*)\]:\s*(?<content>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		static readonly Regex jobAutoUpdateAttemptRegex = new Regex(@"^\[(?<jobCode>[^]]*)\]:\s*(?<content>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		static readonly Regex jobAutoClosureSuccessRegex = new Regex(@"^Status of (?<jobCode>\S+) is changed from '(?<oldStatus>[^']*)' to 'CLS' and Job Close Date is updated to (?<date>.*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		static readonly Regex jobAutoUpdateJobStatusToJFCSuccessRegex = new Regex(@"^Status of (?<jobCode>\S+) is changed from '(?<oldStatus>[^']*)' to 'JFC'", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		const string jobAutoClosureFailure = "This job cannot be closed.";
		const string jobAutoUpdateJobStatusToJFCFailure = "Job status cannot be updated to JFC.";

		IEnumerable<string> GetSuccessfulJobAutoClosureMessagesFromLoggers(IEnumerable<LoggerForTesting> loggers)
		{
			MatchCollection matchCollection;
			var jobAutoUpdateAttempts = new Dictionary<string, string>();
			var notifiedEvents = loggers.SelectMany(x => x.NotifiedEventList);

			foreach (var notifiedEvent in notifiedEvents)
			{
				if ((matchCollection = jobPrefixRegex.Matches(notifiedEvent)).Count == 1)
				{
					var jobCode = matchCollection[0].Groups["jobCode"].Value.Trim();
					var content = matchCollection[0].Groups["content"].Value.Trim();
					var isAutoClosed = false;
					if ((isAutoClosed = content.StartsWith(autoJobClosureEligibilityVerificationDetails)) || content.StartsWith(autoUpdateJobStatusToJFCEligibilityVerificationDetails))
					{
						var details = content
							.Substring(isAutoClosed ? autoJobClosureEligibilityVerificationDetails.Length : autoUpdateJobStatusToJFCEligibilityVerificationDetails.Length).Trim()
							.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => string.Join(" ", x.Split(new[] { '\r', '\n' }, StringSplitOptions.None).Select(y => y.Trim()).Where(y => !string.IsNullOrEmpty(y))).Trim());

						foreach (var detail in details)
						{
							if ((matchCollection = jobAutoUpdateAttemptRegex.Matches(detail)).Count == 1)
							{
								// this job is added to the jobs that will be (attempted to be) auto closed
								var jobAutoUpdateJobCode = matchCollection[0].Groups["jobCode"].Value.Trim();
								var jobAutoUpdateContent = matchCollection[0].Groups["content"].Value.Trim();
								jobAutoUpdateAttempts.Add(jobAutoUpdateJobCode, jobAutoUpdateContent);
							}
						}
					}
					else if (content.StartsWith(jobAutoClosureFailure) || content.StartsWith(jobAutoUpdateJobStatusToJFCFailure))
					{
						if (jobAutoUpdateAttempts.ContainsKey(jobCode))
						{
							// removed because it failed
							jobAutoUpdateAttempts.Remove(jobCode);
						}
					}
					else if ((matchCollection = jobAutoClosureSuccessRegex.Matches(content)).Count == 1 || (matchCollection = jobAutoUpdateJobStatusToJFCSuccessRegex.Matches(content)).Count == 1)
					{
						var jobAutoClosureSuccessJobCode = matchCollection[0].Groups["jobCode"].Value.Trim();
						if (jobAutoUpdateAttempts.ContainsKey(jobAutoClosureSuccessJobCode))
						{
							// this job has completed successfully and is returned and removed
							var result = jobAutoUpdateAttempts[jobAutoClosureSuccessJobCode];
							jobAutoUpdateAttempts.Remove(jobAutoClosureSuccessJobCode);
							yield return result;
						}
					}
				}
			}
		}

		IEnumerable<(ZGuid JobPK, string LogMessage)> GetJobAutoClosureMessagesFromJobs(IEnumerable<Job> jobs, IEnumerable<ZGuid> expectedJobPKs = null)
		{
			foreach (var job in jobs)
			{
				job.Reload();
				var found = false;
				var logs = job.Logs.GetAllLogs().OfType<StmALog>();
				var edts = logs.Where(x => string.Compare("EDT", x.Event.SE_Code, StringComparison.OrdinalIgnoreCase) == 0);
				var refs = edts.Select(x => (string)x.ReferenceFreeText);
				var vals = refs.Where(x => !string.IsNullOrEmpty(x));
				foreach (var val in vals)
				{
					if (val.EndsWith("Satisfied registry settings."))
					{
						found = true;
						yield return (job.PK, val);
					}
				}

				if (expectedJobPKs.Contains(job.PK))
				{
					Assert($"Message:\r\n{logs}", found);
				}
			}
		}
	}
}
