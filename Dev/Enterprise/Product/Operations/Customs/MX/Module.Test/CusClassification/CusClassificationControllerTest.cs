using System;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Module.Testing
{
	[TestedType(typeof(CusClassificationController))]
	class CusClassificationControllerTest : Customs.Module.Testing.SingleTariffClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(CusClassificationController);
	}
}
