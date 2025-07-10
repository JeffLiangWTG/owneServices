using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Japan; }
		}
	}
}
