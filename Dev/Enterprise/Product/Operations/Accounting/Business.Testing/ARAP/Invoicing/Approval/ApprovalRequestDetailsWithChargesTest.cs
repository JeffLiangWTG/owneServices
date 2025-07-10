using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class ApprovalRequestDetailsWithChargesTest<DetailsType, ChargeDetailsType> : ApprovalRequestDetailsTest<DetailsType>
			where DetailsType : ApprovalRequestDetailsWithCharges<ChargeDetailsType>
			where ChargeDetailsType : ApprovalRequestChargeDetails
	{
		public virtual void TestOpertorEqualCore()
		{
			var a = (DetailsType)GetNewBusinessObject();
			var b = (DetailsType)GetNewBusinessObject();

			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			var chargeA1 = a.Charges.AddNew();
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			var chargeB1 = b.Charges.AddNew();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA1.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB1.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			var chargeA2 = a.Charges.AddNew();
			var chargeA3 = a.Charges.AddNew();
			var chargeB2 = b.Charges.AddNew();
			var chargeB3 = b.Charges.AddNew();
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA2.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB2.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);

			chargeA3.JobNumber = "1";
			AssertEquals(false, a == b);
			AssertEquals(false, b == a);

			chargeB3.JobNumber = "1";
			AssertEquals(true, a == b);
			AssertEquals(true, b == a);
		}

		public void TestOperationEqualWhenChargesInDifferentOrder()
		{
			var a = (DetailsType)GetNewBusinessObject();
			var b = (DetailsType)GetNewBusinessObject();
			var c = (DetailsType)GetNewBusinessObject();
			var d = (DetailsType)GetNewBusinessObject();
			var e = (DetailsType)GetNewBusinessObject();
			var f = (DetailsType)GetNewBusinessObject();

			var chargeA1_a = a.Charges.AddNew();
			var chargeA2_a = a.Charges.AddNew();
			var chargeA3_b = a.Charges.AddNew();

			var chargeB1_a = b.Charges.AddNew();
			var chargeB2_b = b.Charges.AddNew();
			var chargeB3_a = b.Charges.AddNew();

			var chargeC1_b = c.Charges.AddNew();
			var chargeC2_a = c.Charges.AddNew();
			var chargeC3_a = c.Charges.AddNew();

			var chargeD1_b = d.Charges.AddNew();
			var chargeD2_b = d.Charges.AddNew();
			var chargeD3_a = d.Charges.AddNew();

			var chargeE1_b = e.Charges.AddNew();
			var chargeE2_a = e.Charges.AddNew();
			var chargeE3_b = e.Charges.AddNew();

			var chargeF1_a = f.Charges.AddNew();
			var chargeF2_b = f.Charges.AddNew();
			var chargeF3_b = f.Charges.AddNew();

			chargeA1_a.JobNumber = "A";
			chargeA2_a.CopyFrom(chargeA1_a);
			chargeB1_a.CopyFrom(chargeA1_a);
			chargeB3_a.CopyFrom(chargeA1_a);
			chargeC2_a.CopyFrom(chargeA1_a);
			chargeC3_a.CopyFrom(chargeA1_a);
			chargeD3_a.CopyFrom(chargeA1_a);
			chargeE2_a.CopyFrom(chargeA1_a);
			chargeF1_a.CopyFrom(chargeA1_a);

			chargeA3_b.JobNumber = "B";
			chargeB2_b.CopyFrom(chargeA3_b);
			chargeC1_b.CopyFrom(chargeA3_b);
			chargeD1_b.CopyFrom(chargeA3_b);
			chargeD2_b.CopyFrom(chargeA3_b);
			chargeE1_b.CopyFrom(chargeA3_b);
			chargeE3_b.CopyFrom(chargeA3_b);
			chargeF2_b.CopyFrom(chargeA3_b);
			chargeF3_b.CopyFrom(chargeA3_b);

#pragma warning disable 1718
			Assert("a == a", a == a);
			Assert("a == b", a == b);
			Assert("a == c", a == c);
			Assert("a != d", a != d);
			Assert("a != e", a != e);
			Assert("a != f", a != f);

			Assert("b == a", b == a);
			Assert("b == b", b == b);
			Assert("b == c", b == c);
			Assert("b != d", b != d);
			Assert("b != e", b != e);
			Assert("b != f", b != f);

			Assert("c == a", c == a);
			Assert("c == b", c == b);
			Assert("c == c", c == c);
			Assert("c != d", c != d);
			Assert("c != e", c != e);
			Assert("c != f", c != f);

			Assert("d != a", d != a);
			Assert("d != b", d != b);
			Assert("d != c", d != c);
			Assert("d == d", d == d);
			Assert("d == e", d == e);
			Assert("d == f", d == f);

			Assert("e != a", e != a);
			Assert("e != b", e != b);
			Assert("e != c", e != c);
			Assert("e == d", e == d);
			Assert("e == e", e == e);
			Assert("e == f", e == f);

			Assert("f != a", f != a);
			Assert("f != b", f != b);
			Assert("f != c", f != c);
			Assert("f == d", f == d);
			Assert("f == e", f == e);
			Assert("f == f", f == f);
#pragma warning restore 1718
		}

		public virtual void TestCopyFromCore()
		{
			var postingRequest = (DetailsType)GetNewBusinessObject();

			postingRequest.MaxAmountToApprove = 11M;
			postingRequest.PostingOption = "COSTS";
			var charge = postingRequest.Charges.AddNew();
			charge.JobNumber = "JOB1";
			charge.Department = "DEP1";
			charge = postingRequest.Charges.AddNew();
			charge.JobNumber = "JOB2";
			charge.Branch = "BRN1";

			var newPostingRequest = (DetailsType)GetNewBusinessObject();

			newPostingRequest.CopyFrom(postingRequest);
			AssertEquals("MaxAmountToApprove", 11M, newPostingRequest.MaxAmountToApprove);
			AssertEquals("PostingOption", "COSTS", newPostingRequest.PostingOption);
			AssertEquals("Charges.Count", 2, newPostingRequest.Charges.Count);
			AssertEquals("Charges[0].JobNumber", "JOB1", newPostingRequest.Charges[0].JobNumber);
			AssertEquals("Charges[0].Department", "DEP1", newPostingRequest.Charges[0].Department);
			AssertEquals("Charges[1].JobNumber", "JOB2", newPostingRequest.Charges[1].JobNumber);
			AssertEquals("Charges[1].Branch", "BRN1", newPostingRequest.Charges[1].Branch);
		}

		protected override DetailsType GetNewFullyPopulatedBusinessObjectForXMLTest()
		{
			var details = base.GetNewFullyPopulatedBusinessObjectForXMLTest();
			details.PostingOption = "COSTS";

			var charge = details.Charges.AddNew();
			charge.JobNumber = "JOB1";
			charge.Department = "DEP1";
			charge.Description = "LineDesc1";
			charge.AccInvMsgPK = TaxMessage.PK;
			charge.TaxDate = TaxDate;

			charge = details.Charges.AddNew();
			charge.JobNumber = "JOB2";
			charge.Branch = "BRN1";
			charge.Description = "LineDesc2";
			charge.AccInvMsgPK = ZGuid.Empty;
			charge.TaxDate = new ZDate(2020, 6, 3);

			return details;
		}

		protected override void AssertFullyPopulatedBusinessObjectForXMLTest(DetailsType details, bool withNewFields = true)
		{
			base.AssertFullyPopulatedBusinessObjectForXMLTest(details, withNewFields);
			AssertEquals("COSTS", details.PostingOption);
			AssertEquals("Charges.Count", 2, details.Charges.Count);
			AssertEquals("Charges[0].JobNumber", "JOB1", details.Charges[0].JobNumber);
			AssertEquals("Charges[0].Department", "DEP1", details.Charges[0].Department);
			AssertEquals("Charges[1].JobNumber", "JOB2", details.Charges[1].JobNumber);
			AssertEquals("Charges[1].Branch", "BRN1", details.Charges[1].Branch);

			AssertEquals(withNewFields ? "LineDesc1" : string.Empty, details.Charges[0].Description);
			AssertEquals(withNewFields ? TaxMessage.PK : ZGuid.Empty, details.Charges[0].AccInvMsgPK);
			AssertEquals(withNewFields ? TaxDate : ZDate.Empty, details.Charges[0].TaxDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			TaxDate = new ZDate(2020, 6, 2);
		}

		protected AccInvMsg TaxMessage;
		protected ZDate TaxDate;
	}
}
