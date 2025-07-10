using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI.DataMapping;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing;

[TestedType(typeof(CommissionImportWizard))]
public class CommissionImportWizardTest : NonPersistentBusinessObjectTestCase
{
	#region Settings

	public void TestGetSettings()
	{
		var collection = new CommissionFlattenedCollection(Factory);
		var collectionInfo = new CommissionImportCollectionInfo(collection, false);
		using (var importWizard = new CommissionImportWizard(collectionInfo, new StmModuleFilterSettingsStorage("XXX", "XXX"), null))
		{
			importWizard.ImportType = CommissionImportTypeList.Codes.Add;
			{
				var settings = importWizard.GetSettings();
				AssertType(typeof(CommissionImportWizardSettings), settings);
				var commissionImportSettings = (CommissionImportWizardSettings)settings;
				AssertEquals(CommissionImportTypeList.Codes.Add, commissionImportSettings.ImportType);
			}

			importWizard.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;
			{
				var settings = importWizard.GetSettings();
				var commissionImportSettings = (CommissionImportWizardSettings)settings;
				AssertEquals(CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip, commissionImportSettings.ImportType);
			}
		}
	}

	public void TestLoadSettings()
	{
		var commissionImportSettings1 = new CommissionImportWizardSettings();
		commissionImportSettings1.ImportType = CommissionImportTypeList.Codes.Add;

		var commissionImportSettings2 = new CommissionImportWizardSettings();
		commissionImportSettings2.ImportType = "";

		var commissionImportSettings3 = new CommissionImportWizardSettings();
		commissionImportSettings3.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;

		var settingsStorage = new StmModuleFilterSettingsStorage("XXX", "XXX");
		settingsStorage.SaveSettings("1", commissionImportSettings1.AsXml());
		settingsStorage.SaveSettings("2", commissionImportSettings2.AsXml());
		settingsStorage.SaveSettings("3", commissionImportSettings3.AsXml());

		var collection = new CommissionFlattenedCollection(Factory);
		var collectionInfo = new CommissionImportCollectionInfo(collection, false);
		using (var importWizard = new CommissionImportWizard(collectionInfo, settingsStorage, null))
		{
			importWizard.Setting = "1";
			AssertEquals(CommissionImportTypeList.Codes.Add, importWizard.ImportType);

			importWizard.Setting = "2";
			AssertEquals("", importWizard.ImportType);

			importWizard.Setting = "3";
			AssertEquals(CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip, importWizard.ImportType);
		}
	}

	#endregion

	#region Overrides

	protected override BusinessObject GetNewBusinessObject()
	{
		var collection = new CommissionFlattenedCollection(Factory);
		var collectionInfo = new CommissionImportCollectionInfo(collection, false);
		return new CommissionImportWizard(collectionInfo, new StmModuleFilterSettingsStorage("XXX", "XXX"), new FileMapper());
	}

	#endregion
}

public class CommissionImportWizardValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateImportType()
	{
		var importWizard = GetNewImportWizard();

		importWizard.ImportType = "XXX";
		AssertListValidationInvalidCodeError(importWizard.ImportTypeInfo, true);
		AssertMandatoryValidationError(importWizard.ImportTypeInfo, false);

		importWizard.ImportType = CommissionImportTypeList.Codes.OverrideIfChangedOtherwiseSkip;
		AssertListValidationInvalidCodeError(importWizard.ImportTypeInfo, false);
		AssertMandatoryValidationError(importWizard.ImportTypeInfo, false);

		importWizard.ImportType = "";
		AssertListValidationInvalidCodeError(importWizard.ImportTypeInfo, false);
		AssertMandatoryValidationError(importWizard.ImportTypeInfo, true);
	}

	#region Implementation

	CommissionImportWizard GetNewImportWizard()
	{
		var collection = new CommissionFlattenedCollection(Factory);
		var collectionInfo = new CommissionImportCollectionInfo(collection, false);
		return new CommissionImportWizard(collectionInfo, new StmModuleFilterSettingsStorage("XXX", "XXX"), null);
	}

	#endregion
}
