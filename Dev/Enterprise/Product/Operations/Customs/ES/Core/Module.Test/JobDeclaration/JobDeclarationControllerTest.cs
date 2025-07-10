using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public class JobDeclarationControllerTest : EU.Module.Testing.JobDeclarationControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(JobDeclarationController); }
		}
	}
}
