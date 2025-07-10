using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CusPermitController))]
	sealed class CusPermitControllerTest : Customs.Module.Testing.CusPermitControllerTest
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
}
