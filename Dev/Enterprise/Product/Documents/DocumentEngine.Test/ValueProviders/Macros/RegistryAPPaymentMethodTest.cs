using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RegistryAPPaymentMethod))]
	sealed class RegistryAPPaymentMethodTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<RegistryAPPaymentMethod>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<registryappaymentmethod>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("< RegistryAPPaymentMethod >", Passes.FirstPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Registry APPaymentMethod>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<RegistryAPPaymentMethod()>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertEquals("Should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, ValueProviderToTest.GetReplacement("<RegistryAPPaymentMethod>", Report));

			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.DirectDebit);
			AssertEquals("Should be DirectDebit", ZArchitecture.Core.ReceiptTypes.DirectDebit, ValueProviderToTest.GetReplacement("<RegistryAPPaymentMethod>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new RegistryAPPaymentMethod();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			OrganisationsDataRegistry.Instance.APPaymentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZArchitecture.Core.ReceiptTypes.Cheque);
		}
	}
}
