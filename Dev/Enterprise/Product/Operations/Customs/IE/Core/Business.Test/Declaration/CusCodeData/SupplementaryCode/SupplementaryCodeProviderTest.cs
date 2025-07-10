using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class ImportInvoiceLineSupplementaryCodeProviderTest : IESupplementaryCodeProviderTest
	{
		public override void TestNumberOfAdditionalSupplementaryCodes()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				base.TestNumberOfAdditionalSupplementaryCodes();
			}
		}

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

		protected override SupplementaryCodeProvider CreateSupplementaryCodeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new SupplementaryCodeProvider(CountryCodeForSupplementaryCodeProvider, line);
		}
	}

	sealed class ExportInvoiceLineSupplementaryCodeProviderTest : IESupplementaryCodeProviderTest
	{
		public override void TestNumberOfAdditionalSupplementaryCodes()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, false))
			{
				base.TestNumberOfAdditionalSupplementaryCodes();
			}
		}

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 97;

		protected override SupplementaryCodeProvider CreateSupplementaryCodeProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var line = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new SupplementaryCodeProvider(CountryCodeForSupplementaryCodeProvider, line);
		}
	}

	sealed class CommonSupplementaryCodeProviderTest : IESupplementaryCodeProviderTest
	{
		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

		protected override SupplementaryCodeProvider CreateSupplementaryCodeProvider() => new SupplementaryCodeProvider(CountryCodeForSupplementaryCodeProvider);
	}

	[TestedType(typeof(SupplementaryCodeProvider))]
	abstract class IESupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, EU.Business.SupplementaryCode>
	{
		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;
		protected override Type ExpectedGetNewValidationReturnType => typeof(EU.Business.SupplementaryCodeValidation);
		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);
	}
}
