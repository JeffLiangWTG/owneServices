using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.GUI.Testing;

class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
{
	protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
	Dictionary<string, int> AEBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseValidateAllExpectedHits => new Dictionary<string, int> { { GlbBranchSchema.Constants.TableName, 6 }, { OrgHeaderSchema.Constants.TableName, 5 } };
	Dictionary<string, int> AEBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseFormMergeExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> AEBaseDeleteExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AELoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEValidateAllExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AELightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEFormMergeExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEUniversalXMLAddExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> AEDeleteExpectedHits => new Dictionary<string, int>();
	protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(AEBaseLoadEditableChildObjectsExpectedHits, AELoadEditableChildObjectsExpectedHits);
	protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(AEBaseValidateAllExpectedHits, AEValidateAllExpectedHits);
	protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(AEBaseLightFormValidationAndSaveExpectedHits, AELightFormValidationAndSaveExpectedHits);
	protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(AEBaseFormMergeExpectedHits, AEFormMergeExpectedHits);
	protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(AEBaseUniversalXMLExportExpectedHits, AEUniversalXMLExportExpectedHits);
	protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(AEBaseUniversalXMLImportUpdateExpectedHits, AEUniversalXMLImportUpdateExpectedHits);
	protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(AEBaseUniversalXMLAddExpectedHits, AEUniversalXMLAddExpectedHits);
	protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(AEBaseDeleteExpectedHits, AEDeleteExpectedHits);

	protected override void SetUp()
	{
		base.SetUp();
		var customsInterface = new LocalCountryCustomsInterface();
		customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
		localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
	}
	IDisposable localCountryCustomsInterface;

	protected override void TearDown()
	{
		base.TearDown();
		localCountryCustomsInterface?.Dispose();
	}
}
