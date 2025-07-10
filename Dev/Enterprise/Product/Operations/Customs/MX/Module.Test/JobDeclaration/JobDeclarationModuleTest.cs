using System;
using Enterprise.Customs.MX.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Mexico;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
	}
}
