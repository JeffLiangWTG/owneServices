using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CSupportingDocumentProvider))]
	sealed class CC044CSupportingDocumentProviderTest : DocumentProviderAbstractTest<CC044CSupportingDocumentProvider>
	{
		protected override string SubType => "SUP";

		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "complement";
			info.CSI_Status = "NEW";
			AssertEquals("complement", Provider.ComplementOfInformation);
		}

		public void TestComplementOfInformation_Conditional()
		{
			info.CSI_ReferenceNumber2 = "complement";
			info.CSI_Status = "MIS";
			AssertNullOrEmpty(Provider.ComplementOfInformation);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertEquals(0, Provider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertNullOrEmpty(Provider.IssuingAuthorityName);
		}

		public void TestValidityDate()
		{
			AssertNull(Provider.ValidityDate);
		}

		public void TestAmount()
		{
			AssertEquals(new decimal(0), Provider.Amount);
		}

		public void TestCurrency()
		{
			AssertNullOrEmpty(Provider.Currency);
		}

		public void TestMeasurementUnitAndQualifier()
		{
			AssertNullOrEmpty(Provider.MeasurementUnitAndQualifier);
		}

		public void TestQuantity()
		{
			AssertNull(Provider.Quantity);
		}
	}
}
