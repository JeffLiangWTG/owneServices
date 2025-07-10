using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeader))]
	sealed class DocJobComInvoiceHeaderTest : DocBaseJobComInvoiceHeaderAbstractTest<JobComInvoiceHeader, DocJobComInvoiceHeader>
	{
		public void TestExchangeHedge()
		{
			InvoiceHeaderInternal.ExchangeHedgeType = "1";

			AssertEquals("ExchangeHedge Code should be ", "1", InvoiceHeaderWrapperInternal.ExchangeHedge.Code);
			AssertEquals("ExchangeHedge Description should be ", BRPairListHelper.GetDescriptionOfPairListInBR(new ExchangeHedgeList(), InvoiceHeaderWrapperInternal.ExchangeHedge.Code), InvoiceHeaderWrapperInternal.ExchangeHedge.Description);
		}

		public void TestExchangeHedgeFinancialInstitution()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BR Financial Institution");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "1", "Test Financial Institution 1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceHeaderInternal.ExchangeHedgeFinancialInstitution = "1";

			AssertEquals("ExchangeHedgeFinancialInstitution Code should be ", "1", InvoiceHeaderWrapperInternal.ExchangeHedgeFI.Code);
			AssertEquals("ExchangeHedgeFinancialInstitution Description should be ", "Test Financial Institution 1", InvoiceHeaderWrapperInternal.ExchangeHedgeFI.Description);
		}

		public void TestExchangeHedgeReason()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "BR Exchange Hedge Reason");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "1", "Test Exchange Hedge Reason 1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceHeaderInternal.ExchangeHedgeReason = "1";

			AssertEquals("ExchangeHedgeReason Code should be ", "1", InvoiceHeaderWrapperInternal.ExchangeHedgeReason.Code);
			AssertEquals("ExchangeHedgeReason Description should be ", "Test Exchange Hedge Reason 1", InvoiceHeaderWrapperInternal.ExchangeHedgeReason.Description);
		}

		public void TestExchangeHedgeROFBasen()
		{
			InvoiceHeaderInternal.ExchangeHedgeROFBACENNumber = "123";
			AssertEquals("ExchangeHedgeROFBasen Code should be ", "123", InvoiceHeaderWrapperInternal.ExchangeHedgeROFBasen);
		}

		public void TestExchangeHedgeValue()
		{
			InvoiceHeaderInternal.ExchangeHedgeValue = 10.9;
			AssertEquals("ExchangeHedgeValue Code should be ", new ZDecimal(10.9), InvoiceHeaderWrapperInternal.ExchangeHedgeValue);
		}

		public void TestSupplierNameAndCountry()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "US";
			supplier.OH_FullName = "TEST ORG";
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_RN_NKCountryCode = "CA";
			supplierAddress.OA_CompanyNameOverride = "TEST ORG 1";

			AssertNull(InvoiceHeaderWrapperInternal.SupplierAddres);
			AssertEquals(ZString.Empty, InvoiceHeaderWrapperInternal.SupplierCountry.CodeAndDescription);

			InvoiceHeaderInternal.JZ_OH_Supplier = supplier.PK;
			InvoiceHeaderInternal.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			InvoiceHeaderInternal.SupplierDocAddressPK = supplierAddress.PK;
			AssertEquals("TEST ORG", InvoiceHeaderWrapperInternal.SupplierAddres.CompanyName);
			AssertEquals("US - Estados Unidos", InvoiceHeaderWrapperInternal.SupplierCountry.CodeAndDescription);

			InvoiceHeaderInternal.JobDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var wapper = DocJobComInvoiceHeader.New(InvoiceHeaderInternal, Factory);
			AssertEquals("TEST ORG 1", wapper.SupplierAddres.CompanyName);
			AssertEquals("CA - Canadá", wapper.SupplierCountry.CodeAndDescription);
		}

		public void TestExchangeHedgePaymentMethod()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "Exchange Hedge Method of Payment");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "60", "PAGAMENTO ANTECIPADO TOTAL OU PREPONDERANTE", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			Factory.Save();

			InvoiceHeaderInternal.ExchangeHedgePaymentMethod = "60";

			AssertEquals("ExchangeHedgePaymentMethod Code should be ", "60", InvoiceHeaderWrapperInternal.ExchangeHedgePaymentMethod.Code);
			AssertEquals("ExchangeHedgePaymentMethod Description should be ", "PAGAMENTO ANTECIPADO TOTAL OU PREPONDERANTE", InvoiceHeaderWrapperInternal.ExchangeHedgePaymentMethod.Description);
		}

		public void TestExchangeHedgePaymentDeadline()
		{
			InvoiceHeaderInternal.ExchangeHedgePaymentDeadline = 60;
			AssertEquals("ExchangeHedgePaymentDeadline Code should be ", new ZDecimal(60), InvoiceHeaderWrapperInternal.ExchangeHedgePaymentDeadline);
		}

		#region Overrides

		public override void TestIncoTermDescription()
		{
			InvoiceHeaderInternal.JZ_IncoTerm = "EXW";
			Assert("Inco term should be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);

			InvoiceHeaderInternal.JZ_IncoTerm = "CIF";
			Assert("Inco term should be not be empty", InvoiceHeaderWrapperInternal.IncoTermDescription != ZString.Empty);
		}

		public override void TestConversionFactorIsWrapped()
		{
			InvoiceHeaderInternal.JZ_InvoiceAmount = 1000m;
			InvoiceHeaderInternal.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, InvoiceHeaderWrapperInternal.ConversionFactor);
		}

		#endregion

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Brazil; }
		}

		protected override DocJobComInvoiceHeader CreateInvoiceHeaderWrapper(JobComInvoiceHeader invoiceHeaderInternal)
		{
			return DocJobComInvoiceHeader.New(invoiceHeaderInternal, Factory);
		}

		#endregion
	}
}
