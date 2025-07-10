using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Finland, partDetails.CustomsCountryCode);
			AssertEquals(typeof(OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestLookups_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestLookups_Export()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestValidation_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	public void TestValidation_Export()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	protected override Type GetExpectedPartType() => typeof(OrgSupplierPart);

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
}
