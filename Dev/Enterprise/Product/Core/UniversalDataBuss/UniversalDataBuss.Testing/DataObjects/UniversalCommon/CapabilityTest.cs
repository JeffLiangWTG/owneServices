using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Capability))]
	class CapabilityTest : DataObjectTestCase<Capability>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Capability.Code), GlbCapabilitySchema.G4_Code.MaxLength },
			{ nameof(Capability.Name), GlbCapabilitySchema.G4_Description.MaxLength }
		};
	}
}
