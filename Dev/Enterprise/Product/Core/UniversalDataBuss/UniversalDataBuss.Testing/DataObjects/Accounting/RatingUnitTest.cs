using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(RatingUnit))]
	class RatingUnitTest : DataObjectTestCase<RatingUnit>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(RatingUnit.Code), JobPaymentBasisSchema.PBS_ChargeableUnit.MaxLength },
				{ nameof(RatingUnit.Description), 80 }
			};
		}
	}
}
