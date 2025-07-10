using System.Windows.Forms;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageForm))]
	class CusTempStorageFormBaseOnlyTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.CreateRelatedCusTempStorageDec();
			Factory.Save();
			return new CusTempStorageForm(header)
			{
				ControllerID = ZArchitecture.Modules.ControllerIDs.Customs.TemporaryStorage
			};
		}
	}
}
