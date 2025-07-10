using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TransactionHeaderReference))]
	class TransactionHeaderReferenceTest : DataObjectTestCase<TransactionHeaderReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(TransactionHeaderReference.Type), AccTransactionHeaderReferenceSchema.AH1_Type.MaxLength },
				{ nameof(TransactionHeaderReference.TypeDescription),  80 },
				{ nameof(TransactionHeaderReference.Reference), AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength },
				{ nameof(TransactionHeaderReference.ReferenceDescription),  80 }
			};
		}
	}
}

