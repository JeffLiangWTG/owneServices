using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.China;
	}
}
