using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(JobDeclarationController))]
	public class JobDeclarationControllerTest : Customs.Module.Testing.JobDeclarationControllerTestCase
	{
		public override Type ControllerToBashType
		{
			get { return typeof(JobDeclarationController); }
		}

		protected override bool CountryHasExWarehouse
		{
			get { return false; }
		}
	}
}
