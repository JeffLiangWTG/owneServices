using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Country))]
	class CountryTest : DataObjectTestCase<Country>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Country.Code), RefCountrySchema.RN_Code.MaxLength },
			{ nameof(Country.Name), RefCountrySchema.RN_Desc.MaxLength }
		};
	}
}

