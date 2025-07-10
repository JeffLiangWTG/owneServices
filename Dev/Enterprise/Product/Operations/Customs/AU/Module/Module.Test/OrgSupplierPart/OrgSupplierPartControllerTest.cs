using System;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartController))]
	sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override Type GetBusinessObjectType() => typeof(AUOrgSupplierPart);

		public override Type ControllerToBashType => typeof(OrgSupplierPartController);
	}
}
