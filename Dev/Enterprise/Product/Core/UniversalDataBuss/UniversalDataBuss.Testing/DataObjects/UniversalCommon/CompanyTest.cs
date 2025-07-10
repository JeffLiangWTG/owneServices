using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Company))]
	class CompanyTest : DataObjectTestCase<Company>
	{
		protected override bool ShouldBeFlattenedIntoAttributes
		{
			get { return false; } // Company is a CodeDecriptionPair, but also has a Country child element.
		}

		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Company.Code), GlbCompanySchema.GC_Code.MaxLength },
			{ nameof(Company.Name), GlbCompanySchema.GC_Name.MaxLength }
		};
	}
}

