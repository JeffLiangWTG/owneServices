using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class TransactionInfoHelperTest : TestCaseWithFactory
	{
		public void TestGetRegistrationCode_NullChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(null);

				transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(null);

				transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { new RegistrationNumber() });
				AssertResult(null);

				transaction.BranchAddress.RegistrationNumberCollection[0].CountryOfIssue = new Country();
				AssertResult(null);

				transaction.BranchAddress.RegistrationNumberCollection[0].CountryOfIssue.Code = "UY";
				AssertResult(null);

				transaction.BranchAddress.RegistrationNumberCollection[0].Type = new RegistrationNumberType();
				AssertResult(null);

				transaction.BranchAddress.RegistrationNumberCollection[0].Type.Code = "BRC";
				AssertResult(null);

				transaction.BranchAddress.RegistrationNumberCollection[0].Value = "345";
				AssertResult("345");

				void AssertResult(ZString? expectedResult)
				{
					AssertEquals(expectedResult, (new TransactionInfoHelper() as ITransactionInfoHelper).GetRegistrationCode(transaction.BranchAddress, "UY", "BRC"));
				}
			});
		}

		public void TestGetRegistrationCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			};

			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>()
			{
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "AR" }, Type = new RegistrationNumberType { Code = "BRC" }, Value = "123" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = "VAT" }, Value = "345" },
				new RegistrationNumber() { CountryOfIssue = new Country { Code = "UY" }, Type = new RegistrationNumberType { Code = "BRC" }, Value = "678" }
			});

			AssertEquals("678", (new TransactionInfoHelper() as ITransactionInfoHelper).GetRegistrationCode(transaction.BranchAddress, "UY", "BRC"));
		}

		public void TestGetStateDescriptionByCountry_NullChecks()
		{
			CombineAssertions(() =>
			{
				var transaction = new TransactionInfo();
				AssertResult(string.Empty);

				transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
				AssertResult(string.Empty);

				void AssertResult(ZString? expectedResult)
				{
					AssertEquals(expectedResult, (new TransactionInfoHelper() as ITransactionInfoHelper).GetStateDescriptionByCountry("MO", "UY", Factory));
				}
			});
		}

		public void TestGetStateDescriptionByCountry()
		{
			var stateCountryCode = Core.Constants.CountryCodes.Uruguay;
			var stateCode = "MO";
			var stateDescription = "Montevideo";
			var resultStateDescription = (new TransactionInfoHelper() as ITransactionInfoHelper).GetStateDescriptionByCountry(stateCountryCode, stateCode, Factory);

			AssertEquals("Doesn't exists any record on RefCountryStates table for UY Country as default.", string.Empty, resultStateDescription);

			var state = Factory.New<MasterFiles.Business.RefCountryStates>();
			state.RW_IsActive = true;
			state.RW_IsSystem = true;
			state.RW_Description = stateDescription;
			state.RW_Code = stateCode;
			state.RW_RN_NKCountryCode = stateCountryCode;
			Factory.Save();

			resultStateDescription = (new TransactionInfoHelper() as ITransactionInfoHelper).GetStateDescriptionByCountry(stateCountryCode, stateCode, Factory);
			AssertEquals(stateDescription, resultStateDescription);
		}

		public void TestGetRefCurrencyDecimalsWithTransactionEmpty()
		{
			var transaction = new TransactionInfo();

			Factory.New<MasterFiles.Business.RefCurrency>();
			Factory.Save();

			var result = (new TransactionInfoHelper() as ITransactionInfoHelper).GetOSCurrencyDecimals(Factory, transaction);

			AssertEquals(0, result);
		}

		public void TestGetRefCurrencyDecimalsWithoutCurrency()
		{
			var transaction = new TransactionInfo();
			transaction.OSCurrency = new Currency();

			Factory.New<MasterFiles.Business.RefCurrency>();
			Factory.Save();

			var result = (new TransactionInfoHelper() as ITransactionInfoHelper).GetOSCurrencyDecimals(Factory, transaction);

			AssertEquals(0, result);
		}

		public void TestGetRefCurrencyDecimals()
		{
			var transaction = new TransactionInfo();
			transaction.OSCurrency = new Currency() { Code = "JOD" };

			Factory.New<MasterFiles.Business.RefCurrency>();
			Factory.Save();

			var result = (new TransactionInfoHelper() as ITransactionInfoHelper).GetOSCurrencyDecimals(Factory, transaction);

			AssertEquals(3, result);
		}

		#region GetGovernmentNumber

		public void TestGetGovernmentNumber()
		{
			List<AuthorizationDetails> details = null;
			AssertResult(string.Empty);

			details = new List<AuthorizationDetails>();
			AssertResult(string.Empty);

			details = new List<AuthorizationDetails> { new AuthorizationDetails() };
			AssertResult(string.Empty);

			details[0].Purpose = new CodeDescriptionPair4Char();
			AssertResult(string.Empty);

			details[0].Purpose.Code = "XXX";
			AssertResult(string.Empty);

			details[0].Purpose.Code = "EINV";
			AssertResult(string.Empty);

			details[0].GovernmentNumber = "1234";
			AssertResult("1234");

			void AssertResult(string expectedResult)
			{
				AssertEquals(expectedResult, (new TransactionInfoHelper() as ITransactionInfoHelper).GetGovernmentNumber(details));
			}
		}

		public void TestGetGovernmentNumber_GetFirstRecord()
		{
			var details = new List<AuthorizationDetails>()
			{
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "XXX" }, GovernmentNumber = "9876" },
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "EINV" }, GovernmentNumber = "1234" },
				new AuthorizationDetails() { Purpose = new CodeDescriptionPair4Char() { Code = "EINV" }, GovernmentNumber = "9875" }
			};

			AssertEquals("1234", (new TransactionInfoHelper() as ITransactionInfoHelper).GetGovernmentNumber(details));
		}

		#endregion

		public void TestGetUniqueIdentifierWithinBatch_TransactionInfoOverload()
		{
			var helper = (new TransactionInfoHelper() as ITransactionInfoHelper);
			AssertEquals(string.Empty, helper.GetUniqueIdentifierWithinBatch((TransactionInfo)null));

			var transaction = new TransactionInfo() { OrganizationAddress = new OrganizationAddress() };

			transaction.Ledger = "AR";
			transaction.TransactionType = TransactionType.INV;
			transaction.OrganizationAddress.OrganizationCode = "ABCDEFG";
			transaction.Number = "0012345";
			AssertEquals("AR:INV:ABCDEFG:0012345", helper.GetUniqueIdentifierWithinBatch(transaction));

			transaction.Ledger = "AP";
			transaction.TransactionType = TransactionType.CRD;
			transaction.OrganizationAddress.OrganizationCode = "HIJKLMN";
			transaction.Number = "0023456";
			AssertEquals("AP:CRD:HIJKLMN:0023456", helper.GetUniqueIdentifierWithinBatch(transaction));

			transaction.OrganizationAddress = null;
			AssertEquals("AP:CRD::0023456", helper.GetUniqueIdentifierWithinBatch(transaction));
		}

		public void TestGetUniqueIdentifierWithinBatch_InvoicingBaseOverload()
		{
			var helper = (new TransactionInfoHelper() as ITransactionInfoHelper);
			AssertEquals(string.Empty, helper.GetUniqueIdentifierWithinBatch((InvoicingBase)null));

			var arOrg = Factory.New<MasterFiles.Business.OrgHeader>();
			arOrg.OH_Code = "ABCDEFG";
			var arInvoice = Factory.New<ARInvoice>();
			arInvoice.AH_TransactionNum = "0012345";
			arInvoice.AH_OH = arOrg.PK;

			AssertEquals("AR:INV:ABCDEFG:0012345", helper.GetUniqueIdentifierWithinBatch(arInvoice));

			var apOrg = Factory.New<MasterFiles.Business.OrgHeader>();
			apOrg.OH_Code = "HIJKLMN";
			var apCreditNote = Factory.New<APCreditNote>();
			apCreditNote.AH_TransactionNum = "0023456";
			apCreditNote.AH_OH = apOrg.PK;

			AssertEquals("AP:CRD:HIJKLMN:0023456", helper.GetUniqueIdentifierWithinBatch(apCreditNote));

			apCreditNote.AH_OH = ZGuid.Empty;
			AssertEquals("AP:CRD::0023456", helper.GetUniqueIdentifierWithinBatch(apCreditNote));
		}

		public void TestGetUniqueIdentifierWithinBatch_UniversalEventTransactionDataObjectOverload()
		{
			var helper = (new TransactionInfoHelper() as ITransactionInfoHelper);
			AssertEquals(string.Empty, helper.GetUniqueIdentifierWithinBatch((UniversalEventTransactionDataObject)null));

			var eventTransactionObject = new UniversalEventTransactionDataObject();
			eventTransactionObject.TransactionLedger = "AP";
			eventTransactionObject.TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			eventTransactionObject.TransactionNumber = "01234567";
			eventTransactionObject.TransactionOrgHeaderCode = "ORGHEADER";

			AssertEquals("AP:INV:ORGHEADER:01234567", helper.GetUniqueIdentifierWithinBatch(eventTransactionObject));

			eventTransactionObject.TransactionLedger = "AR";

			AssertEquals("AR:INV:ORGHEADER:01234567", helper.GetUniqueIdentifierWithinBatch(eventTransactionObject));

			eventTransactionObject.TransactionOrgHeaderCode = ZString.Empty;
			AssertEquals("AR:INV::01234567", helper.GetUniqueIdentifierWithinBatch(eventTransactionObject));

			eventTransactionObject.TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			eventTransactionObject.TransactionOrgHeaderCode = "ORGHEADER";
			AssertEquals("AR:CRD:ORGHEADER:01234567", helper.GetUniqueIdentifierWithinBatch(eventTransactionObject));

			eventTransactionObject.TransactionNumber = "1111111";
			AssertEquals("AR:CRD:ORGHEADER:1111111", helper.GetUniqueIdentifierWithinBatch(eventTransactionObject));
		}

		public void TestGetUniqueIdentifierWithinBatch_UniqueKeyIdentifierPatternOverload()
		{
			var helper = (new TransactionInfoHelper() as ITransactionInfoHelper);

			AssertEquals(":::", helper.GetUniqueIdentifierWithinBatch(string.Empty, string.Empty, string.Empty, string.Empty));
			AssertEquals("AP:INV:ORGHEADER:01234567", helper.GetUniqueIdentifierWithinBatch("AP", "INV", "ORGHEADER", "01234567"));
		}
	}
}
