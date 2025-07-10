using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Group))]
	class GroupTest : DataObjectTestCase<Group>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Group.Code), GlbGroupSchema.GG_Code.MaxLength },
			{ nameof(Group.Name), GlbGroupSchema.GG_Desc.MaxLength }
		};
	}
}
