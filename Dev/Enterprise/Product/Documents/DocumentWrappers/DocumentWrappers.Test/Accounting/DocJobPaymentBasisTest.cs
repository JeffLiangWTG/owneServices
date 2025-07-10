using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobPaymentBasis))]
	sealed class DocJobPaymentBasisTest : DocumentWrapperTestCase
	{
		public void TestAllProperties()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "Freight";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = chargeCode.PK;

			var paymentBasis = charge.PaymentBases.AddNew();
			paymentBasis.PBS_JR = charge.PK;
			paymentBasis.PBS_ChargeableDescription = "AA123456";
			paymentBasis.PBS_ChargeableAmount = 13;
			paymentBasis.PBS_ChargeableUnit = "KG";
			paymentBasis.PBS_PerUnitRate = 6;
			paymentBasis.PBS_RateUnit = "KG";
			paymentBasis.PBS_RX_NKRateCurrency = "BTC";
			paymentBasis.PBS_AdapterID = "Container";
			paymentBasis.PBS_RateReference = "UNT";

			var wrapper = DocJobPaymentBasis.New(paymentBasis, Factory);

			AssertEquals("ChargeCode", "FRT", wrapper.ChargeCode);
			AssertEquals("ChargeDescription", "Freight", wrapper.ChargeDescription);
			AssertEquals("Reference", "AA123456", wrapper.Reference);
			AssertEquals("Quantity", "13", wrapper.Quantity);
			AssertEquals("QuantityUnit", "KG", wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)6.0, wrapper.Rate);
			AssertEquals("RateUnit", "KG", wrapper.RateUnit);
			AssertEquals("Currency", "BTC", wrapper.Currency);
			AssertEquals("RatingAdapterId", "Container", wrapper.Adapter);
		}

		public void TestPerUnitBasis()
		{
			var basis = GetBasis();
			basis.PBS_ChargeableAmount = 20;
			basis.PBS_ChargeableUnit = "KG";
			basis.PBS_MinRate = 1000;
			basis.PBS_PerUnitRate = 100;
			basis.PBS_RateUnit = "KG";
			basis.PBS_RateReference = "UNT";

			var wrapper = DocJobPaymentBasis.New(basis, Factory);

			AssertEquals("Quantity", "20", wrapper.Quantity);
			AssertEquals("QuantityUnit", "KG", wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)100.0, wrapper.Rate);
			AssertEquals("RateUnit", "KG", wrapper.RateUnit);
			AssertEquals("RateReference", "Per Unit", wrapper.RateReference);
		}

		public void TestFlatBasis()
		{
			var basis = GetBasis();
			basis.PBS_FlatRate = 1000;
			basis.PBS_RateReference = "FLT";

			var wrapper = DocJobPaymentBasis.New(basis, Factory);

			AssertEquals("Quantity", ZString.Empty, wrapper.Quantity);
			AssertEquals("QuantityUnit", ZString.Empty, wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)1000.0, wrapper.Rate);
			AssertEquals("RateUnit", ZString.Empty, wrapper.RateUnit);
			AssertEquals("RateReference", "Base Rate", wrapper.RateReference);
		}

		public void TestMinBasis()
		{
			var basis = GetBasis();
			basis.PBS_ChargeableAmount = 20;
			basis.PBS_ChargeableUnit = "KG";
			basis.PBS_MinRate = 5000;
			basis.PBS_PerUnitRate = 100;
			basis.PBS_FlatRate = 1000;
			basis.PBS_RateUnit = "KG";
			basis.PBS_RateReference = "MIN";

			var wrapper = DocJobPaymentBasis.New(basis, Factory);

			AssertEquals("Quantity", ZString.Empty, wrapper.Quantity);
			AssertEquals("QuantityUnit", ZString.Empty, wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)5000.0, wrapper.Rate);
			AssertEquals("RateUnit", ZString.Empty, wrapper.RateUnit);
			AssertEquals("RateReference", "Minimum", wrapper.RateReference);
		}

		public void TestMaxBasis()
		{
			var basis = GetBasis();
			basis.PBS_MaxRate = 1000;
			basis.PBS_RateUnit = "KG";
			basis.PBS_RateReference = "MAX";

			var wrapper = DocJobPaymentBasis.New(basis, Factory);

			AssertEquals("Quantity", ZString.Empty, wrapper.Quantity);
			AssertEquals("QuantityUnit", ZString.Empty, wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)1000.0, wrapper.Rate);
			AssertEquals("RateUnit", ZString.Empty, wrapper.RateUnit);
			AssertEquals("RateReference", "Maximum", wrapper.RateReference);
		}

		public void TestFlatBasisAsMinBasis()
		{
			// It is special case. When payment basis get merged then min base becomes flat basis

			var basis = GetBasis();
			basis.PBS_FlatRate = 5000;
			basis.PBS_RateReference = "MIN";

			var wrapper = DocJobPaymentBasis.New(basis, Factory);

			AssertEquals("Quantity", ZString.Empty, wrapper.Quantity);
			AssertEquals("QuantityUnit", ZString.Empty, wrapper.QuantityUnit);
			AssertEquals("Rate", (ZDecimal)5000.0, wrapper.Rate);
			AssertEquals("RateUnit", ZString.Empty, wrapper.RateUnit);
			AssertEquals("RateReference", "Minimum", wrapper.RateReference);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var paymentBasis = Factory.NewWithValidTestData<JobPaymentBasis>();

			return new DocumentWrapper[]
			{
				DocJobPaymentBasis.New(paymentBasis, Factory)
			};
		}

		JobPaymentBasis GetBasis()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "FRT";
			chargeCode.AC_Desc = "Freight";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = chargeCode.PK;

			var paymentBasis = charge.PaymentBases.AddNew();
			paymentBasis.PBS_JR = charge.PK;
			paymentBasis.PBS_ChargeableDescription = "AA123456";
			paymentBasis.PBS_RX_NKRateCurrency = "BTC";
			paymentBasis.PBS_AdapterID = "Container";

			return paymentBasis;
		}
	}
}
