using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(PostingRelatedJournal))]
	class PostingRelatedJournalTest : DataObjectTestCase<PostingRelatedJournal>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(PostingRelatedJournal.TransactionType), AccTransactionHeaderSchema.AH_TransactionType.MaxLength },
				{ nameof(PostingRelatedJournal.Description), AccTransactionLinesSchema.AL_Desc.MaxLength },
				{ nameof(PostingRelatedJournal.TransactionCategory), AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength },
				{ nameof(PostingRelatedJournal.Ledger), AccTransactionHeaderSchema.AH_Ledger.MaxLength },
				{ nameof(PostingRelatedJournal.GLAccount), AccTransactionHeaderSchema.AH_AG.MaxLength }
			};
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(PostingRelatedJournal.Description)
		};
	}
}

