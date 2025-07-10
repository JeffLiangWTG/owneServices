using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(Branch))]
	class BranchTest : DataObjectTestCase<Branch>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Branch.Code), GlbBranchSchema.GB_Code.MaxLength },
			{ nameof(Branch.Name), GlbBranchSchema.GB_BranchName.MaxLength }
		};
	}
}

