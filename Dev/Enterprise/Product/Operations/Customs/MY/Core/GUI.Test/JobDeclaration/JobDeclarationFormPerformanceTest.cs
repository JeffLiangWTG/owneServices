using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((Business.JobDeclaration)bizO);

		Dictionary<string, int> MYBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseValidateAllExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseFormMergeExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvHeaderChargeSchema.Constants.TableName, 134 }
		};
		Dictionary<string, int> MYBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> MYBaseDeleteExpectedHits => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> MYLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> MYDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(MYBaseLoadEditableChildObjectsExpectedHits, MYLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(MYBaseValidateAllExpectedHits, MYValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(MYBaseLightFormValidationAndSaveExpectedHits, MYLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(MYBaseFormMergeExpectedHits, MYFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(MYBaseUniversalXMLExportExpectedHits, MYUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(MYBaseUniversalXMLImportUpdateExpectedHits, MYUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(MYBaseUniversalXMLAddExpectedHits, MYUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(MYBaseDeleteExpectedHits, MYDeleteExpectedHits);

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

	sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}

	sealed class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override bool DeclarationIsCancelled => true;
	}
}
