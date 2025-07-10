using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditReportItemDataType))]
	sealed class CreditReportsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CreditReportItemDataType>
	{
		protected override string ExpectedEditorName => "CreditReportsRegistryItemEditor";

		protected override CreditReportItemDataType GetNewDataType()
		{
			return new CreditReportItemDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new CreditReportItemCollection();
			var creditReport1 = collection1.AddNew();
			creditReport1.CountryCode = "Dummy Code1";
			creditReport1.Country = "Dummy Country1";
			creditReport1.CountryEnabledForOrganisation = true;
			creditReport1.CountryEnabledForCompany = true;
			creditReport1.ComprehensiveReportEnabled = false;
			creditReport1.CommercialBureauEnquiryEnabled = true;
			creditReport1.LatePaymentRiskEnabled = false;
			creditReport1.FailureRiskEnabled = true;

			var collection2 = new CreditReportItemCollection();
			var creditReport2 = collection2.AddNew();
			creditReport2.CountryCode = "Dummy Code2";
			creditReport2.Country = "Dummy Country2";
			creditReport1.CountryEnabledForOrganisation = false;
			creditReport1.CountryEnabledForCompany = false;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new CreditReportItemDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new CreditReportItemDataType().Serialise(collection2))
			};
		}

		public void TestValidateBeforeRegistryFormSaveCore()
		{
			var proposedValue = new CreditReportItemCollection
			{
				new CreditReportItem
				{
					Country = "Country1",
					CountryCode = "Code1",
					CountryEnabledForCompany = true,
					CountryEnabledForOrganisation = true,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = true,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				},
				new CreditReportItem
				{
					Country = "Country2",
					CountryCode = "Code2",
					CountryEnabledForCompany = true,
					CountryEnabledForOrganisation = true,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				},
				new CreditReportItem
				{
					Country = "Country3",
					CountryCode = "Code3",
					CountryEnabledForCompany = false,
					CountryEnabledForOrganisation = false,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				},
				new CreditReportItem
				{
					Country = "Country4",
					CountryCode = "Code4",
					CountryEnabledForCompany = true,
					CountryEnabledForOrganisation = true,
					FailureRiskEnabled = true,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = true,
					LatePaymentRiskEnabled = true
				},
				new CreditReportItem
				{
					Country = "Country5",
					CountryCode = "Code5",
					CountryEnabledForCompany = true,
					CountryEnabledForOrganisation = true,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				},
				new CreditReportItem
				{
					Country = "Country6",
					CountryCode = "Code6",
					CountryEnabledForCompany = false,
					CountryEnabledForOrganisation = true,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				},
				new CreditReportItem
				{
					Country = "Country7",
					CountryCode = "Code7",
					CountryEnabledForCompany = true,
					CountryEnabledForOrganisation = false,
					FailureRiskEnabled = false,
					ComprehensiveReportEnabled = false,
					CommercialBureauEnquiryEnabled = false,
					LatePaymentRiskEnabled = false
				}
			};

			var registryItem = new CreditReportItemCollectionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, new CreditReportItemCollection());
			var validate = new AnonymousMethod(() => DataType.ValidateBeforeRegistryFormSave(registryItem, proposedValue, Guid.Empty, Guid.Empty, Guid.Empty));
			var ex = AssertExceptionThrown<RegistryValidationException>(validate);

			AssertMultilineASCIIEquals("Please select at least 1 report type when Credit Reports are enabled for Country2, Country5, Country6.", ex.Message);
		}
	}
}
