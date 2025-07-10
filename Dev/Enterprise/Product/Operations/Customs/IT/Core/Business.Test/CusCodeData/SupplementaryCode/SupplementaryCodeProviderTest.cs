using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(SupplementaryCodeProvider))]
sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
{
	public void TestGetBySupplementaryCodeSupporterIT()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
		AssertType<SupplementaryCodeProvider>(provider);
	}

	protected override ZString CountryCodeForSupplementaryCodeProvider => "IT";

	protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

	protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 97;

	protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

	protected override Type ExpectedGetNewLookupsReturnType => typeof(SupplementaryCodeLookups);
}
