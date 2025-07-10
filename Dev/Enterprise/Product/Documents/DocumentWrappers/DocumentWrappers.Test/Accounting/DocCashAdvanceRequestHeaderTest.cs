using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCashAdvanceRequestHeader))]
	sealed class DocCashAdvanceRequestHeaderTest : DocumentWrapperTestCase
	{
		[TestDate(2022, 6, 13, 23, 55, 30, 253)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAllProperties()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.CreateJob(creator.LocalClient, 1.0M, creator.Agent, 1.0M);
			job.JH_JobNum = "S00001001";
			CashAdvanceRequest.CAH_JH_Job = job.PK;
			CashAdvanceRequest.CAH_RequestReferenceNumber = "00001001";
			CashAdvanceRequest.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Paid;
			CashAdvanceRequest.CAH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			CashAdvanceRequest.CAH_OSAmount = 100m;
			CashAdvanceRequest.CAH_LocalAmount = 75m;
			CashAdvanceRequest.CAH_OSPaidAmount = 60m;
			CashAdvanceRequest.CAH_LocalPaidAmount = 45m;
			CashAdvanceRequest.CAH_SystemCreateTimeUtc = ZDateTime.UtcNow;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "ABC PTY LTD";
			CashAdvanceRequest.CAH_OH_Organization = org.PK;
			AssertEquals("JobNumber", "S00001001", CashAdvanceRequestWrapper.JobNumber);
			AssertEquals("TransactionType", "CAH", CashAdvanceRequestWrapper.TransactionType);
			AssertEquals("TransactionNum", "00001001", CashAdvanceRequestWrapper.TransactionNumber);
			AssertEquals("Status", "PAI", CashAdvanceRequestWrapper.Status);
			AssertEquals("Currency", "USD", CashAdvanceRequestWrapper.Currency.Code);
			AssertEquals("OSAmount", 100m, CashAdvanceRequestWrapper.OSAmount);
			AssertEquals("OSPaidAmount", 60m, CashAdvanceRequestWrapper.OSPaidAmount);
			AssertEquals("LocalAmount", 75m, CashAdvanceRequestWrapper.LocalAmount);
			AssertEquals("LocalPaidAmount", 45m, CashAdvanceRequestWrapper.LocalPaidAmount);
			AssertEquals("Organisation Code", "ABC", CashAdvanceRequestWrapper.Organisation.Code);
			AssertEquals("Organisation Name", "ABC PTY LTD", CashAdvanceRequestWrapper.Organisation.Name);
			AssertEquals("CreatedDateTimeLocal", "14-Jun-22", CashAdvanceRequestWrapper.CreatedDateTimeLocal.ToShortDateString());
			using (AccountingConfigurationRegistry.Instance.CashAdvanceRequestDocumentTitle.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "This is a document title"))
			{
				AssertEquals("Message", "This is a document title", CashAdvanceRequestWrapper.DocumentTitle);
			}
			using (AccountingConfigurationRegistry.Instance.CashAdvanceRequestDocumentMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "This is a document message"))
			{
				AssertEquals("Message", "This is a document message", CashAdvanceRequestWrapper.Message);
			}
		}

		public void TestReceiptBankAccount()
		{
			var factory = new BusinessObjectFactory();
			var headerBisObj = factory.NewWithValidTestData<AccGLHeader>();

			var accountBisObj = factory.New<AccBankAccount>();
			accountBisObj.AB_GC = GlbCompany.CurrentCompany.PK;
			accountBisObj.AB_RX_NKAccountCurrency = "USD";
			accountBisObj.AB_IsDefaultReceiptBankAccount = ZBool.True;
			accountBisObj.AB_AG = headerBisObj.PK;
			accountBisObj.AB_Code = "ABCBANK";
			accountBisObj.AB_BSB = "123-456";
			accountBisObj.AB_SWIFT = "77889900";
			accountBisObj.AB_AccountNum = "24681357";
			accountBisObj.AB_BankName = "St George";
			accountBisObj.AB_BankAddress = "bank address ABC";
			factory.Save();

			CashAdvanceRequest.CAH_RX_NKTransactionCurrency = "USD";
			var headerWrapper = DocCashAdvanceRequestHeader.New(CashAdvanceRequest, factory);
			AssertNotNull("Bank account is selected", headerWrapper.ReceiptBankAccount);
			AssertEquals("Is of type bank account", typeof(DocBankAccount), headerWrapper.ReceiptBankAccount.GetType());
			AssertEquals("123-456", headerWrapper.ReceiptBankAccount.BSB);
			AssertEquals("77889900", headerWrapper.ReceiptBankAccount.SWIFT);
			AssertEquals("24681357", headerWrapper.ReceiptBankAccount.AccountNum);
			AssertEquals("St George", headerWrapper.ReceiptBankAccount.BankName);
			AssertEquals("bank address ABC", headerWrapper.ReceiptBankAccount.BankAddress);
		}

		public void TestMailToAddress_SameCountry()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.CreateJob(creator.LocalClient, 1.0M, creator.Agent, 1.0M);
			job.JH_JobNum = "S00001001";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			CashAdvanceRequest.CAH_JH_Job = job.PK;

			var address = Factory.New<OrgAddress>();
			address.OA_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AssertEquals("Mail to Address on Statement (Same Country)", "EAGLE DATAMATION INTERNATIONAL\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY\nAUSTRALIA", CashAdvanceRequestWrapper.MailToAddressWithCountry);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var cashAdvance = Factory.New<AccCashAdvanceRequestHeader>();
			return new DocumentWrapper[] { DocCashAdvanceRequestHeader.New(cashAdvance, Factory) };
		}

		protected override void SetUp()
		{
			CashAdvanceRequest = Factory.New<AccCashAdvanceRequestHeader>();
			CashAdvanceRequestWrapper = DocCashAdvanceRequestHeader.New(CashAdvanceRequest, Factory);
			base.SetUp();
		}

		AccCashAdvanceRequestHeader CashAdvanceRequest;
		DocCashAdvanceRequestHeader CashAdvanceRequestWrapper;
	}
}
