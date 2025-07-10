using CargoWise.Common;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingCountryFactory.Israel;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(IsraelGovernmentAllocatedIDPayablesValidationProvider))]
	public sealed class IsraelGovernmentAllocatedIDPayablesValidationProviderTest : TestCase
	{
		const string ShouldBeInCorrectShapeErrorMessage = "Govt. ID must be between 9 and 30 characters long and only letters and number. The last 9 right digits must be numeric.";

		public void TestValidate()
		{
			var validation = new IsraelGovernmentAllocatedIDPayablesValidationProvider();

			AssertValidate_ILCases(orgCountryCode: "IL", validation);

			AssertValidate_NonILCases(orgCountryCode: "", validation);
			AssertValidate_NonILCases(orgCountryCode: "TR", validation);
		}

		public void TestValidate_LedgerType_Cases()
		{
			var validation = new IsraelGovernmentAllocatedIDPayablesValidationProvider();
			var cases = new (string ledger, string governmentAllocatedID, string orgCountryCode, string expectedMessage)[]
			{
				("AP", "123", "IL", ShouldBeInCorrectShapeErrorMessage),
				("IN", "123", "IL", ShouldBeInCorrectShapeErrorMessage),
				("PA", "123", "IL", ShouldBeInCorrectShapeErrorMessage),
				("AR", "123", "IL", null)
			};

			CombineAssertions(() => cases.ForEach(c => AssertCase(validation, c.ledger, c.governmentAllocatedID, c.orgCountryCode, c.expectedMessage)));
		}

		void AssertValidate_NonILCases(string orgCountryCode, IsraelGovernmentAllocatedIDPayablesValidationProvider validation)
		{
			var countryErrorMessage = "The transaction is not suitable for entering Govt. ID. The country of the creditor's address must be IL.";
			var cases = new (string ledger, string governmentAllocatedID, string expectedMessage)[]
			{
				("AP", "12345678", countryErrorMessage),
				("AP", "123456789", countryErrorMessage),
				("AP", "", null)
			};

			CombineAssertions(() => cases.ForEach(c => AssertCase(validation, c.ledger, c.governmentAllocatedID, orgCountryCode, c.expectedMessage)));
		}

		void AssertValidate_ILCases(string orgCountryCode, IsraelGovernmentAllocatedIDPayablesValidationProvider validation)
		{
			var shouldBeEmptyOrFAL = "The Government Allocated Number can only be overridden for transactions with e-Reporting Status of FAL.";

			var cases = new (string ledger, string governmentAllocatedID, string expectedMessage, string eReportingStatus)[]
			{
				("AP", "12345678", ShouldBeInCorrectShapeErrorMessage, null),
				("AP", ".123456789", ShouldBeInCorrectShapeErrorMessage, null),
				("AP", "0123456789012345678901234567890", ShouldBeInCorrectShapeErrorMessage, null),
				("AP", "123456ABC", ShouldBeInCorrectShapeErrorMessage, null),
				("AP", "", null, null),
				("AP", "123456789", null, null),
				("AP", "ABCD123456789", null, null),
				("AP", "012345678901234567890123456789", null, null),
				("AP", "012345678901234567890123456789", shouldBeEmptyOrFAL, "SUC"),
				("AP", "012345678901234567890123456789", null, "FAL"),
				("AP", "012345678901234567890123456789", null, "")
			};

			CombineAssertions(() => cases.ForEach(c => AssertCase(validation, c.ledger, c.governmentAllocatedID, orgCountryCode, c.expectedMessage, c.eReportingStatus)));
		}

		void AssertCase(IsraelGovernmentAllocatedIDPayablesValidationProvider validation, string ledger, string governmentAllocatedID, string orgCountryCode, string expectedMessage, string eReportingStatus = null)
		{
			var result = validation.ValidateGovernmentAllocatedID(new DummyGovernmentAllocatedIDValidationData(eReportingStatus, governmentAllocatedID, ledger, orgCountryCode));
			AssertEquals(expectedMessage, result);
		}

		record DummyGovernmentAllocatedIDValidationData : IGovernmentAllocatedIDValidationData
		{
			public string EReportingStatus { get; }
			public string GovernmentAllocatedID { get; }
			public string Ledger { get; }
			public string OrgCountryCode { get; }

			public DummyGovernmentAllocatedIDValidationData(string eReportingStatus, string governmentAllocatedID, string ledger, string orgCountryCode)
			{
				EReportingStatus = eReportingStatus;
				GovernmentAllocatedID = governmentAllocatedID;
				Ledger = ledger;
				OrgCountryCode = orgCountryCode;
			}
		}
	}
}
