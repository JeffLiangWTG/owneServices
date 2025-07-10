using System;
using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Malaysia;

		protected override Type GetExpectedJobDeclarationType() => typeof(Business.JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(Business.JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(Business.JobComInvoiceLine);
	}
}
