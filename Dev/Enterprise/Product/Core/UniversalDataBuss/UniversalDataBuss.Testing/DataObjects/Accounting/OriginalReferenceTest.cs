using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(OriginalReference))]
	class OriginalReferenceTest : DataObjectTestCase<OriginalReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(OriginalReference.OriginalTransactionNumber), AccTransactionHeaderSchema.AH_TransactionNum.MaxLength },
				{ nameof(OriginalReference.OriginalTransactionJobInvoiceNumber), AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength },
				{ nameof(OriginalReference.OriginalTransactionReference), AccTransactionHeaderSchema.AH_TransactionReference.MaxLength },
				{ nameof(OriginalReference.OriginalTransactionComplianceSubType), AccTransactionHeaderSchema.AH_ComplianceSubType.MaxLength },
			};
		}
	}
}
