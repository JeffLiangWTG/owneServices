using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

	public void TestAskToOverride01DISupportingDocumentsWhenSaving()
	{
		const string notificationMessage = "Do you want to update the data for document 01DI with a placeholder ('X')?";

		var organisationWithDOI = Factory.New<OrgHeader>();
		organisationWithDOI.OH_Code = "IMPORTER";
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "DOI", permitHolder: organisationWithDOI.PK, "X", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		Factory.Save();

		using (var form = new JobDeclarationForm(declaration))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = organisationWithDOI.PK;
			entryInstruction.ZG_UseDeclarationOfIntent = true;
			var supportingDocument01DI1 = invoice.SupportingDocuments.AddNew();
			var supportingDocument01DI2 = invoice.SupportingDocuments.AddNew();
			supportingDocument01DI1.CSI_Code = supportingDocument01DI2.CSI_Code = Business.UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
			supportingDocument01DI1.CSI_ReferenceNumber = "X";
			supportingDocument01DI2.CSI_ReferenceNumber = "20123111223312345123456";
			CombineAssertions("Should ask", () =>
			{
				Assert(declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
				(form as IShowPreSaveDialog).ShowPreSaveDialogs();
				AssertEquals(notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});

			invoice.SupportingDocuments.RemoveAndDelete(supportingDocument01DI2);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions("Should not ask", () =>
			{
				Assert(!declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
				(form as IShowPreSaveDialog).ShowPreSaveDialogs();
				AssertNotEquals(notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestShipmentDetailsIncoTermPlaceControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new JobDeclarationForm(declaration))
		{
			form.Show();

			declaration.JE_ShipmentIncoTerm = "FOB";

			var agreedPlaceCodeDropEdit = GetAgreedPlaceCodeDropEdit(form);
			var agreedPlaceCodeFindBox = GetAgreedPlaceCodeFindBox(form);
			var additionalDeliveryTerms = GetAdditionalDeliveryTermsTextBox(form);

			CombineAssertions("When JE_ShipmentIncoTerm != XXX", () =>
			{
				AssertEquals("AgreedPlaceDropEdit Visible", false, agreedPlaceCodeDropEdit?.Visible);
				AssertEquals("AgreedPlaceCodeFindBox Visible", true, agreedPlaceCodeFindBox?.Visible);
				AssertEquals("AdditionalDeliveryTermsTextBox Visible", false, additionalDeliveryTerms?.Visible);
			});

			declaration.JE_ShipmentIncoTerm = "XXX";

			agreedPlaceCodeDropEdit = GetAgreedPlaceCodeDropEdit(form);
			agreedPlaceCodeFindBox = GetAgreedPlaceCodeFindBox(form);
			additionalDeliveryTerms = GetAdditionalDeliveryTermsTextBox(form);

			CombineAssertions("When JE_ShipmentIncoTerm = XXX", () =>
			{
				AssertEquals("AgreedPlaceDropEdit Visible", false, agreedPlaceCodeDropEdit?.Visible);
				AssertEquals("AgreedPlaceCodeFindBox Visible", false, agreedPlaceCodeFindBox?.Visible);
				AssertEquals("AdditionalDeliveryTermsTextBox Visible", true, additionalDeliveryTerms?.Visible);
			});
		}

		ZCodeFindBox GetAgreedPlaceCodeFindBox(ZForm form) => form.FindSingleOrDefault<ZCodeFindBox>("AgreedPlaceCodeFindBox");
		ZDropEdit GetAgreedPlaceCodeDropEdit(ZForm form) => form.FindSingleOrDefault<ZDropEdit>("AgreedPlaceCodeDropEdit");
		ZTextBox GetAdditionalDeliveryTermsTextBox(ZForm form) => form.FindSingleOrDefault<ZTextBox>("AdditionalDeliveryTermsTextBox");
	}
}

public abstract class JobDeclarationFormPerformanceTest : EU.GUI.Testing.JobDeclarationFormPerformanceTest
{
	protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

	Dictionary<string, int> ITBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>()
	{
		{ StmDataSchema.Constants.TableName, 5 }
	};
	Dictionary<string, int> ITBaseValidateAllExpectedHits => new Dictionary<string, int>()
	{
		{ StmDataSchema.Constants.TableName, 5 },
		{ RefPacksSchema.Constants.TableName, 6 },
		{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 }
	};
	Dictionary<string, int> ITBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
	{
		{ StmDataSchema.Constants.TableName, 5 },
		{ RefPacksSchema.Constants.TableName, 6 }
	};
	Dictionary<string, int> ITBaseFormMergeExpectedHits => new Dictionary<string, int>()
	{
		{ StmDataSchema.Constants.TableName, 5 }
	};
	Dictionary<string, int> ITBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>()
	{
		{ OrgAddressSchema.Constants.TableName, 5 },
		{ OrgHeaderSchema.Constants.TableName, 5 }
	};
	Dictionary<string, int> ITBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
	{
		{ OrgAddressSchema.Constants.TableName, 9 },
		{ StmDocDataOverrideSchema.Constants.TableName, 8 },
	};
	Dictionary<string, int> ITBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>()
	{
		{ OrgAddressCapabilitySchema.Constants.TableName, 7 },
	};

	Dictionary<string, int> ITBaseDeleteExpectedHits => new Dictionary<string, int>();

	protected virtual Dictionary<string, int> ITLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITValidateAllExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITFormMergeExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITUniversalXMLAddExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> ITDeleteExpectedHits => new Dictionary<string, int>();

	protected override Dictionary<string, int> EULoadEditableChildObjectsExpectedHits => ZipDictionaries(ITBaseLoadEditableChildObjectsExpectedHits, ITLoadEditableChildObjectsExpectedHits);
	protected override Dictionary<string, int> EUValidateAllExpectedHits => ZipDictionaries(ITBaseValidateAllExpectedHits, ITValidateAllExpectedHits);
	protected override Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => ZipDictionaries(ITBaseLightFormValidationAndSaveExpectedHits, ITLightFormValidationAndSaveExpectedHits);
	protected override Dictionary<string, int> EUFormMergeExpectedHits => ZipDictionaries(ITBaseFormMergeExpectedHits, ITFormMergeExpectedHits);
	protected override Dictionary<string, int> EUUniversalXMLExportExpectedHits => ZipDictionaries(ITBaseUniversalXMLExportExpectedHits, ITUniversalXMLExportExpectedHits);
	protected override Dictionary<string, int> EUUniversalXMLImportUpdateExpectedHits => ZipDictionaries(ITBaseUniversalXMLImportUpdateExpectedHits, ITUniversalXMLImportUpdateExpectedHits);
	protected override Dictionary<string, int> EUUniversalXMLAddExpectedHits => ZipDictionaries(ITBaseUniversalXMLAddExpectedHits, ITUniversalXMLAddExpectedHits);
	protected override Dictionary<string, int> EUDeleteExpectedHits => ZipDictionaries(ITBaseDeleteExpectedHits, ITDeleteExpectedHits);
}

sealed class JobDeclarationFormPerformanceTest_Import : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

	protected override Dictionary<string, int> ITUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
	{
		{ OrgHeaderSchema.Constants.TableName, 11 },
	};

	protected override Dictionary<string, int> ITUniversalXMLAddExpectedHits => new Dictionary<string, int>()
	{
		{ OrgHeaderSchema.Constants.TableName, 12 },
	};

	protected override Dictionary<string, int> ITValidateAllExpectedHits
	{
		get
		{
			var result = base.ITValidateAllExpectedHits;
			result.Add("NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8);
			return result;
		}
	}

	protected override Dictionary<string, int> ITLightFormValidationAndSaveExpectedHits
	{
		get
		{
			var result = base.ITLightFormValidationAndSaveExpectedHits;
			result.Add("NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8);
			return result;
		}
	}

	protected override Dictionary<string, int> ITUniversalXMLExportExpectedHits
	{
		get
		{
			var result = base.ITUniversalXMLExportExpectedHits;
			result.Add("NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 6);
			result.Add(ZZRefCusCodeListCombined.Schema.TableName, 5);
			return result;
		}
	}
}

sealed class JobDeclarationFormPerformanceTest_Export : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;

	protected override Dictionary<string, int> ITUniversalXMLAddExpectedHits => new Dictionary<string, int>()
	{
		{ OrgHeaderSchema.Constants.TableName, 11 },
	};

	protected override Dictionary<string, int> ITValidateAllExpectedHits
	{
		get
		{
			var result = base.ITValidateAllExpectedHits;
			result.Add("NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8);
			return result;
		}
	}

	protected override Dictionary<string, int> ITLightFormValidationAndSaveExpectedHits
	{
		get
		{
			var result = base.ITLightFormValidationAndSaveExpectedHits;
			result.Add("NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 8);
			return result;
		}
	}

	protected override Dictionary<string, int> ITUniversalXMLExportExpectedHits
	{
		get
		{
			var result = base.ITUniversalXMLExportExpectedHits;
			result.Add("NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 6);
			return result;
		}
	}
}
