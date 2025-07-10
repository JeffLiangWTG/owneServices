using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(SupplementaryCodeProvider))]
	sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
	{
		public void TestGetBySupplementaryCodeSupporterGB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
			AssertType<SupplementaryCodeProvider>(provider);
		}

		public void TestGetByJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var supplementaryCodeProvider = SupplementaryCodeProvider.GetByJobDeclaration(declaration);
			AssertNotNull("When JE_ApplicationCode is CHF, SupplementaryCodeProvider", supplementaryCodeProvider);
			AssertType<SupplementaryCodeProvider>("When JE_ApplicationCode is CHF, SupplementaryCodeProvider", supplementaryCodeProvider);

			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			supplementaryCodeProvider = SupplementaryCodeProvider.GetByJobDeclaration(declaration);
			AssertNotNull("When JE_ApplicationCode is CDS, SupplementaryCodeProvider", supplementaryCodeProvider);
			AssertType<SupplementaryCodeProviderCDS>("When JE_ApplicationCode is CDS, SupplementaryCodeProvider", supplementaryCodeProvider);
		}

		protected override ZString CountryCodeForSupplementaryCodeProvider => "GBCHF";

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 0;

		protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);
	}
}
