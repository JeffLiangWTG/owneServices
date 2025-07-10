using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionImportWizardSettingsTest : XmlSerializableTestCase<CommissionImportWizardSettings>
	{
		public void TestWriteXml()
		{
			var settings = new CommissionImportWizardSettings();
			settings.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;

			var result = Serialize(settings);
			Assert(result.Contains("<ImportType>OVRDIF</ImportType>"));
		}
	}
}
