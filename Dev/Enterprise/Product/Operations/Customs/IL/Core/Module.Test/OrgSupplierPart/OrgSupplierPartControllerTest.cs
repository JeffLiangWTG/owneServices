using System;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override string CountryCode => Core.Constants.CountryCodes.Israel;
	}
}
