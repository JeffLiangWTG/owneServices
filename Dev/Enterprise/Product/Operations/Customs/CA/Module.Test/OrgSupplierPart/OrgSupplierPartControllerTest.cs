using System;
using Enterprise.Customs.CA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		public override Type ControllerToBashType => typeof(OrgSupplierPartController);

		protected override Type GetBusinessObjectType() => typeof(OrgSupplierPart);

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
