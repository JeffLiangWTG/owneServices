using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AutoRateInvoicingStrategyTest : TestCaseWithFactory
	{
		#region Creditor Overrides

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_ShipmentWithoutConsol()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: ZGuid.Empty);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_ShipmentWithoutConsol_CreditorOverrideHasEmptyPaymentTerm()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: TestHelper.Creditor1.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_ShipmentWithMultipleConsols_SamePrepaidCollect()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = PaymentTerms.Prepaid;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = PaymentTerms.Prepaid;

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: TestHelper.Creditor1.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_ShipmentWithMultipleConsols_DifferentPrepaidCollect()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: Core.Constants.PaymentType.Collect, creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = PaymentTerms.Prepaid;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = PaymentTerms.Collect;

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: ZGuid.Empty);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_ShipmentWithMultipleConsols_DifferentPrepaidCollect_CreditorOverrideHasEmptyPaymentTerm()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: "ALL", creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_PrepaidCollect = PaymentTerms.Prepaid;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_PrepaidCollect = PaymentTerms.Collect;

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: TestHelper.Creditor1.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestHelper.Creditor1.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);
			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = "PPD";

			AssertCostAccount(job: null, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: TestHelper.Creditor1.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorOverride_PrioritizeNonEmptyDeparment()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestHelper.Creditor1.PK);
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditor: TestHelper.Creditor2.PK, department: GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);

			var job = Factory.NewJobForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_PrepaidCollect = "PPD";

			AssertCostAccount(job, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: TestHelper.Creditor2.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorRole_Consol_SendingAgent()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", creditorRole: DocAddressTypes.Codes.OverseasAgent);
			Factory.Save();

			var sendingAgent = TestHelper.CreateOrgHeader("Agent1", true, false);
			var shipment = TestHelper.CreateShipment("S001", origin: "USLAX", destination: "AUSYD", transportMode: Core.Constants.TransportModes.Air);
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var job = Factory.NewJobForTesting<Job>();
			job.Parent = consol;

			AssertCostAccount(job, consol, JobInvoicingConsumerTypes.ForwardingConsol, TestHelper.CC1.AC_Code, expectedCostAccount: sendingAgent.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorRole_Consol_ReceivingAgent()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ForwardingConsolCode, direction: "ALL", transportMode: "ALL", creditorRole: DocAddressTypes.Codes.OverseasAgent);
			Factory.Save();

			var receivingAgent = TestHelper.CreateOrgHeader("Agent1", true, false);
			var shipment = TestHelper.CreateShipment("S001", origin: "AUSYD", destination: "USLAX", transportMode: Core.Constants.TransportModes.Air);
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var job = Factory.NewJobForTesting<Job>();
			job.Parent = consol;

			AssertCostAccount(job, consol, JobInvoicingConsumerTypes.ForwardingConsol, TestHelper.CC1.AC_Code, expectedCostAccount: receivingAgent.PK);
		}

		public void TestAddAutoRates_CreditorOverrides_CreditorRole_Shipment()
		{
			TestHelper.CreateChargeCreditorOverride(TestHelper.CC1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", creditorRole: DocAddressTypes.Codes.OverseasAgent);
			Factory.Save();

			var shipment = TestHelper.CreateShipment("S001", origin: "AUSYD", destination: "USLAX", transportMode: Core.Constants.TransportModes.Air);
			var consol = shipment.Consols.AddNew();
			var collectAgent = TestHelper.CreateOrgHeader("Collect1", true, false);
			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.AgentCollectPK = collectAgent.PK;

			AssertCostAccount(job, shipment, JobInvoicingConsumerTypes.Shipment, TestHelper.CC1.AC_Code, expectedCostAccount: collectAgent.PK);
		}

		void AssertCostAccount(Job job, IJobHeaderParent jobHeaderParent, JobInvoicingConsumerType consumerType, string chargeCode, ZGuid expectedCostAccount)
		{
			AssertAddAutoRates
			(
				existingCharges: Array.Empty<BaseCharge>(),
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(chargeCode: chargeCode, costSell: CostSell.Cost, amount: 100m, consumerType: consumerType)
				},
				expectedResultCharges: new[]
				{
					new { ChargeCode = (ZString)chargeCode, JR_OH_CostAccount = expectedCostAccount },
				},
				propertiesToCompare: charge => new
				{
					ChargeCode = charge.ChargeCode.AC_Code,
					charge.JR_OH_CostAccount,
				},
				jobHeaderParent: jobHeaderParent,
				job: job
			);
		}

		#endregion

		public void TestShouldAddAutoRates()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			AutoRateInvoicingStrategy strategy = new AutoRateInvoicingStrategy(shipment, null);
			AssertEquals("Not added if Job is null", false, strategy.ShouldAddAutoRates);
			Job shipmentJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.JH_Status = JobHeaderStatus.Working.Code;
			strategy = new AutoRateInvoicingStrategy(shipment, shipmentJob);
			AssertEquals("Added if Job is not closed", true, strategy.ShouldAddAutoRates);
			shipmentJob.JH_Status = JobHeaderStatus.Closed.Code;
			Env.Security.ReopenJob.IsAllowed = false;
			strategy = new AutoRateInvoicingStrategy(shipment, shipmentJob);
			AssertEquals("Not Added if Job is closed and security denied", false, strategy.ShouldAddAutoRates);
			shipmentJob.JH_Status = JobHeaderStatus.Closed.Code;
			Env.Security.ReopenJob.IsAllowed = true;
			strategy = new AutoRateInvoicingStrategy(shipment, shipmentJob);
			AssertEquals("Added if Job is closed but security granted to re-open", true, strategy.ShouldAddAutoRates);
		}

		#region Match existing charges by ChargeCode

		public void TestAddAutoRates_Cost_UpdateMatchedExistingChargesOrCreateNew_MatchedByChargeCode()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(chargeCode: "FRT", costAmount: 100m),
					CreateCharge(chargeCode: "BAF", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(chargeCode: "FRT", costSell: CostSell.Cost, amount: 105m),
					CreateAutoRateInfo(chargeCode: "CAF", costSell: CostSell.Cost, amount: 300m)
				},
				expectedResultCharges: new[]
				{
					new { ChargeCode = (ZString)"FRT", JR_OSCostAmt = (ZDecimal)105m },
					new { ChargeCode = (ZString)"BAF", JR_OSCostAmt = (ZDecimal)200m },
					new { ChargeCode = (ZString)"CAF", JR_OSCostAmt = (ZDecimal)300m },
				},
				propertiesToCompare: charge => new
				{
					ChargeCode = charge.ChargeCode.AC_Code,
					charge.JR_OSCostAmt
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingChargesOrCreateNew_MatchedByChargeCode()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(chargeCode: "FRT", costAmount: 100m),
					CreateCharge(chargeCode: "BAF", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(chargeCode: "FRT", costSell: CostSell.Revenue, amount: 105m),
					CreateAutoRateInfo(chargeCode: "CAF", costSell: CostSell.Revenue, amount: 300m)
				},
				expectedResultCharges: new[]
				{
					new { ChargeCode = (ZString)"FRT", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)105m },
					new { ChargeCode = (ZString)"BAF", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)0m },
					new { ChargeCode = (ZString)"CAF", JR_OSCostAmt = (ZDecimal)300m, JR_OSSellAmt = (ZDecimal)300m },
				},
				propertiesToCompare: charge => new
				{
					ChargeCode = charge.ChargeCode.AC_Code,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt
				});
		}

		#endregion

		#region Match existing charges by OrderReference

		public void TestAddAutoRates_Cost_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReference()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(orderReference: "111", costAmount: 100m),
					CreateCharge(orderReference: "222", costAmount: 200m),
					CreateCharge(orderReference: string.Empty, costAmount: 400m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: string.Empty, costSell: CostSell.Cost, amount: 405m),
					CreateAutoRateInfo(orderReference: "333", costSell: CostSell.Cost, amount: 305m),
					CreateAutoRateInfo(orderReference: "111", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_OrderReference = (ZString)"111", JR_OSCostAmt = (ZDecimal)105m },
					new { JR_OrderReference = (ZString)"222", JR_OSCostAmt = (ZDecimal)200m },
					new { JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)405m },

					// Strange behavior but when adding costs, we don't populate order reference on newly created charges,
					// we only populate it when we add revenue (see the test for Revenue)
					new { JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)305m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OrderReference,
					charge.JR_OSCostAmt,
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReference()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(orderReference: "111", costAmount: 100m),
					CreateCharge(orderReference: "222", costAmount: 200m),
					CreateCharge(orderReference: string.Empty, costAmount: 400m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: string.Empty, costSell: CostSell.Revenue, amount: 405m),
					CreateAutoRateInfo(orderReference: "333", costSell: CostSell.Revenue, amount: 305m),
					CreateAutoRateInfo(orderReference: "111", costSell: CostSell.Revenue, amount: 105m)
				},
				expectedResultCharges: new[]
				{
					new { JR_OrderReference = (ZString)"111", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)105m },
					new { JR_OrderReference = (ZString)"222", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)0m },
					new { JR_OrderReference = (ZString)"333", JR_OSCostAmt = (ZDecimal)305m, JR_OSSellAmt = (ZDecimal)305m },
					new { JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)400m, JR_OSSellAmt = (ZDecimal)405m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OrderReference,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt
				});
		}

		public void TestAddAutoRates_Cost_ThereIsMatchingExistingChargeWithNoOrderReference_UpdateTheCharge()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(orderReference: "222", costAmount: 200m),
					CreateCharge(orderReference: string.Empty, costAmount: 100m),
					CreateCharge(orderReference: "333", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: "111", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_OrderReference = (ZString)"222", JR_OSCostAmt = (ZDecimal)200m },
					new { JR_OrderReference = (ZString)"333", JR_OSCostAmt = (ZDecimal)300m },

					// Strange behavior but when adding costs, we don't populate order reference on newly created charges,
					// we only populate it when we add revenue (see the test for Revenue)
					new { JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)105m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OrderReference,
					charge.JR_OSCostAmt
				}
			);
		}

		public void TestAddAutoRates_Revenue_ThereIsMatchingExistingChargeWithNoOrderReference_UpdateTheCharge()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(orderReference: "222", costAmount: 200m),
					CreateCharge(orderReference: string.Empty, costAmount: 100m),
					CreateCharge(orderReference: "333", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: "111", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_OrderReference = (ZString)"222", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)0m },
					new { JR_OrderReference = (ZString)"333", JR_OSCostAmt = (ZDecimal)300m, JR_OSSellAmt = (ZDecimal)0m },
					new { JR_OrderReference = (ZString)"111", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)105m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OrderReference,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt
				});
		}

		public void TestAddAutoRates_Cost_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReferenceContainingJobNum()
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "Job-1";

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "Job-2";

			var currentJob = Factory.NewJobForTesting<Job>();
			currentJob.JH_JobNum = "CurrentJob";

			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(job: job1, costAmount: 100m),
					CreateCharge(job: job2, costAmount: 200m),
					CreateCharge(job: currentJob, costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: string.Empty, costSell: CostSell.Cost, amount: 305m),
					CreateAutoRateInfo(orderReference: "Job-2", costSell: CostSell.Cost, amount: 205m),
					CreateAutoRateInfo(orderReference: "Job-1", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					// Strange behavior but when adding costs, we don't populate order reference on newly created charges,
					// we only populate it when we add revenue (see the test for Revenue)
					new { JR_JobNumber = (ZString)"Job-1", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)105m },
					new { JR_JobNumber = (ZString)"Job-2", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)205m },
					new { JR_JobNumber = (ZString)"CurrentJob", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)305m }
				},
				propertiesToCompare: charge => new
				{
					charge.JR_JobNumber,
					charge.JR_OrderReference,
					charge.JR_OSCostAmt
				},
				job: currentJob);
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReferenceContainingJobNum()
		{
			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_JobNum = "Job-1";

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_JobNum = "Job-2";

			var currentJob = Factory.NewJobForTesting<Job>();
			currentJob.JH_JobNum = "CurrentJob";

			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(job: job1, costAmount: 100m),
					CreateCharge(job: job2, costAmount: 200m),
					CreateCharge(costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: string.Empty, costSell: CostSell.Revenue, amount: 305m),
					CreateAutoRateInfo(orderReference: "Job-2", costSell: CostSell.Revenue, amount: 205m),
					CreateAutoRateInfo(orderReference: "Job-1", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_JobNumber = (ZString)"Job-1", JR_OrderReference = (ZString)"Job-1", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)105m },
					new { JR_JobNumber = (ZString)"Job-2", JR_OrderReference = (ZString)"Job-2", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)205m },
					new { JR_JobNumber = (ZString)"CurrentJob", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)300m, JR_OSSellAmt = (ZDecimal)305m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_JobNumber,
					charge.JR_OrderReference,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt
				},
				job: currentJob);
		}

		public void TestAddAutoRates_Cost_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReferenceContainingRelatedJobNum()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(relatedJobNumber: "Job-1", costAmount: 100m),
					CreateCharge(relatedJobNumber: "Job-2", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: "Job-1", costSell: CostSell.Cost, amount: 105m),
					CreateAutoRateInfo(orderReference: "333", costSell: CostSell.Cost, amount: 300m)
				},
				expectedResultCharges: new[]
				{
					// Strange behavior but when adding costs, we don't populate order reference on newly created charges,
					// we only populate it when we add revenue (see the test for Revenue)
					new { JR_Calc_RelatedJobNumber = (ZString)"Job-1", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)105m },
					new { JR_Calc_RelatedJobNumber = (ZString)"Job-2", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)200m },
					new { JR_Calc_RelatedJobNumber = ZString.Empty, JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)300m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_Calc_RelatedJobNumber,
					charge.JR_OrderReference,
					charge.JR_OSCostAmt
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingChargesOrCreateNew_MatchedByOrderReferenceContainingRelatedJobNum()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(relatedJobNumber: "Job-1", costAmount: 100m),
					CreateCharge(relatedJobNumber: "Job-2", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(orderReference: "333", costSell: CostSell.Revenue, amount: 300m),
					CreateAutoRateInfo(orderReference: "Job-1", costSell: CostSell.Revenue, amount: 105m),
					CreateAutoRateInfo(orderReference: "444", costSell: CostSell.Revenue, amount: 400m)
				},
				expectedResultCharges: new[]
				{
					new { JR_Calc_RelatedJobNumber = ZString.Empty, JR_OrderReference = (ZString)"333", JR_OSCostAmt = (ZDecimal)300m, JR_OSSellAmt = (ZDecimal)300m },
					new { JR_Calc_RelatedJobNumber = (ZString)"Job-1", JR_OrderReference = (ZString)"Job-1", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)105m },
					new { JR_Calc_RelatedJobNumber = (ZString)"Job-2", JR_OrderReference = ZString.Empty, JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)0m },
					new { JR_Calc_RelatedJobNumber = ZString.Empty, JR_OrderReference = (ZString)"444", JR_OSCostAmt = (ZDecimal)400m, JR_OSSellAmt = (ZDecimal)400m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_Calc_RelatedJobNumber,
					charge.JR_OrderReference,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt
				});
		}

		#endregion

		#region Match existing charges by Currency

		public void TestAddAutoRates_Cost_UpdateMatchedExistingCharges_MatchedByCurrency()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(costCurrency: "USD", costAmount: 100m),
					CreateCharge(costCurrency: "AUD", costAmount: 200m),
					CreateCharge(costCurrency: "UAH", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(currency: "EUR", costSell: CostSell.Cost, amount: 305m),
					CreateAutoRateInfo(currency: "AUD", costSell: CostSell.Cost, amount: 205m),
					CreateAutoRateInfo(currency: "USD", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_OSCostCurrencyCode = (ZString)"USD", JR_OSCostAmt = (ZDecimal)105m },
					new { JR_OSCostCurrencyCode = (ZString)"AUD", JR_OSCostAmt = (ZDecimal)205m },
					new { JR_OSCostCurrencyCode = (ZString)"EUR", JR_OSCostAmt = (ZDecimal)305m },
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OSCostCurrencyCode,
					charge.JR_OSCostAmt
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingCharges_MatchedByCurrency()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(costCurrency: "USD", costAmount: 100m),
					CreateCharge(costCurrency: "AUD", costAmount: 200m),
					CreateCharge(costCurrency: "UAH", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(currency: "EUR", costSell: CostSell.Revenue, amount: 305m),
					CreateAutoRateInfo(currency: "AUD", costSell: CostSell.Revenue, amount: 205m),
					CreateAutoRateInfo(currency: "USD", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { JR_OSCostCurrencyCode = (ZString)"USD", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellCurrencyCode = (ZString)"USD", JR_OSSellAmt = (ZDecimal)105m },
					new { JR_OSCostCurrencyCode = (ZString)"AUD", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellCurrencyCode = (ZString)"AUD", JR_OSSellAmt = (ZDecimal)205m },
					new { JR_OSCostCurrencyCode = (ZString)"UAH", JR_OSCostAmt = (ZDecimal)300m, JR_OSSellCurrencyCode = (ZString)"EUR", JR_OSSellAmt = (ZDecimal)305m }
				},
				propertiesToCompare: charge => new
				{
					charge.JR_OSCostCurrencyCode,
					charge.JR_OSCostAmt,
					charge.JR_OSSellCurrencyCode,
					charge.JR_OSSellAmt
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingCharges_ForeignCurrency_NoCriticalValidation()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			var consignor = TestHelper.Debtor;
			consignor.CompanyData.OB_IMBillAgentChargesDirect = true;
			consignor.CompanyData.OB_RX_NKARDDefltCurrency = TestHelper.LocalCurrency.RX_Code;

			var shipment = TestHelper.CreateShipment(shipmentNum: "1001");
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var job = TestHelper.CreateJob(shipment, TestHelper.LocalClient, 0m, consignor, 0m);

			var entity = job.PlugInData as IDocAddresses;
			if (entity != null)
			{
				entity.DocAddresses.AddNew(TestHelper.LocalClient.MainAddress, DocAddressType.ClientRequestedBillingParty);
			}

			TestHelper.CreateCharge(parentJob: job, chargeCode: TestHelper.FRT, "desc");
			job.LocalChargesPK = TestHelper.Debtor1.PK;
			job.AgentCollectPK = ZGuid.Empty;

			TestHelper.CreateExchangeRate(TestHelper.USD, "BUY", 2m);

			var exchangeRate = TestHelper.SetExchangeRate(job, TestHelper.USD, 3m, TestHelper.Debtor1.PK, ExchangeRateOrgTypeEnum.Debtor);

			Factory.Save();

			job = new BusinessObjectFactory().Load<Job>(job.PK);

			var autoRateResult = new AutoRateInfoCollection(Factory)
			{
				CreateAutoRateInfo(currency: "USD", costSell: CostSell.Revenue, amount: 105m),
			};

			var strategy = new AutoRateInvoicingStrategy(shipment, job);
			strategy.AddAutoRates(new TestInteractor(), autoRateResult, CostSell.Revenue, null);

			AssertNoExceptionThrown(() => job.Factory.Save());
		}

		#endregion

		#region Match existing charges by ContainerCode

		public void TestAddAutoRates_Cost_UpdateMatchedExistingCharges_MatchedByContainerCode()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: null, costAmount: 200m),
					CreateCharge(containerCode: "40GP", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "40HC", costSell: CostSell.Cost, amount: 405m),
					CreateAutoRateInfo(containerCode: null, costSell: CostSell.Cost, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)105m },
					new { ContainerCode = string.Empty, Cost = (ZDecimal)205m },
					new { ContainerCode = "40GP", Cost = (ZDecimal)300m },
					new { ContainerCode = "40HC", Cost = (ZDecimal)405m }
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt
				});

			// Not empty AutoRate info container code should match charge with empty container code
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: null, costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "40HC", costSell: CostSell.Cost, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)105m },
					new { ContainerCode = "40HC", Cost = (ZDecimal)205m }
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt
				});

			// Empty AutoRate info container code should match charge with non empty container code
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: "40GP", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: null, costSell: CostSell.Cost, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Cost, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)105m },
					new { ContainerCode = string.Empty, Cost = (ZDecimal)205m }
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt
				});
		}

		public void TestAddAutoRates_Revenue_UpdateMatchedExistingCharges_MatchedByContainerCode()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: null, costAmount: 200m),
					CreateCharge(containerCode: "40GP", costAmount: 300m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "40HC", costSell: CostSell.Revenue, amount: 405m),
					CreateAutoRateInfo(containerCode: null, costSell: CostSell.Revenue, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)100m, Revenue = (ZDecimal)105m },
					new { ContainerCode = string.Empty, Cost = (ZDecimal)200m, Revenue = (ZDecimal)205m },
					new { ContainerCode = "40GP", Cost = (ZDecimal)300m, Revenue = (ZDecimal)0m },
					new { ContainerCode = "40HC", Cost = (ZDecimal)405m, Revenue = (ZDecimal)405m },
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt,
					Revenue = charge.JR_OSSellAmt
				});

			// Not empty autorate info container code should match charge with empty container code
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: null, costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "40HC", costSell: CostSell.Revenue, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)100m, Revenue = (ZDecimal)105m },
					new { ContainerCode = "40HC", Cost = (ZDecimal)200m, Revenue = (ZDecimal)205m }
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt,
					Revenue = charge.JR_OSSellAmt
				});

			// Empty autorate info container code should match charge with non empty container code
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: "40GP", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: null, costSell: CostSell.Revenue, amount: 205m),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 105m),
				},
				expectedResultCharges: new[]
				{
					new { ContainerCode = "20GP", Cost = (ZDecimal)100m, Revenue = (ZDecimal)105m },

					// Not sure if this one is right. It is a side-effect of the way attributes on a charge are implemented.
					// Basically, we have a single set of attributes per charge while a charge consists of two parts - cost and revenue.
					// The way it is implemented that when we update existing charge, we overwrite all attributes using attributes from the new charge.
					// It makes sense if cost charge matches existing cost charge. But, I don't think it is right when a revenue charge overwrites
					// attributes set by cost charge like in this test case. It may lead to side effects.
					new { ContainerCode = string.Empty, Cost = (ZDecimal)200m, Revenue = (ZDecimal)205m },
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt,
					Revenue = charge.JR_OSSellAmt
				});

			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m),
					CreateCharge(containerCode: "40GP", costAmount: 200m),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: null, costSell: CostSell.Revenue, amount: 205m),
				},
				expectedResultCharges: new[]
				{
					// It is an old autorating logic. If a revenue charge is a generic one and calculated for multiple items (like containers in this case),
					// while cost charges are per item (i.e. per container) and are calculated per item, the revenue charge merges with one of them while
					// other charges will be without revenue part at all.
					new { ContainerCode = string.Empty, Cost = (ZDecimal)100m, Revenue = (ZDecimal)205m },
					new { ContainerCode = "40GP", Cost = (ZDecimal)200m, Revenue = (ZDecimal)0m },
				},
				propertiesToCompare: charge => new
				{
					ContainerCode = (string)charge.JobChargeAttrib_ContainerCode,
					Cost = charge.JR_OSCostAmt,
					Revenue = charge.JR_OSSellAmt
				});
		}

		#endregion

		#region Match charges by container number

		public void TestAddAutoRates_MatchedByContainerNumber()
		{
			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m, containerNumber: "CONTAINER1"),
					CreateCharge(containerCode: "20GP", costAmount: 200m, containerNumber: ""),
					CreateCharge(containerCode: "20GP", costAmount: 300m, containerNumber: "CONTAINER3"),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 220m, containerNumber: ""),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 110m, containerNumber: "CONTAINER1"),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 440m, containerNumber: "CONTAINER4"),
				},
				expectedResultCharges: new[]
				{
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)110m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER1" },
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)220m, JobChargeAttrib_ContainerNumber = (ZString)"" },
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)300m, JR_OSSellAmt = (ZDecimal)0m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER3" },
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)440m, JR_OSSellAmt = (ZDecimal)440m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER4" },
				},
				propertiesToCompare: charge => new
				{
					charge.JobChargeAttrib_ContainerCode,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt,
					charge.JobChargeAttrib_ContainerNumber
				}
				);

			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m, containerNumber: "CONTAINER1"),
					CreateCharge(containerCode: "20GP", costAmount: 200m, containerNumber: ""),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 220m, containerNumber: "CONTAINER2"),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 110m, containerNumber: "CONTAINER1"),
				},
				expectedResultCharges: new[]
				{
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)110m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER1" },
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)220m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER2" },
				},
				propertiesToCompare: charge => new
				{
					charge.JobChargeAttrib_ContainerCode,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt,
					charge.JobChargeAttrib_ContainerNumber
				});

			AssertAddAutoRates(
				existingCharges: new[]
				{
					CreateCharge(containerCode: "20GP", costAmount: 100m, containerNumber: "CONTAINER1"),
					CreateCharge(containerCode: "20GP", costAmount: 200m, containerNumber: "CONTAINER2"),
				},
				autoRateResults: new AutoRateInfoCollection(Factory)
				{
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 220m, containerNumber: ""),
					CreateAutoRateInfo(containerCode: "20GP", costSell: CostSell.Revenue, amount: 110m, containerNumber: "CONTAINER1"),
				},
				expectedResultCharges: new[]
				{
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)100m, JR_OSSellAmt = (ZDecimal)110m, JobChargeAttrib_ContainerNumber = (ZString)"CONTAINER1" },
					new { JobChargeAttrib_ContainerCode = (ZString)"20GP", JR_OSCostAmt = (ZDecimal)200m, JR_OSSellAmt = (ZDecimal)220m, JobChargeAttrib_ContainerNumber = (ZString)"" },
				},
				propertiesToCompare: charge => new
				{
					charge.JobChargeAttrib_ContainerCode,
					charge.JR_OSCostAmt,
					charge.JR_OSSellAmt,
					charge.JobChargeAttrib_ContainerNumber
				});
		}

		#endregion

		#region Implementation

		void AssertAddAutoRates(
			IEnumerable<BaseCharge> existingCharges,
			AutoRateInfoCollection autoRateResults,
			IEnumerable<object> expectedResultCharges,
			Func<BaseCharge, object> propertiesToCompare = null,
			Job job = null,
			IJobHeaderParent jobHeaderParent = null)
		{
			if (jobHeaderParent == null)
			{
				jobHeaderParent = Factory.New<ForwardingShipment>();
			}

			if (job == null)
			{
				job = new Job.Loader(jobHeaderParent).TryCreateWithoutMutexForTestOnly();
			}

			existingCharges.Where(c => !job.Charges.Contains(c)).ForEach(c =>
			{
				if (c.JR_JH.IsEmpty)
				{
					c.JR_JH = job.PK;
				}

				if (!job.Charges.Contains(c))
				{
					job.Charges.Add(c);
				}
			});

			var costSell = autoRateResults.First().IsCost ? CostSell.Cost : CostSell.Revenue;
			var strategy = new AutoRateInvoicingStrategy(jobHeaderParent as IBusiness, job);
			strategy.AddAutoRates(new TestInteractor(), autoRateResults, costSell, null);

			var actualCharges = propertiesToCompare != null
				? job.Charges.Select(propertiesToCompare)
				: job.Charges;

			AssertContainsExactElementsInAnyOrder(expectedResultCharges, actualCharges);
		}

		BaseCharge CreateCharge(
			string chargeCode = "FRT",
			string orderReference = null,
			string relatedJobNumber = null,
			string costCurrency = "AUD",
			string sellCurrency = "AUD",
			decimal costAmount = 0m,
			decimal sellAmount = 0m,
			string containerCode = null,
			string containerNumber = null,
			Job job = null)
		{
			var charge = job == null
				? Factory.New<BaseCharge>()
				: job.Charges.AddNew();

			charge.JR_AC = LoadChargeCode(chargeCode).PK;
			charge.JR_OSCostAmt = costAmount;
			charge.JR_RX_NKCostCurrency = costCurrency;
			charge.JR_OSSellAmt = sellAmount;
			charge.JR_RX_NKSellCurrency = sellCurrency;
			charge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;
			charge.JR_OrderReference = orderReference;
			charge.JR_Calc_RelatedJobNumber = relatedJobNumber;

			if (!string.IsNullOrEmpty(containerCode) || !string.IsNullOrEmpty(containerNumber))
			{
				var attributes = new RateAttributeSet();
				AddAtttributes(attributes, containerCode, containerNumber);
				charge.AddAttributes(attributes);
			}

			return charge;
		}

		void AddAtttributes(RateAttributeSet attributes, string containerCode = null, string containerNumber = null)
		{
			if (!string.IsNullOrEmpty(containerCode))
			{
				attributes.Add(JobChargeAttribTypeList.Codes.ContainerCode, containerCode);
			}

			if (!string.IsNullOrEmpty(containerNumber))
			{
				attributes.Add(JobChargeAttribTypeList.Codes.ContainerNumber, containerNumber);
			}
		}

		AutoRateInfo CreateAutoRateInfo(
			string chargeCode = "FRT",
			CostSell costSell = CostSell.Cost,
			string orderReference = null,
			string currency = "AUD",
			decimal amount = 100m,
			string containerCode = null,
			string containerNumber = null,
			JobInvoicingConsumerType consumerType = null)
		{
			var info = new AutoRateInfo(Factory);
			info.ChargeCode = LoadChargeCode(chargeCode);
			info.JobRef = orderReference;
			info.IsCost = costSell == CostSell.Cost;
			info.Currency = currency;

			if (consumerType != null)
			{
				info.SetConsumerType_ForTest(consumerType);
			}

			AddAtttributes(info.Attributes, containerCode, containerNumber);

			var rateInfo = RateInfo.CreateFLT(amount, "AUD");
			var paymentBasis = new PaymentBasis(new Quantity(), rateInfo, AdapterType.Shipment, "Shipment 666");

			info.Bases.Add(paymentBasis);
			return info;
		}

		AccChargeCode LoadChargeCode(string chargeCode)
		{
			var filter = new ZQuery(
				new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
				new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode));

			return Factory.LoadTop1<AccChargeCode>(filter);
		}

		TestObjectCreator TestHelper => testHelper ?? (testHelper = new TestObjectCreator(Factory));
		TestObjectCreator testHelper;

		#endregion
	}
}
