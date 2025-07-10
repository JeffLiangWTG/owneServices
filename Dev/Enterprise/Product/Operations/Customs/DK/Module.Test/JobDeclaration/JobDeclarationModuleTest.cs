using System;
using Enterprise.Customs.DK.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : EU.Module.Testing.JobDeclarationModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Denmark;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
	}
}
