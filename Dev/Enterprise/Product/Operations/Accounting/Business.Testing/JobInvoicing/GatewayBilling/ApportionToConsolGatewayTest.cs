using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing
{
	public class ApportionToConsolGatewayTest : TestCaseWithFactory
	{
		public void TestApportionmentToConsolOneCharge()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_OSCostAmt = 0m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";

				Assert(!gtwCharge.IsRevenuePosted);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				s2ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s2ASCharge.JR_Calc_RelatedJobNumberInfo);

				Assert(!gtwCharge.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwCharge.JR_E6_GatewaySellHeader, consolCost.PK);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);
				AssertEquals(1, c1Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);						//s2Job is going to be null after another fix we are doing in parallel

				var s1Charge = s1Job.Charges[0];
				var c1Charge = c1Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(1111m, c1Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(c1Charge.JR_IsApportioned);
				Assert(c1Charge.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0002", c1Charge.JR_Calc_RelatedJobNumber);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(gtwCharge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journals = Factory.Load<JobRevenueJournal>(journalsQuery);
				var journalLines = journals.SelectMany(x => x.Lines).Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 3333m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -1111m).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();

				foreach (var journal in journals)
				{
					var reversing = reversingFactory.NewReversing(journal);
					reversing.Reverse();
				}

				Factory.Save();

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);
				AssertEquals(0, s3Job.Charges.Count);
				AssertEquals(0, c1Job.Charges.Count);

				listing.ReleaseMutexes();
			}
		}

		public void TestApportionToMultipleConsolsChoice()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD		SGSIN	-	HKHKG	-	USLAX
			//					\		/				 \
			//					   AUMEL					C0005
			//				gC0011								\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|
			//
			var setup = creator.CreateGatewayConsolsAndShipments();
			var c0011 = creator.CreateGatewayConsol("AUSYD", "AUMEL", "C0011", sendingGatewayCompany: GlbCompany.CurrentCompany,voyageFlight: "QF107", prepaidCollect: Enterprise.Core.Constants.PaymentType.Collect);

			var sf = setup.gC0002.SendingForwarder;
			var appointedPortsForSendingAgent = sf.AppointedGatewayAgentPorts.AddNew();
			appointedPortsForSendingAgent.O5_OA_AgentOfficeAddress = sf.MainAddress.PK;
			appointedPortsForSendingAgent.O5_PortOrCountry = "AUMEL";
			appointedPortsForSendingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPortsForSendingAgent.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			setup.gC0002.JK_RL_NKLoadPort = "AUMEL";
			setup.gC0002.JK_OA_SendingForwarderAddress = sf.MainAddress.PK;
			c0011.Shipments.Add(setup.s0001);
			c0011.Shipments.Add(setup.s0002);
			c0011.Shipments.Add(setup.s0003);

			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_OSCostAmt = 0m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				var expected = new[]
				{
					("S0001", "AUSYD - USLAX"),
					("C0011", "AUSYD - AUMEL"),
					("C0002", "AUMEL - SGSIN")
				};

				var expected2 = new[]
				{
					("S0002", "AUBNE - USNYC"),
					("C0001", "AUBNE - AUSYD"),
					("C0011", "AUSYD - AUMEL"),
					("C0002", "AUMEL - SGSIN")
				};

				var expected3 = new[]
				{
					("S0003", "AUBNE - SGSIN"),
					("C0001", "AUBNE - AUSYD"),
					("C0011", "AUSYD - AUMEL"),
					("C0002", "AUMEL - SGSIN")
				};

				var actual = s1ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				var actual2 = s2ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				var actual3 = s3ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				AssertArrayEqualsByElements(expected, actual);
				AssertArrayEqualsByElements(expected2, actual2);
				AssertArrayEqualsByElements(expected3, actual3);

				s2ASCharge.JR_JobNumber = "C0001";
				s3ASCharge.JR_JobNumber = "C0011";

				Assert(!gtwCharge.IsRevenuePosted);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				s2ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s2ASCharge.JR_Calc_RelatedJobNumberInfo);

				s3ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0003");

				s3ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s3ASCharge.JR_Calc_RelatedJobNumberInfo);

				Assert(!gtwCharge.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwCharge.JR_E6_GatewaySellHeader, consolCost.PK);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;
				var c11Job = new JobHeader.Loader(c0011).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, c1Job.Charges.Count);
				AssertEquals(1, c11Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);               //s2Job is going to be null after another fix we are doing in parallel
				AssertEquals(0, s3Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var c1Charge = c1Job.Charges[0];
				var s3Charge = c11Job.Charges[0];

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(1111m, c1Charge.JR_OSCostAmt);
				AssertEquals(1111m, s3Charge.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(c1Charge.JR_IsApportioned);
				Assert(c1Charge.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0002", c1Charge.JR_Calc_RelatedJobNumber);

				Assert(s3Charge.JR_IsApportioned);
				Assert(s3Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(gtwCharge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journals = Factory.Load<JobRevenueJournal>(journalsQuery);
				var journalLines = journals.SelectMany(x => x.Lines).Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 3333m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -1111m).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();

				foreach (var journal in journals)
				{
					var reversing = reversingFactory.NewReversing(journal);
					reversing.Reverse();
				}

				Factory.Save();

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);
				AssertEquals(0, s3Job.Charges.Count);
				AssertEquals(0, c1Job.Charges.Count);
				AssertEquals(0, c11Job.Charges.Count);

				listing.ReleaseMutexes();
			}
		}

		public void TestApportionmentToConsolTwoCharges()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwChargeFRT = gatewayJob.Charges.AddNew();
				gtwChargeFRT.JR_AC = creator.FRT.PK;
				gtwChargeFRT.JR_OH_SellAccount = setup.senAg.PK;
				gtwChargeFRT.JR_OSSellAmt = 3333m;
				gtwChargeFRT.JR_OSCostAmt = 0m;
				gtwChargeFRT.JR_RX_NKSellCurrency = "AUD";
				gtwChargeFRT.JR_JH_InternalJob = gatewayJob.PK;
				gtwChargeFRT.JR_GB_InternalBranch = gtwChargeFRT.JR_GB;
				gtwChargeFRT.JR_GE_InternalDept = gtwChargeFRT.JR_GE;

				var gtwChargeCC1 = gatewayJob.Charges.AddNew();
				gtwChargeCC1.JR_AC = creator.CC1.PK;
				gtwChargeCC1.JR_OH_SellAccount = setup.senAg.PK;
				gtwChargeCC1.JR_OSSellAmt = 999m;
				gtwChargeCC1.JR_OSCostAmt = 0m;
				gtwChargeCC1.JR_RX_NKSellCurrency = "AUD";
				gtwChargeCC1.JR_JH_InternalJob = gatewayJob.PK;
				gtwChargeCC1.JR_GB_InternalBranch = gtwChargeFRT.JR_GB;
				gtwChargeCC1.JR_GE_InternalDept = gtwChargeFRT.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCostFRT = Factory.Load<JobConsolCost>(gtwChargeFRT.JR_E6_GatewaySellHeader);
				var consolCostCC1 = Factory.Load<JobConsolCost>(gtwChargeCC1.JR_E6_GatewaySellHeader);
				var apportionmentChargesFRT = consolCostFRT.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();
				var apportionmentChargesCC1 = consolCostCC1.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentChargesFRT.Count);
				AssertEquals(3, apportionmentChargesCC1.Count);

				var s1ASChargeFRT = apportionmentChargesFRT.Single(x => x.JR_HouseBill == "S0001");
				var s2ASChargeFRT = apportionmentChargesFRT.Single(x => x.JR_HouseBill == "S0002");
				var s3ASChargeFRT = apportionmentChargesFRT.Single(x => x.JR_HouseBill == "S0003");

				var s1ASChargeCC1 = apportionmentChargesCC1.Single(x => x.JR_HouseBill == "S0001");
				var s2ASChargeCC1 = apportionmentChargesCC1.Single(x => x.JR_HouseBill == "S0002");
				var s3ASChargeCC1 = apportionmentChargesCC1.Single(x => x.JR_HouseBill == "S0003");

				s2ASChargeFRT.JR_JobNumber = "C0001";
				s3ASChargeCC1.JR_JobNumber = "C0001";

				Assert(!gtwChargeFRT.IsRevenuePosted);
				Assert(!gtwChargeCC1.IsRevenuePosted);

				Factory.Save();

				s2ASChargeFRT = consolCostFRT.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				s3ASChargeCC1 = consolCostCC1.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0003");

				s2ASChargeFRT.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s2ASChargeFRT.JR_Calc_RelatedJobNumberInfo);

				s3ASChargeCC1.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s3ASChargeCC1.JR_Calc_RelatedJobNumberInfo);

				Assert(!gtwChargeFRT.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwChargeFRT.JR_E6_GatewaySellHeader, consolCostFRT.PK);
				AssertEquals(3333m, consolCostFRT.E6_OSCostAmount);

				Assert(!gtwChargeCC1.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwChargeCC1.JR_E6_GatewaySellHeader, consolCostCC1.PK);
				AssertEquals(999m, consolCostCC1.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;

				AssertEquals(2, s1Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);
				AssertEquals(2, c1Job.Charges.Count);
				AssertEquals(1, s2Job.Charges.Count);

				var s1ChargeFRT = s1Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.FRT.AC_Code);
				var c1ChargeFRT = c1Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.FRT.AC_Code);
				var s3ChargeFRT = s3Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.FRT.AC_Code);

				var s1ChargeCC1 = s1Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.CC1.AC_Code);
				var c1ChargeCC1 = c1Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.CC1.AC_Code);
				var s2ChargeCC1 = s2Job.Charges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == creator.CC1.AC_Code);

				AssertEquals(1111m, s1ChargeFRT.JR_OSCostAmt);
				AssertEquals(1111m, c1ChargeFRT.JR_OSCostAmt);
				AssertEquals(1111m, s3ChargeFRT.JR_OSCostAmt);

				AssertEquals(333m, s1ChargeCC1.JR_OSCostAmt);
				AssertEquals(333m, c1ChargeCC1.JR_OSCostAmt);
				AssertEquals(333m, s2ChargeCC1.JR_OSCostAmt);

				Assert(s1ChargeFRT.JR_IsApportioned);
				Assert(s1ChargeFRT.IsCostPosted);
				Assert(s1ChargeFRT.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1ChargeFRT.JR_Calc_RelatedJobNumber);

				Assert(c1ChargeFRT.JR_IsApportioned);
				Assert(c1ChargeFRT.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0002", c1ChargeFRT.JR_Calc_RelatedJobNumber);

				Assert(s3ChargeFRT.JR_IsApportioned);
				Assert(s3ChargeFRT.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1ChargeFRT.JR_Calc_RelatedJobNumber);

				Assert(s1ChargeCC1.JR_IsApportioned);
				Assert(s1ChargeCC1.IsCostPosted);
				Assert(s1ChargeCC1.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1ChargeCC1.JR_Calc_RelatedJobNumber);

				Assert(c1ChargeCC1.JR_IsApportioned);
				Assert(c1ChargeCC1.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0003", c1ChargeCC1.JR_Calc_RelatedJobNumber);

				Assert(s2ChargeCC1.JR_IsApportioned);
				Assert(s2ChargeCC1.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s2ChargeCC1.JR_Calc_RelatedJobNumber);

				Assert(gtwChargeFRT.IsRevenuePostedWithAutoJobRevenueJournal);
				Assert(gtwChargeCC1.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(2, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journals = Factory.Load<JobRevenueJournal>(journalsQuery);
				var journalLines = journals.SelectMany(x => x.Lines).Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 3333m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -1111m).Count());

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 999m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -333m).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();

				foreach (var journal in journals)
				{
					var reversing = reversingFactory.NewReversing(journal);
					reversing.Reverse();
				}

				Factory.Save();

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);
				AssertEquals(0, s3Job.Charges.Count);
				AssertEquals(0, c1Job.Charges.Count);
			}
		}

		public void TestApportionmentMoreThanOneChargeToConsol()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_OSCostAmt = 0m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";
				s3ASCharge.JR_JobNumber = "C0001";

				Assert(!gtwCharge.IsRevenuePosted);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				s2ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s2ASCharge.JR_Calc_RelatedJobNumberInfo);

				s3ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0003");

				s3ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s3ASCharge.JR_Calc_RelatedJobNumberInfo);

				Assert(!gtwCharge.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwCharge.JR_E6_GatewaySellHeader, consolCost.PK);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(0, s3Job?.Charges?.Count ?? 0);
				AssertEquals(2, c1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);                        //s2Job is going to be null after another fix we are doing in parallel

				var s1Charge = s1Job.Charges[0];
				var c1ChargeForS2 = c1Job.Charges.Cast<Charge>().Single(x => x.JR_Calc_RelatedJobNumber == "S0002");
				var c1ChargeForS3 = c1Job.Charges.Cast<Charge>().Single(x => x.JR_Calc_RelatedJobNumber == "S0003");

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(1111m, c1ChargeForS2.JR_OSCostAmt);
				AssertEquals(1111m, c1ChargeForS3.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(c1ChargeForS2.JR_IsApportioned);
				Assert(c1ChargeForS2.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0002", c1ChargeForS2.JR_Calc_RelatedJobNumber);

				Assert(c1ChargeForS3.JR_IsApportioned);
				Assert(c1ChargeForS3.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(gtwCharge.IsRevenuePostedWithAutoJobRevenueJournal);
				AssertEquals(1, gatewayJob.Charges.Count);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gatewayJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journals = Factory.Load<JobRevenueJournal>(journalsQuery);
				var journalLines = journals.SelectMany(x => x.Lines).Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 3333m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -1111m).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();

				foreach (var journal in journals)
				{
					var reversing = reversingFactory.NewReversing(journal);
					reversing.Reverse();
				}

				Factory.Save();

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gatewayJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);
				AssertEquals(0, s3Job.Charges.Count);
				AssertEquals(0, c1Job.Charges.Count);

				listing.ReleaseMutexes();
			}
		}

		public void TestApportionmentToSameGatewayConsol()
		{
			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "PPD", "SHP", "ALL", "RGT"));
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gtwJob = creator.CreateJob(setup.gC0002))
			{
				var gtwSellCharge = gtwJob.Charges.AddNew();
				gtwSellCharge.JR_AC = creator.FRT.PK;
				gtwSellCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwSellCharge.JR_OSSellAmt = 3333m;
				gtwSellCharge.JR_OSCostAmt = 0m;
				gtwSellCharge.JR_RX_NKSellCurrency = "AUD";
				gtwSellCharge.JR_JH_InternalJob = gtwJob.PK;
				gtwSellCharge.JR_GB_InternalBranch = gtwSellCharge.JR_GB;
				gtwSellCharge.JR_GE_InternalDept = gtwSellCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwSellCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";

				setup.recAg.OH_IsDebtor = true;
				Assert(s3ASCharge.JR_OH_SellAccount != setup.recAg.PK);
				s3ASCharge.JR_JobNumber = "C0002";              //self-apportioned to the same gateway consol
				Assert("Debtor got reset to receiving agent according to the above registry", s3ASCharge.JR_OH_SellAccount == setup.recAg.PK);

				Assert(!gtwSellCharge.IsRevenuePosted);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				s3ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				s3ASCharge.Validation.ValidateJR_Calc_RelatedJobNumber();
				AssertNoNotifications("Lookups.RelatedJobNumbers should correctly have ApportionSplitCharge.ShipmentInfo on the list", s2ASCharge.JR_Calc_RelatedJobNumberInfo);
				AssertNoNotifications(s3ASCharge.JR_Calc_RelatedJobNumberInfo);

				Assert(!gtwSellCharge.JR_E6_GatewaySellHeader.IsEmpty);
				AssertEquals(gtwSellCharge.JR_E6_GatewaySellHeader, consolCost.PK);
				AssertEquals(3333m, consolCost.E6_OSCostAmount);

				var s1Job = new Job.Loader(setup.s0001).Load();
				var s2Job = new Job.Loader(setup.s0002).Load();
				var s3Job = new Job.Loader(setup.s0003).Load();
				var c1Job = new Job.Loader(setup.gC0001).Load();

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(2, gtwJob.Charges.Count);
				AssertEquals(0, s3Job?.Charges?.Count ?? 0);
				AssertEquals(1, c1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);                        //s2Job is going to be null after another fix we are doing in parallel

				var s1Charge = s1Job.Charges[0];
				var c1ChargeForS2 = c1Job.Charges[0];
				var c2ChargeForS3 = gtwJob.Charges.Where(x => x.PK != gtwSellCharge.PK).Single();

				AssertEquals(1111m, s1Charge.JR_OSCostAmt);
				AssertEquals(1111m, c1ChargeForS2.JR_OSCostAmt);
				AssertEquals(1111m, c2ChargeForS3.JR_OSCostAmt);

				Assert(s1Charge.JR_IsApportioned);
				Assert(s1Charge.IsCostPosted);
				Assert(s1Charge.IsCostPostedWithJobRevenueJournal);
				AssertNullOrEmpty(s1Charge.JR_Calc_RelatedJobNumber);

				Assert(c1ChargeForS2.JR_IsApportioned);
				Assert(c1ChargeForS2.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0002", c1ChargeForS2.JR_Calc_RelatedJobNumber);

				Assert(c2ChargeForS3.JR_IsApportioned);
				Assert(c2ChargeForS3.IsCostPostedWithJobRevenueJournal);
				AssertEquals("S0003", c2ChargeForS3.JR_Calc_RelatedJobNumber);

				Assert(gtwSellCharge.IsRevenuePostedWithAutoJobRevenueJournal);

				//check created JRJ

				var journalsQuery = new ZQuery(AccTransactionHeaderSchema.PK, gtwJob.Charges.Cast<JobCharge>().Select(x => x.ARLine?.AL_AH).WhereNotNull().ToArray());
				var journal = Factory.Load<JobRevenueJournal>(journalsQuery).Single();
				var journalLines = journal.Lines.Cast<JobRevenueJournalLine>().ToArray();

				AssertEquals(1, journalLines.Where(x => x.AL_OSAmount == 3333m).Count());
				AssertEquals(3, journalLines.Where(x => x.AL_OSAmount == -1111m).Count());

				//reverse JRJ
				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(journal);
				reversing.Reverse();
				Factory.Save();

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, setup.gC0002.PK));
				AssertEquals(0, consolCosts.Length);

				AssertEquals(0, gtwJob.Charges.Count);
				AssertEquals(0, s1Job.Charges.Count);
				AssertEquals(0, s2Job?.Charges?.Count ?? 0);
				AssertEquals(0, s3Job.Charges.Count);
				AssertEquals(0, c1Job.Charges.Count);

				listing.ReleaseMutexes();
			}
		}

		public void TestJobNumberDropDown()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				var expected = new[]
				{
					("S0001", "AUSYD - USLAX"),
					("C0002", "AUSYD - SGSIN")
				};

				var expected2 = new[]
				{
					("S0002", "AUBNE - USNYC"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN")
				};

				var expected3 = new[]
				{
					("S0003", "AUBNE - SGSIN"),
					("C0001", "AUBNE - AUSYD"),
					("C0002", "AUSYD - SGSIN")
				};

				var actual = s1ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				var actual2 = s2ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				var actual3 = s3ASCharge.Lookups.ApportionTargetJobNumbers.Cast<ICodeDescription>().Select(x => (x.Code, x.Description)).ToArray();
				AssertArrayEqualsByElements(expected, actual);
				AssertArrayEqualsByElements(expected2, actual2);
				AssertArrayEqualsByElements(expected3, actual3);

				s1ASCharge.Job.Dispose();
				s2ASCharge.Job.Dispose();
				s3ASCharge.Job.Dispose();
			}
		}

		public void TestJobNumberDropDown_NoNRE()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting1 = collection.AddNew();
			setting1.ConsolDirection = "ALL";
			setting1.ConsolTransportMode = "ALL";
			setting1.PreviousSendingAgentType = "ALL";
			setting1.InvoiceTargetJobType = "SCL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_IsDebtor = true;
			var otherCompanyBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");
			otherCompanyBranch.GB_OH_OrgProxy = consignee.PK;
			setup.s0002.ConsigneePK = consignee.PK;

			Factory.Save();

			using (var s2Job = creator.CreateJob(setup.s0002))
			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				s2Job.JH_OA_LocalChargesAddr = consignee.MainAddress.PK;

				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";  //when invoiceTarget is supported on the apportionSplitCharge then this would coause NRE in GatewayInvoiceTargetJobFinder.FindDefaultInvoiceTargetJobTypeBasedOnRelatedJobNumber()

				Assert(!gtwCharge.IsRevenuePosted);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);
				AssertEquals(1, c1Job.Charges.Count);
				AssertEquals(0, s2Job.Charges.Count);

				listing.ReleaseMutexes();
			}
		}

		public void TestApportionmentToConsolNoExtraJobsOnShipments()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";

				AssertEquals("S0002", s2ASCharge.JR_HouseBill);

				var jobParentPKs = new[]
				{
					setup.gC0001.PK,
					setup.gC0002.PK,
					setup.c0003.PK,
					setup.c0004.PK,
					setup.c0005.PK,
					setup.s0001.PK,
					setup.s0002.PK,
					setup.s0003.PK,
				};

				AssertEquals(0, new BusinessObjectFactory().Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, jobParentPKs)).Length);

				Factory.Save();

				var savedJobs = new BusinessObjectFactory().Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, jobParentPKs));

				var expectedNoExtraJobsSaved = new[]
				{
					"C0001",
					"C0002",
					"S0001",
					"S0003",
				};

				AssertContainsExactElementsInAnyOrder("S0002 job should not be saved because charge got apportioned to a consol C0001", expectedNoExtraJobsSaved, savedJobs.Select(x => x.JH_JobNum));
				listing.ReleaseMutexes();
			}
		}

		public void TestApportionmentToConsolNoExtraJobsOnConsols()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";
				AssertEquals(setup.s0002, s2ASCharge.RelatedJob);
				AssertEquals("S0002", s2ASCharge.JR_HouseBill);

				s2ASCharge.JR_JobNumber = "S0002";

				var jobParentPKs = new[]
				{
					setup.gC0001.PK,
					setup.gC0002.PK,
					setup.c0003.PK,
					setup.c0004.PK,
					setup.c0005.PK,
					setup.s0001.PK,
					setup.s0002.PK,
					setup.s0003.PK,
				};

				AssertEquals(0, new BusinessObjectFactory().Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, jobParentPKs)).Length);

				Factory.Save();

				var savedJobs = new BusinessObjectFactory().Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, jobParentPKs));

				var expectedNoExtraJobsSaved = new[]
				{
					"C0002",
					"S0001",
					"S0002",
					"S0003",
				};

				AssertContainsExactElementsInAnyOrder("C0001 job should have been disposed", expectedNoExtraJobsSaved, savedJobs.Select(x => x.JH_JobNum));
			}
		}

		public void TestApportionmentToConsolMethods()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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

			var s1line1 = setup.s0001.OuterPackLines.AddNew();
			var s2line1 = setup.s0002.OuterPackLines.AddNew();
			var s3line1 = setup.s0003.OuterPackLines.AddNew();
			var s3line2 = setup.s0003.OuterPackLines.AddNew();

			s1line1.JL_PackageCount = 9;
			s2line1.JL_PackageCount = 19;
			s3line1.JL_PackageCount = 10;
			s3line2.JL_PackageCount = 15;
			s1line1.JL_F3_NKPackType = s2line1.JL_F3_NKPackType = s3line1.JL_F3_NKPackType = s3line2.JL_F3_NKPackType = "PLT";

			s1line1.JL_ActualWeight = 100;
			s2line1.JL_ActualWeight = 200;
			s3line1.JL_ActualWeight = 500;
			s3line2.JL_ActualWeight = 200;
			s1line1.JL_ActualWeightUQ = s2line1.JL_ActualWeightUQ = s3line1.JL_ActualWeightUQ = s3line2.JL_ActualWeightUQ = "KG";

			setup.s0001.JS_ActualWeight = 100;
			setup.s0002.JS_ActualWeight = 200;
			setup.s0003.JS_ActualWeight = 700;
			setup.s0001.JS_UnitOfWeight = setup.s0002.JS_UnitOfWeight = setup.s0003.JS_UnitOfWeight = "KG";

			s1line1.JL_ActualVolume = 2;
			s2line1.JL_ActualVolume = 3;

			s3line1.JL_ActualVolume = 1;
			s3line2.JL_ActualVolume = 4;
			s1line1.JL_ActualVolumeUQ = s2line1.JL_ActualVolumeUQ = s3line1.JL_ActualVolumeUQ = s3line2.JL_ActualVolumeUQ = "M3";

			setup.s0001.JS_ActualVolume = 2;
			setup.s0002.JS_ActualVolume = 3;
			setup.s0003.JS_ActualVolume = 5;
			setup.s0001.JS_UnitOfVolume = setup.s0002.JS_UnitOfVolume = setup.s0003.JS_UnitOfVolume = "M3";

			var c20gp = setup.gC0002.Containers.AddNew();
			var c40gp = setup.gC0002.Containers.AddNew();
			c20gp.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			c40gp.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;

			c20gp.PackLines.Add(s1line1);
			c40gp.PackLines.Add(s2line1);
			c20gp.PackLines.Add(s3line1);
			c40gp.PackLines.Add(s3line2);

			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 10000;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				s2ASCharge.JR_JobNumber = "C0001";

				consolCost.E6_ApportionmentMethod = AllocationMethod.GrossWeight;
				consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.ChargeableUnits s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.ChargeableUnits s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.ChargeableUnits s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.GrossWeight;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.GrossWeight s1ASCharge.JR_OSCostAmt", 1000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.GrossWeight s2ASCharge.JR_OSCostAmt", 2000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.GrossWeight s3ASCharge.JR_OSCostAmt", 7000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.GrossVolume;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.GrossVolume s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.GrossVolume s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.GrossVolume s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.ContainerCount;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.ContainerCount s1ASCharge.JR_OSCostAmt", 2500m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.ContainerCount s2ASCharge.JR_OSCostAmt", 2500m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.ContainerCount s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.TwentyFootEquivalentUnit;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.TwentyFootEquivalentUnit s1ASCharge.JR_OSCostAmt", 1666.67m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.TwentyFootEquivalentUnit s2ASCharge.JR_OSCostAmt", 3333.33m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.TwentyFootEquivalentUnit s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.OuterPackTotal;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.OuterPackTotal s1ASCharge.JR_OSCostAmt", 1698.11m, s1ASCharge.JR_OSCostAmt);		// 9/53 * 10000
					AssertEquals("AllocationMethod.OuterPackTotal s2ASCharge.JR_OSCostAmt", 3584.91m, s2ASCharge.JR_OSCostAmt);		// 19/53 * 10000
					AssertEquals("AllocationMethod.OuterPackTotal s3ASCharge.JR_OSCostAmt", 4716.98m, s3ASCharge.JR_OSCostAmt);		// 25/53 * 10000
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.Shipment s1ASCharge.JR_OSCostAmt", 3333.34m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.Shipment s2ASCharge.JR_OSCostAmt", 3333.33m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.Shipment s3ASCharge.JR_OSCostAmt", 3333.33m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_ApportionmentMethod = AllocationMethod.FreeSpaceContribution;
				CombineAssertions(() =>
				{
					AssertEquals("AllocationMethod.FreeSpaceContribution s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.FreeSpaceContribution s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("AllocationMethod.FreeSpaceContribution s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
				});

				consolCost.E6_PPDCLT = "CCX";
				CombineAssertions(() =>
				{
					AssertEquals("CCX s1ASCharge.JR_OSCostAmt", 0m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("CCX s2ASCharge.JR_OSCostAmt", 0m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("CCX s3ASCharge.JR_OSCostAmt", 0m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("CCX s1ASCharge.JR_IsUsedForApportionment", false, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("CCX s2ASCharge.JR_IsUsedForApportionment", false, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("CCX s3ASCharge.JR_IsUsedForApportionment", false, s3ASCharge.JR_IsUsedForApportionment);
				});

				consolCost.E6_PPDCLT = "LOG";
				CombineAssertions(() =>
				{
					AssertEquals("LOG s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("LOG s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("LOG s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("LOG s1ASCharge.JR_IsUsedForApportionment", true, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("LOG s2ASCharge.JR_IsUsedForApportionment", true, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("LOG s3ASCharge.JR_IsUsedForApportionment", true, s3ASCharge.JR_IsUsedForApportionment);
				});

				consolCost.E6_PPDCLT = "FOG";
				CombineAssertions(() =>
				{
					AssertEquals("FOG s1ASCharge.JR_OSCostAmt", 0m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("FOG s2ASCharge.JR_OSCostAmt", 0m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("FOG s3ASCharge.JR_OSCostAmt", 0m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("FOG s1ASCharge.JR_IsUsedForApportionment", false, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("FOG s2ASCharge.JR_IsUsedForApportionment", false, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("FOG s3ASCharge.JR_IsUsedForApportionment", false, s3ASCharge.JR_IsUsedForApportionment);
				});

				consolCost.E6_PPDCLT = "FDT";
				CombineAssertions(() =>
				{
					AssertEquals("FDT s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("FDT s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("FDT s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("FDT s1ASCharge.JR_IsUsedForApportionment", true, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("FDT s2ASCharge.JR_IsUsedForApportionment", true, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("FDT s3ASCharge.JR_IsUsedForApportionment", true, s3ASCharge.JR_IsUsedForApportionment);
				});

				consolCost.E6_PPDCLT = "LDT";
				CombineAssertions(() =>
				{
					AssertEquals("LDT s1ASCharge.JR_OSCostAmt", 0m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("LDT s2ASCharge.JR_OSCostAmt", 0m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("LDT s3ASCharge.JR_OSCostAmt", 0m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("LDT s1ASCharge.JR_IsUsedForApportionment", false, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("LDT s2ASCharge.JR_IsUsedForApportionment", false, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("LDT s3ASCharge.JR_IsUsedForApportionment", false, s3ASCharge.JR_IsUsedForApportionment);
				});

				consolCost.E6_PPDCLT = "FDT";           // this one wouldn't be needed if not for the bug introduced by 32266 changeset
				consolCost.E6_PPDCLT = "PPD";
				CombineAssertions(() =>
				{
					AssertEquals("PPD s1ASCharge.JR_OSCostAmt", 2000m, s1ASCharge.JR_OSCostAmt);
					AssertEquals("PPD s2ASCharge.JR_OSCostAmt", 3000m, s2ASCharge.JR_OSCostAmt);
					AssertEquals("PPD s3ASCharge.JR_OSCostAmt", 5000m, s3ASCharge.JR_OSCostAmt);
					AssertEquals("PPD s1ASCharge.JR_IsUsedForApportionment", true, s1ASCharge.JR_IsUsedForApportionment);
					AssertEquals("PPD s2ASCharge.JR_IsUsedForApportionment", true, s2ASCharge.JR_IsUsedForApportionment);
					AssertEquals("PPD s3ASCharge.JR_IsUsedForApportionment", true, s3ASCharge.JR_IsUsedForApportionment);
				});

				s1ASCharge.Job.Dispose();
				s2ASCharge.Job.Dispose();
				s3ASCharge.Job.Dispose();

				setup.gC0002.GetApportionments(true).ReleaseMutexes();
			}
		}

		public void TestApportionmentToConsol_ChargeBranchDepartment()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				AssertEquals(3, apportionmentCharges.Count);

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				AssertEquals("Branch is Shipment2's", "BNE", s2ASCharge.Branch.GB_Code);
				AssertEquals("Dept is Shipment2's", "FEA", s2ASCharge.Department.GE_Code);

				//Apportion one of the charges to a consol
				s2ASCharge.JR_JobNumber = "C0001";

				AssertEquals("Branch is Consol1's", "SYD", s2ASCharge.Branch.GB_Code);
				AssertEquals("Dept is Consol1's", "GDA", s2ASCharge.Department.GE_Code);
				AssertEquals("S0002", s2ASCharge.JR_Calc_RelatedJobNumber);

				Factory.Save();

				s2ASCharge = consolCost.ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Single(x => x.JR_HouseBill == "S0002");

				var s1Job = new JobHeader.Loader(setup.s0001).Load() as Job;
				var s2Job = new JobHeader.Loader(setup.s0002).Load() as Job;
				var s3Job = new JobHeader.Loader(setup.s0003).Load() as Job;
				var c1Job = new JobHeader.Loader(setup.gC0001).Load() as Job;

				AssertEquals(1, s1Job.Charges.Count);
				AssertEquals(1, s3Job.Charges.Count);
				AssertEquals(1, c1Job.Charges.Count);

				var s1Charge = s1Job.Charges[0];
				var c1Charge = c1Job.Charges[0];
				var s3Charge = s3Job.Charges[0];

				AssertEquals("Same branch after save", "SYD", c1Charge.Branch.GB_Code);
				AssertEquals("Save dept after save", "GDA", c1Charge.Department.GE_Code);
				AssertEquals("SYD", s1Charge.Branch.GB_Code);
				AssertEquals("FEA", s1Charge.Department.GE_Code);
				AssertEquals("BNE", s3Charge.Branch.GB_Code);
				AssertEquals("FEA", s3Charge.Department.GE_Code);

				s1Job.Dispose();
				s2Job.Dispose();
				s3Job.Dispose();
				c1Job.Dispose();
			}
		}

		public void TestApportionmentToConsol_RelatedJobSetAndClear()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var creator = new TestObjectCreator(Factory);

			//			gC0001		gC0002		C0003		C0004
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
			Factory.Save();

			using (var gatewayJob = creator.CreateJob(setup.gC0002))
			{
				var gtwCharge = gatewayJob.Charges.AddNew();
				gtwCharge.JR_AC = creator.FRT.PK;
				gtwCharge.JR_OH_SellAccount = setup.senAg.PK;
				gtwCharge.JR_OSSellAmt = 3333m;
				gtwCharge.JR_RX_NKSellCurrency = "AUD";
				gtwCharge.JR_JH_InternalJob = gatewayJob.PK;
				gtwCharge.JR_GB_InternalBranch = gtwCharge.JR_GB;
				gtwCharge.JR_GE_InternalDept = gtwCharge.JR_GE;

				var listing = setup.gC0002.GetApportionments(true);
				listing.PrepareForConsolCosting();
				var consolCost = Factory.Load<JobConsolCost>(gtwCharge.JR_E6_GatewaySellHeader);
				var apportionmentCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList();

				var s1ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0001");
				var s2ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0002");
				var s3ASCharge = apportionmentCharges.Single(x => x.JR_HouseBill == "S0003");

				var s1Job = s1ASCharge.InvoicingJob;
				var s2Job = s2ASCharge.InvoicingJob;
				var s3Job = s3ASCharge.InvoicingJob;

				AssertEquals(string.Empty, s2ASCharge.JR_Calc_RelatedJobNumber);

				//Apportion one of the charges to a consol
				s2ASCharge.JR_JobNumber = "C0001";
				AssertEquals("S0002", s2ASCharge.JR_Calc_RelatedJobNumber);

				var c1Job = s2ASCharge.InvoicingJob;

				s2ASCharge.JR_JobNumber = "C0002";
				AssertEquals("S0002", s2ASCharge.JR_Calc_RelatedJobNumber);

				var c2Job = s2ASCharge.InvoicingJob;

				s2ASCharge.JR_JobNumber = "S0002";
				AssertEquals(string.Empty, s2ASCharge.JR_Calc_RelatedJobNumber);

				s2ASCharge.JR_JobNumber = "C0002";
				AssertEquals("S0002", s2ASCharge.JR_Calc_RelatedJobNumber);

				c1Job.Dispose();
				c2Job.Dispose();
				s1Job.Dispose();
				s2Job.Dispose();
				s3Job.Dispose();
			}
		}
	}
}

