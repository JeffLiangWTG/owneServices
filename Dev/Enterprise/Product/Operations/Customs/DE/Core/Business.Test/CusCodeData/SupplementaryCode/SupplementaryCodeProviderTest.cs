using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SupplementaryCodeProvider))]
	class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
	{
		[ExpectNoExceptions]
		public void TestGetBySupplementaryCodeSupporterDE()
		{
			var provider = EU.Business.SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(invoiceLine);
			NUnit.Framework.Assert.That(provider, NUnit.Framework.Is.TypeOf<SupplementaryCodeProvider>());
		}

		[ExpectNoExceptions]
		public void TestNumberOfAdditionalSupplementaryCodes_Export()
		{
			var additionalSupplementaryCodes = invoiceLine.AdditionalSupplementaryCodes as ISupportMaxCountValidation;
			NUnit.Framework.Assert.That(additionalSupplementaryCodes.MaxCountValidator.MaxCount, NUnit.Framework.Is.EqualTo(97));
		}

		[ExpectNoExceptions]
		public void TestNumberOfAdditionalSupplementaryCodes_Import()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var additionalSupplementaryCodes = invoiceLine.AdditionalSupplementaryCodes as ISupportMaxCountValidation;
			NUnit.Framework.Assert.That(additionalSupplementaryCodes.MaxCountValidator.MaxCount, NUnit.Framework.Is.EqualTo(8));
		}

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

		protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);

		protected override ZString CountryCodeForSupplementaryCodeProvider => Core.Constants.CountryCodes.Germany;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
