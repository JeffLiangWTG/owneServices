using System;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	sealed class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType
		{
			get { return typeof(JobDeclarationController); }
		}

		protected override bool CountryHasExWarehouse => false;
	}
}
