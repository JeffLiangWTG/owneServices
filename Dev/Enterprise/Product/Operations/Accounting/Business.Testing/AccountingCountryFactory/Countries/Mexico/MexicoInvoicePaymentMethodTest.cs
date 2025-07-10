using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	public class MexicoInvoicePaymentMethodTest : TestCaseWithFactory
	{
		[TestDate(2020, 11, 8)]
		public void TestMetodoPago_ReturnedValues()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IInvoicePaymentMethodProvider).GetInvoicePaymentMethodProvider();
			var invoiceTermType = string.Empty;
			var invoiceDate = ZDateTime.Empty;
			var dueDate = ZDateTime.Empty;
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, new CodeDescriptionPair("", ""));

			invoiceTermType = nameof(InvoiceTermType.COD);
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, DocWrapperMappingMetodoPago.PUE);

			invoiceTermType = nameof(InvoiceTermType.PIA);
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, DocWrapperMappingMetodoPago.PUE);

			invoiceTermType = nameof(InvoiceTermType.INV);
			invoiceDate = ZDateTime.Today;
			dueDate = ZDateTime.Today.AddDays(1);
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, DocWrapperMappingMetodoPago.PUE);

			dueDate = ZDateTime.Today.AddMonths(1);
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, DocWrapperMappingMetodoPago.PPD);

			dueDate = ZDateTime.Today.AddYears(1);
			AssertMetodoPago_ExpectedCorrectCodes(invoiceTermType, invoiceDate, dueDate, DocWrapperMappingMetodoPago.PPD);

			void AssertMetodoPago_ExpectedCorrectCodes(ZString invoiceTermCode, ZDateTime invoiceDate1, ZDateTime dueDate2, CodeDescriptionPair expectedMetodoPagoCode)
			{
				var paymentMethod = result.GetInvoicePaymentMethod(invoiceTermCode, invoiceDate1, dueDate2);
				AssertEquals("MetodoPago has incorrect code, if month and year of the due date are equal to month and year of transaction date the code should be PUE otherwise PPD.", expectedMetodoPagoCode, paymentMethod);
			}
		}
	}
}
