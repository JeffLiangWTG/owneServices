using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Testing
{
	public class EPaymentTestHelper
	{
		public EPaymentTestHelper(TestObjectCreator testObjectCreator)
		{
			ObjectCreator = testObjectCreator;
		}

		public EPaymentDeal CreateDeal(EPaymentQuote quote)
		{
			if (quote.QU_Status != QuoteStatusCodes.Accepted)
			{
				throw new ArgumentException("Quote must be in Accepted status in order to create a deal.", nameof(quote));
			}
			var deal = Factory.New<EPaymentDeal>();
			deal.AED_QU_Quote = quote.PK;
			deal.AED_GC_Company = quote.QU_GC;
			deal.AED_ProviderCode = ProviderCodes.OFX;
			return deal;
		}

		public AccBankAccount CreateOFXPaymentProviderBankAccount(ZGuid companyPk)
		{
			var bankAccount = ObjectCreator.CreateBankAccount("EPA", "E-Payment Account", ObjectCreator.USD, ObjectCreator.GLHeader1);
			bankAccount.AB_GC = companyPk;
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.EPA;
			bankAccount.AB_PaymentProvider = ProviderCodes.OFX;
			return bankAccount;
		}

		public AccEPaymentStaffToken CreateStaffToken(ZGuid bankAccountPk, ZDateTime expiryDateUtc, ZGuid companyPk, ZString staffCode, ZString status)
		{
			var staffToken = Factory.New<AccEPaymentStaffToken>();
			staffToken.TK_AB = bankAccountPk;
			staffToken.TK_ExpiryUtc = expiryDateUtc;
			staffToken.TK_GC = companyPk;
			staffToken.TK_GS_NKStaffCode = staffCode;
			staffToken.TK_Status = status;
			return staffToken;
		}

		internal EPaymentQuote CreateQuote(GlbCompany company, AccBankAccount bankAccount = null)
		{
			var bankAccountToUse = bankAccount ?? ObjectCreator.USDBankAccount;
			var paymentApproval = ObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, bankAccountToUse, ObjectCreator.USDChequeBook);
			paymentApproval.AV_PaymentType = ReceiptTypes.EPayment;
			paymentApproval.AV_RX_NKPaymentCurrency = "USD";
			paymentApproval.AV_PayExRate = 2.5m;
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-1);
			paymentApproval.AV_PaymentDate = ZDateTime.Today.AddDays(1);
			paymentApproval.AV_PaymentComment = "Paying FreightQuota Invoice 83942";
			paymentApproval.AV_ChequeOrReference = "00009283";
			paymentApproval.AV_GB = company.FirstActiveBranch.PK;
			paymentApproval.AV_GC = company.PK;
			return ObjectCreator.CreateEPaymentQuote(paymentApproval, ProviderCodes.OFX);
		}

		internal void AssertEDIInterchangeForBeneficiaryRequest(IXmlEDIInterchange interchange, List<GEPMessageWithJSONPayload> gepMessageWithJSONPayload)
		{
			Assertion.AssertEquals(ZBool.True, interchange.EI_IsActive);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, interchange.EI_ApplicationCode);
			Assertion.AssertEquals(ProviderCodes.OFX, interchange.EI_InterchangeType);
			Assertion.AssertEquals("TRX", interchange.EI_ReceiveTransmit);
			Assertion.AssertEquals("HQU", interchange.EI_Status);
			Assertion.AssertEquals(GlobalElectronicPaymentEDIInterchangeCreator.DestinationParty, interchange.EI_To);

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var ediMessages = interchange.Factory.Load<XmlEDIMessage>(query);
			Assertion.AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			AssertEDIMessageForBeneficiaryRequest(ediMessages[0], gepMessageWithJSONPayload);
		}

		internal void AssertEDIMessageForBeneficiaryRequest(XmlEDIMessage ediMessage, List<GEPMessageWithJSONPayload> gepMessageWithJSONPayload)
		{
			Assertion.AssertEquals(ZBool.True, ediMessage.EM_IsActive);
			Assertion.AssertEquals(ZString.Empty, ediMessage.EM_MessageOwner);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, ediMessage.EM_ApplicationCode);
			Assertion.AssertEquals(ProviderCodes.OFX, ediMessage.EM_MessageType);
			Assertion.AssertEquals(GEPProviderAPICommandList.Codes.SearchBeneficiary, ediMessage.EM_MessageSubType);
			Assertion.AssertEquals("TRX", ediMessage.EM_ReceiveTransmit);
			Assertion.AssertEquals("SNT", ediMessage.EM_Status);
			Assertion.AssertEquals(Env.CurrentDepartment.PK, ediMessage.EM_GE);
			Assertion.AssertEquals(AccEPaymentBeneficiaryRequestSchema.Constants.TableName, ediMessage.EM_LinkTable);

			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicPayment>(ediMessage.EM_MessageText);
			Assertion.AssertEquals("MessagingSystem", ProviderCodes.OFX, deserializedObj.Header.ElectronicPaymentRequest.MessagingSystem);
			Assertion.AssertEquals("MessageType", GEPProviderAPICommandList.Codes.SearchBeneficiary, deserializedObj.Header.ElectronicPaymentRequest.MessageType);
			Assertion.AssertEquals("IsProductionSystem", false, deserializedObj.Header.ElectronicPaymentRequest.IsProductionSystem);

			var companyCode = deserializedObj.Header.ElectronicPaymentRequest.CompanyCode;
			var matchingGEPMessageDealSpecificElements = gepMessageWithJSONPayload.FirstOrDefault(x => x.CompanyCode == companyCode);
			Assertion.AssertNotNull($"Company [{companyCode}] must be included in the message", matchingGEPMessageDealSpecificElements);
			Assertion.AssertEquals("BranchCode", matchingGEPMessageDealSpecificElements.BranchCode, deserializedObj.Header.ElectronicPaymentRequest.BranchCode);
			Assertion.AssertEquals(matchingGEPMessageDealSpecificElements.BranchPk, ediMessage.EM_GB);

			var beneficiaryRequestMessageFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(deserializedObj.Payload));
			Assertion.AssertEquals("Pay Load", matchingGEPMessageDealSpecificElements.JsonPayload, beneficiaryRequestMessageFromPayload);
		}

		internal void AssertEDIInterchangeForDeal(IXmlEDIInterchange interchange, List<GEPMessageWithJSONPayload> gepMessageDealSpecificElements)
		{
			Assertion.AssertEquals(ZBool.True, interchange.EI_IsActive);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, interchange.EI_ApplicationCode);
			Assertion.AssertEquals("OFX", interchange.EI_InterchangeType);
			Assertion.AssertEquals("TRX", interchange.EI_ReceiveTransmit);
			Assertion.AssertEquals("HQU", interchange.EI_Status);
			Assertion.AssertEquals(GlobalElectronicPaymentEDIInterchangeCreator.DestinationParty, interchange.EI_To);

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var ediMessages = interchange.Factory.Load<XmlEDIMessage>(query);
			Assertion.AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			AssertEDIMessageForDeal(ediMessages[0], gepMessageDealSpecificElements);
		}

		internal void AssertEDIMessageForDeal(XmlEDIMessage ediMessage, List<GEPMessageWithJSONPayload> gepMessageDealSpecificElements)
		{
			Assertion.AssertEquals(ZBool.True, ediMessage.EM_IsActive);
			Assertion.AssertEquals(ZString.Empty, ediMessage.EM_MessageOwner);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, ediMessage.EM_ApplicationCode);
			Assertion.AssertEquals(ProviderCodes.OFX, ediMessage.EM_MessageType);
			Assertion.AssertEquals(GEPProviderAPICommandList.Codes.CreateADeal, ediMessage.EM_MessageSubType);
			Assertion.AssertEquals("TRX", ediMessage.EM_ReceiveTransmit);
			Assertion.AssertEquals("SNT", ediMessage.EM_Status);
			Assertion.AssertEquals(Env.CurrentDepartment.PK, ediMessage.EM_GE);
			Assertion.AssertEquals(AccEPaymentDealSchema.Constants.TableName, ediMessage.EM_LinkTable);

			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicPayment>(ediMessage.EM_MessageText);
			Assertion.AssertEquals("MessagingSystem", ProviderCodes.OFX, deserializedObj.Header.ElectronicPaymentRequest.MessagingSystem);
			Assertion.AssertEquals("MessageType", GEPProviderAPICommandList.Codes.CreateADeal, deserializedObj.Header.ElectronicPaymentRequest.MessageType);
			Assertion.AssertEquals("IsProductionSystem", false, deserializedObj.Header.ElectronicPaymentRequest.IsProductionSystem);

			var companyCode = deserializedObj.Header.ElectronicPaymentRequest.CompanyCode;
			var matchingGEPMessageDealSpecificElements = gepMessageDealSpecificElements.FirstOrDefault(x => x.CompanyCode == companyCode);
			Assertion.AssertNotNull($"Company [{companyCode}] must be included in the message", matchingGEPMessageDealSpecificElements);
			Assertion.AssertEquals("BranchCode", matchingGEPMessageDealSpecificElements.BranchCode, deserializedObj.Header.ElectronicPaymentRequest.BranchCode);
			Assertion.AssertEquals(matchingGEPMessageDealSpecificElements.BranchPk, ediMessage.EM_GB);

			var dealMessageFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(deserializedObj.Payload));
			Assertion.AssertEquals("Pay Load", matchingGEPMessageDealSpecificElements.JsonPayload, dealMessageFromPayload);
		}

		internal void AssertEDIInterchangeForQuote(IXmlEDIInterchange interchange, List<GEPMessageWithUniversalTransactionPayload> gepMessageWithUniversalTransactionPayload)
		{
			Assertion.AssertEquals(ZBool.True, interchange.EI_IsActive);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, interchange.EI_ApplicationCode);
			Assertion.AssertEquals("OFX", interchange.EI_InterchangeType);
			Assertion.AssertEquals("TRX", interchange.EI_ReceiveTransmit);
			Assertion.AssertEquals("HQU", interchange.EI_Status);
			Assertion.AssertEquals(GlobalElectronicPaymentEDIInterchangeCreator.DestinationParty, interchange.EI_To);

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var ediMessages = interchange.Factory.Load<XmlEDIMessage>(query);
			Assertion.AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			AssertEDIMessageForQuote(ediMessages[0], gepMessageWithUniversalTransactionPayload);
		}

		internal void AssertEDIMessageForQuote(XmlEDIMessage ediMessage, List<GEPMessageWithUniversalTransactionPayload> gepMessageWithUniversalTransactionPayload)
		{
			Assertion.AssertEquals(ZBool.True, ediMessage.EM_IsActive);
			Assertion.AssertEquals(ZString.Empty, ediMessage.EM_MessageOwner);
			Assertion.AssertEquals(ApplicationCodeList.Codes.GlobalElectronicPayment, ediMessage.EM_ApplicationCode);
			Assertion.AssertEquals(ProviderCodes.OFX, ediMessage.EM_MessageType);
			Assertion.AssertEquals(GEPProviderAPICommandList.Codes.GetRates, ediMessage.EM_MessageSubType);
			Assertion.AssertEquals("TRX", ediMessage.EM_ReceiveTransmit);
			Assertion.AssertEquals("SNT", ediMessage.EM_Status);
			Assertion.AssertEquals(AccEPaymentQuoteSchema.Constants.TableName, ediMessage.EM_LinkTable);
			Assertion.AssertEquals(Env.CurrentDepartment.PK, ediMessage.EM_GE);

			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicPayment>(ediMessage.EM_MessageText);
			Assertion.AssertEquals("MessagingSystem", ProviderCodes.OFX, deserializedObj.Header.ElectronicPaymentRequest.MessagingSystem);
			Assertion.AssertEquals("MessageType", GEPProviderAPICommandList.Codes.GetRates, deserializedObj.Header.ElectronicPaymentRequest.MessageType);
			Assertion.AssertEquals("IsProductionSystem", false, deserializedObj.Header.ElectronicPaymentRequest.IsProductionSystem);

			var companyCode = deserializedObj.Header.ElectronicPaymentRequest.CompanyCode;
			var matchingGEPMessageQuoteSpecificElements = gepMessageWithUniversalTransactionPayload.FirstOrDefault(x => x.CompanyCode == companyCode);

			Assertion.AssertNotNull($"Company [{companyCode}] must be included in the message", matchingGEPMessageQuoteSpecificElements);
			Assertion.AssertEquals("BranchCode", matchingGEPMessageQuoteSpecificElements.BranchCode, deserializedObj.Header.ElectronicPaymentRequest.BranchCode);
			Assertion.AssertEquals(matchingGEPMessageQuoteSpecificElements.BranchPk, ediMessage.EM_GB);

			var universalTransactionFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(deserializedObj.Payload));
			Assertion.AssertEquals("Pay Load", matchingGEPMessageQuoteSpecificElements.UniversalTransaction, universalTransactionFromPayload);
		}

		internal class GEPMessageWithUniversalTransactionPayload
		{
			public GEPMessageWithUniversalTransactionPayload(string companyCode, string branchCode, string universalTransaction, ZGuid branchPk)
			{
				CompanyCode = companyCode;
				BranchCode = branchCode;
				BranchPk = branchPk;
				UniversalTransaction = universalTransaction;
			}

			internal string CompanyCode { get; }
			internal string BranchCode { get; }
			internal ZGuid BranchPk { get; }
			internal string UniversalTransaction { get; }
		}

		internal class GEPMessageWithJSONPayload
		{
			public GEPMessageWithJSONPayload(string companyCode, string branchCode, string jsonPayload, ZGuid branchPk)
			{
				CompanyCode = companyCode;
				BranchCode = branchCode;
				BranchPk = branchPk;
				JsonPayload = jsonPayload;
			}

			internal string CompanyCode { get; }
			internal string BranchCode { get; }
			internal ZGuid BranchPk { get; }
			internal string JsonPayload { get; }
		}

		#region Implementation

		public TestObjectCreator ObjectCreator { get; }

		BusinessObjectFactory Factory => ObjectCreator.Factory;

		#endregion
	}
}
