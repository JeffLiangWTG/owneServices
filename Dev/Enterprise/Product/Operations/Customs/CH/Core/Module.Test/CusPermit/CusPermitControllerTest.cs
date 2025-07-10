using System;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CusPermitController))]
class CusPermitControllerTest : Customs.Module.Testing.CusPermitControllerTest
{
	public void TestGetForm_ReturnType()
	{
		var permit = Factory.NewWithValidTestData<CusPermitHeader>();
		Factory.Save();
		using (var form = Controller.ShowEditForm(permit))
		{
			AssertType<CusPermitForm>(form);
		}
	}

	public override Type ControllerToBashType => typeof(CusPermitController);
}
