using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(Department))]
	class DepartmentTest : DataObjectTestCase<Department>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Department.Code), GlbDepartmentSchema.GE_Code.MaxLength },
			{ nameof(Department.Name), GlbDepartmentSchema.GE_Desc.MaxLength }
		};
	}
}

