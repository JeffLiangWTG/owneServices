using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI.Testing;

abstract class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
{
	protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

	protected override void DecorateDeclaration(Customs.Business.BaseJobDeclaration baseDeclaration)
	{
		var declaration = (JobDeclaration)baseDeclaration;
		for (var i = 0; i < 6; i++)
		{
			DecorateEntryInstruction(declaration.CustomsEntryInstructions.AddNew(), i);
		}
	}

	protected virtual void DecorateEntryInstruction(CusEntryInstruction cusEntryInstruction, int i)
	{
	}

	Dictionary<string, int> CHBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int> { };
	Dictionary<string, int> CHBaseFormMergeExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> CHBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusVehicleSchema.Constants.TableName, 60 },
			{ CusSupportingInfoSchema.Constants.TableName, 8 },
			{ CusReferenceSchema.Constants.TableName, 6 }
		};
	Dictionary<string, int> CHBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			{ RefPacksSchema.Constants.TableName, 6 },
			{ CusCodeDataSchema.Constants.TableName, 11 }
		};
	Dictionary<string, int> CHBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ RefPacksSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 11 }
		};
	Dictionary<string, int> CHBaseDeleteExpectedHits => new Dictionary<string, int>();

	protected virtual Dictionary<string, int> CHBaseValidateAllExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHValidateAllExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHFormMergeExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHUniversalXMLAddExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> CHDeleteExpectedHits => new Dictionary<string, int>();
	protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(CHBaseLoadEditableChildObjectsExpectedHits, CHLoadEditableChildObjectsExpectedHits);
	protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(CHBaseValidateAllExpectedHits, CHValidateAllExpectedHits);
	protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(CHBaseLightFormValidationAndSaveExpectedHits, CHLightFormValidationAndSaveExpectedHits);
	protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(CHBaseFormMergeExpectedHits, CHFormMergeExpectedHits);
	protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(CHBaseUniversalXMLExportExpectedHits, CHUniversalXMLExportExpectedHits);
	protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(CHBaseUniversalXMLImportUpdateExpectedHits, CHUniversalXMLImportUpdateExpectedHits);
	protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(CHBaseUniversalXMLAddExpectedHits, CHUniversalXMLAddExpectedHits);
	protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(CHBaseDeleteExpectedHits, CHDeleteExpectedHits);
}
