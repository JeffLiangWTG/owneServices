using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[TestedType(typeof(UNLOCO))]
	class UNLOCOTest : DataObjectTestCase<UNLOCO>
	{
		protected override bool ShouldBeFlattenedIntoAttributes => true;

		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(UNLOCO.Code), RefUNLOCOSchema.RL_Code.MaxLength },
			{ nameof(UNLOCO.Name), RefUNLOCOSchema.RL_PortName.MaxLength }
		};
	}
}
