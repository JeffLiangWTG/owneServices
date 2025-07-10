using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TaxTransaction))]
	class TaxTransactionTest : DataObjectTestCase<TaxTransaction>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(TaxTransaction.Ledger), AccTaxTransactionSchema.ATT_Ledger.MaxLength }
			};
		}
	}
}
