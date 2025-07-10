using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(State))]
	class StateTest : DataObjectTestCase<State>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(State.Code), RefCountryStatesSchema.RW_Code.MaxLength },
			{ nameof(State.Name), RefCountryStatesSchema.RW_Description.MaxLength },
			{ nameof(State.Region), RefCountryStatesSchema.RW_RegionName.MaxLength }
		};
	}
}

