using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Test.Utilities.GenericImportExport
{
	[TestedType(typeof(ImportExportWizard))]
	public abstract class ImportExportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSaveSettings_WhenSettingIsNewOrUpdated_ShouldCheckSecurity()
		{
			var isShowSecurityErrorCalled = GetShowSecurityErrorCalled(ZString.Empty, Array.Empty<string>());
			AssertEquals("ShowSecurityError() should not be called if setting is empty", false, isShowSecurityErrorCalled);

			isShowSecurityErrorCalled = GetShowSecurityErrorCalled("BBB", new[] { "AAA" });
			AssertEquals("ShowSecurityError() should be called if setting is newly created", true, isShowSecurityErrorCalled);

			isShowSecurityErrorCalled = GetShowSecurityErrorCalled("AAA", new[] { "AAA" });
			AssertEquals("ShowSecurityError() should not be called if setting has no change", false, isShowSecurityErrorCalled);

			var xmlString = @"
<DataImportWizardSettings>
	<StartingRow>1</StartingRow>
	<Mapping>
		<ColumnIndex>-1</ColumnIndex>
		<Name>Z0_DateOnly</Name>
		<DefaultValue />
		<Expression />
		<MapAs />
		<ProperCase>False</ProperCase>
		<UpdateExisting>False</UpdateExisting>
		<CustomMapList />
		<Delimiter />
		<WesternCharactersOnly>False</WesternCharactersOnly>
	</Mapping>
	<Delimiter>,</Delimiter>
	<TextQualifier>""</TextQualifier>
	<FilterIndex>0</FilterIndex>
</DataImportWizardSettings>";
			isShowSecurityErrorCalled = GetShowSecurityErrorCalled("AAA", new[] { "AAA" }, xmlString);
			AssertEquals("ShowSecurityError() should be called if existing setting has changes", true, isShowSecurityErrorCalled);

			bool GetShowSecurityErrorCalled(string settingName, string[] getSavedSettingsReturn, string loadSettingsReturn = null)
			{
				var showSecurityErrorCalled = false;

				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.HasSecurityRight()).Returns(false);
				settingsStorageStub.Setup(m => m.ShowSecurityError()).Callback(() => showSecurityErrorCalled = true);
				settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns( getSavedSettingsReturn );
				settingsStorageStub.Setup(m => m.LoadSettings(settingName)).Returns(loadSettingsReturn);

				var wizard = NewWizard(settingsStorageStub.Object);
				wizard.Setting = settingName;
				wizard.SaveSettings();
				return showSecurityErrorCalled;
			}
		}

		public abstract ImportExportWizard NewWizard(ISettingsStorage settingsStorage);
	}
}
