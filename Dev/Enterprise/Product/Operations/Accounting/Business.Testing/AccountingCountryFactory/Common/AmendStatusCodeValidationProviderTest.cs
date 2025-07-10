using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class AmendStatusCodeValidationProviderTest : TestCaseWithFactory
	{
		public void TestValidateAmendStatusCode()
		{
			var provider = new AmendStatusCodeValidationProvider();
			AssertValidateAmendStatusCode(provider.ValidateAmendStatusCode);
		}

		public void TestValidateAmendStatusCodeForInvoiceReversal()
		{
			var provider = new AmendStatusCodeValidationProvider();
			AssertValidateAmendStatusCode(provider.ValidateAmendStatusCodeForInvoiceReversal);
		}

		void AssertValidateAmendStatusCode(Action<InvoicingBase> validateAmendStatusCode)
		{
			var originInvoice1 = Factory.New<ARInvoice>();
			var originInvoice2 = Factory.New<ARInvoice>();

			var mockInvoicingBase1 = new Mock<InvoicingBase>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("AccTransactionHeader", originInvoice1.PK));
			mockInvoicingBase1.CallBase = true;
			var mockInvoicingBase2 = new Mock<InvoicingBase>(Factory, ((IBusinessObjectFactoryInternals)Factory).RowFactory.LoadFromPK("AccTransactionHeader", originInvoice2.PK));
			mockInvoicingBase2.CallBase = true;
			var mockIAmendStatusCodeProvider = new Mock<IAmendStatusCodeProvider>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var amendStatusCodeList = new CodeDescriptionPairList();
			amendStatusCodeList.AddPair("01", "Test List Value");

			mockInvoicingBase1.Setup(x => x.IsAllowModifyAmendStatusCode).Returns(true);
			mockInvoicingBase2.Setup(x => x.IsAllowModifyAmendStatusCode).Returns(false);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeList).Returns(amendStatusCodeList);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeReferenceType).Returns("KRE");
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var invoicingBase1 = mockInvoicingBase1.Object;

			var mockInvoicingBaseValidation1 = new Mock<InvoiceBaseValidation>(invoicingBase1);
			mockInvoicingBaseValidation1.Protected().Setup("CheckAH_Calc_AmendStatusCode").Callback(() => validateAmendStatusCode(invoicingBase1));
			mockInvoicingBaseValidation1.CallBase = true;
			var validation1 = mockInvoicingBaseValidation1.Object;

			invoicingBase1.AH_Calc_AmendStatusCode = ZString.Empty;
			validation1.ValidateAH_Calc_AmendStatusCode();
			AssertHasError(invoicingBase1.AH_Calc_AmendStatusCodeInfo, "An amendment status code is required for the amending transaction. Please select an amendment status code.");

			invoicingBase1.AH_Calc_AmendStatusCode = "01";
			validation1.ValidateAH_Calc_AmendStatusCode();
			AssertNoErrors("'01' is a valid value because it is in AmendStatusCodeList", invoicingBase1.AH_Calc_AmendStatusCodeInfo);

			invoicingBase1.AH_Calc_AmendStatusCode = "AA";
			validation1.ValidateAH_Calc_AmendStatusCode();
			AssertHasError(invoicingBase1.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");

			var invoicingBase2 = mockInvoicingBase2.Object;

			var mockInvoicingBaseValidation2 = new Mock<InvoiceBaseValidation>(invoicingBase2);
			mockInvoicingBaseValidation2.Protected().Setup("CheckAH_Calc_AmendStatusCode").Callback(() => validateAmendStatusCode(invoicingBase2));
			mockInvoicingBaseValidation2.CallBase = true;
			var validation2 = mockInvoicingBaseValidation2.Object;

			invoicingBase2.AH_Calc_AmendStatusCode = ZString.Empty;
			validation2.ValidateAH_Calc_AmendStatusCode();
			AssertNoErrors("Do not check empty value in default.", invoicingBase2.AH_Calc_AmendStatusCodeInfo);

			invoicingBase2.AH_Calc_AmendStatusCode = "01";
			validation2.ValidateAH_Calc_AmendStatusCode();
			AssertNoErrors("'01' is a valid value because it is in AmendStatusCodeList", invoicingBase2.AH_Calc_AmendStatusCodeInfo);

			invoicingBase2.AH_Calc_AmendStatusCode = "AA";
			validation2.ValidateAH_Calc_AmendStatusCode();
			AssertHasError(invoicingBase2.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");
		}
	}
}
