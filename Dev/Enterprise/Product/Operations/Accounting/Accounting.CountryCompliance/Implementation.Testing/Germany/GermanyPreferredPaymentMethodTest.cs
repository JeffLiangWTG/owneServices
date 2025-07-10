using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.CountryCompliance.GlobalCountryFactory;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Germany.Testing
{
	public class GermanyPreferredPaymentMethodTest : TestCaseWithFactory
	{
		public void TestCountryFactoryCanReturnPreferredPaymentMethodProvider()
		{
			AssertNotNull(GetFeatureInterface());
		}

		public void TestReturnsPreferredPaymentMethodWarning()
		{
			var warningMessage = "You are saving a German organization with the organization category set to Government. It is recommended to set all payment methods to TRF (Transfer) as you otherwise risk getting electronic invoices rejected.";
			var expectedWarnings = new Dictionary<(string OrgCategory, string PaymentMethod), string>()
			{
				// With OrgCategory Government every payment method but BankTransfer should return the warning
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck), warningMessage },
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck), warningMessage },
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest), warningMessage },
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard), warningMessage },
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard), warningMessage },
				{ (OrgConstants.Category.Government, OrgConstants.CreditAgreedPaymentMethods.Code.EPayment), warningMessage }
			};

			var featureInterface = GetFeatureInterface();
			AssertPreferredPaymentMethodWarnings(featureInterface, expectedWarnings);
		}

		void AssertPreferredPaymentMethodWarnings(IPreferredPaymentMethod featureInterface, Dictionary<(string OrgCategory, string PaymentMethod), string> testCases)
		{
			var orgCategories = GetConstantsFromType(typeof(OrgConstants.Category));
			var paymentMethods = GetConstantsFromType(typeof(OrgConstants.CreditAgreedPaymentMethods.Code));

			foreach (var orgCategory in orgCategories)
			{
				foreach (var paymentMethod in paymentMethods)
				{
					var expectedWarning = testCases.TryGetValue((orgCategory, paymentMethod), out var warning) ? warning : null;
					var actualWarning = featureInterface.GetPreferredPaymentMethodWarning(orgCategory, paymentMethod);
					AssertEquals($"OrgCategory: {orgCategory}, PaymentMethod: {paymentMethod}, Expected warning: {expectedWarning}, Actual warning: {actualWarning}", expectedWarning, actualWarning);
				}
			}
		}

		string[] GetConstantsFromType(Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(fi => (string)fi.GetRawConstantValue())
				.ToArray();
		}

		IPreferredPaymentMethod GetFeatureInterface() => ((IAccountingCountryComplianceGlobalFactory)new AccountingCountryComplianceGlobalFactory()).GetFeatureInterface<IPreferredPaymentMethod>(Constants.CountryCodes.Germany);
	}
}
