using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceHeader>("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseJobComInvoiceHeader)));
		}

		public override void TestLocalCurrencyCodeCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Israel, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
		}

		public void TestBaseType()
		{
			AssertEquals(typeof(AutoILJobComInvoiceHeader), typeof(JobComInvoiceHeader).BaseType);
		}

		public void TestValidation()
		{
			var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertType<JobComInvoiceHeaderValidation>(jobComInvoiceHeader.Validation);
		}

		public void TestJZ_InvoiceType()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals(3, jobComInvoiceHeader.JZ_InvoiceTypeInfo.MaxLength);
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_InvoiceTypeInfo)} caption", "Invoice Type", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_InvoiceTypeInfo).Caption);
		}

		public void TestJZ_PreferenceDocumentType()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals(10, jobComInvoiceHeader.JZ_PreferenceDocumentTypeInfo.MaxLength);
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_PreferenceDocumentTypeInfo)} caption", "Preference Agreement", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_PreferenceDocumentTypeInfo).Caption);
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_PreferenceDocumentTypeInfo)} short caption", "Pref. Agreement", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_PreferenceDocumentTypeInfo).ShortCaption);
		}

		public void TestJZ_InvoiceDisplaySequence()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_InvoiceDisplaySequenceInfo)} caption", "Sequence", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_InvoiceDisplaySequenceInfo).Caption);
		}

		public void TestJZ_IncoTermPlace()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_IncoTermPlaceInfo)} caption", "Country", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_IncoTermPlaceInfo).Caption);
		}

		public void TestJZ_PaymentTerms()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertEquals($"{nameof(jobComInvoiceHeader.JZ_PaymentTermsInfo)} caption", "Payment Terms", DataBoundResourceStrings.GetDataForProperty(jobComInvoiceHeader.JZ_PaymentTermsInfo).Caption);
		}

		public void TestDecimalPlaces()
		{
			var jobComInvoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			AssertDecimalPlaces(jobComInvoiceHeader, "JZ_InvoiceAmount");
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

		static void AssertDecimalPlaces(JobComInvoiceHeader invoiceLine, string propertyName)
		{
			CombineAssertions($"Check decimal places for {propertyName}", () =>
			{
				var decimalPlacesAttribute = invoiceLine.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(DecimalPlacesAttribute), false).FirstOrDefault() as DecimalPlacesAttribute;
				AssertNotNull("DecimalPlaces attribute should be defined", decimalPlacesAttribute);
				AssertEquals(2, decimalPlacesAttribute.DecimalPlaces);
			});
		}
	}
}

