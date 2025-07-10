using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationController))]
class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
{
	public override Type ControllerToBashType => typeof(JobDeclarationController);

	protected override bool CountryHasExWarehouse => false;
}
