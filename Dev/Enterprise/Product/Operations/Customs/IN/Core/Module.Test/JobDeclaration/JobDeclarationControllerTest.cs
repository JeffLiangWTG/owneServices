using System;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(JobDeclarationController))]
sealed class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
{
	public override Type ControllerToBashType => typeof(JobDeclarationController);
}
