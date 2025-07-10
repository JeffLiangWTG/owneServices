using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	sealed class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
