using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	class AssignLocationAndReferenceFormHelperTest : TestCaseWithFactory
	{
		public void TestGetDataFromUpdateLocationAndReferenceForm()
		{
			using (var form = new ZForm())
			{
				ZFormModaliser.ShowDialogsInTest = false;
				CombineAssertions(() =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					var response = AssignLocationAndReferenceFormHelper.GetDataFromUpdateLocationAndReferenceForm();
					AssertNull("GetDataFromUpdateLocationAndReferenceForm returns null when cancel", response);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					response = AssignLocationAndReferenceFormHelper.GetDataFromUpdateLocationAndReferenceForm();
					AssertNotNull("GetDataFromUpdateLocationAndReferenceForm returns DataToUpdateLocationAndReference object when Ok", response);
				});
			}
		}
	}
}
