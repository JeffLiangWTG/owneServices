using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	public class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public void TestTopLevelMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();
				AssertType<EDIMenu>(testForm.TopLevelMenu);
			}
		}

		public override CargoWise.Types.ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceheader = declaration.Invoices.AddNew();
			invoiceheader.InvoiceLines.AddNew();
			declaration.CusContainers.AddNew();
			declaration.Bills.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			_ = entryInstruction.GoodsLocationDescription;
			return declaration;
		}
	}

	class JobDeclarationFormPerformanceTest : EU.GUI.Testing.JobDeclarationFormPerformanceTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);
		Dictionary<string, int> FRBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		Dictionary<string, int> FRBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8 },
			{ RefPacksSchema.Constants.TableName, 6 }
		};

		Dictionary<string, int> FRBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8 },
			{ RefPacksSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> FRBaseFormMergeExpectedHits => new Dictionary<string, int>()
		{
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, 60 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 12 }
		};
		Dictionary<string, int> FRBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ OrgHeaderSchema.Constants.TableName, 12 },
			{ OrgAddressSchema.Constants.TableName, 10 },
			{ RefPacksSchema.Constants.TableName, 6 },
			{ CusPermitHeaderSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> FRBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 },
		};

		Dictionary<string, int> FRBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ OrgHeaderSchema.Constants.TableName, 11 },
			{ OrgAddressSchema.Constants.TableName, 11 },
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};
		Dictionary<string, int> FRBaseDeleteExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 }
		};

		protected virtual Dictionary<string, int> FRUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 9 }
		};
		protected virtual Dictionary<string, int> FRUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 8 }
		};
		protected virtual Dictionary<string, int> FRLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> FRValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> FRLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> FRFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> FRUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> FRDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> EULoadEditableChildObjectsExpectedHits => ZipDictionaries(FRBaseLoadEditableChildObjectsExpectedHits, FRLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> EUValidateAllExpectedHits => ZipDictionaries(FRBaseValidateAllExpectedHits, FRValidateAllExpectedHits);
		protected override Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => ZipDictionaries(FRBaseLightFormValidationAndSaveExpectedHits, FRLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> EUFormMergeExpectedHits => ZipDictionaries(FRBaseFormMergeExpectedHits, FRFormMergeExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLExportExpectedHits => ZipDictionaries(FRBaseUniversalXMLExportExpectedHits, FRUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> EUUniversalXMLImportUpdateExpectedHits => ZipDictionaries(FRBaseUniversalXMLImportUpdateExpectedHits, FRUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(FRBaseUniversalXMLAddExpectedHits, FRUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> EUDeleteExpectedHits => ZipDictionaries(FRBaseDeleteExpectedHits, FRDeleteExpectedHits);
	}

	class JobDeclarationFormForTest : JobDeclarationForm
	{
		public JobDeclarationFormForTest()
		{
		}

		public JobDeclarationFormForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public new ZMenuItem TopLevelMenu => base.TopLevelMenu;
	}
}
