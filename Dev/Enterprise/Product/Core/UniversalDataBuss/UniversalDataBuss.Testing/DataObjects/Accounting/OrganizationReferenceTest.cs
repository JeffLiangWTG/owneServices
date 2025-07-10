using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(OrganizationReference))]
	class OrganizationReferenceTest : DataObjectTestCase<OrganizationReference>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			int orgRefKeyMaxLength = new[] { OrgHeaderSchema.OH_Code.MaxLength, GetDefaultFieldLength() }.Max();

			return new Dictionary<string, int>()
			{
				{ nameof(OrganizationReference.Key), orgRefKeyMaxLength },
				{ nameof(OrganizationReference.Type), GetDefaultFieldLength() }
			};
		}
	}
}
