using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(SupportingDocumentProvider))]
	class SupportingDocumentProviderTest : DocumentProviderAbstractTest<SupportingDocumentProvider>
	{
		protected override string SubType => "SUP";

		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "SupDocRef";
			AssertEquals("SupDocRef", Provider.ComplementOfInformation);
		}

		public void TestDocumentLineItemNumber()
		{
			info.CSI_ItemNumber = 4;
			AssertEquals(4, Provider.DocumentLineItemNumber);
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
