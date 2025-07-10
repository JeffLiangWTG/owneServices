using System;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(JobDeclarationShipmentController); }
		}
	}
}
