using System;
using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

[TestedType(typeof(JobDeclarationModule))]
sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
{
	protected override string CountryCode => Core.Constants.CountryCodes.UnitedArabEmirates;

	protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

	protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

	protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

	protected override List<string> FetchHintIgnoreField
	{
		get
		{
			var result = base.FetchHintIgnoreField;
			result.Add(Customs.Business.AutoJobDeclaration.Schema.JE_MessageSubType);
			return result;
		}
	}
}
