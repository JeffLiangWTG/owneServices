using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(AuthorizationDetails))]
	class AuthorizationDetailsTest : DataObjectTestCase<AuthorizationDetails>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(AuthorizationDetails.Version),                  10 },    // Arbitrary length for version string (expecting "1" in most cases)
				{ nameof(AuthorizationDetails.GovernmentNumber),         AccTransactionHeaderAuthorisationRecordSchema.AHF_Number.MaxLength },
				{ nameof(AuthorizationDetails.GovernmentCounter),        AccTransactionHeaderAuthorisationRecordSchema.AHF_Counter.MaxLength },
				{ nameof(AuthorizationDetails.URL),                      2048 },  // DB field is nvarchar(max); 2k is the longest you can safely make a URL - https://stackoverflow.com/questions/417142/what-is-the-maximum-length-of-a-url-in-different-browsers
				{ nameof(AuthorizationDetails.DebtorRegistrationNumber), AccTransactionHeaderAuthorisationRecordSchema.AHF_DebtorNumber.MaxLength },
				{ nameof(AuthorizationDetails.IssuerCertificateID),      AccTransactionHeaderAuthorisationRecordSchema.AHF_IssuerCertificateIdentifier.MaxLength },
				{ nameof(AuthorizationDetails.PlaceOfIssue),             AccTransactionHeaderAuthorisationRecordSchema.AHF_PlaceOfIssue.MaxLength },
				{ nameof(AuthorizationDetails.GovernmentBatchReference), AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber.MaxLength },
			};
		}
	}
}

