using System;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);
	}
}
