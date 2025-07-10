using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZArchitecture.GUI.ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		Dictionary<string, int> CABaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> CABaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 11 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ GlbBranchSchema.Constants.TableName, 5 },
			{ CusCALPCOSchema.Constants.TableName, 10 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 }
		};
		Dictionary<string, int> CABaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 11 },
			{ CusCALPCOSchema.Constants.TableName, 9 }
		};
		Dictionary<string, int> CABaseFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> CABaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 12 },
			{ CusCodeDataSchema.Constants.TableName, 14 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 },
			{ CusCALPCOSchema.Constants.TableName, 60 }
		};
		Dictionary<string, int> CABaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 10 },
			{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			{ StmNoteSchema.Constants.TableName, 9 },
			{ GenPivotSchema.Constants.TableName, 6 },
			{ CusCALPCOSchema.Constants.TableName, 9 }
		};
		Dictionary<string, int> CABaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
		};
		Dictionary<string, int> CABaseDeleteExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 5 },
			{ StmNoteSchema.Constants.TableName, 6 },
			{ GenPivotSchema.Constants.TableName, 6 },
			{ CusCALPCOSchema.Constants.TableName, 5 }
		};

		protected virtual Dictionary<string, int> CALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CAValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};
		protected virtual Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CAUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CAUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> CADeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(CABaseLoadEditableChildObjectsExpectedHits, CALoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(CABaseValidateAllExpectedHits, CAValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(CABaseLightFormValidationAndSaveExpectedHits, CALightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(CABaseFormMergeExpectedHits, CAFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(CABaseUniversalXMLExportExpectedHits, CAUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(CABaseUniversalXMLImportUpdateExpectedHits, CAUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(CABaseUniversalXMLAddExpectedHits, CAUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(CABaseDeleteExpectedHits, CADeleteExpectedHits);
	}
}
