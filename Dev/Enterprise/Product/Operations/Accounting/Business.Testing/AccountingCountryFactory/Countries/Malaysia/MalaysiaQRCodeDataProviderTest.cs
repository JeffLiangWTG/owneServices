using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class MalaysiaQRCodeDataProviderTest : TestCaseWithFactory
	{
		public void TestGetQRCodeString()
		{
			var baseUrl = "test.com";
			var uuid = "ARAETWMPPPNFCB9Y2CGRP1ZH10";
			var longId = "RQSM4GX9XPHNPJDV2CGRP1ZH10yHdFGx1716970602";

			AssertQrCode("All Conditions are Met", true, baseUrl, uuid, longId, expectedQrCode: $"{baseUrl}/{uuid}/share/{longId}");
			AssertQrCode("EnableEInvoicingFunctionality is False", false, baseUrl, uuid, longId,  expectedQrCode: string.Empty);
			AssertQrCode("base url is Empty", true, string.Empty, uuid, longId, expectedQrCode: string.Empty);
			AssertQrCode("uuid is Empty", true, baseUrl, string.Empty, longId, expectedQrCode: string.Empty);
			AssertQrCode("longid is Empty", true, baseUrl, uuid, string.Empty, expectedQrCode: string.Empty);
		}

		void AssertQrCode(string testCondition, bool isEnableEInvoicing, string baseUrl, string uuid, string longId, string expectedQrCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Malaysia))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableEInvoicing))
			using (AccountingConfigurationRegistry.Instance.MalaysiaEInvoicingPortalBaseUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, baseUrl))
			{
				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				invoice.AH_GovernmentAllocatedID = uuid;
				TestObjectCreator.CreateInvoiceLine(invoice, 110m, invoice.TransactionCurrency, 1m);

				if (!longId.IsNullOrEmpty())
				{
					var authorisationRecord = TestObjectCreator.CreateTransactionHeaderAuthorisationRecord(invoice);
					authorisationRecord.AHF_Number = longId;
				}

				Factory.Save();

				var qrCodeDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Malaysia) as IQRCodeDataProvider;
				AssertEquals($"QR Code String is Incorret When {testCondition}", expectedQrCode, qrCodeDataProvider.GetTransactionQRCodeString(invoice));
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
