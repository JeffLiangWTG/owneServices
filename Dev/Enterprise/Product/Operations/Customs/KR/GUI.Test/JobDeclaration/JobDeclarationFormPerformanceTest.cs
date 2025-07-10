using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(KRBaseDeleteExpectedHits, KRDeleteExpectedHits);

		Dictionary<string, int> KRBaseDeleteExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> KRDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(KRBaseValidateAllExpectedHits, KRValidateAllExpectedHits);
		Dictionary<string, int> KRBaseValidateAllExpectedHits => new Dictionary<string, int>() {
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 5 },
			{ OrgContactSchema.Constants.TableName, 5 },//This is for KRC
			{ OrgCusCodeSchema.Constants.TableName, 5 }
		};
		Dictionary<string, int> KRValidateAllExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(KRBaseLightFormValidationAndSaveExpectedHits, KRLightFormValidationAndSaveExpectedHits);
		Dictionary<string, int> KRBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>() {
			{ JobComInvLineRefsSchema.Constants.TableName, 5 },
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 5 },
			{ OrgContactSchema.Constants.TableName, 5 },
			{ OrgCusCodeSchema.Constants.TableName, 5 }
		};
		Dictionary<string, int> KRLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(KRBaseuniversalXMLImportUpdateExpectedHits, KRuniversalXMLImportUpdateExpectedHits);
		Dictionary<string, int> KRBaseuniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>() {
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 12 }
		};
		Dictionary<string, int> KRuniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	}
}
