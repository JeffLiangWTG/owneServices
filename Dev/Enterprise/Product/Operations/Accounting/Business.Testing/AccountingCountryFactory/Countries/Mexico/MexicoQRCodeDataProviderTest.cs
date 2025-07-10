using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class MexicoQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestGetTransactionQRCodeString()
		{
			var builder = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Mexico) as IQRCodeDataProvider);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				AssertEquals(string.Empty, builder.GetTransactionQRCodeString(invoice));

				invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				var invoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoice);
				invoicePivot.AIP_Status = Constants.EInvoicingPivotState.Sent;
				var invoiceBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(invoicePivot, 103, invoicePivot.AIP_Status);

				var authorizationRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice);
				authorizationRecord.AHF_IDNumber = "AOIVD8801";
				authorizationRecord.AHF_Number = "13579";

				var fromBase64String = "T3IbVbFTnIPYUTCODK89pw+aaQcCBlYEESj6otOaeA2kxSvxSfrjSHJ8dO5ZhK5TyJK48qA+ns62ldBw/SvCC+h9xE0VEpvS7qw2bIhB67d6jYRfCaponFxbf50zs25AyLDuC+gNgrPEZ75Oh1zKJFUQgOtohcSV4ZnZBJrJMl4Zh8rRiPDupIc4d4kD3ny3GSnyIgty1j3ssXINXt01RKoadg63A6+wRFmP3i+Ci7bbQllYMfCOtSjO0CE7TQPlzjeRkA1Y2fOB7vsc5BRTw5BTHLxUa34OX5koY+mJUjkThL5dMsnKwb6E/KFK9XHa9+NBp+ILl3hAq4QY3dZVTQ==";
				var expectedResult = "3dZVTQ==";
				var valueIssuerAuthorisationData = Convert.FromBase64String(fromBase64String);
				authorizationRecord.AHF_IssuerAuthorizationData = valueIssuerAuthorisationData;
				Factory.Save();

				AssertLink("13579", "AOIVD8801", "121.0", expectedResult);

				var header = invoice.Header;

				header.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.CFD, "111111", GlbCompany.CurrentCompany.Country);
				AssertLink("13579", "AOIVD8801", "121.0", expectedResult);

				header.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFG, "333333", GlbCompany.CurrentCompany.Country);
				AssertLink("13579", "AOIVD8801", "121.0", expectedResult, "333333");

				header.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "222222", GlbCompany.CurrentCompany.Country);
				AssertLink("13579", "AOIVD8801", "121.0", expectedResult, "222222");

				void AssertLink(string governmentAllocatedNumber, string authorisationRecordIDNumber, string oSInvoiceTotal, string issuerAuthorizationData, string debtorOrganizationCode = "")
				{
					var result = builder.GetTransactionQRCodeString(invoice);
					var expected = $"https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?id={governmentAllocatedNumber}&re={authorisationRecordIDNumber}&rr={debtorOrganizationCode}&tt={oSInvoiceTotal}&fe={issuerAuthorizationData}";
					AssertEquals(expected, result);
				}
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator = testObjectCreator ?? new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
