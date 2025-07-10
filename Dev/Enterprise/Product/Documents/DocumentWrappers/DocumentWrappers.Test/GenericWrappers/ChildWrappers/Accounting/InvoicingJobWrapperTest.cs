using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(InvoicingJobWrapper))]
	sealed class InvoicingJobWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (InvoicingJobWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertNotNull("wrapperEmpty.Charges", wrapperEmpty.Charges);
			AssertEquals("wrapperEmpty.Debtor", null, wrapperEmpty.Debtor);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestWrapperMappingFull()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var job = Factory.NewJobForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "11112222";
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "AAA";
			debtor.OH_FullName = "Battleship Potemkin";

			var charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = debtor.PK;

			var wrapperFull = new InvoicingJobWrapper(job, debtor, Factory);
			AssertEquals("wrapperFull.ToString()", "11112222", wrapperFull.ToString());
			AssertEquals("wrapperFull.JobNum", "11112222", wrapperFull.JobNum);
			AssertNotNull("wrapperFull.Debtor", wrapperFull.Debtor);
			AssertEquals("wrapperFull.Debtor", "AAA", wrapperFull.Debtor.CompanyCode);
			AssertEquals("wrapperFull.Debtor", "Battleship Potemkin", wrapperFull.Debtor.CompanyName);
			AssertEquals("wrapperFull.Charges", 1, wrapperFull.Charges.Count);
		}

		public void TestCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var job = Factory.NewJobForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "11112222";
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var debtor1 = Factory.New<OrgHeader>();
			debtor1.OH_Code = "AAA";
			debtor1.OH_FullName = "Albert";

			var charge1Debtor1 = job.Charges.AddNew();
			charge1Debtor1.JR_OH_SellAccount = debtor1.PK;

			var charge2Debtor1 = job.Charges.AddNew();
			charge2Debtor1.JR_OH_SellAccount = debtor1.PK;

			var debtor2 = Factory.New<OrgHeader>();
			debtor2.OH_Code = "BBB";
			debtor2.OH_FullName = "Bonifacy";

			var charge1Debtor2 = job.Charges.AddNew();
			charge1Debtor2.JR_OH_SellAccount = debtor2.PK;

			var wrapper = new InvoicingJobWrapper(job, null, Factory);

			AssertContainsExactElementsInAnyOrder("expected all charges", new ZGuid[] { charge1Debtor1.PK, charge2Debtor1.PK, charge1Debtor2.PK },
				wrapper.Charges.Select(charge => ((DocJobInvoicingJobCharge)charge).ChargePK));

			wrapper = new InvoicingJobWrapper(job, debtor1, Factory);

			AssertContainsExactElementsInAnyOrder("expected debtor 1 charges only", new ZGuid[] { charge1Debtor1.PK, charge2Debtor1.PK },
				wrapper.Charges.Select(charge => ((DocJobInvoicingJobCharge)charge).ChargePK));

			wrapper = new InvoicingJobWrapper(job, debtor2, Factory);

			AssertContainsExactElementsInAnyOrder("expected debtor 2 charges only", new ZGuid[] { charge1Debtor2.PK },
				wrapper.Charges.Select(charge => ((DocJobInvoicingJobCharge)charge).ChargePK));
		}

		public void TestPaymentBases()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var job = Factory.NewJobForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "11112222";
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var charge1 = job.Charges.AddNew();
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "BBB";
			var chargeOverride = chargeCode1.ChargeTypeOverrides.AddNew();
			chargeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeOverride.AN_JobType = "ALL";
			chargeOverride.AN_JobDirection = "ALL";
			charge1.JR_AC = chargeCode1.PK;

			var charge2 = job.Charges.AddNew();
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "CCC";
			var chargeOverride2 = chargeCode1.ChargeTypeOverrides.AddNew();
			chargeOverride2.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			chargeOverride2.AN_JobType = "ALL";
			chargeOverride2.AN_JobDirection = "ALL";
			charge2.JR_AC = chargeCode2.PK;

			Factory.Save();

			var paymentBasis1 = charge1.PaymentBases.AddNew();
			paymentBasis1.PBS_AdapterID = "Consol";
			paymentBasis1.PBS_ChargeableAmount = 10;
			paymentBasis1.PBS_ChargeableUnit = "KG";
			paymentBasis1.PBS_ChargeableDescription = "AB1111111";
			paymentBasis1.PBS_PerUnitRate = 20;
			paymentBasis1.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			var paymentBasis2 = charge2.PaymentBases.AddNew();
			paymentBasis2.PBS_AdapterID = "Shipment";
			paymentBasis2.PBS_ChargeableAmount = 20;
			paymentBasis2.PBS_ChargeableUnit = "M3";
			paymentBasis2.PBS_ChargeableDescription = "AB222222";
			paymentBasis2.PBS_PerUnitRate = 30;
			paymentBasis2.PBS_RateReference = nameof(RateInfo.RateInfoType.UNT);

			var jobWrapper = new InvoicingJobWrapper(job, Factory);

			Func<DocJobPaymentBasis, string> toString = basis =>
			{
				return Invariant($"{basis.Adapter}|{basis.Quantity}|{basis.QuantityUnit}|{basis.Reference}");
			};

			var expected = new[]
			{
				"Consol|10|KG|AB1111111",
				"Shipment|20|M3|AB222222",
			};

			paymentBasis1.PBS_IsCost = true;
			paymentBasis2.PBS_IsCost = true;
			AssertContainsExactElementsInAnyOrder(expected, jobWrapper.CostPaymentBases.Cast<DocJobPaymentBasis>().Select(toString));

			paymentBasis1.PBS_IsCost = false;
			paymentBasis2.PBS_IsCost = false;
			AssertContainsExactElementsInAnyOrder(expected, jobWrapper.SellPaymentBases.Cast<DocJobPaymentBasis>().Select(toString));
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Debtor :  is null
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new InvoicingJobWrapper(Factory.NewJobForTesting<Job>(), Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Invoicing Job                                  (Default Field: JobNum)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Debtor                                  Organisation
JobNum                                  String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new InvoicingJobWrapper(Factory.NewJobForTesting<Job>(), Factory);
		}
	}
}
