using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(JobDeclarationShipmentController))]
class JobDeclarationShipmentControllerTest : Customs.Module.Testing.JobDeclarationShipmentControllerTest
{
	public override Type ControllerToBashType => typeof(JobDeclarationShipmentController);
}
