using System;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(JobDeclarationShipmentController))]
	public class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
	{
		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JobDeclarationShipmentController);
			}
		}
	}
}
