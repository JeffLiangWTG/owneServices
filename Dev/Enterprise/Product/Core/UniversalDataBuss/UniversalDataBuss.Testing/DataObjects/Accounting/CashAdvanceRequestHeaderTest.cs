using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(CashAdvanceRequestHeader))]
	class CashAdvanceRequestHeaderTest : DataObjectTestCase<CashAdvanceRequestHeader>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(CashAdvanceRequestHeader.RequestReferenceNumber), AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber.MaxLength },
				{ nameof(CashAdvanceRequestHeader.Status), AccCashAdvanceRequestHeaderSchema.CAH_Status.MaxLength },
				{ nameof(CashAdvanceRequestHeader.Ledger), AccCashAdvanceRequestHeaderSchema.CAH_Ledger.MaxLength }
			};
		}
	}
}

