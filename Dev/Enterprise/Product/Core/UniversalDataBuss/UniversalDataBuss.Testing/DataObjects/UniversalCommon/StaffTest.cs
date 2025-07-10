using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Staff))]
	class StaffTest : DataObjectTestCase<Staff>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>()
		{
			{ nameof(Staff.Code), GlbStaffSchema.GS_Code.MaxLength },
			{ nameof(Staff.Name), GlbStaffSchema.GS_FullName.MaxLength }
		};
	}
}

