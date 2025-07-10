using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class PartPivotLayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestSupportingDocumentsFieldsControlBindingString()
		{
			using (var control = new PartPivotLayoutSupportingDocumentsUserControlForTest())
			{
				AssertEquals("SupportingDocumentsFieldsControlBindingString", "PivotsForBinding.SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingString);
			}
		}
	}

	public class PartPivotLayoutSupportingDocumentsUserControlForTest : PartPivotLayoutSupportingDocumentsUserControl
	{
		public new string GetSupportingDocumentsFieldsControlBindingString => GetSupportingDocumentsFieldsControlBindingString();
	}
}
