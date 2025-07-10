using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(OrganisationData))]
	internal class OrganisationDataTest : DataObjectTestCase<OrganisationData>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(OrganisationData.Code), OrgHeaderSchema.OH_Code.MaxLength },
			{ nameof(OrganisationData.Name), OrgHeaderSchema.OH_FullName.MaxLength }
		};
	}
}
