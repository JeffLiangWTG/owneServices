using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	abstract class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override void FinaliseCreateFullyPopulatedObject(BaseJobDeclaration declaration)
		{
			base.FinaliseCreateFullyPopulatedObject(declaration);
			declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
			{
				x.Attributes.Cast<AttributeCusCodeData>().Where(a => a.TariffProfileQuestion == null).DeleteAll();
			});
		}

		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		protected virtual Dictionary<string, int> BRBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> BRBaseValidateAllExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> BRBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> BRBaseFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};
		protected virtual Dictionary<string, int> BRBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRBaseDeleteExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};

		protected virtual Dictionary<string, int> BRLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>() { };
		protected virtual Dictionary<string, int> BRUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> BRDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(BRBaseLoadEditableChildObjectsExpectedHits, BRLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(BRBaseValidateAllExpectedHits, BRValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(BRBaseLightFormValidationAndSaveExpectedHits, BRLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(BRBaseFormMergeExpectedHits, BRFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(BRBaseUniversalXMLExportExpectedHits, BRUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(BRBaseUniversalXMLImportUpdateExpectedHits, BRUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(BRBaseUniversalXMLAddExpectedHits, BRUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(BRBaseDeleteExpectedHits, BRDeleteExpectedHits);
	}
}
