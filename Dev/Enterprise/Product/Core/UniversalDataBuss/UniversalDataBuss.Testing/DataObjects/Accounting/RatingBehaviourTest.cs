using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(RatingBehaviour))]
	class RatingBehaviourTest : DataObjectTestCase<RatingBehaviour>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(RatingUnit.Code), JobConsolCostSchema.E6_RatingBehaviour.MaxLength },
				{ nameof(RatingUnit.Description), 35 }
			};
		}
	}
}
