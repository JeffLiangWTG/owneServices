using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class DeclarationOfIntentRefresherTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DeclarationOfIntentRefresher(null));
		AssertNoExceptionThrown(() => new DeclarationOfIntentRefresher(declaration));
	}

	#region TestDefaultZG_UseDeclarationOfIntent

	public void TestDefaultZG_UseDeclarationOfIntentAllEntryInstructions()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent();
		CombineAssertions("IMP and no 'USE' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Never);
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent();
		CombineAssertions("IMP and 'USE - Never' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent();
		CombineAssertions("IMP and 'USE - Always' rule", () =>
		{
			Assert(entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(entryInstruction2.ZG_UseDeclarationOfIntent);
		});

		declaration.JE_MessageType = "XXX";
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent();
		CombineAssertions("XXX and 'USE - Always' rule", () =>
		{
			Assert(!entryInstruction1.ZG_UseDeclarationOfIntent);
			Assert(!entryInstruction2.ZG_UseDeclarationOfIntent);
		});
	}

	public void TestDefaultZG_UseDeclarationOfIntentSingleEntryInstructionThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(null));
	}

	public void TestDefaultZG_UseDeclarationOfIntentSingleEntryInstruction()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(entryInstruction);
		Assert("IMP and no 'USE' rule", !entryInstruction.ZG_UseDeclarationOfIntent);

		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Never);
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(entryInstruction);
		Assert("IMP and 'USE - Never'", !entryInstruction.ZG_UseDeclarationOfIntent);

		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(entryInstruction);
		Assert("IMP and 'USE - Always' rule", entryInstruction.ZG_UseDeclarationOfIntent);

		declaration.JE_MessageType = "XXX";
		declaration.DeclarationOfIntentRefresher.DefaultZG_UseDeclarationOfIntent(entryInstruction);
		Assert("XXX and 'USE - Always' rule'", !entryInstruction.ZG_UseDeclarationOfIntent);
	}

	#endregion

	#region TestDefaultSupportingDocument01DI

	public void TestDefaultSupportingDocument01DIAllEntryInstructionsWhenMessageTypeChanges()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		var invoice3 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice3.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DIAllEntryInstructionsWhenImporterChanges()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		var invoice3 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice3.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		declaration.JE_OH_Importer = organisationWithDOI.PK;
		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DISingleEntryInstructionWhenZG_UseDeclarationOfIntentChanges()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Never);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction2.PK;

		var invoice3 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice3.InvoiceLines.AddNew();
		invoiceLine3.JI_CEI = entryInstruction2.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions(nameof(entryInstruction1), () =>
		{
			entryInstruction1.ZG_UseDeclarationOfIntent = true;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions(nameof(entryInstruction2), () =>
		{
			entryInstruction2.ZG_UseDeclarationOfIntent = true;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DISingleEntryInstructionWhenNewInvoiceLineIsLinked()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();

		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var invoice3 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoice3.InvoiceLines.AddNew();

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions($"Linking {nameof(invoiceLine1)}", () =>
		{
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 0);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions($"Linking {nameof(invoiceLine2)}", () =>
		{
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions($"Linking {nameof(invoiceLine3)}", () =>
		{
			invoiceLine3.JI_CEI = entryInstruction2.PK;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice2.SupportingDocuments, expectedCount: 1);
			Assert01DISupportingDocumentWithValidValues(invoice3.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DISingleEntryInstructionWhenNewInvoiceLineIsAdded()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction2.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
		});

		var invoiceLine1 = Factory.New<JobComInvoiceLine>();
		CombineAssertions($"Linking {nameof(invoiceLine1)} to invoice and entry instruction", () =>
		{
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_JZ = invoice1.PK;
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
		});

		CombineAssertions($"Adding {nameof(invoiceLine1)} to the collection", () =>
		{
			invoice1.InvoiceLines.Add(invoiceLine1);
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DISingleEntryInstructionWithoutSettingIssueDate()
	{
		SetAuthorisationRule(CusAuthorisationRuleUseValueList.Codes.Always);
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		AddExpectedSupportingDocument(invoice1.SupportingDocuments);

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
		});

		doiAuthorisation.CPH_Number = InvalidDOIAuthorisationNumber;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithEmptyDate(invoice1.SupportingDocuments, expectedCount: 1);
		});
	}

	public void TestDefaultSupportingDocument01DIThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => declaration.DeclarationOfIntentRefresher.DefaultSupportingDocument01DI(null));
	}

	public void TestResetSupportingDocument01DIButNotDefaultAsInvalidMessageType()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryInstruction1.ZG_UseDeclarationOfIntent = true;

		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
		});
	}

	public void TestResetSupportingDocument01DIButNotDefaultAsRemovedDoiAuthorisation()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
		});

		var organisationWithoutDOI = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = organisationWithoutDOI.PK;
		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
		});
	}

	public void TestResetSupportingDocument01DIButNotDefaultAsRemovedFlag()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		CombineAssertions("PRE-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 1);
		});

		entryInstruction1.ZG_UseDeclarationOfIntent = false;
		CombineAssertions("POST-CONDITION", () =>
		{
			Assert01DISupportingDocumentWithValidValues(invoice1.SupportingDocuments, expectedCount: 0);
		});
	}

	void Assert01DISupportingDocumentWithValidValues(SupportingDocumentCollection supportingDocumentCollection, int expectedCount)
	{
		Assert01DISupportingDocument(supportingDocumentCollection, expectedCount, new ZDateTime(2020, 01, 01), ValidDOIAuthorisationNumber);
	}

	void Assert01DISupportingDocumentWithEmptyDate(SupportingDocumentCollection supportingDocumentCollection, int expectedCount)
	{
		Assert01DISupportingDocument(supportingDocumentCollection, expectedCount, ZDateTime.Empty, InvalidDOIAuthorisationNumber);
	}

	void Assert01DISupportingDocument(SupportingDocumentCollection supportingDocumentCollection, int expectedCount, ZDateTime expectedDateTime, ZString expectedReferenceNumber)
	{
		var supportingDocuments01DI = supportingDocumentCollection.Cast<SupportingDocument>().Where(x => x.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent).ToArray();
		AssertEquals(expectedCount, supportingDocuments01DI.Length);
		if (expectedCount > 0)
		{
			AssertEquals(UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent, supportingDocuments01DI[0].CSI_Code);
			AssertEquals(expectedReferenceNumber, supportingDocuments01DI[0].CSI_ReferenceNumber);
			AssertEquals(Core.Constants.CountryCodes.Italy, supportingDocuments01DI[0].CSI_RN_NKCountryCode);
			AssertEquals(expectedDateTime, supportingDocuments01DI[0].CSI_DateOfIssue);
		}
	}

	void AddExpectedSupportingDocument(SupportingDocumentCollection supportingDocumentCollection)
	{
		Add01DISupportingDocument(supportingDocumentCollection, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
	}

	SupportingDocument Add01DISupportingDocument(SupportingDocumentCollection supportingDocumentCollection, ZString referenceNumber, ZString countryCode, ZDateTime dateOfIssue)
	{
		var newSupportingDocument = supportingDocumentCollection.AddNew();
		newSupportingDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		newSupportingDocument.CSI_ReferenceNumber = referenceNumber;
		newSupportingDocument.CSI_RN_NKCountryCode = countryCode;
		newSupportingDocument.CSI_DateOfIssue = dateOfIssue;
		return newSupportingDocument;
	}

	#endregion

	#region 01DI overwrite

	public void TestShouldAskToOverwrite()
	{
		doiAuthorisation.CPH_Number = PlaceholderDOIAuthorisationNumber;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoice1.SupportingDocuments.RemoveAndDeleteAll();
		Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
		Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
		Add01DISupportingDocument(invoice1.SupportingDocuments, PlaceholderDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
		CombineAssertions("All conditions are met", () =>
		{
			Assert("IsImport", declaration.IsImport);
			AssertEquals("Placeholder authorisation", PlaceholderDOIAuthorisationNumber, doiAuthorisation.CPH_Number);

			var supportingDocuments01DI = invoice1.SupportingDocuments
				.GetDeclarationOfIntentSupportingDocuments()
				.Select(x => x.CSI_ReferenceNumber)
				.OrderBy(x => x).ToArray();

			AssertEquals("SupportingDocuments count", 3, supportingDocuments01DI.Length);
			AssertEquals("SupportingDocuments distinct count", 2, supportingDocuments01DI.Distinct().Count());
			AssertEquals(ValidDOIAuthorisationNumber, supportingDocuments01DI[0]);
			AssertEquals(ValidDOIAuthorisationNumber, supportingDocuments01DI[1]);
			AssertEquals(PlaceholderDOIAuthorisationNumber, supportingDocuments01DI[2]);
			Assert("ShouldAskToOverwrite", declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		CombineAssertions("Not an import declaration", () =>
		{
			Assert("IsImport", !declaration.IsImport);
			Assert("ShouldAskToOverwrite", !declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		doiAuthorisation.CPH_Number = ValidDOIAuthorisationNumber;
		CombineAssertions("Not a placeholder authorisation", () =>
		{
			AssertNotEquals(PlaceholderDOIAuthorisationNumber, doiAuthorisation.CPH_Number);
			Assert("ShouldAskToOverwrite", !declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
		});

		doiAuthorisation.CPH_Number = PlaceholderDOIAuthorisationNumber;
		entryInstruction1.ZG_UseDeclarationOfIntent = false;
		Assert("EntryInstruction.ZG_UseDeclarationOfIntent is not flagged", !declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());

		entryInstruction1.ZG_UseDeclarationOfIntent = true;
		CombineAssertions("EntryInstruction.Invoices only have the placeholder supporting document", () =>
		{
			var supportingDocuments01DI = invoice1.SupportingDocuments.GetDeclarationOfIntentSupportingDocuments().ToArray();
			AssertEquals(1, supportingDocuments01DI.Length);
			AssertEquals(PlaceholderDOIAuthorisationNumber, supportingDocuments01DI[0].CSI_ReferenceNumber);
			Assert("ShouldAskToOverwrite", !declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
		});

		Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
		Add01DISupportingDocument(invoice1.SupportingDocuments, InvalidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
		CombineAssertions("EntryInstruction.Invoices don't have 2 distinct 01DI supporting documents and one them is placeholder", () =>
		{
			var supportingDocuments01DI = invoice1.SupportingDocuments
				.GetDeclarationOfIntentSupportingDocuments()
				.Select(x => x.CSI_ReferenceNumber)
				.OrderBy(x => x).ToArray();

			AssertEquals("SupportingDocuments count", 3, supportingDocuments01DI.Length);
			AssertEquals("SupportingDocuments distinct count", 3, supportingDocuments01DI.Distinct().Count());
			AssertEquals(ValidDOIAuthorisationNumber, supportingDocuments01DI[0]);
			AssertEquals(InvalidDOIAuthorisationNumber, supportingDocuments01DI[1]);
			AssertEquals(PlaceholderDOIAuthorisationNumber, supportingDocuments01DI[2]);
			Assert("ShouldAskToOverwrite", !declaration.DeclarationOfIntentRefresher.ShouldAskToOverwrite());
		});
	}

	public void TestOverwriteOnSave()
	{
		doiAuthorisation.CPH_Number = PlaceholderDOIAuthorisationNumber;
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisationWithDOI.PK;

		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.ZG_UseDeclarationOfIntent = true;

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction1.PK;

		CombineAssertions("Placeholder has been updated", () =>
		{
			var validDOISupportingDocument = Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			var placeholderDOISupportingDocument = Add01DISupportingDocument(invoice1.SupportingDocuments, PlaceholderDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			declaration.DeclarationOfIntentRefresher.OverwritePlaceholderSupportingDocumentValues();

			AssertEquals(validDOISupportingDocument.CSI_Code, placeholderDOISupportingDocument.CSI_Code);
			AssertEquals(validDOISupportingDocument.CSI_ReferenceNumber, placeholderDOISupportingDocument.CSI_ReferenceNumber);
			AssertEquals(validDOISupportingDocument.CSI_DateOfIssue, placeholderDOISupportingDocument.CSI_DateOfIssue);
			AssertEquals(validDOISupportingDocument.CSI_RN_NKCountryCode, placeholderDOISupportingDocument.CSI_RN_NKCountryCode);
		});

		CombineAssertions("EntryInstruction.ZG_DeclarationOfIntent = false, placeholder 01DI is not updated", () =>
		{
			entryInstruction1.ZG_UseDeclarationOfIntent = false;
			invoice1.SupportingDocuments.RemoveAndDeleteAll();
			Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			var placeholderDOISupportingDocument = Add01DISupportingDocument(invoice1.SupportingDocuments, PlaceholderDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			declaration.DeclarationOfIntentRefresher.OverwritePlaceholderSupportingDocumentValues();

			AssertEquals(UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent, placeholderDOISupportingDocument.CSI_Code);
			AssertEquals(PlaceholderDOIAuthorisationNumber, placeholderDOISupportingDocument.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2020, 01, 01), placeholderDOISupportingDocument.CSI_DateOfIssue);
			AssertEquals(Core.Constants.CountryCodes.Italy, placeholderDOISupportingDocument.CSI_RN_NKCountryCode);
		});

		CombineAssertions("EntryInstruction has not 2 distinct 01DI documents and one is placeholder, placeholder 01DI is not updated", () =>
		{
			entryInstruction1.ZG_UseDeclarationOfIntent = true;
			invoice1.SupportingDocuments.RemoveAndDeleteAll();
			Add01DISupportingDocument(invoice1.SupportingDocuments, ValidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			Add01DISupportingDocument(invoice1.SupportingDocuments, InvalidDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			var placeholderDOISupportingDocument = Add01DISupportingDocument(invoice1.SupportingDocuments, PlaceholderDOIAuthorisationNumber, Core.Constants.CountryCodes.Italy, new ZDateTime(2020, 01, 01));
			declaration.DeclarationOfIntentRefresher.OverwritePlaceholderSupportingDocumentValues();

			AssertEquals(UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent, placeholderDOISupportingDocument.CSI_Code);
			AssertEquals(PlaceholderDOIAuthorisationNumber, placeholderDOISupportingDocument.CSI_ReferenceNumber);
			AssertEquals(new ZDateTime(2020, 01, 01), placeholderDOISupportingDocument.CSI_DateOfIssue);
			AssertEquals(Core.Constants.CountryCodes.Italy, placeholderDOISupportingDocument.CSI_RN_NKCountryCode);
		});
	}

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		organisationWithDOI = Factory.New<OrgHeader>();
		organisationWithDOI.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Italy;
		doiAuthorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent, permitHolder: organisationWithDOI.PK, ValidDOIAuthorisationNumber, startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
	OrgHeader organisationWithDOI;
	CusAuthorisationHeader doiAuthorisation;

	const string ValidDOIAuthorisationNumber = "20123111223312345123456";
	const string InvalidDOIAuthorisationNumber = "AABBCCDDEEFFAAAAABBBBBB";
	const string PlaceholderDOIAuthorisationNumber = "X";

	#region Implementation

	void SetAuthorisationRule(ZString valueFrom)
	{
		var useRule = doiAuthorisation.CusAuthorisationRules.Find(x => x.CPR_RuleCode == ITCusAuthorisationRuleTypeList.Codes.Use).SingleOrDefault();
		if (useRule == null)
		{
			useRule = doiAuthorisation.CusAuthorisationRules.AddNew();
			useRule.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Use;
		}
		useRule.CPR_ValueFrom = valueFrom;
	}

	#endregion
}
