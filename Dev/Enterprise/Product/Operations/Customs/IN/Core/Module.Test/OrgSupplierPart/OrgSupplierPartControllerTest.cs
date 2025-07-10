using System;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(OrgSupplierPartController))]
sealed class OrgSupplierPartControllerTest : Customs.Module.Testing.OrgSupplierPartControllerTest
{
	public override Type ControllerToBashType => typeof(OrgSupplierPartController);

	protected override string CountryCode => Core.Constants.CountryCodes.India;
}
