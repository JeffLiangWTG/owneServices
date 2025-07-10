using System;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JobDeclarationController);
			}
		}
	}
}
