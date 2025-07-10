using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(CashAdvanceRequestLine))]
	class CashAdvanceRequestLineTest : DataObjectTestCase<CashAdvanceRequestLine>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(CashAdvanceRequestLine.RequestReferenceNumber), AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber.MaxLength },
				{ nameof(CashAdvanceRequestLine.Status), AccCashAdvanceRequestLineSchema.CAL_Status.MaxLength }
			};
		}
	}
}

