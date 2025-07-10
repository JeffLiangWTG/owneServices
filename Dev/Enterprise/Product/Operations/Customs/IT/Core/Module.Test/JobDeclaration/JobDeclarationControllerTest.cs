using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(JobDeclarationController))]
sealed class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
{
	public override Type ControllerToBashType
	{
		get { return typeof(JobDeclarationController); }
	}
}
