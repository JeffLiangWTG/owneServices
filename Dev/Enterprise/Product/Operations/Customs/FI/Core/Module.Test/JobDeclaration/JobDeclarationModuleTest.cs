using System;
using Enterprise.Customs.FI.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.Finland;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
}
