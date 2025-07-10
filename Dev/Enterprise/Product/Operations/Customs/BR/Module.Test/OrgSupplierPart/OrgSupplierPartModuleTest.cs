using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	public class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode
		{
			get
			{
				return Core.Constants.CountryCodes.Brazil;
			}
		}
	}
}
