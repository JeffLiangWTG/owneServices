using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(PostingJournal))]
	class PostingJournalTest : DataObjectTestCase<PostingJournal>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(PostingJournal.TransactionType), AccTransactionHeaderSchema.AH_TransactionType.MaxLength },
				{ nameof(PostingJournal.Description), AccTransactionLinesSchema.AL_Desc.MaxLength },
				{ nameof(PostingJournal.RevenueRecognitionType), AccTransactionLinesSchema.AL_RevRecognitionType.MaxLength },
				{ nameof(PostingJournal.TransactionCategory), AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength },
				{ nameof(PostingJournal.GovernmentReportingChargeCode), AccTransactionLinesSchema.AL_GovtChargeCode.MaxLength },
				{ nameof(PostingJournal.LocalGLAccount), AccGLAccountDescriptorSchema.AJ_LocalAccountNumber.MaxLength },
				{ nameof(PostingJournal.SupplyType), AccTransactionLinesSchema.AL_SupplyType.MaxLength }
			};
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(PostingJournal.Description)
		};
	}
}

