using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(AdditionalReference))]
	class AdditionalReferenceTest : DataObjectTestCase<AdditionalReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(AdditionalReference.ReferenceNumber), 35 },
				{ nameof(AdditionalReference.ContextInformation), 50 }
			};
		}
	}
}
