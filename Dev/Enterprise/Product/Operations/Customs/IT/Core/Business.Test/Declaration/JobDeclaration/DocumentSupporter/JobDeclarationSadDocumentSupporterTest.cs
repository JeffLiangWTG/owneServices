using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationSadDocumentSupporter))]
sealed class JobDeclarationSadDocumentSupporterTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when parent JobDeclaration is null", () => new JobDeclarationSadDocumentSupporter(null));

		var sadDocumentSupporter = new JobDeclarationSadDocumentSupporter(declaration);
		AssertNotNull("Factory", sadDocumentSupporter.Factory);
	}

	public void TestLookups()
	{
		var declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();

		AssertNotNull("Lookups", declarationSadDocumentSupporter.Lookups);
	}

	public void TestValidation()
	{
		var declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		AssertNotNull("Validation", declarationSadDocumentSupporter.Validation);
	}

	public void TestDefaultValueForImport()
	{
		declaration.JE_MessageType = "IMP";
		var declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		CombineAssertions("Assert Default values", () =>
		{
			AssertEquals("Default Value when Declaration has no Entry Headers, BGMReferenceToPrint", "", declarationSadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("Default value for LayoutStyle", "C", declarationSadDocumentSupporter.LayoutStyle);
		});

		declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "TEST";
		declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		CombineAssertions("Assert Default values", () =>
		{
			AssertEquals("Default Value when Declaration has one Entry, BGMReferenceToPrint", "TEST", declarationSadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("Default value for LayoutStyle", "C", declarationSadDocumentSupporter.LayoutStyle);
		});

		declaration.CustomsEntryHeaders.AddNew();
		declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		CombineAssertions("Assert Default values", () =>
		{
			AssertEquals("Default Value when Declaration has more than one Entry, BGMReferenceToPrint", "", declarationSadDocumentSupporter.BGMReferenceToPrint);
			AssertEquals("Default value for LayoutStyle", "C", declarationSadDocumentSupporter.LayoutStyle);
		});
	}

	public void TestDefaultValueForExport()
	{
		declaration.JE_MessageType = "EXP";
		var declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		AssertEquals("When MessageType is EXP, Default value for LayoutStyle", "3", declarationSadDocumentSupporter.LayoutStyle);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
			AssertEquals("With UCC6 declaration, when MessageType is EXP, Default value for LayoutStyle", "C", declarationSadDocumentSupporter.LayoutStyle);
		}
	}

	public void TestGetSelectedEntryHeader()
	{
		declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "TESTX";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TESTX2";
		declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "TEST";

		var declarationSadDocumentSupporter = GetNewJobDeclarationSadDocumentSupporter();
		declarationSadDocumentSupporter.BGMReferenceToPrint = "TESTX2";
		var entryToPrint = declarationSadDocumentSupporter.GetSelectedEntryHeader();

		AssertNotNull("Entry To Print", entryToPrint);
		CombineAssertions("Entry to print properties", () =>
		{
			AssertEquals("BGMReference", "TESTX2", entryToPrint.CH_BGMReference);
			AssertSame("Same object", entryHeader, entryToPrint);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewJobDeclarationSadDocumentSupporter();

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
	JobDeclarationSadDocumentSupporter GetNewJobDeclarationSadDocumentSupporter() => new JobDeclarationSadDocumentSupporter(declaration);
}
