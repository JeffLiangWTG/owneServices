using System;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType => typeof(JobDeclarationController);
	}
}
