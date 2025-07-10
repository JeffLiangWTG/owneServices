using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(LinkedTransactionID))]
	class LinkedTransactionIDTest : DataObjectTestCase<LinkedTransactionID>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(LinkedTransactionID.Type), 35 },	// This value will not be saved to database and it's in line with Type in DataSource.cs
				{ nameof(LinkedTransactionID.Key), 50 },	// This value will not be saved to database and it's in line with Key in DataSource.cs
			};
		}
	}
}
