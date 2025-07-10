using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class JobDeclarationTest : EU.Business.Declaration.Testing.JobDeclarationAbstractTest<JobDeclaration>
{
	public void TestLookups_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationLookups>(declaration.Lookups);
	}

	public void TestLookups_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationLookups>(declaration.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobDeclarationLookups>(declaration.Lookups);
	}

	public void TestValidation_Import()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestValidation_Export()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobDeclarationValidation>(declaration.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobDeclarationValidation>(declaration.Validation);
	}

	public void TestInvoices()
	{
		AssertType<InvoiceHeaderActiveCollection>(declaration.Invoices);
	}

	public void TestInvoiceLines()
	{
		AssertType<InvoiceLineCompleteCollection>(declaration.InvoiceLines);
	}

	public void TestJobComInvoiceGroupHeaders()
	{
		AssertType<BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>>(declaration.JobComInvoiceGroupHeaders);
	}

	public override void TestLocalCurrencyCoreOverride()
	{
		AssertEquals(Core.Constants.CurrencyCodes.Finland, declaration.LocalCurrencyCode);
	}

	public override void TestAreMultipleEntryInstructionsAllowed()
	{
		AssertEquals(true, declaration.AreMultipleEntryInstructionsAllowed);
	}

	protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
