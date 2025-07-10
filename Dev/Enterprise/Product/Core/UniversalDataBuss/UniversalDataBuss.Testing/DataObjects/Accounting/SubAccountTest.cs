using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(SubAccount))]
	class SubAccountTest : DataObjectTestCase<SubAccount>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			int subAccountCodeMaxLength = new[] { OrgHeaderSchema.OH_Code.MaxLength, AccGroupsSchema.AR_Code.MaxLength, GlbStaffSchema.GS_Code.MaxLength, GlbGroupSchema.GG_Code.MaxLength, GetDefaultFieldLength() }.Max();

			return new Dictionary<string, int>()
			{
				{ nameof(SubAccount.Code), subAccountCodeMaxLength }
			};
		}
	}
}
