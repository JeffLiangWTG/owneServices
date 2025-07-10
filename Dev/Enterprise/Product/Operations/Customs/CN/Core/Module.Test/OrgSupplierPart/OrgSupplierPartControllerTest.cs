using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		protected override string CountryCode => Enterprise.Core.Constants.CountryCodes.China;
	}
}
