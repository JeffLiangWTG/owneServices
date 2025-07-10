using System;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class IncoTermsTest : TestCase
	{
		public void TestIncoterms2000EffectiveDate()
		{
			AssertEquals(new DateTime(2000, 1, 1), Constants.IncoTerms.Incoterms2000EffectiveDate);
		}

		public void TestIncoterms2000Codes()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "EXW", "FCA", "FAS", "FOB", "CFR", "CIF", "CPT", "CIP", "DDP", "DAF", "DES", "DEQ", "DDU" },
				Constants.IncoTerms.Incoterms2000);
		}

		public void TestIncoterms2010EffectiveDate()
		{
			AssertEquals(new DateTime(2011, 1, 1), Constants.IncoTerms.Incoterms2010EffectiveDate);
		}

		public void TestIncoterms2010Codes()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "EXW", "FCA", "FAS", "FOB", "CFR", "CIF", "CPT", "CIP", "DDP", "DAT", "DAP" },
				Constants.IncoTerms.Incoterms2010);
		}

		public void TestIncoterms2020EffectiveDate()
		{
			AssertEquals(new DateTime(2020, 1, 1), Constants.IncoTerms.Incoterms2020EffectiveDate);
		}

		public void TestIncoterms2020Codes()
		{
			var codes = new string[] { "EXW", "FCA", "FC1", "FC2", "FAS", "FOB", "CFR", "CIF", "CPT", "CIP", "DDP", "DAP", "DPU" };
			AssertContainsExactElementsInAnyOrder(codes, Constants.IncoTerms.Incoterms2020);
		}

		public void TestIncoTermDefaultFromPaymentType()
		{
			AssertEquals("Prepaid should default to CostAndFreight", Constants.IncoTerms.CostAndFreight, Constants.IncoTerms.DefaultIncoFromPaymentType(Constants.PaymentType.Prepaid));
			AssertEquals("Collect should default to FreeOnBoard", Constants.IncoTerms.FreeOnBoard, Constants.IncoTerms.DefaultIncoFromPaymentType(Constants.PaymentType.Collect));
			AssertEquals("Empty type, no default", "", Constants.IncoTerms.DefaultIncoFromPaymentType(""));
			AssertEquals("Invalid type, no default", "", Constants.IncoTerms.DefaultIncoFromPaymentType("XXX"));
		}
	}
}
