using System;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Botswana;

		public override Type ModuleToBashType => typeof(OrgSupplierPartModule);
	}
}
