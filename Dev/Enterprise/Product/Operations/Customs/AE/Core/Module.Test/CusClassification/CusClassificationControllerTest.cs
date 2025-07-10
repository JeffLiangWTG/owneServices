using System;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(CusClassificationController))]
public class CusClassificationControllerTest : Customs.Module.Testing.SingleTariffClassificationControllerTest
{
	public override Type ControllerToBashType
	{
		get
		{
			return typeof(CusClassificationController);
		}
	}
}
