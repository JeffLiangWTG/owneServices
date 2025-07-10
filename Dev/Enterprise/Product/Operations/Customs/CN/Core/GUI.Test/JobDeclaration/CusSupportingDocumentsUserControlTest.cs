using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CusSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using (var control = new CusSupportingDocumentsUserControl())
			{
				TestUtility.AssertControlExistance(control, "CusSupportingDocumentsGrid", "FilteredCusSupportingDocuments");
				TestUtility.AssertControlExistance(control, "CusSupportingDocumentsGroupBox");
			}
		}
	}
}
