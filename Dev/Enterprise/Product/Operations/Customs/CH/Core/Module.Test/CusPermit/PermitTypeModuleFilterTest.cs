using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(Customs.Module.PermitTypeModuleFilter))]
class PermitTypeModuleFilterTest : Customs.Module.Testing.PermitTypeModuleFilterTest
{
	public void TestCheckPropertyMaxLength()
	{
		var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new CodeDescriptionPairList());
		AssertEquals(PermitTypeModuleFilter.Schema.Property1MaxLength, permitTypeModuleFilter.GetPossiblyCustomPropertyMaxLength(nameof(PermitTypeModuleFilter.Property1)));
	}
}
