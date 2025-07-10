using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class SACJobComInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestNonequalBalanceIsNotCheckedForSAC()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			header.JZ_InvoiceAmount = 100m;
			declaration.ResumeApportionment();
			AssertHasMessageErrors(header.JZ_Calc_BalanceInfo);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			header.JZ_InvoiceAmount = 110m; //because of the base behaviour in setter of JZ_InvoiceAmount I had to change the value or call RunPreSaveValidation
			declaration.ResumeApportionment();
			AssertNoMessageErrors(header.JZ_Calc_BalanceInfo);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			header.JZ_InvoiceAmount = 120m;
			declaration.ResumeApportionment();
			AssertNoMessageErrors(header.JZ_Calc_BalanceInfo);
		}

		public void TestBalanceCannotBeNegativeForSACWithLine() //Total line prices should not exceed total amount
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = header.JobComInvoiceLines.AddNew();
			JobComInvoiceLine line2 = header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 100m;
			line1.JI_LinePrice = 50m;
			line2.JI_LinePrice = 200m;
			header.RunPreSaveValidation();
			declaration.ResumeApportionment();
			AssertHasMessageErrors(header.JZ_Calc_BalanceInfo);
		}

		public void TestBalanceIsNotValidatedWhenSACWithoutLines()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();

			JobComInvoiceLine line = header.JobComInvoiceLines.AddNew();

			header.JZ_InvoiceAmount = 100m;
			line.JI_LinePrice = 1000m;
			header.RunPreSaveValidation();
			AssertNoNotifications(header.JZ_Calc_BalanceInfo);
		}

		public void TestEmptyBuyerIsNotError()
		{
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertEquals("Buyer is not mandatory for SAC", false, invoice.JZ_OH_BuyerInfo.HasNotifications());
		}

		public void TestEmptySupplierIsNotError()
		{
			invoice.JZ_OH_Supplier = ZGuid.Empty;
			AssertEquals("Supplier is not mandatory for SAC", false, invoice.JZ_OH_SupplierInfo.HasNotifications());
		}
		public void TestFOBAmountOverScreenFreeAmountIsMessageErrorForSAC()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoiceHeaderMock = Factory.NewMoq<JobComInvoiceHeader>();
			var invoiceHeader = invoiceHeaderMock.Object;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_JE = declaration.PK;
			AssertNoMessageErrors(invoiceHeader.JZ_Calc_FOBAmountInfo);

			TaxOrFeeTestHelper.SetDeminimus(Factory, Deminimus);
			var deminimus = Deminimus;

			invoiceHeaderMock.Setup(m => m.JZ_Calc_FOBAmount).Returns(deminimus + 1);
			invoiceHeader.RunPreSaveValidation();
			AssertHasMessageErrors(invoiceHeader.JZ_Calc_FOBAmountInfo);

			var newFactory = new BusinessObjectFactory();
			var declaration1 = JobDeclaration.New(newFactory);
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration1.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoiceHeaderMock1 = newFactory.NewMoq<JobComInvoiceHeader>();
			var invoiceHeader1 = invoiceHeaderMock1.Object;
			invoiceHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader1.JZ_JE = declaration1.PK;

			TaxOrFeeTestHelper.SetDeminimus(newFactory, deminimus + 3000);
			invoiceHeaderMock1.Setup(m => m.JZ_Calc_FOBAmount).Returns(deminimus + 1);
			invoiceHeader1.RunPreSaveValidation();
			AssertNoMessageErrors(invoiceHeader1.JZ_Calc_FOBAmountInfo);
		}

		ZDecimal Deminimus => UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());

		#region Implementation

		JobDeclaration sACDeclaration;
		JobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			sACDeclaration = JobDeclaration.New(Factory);
			sACDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			sACDeclaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			sACDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			TaxOrFeeTestHelper.SetUp();

			AssertEquals("IsSACWithoutLines", true, sACDeclaration.IsSACWithoutLines);

			invoice = sACDeclaration.Invoices.AddNew();
		}

		#endregion
	}
}
