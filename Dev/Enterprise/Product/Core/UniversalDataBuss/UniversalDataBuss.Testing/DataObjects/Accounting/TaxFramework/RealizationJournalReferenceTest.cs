using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(RealizationJournalReference))]
	class RealizationJournalReferenceTest : DataObjectTestCase<RealizationJournalReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(RealizationJournalReference.Ledger), AccTransactionHeaderSchema.AH_Ledger.MaxLength },
				{ nameof(RealizationJournalReference.TransactionNumber), AccTransactionHeaderSchema.AH_TransactionNum.MaxLength },
				{ nameof(RealizationJournalReference.Category), AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength },
				{ nameof(RealizationJournalReference.Description), AccTransactionHeaderSchema.AH_Desc.MaxLength }
			};
		}
	}
}
