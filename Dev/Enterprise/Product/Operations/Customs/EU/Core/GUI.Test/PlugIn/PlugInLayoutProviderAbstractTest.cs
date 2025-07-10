using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing;

[TestsSubclassesOf(typeof(IPlugInLayoutProvider))]
public abstract class PlugInLayoutProviderAbstractTest : TestCaseWithFactory
{
	public void TestGetInvoiceLinePreviousDocumentsDetailsLayout() => CombineAssertions(() =>
	{
		foreach (var (description, declaration) in JobDeclarationsForTest)
		{
			AssertEquals(description, GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(declaration), provider.GetInvoiceLinePreviousDocumentsDetailsLayout(declaration).GetType());
		}
	});

	public void TestGetInvoiceHeaderPreviousDocumentsDetailsLayout() => CombineAssertions(() =>
	{
		foreach (var (description, declaration) in JobDeclarationsForTest)
		{
			AssertEquals(description, GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(declaration), provider.GetInvoiceHeaderPreviousDocumentsDetailsLayout(declaration).GetType());
		}
	});

	public void TestGetEntryInstructionPreviousDocumentsDetailsLayout() => CombineAssertions(() =>
	{
		foreach (var (description, declaration) in JobDeclarationsForTest)
		{
			AssertEquals(description, GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(declaration), provider.GetEntryInstructionPreviousDocumentsDetailsLayout(declaration).GetType());
		}
	});

	public void TestGetDeclarationPreviousDocumentsDetailsLayout() => CombineAssertions(() =>
	{
		foreach (var (description, declaration) in JobDeclarationsForTest)
		{
			AssertEquals(description, GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(declaration), provider.GetDeclarationPreviousDocumentsDetailsLayout(declaration).GetType());
		}
	});

	protected abstract string CountryOrGroupingCode { get; }

	protected abstract Type GetExpectedInvoiceHeaderPreviousDocumentsDetailsLayoutType(JobDeclaration declaration);

	protected abstract Type GetExpectedInvoiceLinePreviousDocumentsDetailsLayoutType(JobDeclaration declaration);

	protected abstract Type GetExpectedDeclarationPreviousDocumentsDetailsLayoutType(JobDeclaration declaration);

	protected abstract Type GetExpectedEntryInstructionPreviousDocumentsDetailsLayoutType(JobDeclaration declaration);

	protected virtual IEnumerable<(string, JobDeclaration)> JobDeclarationsForTest => new[] { ("Declaration is null", (JobDeclaration)null) };

	protected override void SetUp()
	{
		base.SetUp();
		provider = PlugInLayoutProvider.GetLayoutProvider(CountryOrGroupingCode);
	}

	protected IPlugInLayoutProvider provider;
}

