using System;
using Enterprise.Customs.AsycudaCustoms.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Botswana;

		public override Type ModuleToBashType => typeof(JobDeclarationModule);

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);
	}
}
