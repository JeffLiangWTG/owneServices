using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	internal sealed class UpdatedChargesStrategyTest : ChargeSpecificationStrategyTest<UpdatedChargesStrategy>
	{
		public void TestSkipHeaders()
		{
			IChargeSpecificationStrategy strategy = NewStrategy();

			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";

			GlbCompany company3 = Factory.New<GlbCompany>();
			company3.GC_Code = "CO3";

			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";

			GlbBranch branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BR2";

			GlbBranch branch3 = company3.Branches.AddNew();
			branch3.GB_Code = "BR3";

			AccChargeCode frt1 = GetChargeCode(company1, "FRT");
			AccChargeCode frt2 = GetChargeCode(company2, "FRT");
			AccChargeCode frt3 = GetChargeCode(company3, "FRT");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job.Loader loader = new Job.Loader(shipment);

			Job job = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch1);
			AddCharge(job, frt1, 1000, "AUD");

			Job jobWithPostedRevenue = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch2);
			AddCharge(jobWithPostedRevenue, frt2, 400, "AUD", true, false);

			Job jobWithPostedCost = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch3);
			AddCharge(jobWithPostedCost, frt3, 500, "AUD", false, true);

			const string expectedLog =
@"Warning: The Invoicing Job for company CO2 was skipped during the Import process as it has posted charges.
Warning: The Invoicing Job for company CO3 was skipped during the Import process as it has posted charges.";

			CombineAssertions(delegate
			{
				AssertEquals(false, strategy.SkipJobHeader(job));
				AssertEquals(true, strategy.SkipJobHeader(jobWithPostedRevenue));
				AssertEquals(true, strategy.SkipJobHeader(jobWithPostedCost));

				strategy.NotifySkippedHeaders();

				AssertMultilineASCIIEquals("", expectedLog, Buffer.AsString);
			});
		}

		public void TestSkipHeaders_Ignore()
		{
			IChargeSpecificationStrategy strategy = NewStrategy(Context, Xsd.PostedChargeHandling.Ignore);

			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "CO1";

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "CO2";

			GlbCompany company3 = Factory.New<GlbCompany>();
			company3.GC_Code = "CO3";

			GlbBranch branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";

			GlbBranch branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BR2";

			GlbBranch branch3 = company3.Branches.AddNew();
			branch3.GB_Code = "BR3";

			AccChargeCode frt1 = GetChargeCode(company1, "FRT");
			AccChargeCode frt2 = GetChargeCode(company2, "FRT");
			AccChargeCode frt3 = GetChargeCode(company3, "FRT");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job.Loader loader = new Job.Loader(shipment);

			Job job = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch1);
			AddCharge(job, frt1, 1000, "AUD");

			Job jobWithPostedRevenue = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch2);
			AddCharge(jobWithPostedRevenue, frt2, 400, "AUD", true, false);

			Job jobWithPostedCost = loader.TryLoadOrCreateWithoutMutexForTestOnly(branch3);
			AddCharge(jobWithPostedCost, frt3, 500, "AUD", false, true);

			CombineAssertions(delegate
			{
				AssertEquals(false, strategy.SkipJobHeader(job));
				AssertEquals(false, strategy.SkipJobHeader(jobWithPostedRevenue));
				AssertEquals(false, strategy.SkipJobHeader(jobWithPostedCost));

				strategy.NotifySkippedHeaders();

				AssertMultilineASCIIEquals("", "", Buffer.AsString);
			});
		}

		public void TestMatching()
		{
			AccChargeCode frt = GetChargeCode("FRT");
			AccChargeCode opch = GetChargeCode("OPCH");
			AccChargeCode dpch = GetChargeCode("DPCH");

			GlbBranch branch1 = GetBranch("BR1");
			GlbBranch branch2 = GetBranch("BR2");

			GlbDepartment department1 = GetDepartment("DE1");
			GlbDepartment department2 = GetDepartment("DE2");

			OrgHeader debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			debtor1.OH_Code = "DEBTOR1";

			OrgHeader debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			debtor2.OH_Code = "DEBTOR2";

			OrgHeader creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "CREDITOR1";

			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_Code = "CREDITOR2";

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly(branch1);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			Charge chargeF1 = AddCharge(job, frt, 101, "AUD", branch1, department1, debtor1, creditor1);
			Charge chargeF2 = AddCharge(job, frt, 102, "AUD", branch1, department1, debtor1, creditor1);
			Charge chargeF3 = AddCharge(job, frt, 103, "AUD", branch2, department1, debtor1, creditor1);
			Charge chargeF4 = AddCharge(job, frt, 104, "AUD", branch1, department2, debtor1, creditor1);
			Charge chargeF5 = AddCharge(job, frt, 105, "AUD", branch1, department1, debtor2, creditor1);
			Charge chargeF6 = AddCharge(job, frt, 106, "AUD", branch1, department1, debtor1, creditor2);

			Charge chargeO1 = AddCharge(job, opch, 111, "AUD", branch1, department1, debtor1, creditor1);
			Charge chargeO2 = AddCharge(job, opch, 112, "AUD", branch1, department1, debtor1, creditor1);

			Charge chargeD1 = AddCharge(job, dpch, 121, "AUD", branch1, department1, debtor1, creditor1);
			Charge chargeD2 = AddCharge(job, dpch, 122, "AUD", branch1, department1, debtor1, creditor1);

			IChargeSpecificationStrategy strategy = NewStrategy();

			AssertEquals(chargeF6, strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()
			{
				Creditor = new Xsd.Organisation() { EDICode = "CREDITOR2" },
			}));

			AssertEquals(chargeF5, strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()
			{
				Debtor = new Xsd.Organisation() { EDICode = "DEBTOR2" },
			}));

			AssertEquals(chargeF4, strategy.MatchCharge(job, frt, null, department2, new Xsd.ChargeLine()));
			AssertEquals(chargeF3, strategy.MatchCharge(job, frt, branch2, null, new Xsd.ChargeLine()));
			AssertEquals(chargeF1, strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()));
			AssertEquals(chargeF2, strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()));
			AssertEquals(null, strategy.MatchCharge(job, frt, null, null, new Xsd.ChargeLine()));
			AssertEquals(chargeO1, strategy.MatchCharge(job, opch, null, null, new Xsd.ChargeLine()));

			strategy.RemoveUnmatchedCharges();

			AssertContainsExactElementsInAnyOrder(
				(c) => string.Format("{0}: {1:N2} {2}", c.ChargeCode == null ? "<null>" : c.ChargeCode.AC_Code.ToString(), c.JR_OSSellAmt, c.JR_RX_NKSellCurrency),
				new Charge[]
				{
					chargeF1,
					chargeF2,
					chargeF3,
					chargeF4,
					chargeF5,
					chargeF6,
					chargeO1,
					chargeD1,
					chargeD2,
				},
				job.Charges.ToArray<Charge>());
		}

		#region Implementation

		protected override Xsd.ChargesSpecified Value
		{
			get { return Xsd.ChargesSpecified.UpdatedCharges; }
		}

		#endregion
	}
}
