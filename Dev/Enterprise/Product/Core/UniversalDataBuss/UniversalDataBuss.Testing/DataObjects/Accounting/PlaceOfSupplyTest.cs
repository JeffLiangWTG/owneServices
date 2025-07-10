using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(PlaceOfSupply))]
	class PlaceOfSupplyTest : DataObjectTestCase<PlaceOfSupply>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(PlaceOfSupply.Location), AccTransactionLinesSchema.AL_PlaceOfSupply.MaxLength },
				{ nameof(PlaceOfSupply.LocationType), AccTransactionLinesSchema.AL_PlaceOfSupplyType.MaxLength }
			};
		}
	}
}
