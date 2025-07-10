using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SingleDecHeaderProvider))]
	sealed class SingleDecHeaderProviderBaseOnlyTest : ImportHeaderProviderAbstractTest<SingleDecHeaderProvider>
	{
		public void TestDeclarantIsConsigneeFlag()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			declaration.ImporterDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("Different Orgs", false, Provider.DeclarantIsConsigneeFlag);
				declaration.ImporterDocumentaryAddress.OrganisationPK = declarant.PK;
				AssertEquals("Same Orgs", true, Provider.DeclarantIsConsigneeFlag);
			});
		}

		public void TestDeliveryTermsPlace()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_IncoTermPlace = "Düsseldorf";

			AssertEquals("Düsseldorf", Provider.DeliveryTermsPlace);
		}

		public void TestDeliveryTermsKey()
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.ZG_AgreedPlaceCode = "X";

			AssertEquals("X", Provider.DeliveryTermsKey);
		}

		public void TestPaymentTransactionAmount_And_CurrencyCode()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_ValuationCode = "01";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1.1;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10.01;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
			invoiceLine3.JI_LinePrice = 100.001;

			var invoiceLine4 = invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 1000.0001;

			CombineAssertions(() =>
			{
				AssertEquals(11.11m, Provider.PaymentTransaction.Value);
				AssertEquals("USD", Provider.PaymentTransaction.CurrencyCode);
			});
		}

		public void TestPaymentTransaction_Null_23() => AssertPaymentTransaction_Null("23");

		public void TestPaymentTransaction_Null_24() => AssertPaymentTransaction_Null("24");

		public void TestForeignTradeStatisticsGoodsStatus()
		{
			declaration.JE_StatisticStatus = "0";
			AssertEquals("0", Provider.ForeignTradeStatisticsGoodsStatus);
		}

		public void TestForeignTradeStatisticsDestinationCountry()
		{
			declaration.JE_GoodsDestination = CountryCodes.Germany;
			AssertEquals(CountryCodes.Germany, Provider.ForeignTradeStatisticsDestinationCountry);
		}

		public void TestForeignTradeStatisticsDestinationFederalState()
		{
			var loader = new RefUNLOCO.Loader(Factory);
			var berlin = loader.Load("DEBER");
			berlin.CountryStates.RW_Code = "HH";
			declaration.JE_RL_NKFinalDestination = berlin.Code;
			declaration.JE_GoodsDestination = CountryCodes.Germany;

			AssertEquals("02", Provider.ForeignTradeStatisticsDestinationFederalState);
		}

		public void TestEntryCustomsOfficeReferenceNumber()
		{
			var co = declaration.CustomsOffices.AddNew();
			co.CY_Data = "DE008899";
			co.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent;
			AssertEquals("DE008899", Provider.EntryCustomsOfficeReferenceNumber);
		}

		protected override SingleDecHeaderProvider GetProvider() => new SingleDecHeaderProviderForTest(entryHeader);

		void AssertPaymentTransaction_Null(ZString valuationCode)
		{
			var invoice = AddInvoiceWithInvoiceLine();
			invoice.JZ_InvoiceAmount = 123.45m;
			invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_ValuationCode = valuationCode;
			var invoice2 = AddInvoiceWithInvoiceLine();
			invoice2.JZ_InvoiceAmount = 123.55m;
			invoice2.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.UnitedStates;
			AssertNull(valuationCode, Provider.PaymentTransaction);
		}
	}

	sealed class SingleDecHeaderProviderForTest : SingleDecHeaderProvider
	{
		public SingleDecHeaderProviderForTest(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}
	}
}
