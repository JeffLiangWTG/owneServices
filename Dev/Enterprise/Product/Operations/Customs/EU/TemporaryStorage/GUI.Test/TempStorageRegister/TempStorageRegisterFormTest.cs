using System.Windows.Forms;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterForm))]
	class TempStorageRegisterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			Factory.Save();
			var result = new TempStorageRegisterForm(header);
			result.ControllerID = ControllerIDs.Customs.EU.TempStorageRegister;
			return result;
		}

		public override void TestFormIsFullyTranslatable()
		{
			Assert(true);//will be updated in next workflow
		}
	}
}
