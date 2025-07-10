#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ProductionRules.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using BeneficiaryRequestStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using Event = Enterprise.ZArchitecture.Business.Event;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;
using IntegrationCustoms = Enterprise.Integration.Customs;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business
{
	/// <summary>
	/// Class to create tests objects to simplify testing setup
	/// </summary>
	[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Testing code")]
	[SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Testing code")]
	public class TestObjectCreator
	{
		public static ZGuid StandardTemplatePK
		{
			get { return new ZGuid("C067FF8A-63F6-4504-9523-741AAB00D82A"); }
		}

		int ChequeCount;
		public readonly BusinessObjectFactory Factory;
		readonly bool IsLoadBeforeCreate;

		public TestObjectCreator(BusinessObjectFactory factory, bool isLoadBeforeCreate = false) : this(factory)
		{
			IsLoadBeforeCreate = isLoadBeforeCreate;
		}

		public TestObjectCreator(BusinessObjectFactory factory)
		{
			if (ValidTestDataGenerationException.ShouldThrow)
			{
				throw new ValidTestDataGenerationException("This class is for testing only.");
			}

			this.Factory = factory;
		}

		BusinessObjectFactory GetNewFactory()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = Factory.RefreshEnabled };
			if (Factory.IsValidationSuspended)
			{
				newFactory.SuspendValidation();
			}

			return newFactory;
		}

		public ZDateTime Today = ZDateTime.Today;
		public ZDateTime Tomorrow = ZDateTime.Today.AddDays(1);

		#region Cash Advance

		public AccCashAdvanceRequestHeader CreateCashAdvanceRequestHeader(ZGuid jobPk, ZGuid orgPk, ZString ledger, ZDecimal localAmount, ZDecimal osAmount, ZString currency, ZString status, BusinessObjectFactory newFactory = null)
		{
			var factoryToUse = newFactory ?? Factory;
			var header = factoryToUse.New<AccCashAdvanceRequestHeader>();
			header.CAH_RequestReferenceNumber = header.PK.ToString().Substring(0, 8);
			header.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.CAH_JH_Job = jobPk;
			header.CAH_Ledger = ledger;
			header.CAH_LocalAmount = localAmount;
			header.CAH_OSAmount = osAmount;
			header.CAH_RX_NKTransactionCurrency = currency;
			header.CAH_Status = status;
			header.CAH_OH_Organization = orgPk;
			return header;
		}

		public AccCashAdvanceRequestLine CreateCashAdvanceRequestLine(ZGuid requestHeaderPk, ZDecimal localAmount, ZDecimal osAmount, ZString status, BusinessObjectFactory newFactory = null)
		{
			var factoryToUse = newFactory ?? Factory;
			var line = factoryToUse.New<AccCashAdvanceRequestLine>();
			line.CAL_CAH_RequestHeader = requestHeaderPk;
			line.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
			line.CAL_LocalAmount = localAmount;
			line.CAL_OSAmount = osAmount;
			line.CAL_Status = status;
			return line;
		}

		#endregion

		#region Purchase Order
		public AccPayableOrderHeader CreateNewPendingApprovalPurchaseOrder(ZDecimal amount)
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order.APH_Disposition = Constants.PayableOrderDisposition.PendingApproval;

			var line = order.OrderLines.AddNew();
			line.GenericCharge = CC1.PK;
			line.APL_LinePrice = amount;
			Factory.Save();

			return order;
		}
		#endregion

		#region Electronic Payment

		public EPaymentQuote CreateEPaymentQuote(PaymentApprovalBase paymentApproval = null, string providerType = EPaymentProviderCodes.Codes.OFX)
		{
			if (paymentApproval == null)
			{
				paymentApproval = CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.EFT, AUDBankAccount, AUDChequeBook);
				paymentApproval.AV_Amount = 100;
			}

			var quote = Factory.New<EPaymentQuote>();
			quote.QU_AV = paymentApproval.PK;
			quote.QU_ProviderCode = providerType;
			quote.QU_ToAmount = paymentApproval.AV_Amount;
			quote.QU_RX_NKToCurrency = paymentApproval.AV_RX_NKPaymentCurrency;
			quote.QU_RX_NKFromCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			quote.QU_GC = GlbCompany.CurrentCompany.PK;
			return quote;
		}

		public EPaymentQuote CreateValidEPaymentQuoteForStatus(ZString status, PaymentApprovalBase paymentApproval = null)
		{
			var quote = CreateEPaymentQuote(paymentApproval, EPaymentProviderCodes.Codes.OFX);
			paymentApproval = paymentApproval ?? Factory.Load<PaymentApprovalBase>(quote.PaymentApproval.PK);
			quote.QU_Status = status;

			switch (status)
			{
				case QuoteStatusCodes.Accepted:
				case QuoteStatusCodes.Expired:
				case QuoteStatusCodes.Received:
				case QuoteStatusCodes.Error:
					quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
					quote.QU_ExchangeRate = 1;
					quote.QU_ExchangeRateInverted = 1;
					quote.QU_FromAmount = paymentApproval.AV_Calc_LocalAmount;
					quote.QU_ProviderReference = "12345";
					break;

				default:
					break;
			}

			return quote;
		}

		public EPaymentDeal CreateEPaymentDeal(PaymentApprovalBase paymentApproval = null, EPaymentQuote quote = null)
		{
			quote = quote ?? CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			var deal = Factory.New<EPaymentDeal>();
			deal.AED_QU_Quote = quote.PK;
			deal.AED_GC_Company = GlbCompany.CurrentCompany.PK;
			deal.AED_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			return deal;
		}

		public EPaymentDeal CreateValidEPaymentDealForStatus(ZString status, PaymentApprovalBase paymentApproval = null, EPaymentQuote quote = null)
		{
			quote = quote ?? CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			var deal = CreateEPaymentDeal(paymentApproval, quote);
			// Manually run this ResetCurrentDeal method, because in testing, we don't necessarily begin with the normal sequence
			// of AED_status = DealStatusCodes.Queued -- which would trigger this method to be run on saving.
			paymentApproval?.ResetCurrentDeal();
			deal.AED_Status = status;
			switch (status)
			{
				case DealStatusCodes.Accepted:
				case DealStatusCodes.InProgress:
				case DealStatusCodes.Paid:
				case DealStatusCodes.Failed:
					deal.AED_ProviderReference = deal.PK.ToString().Substring(0, 8);
					deal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
					break;
				case DealStatusCodes.Declined:
					deal.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
					break;
				default:
					break;
			}

			return deal;
		}

		public AccBankAccount CreateEPaymentBankAccount()
		{
			var bankAccount = CreateBankAccount("EPA", "E-Payment Account", USD, GLHeader1, AccountTypeCodeDescriptionPairList.Codes.EPA);
			bankAccount.AB_GC = Env.CurrentCompanyPK;
			bankAccount.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			return bankAccount;
		}

		public PaymentApprovalWithAuthorisation GetNewTestAPPaymentApproval(OrgHeader org, AccBankAccount bank, ZString receiptType,
			ZString chequeOrRef, ZDecimal amount, AccChequeBook chequeBook = null)
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = org.PK;
			paymentApproval.AV_AB = bank.PK;
			paymentApproval.AV_PaymentType = receiptType;
			paymentApproval.AV_AK = chequeBook?.PK ?? ZGuid.Empty;
			paymentApproval.AV_ChequeOrReference = chequeOrRef;
			paymentApproval.AV_Amount = amount;
			return paymentApproval;
		}

		public PaymentApprovalWithAuthorisation GetNewTestAPPaymentApproval(OrgHeader org, AccBankAccount bank, ZString receiptType, ZString chequeOrRef,
			ZDecimal amount, ZString currency, ZDecimal exRate, ZString status, AccChequeBook chequeBook = null)
		{
			var paymentApproval = GetNewTestAPPaymentApproval(org, bank, receiptType, chequeOrRef, amount, chequeBook);
			paymentApproval.AV_RX_NKPaymentCurrency = currency;
			paymentApproval.AV_PayExRate = exRate;
			paymentApproval.AV_Status = status;
			paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			return paymentApproval;
		}

		public AccEPaymentStaffToken CreateEPaymentStaffToken(ZGuid bankAccountPk, ZDateTime expiryDateUtc, ZString staffCode, ZString status)
		{
			var staffToken = Factory.New<AccEPaymentStaffToken>();
			staffToken.TK_AB = bankAccountPk;
			staffToken.TK_ExpiryUtc = expiryDateUtc;
			staffToken.TK_GC = Env.CurrentCompanyPK;
			staffToken.TK_GS_NKStaffCode = staffCode;
			staffToken.TK_Status = status;
			return staffToken;
		}

		public AccEPaymentBeneficiaryRequest CreateValidEPaymentBeneficiaryRequestForStatus(ZString status)
		{
			var request = Factory.New<AccEPaymentBeneficiaryRequest>();
			request.ABR_GC_Company = Env.CurrentCompany.PK;
			request.ABR_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			request.ABR_InternalReference = request.PK.ToString().Substring(0, 8);
			request.ABR_Status = status;
			switch (status)
			{
				case BeneficiaryRequestStatusCodes.Partial:
				case BeneficiaryRequestStatusCodes.Received:
				case BeneficiaryRequestStatusCodes.Error:
					request.ABR_LastResponseReceivedUtc = ZDateTime.UtcNow;
					break;
				default:
					break;
			}

			return request;
		}

		public AccEPaymentBeneficiary CreateEPaymentBeneficiary(ZString providerReference)
		{
			var beneficiary = Factory.New<AccEPaymentBeneficiary>();
			beneficiary.ABF_GC_Company = Env.CurrentCompanyPK;
			beneficiary.ABF_RN_NKCountryCode = CountryCodes.UnitedStates;
			beneficiary.ABF_RX_NKAccountCurrency = CurrencyCodes.UnitedStates;
			beneficiary.ABF_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			beneficiary.ABF_ProviderReference = providerReference;
			return beneficiary;
		}

		public static IDisposable SetupEnableEPaymentFunctionalityRegistry(Guid companyPK, bool isOFXEPaymentsEnabled)
		{
			if (companyPK == Guid.Empty)
			{
				throw new ArgumentException("companyPK cannot be empty. Enable E-Payment Functionality registry can be configured at company level only.");
			}
			else
			{
				var company = new BusinessObjectFactory().Load<GlbCompany>(companyPK);
				if (company != null)
				{
					var collection = new EPaymentConfigurationCollection();
					var config = collection.AddNew();
					config.CountryCode = company.Country.Code;
					config.CountryDescription = company.Country.Description;
					config.OFXEPaymentEnabled = isOFXEPaymentsEnabled;
					return AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, collection);
				}
				else
				{
					throw new ArgumentException("Please provide a valid companyPK. Enable E-Payment Functionality registry can be configured at company level only.");
				}
			}
		}

		#endregion

		#region Random string

		public static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		public static int GetRandomInt(int minVale, int maxValue) => Generator.Next(minVale, maxValue);

		static Random Generator
		{
			get { return generator ?? (generator = new Random()); }
		}
		[ThreadStatic]
		static Random generator;

		#endregion

		#region Job and JobExchangeRate

		public Job InsertJobHeader(ZGuid gB_PK, ZGuid gE_PK)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = gB_PK;
			job.JH_GE = gE_PK;
			return job;
		}

		public Job CreateJob(OrgHeader localClient, decimal localClientCFX, OrgHeader agent, decimal agentCFX)
		{
			return CreateJob(GetRandomString(10), localClient, localClientCFX, agent, agentCFX);
		}

		public Job CreateJob(ZString jobNumber, OrgHeader localClient, decimal localClientCFX, OrgHeader agent, decimal agentCFX)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_OA_LocalChargesAddr = localClient != null ? localClient.MainAddress.PK : ZGuid.Empty;
			job.JH_OA_AgentCollectAddr = agent != null ? agent.MainAddress.PK : ZGuid.Empty;

			localClient?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			agent?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
			return job;
		}

		public Job CreateJob(IJobInvoicingPlugIn parent, OrgHeader localClientOrg, decimal localClientCFX, OrgHeader agentOrg, decimal agentCFX)
		{
			return CreateJob(parent, false, false, true, localClientOrg, localClientCFX, agentOrg, agentCFX);
		}

		public Job CreateJob(IJobInvoicingPlugIn parent, bool createWithMutex = true, bool setCurrentDepartment = true, bool setCurrentBranch = true,
			OrgHeader localClientOrg = null, decimal localClientCFX = 0m,
			OrgHeader agentOrg = null, decimal agentCFX = 0m,
			BusinessObjectFactory newFactory = null)
		{
			var jobFactory = newFactory ?? Factory;
			var job = createWithMutex ? new Job.Loader(jobFactory, parent).TryCreateWithMutex() : new Job.Loader(jobFactory, parent).TryCreateWithoutMutexForTestOnly();
			AccountingTestHelper.AssertNotNull("Test expects job is created.", job);
			if (localClientOrg != null)
			{
				job.JH_OA_LocalChargesAddr = localClientOrg.MainAddress.PK;
			}
			if (agentOrg != null)
			{
				job.JH_OA_AgentCollectAddr = agentOrg.MainAddress.PK;
			}
			if (localClientCFX != 0)
			{
				localClientOrg?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			}
			if (agentCFX != 0)
			{
				agentOrg?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
			}
			if (setCurrentBranch || job.Branch == null)
			{
				job.JH_GB = GlbBranch.CurrentBranch.PK;
			}
			if (setCurrentDepartment)
			{
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			}
			else if (job.Department?.GE_Misc ?? true)
			{
				job.JH_GE = jobFactory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			}

			return job;
		}

		public OrgRelatedParty CreateARSettlementGroup(OrgHeader parent, OrgHeader relatedParty)
		{
			OrgRelatedParty arSettlementGroup = Factory.NewWithValidTestData<OrgRelatedParty>();
			arSettlementGroup.PR_OH_Parent = parent.PK;
			arSettlementGroup.PR_PartyType = "ARS";
			arSettlementGroup.PR_FreightDirection = "AR";
			arSettlementGroup.PR_Location = "";
			arSettlementGroup.PR_GC = GlbCompany.CurrentCompany.PK;
			arSettlementGroup.PR_OH_RelatedParty = relatedParty.PK;
			return arSettlementGroup;
		}

		public OrgRelatedParty CreateRelatedParty(OrgHeader parent, OrgHeader relatedParty, ZString partyType, string freightDirection = "", string location = "", ZGuid? companyPK = null)
		{
			OrgRelatedParty arSettlementGroup = Factory.NewWithValidTestData<OrgRelatedParty>();
			arSettlementGroup.PR_OH_Parent = parent.PK;
			arSettlementGroup.PR_PartyType = partyType;
			arSettlementGroup.PR_FreightDirection = freightDirection;
			arSettlementGroup.PR_Location = location;
			arSettlementGroup.PR_GC = companyPK ?? ZGuid.Empty;
			arSettlementGroup.PR_OH_RelatedParty = relatedParty.PK;
			return arSettlementGroup;
		}

		public JobDocAddress CreateJobDocAddress(ZGuid jobParentID, string addressType, ZGuid addressPK)
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_OA_Address = addressPK;
			address.E2_AddressType = addressType;
			address.E2_ParentID = jobParentID;
			return address;
		}
		//#warning delete?
		internal ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			exchangeRate.JF_IsTransformed = true;
			return exchangeRate;
		}

		public ExchangeRate SetExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate, ZGuid? orgPk = null, ExchangeRateOrgTypeEnum orgType = ExchangeRateOrgTypeEnum.None)
		{
			var rate = parentJob.ExchangeRates.AddRate(currency, buyRate, orgPk ?? ZGuid.Empty, orgType);
			if (rate != null)
			{
				rate.JF_BaseRate = buyRate;
				rate.JF_IsTransformed = true;
			}
			return rate;
		}

		public void CreateExchangeRate(RefCurrency currency, decimal buyRate)
		{
			CreateExchangeRate(currency, Core.Constants.ExchangeRateTypes.Code.BuyRate, buyRate);
		}

		public void CreateExchangeRate(RefCurrency currency, string rateType, decimal rate)
		{
			CreateExchangeRate(currency, rateType, rate, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		}

		public void CreateUSDBuyRate(decimal rate, ZDateTime date)
		{
			CreateExchangeRate(this.USD, "BUY", rate, date, date);
		}

		public void CreateExchangeRate(RefCurrency currency, string rateType, decimal rate, ZDateTime startDate, ZDateTime endDate)
		{
			CreateExchangeRate(currency, rateType, rate, startDate, endDate, ZGuid.Empty);
		}

		public void CreateExchangeRate(RefCurrency currency, string rateType, decimal rate, ZDateTime startDate, ZDateTime endDate, ZGuid localClient)
		{
			var curencyInNewFactory = GetNewFactory().Load<RefCurrency>(currency.PK);
			RefExchangeRate exchangeRate = curencyInNewFactory.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = rateType;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_StartDate = new ZDateTime(startDate.Year, startDate.Month, startDate.Day);
			exchangeRate.RE_ExpiryDate = new ZDateTime(endDate.Year, endDate.Month, endDate.Day);
			exchangeRate.RE_OH_Client = localClient;
			curencyInNewFactory.Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public AccChargeGovtChargeCodeOverride SetupOrCreateGovtChargeCodeOverride(AccChargeCode chargeCode, ZString costSellAll, ZString jobType, ZString transportMode, ZString direction, ZString govtChargeCode)
		{
			var overrideGovt = chargeCode.GovtChargeCodeOverrides.Cast<AccChargeGovtChargeCodeOverride>()
				.FirstOrDefault(item => item.ACG_JobType == jobType
					&& item.ACG_CostSellAll == costSellAll
					&& item.ACG_TransportMode == transportMode
					&& item.ACG_Direction == direction);

			if (overrideGovt == null)
			{
				overrideGovt = chargeCode.GovtChargeCodeOverrides.AddNew();
				overrideGovt.ACG_CostSellAll = costSellAll;
				overrideGovt.ACG_JobType = jobType;
				overrideGovt.ACG_TransportMode = transportMode;
				overrideGovt.ACG_Direction = direction;
			}

			overrideGovt.ACG_GovtChargeCode = govtChargeCode;

			return overrideGovt;
		}

		public IJobInvoicingPlugIn GetTestShipmentPlugIn()
		{
			return GetTestShipmentPlugIn("S00001001", GetIJobInvoicingSupporterMock());
		}

		public IJobInvoicingPlugIn GetTestShipmentPlugIn(string jobNumber)
		{
			return GetTestShipmentPlugIn(jobNumber, GetIJobInvoicingSupporterMock());
		}

		public Mock<IJobInvoicingSupporter> GetIJobInvoicingSupporterMock()
		{
			return GetIJobInvoicingSupporterMock(JobInvoicingConsumerTypes.Shipment);
		}

		public Mock<IJobInvoicingSupporter> GetIJobInvoicingSupporterMock(JobInvoicingConsumerType consumerType)
		{
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.Consignee).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.Consignor).Returns(Factory.NewWithValidTestData<OrgHeader>());
			mockSupporter.Setup(m => m.IsDirectShipment).Returns(false);
			mockSupporter.Setup(m => m.ActualChargeable).Returns(100m);
			mockSupporter.Setup(m => m.ActualChargeableUnit).Returns((ZString)Core.Constants.Weight.Kilograms);
			mockSupporter.Setup(m => m.ConsolType).Returns((ZString)Constants.AgentType.Agent);
			mockSupporter.Setup(m => m.ConsolRateCurrency).Returns(USD);
			mockSupporter.Setup(m => m.ConsolExchangeRate).Returns(0.9m);
			mockSupporter.Setup(m => m.Origin).Returns(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			mockSupporter.Setup(m => m.Destination).Returns(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX"));
			mockSupporter.Setup(m => m.TransportMode).Returns((ZString)Constants.TransportModes.Air);
			mockSupporter.Setup(m => m.ContainerMode).Returns((ZString)Constants.ContainerModes.Loose);
			mockSupporter.Setup(m => m.IsImport).Returns(false);
			mockSupporter.Setup(m => m.IsExport).Returns(true);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.ConsumerType).Returns(consumerType);
			mockSupporter.Setup(m => m.MasterBillNumber).Returns((ZString)"MASTERBILL01");
			mockSupporter.Setup(m => m.HouseBillNumber).Returns((ZString)"HOUSEBILL01");
			mockSupporter.Setup(m => m.ATA).Returns(ZDateTime.Empty);
			mockSupporter.Setup(m => m.ETA).Returns(ZDateTime.Now.AddDays(1));
			mockSupporter.Setup(m => m.ATD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ETD).Returns(ZDateTime.Now.AddDays(-1));
			mockSupporter.Setup(m => m.ActualWeight).Returns((ZDecimal)100m);
			mockSupporter.Setup(m => m.ActualWeightUnit).Returns((ZString)Core.Constants.Weight.Pounds);
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			mockSupporter.Setup(m => m.DefaultChargeGroup).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);
			mockSupporter.Setup(m => m.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate)).Returns(ZDateTime.Empty);
			mockSupporter.Setup(m => m.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, It.IsAny<string>())).Returns(ZDateTime.Empty);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);
			mockSupporter.Setup(m => m.ServiceLevel).Returns("STD");
			mockSupporter.Setup(m => m.CustomsEntryNumberType).Returns("TF");
			mockSupporter.Setup(m => m.CommunityTransitStatus).Returns("TF");

			return mockSupporter;
		}

		public Mock<IJobInvoicingPlugIn> GetTestShipmentPlugInMock(string jobNumber, Mock<IJobInvoicingSupporter> mockSupporter)
		{
			var mockPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockPlugIn.Setup(m => m.Factory).Returns(Factory);

			return mockPlugIn;
		}

		public IJobInvoicingPlugIn GetTestShipmentPlugIn(string jobNumber, Mock<IJobInvoicingSupporter> mockSupporter)
		{
			var mockPlugIn = GetTestShipmentPlugInMock(jobNumber, mockSupporter);
			mockPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockPlugIn.Setup(m => m.JobNumber).Returns(jobNumber);
			mockPlugIn.Setup(m => m.IsDeleted).Returns(false);
			mockPlugIn.Setup(m => m.IsInDatabase).Returns(true);
			return mockPlugIn.Object;
		}

		#endregion

		#region Setting CurrentCompany properties

		public bool SetCurrentCompanyReciprocal(bool newValue)
		{
			var oldValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = newValue;
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = newValue;

			var newFactory = new BusinessObjectFactory();
			var currentCompany = newFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = newValue;
			newFactory.Save();

			AccountingTestHelper.AssertEquals("Precondition: IsReciprocal has been set properly for Env.CurrentCompany", newValue, Env.CurrentCompany.IsReciprocal);
			AccountingTestHelper.AssertEquals("Precondition: IsReciprocal has been set properly for GlbCompany.CurrentCompany", newValue, GlbCompany.CurrentCompany.GC_IsReciprocal);

			return oldValue;
		}

		public string SetCurrentCompanyCountryCode(string newValue)
		{
			var oldValue = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(newValue);
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).SetCountry(newValue);
			return oldValue;
		}

		#endregion

		#region Invoices and Credit Notes

		#region Transaction with minimum Test Data

		public static T CreateInvoiceWithMinimumTestData<T>(BusinessObjectFactory factory)
			where T : InvoicingBase
		{
			var transaction = factory.New<T>();
			FillInvoiceWithMinimumTestData(transaction);

			return transaction;
		}

		public static void FillInvoiceWithMinimumTestData(InvoicingBase header)
		{
			if (header.AH_TransactionNum == ZString.Empty &&
				(header.AH_Ledger == LedgerTypes.AccountsPayable || header.AH_Ledger == LedgerTypes.IncompleteTransactions || header.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions))
			{
				header.AH_TransactionNum = Guid.NewGuid().ToString();
			}
		}

		#endregion

		#region Brexit data setup

		public ZDateTime SetupPostBrexitData()
		{
			var brexitDate = new ZDateTime("2019-03-30 00:00:00");
			Db.Connection.ExecuteNonQuery(FormattableString.Invariant($@"
IF NOT EXISTS (SELECT null FROM RefDatabase_RefDataGrouping WHERE ZZZ_DataGrouping = 'ZZ')
INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'ZZ', 'TEST', NULL)
INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), 'FUNC', 'JUST FOR TEST', 1, 'ZZ')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES (NEWID(), 'FUNC', 'ACCBREXIT', '_X_1', '{brexitDate}', '2079-06-06 23:59:00', 'ZZ')"));

			var gbCountry = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			gbCountry.RN_EconomicGrouping = "";
			return brexitDate;
		}

		#endregion

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public ARInvoice CreateARInvoice<T>(ZString transactionNum, RefCurrency currency, decimal exchangeRate, OrgHeader debtor, string complianceSubType = "", string complianceReference = "") where T : ARInvoice
		{
			var result = (ARInvoice)CreateInvoice(typeof(T), transactionNum, currency, exchangeRate);
			if (!complianceSubType.IsNullOrEmpty())
			{
				result.AH_ComplianceSubType = complianceSubType;
			}
			if (!complianceReference.IsNullOrEmpty())
			{
				result.AH_TransactionReference = complianceReference;
			}
			result.AH_OH = debtor.PK;
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public APInvoice CreateAPInvoice<T>(ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount, OrgHeader creditor, bool addGLAccount = true) where T : APInvoice
		{
			APInvoice invoice = CreateAPInvoice<T>(transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localExtraTaxAmount, addGLAccount);
			invoice.AH_OH = creditor.PK;
			return invoice;
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public APInvoice CreateAPInvoice<T>(ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount, bool addGLAccount = true) where T : APInvoice
		{
			APInvoice invoice = (APInvoice)CreateInvoice(typeof(T), currency, exchangeRate);
			invoice.AH_TransactionNum = transactionNum;
			CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localExtraTaxAmount, addGLAccount);
			return invoice;
		}

		public APPayment CreateCashAPPaymentForInvoice(APInvoice aPInvoice)
		{
			APPayment result = Factory.New<APPayment>();
			result.AH_ExchangeRate = aPInvoice.AH_ExchangeRate;
			result.AH_OSExTaxAmount = aPInvoice.AH_OSExTaxAmount;
			result.AH_PostDate = aPInvoice.AH_PostDate;
			result.AH_DueDate = aPInvoice.AH_PostDate;
			result.AH_Desc = "Payment for Invoice " + aPInvoice.AH_TransactionNum.ToString();
			result.AH_OH = aPInvoice.AH_OH;
			result.AH_GB = aPInvoice.AH_GB;
			result.AH_GE = aPInvoice.AH_GE;
			result.AH_ReceiptType = ReceiptTypes.Cash;
			result.AH_AB = this.AUDBankAccount.PK;
			result.AH_ChequeOrReference = "Cash";
			return result;
		}

		public APBulkInvoicePoster CreateAPBulkInvoicePoster(IEnumerable<Accrual> accruals, bool setupRegistry = false)
		{
			if (setupRegistry)
			{
				AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, this.GLHeader1.PK.ToGuid());
			}
			var poster = new APBulkInvoicePoster(Factory);
			poster.Accruals.AddRange(accruals);
			poster.UpdateRetrievedAccrualsTotal();
			poster.ExpectedBatchTotal = poster.RetrievedAccrualTotal;

			return poster;
		}

		public APInvoiceForBulkPoster CreateAPInvoiceForBulkPoster(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount, ZGuid taxRate, ZGuid orgPK)
		{
			APInvoiceForBulkPoster invoice = (APInvoiceForBulkPoster)CreateInvoice(invoiceType, currency, exchangeRate);
			invoice.AH_OH = orgPK;
			invoice.TaxRate = taxRate;
			invoice.AH_TransactionNum = transactionNum;
			CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localExtraTaxAmount);

			return invoice;
		}

		public APInvoiceForBulkPoster CreateAPInvoiceForBulkPoster(APBulkInvoicePoster poster, ZString transactionNum, decimal osExTaxAmount, decimal osTaxAmount, OrgHeader org)
		{
			var invoice = poster.Invoices.AddNew();
			invoice.AH_OH = org.PK;
			invoice.AH_TransactionNum = transactionNum;
			invoice.AH_OSExTaxAmount = osExTaxAmount;
			invoice.AH_OSTaxAmount = osTaxAmount;
			invoice.AH_LocalTaxAmount = osTaxAmount;

			return invoice;
		}

		public void CreateJobShipmentWithFIDCharge(string shipmentNum, OrgHeader org, AccChargeCode chargeCode, decimal osCostAmt, decimal osSellAmt, bool includeLinesWithDifferentBranchDepartment = false)
		{
			var shipment = CreateShipment(shipmentNum);
			shipment.JS_RS_NKServiceLevel = "STD";
			using (var job = CreateJob(shipment))
			{
				job.JH_OA_LocalChargesAddr = org.Addresses.MainAddress.PK;
				org.CompanyData.InvoiceTypes.AddNew();
				org.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				org.CompanyData.InvoiceTypes[0].PI_RS_NKServiceLevel = "STD";
				var charge = CreateCharge(job, chargeCode, osCostAmt, osSellAmt);
				charge.JR_OH_SellAccount = org.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge.JR_AT_SellGSTRate = GST1.PK;

				if (includeLinesWithDifferentBranchDepartment)
				{
					var charge2 = CreateCharge(job, chargeCode, osCostAmt, osSellAmt);
					charge2.JR_OH_SellAccount = org.PK;
					charge2.JR_GB = NonCurrentBranch.PK;
					charge2.JR_GE = FIADepartment.PK;
					charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					charge2.JR_AT_SellGSTRate = GST1.PK;
				}

				Factory.Save();
			}
		}

		public InvoicingBase CreateInvoice(Type invoiceType, RefCurrency currency = null, decimal? exchangeRate = null, OrgHeader organisation = null, ZDateTime? invoiceDate = null)
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(invoiceType);

			invoice.AH_Desc = "Test Invoice";

			if (invoiceDate.HasValue)
			{
				invoice.AH_InvoiceDate = invoiceDate.Value;
			}
			if (currency != null)
			{
				invoice.ExchangeRate.Currency = currency.RX_Code;
			}
			if (exchangeRate != null)
			{
				invoice.ExchangeRate.Rate = exchangeRate.Value;
			}
			if (organisation != null)
			{
				invoice.AH_OH = organisation.PK;
			}
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				invoice.AH_TransactionNum = Guid.NewGuid().ToString();
			}
			if (invoice.ExchangeRate.Rate == 0)
			{
				invoice.ExchangeRate.Rate = 1;
			}

			return invoice;
		}

		public InvoicingBase CreateInvoice(Type invoiceType, ZString transactionNum, RefCurrency currency = null, decimal? exchangeRate = null, OrgHeader organisation = null, ZDateTime? invoiceDate = null)
		{
			InvoicingBase invoice = CreateInvoice(invoiceType, currency, exchangeRate, organisation, invoiceDate);
			invoice.AH_TransactionNum = transactionNum;

			return invoice;
		}

		public InvoicingBase CreateInvoiceWithLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount)
		{
			InvoicingBase invoice = CreateInvoice(invoiceType, transactionNum, currency, exchangeRate);
			CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, 0m, localExTaxAmount, localTaxAmount, 0m);

			return invoice;
		}

		public Invoice CreateCashInvoice(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, OrgHeader organisation, decimal osExTaxAmount, decimal osTaxAmount, ZGuid genericChargePK, AccBankAccount bankAccount)
		{
			if (!(typeof(ARInvoice).IsAssignableFrom(invoiceType) || typeof(APInvoice).IsAssignableFrom(invoiceType)))
			{
				throw new ArgumentException("Cash invoice is only applicable for AR and AP invoice");
			}

			var invoice = (Invoice)CreateInvoice(invoiceType, transactionNum, currency, exchangeRate, organisation);
			CreateInvoiceLine(invoice, currency, exchangeRate, osExTaxAmount, osTaxAmount, genericChargePK);

			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;

			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			invoice.ReceiptPaymentAH_AB = bankAccount.PK;
			invoice.ReceiptPaymentAH_ChequeOrReference = "CASH";
			invoice.ReceiptPaymentAH_InvoiceDate = ZDateTime.Today;
			invoice.ReceiptPaymentAH_PostDate = ZDateTime.Today;
			invoice.ReceiptPaymentAH_Desc = "Desc";
			return invoice;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount, bool addGLAccount = true)
		{
			InvoicingLineBase line = CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, addGLAccount);
			line.AL_LocalExTaxAmount = localExTaxAmount;
			line.AL_LocalTaxAmount = localTaxAmount;
			line.AL_LocalExtraTaxAmount = localExtraTaxAmount;
			line.AL_GB = invoice.Company.Branches[0].PK;

			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount, ZGuid genericChargePK)
		{
			InvoicingLineBase line = CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localExtraTaxAmount);
			line.GenericCharge = genericChargePK;
			if (line.GenericChargeInfo.HasNotifications())
			{ ((InvoicingLineBaseValidation)line.Validation).ValidateGenericCharge(); }
			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, ZGuid genericChargePK)
		{
			var line = CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, 0M);
			var previousLineDesc = line.AL_Desc;
			line.GenericCharge = genericChargePK;
			if (line.AL_Desc.IsEmpty && !previousLineDesc.IsEmpty)
			{
				line.AL_Desc = previousLineDesc;
			}

			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, bool addGLAccount = true, AccTaxRate taxRate = null)
		{
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			if (invoice.IsInDatabase)
			{
				invoice.AH_FullyPaidDate = ZDateTime.Empty; // To clear FullyPaidDate set OnSaving empty invoice
			}

			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			line.AL_OSExtraTaxAmount = oSExtraTaxAmount;
			line.AL_Desc = "tee he he";
			line.AL_GB = invoice.Company.Branches[0].PK;
			if (addGLAccount)
			{
				line.AL_AG = GLHeader1.PK;
			}

			if (taxRate != null)
			{
				line.AL_AT = taxRate.PK;
			}

			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, ZGuid genericChargePK)
		{
			InvoicingLineBase line = CreateInvoiceLine(invoice, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount);
			var oldValue = line.AL_OSTaxAmount;
			line.GenericCharge = genericChargePK;
			line.AL_OSTaxAmount = oldValue;
			if (line.GenericChargeInfo.HasNotifications())
			{ ((InvoicingLineBaseValidation)line.Validation).ValidateGenericCharge(); }
			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal aL_OSExTaxAmount)
		{
			return CreateInvoiceLine(invoice, aL_OSExTaxAmount, currency, exchangeRate);
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, RefCurrency currency, decimal exchangeRate, decimal aL_OSExTaxAmount, ZGuid aL_GB, bool setTaxes = true)
		{
			return CreateInvoiceLine(invoice, aL_OSExTaxAmount, currency, exchangeRate, aL_GB, setTaxes);
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, ZGuid genericChargePK, decimal aL_OSExTaxAmount, RefCurrency currency = null, decimal? exchangeRate = null)
		{
			var line = CreateInvoiceLine(invoice, aL_OSExTaxAmount, currency, exchangeRate);
			line.GenericCharge = genericChargePK;
			line.AL_AT = GST1.PK;
			if (line.AL_Desc.IsEmpty)
			{
				line.AL_Desc = "Line Description";
			}

			return line;
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase invoice, decimal aL_OSExTaxAmount, RefCurrency currency = null, decimal? exchangeRate = null, ZGuid? aL_GB = null, bool setTaxes = true)
		{
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			if (setTaxes)
			{
				line.AL_AT = GST1.PK;
				line.AL_AW = WHT1.PK;
			}
			if (currency != null)
			{
				line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			}
			if (exchangeRate != null)
			{
				line.AL_ExchangeRate = exchangeRate.Value;
			}
			line.AL_OSExTaxAmount = aL_OSExTaxAmount;
			line.AL_Desc = "tee he he";
			if (aL_GB.HasValue)
			{
				line.AL_GB = aL_GB.Value;
			}
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public InvoicingBase CreateInvoiceWithLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, OrgHeader organisation, ZGuid chargeCode, ZString transactionCategory)
		{
			InvoicingBase invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount, organisation, chargeCode);
			invoice.AH_TransactionCategory = transactionCategory;

			return invoice;
		}

		public InvoicingBase CreateInvoiceWithLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, OrgHeader organisation, ZGuid chargeCode)
		{
			InvoicingBase invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount);
			invoice.AH_OH = organisation.PK;
			invoice.Lines[0].GenericCharge = chargeCode;

			if (organisation.MainAddress != null)
			{
				invoice.AH_OA_InvoiceAddressOverride = organisation.MainAddress.PK;
			}
			return invoice;
		}

		public InvoicingBase CreateInvoiceWithGLHeaderLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, OrgHeader organisation, ZGuid accGLHeaderPK)
		{
			InvoicingBase invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount);
			invoice.AH_OH = organisation.PK;
			invoice.Lines[0].AL_AG = accGLHeaderPK;

			if (organisation.MainAddress != null)
			{
				invoice.AH_OA_InvoiceAddressOverride = organisation.MainAddress.PK;
			}
			return invoice;
		}

		public InvoicingBase CreateInvoiceWithLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, OrgHeader organisation, ZGuid chargeCode, ZDateTime dueDate, ZBool postToGL)
		{
			InvoicingBase invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount, organisation, chargeCode);
			invoice.AH_PostToGL = postToGL ? "Y" : "N";
			invoice.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_ReverseToGL = postToGL ? "Y" : "N");
			invoice.AH_DueDate = dueDate;
			return invoice;
		}

		public InvoicingBase CreateInvoiceWithLine(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, OrgHeader organisation, ZGuid chargeCode, ZDateTime postDate, ZDateTime dueDate, ZDateTime invoiceDate, ZBool postToGL)
		{
			InvoicingBase invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount, organisation, chargeCode, dueDate, postToGL);
			invoice.AH_PostDate = postDate;
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.AH_DueDate = dueDate;
			return invoice;
		}

		public void InitializeInvoicingBase(InvoicingBase invoicingBase, string transactionNum, OrgHeader orgHeader, AccChargeCode chargeCode, AccTaxRate taxRate)
		{
			invoicingBase.AH_OH = orgHeader.PK;
			invoicingBase.AH_InvoiceDate = ZDateTime.Now;
			invoicingBase.ExchangeRate.Currency = AUD.Code;
			invoicingBase.ExchangeRate.Rate = 1m;
			invoicingBase.AH_TransactionNum = transactionNum;

			var amountValue = 100m;
			foreach (InvoicingLineBase line in invoicingBase.Lines)
			{
				line.AL_LocalExTaxAmount = amountValue;
				amountValue += 10m;
				line.AL_AC = chargeCode.PK;
				line.AL_AT = taxRate?.PK ?? ZGuid.Empty;
				line.AL_LocalTaxAmount = line.AL_LocalExTaxAmount / 10m;
				line.AL_LocalExtraTaxAmount = 0m;
				line.AL_GB = invoicingBase.AH_GB;
				line.AL_Desc = Guid.NewGuid().ToString();
			}
		}

		public AccCollectionBatch CreateCollectionBatch(AccBankAccount bankAccount, GlbCompany company, ZString batchNumber, ZDecimal batchAmount, ZBool isCancelled, string batchType = "")
		{
			AccCollectionBatch batch = Factory.New<AccCollectionBatch>();
			batch.ACB_AB = bankAccount.PK;
			batch.ACB_GC = company.PK;
			batch.IsCancelled = isCancelled;
			batch.ACB_BatchNumber = batchNumber;
			batch.ACB_TotalAmount = batchAmount;
			batch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.ribaFormat;
			batch.ACB_Type = batchType;
			return batch;
		}

		public AccCollectionOrder CreateCollectionOrder(AccCollectionBatch batch, ZDate collectionDate, OrgHeader debtor, ZString orderNumber, ZDecimal orderAmount, ZBool isCancelled)
		{
			AccCollectionOrder order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = collectionDate;
			order.ACO_IsCancelled = isCancelled;
			order.ACO_OH_Debtor = debtor.PK;
			order.ACO_OrderNumber = orderNumber;
			order.ACO_Amount = orderAmount;
			return order;
		}

		public AccCollectionOrderLine CreateCollectionOrderLine(AccCollectionOrder order, TransactionHeader invoice, ZBool isCancelled)
		{
			AccCollectionOrderLine orderLine = Factory.New<AccCollectionOrderLine>();
			orderLine.AOL_ACO = order.PK;
			orderLine.AOL_AH = invoice.PK;
			orderLine.IsCancelled = isCancelled;
			return orderLine;
		}

		public void AttachJobToAPLine(TransactionLine line, JobHeader job = null)
		{
			if (line.AL_LineType == TransactionLineTypes.Cost || line.AL_LineType == TransactionLineTypes.UnapprovedCost)
			{
				line.AL_JH = (job ?? Job1).PK;
			}
		}

		public void AttachChargeToAPLine(TransactionLine line)
		{
			if (line.AL_LineType == TransactionLineTypes.Cost || line.AL_LineType == TransactionLineTypes.UnapprovedCost)
			{
				if (line.RelatedJobCharge == null)
				{
					CreateJobCharge(line, line.InvoicingJob, line.ChargeCode, line.TransactionCurrency);
				}
				else
				{
					line.RelatedJobCharge.SetAmountsFromLinkedLinesForTests();
				}
			}
		}

		public void AddTransactionsToComplianceReport(int numberOfInvoices, AccComplianceReport report, ref int transactionCount)
		{
			for (int i = 0; i < numberOfInvoices; i++)
			{
				var invoiceNumber = transactionCount + i;
				var invoice = CreateAPInvoice<APInvoice>($"I{invoiceNumber:0000}", AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, ABIGAS);
				AccountingTestHelper.AssertEquals("Has Lines", invoice.Lines.Count > 0, true);
				var line1 = invoice.Lines[0];
				line1.AL_AT = GST1.PK;
				var line2 = CreateInvoiceLine(invoice, AUD, 1m, 200m);
				line2.AL_AT = GSTFREE1.PK;
				Factory.Save();

				CreateComplianceReportQueueEntry(report, line1, line2);
			}

			transactionCount += numberOfInvoices;
			report.ClearReportLines_ForTestOnly();
		}

		#endregion

		#region Tax Branch

		public DisposableList SetUpTaxBranchRegistry(bool enableTaxBranch) => new DisposableList(new IDisposable[] {
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranch),
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranch)
		});

		#endregion

		#region EInvoicing

		public AccEInvoicingBatch CreateEInvoicingBatch(ZInt batchNumber, ZString batchState, GlbCompany company, bool checkForExist = false)
		{
			if (checkForExist)
			{
				var batch = Factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_BatchNumber, batchNumber)
						.AddToFilter(AccEInvoicingBatchSchema.AIB_GC, company.PK)).FirstOrDefault();
				if (batch != null)
				{
					batch.AIB_Status = batchState;
					batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow;
					batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
					return batch;
				}
			}
			return CreateEInvoicingBatch(batchNumber, batchState, company);
		}

		public AccEInvoicingBatch CreateEInvoicingBatch(ZInt batchNumber, ZString batchState, GlbCompany company, string governmentAllocatedNumber)
		{
			var batch = CreateEInvoicingBatch(batchNumber, batchState, company);
			if (!governmentAllocatedNumber.IsNullOrEmpty())
			{
				batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
			}
			return batch;
		}

		public AccEInvoicingBatch CreateEInvoicingBatch(ZInt batchNumber, ZString batchState, GlbCompany company)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = batchNumber;
			batch.AIB_Status = batchState;
			batch.AIB_GC = company.PK;
			batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			return batch;
		}

		public ZGuid CreateEInvoicingBatch(int batchNumber = 0)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_Status = EInvoicingBatchState.Sent;
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_BatchNumber = batchNumber == 0 ? new Random().Next() : batchNumber;
			batch.AIB_SystemCreateTimeUtc = ZDateTime.Now.ToDateTime();
			return batch.PK;
		}

		public AccEInvoicingBatch CreateEInvoicingBatchForPivot(AccEInvoicingTransactionPivot pivot, ZInt batchNumber, ZString batchState)
		{
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = batchNumber;
			batch.AIB_Status = batchState;
			batch.AIB_GC = pivot.AIP_GC;
			batch.AIB_SystemCreateTimeUtc = ZDateTime.UtcNow;
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			pivot.AIP_AIB = batch.PK;
			return batch;
		}

		public AccEInvoicingTransactionPivot CreateEInvoicingTransactionPivot(AccEInvoicingBatch batch, InvoicingBase invoice, ZString pivotState, string actionType = Core.Constants.EInvoicingPivotActionType.Submit, bool overridePivot = true)
		{
			var pivot = batch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault(x => x.AIP_ParentID == invoice.PK && x.AIP_Status != Constants.EInvoicingPivotState.Discarded && x.AIP_ActionType == actionType);
			if (pivot == null)
			{
				var pivots = Factory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType));
				pivot = pivots.FirstOrDefault(x => x.AIP_Status != Constants.EInvoicingPivotState.Discarded);
				if (pivot != null && overridePivot)
				{
					pivot.AIP_AIB = batch.PK;
					batch.TransactionPivots.Add(pivot);
				}
				else
				{
					pivot = batch.TransactionPivots.AddNew();
					pivot.AIP_ParentID = invoice.PK;
					pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
					pivot.SetCompanyAndCountryCode(batch.Company);
				}
			}
			pivot.AIP_Status = pivotState;
			pivot.AIP_ActionType = actionType;
			return pivot;
		}

		public AccEInvoicingTransactionPivot CreateEInvoicingTransactionPivot(AccEInvoicingBatch batch, AccComplianceDocumentHeader complianceDocument, ZString pivotState)
		{
			var pivot = batch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault(x => x.AIP_ParentID == complianceDocument.PK);
			if (pivot == null)
			{
				pivot = batch.TransactionPivots.AddNew();
				pivot.AIP_ParentID = complianceDocument.PK;
				pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
				pivot.AIP_Status = pivotState;
				pivot.SetCompanyAndCountryCode(batch.Company);
			}

			return pivot;
		}

		public AccEInvoicingTransactionPivot CreateEInvoicingTransactionPivot(AccComplianceDocumentHeader complianceDocument, ZString pivotState)
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = complianceDocument.PK;
			pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = pivotState;
			pivot.SetCompanyAndCountryCode(GlbCompany.CurrentCompany);

			return pivot;
		}

		public AccEInvoicingTransactionPivot CreateEInvoicingTransactionPivot(TransactionHeader header, string actionType = EInvoicingPivotActionType.Submit, string status = EInvoicingPivotState.Queued)
		{
			var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, header.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType))
				?? Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = header.PK;
			pivot.AIP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			pivot.SetCompanyAndCountryCode(GlbCompany.CurrentCompany);
			pivot.AIP_ActionType = actionType;
			pivot.AIP_Status = status;

			return pivot;
		}

		public void UpdateEInvoicingTransactionPivot(ZGuid parentPk, ZString errorStatus, ZString errorDescription, ZGuid batchPK, ZDateTime lastResponseReceived, ZDateTime sentTime, ZBool isNotifiedByEmail, string actionType = EInvoicingPivotActionType.Submit)
		{
			var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, actionType));
			if (pivot != null)
			{
				pivot.AIP_AIB = batchPK;
				pivot.AIP_Status = errorStatus;
				pivot.AIP_ErrorDescription = errorDescription;
				pivot.AIP_LastResponseReceivedUtc = lastResponseReceived;
				pivot.AIP_LastSentTimeUtc = sentTime;
				pivot.AIP_IsNotifiedByEmail = isNotifiedByEmail;
				Factory.Save();
			}
		}

		public AccTransactionHeaderAuthorisationRecord CreateTransactionHeaderAuthorisationRecord(TransactionHeader header)
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(header.Company.GC_RN_NKCountryCode) as IComplianceInfoElectronicInvoicing;
			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = header.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_RecordType = complianceInfo?.GetAccTransactionHeaderAuthorisationRecordType() ?? string.Empty;

			return authRecord;
		}

		public AccTransactionHeaderAuthorisationRecord GetOrCreateTransactionHeaderAuthorisationRecord(TransactionHeader header)
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(header.Company.GC_RN_NKCountryCode) as IComplianceInfoElectronicInvoicing;
			var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, header.PK);
			query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, complianceInfo.GetAccTransactionHeaderAuthorisationRecordType());

			var authRecord = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(query) ?? Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = header.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_RecordType = complianceInfo?.GetAccTransactionHeaderAuthorisationRecordType() ?? string.Empty;

			return authRecord;
		}

		public DisposableList SetUpForTestingEInvoicingTurkey_Receivables(Guid branchGuId, DateTime? date, string complianceRegistryValue = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post)
			=> new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey),
			Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchGuId, GlbDepartment.CurrentDepartment.PK.ToGuid()),
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceRegistryValue)
		});

		public DisposableList SetUpForTestingEInvoicingTurkey_Payables(Guid branchGuId, DateTime? date) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey),
			Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchGuId, GlbDepartment.CurrentDepartment.PK.ToGuid()),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
		});

		public DisposableList SetUpForTestingEInvoicingTurkey_ARAP(Guid branchGuId, DateTime? date, string complianceRegistryValue = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post)
			=> new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey),
			Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchGuId, GlbDepartment.CurrentDepartment.PK.ToGuid()),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceRegistryValue),
			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
		});

		public DisposableList SetUpForTestingEInvoicingEgypt(Guid branchGuId, DateTime? date, string complianceRegistryValue = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt),
			Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchGuId, GlbDepartment.CurrentDepartment.PK.ToGuid()),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
			AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.EInvoicingPivotState.Pending),
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceRegistryValue)
		});

		public DisposableList SetUpForTestingEInvoicingChina(DateTime? date) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true),
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "0001"),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
		});

		public DisposableList SetUpForTestingEInvoicingJordan(bool isEnableEInvoicingFunctionality, DateTime? date) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Jordan),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality),
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, date),
		});

		public DisposableList SetUpForTestingEInvoicing(string country, bool isEnableEInvoicingFunctionality) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(country),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality),
		});

		public DisposableList SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(string country, bool isEnableEInvoicingFunctionality) => new DisposableList(new IDisposable[] {
			GlbCompany.CurrentCompany.TemporarilySetCountry(country),
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableEInvoicingFunctionality),
		});

		#endregion

		#region Adjustment Notes

		public T CreateAdjustmentNote<T>(string transactionNumber, decimal invoiceAmount, decimal taxAmount, ZDateTime postDate, ZGuid organisationPK) where T : AdjustmentNote
		{
			T adjustmentNote = Factory.New<T>();

			adjustmentNote.AH_TransactionNum = transactionNumber;
			adjustmentNote.AH_OH = organisationPK;
			adjustmentNote.AH_InvoiceAmount = invoiceAmount;
			adjustmentNote.AH_LocalExTaxAmount = invoiceAmount;
			adjustmentNote.AH_GSTAmount = taxAmount;
			adjustmentNote.AH_LocalTaxAmount = taxAmount;
			adjustmentNote.AH_PostDate = postDate;
			adjustmentNote.AH_DueDate = postDate;

			return adjustmentNote;
		}

		public InvoicingLineBase CreateAdjusmentNoteLine(AdjustmentNote adjustmentNote, ZGuid chargeCodePK, decimal amount, decimal taxAmount)
		{
			InvoicingLineBase line = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			line.AL_AC = chargeCodePK;
			line.AL_LineAmount = amount;
			line.AL_GSTVAT = taxAmount;
			line.AL_OSExTaxAmount = amount;
			line.AL_OSTaxAmount = taxAmount;
			line.AL_AG = GLHeader1.PK;
			return line;
		}

		public InvoicingLineBase CreateAdjusmentNoteLine(AdjustmentNote adjustmentNote, ZGuid chargeCodePK, decimal amount, decimal taxAmount, AccTaxRate taxID)
		{
			InvoicingLineBase line = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			line.AL_AC = chargeCodePK;
			line.AL_LineAmount = amount;
			line.AL_GSTVAT = taxAmount;
			line.AL_AT = taxID.PK;
			line.AL_OSExTaxAmount = amount;
			line.AL_OSTaxAmount = taxAmount;
			line.AL_AG = GLHeader1.PK;
			return line;
		}

		#endregion

		#region Currency

		#region Local currency

		public RefCurrency GetCurrency(string currencyCode)
		{
			return RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
		}

		public RefCurrency LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		#endregion

		#region USD

		RefCurrency fUSD;
		public RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region CurrencyWithoutCents

		RefCurrency fCurrencyWithoutCents;
		public RefCurrency CurrencyWithoutCents
		{
			get
			{
				if (fCurrencyWithoutCents == null)
				{
					fCurrencyWithoutCents = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
				}
				return fCurrencyWithoutCents;
			}
		}

		#endregion

		#region CNY

		RefCurrency fCNY;
		public RefCurrency CNY
		{
			get
			{
				if (fCNY == null)
				{
					fCNY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
				}
				return fCNY;
			}
		}

		#endregion

		#region TWD

		RefCurrency fTWD;
		public RefCurrency TWD
		{
			get
			{
				if (fTWD == null)
				{
					fTWD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "TWD");
				}
				return fTWD;
			}
		}
		#endregion

		#region IDR

		RefCurrency fIDR;
		public RefCurrency IDR
		{
			get
			{
				if (fIDR == null)
				{
					fIDR = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "IDR");
				}
				return fIDR;
			}
		}
		#endregion

		#region GBP

		RefCurrency fGBP;
		public RefCurrency GBP
		{
			get
			{
				if (fGBP == null)
				{
					fGBP = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GBP");
				}
				return fGBP;
			}
		}

		#endregion

		#region VND

		RefCurrency fVND;
		public RefCurrency VND
		{
			get
			{
				if (fVND == null)
				{
					fVND = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "VND");
				}
				return fVND;
			}
		}
		#endregion

		#endregion

		#region Organisations

		public OrgHeader GetOrganisation()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, "ABIGAS");
			BusinessObject[] objects = Factory.Load(typeof(OrgHeader), filter);
			return (OrgHeader)objects[0];
		}

		public OrgHeader CreateOrgHeaderDE(string code, bool isCreditor, bool isDebtor, bool isAPGSTApplicable, bool isAPWHTApplicable, bool isARGSTApplicable, bool isARWHTApplicable, bool isOrgProxyForAnotherCompany = false)
		{
			var orgHeader = CreateOrgHeader(code, isCreditor, isDebtor, isAPGSTApplicable, isAPWHTApplicable, isARGSTApplicable, isARWHTApplicable, isOrgProxyForAnotherCompany);
			//orgHeader.OH_RL_NKClosestPort = "DEHAM";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			orgHeader.OH_Category = OrgConstants.Category.Government;
			return orgHeader;
		}

		public OrgHeader CreateOrgHeader(string code, bool isCreditor, bool isDebtor, bool isAPGSTApplicable, bool isAPWHTApplicable, bool isARGSTApplicable, bool isARWHTApplicable, bool isOrgProxyForAnotherCompany = false)
		{
			BusinessObjectFactory newFactory = GetNewFactory();

			OrgHeader header = newFactory.New<OrgHeader>();
			header.OH_Code = "Z" + code;
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";

			header.OH_IsDebtor = isDebtor;
			header.OH_IsCreditor = isCreditor;

			header.CompanyData.SetARTaxApplicable(isARGSTApplicable);
			header.MiscServ.OM_ARWHTApplicable = isARWHTApplicable;

			header.CompanyData.SetAPTaxApplicable(isAPGSTApplicable);
			header.MiscServ.OM_APWHTApplicable = isAPWHTApplicable;

			if (isOrgProxyForAnotherCompany)
			{
				NonCurrentCompanyBranch.GB_OH_OrgProxy = header.PK;
			}

			newFactory.Save();

			return Factory.Load<OrgHeader>(header.PK);
		}

		public OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor, string closestPort = null, bool enableDataRefreshBus = true)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = enableDataRefreshBus };

			var header = newFactory.New<OrgHeader>();
			header.OH_Code = "Z" + code;
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			if (!string.IsNullOrEmpty(closestPort))
			{
				header.OH_RL_NKClosestPort = closestPort;
			}
			header.OH_IsDebtor = debtor;
			header.OH_IsCreditor = creditor;
			if (debtor)
			{
				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;
			}
			if (creditor)
			{
				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;
			}

			newFactory.Save();

			return Factory.Load<OrgHeader>(header.PK);
		}

		public OrgRelatedParty AddOrgRelatedParty(OrgHeader parentOrg, string partyType, OrgHeader relatedOrg, ZGuid companyPK)
		{
			var orgRelatedParty = parentOrg.AllRelatedParties.AddNew();
			orgRelatedParty.PR_FreightDirection = "AAA";
			orgRelatedParty.PR_FreightTransportMode = "AAA";
			orgRelatedParty.PR_FreightContainerMode = "AAA";
			orgRelatedParty.PR_GC = companyPK;
			orgRelatedParty.PR_OH_RelatedParty = relatedOrg.PK;
			orgRelatedParty.PR_PartyType = partyType;
			orgRelatedParty.PR_Service = "AAA";
			orgRelatedParty.PR_Location = "AAAAA";

			return orgRelatedParty;
		}

		OrgHeader CreateOrgHeaderWithGlobalCreditLimit(string code, string currency, decimal limit, string invoiceTerm, bool isGlobalCreditGroupChild)
		{
			var globalCreditLimitOrgParent = CreateOrgHeader(code, true, true);
			globalCreditLimitOrgParent.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = invoiceTerm;
			globalCreditLimitOrgParent.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			globalCreditLimitOrgParent.MiscServ.OM_ARGlobalCreditApproved = true;
			globalCreditLimitOrgParent.MiscServ.OM_RX_NKARGlobalCreditCurrency = currency;
			globalCreditLimitOrgParent.MiscServ.OM_ARGlobalCreditLimit = limit;

			var globalCreditLimitOrgChild = CreateOrgHeader(code + "C", true, true);
			globalCreditLimitOrgChild.MiscServ.OM_OH_ARGlobalCreditGroup = globalCreditLimitOrgParent.PK;
			globalCreditLimitOrgChild.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = invoiceTerm;
			globalCreditLimitOrgChild.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			globalCreditLimitOrgChild.MiscServ.OM_ARGlobalCreditApproved = true;
			globalCreditLimitOrgChild.MiscServ.OM_RX_NKARGlobalCreditCurrency = currency;

			Factory.Save();

			return isGlobalCreditGroupChild ? globalCreditLimitOrgChild : globalCreditLimitOrgParent;
		}

		public OrgHeader CreateOrgHeaderWithGlobalCreditLimitParent(string code, string currency, decimal limit, string invoiceTerm = "INV")
		{
			return CreateOrgHeaderWithGlobalCreditLimit(code, currency, limit, invoiceTerm, false);
		}

		public OrgHeader CreateOrgHeaderWithGlobalCreditLimitChild(string code, string currency, decimal limit, string invoiceTerm = "INV")
		{
			return CreateOrgHeaderWithGlobalCreditLimit(code, currency, limit, invoiceTerm, true);
		}

		public void SetCustomsCodeForOrgHeader(OrgHeader header, string codeType, string countryCode, string customsCode)
		{
			header.SetCustomsCode(codeType, RefCountry.LoadFromCountryCode(Factory, countryCode), customsCode);
		}

		public OrgDebtorGroup CreateDebtorGroup()
		{
			return Factory.NewWithValidTestData<OrgDebtorGroup>();
		}

		public OrgCreditorGroup CreateCreditorGroup()
		{
			return Factory.NewWithValidTestData<OrgCreditorGroup>();
		}

		public OrgAddress CreateAddress(OrgHeader header, string streetAddress = "111 Bourke Road")
		{
			BusinessObjectFactory newFactory = GetNewFactory();
			var headerInNewFactory = newFactory.Load<OrgHeader>(header.PK);
			var address = headerInNewFactory.Addresses.AddNew();
			address.OA_Address1 = streetAddress;
			address.OA_City = headerInNewFactory.MainAddress.OA_City;
			address.OA_State = headerInNewFactory.MainAddress.OA_State;
			newFactory.Save();

			return Factory.Load<OrgAddress>(address.PK);
		}

		public OrgAddress CreateAddress(OrgHeader orgHeader, string country, string language, string address, string companyName, string capability, string mainAddress)
		{
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = country;
			orgAddress.OA_Language = language;
			orgAddress.OA_Address1 = address;
			orgAddress.OA_CompanyNameOverride = companyName;
			orgAddress.AddressCapability.SetCapabilityEnabled(capability);
			orgAddress.AddressCapability.SetIsMainAddress(mainAddress);
			return orgAddress;
		}

		public OrgAddress CreateAddress(OrgHeader org, OrgAddressType addressType, bool isMain)
		{
			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType.Code);
			if (isMain)
			{
				address.AddressCapability.SetIsMainAddress(addressType.Code);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(addressType.Code);
			}
			address.OA_Language = "";
			address.OA_Address1 = addressType.ToString() + (isMain ? " Main " : " Secondary ") + address.OA_Language;
			return address;
		}

		public OrgAddress CreateAddress(OrgHeader header, OrgAddressType addressType, bool isMain, string streetAddress1, string streetAddress2, string city, string stateCode, string countryCode, string postCode, string phone, string email)
		{
			var address = header.Addresses.AddNew();
			PopulateOrgAddress(address, addressType, isMain, streetAddress1, streetAddress2, city, stateCode, countryCode, postCode, phone, email);
			return address;
		}

		public void PopulateOrgAddress(OrgAddress address, OrgAddressType addressType, bool isMain, string streetAddress1, string streetAddress2, string city, string stateCode, string countryCode, string postCode, string phone, string email)
		{
			if (isMain)
			{
				address.AddressCapability.SetIsMainAddress(addressType.Code);
			}
			else
			{
				address.AddressCapability.SetIsNotMainAddress(addressType.Code);
			}

			address.OA_Address1 = streetAddress1;
			address.OA_Address2 = streetAddress2;
			address.OA_City = city;
			address.OA_State = stateCode;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_PostCode = postCode;
			address.OA_Phone = phone;
			address.OA_Email = email;
		}

		public OrgTranslatedAddress CreateTranslatedAddress(OrgAddress parent, ZString language, ZString companyName, ZString address)
		{
			var translatedAddress = parent.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = language;
			translatedAddress.OTA_CompanyName = companyName;
			translatedAddress.OTA_Address1 = address;
			return translatedAddress;
		}

		public OrgContact CreateContact(OrgHeader header, string name = "John", string email = null)
		{
			BusinessObjectFactory newFactory = GetNewFactory();
			var headerInNewFactory = newFactory.Load<OrgHeader>(header.PK);
			var contact = headerInNewFactory.Contacts.AddNew();
			contact.OC_ContactName = name;
			if (!string.IsNullOrEmpty(email))
			{
				contact.OC_Email = email;
			}
			newFactory.Save();

			return Factory.Load<OrgContact>(contact.PK);
		}

		public OrgInvoiceType CreateOrgInvoiceType(OrgCompanyData companyData, ZString jobTypeCode, ZString transportMode, ZString serviceDirection, ZString serviceLevel, ZString type, ZString secondaryType)
		{
			var invoiceType = companyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = jobTypeCode;
			invoiceType.PI_ServiceDirection = serviceDirection;
			invoiceType.PI_TransportMode = transportMode;
			invoiceType.PI_RS_NKServiceLevel = serviceLevel;
			invoiceType.PI_Type = type;
			invoiceType.PI_SecondaryType = secondaryType;
			return invoiceType;
		}

		public OrgCusCode CreateCustomsCodesIfNotExists(OrgHeader header, ZString codeCountry, ZString codeType, ZString customsRegNo)
		{
			if (header != null)
			{
				var existingCustomCode = header.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == codeCountry && x.OK_CodeType == codeType && x.OK_CustomsRegNo == customsRegNo);
				return existingCustomCode ?? CreateCustomsCodes(header, codeCountry, codeType, customsRegNo);
			}
			return null;
		}

		public OrgCusCode CreateCustomsCodes(OrgHeader header, ZString codeCountry, ZString codeType, ZString customsRegNo)
		{
			if (header != null)
			{
				var customsCode = header.CustomsCodes.AddNew();
				customsCode.OK_RN_NKCodeCountry = codeCountry;
				customsCode.OK_CodeType = codeType;
				customsCode.OK_CustomsRegNo = customsRegNo;
				return customsCode;
			}
			return null;
		}

		#endregion

		#region Job headers

		public Job CreateJobHeader()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_JobNum = GetRandomString(20);
			return jobHeader;
		}

		public JobHeader JobHeader1
		{
			get
			{
				if (fJobHeader1 == null)
				{
					fJobHeader1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				}
				return fJobHeader1;
			}
		}
		JobHeader fJobHeader1;

		public JobChargeRevRecognition CreateJobChargeRevRecognition(Job job, ZString recognitionType, ZDateTime recognitionDate)
		{
			var checkExistingJobChargeRevRecognitionQuery = new ZQuery(JobChargeRevRecognitionSchema.D3_JH, job.PK);
			checkExistingJobChargeRevRecognitionQuery.AddToFilter(JobChargeRevRecognitionSchema.D3_RecognitionType, recognitionType);
			var existingJobChargeRevRecognition = Factory.LoadTop1<JobChargeRevRecognition>(checkExistingJobChargeRevRecognitionQuery);

			if (existingJobChargeRevRecognition != null)
			{
				return existingJobChargeRevRecognition;
			}
			else
			{
				JobChargeRevRecognition revenueRecognition = Factory.New<JobChargeRevRecognition>();
				revenueRecognition.D3_JH = job.PK;
				revenueRecognition.D3_RecognitionType = recognitionType;
				if (revenueRecognition.D3_RecognitionType == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate)
				{
					revenueRecognition.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.Immediate;
				}
				else
				{
					revenueRecognition.D3_RecognitionDate = recognitionDate;
				}
				return revenueRecognition;
			}
		}

		public void CreateJobChargeRevRecognitionForLines(InvoicingLineBaseCollection lines, ZString recognitionType, ZDateTime recognitionDate)
		{
			foreach (InvoicingLineBase line in lines)
			{
				line.AL_RevRecognitionType = recognitionType;
				CreateJobChargeRevRecognition(line.InvoicingJob, recognitionType, recognitionDate);
			}
		}

		#endregion

		#region Job charges

		public Charge CreateChargeWithTarget(TransactionLine transactionLine, IJobHeaderParent relatedJob, IJobHeaderParent targetJob)
		{
			var job = (Job)transactionLine.Job;
			var chargeCode = transactionLine.ChargeCode;
			var currency = transactionLine.TransactionCurrency;

			var charge = job.Charges.AddNew();

			var currencyCode = currency != null ? currency.RX_Code : transactionLine.AL_RX_NKTransactionCurrency;
			charge.JR_RX_NKCostCurrency = currencyCode;
			charge.JR_RX_NKSellCurrency = currencyCode;
			charge.JR_AC = chargeCode != null ? chargeCode.PK : transactionLine.AL_AC;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;

			if (relatedJob != null)
			{
				charge.JR_Calc_RelatedJobNumber = relatedJob.JobNumber;
			}

			charge.JR_OH_SellAccount = transactionLine.TransactionHeader.AH_OH;

			if (targetJob != null)
			{
				charge.JR_Calc_InvoiceTarget = targetJob.JobNumber;
			}

			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_A9_SellVATClass = transactionLine.AL_A9_VATClass;
				charge.JR_AT_SellGSTRate = transactionLine.AL_AT;
			}
			else
			{
				charge.JR_A9_CostVATClass = transactionLine.AL_A9_VATClass;
				charge.JR_AT_CostGSTRate = transactionLine.AL_AT;
			}

			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = transactionLine.PK;
			}
			else
			{
				charge.JR_AL_APLine = transactionLine.PK;
			}

			charge.SetAmountsFromLinkedLinesForTests();

			return charge;
		}

		public Charge CreateCharge(Job job, AccChargeCode chargeCode, APInvoiceLine lineAP, ARInvoiceLine lineAR)
		{
			var charge = job.Charges.AddNew();
			charge.JR_JH = job.PK;

			if (lineAR != null)
			{
				charge.JR_AL_ARLine = lineAR.PK;
				charge.JR_RX_NKSellCurrency = lineAR.AL_RX_NKTransactionCurrency;
				charge.JR_A9_SellVATClass = lineAR.AL_A9_VATClass;
				charge.JR_AT_SellGSTRate = lineAR.AL_AT;
			}

			if (lineAP != null)
			{
				charge.JR_AL_APLine = lineAP.PK;
				charge.JR_RX_NKCostCurrency = lineAP.AL_RX_NKTransactionCurrency;
				charge.JR_A9_CostVATClass = lineAP.AL_A9_VATClass;
				charge.JR_AT_CostGSTRate = lineAP.AL_AT;
			}

			charge.JR_AC = chargeCode.PK;
			charge.SetAmountsFromLinkedLinesForTests();

			return charge;
		}

		public Charge CreateCharge(TransactionLine transactionLine)
		{
			return CreateCharge(transactionLine, transactionLine.InvoicingJob, transactionLine.ChargeCode, transactionLine.TransactionCurrency);
		}

		public Charge CreateCharge(TransactionLine transactionLine, Job job, AccChargeCode chargeCode = null, RefCurrency currency = null, string apInvoiceNumber = "", OrgHeader costAccount = null, OrgHeader sellAccount = null)
		{
			var charge = job.Charges.AddNew();

			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = transactionLine.PK;
			}
			else
			{
				charge.JR_AL_APLine = transactionLine.PK;
			}

			var currencyCode = currency != null ? currency.RX_Code : transactionLine.AL_RX_NKTransactionCurrency;
			charge.JR_RX_NKCostCurrency = currencyCode;
			charge.JR_RX_NKSellCurrency = currencyCode;
			charge.JR_AC = chargeCode != null ? chargeCode.PK : transactionLine.AL_AC;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = job.PK;

			if (!string.IsNullOrEmpty(apInvoiceNumber))
			{
				charge.JR_APInvoiceNum = apInvoiceNumber;
			}

			if (costAccount != null)
			{
				charge.JR_OH_CostAccount = costAccount.PK;
			}

			if (sellAccount != null)
			{
				charge.JR_OH_SellAccount = sellAccount.PK;
			}

			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_SellSupplyType = transactionLine.AL_SupplyType;
				charge.JR_A9_SellVATClass = transactionLine.AL_A9_VATClass;
				charge.JR_AT_SellGSTRate = transactionLine.AL_AT;
			}
			else
			{
				charge.JR_CostSupplyType = transactionLine.AL_SupplyType;
				charge.JR_A9_CostVATClass = transactionLine.AL_A9_VATClass;
				charge.JR_AT_CostGSTRate = transactionLine.AL_AT;
			}

			charge.SetAmountsFromLinkedLinesForTests();

			return charge;
		}

		public Charge CreateMarginCharge(Job job, RefCurrency costCurrency, RefCurrency sellCurrency, ZDecimal foreignCostAmount)
		{
			return CreateCharge(job, costCurrency, sellCurrency, foreignCostAmount, GlbBranch.CurrentBranch, FEADepartment, CC1, Constants.ChargeType.Margin);
		}

		public Charge CreateDSBCharge(Job job, RefCurrency costCurrency, RefCurrency sellCurrency, ZDecimal foreignCostAmount)
		{
			return CreateDSBCharge(job, costCurrency, sellCurrency, foreignCostAmount, GlbBranch.CurrentBranch, FEADepartment, DSBChargeCode);
		}

		public Charge CreateDSBCharge(Job job, RefCurrency costCurrency, RefCurrency sellCurrency, ZDecimal foreignCostAmount, GlbBranch branch, GlbDepartment department, AccChargeCode code)
		{
			return CreateCharge(job, costCurrency, sellCurrency, foreignCostAmount, branch, department, code, Constants.ChargeType.Disbursement);
		}

		public Charge CreateCharge(Job job, RefCurrency costCurrency, RefCurrency sellCurrency, ZDecimal foreignCostAmount, GlbBranch branch, GlbDepartment department, AccChargeCode code, ZString type)
		{
			var charge = job.Charges.AddNew();
			charge.JR_ChargeType = type;
			charge.JR_AC = code.PK;
			charge.JR_GB = branch.PK;
			charge.JR_GE = department.PK;
			charge.JR_RX_NKCostCurrency = costCurrency.Code;
			charge.JR_OSCostAmt = foreignCostAmount;
			charge.JR_OH_SellAccount = Debtor.PK;
			charge.JR_OH_CostAccount = Creditor1.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.Code;
			return charge;
		}

		public JobCharge CreateJobCharge(TransactionLine transactionLine, JobHeader jobHeader, AccChargeCode chargeCode, RefCurrency currency = null, RefCurrency sellInvoiceCurrency = null)
		{
			var jobCharge = Factory.New<JobCharge>();

			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_JH = jobHeader.PK;

			var currencyCode = currency != null ? currency.RX_Code : transactionLine.AL_RX_NKTransactionCurrency;
			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				jobCharge.JR_RX_NKSellCurrency = currencyCode;
				if (sellInvoiceCurrency != null)
				{
					jobCharge.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrency.RX_Code;
				}
				jobCharge.JR_AL_ARLine = transactionLine.PK;
			}
			else
			{
				jobCharge.JR_RX_NKCostCurrency = currencyCode;
				jobCharge.JR_AL_APLine = transactionLine.PK;
			}

			if (transactionLine.AL_LineType == TransactionLineTypes.Revenue || transactionLine.AL_LineType == TransactionLineTypes.WIP)
			{
				jobCharge.JR_SellSupplyType = transactionLine.AL_SupplyType;
				jobCharge.JR_A9_SellVATClass = transactionLine.AL_A9_VATClass;
				jobCharge.JR_AT_SellGSTRate = transactionLine.AL_AT;
			}
			else
			{
				jobCharge.JR_CostSupplyType = transactionLine.AL_SupplyType;
				jobCharge.JR_A9_CostVATClass = transactionLine.AL_A9_VATClass;
				jobCharge.JR_AT_CostGSTRate = transactionLine.AL_AT;
			}

			jobCharge.SetAmountsFromLinkedLinesForTests();

			return jobCharge;
		}

		public void PostCost(JobCharge charge, Guid invoicePK, BusinessObjectFactory factory = null)
		{
			var factoryToUse = factory ?? Factory;

			var invoice = invoicePK == Guid.Empty ? factoryToUse.NewWithValidTestData<AccTransactionHeader>() : factoryToUse.NewWithPrimaryKey<AccTransactionHeader>(invoicePK);
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_InvoiceDate = ZDateTime.Now;

			charge.APLine.AL_OSAmount = (-1) * charge.JR_OSCostAmt;
			charge.APLine.AL_LineAmount = charge.JR_LocalSellAmt = (-1) * charge.JR_LocalCostAmt;
			charge.APLine.AL_AH = invoice.PK;
			charge.APLine.AL_LineType = TransactionLineTypes.Cost;
			charge.APLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_InvoiceAmount = charge.APLine.AL_LineAmount;
			invoice.AH_GSTAmount = charge.APLine.AL_GSTVAT;
			invoice.AH_OutstandingAmount = invoice.AH_LocalTotal;
			invoice.AH_OSTotal = invoice.AH_LocalTotal;
			invoice.AH_IsOSOutstandingAmountApplicable = AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value;
			invoice.AH_OSOutstandingAmount = invoice.AH_IsOSOutstandingAmountApplicable ? invoice.AH_LocalTotal : 0;

			charge.JR_APInvoiceNum = "111";
			charge.JR_APInvoiceDate = new ZDateTime(2010, 01, 05);
			charge.JR_PaymentDate = new ZDateTime(2010, 01, 10);
			charge.JR_OH_CostAccount = factoryToUse.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_CostReference = "ABC";
		}

		public Charge CreateChargeWithPaymentBasis(Job job, AccChargeCode code, OrgHeader debtor, decimal sellAmount)
		{
			var charge = CreateCharge(job, code, job.JH_JobNum, USD, 0m, null, USD, sellAmount, debtor);
			charge.RevenueCalculationDescription = ZBlob.FromAscii("Lets pretend this charge was actually autorated");
			charge.AddPaymentBases(new[] { CreatePaymentBasis(sellAmount, job.JH_JobNum) }, false);

			return charge;
		}

		public PaymentBasis CreatePaymentBasis(ZDecimal amount, ZString operationalJobCode)
		{
			var quantity = new Quantity(1, QuantityUnit.HB);
			var rateInfo = RateInfo.CreateFLT(amount, Constants.CurrencyCodes.UnitedStates);
			var result = new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, operationalJobCode);

			return result;
		}

		#endregion

		#region Charge codes

		public AccChargeCode InsertMarginChargeCode(int marginPercentage)
		{
			AccChargeCode code = InsertChargeCode(Core.Constants.ChargeType.Margin);
			code.AC_MarginPercentage = marginPercentage;

			return code;
		}

		public AccChargeCode InsertChargeCode(ZString chargeType)
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = GetRandomString(10);
			chargeCode.AC_Desc = GetRandomString(80);
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.SetGLAccountDataForTesting(GLHeader1);

			return chargeCode;
		}

		public AccChargeCode CreateGlobalChargeCode(string code)
		{
			FixSingaporeCompanyData();
			AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = code;
			globalChargeCode.AC_Desc = code + " Global Charge";
			globalChargeCode.AC_ChargeType = ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_RateCalculator = FlatCalculator.Code;
			SetTemporaryDepartmentValueOnChargeCode(globalChargeCode);

			return globalChargeCode;
		}

		void FixSingaporeCompanyData()
		{
			var singaporeCompany = SingaporeCompany;

			if (singaporeCompany.GC_IsGSTRegistered)
			{
				singaporeCompany.GC_IsGSTRegistered = false;
				Factory.Save();
			}
		}

		GlbCompany SingaporeCompany
		{
			get { return Factory.GetCachedValue("SingaporeCompany", () => Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"))); }
		}

		void SetTemporaryDepartmentValueOnChargeCode(AccChargeCode accChargeCode)
		{
			accChargeCode.AC_DepartmentFilterList = "ALL";
		}

		public AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT, string departmentFilter, GlbCompany company = null, string goodsServiceType = GoodServiceTypes.Codes.SRV, bool createWithoutZZ = false)
		{
			var finalCode = createWithoutZZ ? code : "ZZ" + code;
			var companyPK = (company ?? GlbCompany.CurrentCompany).PK;

			var checkExistingChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, companyPK);
			checkExistingChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, finalCode);
			var existingChargeCode = Factory.LoadTop1<AccChargeCode>(checkExistingChargeCodeQuery);

			if (existingChargeCode == null)
			{
				AccChargeCode chargeCode = GetNewFactory().New<AccChargeCode>();
				chargeCode.AC_Code = finalCode;
				chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
				chargeCode.AC_Desc = description;
				chargeCode.AC_ChargeType = chargeType;
				chargeCode.AC_GoodsServiceType = goodsServiceType;
				chargeCode.AC_MarginPercentage = marginPercentage;
				chargeCode.AC_GC = companyPK;
				chargeCode.AC_AT_GSTRate = gST == null ? ZGuid.Empty : gST.PK;
				chargeCode.AC_AW_WithholdingTaxRate = wHT == null ? ZGuid.Empty : wHT.PK;
				chargeCode.AC_DepartmentFilterList = departmentFilter;
				chargeCode.AC_IsActive = true;
				chargeCode.SetGLAccountDataForTesting(GLHeader1);
				chargeCode.Factory.Save();

				return Factory.Load<AccChargeCode>(chargeCode.PK);
			}

			return existingChargeCode;
		}

		public AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT, GlbCompany company = null, string goodsServiceType = GoodServiceTypes.Codes.SRV, bool createWithoutZZ = false)
		{
			return CreateChargeCode(code, description, chargeType, marginPercentage, gST, wHT, "ALL", company: company, goodsServiceType: goodsServiceType, createWithoutZZ: createWithoutZZ);
		}

		public AccChargeCode CreateChargeCode(string code, bool createWithoutZZ = false)
		{
			var gst = CreateTaxRate("GST", "GSTRate", 10);
			var chargeCode = CreateChargeCode(code, "", Constants.ChargeType.Margin, 100, gst, null, "ALL", createWithoutZZ: createWithoutZZ);
			return chargeCode;
		}

		public AccChargeTaxOverride CreateTaxOverride(AccChargeCode chargeCode,
			ZGuid taxCodePK = default(ZGuid),
			ZGuid vatClassPK = default(ZGuid),
			string direction = "ALL",
			string costSellAll = "ALL",
			string incoTerm = "ALL",
			string jobType = "ALL",
			string origin = "ALL",
			string destination = "ALL",
			string transactionContext = "STD",
			string defaultingRule = "NON",
			string homeCountryOrZone = "")
		{
			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_ParentID = chargeCode.PK;
			taxOverride.AO_AT = taxCodePK;
			taxOverride.AO_A9_DefaultVATClass = vatClassPK;
			taxOverride.AO_Direction = direction;
			taxOverride.AO_CostSellAll = costSellAll;
			taxOverride.AO_IncoTerm = incoTerm;
			taxOverride.AO_JobType = jobType;
			taxOverride.AO_Origin = origin;
			taxOverride.AO_Destination = destination;
			taxOverride.AO_TransactionContext = transactionContext;
			taxOverride.AO_DefaultingRule = defaultingRule;
			taxOverride.AO_HomeCountryOrZone = homeCountryOrZone;
			return taxOverride;
		}

		public AccChargeTaxOverride CreateTaxOverride(AccTaxOverrideGroup group,
			ZGuid taxCodePK = default(ZGuid),
			ZGuid vatClassPK = default(ZGuid),
			string direction = "ALL",
			string costSellAll = "ALL",
			string incoTerm = "ALL",
			string jobType = "ALL",
			string origin = "ALL",
			string destination = "ALL",
			string transactionContext = "STD",
			string defaultingRule = "NON",
			string homeCountryOrZone = "")
		{
			var taxOverride = group.TaxOverrides.AddNew();
			taxOverride.AO_ParentID = group.PK;
			taxOverride.AO_AT = taxCodePK;
			taxOverride.AO_A9_DefaultVATClass = vatClassPK;
			taxOverride.AO_Direction = direction;
			taxOverride.AO_CostSellAll = costSellAll;
			taxOverride.AO_IncoTerm = incoTerm;
			taxOverride.AO_JobType = jobType;
			taxOverride.AO_Origin = origin;
			taxOverride.AO_Destination = destination;
			taxOverride.AO_TransactionContext = transactionContext;
			taxOverride.AO_DefaultingRule = defaultingRule;
			taxOverride.AO_HomeCountryOrZone = homeCountryOrZone;
			return taxOverride;
		}

		public void CreateTaxOverrides(AccChargeCode chargeCode, params Action<AccChargeTaxOverride>[] overrideSettings)
		{
			foreach (var overrideSetting in overrideSettings)
			{
				overrideSetting(CreateTaxOverride(chargeCode));
			}
		}

		public AccChargeRevRecOverride CreateRevenueRecognitionOverride(AccChargeCode chargeCode,
			string jobType = "ALL",
			string direction = "",
			string mode = "",
			string brokerType = "",
			string recognitionType = "IMM")
		{
			var revenueRecognize = chargeCode.RevenueRecOverrides.AddNew();
			revenueRecognize.AE_JobType = jobType;
			revenueRecognize.AE_Direction = direction;
			revenueRecognize.AE_Mode = mode;
			revenueRecognize.AE_BrokerType = brokerType;
			revenueRecognize.AE_RecognitionType = recognitionType;
			return revenueRecognize;
		}

		public AccChargeCreditorOverride CreateChargeCreditorOverride(AccChargeCode chargeCode,
			string jobType = "ALL",
			string direction = "",
			string transportMode = "",
			string paymentTerm = "ALL",
			ZGuid department = default(ZGuid),
			string defaultingRule = "SCA",
			ZGuid creditor = default(ZGuid),
			string creditorRole = ""
			)
		{
			var accChargeCreditorOverride = chargeCode.CreditorOverrides.AddNew();
			accChargeCreditorOverride.ACC_JobType = jobType;
			accChargeCreditorOverride.ACC_DefaultingRule = defaultingRule;
			accChargeCreditorOverride.ACC_Direction = direction;
			accChargeCreditorOverride.ACC_GE_Department = department;
			accChargeCreditorOverride.ACC_PaymentTerm = paymentTerm;
			accChargeCreditorOverride.ACC_TransportMode = transportMode;
			accChargeCreditorOverride.ACC_OH_Creditor = creditor;
			accChargeCreditorOverride.ACC_CreditorRole = creditorRole;
			return accChargeCreditorOverride;
		}

		public AccChargeCode GetChargeCode(string chargeType)
		{
			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, chargeType);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode charge = Factory.LoadTop1<AccChargeCode>(filter);

			if (charge == null)
			{
				return InsertChargeCode(chargeType);
			}

			return charge;
		}

		public ZGuid GetChargeCodeByCode(ZString chargeCode)
		{
			ZQuery chargeCodeFilter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
			chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			ZGuid chargeCodePK = Factory.LoadTop1(typeof(AccChargeCode), chargeCodeFilter).PK;
			return chargeCodePK;
		}

		public AccTransactionLines CreateRevenueLineAndCharge(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			return CreateChargeAndPostSellSideOnly(jobPK, lineAmount, chargeCode).Item1;
		}

		public (AccTransactionLines, Charge) CreateChargeAndPostSellSideOnly(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			ZGuid chargeCodePK = GetChargeCodeByCode(chargeCode);
			AccTransactionHeader arHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			arHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			arHeader.AH_TransactionType = TransactionTypes.Invoice;
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_JH = jobPK;
			line.AL_LineAmount = line.AL_OSAmount = lineAmount;
			line.AL_AH = arHeader.PK;
			line.AL_AC = chargeCodePK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_GE = Env.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = jobPK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalSellAmt = lineAmount;
			charge.JR_LocalCostAmt = 0M;
			return (line, charge);
		}

		public AccTransactionLines CreateWIPLineAndCharge(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			return CreateChargeAndAssociatedWIPs(jobPK, lineAmount, chargeCode).Item1;
		}

		public (AccTransactionLines, Charge) CreateChargeAndAssociatedWIPs(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			ZGuid chargeCodePK = GetChargeCodeByCode(chargeCode);
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line.AL_JH = jobPK;
			line.AL_LineAmount = line.AL_OSAmount = lineAmount * -1;
			line.AL_AC = chargeCodePK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_GE = Env.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = jobPK;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalSellAmt = lineAmount;
			charge.JR_LocalCostAmt = 0M;
			charge.JR_OH_SellAccount = line.AL_OH = ZGuid.Empty;
			return (line, charge);
		}

		public AccTransactionLines CreateCostLineAndCharge(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			return CreateChargeAndPostCostSideOnly(jobPK, lineAmount, chargeCode).Item1;
		}

		public (AccTransactionLines, Charge) CreateChargeAndPostCostSideOnly(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			ZGuid chargeCodePK = GetChargeCodeByCode(chargeCode);
			AccTransactionHeader apHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			apHeader.AH_Ledger = LedgerTypes.AccountsPayable;
			apHeader.AH_TransactionType = TransactionTypes.Invoice;
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = apHeader.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_LineAmount = line.AL_OSAmount = lineAmount * -1;
			line.AL_JH = jobPK;
			line.AL_AC = chargeCodePK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_GE = Env.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = jobPK;
			charge.JR_AL_APLine = line.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalCostAmt = lineAmount;
			charge.JR_LocalSellAmt = 0M;
			return (line, charge);
		}

		public AccTransactionLines CreateAccrualLineAndCharge(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			return CreateChargeAndAssociatedAccruals(jobPK, lineAmount, chargeCode).Item1;
		}

		public (AccTransactionLines, Charge) CreateChargeAndAssociatedAccruals(ZGuid jobPK, ZDecimal lineAmount, ZString chargeCode)
		{
			ZGuid chargeCodePK = GetChargeCodeByCode(chargeCode);
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			line.AL_LineAmount = line.AL_OSAmount = lineAmount;
			line.AL_JH = jobPK;
			line.AL_AC = chargeCodePK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_ReverseDate = ZDateTime.Empty;
			line.AL_GE = Env.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = jobPK;
			charge.JR_AL_APLine = line.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalCostAmt = lineAmount;
			charge.JR_LocalSellAmt = 0M;
			charge.JR_OH_CostAccount = line.AL_OH = ZGuid.Empty;
			return (line, charge);
		}

		public GlobalChargeCodeMapOrganization CreateGlobalChargeCodeMap(string code, string description, ZGuid orgHeaderPK)
		{
			var globalChargeCodeMap = Factory.New<GlobalChargeCodeMapOrganization>();
			globalChargeCodeMap.YG_Code = code;
			globalChargeCodeMap.YG_Desc = description;
			globalChargeCodeMap.YG_OH = orgHeaderPK;
			return globalChargeCodeMap;
		}

		public GlobalChargeCodeMapOrganization CreateGlobalChargeCodeMapWithPivot(string code, string description, ZGuid orgHeaderPK, ZGuid chargeCodePK, ZString type)
		{
			var globalChargeCodeMap = CreateGlobalChargeCodeMap(code, description, orgHeaderPK);
			CreateGlobalChargeCodeMapPivot(globalChargeCodeMap, chargeCodePK, type, ZGuid.Empty);
			return globalChargeCodeMap;
		}

		public GlobalChargeCodeMapPivotOrganization CreateGlobalChargeCodeMapPivot(GlobalChargeCodeMapOrganization chargeCodeMap, ZGuid chargeCodePK, ZString type, ZGuid localClientPK)
		{
			var globalChargeCodePivot = chargeCodeMap.PivotCollection.AddNew();
			globalChargeCodePivot.YP_AC = chargeCodePK;
			globalChargeCodePivot.YP_TYPE = type;
			globalChargeCodePivot.YP_OH_LocalClientOverride = localClientPK;
			return globalChargeCodePivot;
		}

		#endregion

		#region ChargeTypeOverride

		public AccChargeTypeOverride CreateChargeTypeOverride(AccChargeCode chargeCode, string chargeType, decimal margin, string jobType = "ALL", string direction = "ALL", string invoiceType = InvoiceTypesList.Codes.FinalInvoice)
		{
			var chargeTypeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeTypeOverride.AN_ChargeType = chargeType;
			chargeTypeOverride.AN_JobType = jobType;
			chargeTypeOverride.AN_JobDirection = direction;
			chargeTypeOverride.AN_InvoiceType = invoiceType;
			chargeTypeOverride.AN_MarginPercentage = margin;
			return chargeTypeOverride;
		}

		#endregion

		#region Tax rates

		public AccTaxRate CreateTaxRateWithoutZZ(string code, string description, int rateNum, int rateDenom = 1, string country = null, string type = null)
		{
			ZQuery query = new ZQuery(AccTaxRateSchema.AT_Code, code);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AccTaxRate result = Factory.LoadTop1<AccTaxRate>(query);
			if (result == null)
			{
				result = GetNewFactory().New<AccTaxRate>();
				result.AT_Code = code;
				result.AT_Description = description;
				result.AT_IsActive = true;
				result.AT_RN_NKCountry = country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				// AccTaxRate is not valid with empty AT_Type.
				result.AT_Type = type ?? (result.AT_Type.IsEmpty ? (ZString)AccTaxRate.Types.Rated : result.AT_Type);
				result.SetRate_ForTestOnly(rateNum, rateDenom);
				result.Factory.Save();
				result = Factory.Load<AccTaxRate>(result.PK);
			}
			return result;
		}

		public AccTaxRate CreateTaxRate(string code, string description, int rateNum, int rateDenom = 1, string country = null, string type = null)
		{
			return CreateTaxRateWithoutZZ("ZZ" + code, description, rateNum, rateDenom, country, type);
		}

		public AccTaxRate CreateTaxRate(string code, string description, string type, int rate, string extraType, int extraTaxRateNumerator, int extraTaxRateDenominator, string country = null)
		{
			return CreateTaxRate(code, description, type, rate, 1, extraType, extraTaxRateNumerator, extraTaxRateDenominator, country);
		}

		public AccTaxRate CreateTaxRate(string code, string description, string type, int rateNum, int rateDenom, string extraType, int extraTaxRateNumerator, int extraTaxRateDenominator, string country = null)
		{
			var result = GetNewFactory().Load<AccTaxRate>(CreateTaxRate(code, description, rateNum, rateDenom, country, type).PK);
			result.AT_ExtraTaxRateType = extraType;
			result.SetExtraRate_ForTestOnly(extraTaxRateNumerator, extraTaxRateDenominator);
			result.Factory.Save();

			return Factory.Load<AccTaxRate>(result.PK);
		}

		public AccInvMsg CreateTaxMsg(string code, string description, string englishMsg, string localMsg, string countryCode = "", string taxGroupCode = "", string taxGroupDescription = "")
		{
			var result = Factory.New<AccInvMsg>();
			result.A9_Code = code;
			result.A9_Description = description;
			result.A9_IsActive = true;
			result.A9_EnglishMsg = englishMsg;
			result.A9_LocalMsg = localMsg;
			result.A9_RN_NKCountryCode = string.IsNullOrEmpty(countryCode) ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : (ZString)countryCode;
			if (!string.IsNullOrEmpty(taxGroupCode))
			{
				var testValueCollection = new CodeDescriptionBoolRelatedItemCollection();
				testValueCollection.Add(taxGroupCode, (NoResString)taxGroupDescription, booleanValue: true, taxGroupCode);
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testValueCollection);

				var taxMessageGroupCode = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
				result.A9_TaxGroupCode = taxMessageGroupCode[0]?.Code.ToString();
			}
			return result;
		}

		public AccWithholding CreateOrLoadWithholdingTax(string code, string description, decimal rate)
		{
			var zzCode = "ZZ" + code;
			var companyPK = GlbCompany.CurrentCompany.PK;

			var existingWithholdingTaxQuery = new ZQuery(AccWithholdingSchema.AW_GC, companyPK);
			existingWithholdingTaxQuery.AddToFilter(AccWithholdingSchema.AW_Code, zzCode);
			var existingWithholdingTax = Factory.LoadTop1<AccWithholding>(existingWithholdingTaxQuery);

			if (existingWithholdingTax == null)
			{
				AccWithholding taxRate = GetNewFactory().New<AccWithholding>();
				taxRate.AW_Code = zzCode;
				taxRate.AW_Description = description;
				taxRate.AW_IsActive = true;
				taxRate.AW_Rate = rate;
				taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
				taxRate.Factory.Save();
				return Factory.Load<AccWithholding>(taxRate.PK);
			}
			else
			{
				return existingWithholdingTax;
			}
		}

		AccTaxRate excludedTax;
		public AccTaxRate ExcludedTax
		{
			get
			{
				if (excludedTax == null)
				{
					excludedTax = Factory.NewWithValidTestData<AccTaxRate>();
					excludedTax.AT_Code = "EXL";
					excludedTax.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
					excludedTax.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return excludedTax;
			}
		}

		AccTaxRate exempt;
		public AccTaxRate EXEMPT
		{
			get
			{
				if (exempt == null)
				{
					exempt = CreateTaxRate("EXEMPT", "EXEMPT", AccTaxRate.Types.Exempt, 0, string.Empty, 0, 1);
				}
				return exempt;
			}
		}

		public TaxIdAndTaxMessageCombinationRulesConfiguration CreateTaxIdAndTaxMessageCombinationRulesConfiguration(params (string LineType, AccTaxRate TaxRate, AccInvMsg TaxMessage)[] settings)
		{
			var config = new TaxIdAndTaxMessageCombinationRulesConfiguration();

			foreach (var setting in settings)
			{
				config.TaxIdAndTaxMessageCombinationRulesCollection.Add(new TaxIdAndTaxMessageCombinationRules()
				{
					LineType = setting.LineType,
					TaxRate = setting.TaxRate.PK,
					TaxMessage = setting.TaxMessage.PK
				});
			}

			return config;
		}

		#endregion

		#region Bank Account

		public AccBankAccount InsertBankAccount(ZString currencyNK)
		{
			AccBankAccount result = Factory.New<AccBankAccount>();
			result.AB_RX_NKAccountCurrency = currencyNK;
			result.AB_GC = GlbCompany.CurrentCompany.PK;
			result.AB_GB = GlbBranch.CurrentBranch.PK;
			result.AB_AG = InsertGLHeader().PK;
			result.AB_Code = "ABCBANK";
			return result;
		}

		public AccBankAccount CreateBankAccount(string code, string desc, RefCurrency currency, AccGLHeader gLHeader, string accountType = "")
		{
			var bankAccount = Factory.New<AccBankAccount>();

			bankAccount.AB_AccountType = !string.IsNullOrEmpty(accountType) ? (ZString)accountType : bankAccount.AB_AccountType;
			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;
			bankAccount.AB_AG = (gLHeader != null) ? gLHeader.PK : CreateGLHeader().PK;
			bankAccount.AB_BankName = desc;
			bankAccount.AB_BankAbbreviation = GetRandomString(3);
			bankAccount.AB_BSB = GetRandomString(10);
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		public AccBankAccount CreateBankAccount(string code, string desc, string name, string abbreviation, RefCurrency currency, string bSB, string accountNumber, AccGLHeader gLHeader)
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();

			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;
			bankAccount.AB_AG = gLHeader.PK;
			bankAccount.AB_BankName = name;
			bankAccount.AB_BankAbbreviation = abbreviation;
			bankAccount.AB_BSB = bSB;
			bankAccount.AB_AccountNum = accountNumber;
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		public AccAPAccountDetails AddAPBankAccountDetails(OrgHeader creditor, string paymentMethod, RefCurrency currency)
		{
			return AddAPBankAccountDetails(creditor, paymentMethod, currency.RX_Code);
		}

		public AccAPAccountDetails AddAPBankAccountDetails(OrgHeader creditor, string paymentMethod, string currencyCode)
		{
			var accountDetails = creditor.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = paymentMethod;
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_RX_NKAccountCurrency = currencyCode;
			accountDetails.A1_IsDefaultAccount = true;
			return accountDetails;
		}

		#endregion

		#region Branch

		public GlbBranch CreateBranch(string code, GlbCompany company, OrgHeader organisationProxy = null)
		{
			return CreateBranch(code, "Branch" + code, company, organisationProxy);
		}

		public GlbBranch CreateBranch(string code, string name, GlbCompany company, OrgHeader organisationProxy = null)
		{
			var branch = GetNewFactory().New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_BranchName = name;
			branch.GB_GC = company.PK;
			var unlocoQuery = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company.GC_RN_NKCountryCode) { OrderBy = RefUNLOCOSchema.Constants.RL_Code };
			branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(unlocoQuery).Code;
			if (organisationProxy != null)
			{
				branch.GB_OH_OrgProxy = organisationProxy.PK;
			}

			branch.Factory.Save();

			return Factory.Load<GlbBranch>(branch.PK);
		}

		public GlbBranch LoadIntercompanyBranch()
		{
			ZQuery query = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			Guid demoCompanyGuid = new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			query.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, demoCompanyGuid);
			return Factory.LoadTop1<GlbBranch>(query);
		}

		#endregion

		public AccGLAccountDescriptor CreateAccountDescriptor(ZGuid glHeaderPK, ZString accountNum, ZString accountType, ZString reportCategory, ZString language, ZString description, ZString country, ZString debitCredit)
		{
			AccGLAccountDescriptor descriptor = Factory.New<AccGLAccountDescriptor>();
			descriptor.AJ_Language = language;
			descriptor.AJ_AccountDescription = description;
			descriptor.AJ_RN_NKCountryOfCompliance = country;
			descriptor.AJ_ReportType = accountType;
			descriptor.AJ_ReportCategory = reportCategory;
			descriptor.AJ_LocalAccountNumber = accountNum;
			descriptor.AJ_DebitCredit = debitCredit;
			descriptor.ParentGLHeaderPK = glHeaderPK;
			return descriptor;
		}

		public AccGLAccountDescriptor CreateAccountDescriptor(AccGLHeader glHeader, ZString accountNum, ZString accountType, ZString reportCategory, ZString language, ZString description, ZString country, ZString debitCredit)
		{
			var glHeaderPK = glHeader != null ? glHeader.PK : CreateGLHeader().PK;
			return CreateAccountDescriptor(glHeaderPK, accountNum, accountType, reportCategory, language, description, country, debitCredit);
		}

		public GLDescriptorPivot CreateGLDescriptorPivot(AccGLAccountDescriptor testAccGLAccountDescriptor, AccGLHeader glHeader, ZString accountType, ZString reportCategory)
		{
			GLDescriptorPivot testGlDescriptorPivot = Factory.New<GLDescriptorPivot>();
			testGlDescriptorPivot.YJ_AJ = testAccGLAccountDescriptor.Clone().PK;
			testGlDescriptorPivot.YJ_AG = glHeader.PK;
			testGlDescriptorPivot.ReportType = accountType;
			testGlDescriptorPivot.ReportCategory = reportCategory;
			return testGlDescriptorPivot;
		}

		public AccGLAccountDescriptor CreateAccountDesriptorLight(ZString accountNum, ZString reportType, ZString language, ZString country)
		{
			var descriptor = Factory.New<AccGLAccountDescriptor>();
			descriptor.AJ_Language = language;
			descriptor.AJ_RN_NKCountryOfCompliance = country;
			descriptor.AJ_ReportType = reportType;
			descriptor.AJ_LocalAccountNumber = accountNum;
			descriptor.AJ_AccountDescription = accountNum + " - Description";
			// AccGLAccountDescriptor is not valid with empty AJ_DebitCredit.
			descriptor.AJ_DebitCredit = Constants.DebitCredit.Debit;
			return descriptor;
		}

		public GLDescriptorPivot CreateGLDescriptorPivotLight(AccGLAccountDescriptor testAccGLAccountDescriptor, AccGLHeader glHeader)
		{
			var testGlDescriptorPivot = Factory.New<GLDescriptorPivot>();
			testGlDescriptorPivot.YJ_AJ = testAccGLAccountDescriptor.PK;
			testGlDescriptorPivot.YJ_AG = glHeader.PK;
			return testGlDescriptorPivot;
		}

		public void CreateLocalAccountMappingForGLHeader(AccGLHeader glHeader, string language, string country)
		{
			var agd = CreateAccountDesriptorLight(country + "-" + glHeader.AG_AccountNum, "XXX", language, country);
			_ = CreateGLDescriptorPivotLight(agd, glHeader);
			Factory.Save();
		}

		public AccGLHeader CreateRetainedEarningsAccount()
		{
			// GL account for retained earnings
			var accountNum = AccountingConstants.RegistryDefaultValues.PLAppropriationAccount;
			var retainedEarningsAccount = Factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, accountNum)).FirstOrDefault();
			if (retainedEarningsAccount == null)
			{
				retainedEarningsAccount = Factory.NewWithValidTestData<AccGLHeader>();
				retainedEarningsAccount.AG_AccountNum = accountNum;
			}
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid(retainedEarningsAccount.PK.ToString()));
			return retainedEarningsAccount;
		}

		#region GenExportBatchSequence

		public GenExportBatchSequence CreateGenExportBatchSequenceHeader(int xB_BatchNumber, ZGuid headerPK, int xB_Sequence)
		{
			return CreateGenExportBatchSequence(xB_BatchNumber, headerPK, xB_Sequence,
				Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport,
				AccTransactionHeaderSchema.Constants.Prefix);
		}

		public GenExportBatchSequence CreateGenExportBatchSequencePostLine(int xB_BatchNumber, ZGuid linePK, int xB_Sequence)
		{
			return CreateGenExportBatchSequence(xB_BatchNumber, linePK, xB_Sequence,
				Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport,
				AccTransactionLinesSchema.Constants.Prefix);
		}

		public GenExportBatchSequence CreateGenExportBatchSequenceReverseLine(int xB_BatchNumber, ZGuid linePK, int xB_Sequence)
		{
			return CreateGenExportBatchSequence(xB_BatchNumber, linePK, xB_Sequence,
				Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport,
				AccTransactionLinesSchema.Constants.Prefix);
		}

		public GenExportBatchSequence CreateGenExportBatchSequenceWebServicePostLine(int xB_BatchNumber, ZGuid linePK, int xB_Sequence)
		{
			return CreateGenExportBatchSequence(xB_BatchNumber, linePK, xB_Sequence,
				Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServicePost,
				AccTransactionLinesSchema.Constants.Prefix);
		}

		public GenExportBatchSequence CreateGenExportBatchSequenceWebServiceReverseLine(int xB_BatchNumber, ZGuid linePK, int xB_Sequence)
		{
			return CreateGenExportBatchSequence(xB_BatchNumber, linePK, xB_Sequence,
				Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionExportWebServiceReverse,
				AccTransactionLinesSchema.Constants.Prefix);
		}

		GenExportBatchSequence CreateGenExportBatchSequence(int xB_BatchNumber, ZGuid xB_ParentID, int xB_Sequence, ZString batchType, ZString tablePrefix)
		{
			GenExportBatchSequence batch = Factory.New<GenExportBatchSequence>();
			batch.XB_Type = batchType;
			batch.XB_BatchNumber = xB_BatchNumber;
			batch.XB_ParentID = xB_ParentID;
			batch.XB_ParentTableCode = tablePrefix;
			batch.XB_Sequence = xB_Sequence;

			return batch;
		}

		#endregion

		#region GL Account

		public AccGLHeader CreateGLHeader()
		{
			var accountNum = GetRandomString(10);
			return CreateGLHeader(accountNum);
		}

		public AccGLHeader CreateGLHeaderWithSubAccount(ZString subAccountType, ZBool isSubClassValidationRuleMandatory)
		{
			var result = CreateGLHeader();
			CreateGLHeaderSubAccount(result, subAccountType, isSubClassValidationRuleMandatory);
			return result;
		}

		public AccGLHeader CreateGLHeader(string accountNum)
		{
			AccGLHeader result = null;

			if (IsLoadBeforeCreate)
			{ result = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, accountNum)); }
			if (result == null)
			{
				result = GetNewFactory().NewWithValidTestData<AccGLHeader>();
				result.AG_AccountNum = accountNum;
				result.Factory.Save();

				result = Factory.Load<AccGLHeader>(result.PK);
			}

			return result;
		}

		public AccGLHeader InsertGLHeader()
		{
			return CreateGLHeader();
		}

		public AccGLHeader CreateAccGLHeader(ZString accountNum, ZString column, ZString description, ZString accountType, ZString debitCredit, string cashFlowType = "", string units = "")
		{
			AccGLHeader header = null;
			if (IsLoadBeforeCreate)
			{ header = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, accountNum)); }
			if (header == null)
			{
				header = GetNewFactory().New<AccGLHeader>();
				header.AG_AccountNum = accountNum;
				header.AG_AccountType = accountType;
				header.AG_Column = column;
				header.AG_DebitCredit = debitCredit;
				header.AG_Description = description;
				header.AG_CashFlowType = cashFlowType;
				header.AG_StatisticalUnits = units;
				header.Factory.Save();

				header = Factory.Load<AccGLHeader>(header.PK);
			}

			return header;
		}

		public AccGLHeader CreateGLAccountAndLocalAccountMapping(string accountNum, string language, string country, string accountType)
		{
			var glHeader = CreateGLHeader(accountNum, accountType);
			CreateLocalAccountMappingForGLHeader(glHeader, language, country);

			return glHeader;
		}

		public AccGLHeader CreateGLHeader(string accountNum, string accountType)
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = accountNum;
			glHeader.AG_AccountType = accountType;
			return glHeader;
		}

		public AccGLHeader CreateARControlAccount()
		{
			return CreateAccGLHeader("6001.01.00", "AS", "Revenue Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateAPControlAccount()
		{
			return CreateAccGLHeader("6201.01.01", "LI", "Cost Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateARSuspenseControlAccount()
		{
			return CreateAccGLHeader("6301.01.00", "AS", "Revenue Suspense Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateAPSuspenseControlAccount()
		{
			return CreateAccGLHeader("7991.01.00", "LI", "Cost Suspense Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateInputTaxReceivablePendingAccount()
		{
			return CreateAccGLHeader("6310.11.00", "AS", "INPUT TAX RECEIVABLE - PENDING", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateOutputTaxPayablePendingAccount()
		{
			return CreateAccGLHeader("8310.11.00", "AS", "OUTPUT TAX PAYABLE - PENDING", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateAccruedRevenueControlAccount()
		{
			return CreateAccGLHeader("8888.11.00", "AS", "Accrued Revenue", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateAccruedCostControlAccount()
		{
			return CreateAccGLHeader("8888.11.01", "AS", "Accrued Cost", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateJobRevenueJournalControlAccount()
		{
			return CreateAccGLHeader("6555.55.55", "AS", "Job Revenue Journal Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateCFXAccount()
		{
			return CreateAccGLHeader("5300.50.00", "AS", "EXCHANGE RESERVE Custom", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateCurrentCompanyIntercompanyClearingGLHeader()
		{
			return CreateAccGLHeader("8810.10.00", "AS", "CLEARING ACCOUNT 1", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader CreateIntercompanyIntercompanyClearingGLHeader()
		{
			return CreateAccGLHeader("8810.20.00", "AS", "CLEARING ACCOUNT 2", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader GSTOutputControlAccount()
		{
			return CreateAccGLHeader("9100.00.00", "AS", "GST Output Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
		}

		public AccGLHeader GSTInputControlAccount()
		{
			return CreateAccGLHeader("9100.00.01", "AS", "GST Input Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateTaxExpenseLinkAccount()
		{
			return CreateAccGLHeader("6361.00.00", "AS", "OTHER TAX - CLEARING - TAX EXPENSE", AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreatePendingPrepaidTaxControlAccount()
		{
			return CreateAccGLHeader("6355.00.00", "AS", "OTHER TAX - PENDING PREPAID TAX", AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreatePrepaidAssetTaxControlAccount()
		{
			return CreateAccGLHeader("6350.00.00", "AS", "OTHER TAX - REALISED PREPAID TAX", AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit);
		}

		public AccGLHeader CreateRemittanceLibilityTaxControlAccount()
		{
			return CreateAccGLHeader("8320.00.00", "AS", "OTHER TAX - REALISED REMITTANCE LY", AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
		}

		public AccGLHeader GetGLAccountFromDB()
		{
			return Factory.LoadTop1<AccGLHeader>(new ZQuery());
		}

		public AccGLHeader GetGLAccountFromDB(string code)
		{
			return Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, code);
		}

		public static IDisposable SetTemporaryControlAccounts()
		{
			var testObjectFactory = new BusinessObjectFactory();
			var regValueCreator = new TestObjectCreator(testObjectFactory, true);

			var aRControlAccount = regValueCreator.CreateARControlAccount();
			var aPControlAccount = regValueCreator.CreateAPControlAccount();
			var aRSuspenseControlAccount = regValueCreator.CreateARSuspenseControlAccount();
			var aPSuspenseControlAccount = regValueCreator.CreateAPSuspenseControlAccount();
			var wIPControlAccount = regValueCreator.CreateAccruedRevenueControlAccount();
			var aCRControlAccount = regValueCreator.CreateAccruedCostControlAccount();
			var gSTOutputControlAccount = regValueCreator.GSTOutputControlAccount();
			var pendingGSTOutputControlAccount = regValueCreator.CreateOutputTaxPayablePendingAccount();
			var gSTInputControlAccount = regValueCreator.GSTInputControlAccount();
			var pendingGSTInputControlAccount = regValueCreator.CreateInputTaxReceivablePendingAccount();
			var jobRevenueJournalControlAccount = regValueCreator.CreateJobRevenueJournalControlAccount();
			var cFXAccount = regValueCreator.CreateCFXAccount();

			testObjectFactory.Save();

			var disposableRegs = new List<IDisposable>();
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.ARControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.APControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, wIPControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aCRControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gSTOutputControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingGSTOutputControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gSTInputControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingGSTInputControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()));
			disposableRegs.Add(AccountingConfigurationRegistry.Instance.CFXAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cFXAccount.PK.ToGuid()));

			DisposableAction disposableAction = new DisposableAction(() =>
			{
				if (disposableRegs != null)
				{
					disposableRegs.ForEach(x => x.Dispose());
				}

				aPControlAccount.Delete();
				aRControlAccount.Delete();
				aRSuspenseControlAccount.Delete();
				aPSuspenseControlAccount.Delete();
				wIPControlAccount.Delete();
				aCRControlAccount.Delete();
				gSTOutputControlAccount.Delete();
				pendingGSTOutputControlAccount.Delete();
				gSTInputControlAccount.Delete();
				pendingGSTInputControlAccount.Delete();
				jobRevenueJournalControlAccount.Delete();
				cFXAccount.Delete();
				testObjectFactory.Save();

				testObjectFactory = null;
				regValueCreator = null;
			}
				);

			return disposableAction;
		}
		#endregion

		#region GL Header Sub Account

		public AccGLHeaderSubAccount CreateGLHeaderSubAccount(AccGLHeader header, ZString subAccoutTypeOrTableCode, ZBool isSubClassValidationRuleMandatory)
		{
			var subAccount = header.SubAccountTypes.AddNew();
			subAccount.ASA_SubClass = subAccoutTypeOrTableCode;
			subAccount.ASA_IsSubClassValidationRuleMandatory = isSubClassValidationRuleMandatory;
			return subAccount;
		}

		#endregion

		#region GL Aggregate

		public AccGLAggregate CreateAccGLAggregate(decimal amount, int period, ZGuid glAccountPK, ZGuid branchPK, ZGuid companyPK, ZGuid departmentPK, string transactionCategory)
		{
			AccGLAggregate aggregate = Factory.New<AccGLAggregate>();
			aggregate.AA_Amount = amount;
			aggregate.AA_Period = period;
			aggregate.AA_AG = glAccountPK;
			aggregate.AA_GB = branchPK;
			aggregate.AA_GC = companyPK;
			aggregate.AA_GE = departmentPK;
			aggregate.AA_TransactionCategory = transactionCategory;
			return aggregate;
		}

		public AccGLAggregate CreateAccGLAggregate(decimal amount, int period, ZGuid glAccountPK, string transactionCategory)
		{
			return CreateAccGLAggregate(amount, period, glAccountPK, DefaultBranchPK, DefaultCompanyPK, DepartmentBrnPK, transactionCategory);
		}

		public void InsertAccGLAggregate(decimal amount, int period, ZGuid glAccountPK, string transactionCategory)
		{
			CreateAccGLAggregate(amount, period, glAccountPK, transactionCategory);
			Factory.Save();
		}

		#endregion

		#region GL Budget

		public GLBudget CreateAccGLBudget(int year, decimal opening, decimal closing, ZGuid glAccountPK, ZGuid branchPK, ZGuid departmentPK, string allocationType, decimal allocationValue, decimal allocationIncrement)
		{
			GLBudget budget = Factory.New<GLBudget>();
			budget.AU_Year = year;
			budget.AU_Opening = opening;
			budget.AU_Closing = closing;
			budget.AU_AG = glAccountPK;
			budget.AU_GB = branchPK;
			budget.AU_GE = departmentPK;
			budget.AU_AllocationType = allocationType;
			budget.AU_AllocationValue = allocationValue;
			budget.AU_AllocationIncrement = allocationIncrement;
			return budget;
		}

		#endregion

		#region Cheque Book

		public AccChequeBook InsertChequeBook(ZDecimal start, ZDecimal current, ZDecimal end, ZGuid bankAccountPK)
		{
			AccChequeBook result = Factory.New<AccChequeBook>();
			result.AK_Code = GetRandomString(10);
			result.AK_Desc = GetRandomString(35);
			result.AK_StartNo = start;
			result.AK_LastNo = end;
			result.AK_CurrentNo = current;
			result.AK_AB = bankAccountPK;
			result.AK_GB = GlbBranch.CurrentBranch.PK;
			return result;
		}

		public AccChequeBook CreateChequeBook(ZDecimal start, ZDecimal current, ZDecimal end, AccBankAccount bankAccount)
		{
			AccChequeBook result = Factory.New<AccChequeBook>();

			result.AK_Code = GetRandomString(10);
			result.AK_Desc = GetRandomString(35);
			result.AK_StartNo = start;
			result.AK_LastNo = end;
			result.AK_CurrentNo = current;
			result.AK_AB = bankAccount.PK;
			result.AK_GB = GlbBranch.CurrentBranch.PK;

			return result;
		}

		public AccChequeBook CreateChequeBook(string description, int numberOfCheques, AccBankAccount bankAccount)
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();

			chequeBook.AK_Code = ChequeCount.ToString();
			chequeBook.AK_Desc = description;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = numberOfCheques;

			++ChequeCount;
			return chequeBook;
		}

		public void SetupAutoPrintChequeBook(AccBankAccount bankAccount, AccChequeBook chequeBook, BusinessObjectFactory factory)
		{
			bankAccount.AB_SO_ChequeTemplate = StandardTemplatePK;
			var printQueue = CreatePrintQueue(factory);
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
		}

		public AccChequeBook GetAutoPrintChequeBook(BusinessObjectFactory newFactory, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			BusinessObjectFactory testFactory = newFactory;
			AccBankAccount bankAccount = testFactory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			var printQueue = CreatePrintQueue(testFactory);
			AccChequeBook chequeBook = testFactory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			testFactory.Save();
			return chequeBook;
		}

		public IStmPrintQueue CreatePrintQueue(BusinessObjectFactory factory)
		{
			var printQueue = (IStmPrintQueue)factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IStmPrintQueue)));
			printQueue.SQ_ServerName = "TestServer";
			return printQueue;
		}

		#endregion

		#region Transactions

		public AccTransactionHeader InsertTransaction(string transactionType)
		{
			// AccTransactionHeader is not valid with empty AH_Ledger.
			return InsertTransaction(transactionType, LedgerTypes.AccountsReceivable);
		}

		public AccTransactionHeader InsertTransaction(string transactionType, string ledger, ZDateTime dueDate, ZBool postToGL, ZGuid orgPK, ZGuid glAccountPK, ZString currency, decimal exchangeRate, decimal oSAmount, decimal localAmount)
		{
			AccTransactionHeader result = InsertTransaction(transactionType, ledger, dueDate, postToGL, orgPK, glAccountPK);
			result.AH_RX_NKTransactionCurrency = currency;
			result.AH_ExchangeRate = exchangeRate;
			result.AH_OSTotal = oSAmount;
			result.AH_InvoiceAmount = localAmount;
			result.AH_OutstandingAmount = localAmount;
			result.AH_OSOutstandingAmount = result.AH_IsOSOutstandingAmountApplicable ? oSAmount : 0m;
			return result;
		}

		public AccTransactionHeader InsertTransaction(string transactionType, string ledger, ZDateTime dueDate, ZBool postToGL, ZGuid orgPK, ZGuid glAccountPK)
		{
			AccTransactionHeader result = InsertTransaction(transactionType, ledger);
			result.AH_DueDate = dueDate;
			result.AH_AG = glAccountPK;
			result.AH_OH = orgPK;
			result.AH_PostToGL = postToGL ? "Y" : "N";
			return result;
		}

		public AccTransactionHeader InsertTransaction(string transactionType, string ledger)
		{
			AccTransactionHeader result = Factory.NewWithValidTestData<AccTransactionHeader>();
			result.AH_TransactionType = transactionType;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_GE = GlbDepartment.CurrentDepartment.PK;
			result.AH_PostDate = ZDateTime.Now;
			result.AH_InvoiceDate = ZDateTime.Now;
			result.AH_Ledger = ledger;
			result.AH_IsOSOutstandingAmountApplicable = AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value;
			return result;
		}

		public TransactionLine InsertTransactionLine(string lineType, ZGuid headerPK, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountExTax, GetNewInvoiceLine newLineDelegate)
		{
			TransactionLine line = Factory.New<TransactionLine>();
			line.AL_AH = headerPK;
			if (job != null)
			{
				line.AL_JH = job.PK;
			}

			line.AL_LineType = lineType;
			line.AL_AC = chargeCode.PK;
			line.AL_Desc = desc;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = oSAmountExTax;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public delegate void PopulateJobChargeDetails(BaseCharge jobCharge);

		public Accrual CreateAccrual(Job jobHeader, PopulateJobChargeDetails jobChargeDelegate = null)
		{
			var linkedCharge = Factory.NewWithValidTestData<BaseCharge>();
			linkedCharge.JR_JH = jobHeader.PK;
			linkedCharge.JR_GB = jobHeader.JH_GB;
			linkedCharge.JR_GE = jobHeader.JH_GE;
			linkedCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			linkedCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			jobChargeDelegate?.Invoke(linkedCharge);

			var accrual = Factory.New<Accrual>();
			accrual.SetValues(jobHeader, linkedCharge);
			linkedCharge.JR_AL_APLine = accrual.PK;
			accrual.AL_AG = GLHeader1.PK;

			return accrual;
		}

		public Accrual CreateAccrual(BaseCharge relatedCharge = null)
		{
			if (relatedCharge == null)
			{
				relatedCharge = Factory.New<BaseCharge>();
				JobHeader jobHeader = CreateJobHeader();
				relatedCharge.JR_JH = jobHeader.PK;
				relatedCharge.FillWithValidTestData();
			}
			var accrual = Factory.New<Accrual>();
			accrual.SetValues(relatedCharge.InvoicingJob, relatedCharge);
			relatedCharge.JR_AL_APLine = accrual.PK;

			return accrual;
		}

		public Accrual CreateAccrual(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST, BaseCharge relatedCharge = null, OrgHeader creditor = null)
		{
			Accrual line = Factory.New<Accrual>();
			line.AL_AG = GLHeader1.PK;

			if (job != null)
			{
				line.AL_JH = job.PK;
			}
			if (creditor != null)
			{
				line.AL_OH = creditor.PK;
			}
			line.AL_AC = chargeCode.PK;
			line.AL_Desc = desc;
			line.AL_LineAmount = line.AL_OverseasTotal = oSAmountIncGST;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseCharge linkedCharge = relatedCharge ?? Factory.NewWithValidTestData<BaseCharge>();
			linkedCharge.JR_AL_APLine = line.PK;
			if (job != null)
			{
				linkedCharge.JR_JH = job.PK;
			}
			if (creditor != null)
			{
				linkedCharge.JR_OH_CostAccount = creditor.PK;
			}
			linkedCharge.JR_AC = line.AL_AC;
			line.AL_ExchangeRate = exchangeRate;
			linkedCharge.JR_LocalCostAmt = linkedCharge.JR_OSCostAmt = oSAmountIncGST;
			if (relatedCharge == null && !chargeCode.IsDisbursement)    // Stop creating WIP as we want just new Accrual
			{
				linkedCharge.JR_LocalSellAmt = linkedCharge.JR_OSSellAmt = 0m;
			}

			return line;
		}

		public Accrual CreateAccrual(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST, decimal oSExTaxAmount)
		{
			Accrual line = CreateAccrual(job, chargeCode, exchangeRate, desc, oSAmountIncGST);
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.RelatedJobCharge.SetAmountsFromLinkedLinesForTests();

			return line;
		}

		public Accrual CreateAccrual(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST, decimal oSExTaxAmount, decimal oSTaxAmount)
		{
			Accrual line = CreateAccrual(job, chargeCode, exchangeRate, desc, oSAmountIncGST);
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			line.RelatedJobCharge.SetAmountsFromLinkedLinesForTests();

			return line;
		}

		#region WIP

		public WIP CreateWIP(Job job, Guid pk, PopulateJobChargeDetails jobChargeDelegate = null)
		{
			var linkedCharge = CreateLinkedCharge(job, jobChargeDelegate);

			var wip = Factory.NewWithPrimaryKey<WIP>(pk);
			wip.FillWithValidTestData();
			SetupWip(job, linkedCharge, wip);

			return wip;
		}

		public WIP CreateWIP(Job job, PopulateJobChargeDetails jobChargeDelegate = null)
		{
			var linkedCharge = CreateLinkedCharge(job, jobChargeDelegate);

			var wip = Factory.New<WIP>();
			SetupWip(job, linkedCharge, wip);

			return wip;
		}

		public WIP CreateWIP()
		{
			var line = Factory.New<WIP>();

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			line.AL_ReverseDate = ZDateTime.Empty;
			line.AL_LineType = Enterprise.ZArchitecture.Core.TransactionLineTypes.WIP;
			line.AL_OSAmount = line.AL_LineAmount = 10m;
			line.AL_AG = GLHeader1.PK;

			var linkedCharge = Factory.New<BaseCharge>();
			linkedCharge.JR_AL_ARLine = line.PK;
			var jobHeader = CreateJobHeader();
			linkedCharge.JR_JH = jobHeader.PK;
			line.AL_JH = jobHeader.PK;
			linkedCharge.FillWithValidTestData();
			line.AL_AC = linkedCharge.JR_AC;
			linkedCharge.JR_OSSellAmt = -10m;
			linkedCharge.JR_OSCostAmt = 0m;

			return line;
		}

		public WIP CreateWIP(BaseCharge relatedCharge)
		{
			var line = Factory.New<WIP>();

			line.AL_JH = relatedCharge.JR_JH;
			line.AL_AC = relatedCharge.JR_AC;
			line.AL_Desc = relatedCharge.JR_Desc;
			line.AL_LineAmount = line.AL_OverseasTotal = relatedCharge.JR_OSSellAmt;
			line.AL_GB = relatedCharge.JR_GB;
			line.AL_GE = relatedCharge.JR_GE;
			line.AL_RX_NKTransactionCurrency = relatedCharge.JR_RX_NKSellCurrency;
			relatedCharge.JR_AL_ARLine = line.PK;
			line.AL_ExchangeRate = relatedCharge.JR_OSSellExRate;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public WIP CreateWIP(Job job, AccChargeCode chargeCode, decimal exchangeRate, string desc, decimal oSAmountIncGST, BaseCharge relatedCharge = null, OrgHeader debtor = null)
		{
			var linkedCharge = relatedCharge ?? Factory.NewWithValidTestData<BaseCharge>();
			linkedCharge.JR_JH = job.PK;
			linkedCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			linkedCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			if (debtor != null)
			{
				linkedCharge.JR_OH_SellAccount = debtor.PK;
			}
			linkedCharge.JR_AC = chargeCode.PK;
			linkedCharge.JR_Desc = desc;
			linkedCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			linkedCharge.JR_OSCostExRate = exchangeRate;
			linkedCharge.JR_OSSellAmt = oSAmountIncGST;
			linkedCharge.JR_LocalSellAmt = oSAmountIncGST;
			if (relatedCharge == null)  // Stop creating Accrual when we only need a WIP
			{
				linkedCharge.JR_OSCostAmt = 0m;
			}

			var wip = Factory.New<WIP>();
			wip.AL_AG = GLHeader1.PK;
			wip.SetValues(linkedCharge.InvoicingJob, linkedCharge);
			linkedCharge.JR_AL_ARLine = wip.PK;

			return wip;
		}

		BaseCharge CreateLinkedCharge(Job job, PopulateJobChargeDetails jobChargeDelegate = null)
		{
			var linkedCharge = Factory.NewWithValidTestData<BaseCharge>();
			linkedCharge.JR_JH = job.PK;
			linkedCharge.JR_GB = job.JH_GB;
			linkedCharge.JR_GE = job.JH_GE;
			linkedCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			linkedCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			jobChargeDelegate?.Invoke(linkedCharge);

			return linkedCharge;
		}

		void SetupWip(Job job, BaseCharge linkedCharge, WIP wip)
		{
			wip.SetValues(job, linkedCharge);
			linkedCharge.JR_AL_ARLine = wip.PK;
			wip.AL_AG = GLHeader1.PK;
		}

		#endregion

		public APInvoiceLine CreateCostLine(BaseCharge relatedCharge, ZGuid transactionHeaderPK)
		{
			var line = Factory.New<APInvoiceLine>();

			line.AL_AH = transactionHeaderPK;
			line.AL_JH = relatedCharge.JR_JH;
			line.AL_PlaceOfSupply = relatedCharge.JR_CostPlaceOfSupply;
			line.AL_AC = relatedCharge.JR_AC;
			line.AL_Desc = relatedCharge.JR_Desc;
			line.AL_OSExTaxAmount = relatedCharge.JR_OSCostAmt;
			line.AL_GB = relatedCharge.JR_GB;
			line.AL_GE = relatedCharge.JR_GE;
			line.AL_RX_NKTransactionCurrency = relatedCharge.JR_RX_NKCostCurrency;
			line.AL_AT = relatedCharge.JR_AT_CostGSTRate;
			if (relatedCharge.Accrual != null && !relatedCharge.Accrual.IsReversed)
			{
				relatedCharge.ReverseAccrual(ZDateTime.Now);
			}
			relatedCharge.JR_AL_APLine = line.PK;
			line.AL_ExchangeRate = relatedCharge.JR_OSCostExRate;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public ARInvoiceLine CreateRevenueLine(BaseCharge relatedCharge, ZGuid transactionHeaderPK)
		{
			var line = Factory.New<ARInvoiceLine>();

			line.AL_AH = transactionHeaderPK;
			line.AL_JH = relatedCharge.JR_JH;
			line.AL_PlaceOfSupply = relatedCharge.JR_SellPlaceOfSupply;
			line.AL_AC = relatedCharge.JR_AC;
			line.AL_Desc = relatedCharge.JR_Desc;
			line.AL_OSExTaxAmount = relatedCharge.JR_OSSellAmt;
			line.AL_GB = relatedCharge.JR_GB;
			line.AL_GE = relatedCharge.JR_GE;
			line.AL_RX_NKTransactionCurrency = relatedCharge.JR_RX_NKSellCurrency;
			line.AL_AT = relatedCharge.JR_AT_SellGSTRate;
			if (relatedCharge.WIP != null && !relatedCharge.WIP.IsReversed)
			{
				relatedCharge.ReverseWIP(ZDateTime.Now);
			}
			relatedCharge.JR_AL_ARLine = line.PK;
			line.AL_ExchangeRate = relatedCharge.JR_OSSellExRate;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public void CreateRelatedARAndAPInvoices(ZGuid jobPK, AccChargeCode chargeCode, out ARInvoice aRInvoice, out APInvoice aPInvoice)
		{
			aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine aRLine = (ARInvoiceLine)aRInvoice.Lines.AddNew();
			aRLine.AL_GB = GlbBranch.CurrentBranch.PK;
			aRLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			aRLine.AL_JH = jobPK;
			aRLine.AL_AC = chargeCode.PK;
			aRLine.AL_OSExTaxAmount = 10m;
			aRLine.AL_AG = GLHeader1.PK;

			aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine aPLine = (APInvoiceLine)aPInvoice.Lines.AddNew();
			aPLine.AL_GB = GlbBranch.CurrentBranch.PK;
			aPLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			aPLine.AL_JH = jobPK;
			aPLine.AL_AC = chargeCode.PK;
			aPLine.AL_OSExTaxAmount = 10m;
			aPLine.AL_AG = GLHeader1.PK;

			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobPK;
			jobCharge.JR_AL_ARLine = aRLine.PK;
			jobCharge.JR_AL_APLine = aPLine.PK;
			jobCharge.SetAmountsFromLinkedLinesForTests();
		}

		public ARInvoiceLine CreateARInvoiceLine(ARInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountExTax)
		{
			return (ARInvoiceLine)CreateInvoiceLine(TransactionLineTypes.Revenue, parent, job, chargeCode, currency, exchangeRate, desc, oSAmountExTax, (factory) => factory.New<ARInvoiceLine>());
		}

		public ARInvoiceLine CreateARInvoiceLineWithJobCharge(ARInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountExTax, ZGuid ratePK)
		{
			var line = CreateARInvoiceLine(parent, job, chargeCode, currency, exchangeRate, desc, oSAmountExTax);
			line.AL_AT = ratePK;
			CreateJobCharge(line, job, chargeCode);
			return line;
		}

		public APInvoiceLine CreateAPInvoiceLine(APInvoice parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal oSAmountExTax)
		{
			return (APInvoiceLine)CreateInvoiceLine(TransactionLineTypes.Cost, parent, job, chargeCode, currency, exchangeRate, desc, oSAmountExTax, (factory) => factory.New<APInvoiceLine>());
		}

		public InvoicingLineBase CreateInvoiceLine(InvoicingBase parent, Job job, AccChargeCode chargeCode, decimal oSAmountExTax, RefCurrency currency = null, decimal? exchangeRate = null, string desc = null, bool setCurrentDepartment = true, AccTaxRate taxRate = null)
		{
			return CreateInvoiceLine(null, parent, job, chargeCode, currency, exchangeRate, desc, oSAmountExTax, setCurrentDepartment: setCurrentDepartment, taxRate: taxRate);
		}

		public InvoicingLineBase CreateInvoiceLine(string lineType, InvoicingBase parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal? exchangeRate, string desc, decimal oSAmountExTax, GetNewInvoiceLine newLineDelegate = null, bool setCurrentDepartment = true, AccTaxRate taxRate = null)
		{
			InvoicingLineBase line = (parent == null) ? newLineDelegate(Factory) : (InvoicingLineBase)parent.Lines.AddNew();

			if (job != null)
			{
				line.AL_JH = job.PK;
			}

			if (lineType != null)
			{
				line.AL_LineType = lineType;
			}

			if (parent != null && (parent.AH_Ledger == LedgerTypes.AccountsPayable || parent.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions))
			{
				line.GenericCharge = chargeCode.PK;
			}
			else
			{
				line.AL_AC = chargeCode.PK;
			}
			if (desc != null)
			{
				line.AL_Desc = desc;
			}

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			if (setCurrentDepartment)
			{
				line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			}
			if (currency != null)
			{
				line.ExchangeRate.Currency = currency.RX_Code;
			}

			if (exchangeRate != null)
			{
				line.ExchangeRate.Rate = exchangeRate.Value;
			}

			line.AL_OSExTaxAmount = oSAmountExTax;
			if (line.AL_AG.IsEmpty)
			{
				line.AL_AG = GLHeader1.PK;
			}

			if (taxRate != null)
			{
				line.AL_AT = taxRate.PK;
			}

			return line;
		}
		public delegate InvoiceLine GetNewInvoiceLine(BusinessObjectFactory factory);

		public JobRevenueJournal CreateJobRevenueJournal(AccChargeCode chargeCode, Job job, ZDecimal osAmount)
		{
			return CreateJobRevenueJournal(typeof(JobRevenueJournal), chargeCode, job, osAmount);
		}

		public JobRevenueJournal CreateJobRevenueJournal(Type jobRevenueJournalType, AccChargeCode chargeCode, Job job, ZDecimal osAmount)
		{
			return CreateJobRevenueJournal(jobRevenueJournalType, chargeCode, job.PK, osAmount);
		}

		public JobRevenueJournal CreateJobRevenueJournal(Type jobRevenueJournalType, AccChargeCode chargeCode, ZGuid jobPK, ZDecimal osAmount)
		{
			JobRevenueJournal journal = (JobRevenueJournal)Factory.New(jobRevenueJournalType);
			journal.AH_Desc = "Desc";

			CreateJobRevenueJournalLine(journal, chargeCode, jobPK, osAmount, DebitCreditDataEntry.DR);
			CreateJobRevenueJournalLine(journal, chargeCode, jobPK, osAmount, DebitCreditDataEntry.CR);

			return journal;
		}

		public JobRevenueJournalLine CreateJobRevenueJournalLine(JobRevenueJournal parent, AccChargeCode chargeCode, ZGuid jobPK, ZDecimal osAmount, ZString debitCredit)
		{
			var line = (JobRevenueJournalLine)parent.Lines.AddNew();
			line.AL_AC = chargeCode.PK;
			line.AL_JH = jobPK;
			line.OSUnsignedLineAmount = osAmount;
			line.DebitCreditSign = debitCredit;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		#region Receipt / Payment

		public APPayment CreateAPPayment(ZDecimal exchangeRate, ZDecimal osExTaxAmount, ZDateTime postDate, ZDateTime dueDate, ZGuid organisationPK, ZGuid bankAccountPK)
		{
			APPayment result = Factory.New<APPayment>();
			PopulateReceiptPayment(result, exchangeRate, osExTaxAmount, postDate, dueDate, organisationPK, bankAccountPK);
			return result;
		}

		public ARPayment CreateARPayment(ZDecimal exchangeRate, ZDecimal osExTaxAmount, ZDateTime postDate, ZDateTime dueDate, ZGuid organisationPK, ZGuid bankAccountPK)
		{
			ARPayment result = Factory.New<ARPayment>();
			PopulateReceiptPayment(result, exchangeRate, osExTaxAmount, postDate, dueDate, organisationPK, bankAccountPK);
			return result;
		}

		public APReceipt CreateAPReceipt(ZDecimal exchangeRate, ZDecimal osExTaxAmount, ZDateTime postDate, ZDateTime dueDate, ZGuid organisationPK, ZGuid bankAccountPK)
		{
			APReceipt result = Factory.New<APReceipt>();
			PopulateReceiptPayment(result, exchangeRate, osExTaxAmount, postDate, dueDate, organisationPK, bankAccountPK);
			return result;
		}

		public ARReceipt CreateARReceipt(ZDecimal exchangeRate, ZDecimal osExTaxAmount, ZDateTime postDate, ZDateTime dueDate, ZGuid organisationPK, ZGuid bankAccountPK)
		{
			ARReceipt result = Factory.New<ARReceipt>();
			PopulateReceiptPayment(result, exchangeRate, osExTaxAmount, postDate, dueDate, organisationPK, bankAccountPK);
			return result;
		}

		void PopulateReceiptPayment(ReceiptPaymentBase receiptPayment, ZDecimal exchangeRate, ZDecimal osExTaxAmount, ZDateTime postDate, ZDateTime dueDate, ZGuid organisationPK, ZGuid bankAccountPK)
		{
			receiptPayment.AH_PostDate = postDate;
			receiptPayment.AH_DueDate = dueDate;
			receiptPayment.AH_OH = organisationPK;
			receiptPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			receiptPayment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			receiptPayment.AH_ReceiptType = ReceiptTypes.Cash;
			receiptPayment.AH_AB = bankAccountPK;
			receiptPayment.AH_ChequeOrReference = "CASH";
			receiptPayment.AH_ExchangeRate = exchangeRate;
			receiptPayment.AH_OSExTaxAmount = osExTaxAmount;
		}

		#endregion

		#region Discount

		public APDiscount CreateAPDiscount(decimal amount, ZDateTime postDate, ZGuid organisationPK)
		{
			APDiscount discount = Factory.New<APDiscount>();
			PopulateDiscount(discount, amount, postDate, organisationPK);
			return discount;
		}

		public ARDiscount CreateARDiscount(decimal amount, ZDateTime postDate, ZGuid organisationPK)
		{
			ARDiscount discount = Factory.New<ARDiscount>();
			PopulateDiscount(discount, amount, postDate, organisationPK);
			return discount;
		}

		void PopulateDiscount(Discount discount, decimal amount, ZDateTime postDate, ZGuid organisationPK, string transactionNumber = "VALUEFORTEST")
		{
			discount.AH_ExchangeRate = 1.0m;
			discount.AH_InvoiceAmount = amount;
			discount.AH_OSExTaxAmount = amount;
			discount.AH_LocalExTaxAmount = amount;
			discount.AH_OutstandingAmount = 0m;
			discount.AH_OSOutstandingAmount = 0m;
			discount.AH_FullyPaidDate = postDate;
			discount.AH_PostDate = postDate;
			discount.AH_DueDate = postDate;
			discount.AH_OH = organisationPK;
			discount.AH_TransactionNum = transactionNumber;
		}

		#endregion

		#region Overpayment

		public T CreateOverpayment<T>(decimal amount, ZDateTime postDate, ZGuid organisationPK, string transactionNumber = "VALUEFORTEST") where T : Overpayment
		{
			T overpayment = Factory.New<T>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			overpayment.AH_Ledger = overpayment.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : overpayment.AH_Ledger;
			overpayment.AH_ExchangeRate = 1.0m;
			overpayment.AH_InvoiceAmount = amount;
			overpayment.AH_OSExTaxAmount = amount;
			overpayment.AH_LocalExTaxAmount = amount;
			overpayment.AH_OutstandingAmount = 0m;
			overpayment.AH_OSOutstandingAmount = 0m;
			overpayment.AH_FullyPaidDate = postDate;
			overpayment.AH_PostDate = postDate;
			overpayment.AH_DueDate = postDate;
			overpayment.AH_OH = organisationPK;
			overpayment.AH_TransactionNum = transactionNumber;

			return overpayment;
		}

		#endregion

		#region Exchange Difference

		public T CreateExchangeDifference<T>(decimal amount, ZDateTime postDate, ZGuid organisationPK, string transactionNumber = "VALUEFORTEST") where T : ExchangeDifference
		{
			T exchangeDifference = Factory.New<T>();

			exchangeDifference.AH_ExchangeRate = 1.0m;
			exchangeDifference.AH_InvoiceAmount = amount;
			exchangeDifference.AH_OSExTaxAmount = amount;
			exchangeDifference.AH_LocalExTaxAmount = amount;
			exchangeDifference.AH_OutstandingAmount = 0m;
			exchangeDifference.AH_OSOutstandingAmount = 0m;
			exchangeDifference.AH_FullyPaidDate = postDate;
			exchangeDifference.AH_PostDate = postDate;
			exchangeDifference.AH_DueDate = postDate;
			exchangeDifference.AH_OH = organisationPK;
			exchangeDifference.AH_TransactionNum = transactionNumber;

			return exchangeDifference;
		}

		#endregion

		#region ARAP Journal

		public T CreateJournal<T>(decimal amount, ZDateTime postDate, ZGuid organisationPK, string transactionNumber = "VALUEFORTEST") where T : Journal
		{
			T journal = Factory.New<T>();

			journal.AH_ExchangeRate = 1.0m;
			journal.AH_InvoiceAmount = amount;
			journal.AH_OSExTaxAmount = amount;
			journal.AH_LocalExTaxAmount = amount;
			journal.AH_PostDate = postDate;
			journal.AH_DueDate = postDate;
			journal.AH_OH = organisationPK;
			journal.AH_TransactionNum = transactionNumber;

			return journal;
		}

		#endregion

		#region ARAP Transfer

		public T CreateTransfer<T>(decimal amount, ZDateTime postDate, ZGuid fromOrganisationPK, ZGuid toOrganisationPK) where T : Transfer
		{
			T transfer = (T)Transfer.New(typeof(T), Factory);

			transfer.AH_ExchangeRateAmount = 1.0m;
			transfer.AH_InvoiceAmount = amount;
			transfer.AH_OSTotal = amount;
			transfer.AH_PostDate = postDate;
			transfer.AH_FromAccount = fromOrganisationPK;
			transfer.AH_ToAccount = toOrganisationPK;

			return transfer;
		}

		public TransferRow CreateTransferRow(Type transferType, ZString transactionNum, ZDecimal amount, bool createdByMatching, int transactionCount)
		{
			TransferRow result = (TransferRow)Factory.New(transferType);
			result.AH_TransactionNum = transactionNum;
			result.AH_OSTotal = amount;
			result.AH_InvoiceAmount = amount;
			result.AH_TransactionCreatedByMatching = createdByMatching;
			result.AH_TransactionCount = (byte)transactionCount;

			return result;
		}

		#endregion

		#region Contra

		public Contra CreateContra(decimal amount, ZDateTime postDate, ZGuid aROrganisationPK, ZGuid aPOrganisationPK)
		{
			Contra contra = Contra.New(Factory);

			contra.AH_ARAccount = aROrganisationPK;
			contra.AH_APAccount = aPOrganisationPK;
			contra.AH_ExchangeRateAmount = 1.0m;
			contra.AH_InvoiceAmount = amount;
			contra.AH_OSTotal = amount;
			contra.AH_PostDate = postDate;

			return contra;
		}

		public ContraRow CreateContraRow(Type contraType, ZString transactionNum, ZDecimal amount, bool createdByMatching, int transactionCount)
		{
			ContraRow result = (ContraRow)Factory.New(contraType);
			result.AH_TransactionNum = transactionNum;
			result.AH_OSTotal = amount;
			result.AH_InvoiceAmount = amount;
			result.AH_TransactionCreatedByMatching = createdByMatching;
			result.AH_TransactionCount = (byte)transactionCount;

			return result;
		}

		#endregion

		#region GL Journal

		public GLJournal CreateGLJournal(string transactionType, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime? dueDate = null)
		{
			return CreateGLJournal<GLJournal>(transactionType, invoiceDate, postDate, dueDate);
		}

		public FCBAdjustmentJournal CreateFCBJournal(ZDateTime invoiceDate, ZDateTime postDate)
		{
			var glJournal = Factory.New<FCBAdjustmentJournal>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			glJournal.AH_Ledger = glJournal.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : glJournal.AH_Ledger;
			glJournal.AH_InvoiceDate = invoiceDate;
			glJournal.AH_PostDate = postDate;

			return glJournal;
		}

		public GLJournal CreateGLJournal<T>(string transactionType, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime? dueDate = null) where T : GLJournal
		{
			var glJournal = Factory.New<T>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			glJournal.AH_Ledger = glJournal.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : glJournal.AH_Ledger;

			glJournal.AH_TransactionType = transactionType;

			glJournal.AH_InvoiceDate = invoiceDate;
			glJournal.AH_PostDate = postDate;
			if (transactionType == TransactionTypes.GLAutoJournal || transactionType == TransactionTypes.GLReversingJournal)
			{
				if (!dueDate.HasValue)
				{
					throw new Exception("Due Date required for GLAutoJournal and GLReversingJournal.");
				}
				glJournal.AH_DueDate = dueDate.Value;
			}

			return glJournal;
		}

		public GLJournalLine CreateGLJournalLine(GLJournal glJournal, decimal unsignedLineAmount, DebitCredit debitOrCredit, ZGuid glAccountPK)
		{
			GLJournalLine line = (GLJournalLine)glJournal.Lines.AddNew();

			line.UnsignedOSLineAmount = unsignedLineAmount;
			line.DebitCreditSign = debitOrCredit.ToString();
			line.AL_AG = glAccountPK;

			return line;
		}

		#endregion

		#region Cash Book Exchange Difference

		public CashbookExchangeDiff CreateCashbookExchangeDifference(ZDateTime postDate, decimal amount, AccBankAccount bankAccount, bool saveCashBook = true)
		{
			CashbookExchangeDiff exchangeDifference = Factory.New<CashbookExchangeDiff>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			exchangeDifference.AH_Ledger = exchangeDifference.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : exchangeDifference.AH_Ledger;
			exchangeDifference.AH_InvoiceDate = postDate;
			exchangeDifference.AH_PostDate = postDate;
			exchangeDifference.AH_AB = bankAccount.PK;
			exchangeDifference.AH_Desc = "test bank currency adjustment";
			exchangeDifference.AH_ExchangeRate = exchangeDifference.AH_ExchangeRate == 0 ? (ZDecimal)1m : exchangeDifference.AH_ExchangeRate;

			if (saveCashBook)
			{
				Factory.Save();
			}

			// Amounts gets reset in CashbookExchangeDiff.OnFactorySavingBeforeTransactionCore
			exchangeDifference.AH_ExchangeRate = 1.5m;
			exchangeDifference.AH_LocalExTaxAmount = amount;
			exchangeDifference.AH_LocalTaxAmount = 0m;

			return exchangeDifference;
		}

		public CashbookExchangeDiff CreateCashbookExchangeDifference(ZDateTime postDate, decimal amount, string currency, bool saveCashBook = true)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = currency;
			bankAccount.AB_OpenBalance = -amount;
			Factory.Save();

			CashbookExchangeDiff exchangeDifference = Factory.New<CashbookExchangeDiff>();

			exchangeDifference.AH_Ledger = exchangeDifference.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : exchangeDifference.AH_Ledger;
			exchangeDifference.AH_InvoiceDate = postDate;
			exchangeDifference.AH_PostDate = postDate;
			exchangeDifference.AH_AB = bankAccount.PK;
			exchangeDifference.AH_Desc = "test bank currency adjustment";

			if (saveCashBook)
			{
				Factory.Save();
			}

			AccountingTestHelper.AssertEquals("LocalExTaxAmount should be equal to amount.", amount, exchangeDifference.AH_LocalExTaxAmount);

			return exchangeDifference;
		}

		#endregion

		#region Cash Book Bank Transfer

		public BankTransfer CreateBankTransfer(ZDateTime postDate, ZGuid bankFromPK, ZGuid bankToPK, decimal localAmount, decimal exchangeRate)
		{
			BankTransfer transfer = new BankTransfer(Factory, null)
			{
				AH_PostDate = postDate,
				BankTransferFromPK = bankFromPK,
				BankTransferToPK = bankToPK,
				LocalBuyAmount = localAmount,
				SellAmount = localAmount,
				BuyAmount = localAmount * exchangeRate
			};

			return transfer;
		}

		#endregion

		#region Cash Book Direct Payment

		public DirectPayment CreateDirectPayment(ZDateTime postDate, decimal amount, decimal taxAmount, decimal amount2, decimal taxAmount2)
		{
			return CreateDirectPayment(postDate, amount, taxAmount, amount2, taxAmount2, AUDBankAccount.PK, 1M);
		}

		public DirectPayment CreateDirectPayment(ZDateTime postDate, decimal amount, decimal taxAmount, decimal amount2, decimal taxAmount2, ZGuid bankAccountPK, ZDecimal exchangeRate)
		{
			DirectPayment payment = Factory.New<DirectPayment>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			payment.AH_Ledger = payment.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : payment.AH_Ledger;

			payment.AH_AB = bankAccountPK;
			payment.AH_TransactionReference = "abc";
			payment.AH_Desc = "test receipt";
			payment.AH_InvoiceDate = postDate;
			payment.AH_PostDate = postDate;
			payment.AH_ChequeOrReference = "000123";
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_ChequeDrawer = "bbb";
			payment.AH_DrawerBank = "ccc";
			payment.AH_DrawerBranch = "ddd";
			payment.AH_ExchangeRate = exchangeRate;

			CreateDirectTransactionLine(payment, GLHeader1, amount, taxAmount, "test line");
			CreateDirectTransactionLine(payment, GLHeader2, amount2, taxAmount2, "test line 2");

			return payment;
		}

		public DirectTransactionLineBase CreateDirectTransactionLine(DirectTransactionHeaderBase parent, AccGLHeader glHeader, decimal amount, decimal taxAmount, string desc)
		{
			var line = (DirectTransactionLineBase)parent.Lines.AddNew();
			line.AL_AG = glHeader.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OSExTaxAmount = amount;
			line.AL_OSTaxAmount = taxAmount;
			line.AL_Desc = desc;

			return line;
		}

		#endregion

		#region Cash Book Direct Receipt

		public DirectReceipt CreateDirectReceipt(ZDateTime postDate, decimal amount, decimal taxAmount, decimal amount2, decimal taxAmount2)
		{
			return CreateDirectReceipt(postDate, amount, taxAmount, amount2, taxAmount2, AUDBankAccount.PK, 1m);
		}

		public DirectReceipt CreateDirectReceipt(ZDateTime postDate, decimal amount, decimal taxAmount, decimal amount2, decimal taxAmount2, ZGuid bankAccountPK, ZDecimal exchangeRate)
		{
			DirectReceipt receipt = Factory.New<DirectReceipt>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			receipt.AH_Ledger = receipt.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : receipt.AH_Ledger;

			receipt.AH_AB = bankAccountPK;
			receipt.AH_TransactionReference = "abc";
			receipt.AH_Desc = "test receipt";
			receipt.AH_InvoiceDate = postDate;
			receipt.AH_PostDate = postDate;
			receipt.AH_ChequeOrReference = "00123";
			receipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			receipt.AH_ChequeDrawer = "bbb";
			receipt.AH_DrawerBank = "ccc";
			receipt.AH_DrawerBranch = "ddd";
			receipt.AH_ExchangeRate = exchangeRate;

			CreateDirectTransactionLine(receipt, GLHeader1, amount, taxAmount, "test line");
			CreateDirectTransactionLine(receipt, GLHeader2, amount2, taxAmount2, "test line 2");

			return receipt;
		}

		#endregion

		#region Job Costing Journal

		public JCJournalHeader CreateJCJournalHeader(ZDateTime postDate, decimal amount)
		{
			JCJournalHeader journal = Factory.New<JCJournalHeader>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			journal.AH_Ledger = journal.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : journal.AH_Ledger;

			journal.AH_PostDate = postDate;
			journal.AH_InvoiceAmount = amount;

			return journal;
		}

		public JCJournalLine CreateJCJournalLine(JCJournalHeader journal, AccChargeCode chargeCode, Job job, ZDateTime postDate, decimal amount, bool addGLAccount = true)
		{
			JCJournalLine journalLine = journal.Lines.AddNew();

			journalLine.AL_AC = chargeCode.PK;
			journalLine.AL_OSExTaxAmount = amount;
			journalLine.AL_PostDate = postDate;
			journalLine.AL_ReverseDate = ZDateTime.Empty;
			if (addGLAccount)
			{
				journalLine.AL_AG = GLHeader1.PK;
			}

			if (job != null)
			{
				journalLine.AL_JH = job.PK;

				BaseCharge linkedCharge = Factory.NewWithValidTestData<BaseCharge>();
				linkedCharge.JR_AL_CFXLine = journalLine.PK;
				linkedCharge.JR_LineCFX = amount;
				linkedCharge.JR_JH = job.PK;
			}

			return journalLine;
		}

		#endregion

		#region Line & Header Sub Account

		public T CreateTransactionLineSubAccount<T>(ZGuid linePK, string subAccoutTypeOrTableCode, ZGuid subAccountParentId) where T : AccTransactionLineSubAccount
		{
			var parentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccoutTypeOrTableCode);
			var subAccount = Factory.New<T>();
			subAccount.AL1_AL = linePK;
			subAccount.AL1_SubClassParentTableCode = parentTableCode;
			subAccount.AL1_SubClassParentId = subAccountParentId;
			return subAccount;
		}

		public void SetUpTransactionLineSubAccount(InvoicingLineBase line, string subAccoutTypeOrTableCode, ZGuid subAccountParentId)
		{
			var parentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccoutTypeOrTableCode);
			var accTransactionLineSubAccount = line.SubAccounts.OfType<AccTransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode.Equals(parentTableCode));
			if (accTransactionLineSubAccount != null)
			{
				accTransactionLineSubAccount.AL1_AL = line.PK;
				accTransactionLineSubAccount.AL1_SubClassParentTableCode = parentTableCode;
				accTransactionLineSubAccount.AL1_SubClassParentId = subAccountParentId;
			}
		}

		public AccTransactionHeaderSubAccount CreateTransactionHeaderSubAccount(ZGuid headerPK, string subAccoutTypeOrTableCode, ZGuid subAccountParentId)
		{
			var parentTableCode = SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccoutTypeOrTableCode);
			var subAccount = Factory.New<AccTransactionHeaderSubAccount>();
			subAccount.AHS_AH = headerPK;
			subAccount.AHS_SubClassParentTableCode = parentTableCode;
			subAccount.AHS_SubClassParentId = subAccountParentId;
			return subAccount;
		}

		#endregion

		#endregion

		#region Dsb Job Close Batch

		public DsbJobCloseBatch CreateDsbJobCloseBatch(string batchNum)
		{
			var utcNow = ZDateTime.UtcNow;
			var dsbJobCloseBatch = Factory.New<DsbJobCloseBatch>();
			dsbJobCloseBatch.JBB_BatchNumber = batchNum;
			dsbJobCloseBatch.JBB_GC = Env.CurrentCompanyPK;
			dsbJobCloseBatch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
			dsbJobCloseBatch.JBB_TotalAmount = 100m;
			dsbJobCloseBatch.JBB_LargestAmount = 10000m;
			dsbJobCloseBatch.JBB_SmallestAmount = 0m;
			dsbJobCloseBatch.JBB_GS_NKApprovingUser = "E";
			dsbJobCloseBatch.JBB_ApprovalTimeUtc = utcNow;
			dsbJobCloseBatch.JBB_SystemCreateTimeUtc = utcNow.AddMinutes(-1);
			dsbJobCloseBatch.JBB_SystemCreateUser = "E";
			return dsbJobCloseBatch;
		}

		public void PrepareDsbJobCloseBatchEnvironment(
				out Job job1, out Job job2, out DsbJobCloseBatch batch1,
				out InvoiceLine line1j1, out InvoiceLine line1j2, out InvoiceLine line2j1, out InvoiceLine line2j2,
				out AccChargeCode chargeCode1, out AccChargeCode chargeCode2,
				out Charge charge1Cst, out Charge charge1Rev, out Charge charge2Cst, out Charge charge2Rev)
		{
			var surplusGLAccount1 = CreateGLHeader("99991001");
			var shortfallGLAccount1 = CreateGLHeader("99991002");
			var surplusGLAccount2 = CreateGLHeader("99992001");
			var shortfallGLAccount2 = CreateGLHeader("99992002");

			var costGLAccount1 = CreateGLHeader("39991001");
			var revenueGLAccount1 = CreateGLHeader("39991002");
			var costGLAccount2 = CreateGLHeader("39992001");
			var revenueGLAccount2 = CreateGLHeader("39992002");

			Factory.Save();

			chargeCode1 = DSBChargeCode;
			chargeCode1.AC_AG_CostAccount = costGLAccount1.PK;
			chargeCode1.AC_AG_RevenueAccount = revenueGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementShortfallAccount = shortfallGLAccount1.PK;
			chargeCode1.AC_AG_DisbursementSurplusAccount = surplusGLAccount1.PK;

			chargeCode2 = DSBChargeCode1;
			chargeCode2.AC_AG_CostAccount = costGLAccount2.PK;
			chargeCode2.AC_AG_RevenueAccount = revenueGLAccount2.PK;
			chargeCode2.AC_AG_DisbursementShortfallAccount = shortfallGLAccount2.PK;
			chargeCode2.AC_AG_DisbursementSurplusAccount = surplusGLAccount2.PK;

			Factory.Save();

			job1 = CreateJobHeader();
			job2 = CreateJobHeader();

			Factory.Save();

			charge1Cst = CreateCharge(job1, chargeCode1, 100m, 100m);
			charge1Rev = CreateCharge(job1, chargeCode1, 200m, 200m);
			charge2Cst = CreateCharge(job2, chargeCode2, 200m, 200m);
			charge2Rev = CreateCharge(job2, chargeCode2, 50m, 50m);

			Factory.Save();

			batch1 = CreateDsbJobCloseBatch("B001");
			batch1.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;

			Factory.Save();

			var header1 = CreateInvoice(typeof(APInvoice));
			line1j1 = CreateCostLine(charge1Cst, header1.PK);
			line1j2 = CreateCostLine(charge2Cst, header1.PK);
			line1j1.AL_AG = charge1Cst.ChargeCode.AC_AG_CostAccount;
			line1j2.AL_AG = charge2Cst.ChargeCode.AC_AG_CostAccount;
			line1j1.AL_JBB = batch1.PK;
			line1j2.AL_JBB = batch1.PK;

			Factory.Save();

			var header2 = CreateInvoice(typeof(ARInvoice));
			line2j1 = CreateRevenueLine(charge1Rev, header2.PK);
			line2j2 = CreateRevenueLine(charge2Rev, header2.PK);
			line2j1.AL_AG = charge1Rev.ChargeCode.AC_AG_RevenueAccount;
			line2j2.AL_AG = charge2Rev.ChargeCode.AC_AG_RevenueAccount;
			line2j1.AL_JBB = batch1.PK;
			line2j2.AL_JBB = batch1.PK;

			Factory.Save();
		}

		public void CreateGLJournalWithDSBJobCloseBatch(out GLJournal glJournal, out DsbJobCloseBatch batch)
		{
			glJournal = CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);

			batch = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch.JBB_BatchNumber = "B001";
			batch.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Close;
			batch.JBB_SystemCreateTimeUtc = ZDateTime.Now;
			batch.JBB_AH_Journal = glJournal.PK;
		}

		#endregion

		#region AR Receipt

		public ARReceipt CreateARReceipt(ZString receiptType, ZString transactionType, ZString ledger, ZDecimal amount, ZGuid bankAccountPK)
		{
			return (ARReceipt)CreateReceiptOrPayment(receiptType, transactionType, ledger, amount, bankAccountPK);
		}

		public ReceiptPaymentBase CreateReceiptOrPayment(ZString receiptType, ZString transactionType, ZString ledger, ZDecimal amount, ZGuid bankAccountPK)
		{
			ReceiptPaymentBase testReceiptPayment = Factory.New<ARReceipt>();
			testReceiptPayment.AH_ReceiptType = receiptType;
			testReceiptPayment.AH_TransactionType = transactionType;
			testReceiptPayment.AH_Ledger = ledger;
			testReceiptPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testReceiptPayment.AH_LocalExTaxAmount = amount;

			testReceiptPayment.AH_OSExTaxAmount = amount;
			testReceiptPayment.AH_AB = bankAccountPK;
			testReceiptPayment.AH_ExchangeRate = testReceiptPayment.AH_ExchangeRate == 0 ? (ZDecimal)1m : testReceiptPayment.AH_ExchangeRate;

			if (receiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
			{
				testReceiptPayment.AH_ChequeOrReference = "000010001";
				testReceiptPayment.AH_ChequeDrawer = "Alicia";
				testReceiptPayment.AH_DrawerBank = "CBA";
				testReceiptPayment.AH_DrawerBranch = "Mascot Branch";
			}

			return testReceiptPayment;
		}

		#endregion

		#region AccReceivedCheques

		public AccReceivedCheque CreateAccReceivedCheque(ZDecimal amount, ZString bankName, ZString chequeDrawer, ZString currency, ZString chequeNumber, ZString chequeReference, ZDate dueDate, ZGuid givenBankAccount, ZString paymentLocation, ZGuid receivedFrom, ZString status)
		{
			var receivedCheque = Factory.New<AccReceivedCheque>();
			receivedCheque.RCH_AB_GivenBankAccount = givenBankAccount;
			receivedCheque.RCH_Amount = amount;
			receivedCheque.RCH_BankName = bankName;
			receivedCheque.RCH_ChequeNumber = chequeNumber;
			receivedCheque.RCH_ChequeDrawer = chequeDrawer;
			receivedCheque.RCH_ChequeReference = chequeReference;
			receivedCheque.RCH_DueDate = dueDate;
			receivedCheque.RCH_GC_Company = GlbCompany.CurrentCompany.PK;
			receivedCheque.RCH_OH_ReceivedFrom = receivedFrom;
			receivedCheque.RCH_PaymentLocation = paymentLocation;
			receivedCheque.RCH_RX_NKChequeCurrency = currency;
			receivedCheque.RCH_Status = status;
			receivedCheque.RCH_VATRegNo = "123";

			return receivedCheque;
		}

		public void CreateChequesAndSave()
		{
			CreateAccReceivedCheque(100, "BankA", "DrawerA", AUD.Code, "0001", "R001", ZDate.Today.AddDays(10), AUDBankAccount.PK, "LocationA", DebtorDE.PK, "COH");
			CreateAccReceivedCheque(200, "BankB", "DrawerB", USD.Code, "0002", "R002", ZDate.Today.AddDays(20), USDBankAccount.PK, "LocationB", DebtorTR.PK, "COH");
			Factory.Save();
		}

		#endregion

		#region Departments

		public GlbDepartment CreateDepartment(string code)
		{
			return CreateDepartment(code, "Department" + code);
		}

		public GlbDepartment CreateDepartment(string code, string description)
		{
			var department = GetNewFactory().New<GlbDepartment>();
			department.GE_Code = code;
			department.GE_Desc = description;
			department.GE_IsActive = true;
			department.Factory.Save();

			return Factory.Load<GlbDepartment>(department.PK);
		}

		GlbDepartment fMiscDepartment;
		public GlbDepartment MiscDepartment
		{
			get
			{
				if (fMiscDepartment == null)
				{
					ZQuery miscDepartmentQuery = new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK);
					miscDepartmentQuery.AddToFilter(GlbDepartmentSchema.GE_Misc, true);
					fMiscDepartment = Factory.LoadTop1<GlbDepartment>(miscDepartmentQuery);
				}
				return fMiscDepartment;
			}
		}

		GlbDepartment fNonCurrentDepartment;
		public GlbDepartment NonCurrentDepartment
		{
			get
			{
				if (fNonCurrentDepartment == null)
				{
					ZQuery nonCurrentDepartmentQuery = new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK) { OrderBy = GlbDepartmentSchema.GE_Code.Name };
					nonCurrentDepartmentQuery.AddToFilter(GlbDepartmentSchema.GE_Misc, false);
					fNonCurrentDepartment = Factory.LoadTop1<GlbDepartment>(nonCurrentDepartmentQuery);
				}
				return fNonCurrentDepartment;
			}
		}

		public GlbDepartment GEADepartment
		{
			get
			{
				return geaDepartment ?? (geaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA")));
			}
		}
		GlbDepartment geaDepartment;

		public GlbDepartment FESDepartment
		{
			get
			{
				return fFESDepartment ?? (fFESDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")));
			}
		}
		GlbDepartment fFESDepartment;

		public GlbDepartment FISDepartment
		{
			get
			{
				return fFISDepartment ?? (fFISDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")));
			}
		}
		GlbDepartment fFISDepartment;

		public GlbDepartment FIADepartment
		{
			get
			{
				return fFIADepartment ?? (fFIADepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIA")));
			}
		}
		GlbDepartment fFIADepartment;

		public GlbDepartment FEADepartment
		{
			get
			{
				return feaDepartment ?? (feaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")));
			}
		}
		GlbDepartment feaDepartment;

		GlbBranch fNonCurrentBranch;
		public GlbBranch NonCurrentBranch
		{
			get
			{
				if (fNonCurrentBranch == null)
				{
					ZQuery nonCurrentBranchQuery = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name };
					nonCurrentBranchQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
					fNonCurrentBranch = Factory.LoadTop1<GlbBranch>(nonCurrentBranchQuery);
				}
				return fNonCurrentBranch;
			}
		}

		GlbBranch fNonCurrentCompanyBranch;
		public GlbBranch NonCurrentCompanyBranch
		{
			get
			{
				if (fNonCurrentCompanyBranch == null)
				{
					fNonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbBranchSchema.GB_Code.Name });
				}

				return fNonCurrentCompanyBranch;
			}
		}

		GlbCompany fNonCurrentCompany;
		public GlbCompany NonCurrentCompany
		{
			get
			{
				if (fNonCurrentCompany == null)
				{
					fNonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK) { OrderBy = GlbCompanySchema.GC_Code.Name });
				}

				return fNonCurrentCompany;
			}
		}

		public GlbCompany NonCurrentNonDemoCompany
		{
			get
			{
				if (nonCurrentNonDemoCompany == null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");

					nonCurrentNonDemoCompany = Factory.LoadTop1<GlbCompany>(query);
				}
				return nonCurrentNonDemoCompany;
			}
		}
		GlbCompany nonCurrentNonDemoCompany;

		#endregion

		#region Credit Notes

		public APCreditNote CreateAPCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc)
		{
			APCreditNote creditNote = Factory.New<APCreditNote>();

			creditNote.AH_InvoiceDate = ZDateTime.Today;
			creditNote.AH_TransactionNum = creditNoteNumber;
			creditNote.AH_OH = account.PK;
			creditNote.AH_RX_NKTransactionCurrency = currency.RX_Code;
			creditNote.AH_ExchangeRate = exchangeRate;
			creditNote.AH_Desc = desc;
			creditNote.AH_PostDate = ZDateTime.Today;
			creditNote.AH_DueDate = ZDateTime.Today.AddMonths(1);

			return creditNote;
		}

		public APCreditNote CreateAPCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc, ZDateTime dueDate, ZBool postToGL)
		{
			APCreditNote creditNote = CreateAPCreditNote(creditNoteNumber, account, currency, exchangeRate, desc);
			creditNote.AH_DueDate = dueDate;
			creditNote.AH_PostToGL = postToGL ? "Y" : "N";
			return creditNote;
		}

		public APCreditNote CreateAPCreditNoteWithLine(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc, Job job, AccChargeCode chargeCode, decimal aH_OSExTaxAmount, ZDateTime dueDate, ZBool postToGL)
		{
			APCreditNote creditNote = CreateAPCreditNote(creditNoteNumber, account, currency, exchangeRate, desc, dueDate, postToGL);
			CreateAPCreditNoteLine(creditNote, job, chargeCode, currency, exchangeRate, desc, aH_OSExTaxAmount);
			creditNote.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_ReverseToGL = postToGL ? "Y" : "N");

			return creditNote;
		}

		public APCreditNoteLine CreateAPCreditNoteLine(APCreditNote parent, Job job, AccChargeCode chargeCode, RefCurrency currency, decimal exchangeRate, string desc, decimal aH_OSExTaxAmount)
		{
			APCreditNoteLine line = (APCreditNoteLine)parent.Lines.AddNew();

			if (job != null)
			{
				line.AL_JH = job.PK;
			}

			line.AL_AG = GLHeader1.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = chargeCode.PK;
			line.AL_Desc = desc;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			line.AL_AT = chargeCode.GSTRate.PK;
			line.AL_AW = chargeCode.WithholdingTaxRate.PK;
			line.AL_OSTaxAmount = parent.Header.CompanyData.IsAPTaxApplicable ? aH_OSExTaxAmount * (chargeCode.GSTRate.GetRate_ForTestOnly() / 100) : 0M;
			line.AL_OSWHTAmount = parent.Header.MiscServ.OM_APWHTApplicable ? aH_OSExTaxAmount * (chargeCode.WithholdingTaxRate.AW_Rate / 100) : 0M;

			return line;
		}

		public ARCreditNote CreateARCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency = null, decimal? exchangeRate = null, string desc = null)
		{
			ARCreditNote creditNote = Factory.New<ARCreditNote>();

			creditNote.AH_InvoiceDate = ZDateTime.Today;
			creditNote.AH_TransactionNum = creditNoteNumber;
			creditNote.AH_OH = account.PK;
			if (currency != null)
			{
				creditNote.AH_RX_NKTransactionCurrency = currency.RX_Code;
			}
			if (exchangeRate != null)
			{
				creditNote.AH_ExchangeRate = exchangeRate.Value;
			}
			if (desc != null)
			{
				creditNote.AH_Desc = desc;
			}
			creditNote.AH_PostDate = ZDateTime.Today;
			creditNote.AH_DueDate = ZDateTime.Today.AddMonths(1);

			return creditNote;
		}

		public ARCreditNote CreateARCreditNote(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc, ZDateTime dueDate, ZBool postToGL)
		{
			ARCreditNote creditNote = CreateARCreditNote(creditNoteNumber, account, currency, exchangeRate, desc);
			creditNote.AH_DueDate = dueDate;
			creditNote.AH_PostToGL = postToGL ? "Y" : "N";
			return creditNote;
		}

		public ARCreditNote CreateARCreditNote(decimal amount, ZGuid headerPK, ZGuid branchPK, ZGuid departmentPK)
		{
			var creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			creditNote.AH_OH = headerPK;
			creditNote.AH_LocalExTaxAmount = amount;
			creditNote.AH_GB = branchPK;
			creditNote.AH_GE = departmentPK;
			return creditNote;
		}

		public ARCreditNote CreateARCreditNoteWithLine(string creditNoteNumber, OrgHeader account, RefCurrency currency, decimal exchangeRate, string desc, Job job, AccChargeCode chargeCode, decimal aH_OSExTaxAmount, ZDateTime dueDate, ZBool postToGL)
		{
			ARCreditNote creditNote = CreateARCreditNote(creditNoteNumber, account, currency, exchangeRate, desc, dueDate, postToGL);
			CreateARCreditNoteLine(creditNote, job, chargeCode, aH_OSExTaxAmount, currency, exchangeRate, desc);
			creditNote.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_ReverseToGL = postToGL ? "Y" : "N");

			return creditNote;
		}

		public ARCreditNoteLine CreateARCreditNoteLine(ARCreditNote parent, Job job, AccChargeCode chargeCode, decimal aH_OSExTaxAmount, RefCurrency currency = null, decimal? exchangeRate = null, string desc = null)
		{
			ARCreditNoteLine line = (ARCreditNoteLine)parent.Lines.AddNew();

			if (job != null)
			{
				line.AL_JH = job.PK;
			}

			line.GenericCharge = chargeCode.PK;
			if (desc != null)
			{
				line.AL_Desc = desc;
			}
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			if (currency != null)
			{
				line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			}
			if (exchangeRate != null)
			{
				line.AL_ExchangeRate = exchangeRate.Value;
			}
			line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			line.AL_AG = GLHeader1.PK;

			return line;
		}

		public void AddLineToCreditNote(ARCreditNote creditNote, ZGuid jobPK, ZGuid branchPK, ZGuid departmentPK, decimal amount)
		{
			var creditNoteLine = (InvoicingLineBase)creditNote.Lines.AddNew();
			creditNoteLine.AL_JH = jobPK;
			creditNoteLine.AL_AC = FRT.PK;
			creditNoteLine.AL_GB = branchPK;
			creditNoteLine.AL_GE = departmentPK;
			creditNoteLine.AL_RX_NKTransactionCurrency = AUD.RX_Code;
			creditNoteLine.AL_OSAmount = -amount;
			creditNoteLine.AL_LineAmount = -amount;
			creditNoteLine.AL_AG = GLHeader1.PK;
		}

		public ARCreditNote CreateARCreditNoteReverseTransaction(InvoicingBase invoice)
		{
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			creditNote.OriginalTransaction = invoice;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.IsReverseTransaction = true;
			return creditNote;
		}

		#endregion

		#region Gateway

		public static IDisposable SetupGatewayDepartmentDefaultingRegistry(string direction, string transportMode, string consolType, ZGuid deptPK)
		{
			var collection = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentGateway.Value;
			var defaultDept = collection.AddNew();

			defaultDept.Direction = direction;
			defaultDept.TransportMode = transportMode;
			defaultDept.ConsolType = consolType;
			defaultDept.Department = deptPK;

			return AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentGateway.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public static IDisposable SetupDebtorDefaultingRegistry((string consolDirection, string consolTransportMode, string chargeCodeGroup, string paymentTerm, string relatedJob, string prevSA, string debtor) config)
		{
			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = config.consolDirection,
				ConsolTransportMode = config.consolTransportMode,
				ChargeGroup = config.chargeCodeGroup,
				ConsolPaymentTerm = config.paymentTerm,
				RelatedJob = config.relatedJob,
				PreviousSendingAgent = config.prevSA,
				Debtor = config.debtor
			};

			var collection = AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.Value;
			collection.Add(configuration);

			return AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public static IDisposable SetupInvoiceTargeJobDefaultingRegistry((string consolDirection, string consolTransportMode, string previousSendingAgentType, string invoiceTargetJobType) config)
		{
			var configuration = new GatewayChargeDefaultInvoiceTargetJobConfiguration()
			{
				ConsolDirection = config.consolDirection,
				ConsolTransportMode = config.consolTransportMode,
				PreviousSendingAgentType = config.previousSendingAgentType,
				InvoiceTargetJobType = config.invoiceTargetJobType
			};

			var collection = AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.Value;
			collection.Add(configuration);

			return AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a test setup method")]
		public (
			ForwardingConsol gC0001, ForwardingConsol gC0002, ForwardingConsol c0003, ForwardingConsol c0004, ForwardingConsol c0005,
			ForwardingShipment s0001, ForwardingShipment s0002, ForwardingShipment s0003,
			OrgHeader senAg, OrgHeader recAg, OrgHeader shPicAg, OrgHeader shDelAg, OrgHeader prevSenAg)
			CreateGatewayConsolsAndShipments(string gatewayConsolPaymentTerm = Constants.PaymentType.Prepaid)
		{
			ForwardingConsol gC0001, gC0002, c0003, c0004, c0005;
			ForwardingShipment s0001, s0002, s0003;
			OrgHeader senAg, recAg, shPicAg, shDelAg, prevSenAg;

			prevSenAg = DebtorSisterOrgProxy;

			if (gatewayConsolPaymentTerm == Constants.PaymentType.Collect)
			{
				gC0002 = CreateGatewayConsol("SGSIN", "AUSYD", "C0002", receivingGatewayCompany: GlbCompany.CurrentCompany);
				s0001 = CreateShipment("S0001", "SGSIN", "USCHI", gC0002, incoTerm: "CIF", housebill: "S0001");
				s0002 = CreateShipment("S0002", "CNSHA", "USNYC", gC0002, incoTerm: "CIF", housebill: "S0002");
				s0003 = CreateShipment("S0003", "CNSHA", "AUSYD", gC0002, incoTerm: "CIF", housebill: "S0003");

				//			gC0001		gC0002		C0003		C0004
				//	CNSHA	-	SGSIN	-	AUSYD	-	USLAX	-	USCHI
				//												 \
				//													C0005
				//														\
				//															USNYC
				//				|-	-	-	-	-	S0001	-	-	-|
				//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|
				//
				c0004 = CreateConsol("USLAX", "USCHI", "C0004");
				gC0001 = CreateGatewayConsol("CNSHA", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
				gC0001.JK_OA_SendingForwarderAddress = prevSenAg.MainAddress.PK;
				c0003 = CreateConsol("AUSYD", "USLAX", "C0003");
				c0005 = CreateConsol("USLAX", "USNYC", "C0005", voyageFlight: "QF106");
			}
			else
			{
				gC0002 = CreateGatewayConsol("AUSYD", "SGSIN", "C0002", sendingGatewayCompany: GlbCompany.CurrentCompany);
				s0001 = CreateShipment("S0001", "AUSYD", "USLAX", gC0002, incoTerm: "CIF", housebill: "S0001");
				s0002 = CreateShipment("S0002", "AUBNE", "USNYC", gC0002, incoTerm: "CIF", housebill: "S0002");
				s0003 = CreateShipment("S0003", "AUBNE", "SGSIN", gC0002, incoTerm: "CIF", housebill: "S0003");

				//			C0001		gC0002		C0003		C0004
				//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
				//												 \
				//													C0005
				//														\
				//															USNYC
				//				|-	-	-	-	-	S0001	-	-	-|
				//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
				//	|-	-	-	S0003	-	-|
				//
				c0004 = CreateConsol("HKHKG", "USLAX", "C0004");
				gC0001 = CreateGatewayConsol("AUBNE", "AUSYD", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
				gC0001.JK_OA_SendingForwarderAddress = prevSenAg.MainAddress.PK;
				c0003 = CreateConsol("SGSIN", "HKHKG", "C0003");
				c0005 = CreateConsol("HKHKG", "USNYC", "C0005", voyageFlight: "QF106");
			}

			CreateShipment("S0004", "AUMEL", "USLAX");
			gC0002.JK_PrepaidCollect = gatewayConsolPaymentTerm;

			s0001.Consols.Add(c0004);
			s0001.Consols.Add(c0003);
			s0002.Consols.Add(c0005);
			s0002.Consols.Add(c0003);
			s0002.Consols.Add(gC0001);
			s0003.Consols.Add(gC0001);

			senAg = gC0002.SendingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_SendingForwarderAddress = senAg.MainAddress.PK;

			recAg = gC0002.ReceivingForwarder ?? Factory.NewWithValidTestData<OrgHeader>();
			gC0002.JK_OA_ReceivingForwarderAddress = recAg.MainAddress.PK;

			shDelAg = Factory.NewWithValidTestData<OrgHeader>();
			s0002.JS_OH_DeliveryAgent = shDelAg.PK;

			shPicAg = Factory.NewWithValidTestData<OrgHeader>();
			s0002.PickupAgentPK = shPicAg.PK;

			return (gC0001, gC0002, c0003, c0004, c0005, s0001, s0002, s0003, senAg, recAg, shPicAg, shDelAg, prevSenAg);
		}

		(OrgHeader sendingAgent, OrgHeader receivingAgent) GetAgents(GlbCompany sendingGatewayCompany, GlbCompany receivingGatewayCompany, string origin, string destination, string agentStatus)
		{
			return (GetAgent(sendingGatewayCompany, origin), GetAgent(receivingGatewayCompany, destination));

			OrgHeader GetAgent(GlbCompany gatewayCompany, string portOrCountry)
			{
				if (gatewayCompany != null)
				{
					var agent = Factory.Load<OrgHeader>(gatewayCompany.OrgProxy.PK);
					SetupPort(agent, portOrCountry, agentStatus);
					return agent;
				}
				return null;
			}
		}

		internal void SetupPort(OrgHeader agent, string portOrCountry, string agentStatus)
		{
			var appPort = agent.AppointedGatewayAgentPorts.AddNew();
			appPort.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
			appPort.O5_PortOrCountry = portOrCountry;
			appPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort.O5_AirAgentStatus = agentStatus;
			appPort.O5_RailAgentStatus = agentStatus;
			appPort.O5_SeaAgentStatus = agentStatus;
			appPort.O5_RoadAgentStatus = agentStatus;
		}

		public ForwardingConsol CreateGatewayConsol(
			string origin = "AUSYD",
			string destination = "NZAKL",
			string consolNum = "C001",
			GlbCompany sendingGatewayCompany = null,
			GlbCompany receivingGatewayCompany = null,
			OrgHeader sendingGatewayAgent = null,
			OrgHeader receivingGatewayAgent = null,
			string transportMode = "AIR",
			string consolType = Constants.AgentType.Agent,
			string agentStatus = AgentStatusList.Codes.GatewayAgent,
			string voyageFlight = "QF105",
			string prepaidCollect = Constants.PaymentType.Prepaid)
		{
			if (sendingGatewayAgent == null && receivingGatewayAgent == null)
			{
				var agents = GetAgents(sendingGatewayCompany, receivingGatewayCompany, origin, destination, agentStatus);
				sendingGatewayAgent = agents.sendingAgent;
				receivingGatewayAgent = agents.receivingAgent;
			}

			if (sendingGatewayAgent == null && receivingGatewayAgent == null)
			{
				throw new ArgumentException("Please provide either a sending or receiving gateway company.");
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = consolType;
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_UniqueConsignRef = consolNum;
			consol.JK_PrepaidCollect = prepaidCollect;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = voyageFlight;

			var gateway = (IGateway)consol;
			if (sendingGatewayAgent != null)
			{
				consol.JK_OA_SendingForwarderAddress = sendingGatewayAgent.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = agentStatus;
				AccountingTestHelper.AssertEquals("Precondition: consol.IsGateway.", true, gateway.GatewayBillingSupporter.IsGatewayBillingEnabled(sendingGatewayCompany));
			}

			if (receivingGatewayAgent != null)
			{
				consol.JK_OA_ReceivingForwarderAddress = receivingGatewayAgent.MainAddress.PK;
				consol.JK_ReceivingForwarderHandlingType = agentStatus;
				AccountingTestHelper.AssertEquals("Precondition: consol.IsGateway.", true, gateway.GatewayBillingSupporter.IsGatewayBillingEnabled(receivingGatewayCompany));
			}

			return consol;
		}

		#endregion

		#region Forwarding Consol

		public ForwardingConsol CreateConsol(
				string origin = "AUSYD",
				string destination = "NZAKL",
				string consolNum = "C001",
				bool saveIt = true,
				bool enableDataRefreshBus = true,
				string voyageFlight = "QF105",
				string prepaidCollect = Constants.PaymentType.Prepaid,
				string transportMode = "AIR",
				OrgHeader sendingForwarderAddress = null,
				OrgHeader receivingForwarderAddress = null)
		{
			var factory = saveIt ? new BusinessObjectFactory { RefreshEnabled = enableDataRefreshBus } : Factory;
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_UniqueConsignRef = consolNum;
			consol.JK_PrepaidCollect = prepaidCollect;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = voyageFlight;

			if (sendingForwarderAddress != null)
			{
				consol.SetDefaultSendingForwarderAddress(sendingForwarderAddress);
			}
			if (receivingForwarderAddress != null)
			{
				consol.SetDefaultReceivingForwarderAddress(receivingForwarderAddress);
			}

			if (saveIt)
			{
				consol.Factory.Save();
			}

			return saveIt ? Factory.Load<ForwardingConsol>(consol.PK) : consol;
		}

		public ForwardingConsol CreateConsolWithShipmentJobs(
			string consolNum,
			string origin = "AUSYD",
			string destination = "NZAKL",
			IEnumerable<string> shipmentJobNums = null,
			Action<ForwardingShipment> shipmentInvoker = null)
		{
			var consol = CreateConsol(origin, destination, consolNum);
			shipmentJobNums?.ForEach(shipmentJobNum => {
				var shipment = CreateShipment(shipmentJobNum, origin, destination, consol);
				shipmentInvoker?.Invoke(shipment);
			});

			return consol;
		}

		public JobConsolCost CreateConsolCostInNonCurrentCompany(IJobCostingPlugIn consol, AccChargeCode chargeCode, OrgHeader creditor)
		{
			JobConsolCost cost = null;
			using (NonCurrentCompanyBranch.SetAsTemporaryContext())
			{
				cost = CreateConsolCost(consol, chargeCode, creditor);
			}
			return cost;
		}

		public JobConsolCost CreateConsolCost(IJobCostingPlugIn consol, AccChargeCode chargeCode, OrgHeader creditor = null, ApportionmentListing apportionmentListing = null)
		{
			ApportionmentListing listing = apportionmentListing ?? consol.GetApportionments();
			JobConsolCost cost = listing.CostsCollection.TryAddNew();
			if (cost != null)
			{
				cost.E6_AC_ChargeCode = chargeCode.PK;
				if (creditor != null)
				{
					cost.E6_OH_Creditor = creditor.PK;
				}
			}
			return cost;
		}

		public JobConsolCost CreateGatewayConsolCost(ForwardingConsol consol, AccChargeCode chargeCode, OrgHeader creditor = null, ApportionmentListing apportionmentListing = null)
		{
			ApportionmentListing listing = apportionmentListing ?? consol.GetApportionments(true);
			JobConsolCost cost = listing.CostsCollection.TryAddNew();
			if (cost != null)
			{
				cost.E6_AC_ChargeCode = chargeCode.PK;
				if (creditor != null)
				{
					cost.E6_OH_Creditor = creditor.PK;
				}
			}
			cost.ApportionmentCharges.ForEach(x => ((ApportionSplitCharge)x).JR_E6_GatewaySellHeader = cost.PK);
			return cost;
		}

		public void SetupConsolRelatedARInvoice(InvoicingBase arInvoice, ForwardingConsol consol)
		{
			arInvoice.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
		}

		public void SetupConsolRelatedAPInvoice(InvoicingBase apInvoice, ForwardingConsol consol, AccChargeCode chargeCode, ZDecimal amount)
		{
			CreateConsolCost(apInvoice, consol, chargeCode, amount);
			apInvoice.ImportAllApportionmentsFromCosting();
		}

		public JobConsolCost CreateConsolCost(IJobCostingPlugIn consol, AccChargeCode chargeCode, ZDecimal amount, OrgHeader creditor = null, string apportionmentMethod = null,
			ApportionmentListing apportionmentListing = null)
		{
			var cost = CreateConsolCost(consol, chargeCode, creditor, apportionmentListing);
			if (cost != null)
			{
				cost.E6_OSCostAmount = amount;
				if (apportionmentMethod != null)
				{
					cost.E6_ApportionmentMethod = apportionmentMethod;
				}
			}
			return cost;
		}

		public JobConsolCost CreateConsolCost(InvoicingBase parentInvoice, ForwardingConsol consol, AccChargeCode chargeCode, ZDecimal amount, OrgHeader creditor = null)
		{
			var cost = parentInvoice.ConsolCosting.ConsolCosts.AddNew();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_OSCostAmount = amount;
			cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
			if (creditor != null)
			{
				cost.E6_OH_Creditor = creditor.PK;
			}
			return cost;
		}

		public JobConsolCost CreateConsolCost(IJobCostingPlugIn consol, AccChargeCode chargeCode, RefCurrency currency, ZDecimal exchangeRate, ZDecimal amount, OrgHeader creditor = null, string apportionmentMethod = null,
			ApportionmentListing apportionmentListing = null)
		{
			var cost = CreateConsolCost(consol, chargeCode, amount, creditor, apportionmentMethod, apportionmentListing);
			if (cost != null)
			{
				cost.E6_RX_NKCurrency = currency.RX_Code;
				cost.E6_ExchangeRate = exchangeRate;
			}
			return cost;
		}

		public JobConsolCost CreateConsolCost(IJobCostingPlugIn consol, AccChargeCode chargeCode, OrgHeader creditor, ZDecimal oSCostAmount, ZBool apportionToRelatedShipments, string apportionmentMethod = null)
		{
			JobConsolCost cost = CreateConsolCost(consol, chargeCode, oSCostAmount, creditor);
			if (cost != null)
			{
				cost.E6_ApportionToRelatedShipments = apportionToRelatedShipments;
				if (apportionmentMethod != null)
				{
					cost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToList().ForEach(charge => charge.JR_IsUsedForApportionment = true);
					cost.E6_ApportionmentMethod = apportionmentMethod;
				}
			}
			return cost;
		}

		public void SetAPInvoiceInfo(JobConsolCost consolCost, string invoiceNumber, ZDateTime invoiceDate)
		{
			consolCost.E6_InvoiceNum = invoiceNumber;
			consolCost.E6_InvoiceDate = invoiceDate;
		}

		public void SetAPInvoiceInfo(JobConsolCost consolCost, string invoiceNumber, ZDateTime invoiceDate, ZDateTime dueDate)
		{
			SetAPInvoiceInfo(consolCost, invoiceNumber, invoiceDate);
			consolCost.E6_PaymentDate = dueDate;
		}

		#endregion

		#region Shipment Job

		public Job CreateAndSaveTestShipmentJob(ZString jobHeaderStatus, bool createJobWithMutex = true, string departmentCode = null)
		{
			BusinessObjectFactory testFactory = GetNewFactory();
			ForwardingShipment shipment = testFactory.New<ForwardingShipment>();
			Job shipmentJob = createJobWithMutex ? new Job.Loader(testFactory, shipment).TryCreateWithMutex() : new Job.Loader(testFactory, shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.PlugInData = shipment;
			if (string.IsNullOrEmpty(departmentCode))
			{
				shipmentJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).PK;
			}
			else
			{
				// Select a specific Department so that the tests will have correct expected results
				shipmentJob.JH_GE = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)).First(x => string.Compare(departmentCode, x.GE_Code, StringComparison.OrdinalIgnoreCase) == 0).PK;
			}
			shipmentJob.JH_Status = jobHeaderStatus;
			testFactory.Save();

			var jobInCurrentFactory = Factory.Load<Job>(shipmentJob.PK);
			var shipmentInCurrentFactory = Factory.Load<ForwardingShipment>(shipment.PK);
			jobInCurrentFactory.PlugInData = shipmentInCurrentFactory;

			return jobInCurrentFactory;
		}

		public ForwardingShipment CreateAndSaveTestForwardingShipmentJob(ZString jobHeaderStatus, bool createJobWithMutex = true)
		{
			BusinessObjectFactory testFactory = GetNewFactory();
			ForwardingShipment shipment = testFactory.New<ForwardingShipment>();
			Job shipmentJob = createJobWithMutex ? new Job.Loader(testFactory, shipment).TryCreateWithMutex() : new Job.Loader(testFactory, shipment).TryCreateWithoutMutexForTestOnly();
			shipmentJob.PlugInData = shipment;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_Status = jobHeaderStatus;
			testFactory.Save();

			return Factory.Load<ForwardingShipment>(shipment.PK);
		}

		#endregion

		#region Forwarding Shipment

		public ForwardingShipment CreateShipment(string shipmentNum, bool saveIt)
		{
			return CreateShipment(shipmentNum, "", saveIt: saveIt);
		}

		public ForwardingShipment CreateShipment(string shipmentNum, ForwardingConsol consol)
		{
			return CreateShipment(shipmentNum, origin: null, destination: null, consol: consol);
		}

		public ForwardingShipment CreateShipment(string shipmentNum, string origin = null, string destination = null, ForwardingConsol consol = null, bool saveIt = false, string transportMode = null, string incoTerm = null, string housebill = null)
		{
			ForwardingShipment shipment;

			var factory = saveIt ? GetNewFactory() : Factory;
			if (consol != null)
			{
				var consolForShipment = saveIt ? factory.Load<ForwardingConsol>(consol.PK) : consol;
				shipment = consolForShipment.Shipments.AddNew();
			}
			else
			{
				shipment = factory.NewWithValidTestData<ForwardingShipment>();
			}

			SetShipmentDefaultsForTest(shipment, shipmentNum, origin == null && consol != null ? consol.JK_RL_NKLoadPort : (ZString)origin, destination == null && consol != null ? consol.JK_RL_NKDischargePort : (ZString)destination, transportMode ?? consol?.TransportMode, incoTerm, housebill);
			if (saveIt)
			{
				shipment.Factory.Save();
				shipment = Factory.Load<ForwardingShipment>(shipment.PK);
			}
			return shipment;
		}

		public ForwardingShipment CreateShipmentWithCoLoadMaster(string shipmentNum, string origin, string destination, ForwardingShipment coloadMasterShipment)
		{
			var shipment = CreateShipment(shipmentNum, origin, destination);
			coloadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coloadMasterShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			shipment.JS_JS_ColoadMasterShipment = coloadMasterShipment.PK;
			return shipment;
		}

		public ForwardingShipment CreateMasterShipment(string shipmentNum, ForwardingConsol consol)
		{
			var shipment = CreateShipment(shipmentNum, "", "", consol);
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_PackingMode = "BCN";
			shipment.JS_UniqueConsignRef = "Master";

			return shipment;
		}

		public ForwardingShipment CreateShipmentWithCoLoadMaster(string shipmentNum, ForwardingConsol consol, ForwardingShipment coloadMasterShipment)
		{
			var shipment = CreateShipment(shipmentNum, "", "", consol);
			coloadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_JS_ColoadMasterShipment = coloadMasterShipment.PK;

			return shipment;
		}

		public InvoiceLine CreateShipmentChargeJobAndLine(string uniqueNumber, InvoicingBase invoice, OrgHeader debtor, bool createARLine)
		{
			var shipment = CreateShipment(uniqueNumber);
			var job = CreateJob(shipment, false);
			invoice.AH_JH = job.PK;
			var charge = CreateCharge(job, CC1, "CC1", AUD, 1000, Creditor1, uniqueNumber, AUD, 1000, debtor);
			InvoiceLine line = createARLine ? CreateRevenueLine(charge, invoice.PK) : CreateCostLine(charge, invoice.PK);
			return line;
		}

		void SetShipmentDefaultsForTest(ForwardingShipment shipment, string shipmentNum, string origin, string destination, string transportMode = null, string incoTerm = null, string housebill = "UVWXYZ")
		{
			shipment.JS_TransportMode = transportMode ?? "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = incoTerm ?? "FOB";
			shipment.JS_UniqueConsignRef = shipmentNum;
			shipment.JS_HouseBill = housebill;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ActualChargeable = 100M;
		}

		#endregion

		#region Pay invoice by web service

		public void PartPayInvoiceViaWebService(InvoicingBase invoice, ZDecimal amount)
		{
			var description = string.Format("Paid to {0} Outstanding Amount by Transaction Payment Web Service", amount);

			var sql = string.Format(@"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser)
VALUES (NEWID(), 'AccTransactionHeader', '{0}', 'N', '{1}', GETDATE(), 'EDT', GETDATE(), 'ZZ')", invoice.PK, description);

			Db.Connection.ExecuteNonQuery(sql);
		}

		#endregion

		public IJobInvoicingPlugIn CreateJobPlugIn(JobInvoicingConsumerType consumerType)
		{
			return Factory.NewWithValidTestData(consumerType.BizoType) as IJobInvoicingPlugIn;
		}

		#region Cartage

		public CommonCartage CreateCartage()
		{
			return Factory.NewWithValidTestData<CommonCartage>();
		}

		#endregion

		#region Charge

		public Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, decimal osCostAmt, decimal osSellAmt)
		{
			return CreateCharge(parentJob, chargeCode, "Desc", osCostAmt: osCostAmt, osSellAmt: osSellAmt);
		}

		public Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, decimal osCostAmt, OrgHeader creditor, RefCurrency sellCurrency, decimal osSellAmt, OrgHeader debtor)
		{
			return CreateCharge(parentJob, chargeCode, desc, costCurrency, osCostAmt, creditor, null, sellCurrency, osSellAmt, debtor);
		}

		public Charge CreateCharge(Job parentJob, AccChargeCode chargeCode = null, string desc = null, RefCurrency costCurrency = null, decimal osCostAmt = 100m, OrgHeader creditor = null,
			string invoiceNum = null, RefCurrency sellCurrency = null, decimal osSellAmt = 100m, OrgHeader debtor = null, AccBankAccount paymentBankAccount = null)
		{
			Charge charge = parentJob.Charges.AddNew();

			if (charge.JR_GB.IsEmpty)
			{
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
			}
			charge.JR_AC = (chargeCode ?? CC1).PK;
			if (!desc.IsNullOrEmpty())
			{
				charge.JR_Desc = desc;
			}
			charge.JR_OH_CostAccount = creditor == null ? ZGuid.Empty : creditor.PK;
			if (costCurrency != null)
			{
				charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			}
			charge.JR_OSCostAmt = osCostAmt;
			if (!invoiceNum.IsNullOrEmpty())
			{
				charge.JR_APInvoiceNum = invoiceNum;
			}
			if (!charge.JR_APInvoiceNum.IsEmpty)
			{
				charge.JR_APInvoiceDate = ZDateTime.Today;
				charge.JR_PaymentDate = ZDateTime.Today.AddDays(1);
			}
			charge.JR_OH_SellAccount = debtor == null ? Guid.Empty : debtor.PK;
			if (sellCurrency != null)
			{
				charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			}
			charge.JR_OSSellAmt = osSellAmt;
			if (charge.JR_GE.IsEmpty)
			{
				charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			}
			if (charge.Department.GE_Misc)
			{
				charge.Department.GE_Misc = false;
				charge.JR_GE = charge.Department.PK;
			}
			if (charge.JR_OSCostAmt != osCostAmt)
			{
				charge.JR_OSCostAmt = osCostAmt;
			}
			if (charge.JR_OSSellExRate == 1m && charge.JR_LocalSellAmt == 0m)
			{
				charge.JR_LocalSellAmt = osSellAmt;
			}

			if (paymentBankAccount != null)
			{
				charge.JR_PaymentType = ReceiptTypes.Cash;
				charge.JR_AB = paymentBankAccount.PK;
			}

			return charge;
		}

		#endregion

		#region Costings and Client Rates

		public ClientRate CreateFlatCalculatorClientRate(ZString transportMode, ZString packingMode, ZString origin, ZString destination, OrgHeader org, ZString chargeCode, ZDecimal flatCalculatorRate)
		{
			return CreateFlatCalculatorClientRate<ClientRate>(transportMode, packingMode, origin, destination, org, chargeCode, flatCalculatorRate);
		}

		public Costing CreateFlatCalculatorCosting(ZString transportMode, ZString packingMode, ZString origin, ZString destination, OrgHeader org, ZString chargeCode, ZDecimal flatCalculatorRate)
		{
			return CreateFlatCalculatorClientRate<Costing>(transportMode, packingMode, origin, destination, org, chargeCode, flatCalculatorRate);
		}

		T CreateFlatCalculatorClientRate<T>(ZString transportMode, ZString packingMode, ZString origin, ZString destination, OrgHeader org, ZString chargeCode, ZDecimal flatCalculatorRate) where T : RatingHeader
		{
			T clientRate = Factory.New<T>();

			RateEntry rateEntry = clientRate.AddRateEntry(transportMode, packingMode, origin, destination);
			rateEntry.Parent.TH_OH = org != null ? org.PK : ZGuid.Empty;
			rateEntry.RateLines.RemoveAndDeleteAll();
			RateLine rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = flatCalculatorRate;

			return clientRate;
		}

		#endregion

		#region Create JobPaymentBasis

		public void CreateJobPaymentBasis(Guid? jobChargePK, Guid? consolCostPK, bool isCost, string adapterType, string operationalJobCode, decimal chargeableAmount = 0, string chargeableUnit = "KG", string chargeableUnitType = "Weight", string rateCurrency = "AUD")
		{
			var jobPaymentBasis = Factory.NewWithValidTestData<JobPaymentBasis>();
			jobPaymentBasis.PBS_JR = jobChargePK ?? Guid.Empty;
			jobPaymentBasis.PBS_E6 = consolCostPK ?? Guid.Empty;
			jobPaymentBasis.PBS_IsCost = isCost;
			jobPaymentBasis.PBS_AdapterType = adapterType;
			jobPaymentBasis.PBS_AdapterID = operationalJobCode;
			jobPaymentBasis.PBS_ChargeableAmount = chargeableAmount;
			jobPaymentBasis.PBS_ChargeableUnit = chargeableUnit;
			jobPaymentBasis.PBS_ChargeableUnitType = chargeableUnitType;
			jobPaymentBasis.PBS_RX_NKRateCurrency = rateCurrency;
		}

		#endregion

		#region Data Export Batch

		public IDataExportBatchSource CreateDataExportBatchSource_TransactionHeader()
		{
			DataExportBatchSource_TransactionHeader header = Factory.New<DataExportBatchSource_TransactionHeader>();
			// AccTransactionHeader is not valid with empty AH_Ledger.
			header.AH_Ledger = header.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : header.AH_Ledger;
			return header;
		}

		public IDataExportBatchSource CreateDataExportBatchSource_TransactionLine()
		{
			var line = Factory.New<DataExportBatchSource_TransactionLine>();
			line.AL_AG = GLHeader1.PK;
			return line;
		}

		public IDataExportBatchSource CreateDataExportBatchSource_TransactionLine(TransactionHeader header)
		{
			var line = Factory.New<DataExportBatchSource_TransactionLine>();
			line.AL_AH = header.PK;
			line.AL_AG = GLHeader1.PK;
			return line;
		}

		class DataExportBatchSource_TransactionHeader : TransactionHeader
		{
			public DataExportBatchSource_TransactionHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			protected override bool InvertSigns
			{
				get { return false; }
			}
			protected override ZString TransactionType
			{
				get { return TransactionTypes.Invoice; }
			}
			protected override ZString Ledger
			{
				get { return LedgerTypes.AccountsReceivable; }
			}
			protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
			{
				get { return AccountingNumberFountainWrapperFactory.Instance.ARInvoiceNo; }
			}
		}

		class DataExportBatchSource_TransactionLine : TransactionLine
		{
			public DataExportBatchSource_TransactionLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString LineType
			{
				get { return TransactionLineTypes.Accrual; }
			}

			protected override bool InvertSigns
			{
				get { return false; }
			}
		}

		public GenExportBatchSequence CreateDataExportBatchForHeader(IDataExportBatchSource parent)
		{
			var batch = CreateDataExportBatch(parent);
			batch.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
			batch.XB_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			return batch;
		}

		public GenExportBatchSequence CreateDataExportBatchForLine(IDataExportBatchSource parent)
		{
			var batch = CreateDataExportBatch(parent);
			batch.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport;
			batch.XB_ParentTableCode = AccTransactionLinesSchema.Constants.Prefix;
			return batch;
		}

		GenExportBatchSequence CreateDataExportBatch(IDataExportBatchSource parent)
		{
			var batch = Factory.New<GenExportBatchSequence>();
			// GenExportBatchSequence is not valid with empty XB_Type.
			batch.XB_Type = batch.XB_Type.IsEmpty ? (ZString)Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport : batch.XB_Type;
			batch.XB_BatchNumber = 100;
			batch.XB_ParentID = ((BusinessObject)parent).PK;
			batch.XB_Sequence = 0;
			return batch;
		}

		#endregion

		#region Hot Cheque

		public AccHotCheque CreateHotCheque(Job chequeJob, string payee, ZDecimal amount, ZString description)
		{
			AccHotCheque hotCheque = Factory.New<AccHotCheque>();

			hotCheque.AQ_ChequeDate = ZDateTime.Now;
			hotCheque.AQ_OH = Creditor1.PK;
			hotCheque.AQ_AK = AUDChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "1";
			hotCheque.AQ_ChequePayee = payee;
			hotCheque.AQ_Amount = amount;
			hotCheque.AQ_JH = chequeJob.PK;
			hotCheque.AQ_Description = description;
			hotCheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;

			return hotCheque;
		}

		#endregion

		#region Compliance Document

		public AccComplianceDocumentHeader CreateComplianceDocumentHeader(ZString ledger, ZString desc, ZString subType, string transactionType = "INV", OrgHeader organisation = null, string documentNumber = null, AccComplianceSequence complianceSequence = null)
		{
			AccComplianceDocumentHeader header = null;
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
					break;
				case LedgerTypes.AccountsReceivable:
					header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
					break;
				default:
					return header;
			}

			header.ADH_TransactionType = transactionType;
			header.ADH_Description = desc;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_ComplianceSubType = subType;
			if (organisation != null)
			{
				header.ADH_OH_Organisation = organisation.PK;
			}
			if (documentNumber != null)
			{
				header.ADH_DocumentNumber = documentNumber;
			}
			if (complianceSequence != null)
			{
				header.ADH_XD_ComplianceBook = complianceSequence.PK;
			}
			return header;
		}

		public AccComplianceDocumentLine CreateComplianceDocumentLine(AccComplianceDocumentHeader header, ZString desc, int sequence = 1)
		{
			var line = Factory.New<AccComplianceDocumentLine>();
			line.ADL_ADH = header.PK;
			line.ADL_Description = desc;
			line.ADL_Sequence = sequence;
			return line;
		}

		public AccComplianceDocumentPivot CreateComplianceDocumentPivot(AccComplianceDocumentLine complianceLine, AccTransactionLines transactionLine)
		{
			var pivot = Factory.New<AccComplianceDocumentPivot>();
			pivot.ADP_ADL = complianceLine.PK;
			pivot.ADP_AL = transactionLine.PK;
			return pivot;
		}

		public AccComplianceDocumentHeader CreateComplianceDocumentHeaderWithLine(ZString ledger, ZString desc, ZString documentNumber, ZString subType, ZString lineDesc, AccTransactionLines transactionLine, OrgHeader organisation = null, AccComplianceSequence complianceSequence = null)
		{
			var header = CreateComplianceDocumentHeader(ledger, desc, subType, transactionLine.TransactionHeader?.AH_TransactionType, organisation, documentNumber, complianceSequence);
			var line = CreateComplianceDocumentLine(header, lineDesc);
			CreateComplianceDocumentPivot(line, transactionLine);

			return header;
		}

		#endregion

		#region Compliance Invoice subtype

		public static string[] CountriesSupportComplianceSubtype => new string[]
		{
			CountryCodes.Peru,
			CountryCodes.Indonesia,
			CountryCodes.VietNam,
			CountryCodes.China,
			CountryCodes.Poland,
			CountryCodes.Mexico,
			CountryCodes.Chile,
			CountryCodes.Argentina,
			CountryCodes.Ecuador,
			CountryCodes.CostaRica,
			CountryCodes.Guatemala,
			CountryCodes.Honduras,
			CountryCodes.ElSalvador,
			CountryCodes.Italy
		};

		public ARInvoice SetupComplianceInvoice(ZString subType, bool isForceSave = true, ZDateTime? invoiceAndPostDate = null, string complianceNumber = "")
		{
			ARInvoice complianceInv = Factory.NewWithValidTestData<ARInvoice>();
			complianceInv.AH_ComplianceSubType = subType;
			complianceInv.AH_TransactionReference = complianceNumber;
			complianceInv.AH_InvoicePrinted = false;
			if (invoiceAndPostDate.HasValue)
			{
				complianceInv.AH_PostDate = complianceInv.AH_InvoiceDate = invoiceAndPostDate.Value;
			}
			OrgHeader chinaOrg = Factory.NewWithValidTestData<OrgHeader>();
			chinaOrg.OH_RL_NKClosestPort = "CNSHA";
			complianceInv.AH_OH = chinaOrg.PK;
			if (isForceSave)
			{
				Factory.Save();
			}
			return complianceInv;
		}

		public ZGuid SetupComplianceMenuAndPivot(ZString menuName)
		{
			StmTemplate classATemplate = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, "Class A Invoice Preprinted"));

			StmTemplate template = Factory.NewWithValidTestData<StmTemplate>();
			template.SO_DataContext = "ARInvoice";
			template.SO_Template = classATemplate.SO_Template;

			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = menuName;
			menu.SU_BusinessContext = "ARInvoice";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "A/R";

			StmMenuTemplatePivot pivot = Factory.NewWithValidTestData<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menu.PK;
			return menu.PK;
		}

		public AccComplianceSequence SetupComplianceSequence(ZGuid menuPK, ZString sequenceClass, ZString prefix, ZDecimal startNo, ZDecimal endNo, ZDecimal nextNo, string allocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch)
		{
			AccComplianceSequence sequence = CreateNewComplianceSequence(menuPK, sequenceClass, startNo, endNo, nextNo);
			sequence.XD_AllocationLevel = allocationLevel;

			if (allocationLevel == Core.Constants.ComplianceBookAllocationLevel.Branch)
			{
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			}
			else if (allocationLevel == Core.Constants.ComplianceBookAllocationLevel.BranchDepartment)
			{
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			}

			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_Prefix = prefix;

			Factory.Save();

			return sequence;
		}

		public AccComplianceSequence SetupComplianceSequence(ZGuid templatePK, ZString sequenceClass, ZString prefix, ZDecimal startNo, ZDecimal endNo, ZDecimal nextNo, ZGuid companyPK, ZGuid branchPK)
		{
			AccComplianceSequence sequence = CreateNewComplianceSequence(templatePK, sequenceClass, startNo, endNo, nextNo);
			sequence.XD_GB_BranchOwner = branchPK;
			sequence.XD_GC_Company = companyPK;
			sequence.XD_Prefix = prefix;
			Factory.Save();
			return sequence;
		}

		public AccComplianceSequence SetupComplianceSequence(ZGuid templatePK, ZString sequenceClass, ZString prefix, ZDecimal startNo, ZDecimal endNo, ZDecimal nextNo, ZGuid companyPK, ZGuid branchPK, ZDate startDate, ZDate expiryDate)
		{
			AccComplianceSequence sequence = CreateNewComplianceSequence(templatePK, sequenceClass, startNo, endNo, nextNo);
			sequence.XD_GB_BranchOwner = branchPK;
			sequence.XD_GC_Company = companyPK;
			sequence.XD_Prefix = prefix;
			sequence.XD_StartDate = startDate;
			sequence.XD_ExpiryDate = expiryDate;
			Factory.Save();
			return sequence;
		}

		public AccComplianceSequence CreateNewComplianceSequence(ZGuid menuPK, ZString sequenceClass, ZDecimal startNo, ZDecimal endNo, ZDecimal nextNo)
		{
			AccComplianceSequence sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_Code = "LM1";
			//sequence.XD_Prefix = "ABC";
			sequence.XD_StartNumber = startNo;
			sequence.XD_EndNumber = endNo;
			sequence.XD_NextNumber = nextNo;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_SU_MenuItem = menuPK;
			sequence.XD_SQ_DocumentPrintQueue = ZGuid.Empty;
			sequence.XD_IsActive = true;

			//BusinessObject PrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			//sequence.XD_SQ_DocumentPrintQueue = PrintQueue.PK;

			return sequence;
		}

		public void CreateNewComplianceSequence(ZGuid companyPK, ZGuid branchPK, string subType)
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = companyPK;
			complianceSequence.XD_GB_BranchOwner = branchPK;
			complianceSequence.XD_Code = subType;
			complianceSequence.XD_SequenceClass = subType;
			complianceSequence.XD_Prefix = subType;
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			Factory.Save();
		}

		#endregion

		#region Period Management

		public PeriodManager CreateTestPeriods(ZDateTime startDateOfFinancialYear, string periodFormat = null, bool reopenPeriods = false)
		{
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = startDateOfFinancialYear;
			if (periodFormat != null)
			{
				testPeriodSettings.PeriodFormat = periodFormat;
			}

			var testManager = new PeriodManager(Factory);

			if (reopenPeriods)
			{
				AccPeriodManagement periodManagement = null;
				CreatePeriodIfNotExist(testManager, testPeriodSettings, out periodManagement);
				if (periodManagement != null)
				{
					ReopenPeriod(periodManagement);
				}
			}
			else
			{
				testManager.CreatePeriodData(testPeriodSettings, Factory);
			}

			return testManager;
		}

		public void DeleteAllPeriodsForCurrentCompany()
		{
			var testManager = new PeriodManager(Factory);
			testManager.Periods.RemoveAndDeleteAll();
			Factory.Save();
		}

		void CreatePeriodIfNotExist(PeriodManager periodManager, NewYearPeriodSettings periodSettings, out AccPeriodManagement periodManagement)
		{
			var calculator = new AccountingPeriodCalculator(Factory);
			periodManagement = calculator.GetPeriodManagementFromDate(ZDateTime.Today);

			if (periodManagement == null)
			{
				periodManager.CreatePeriodData(periodSettings, Factory);
				periodManagement = calculator.GetPeriodManagementFromDate(ZDateTime.Today);
			}
		}

		void ReopenPeriod(AccPeriodManagement periodManagement)
		{
			if (periodManagement.AM_IsSubLedgerClosed)
			{
				periodManagement.AM_IsSubLedgerClosed = false;
				periodManagement.Factory.Save();
			}

			if (periodManagement.AM_IsGeneralLedgerClosed)
			{
				periodManagement.AM_IsGeneralLedgerClosed = false;
				periodManagement.Factory.Save();
			}
		}

		public void CreateTestPeriodsForEntireYear(int year)
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		public void CreateTestPeriodsForEntireYear(GlbCompany company, int year)
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(year, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		#endregion

		#region Apportionment Template

		public void CreateTemplateLine(AccApportionmentTemplate template, ZString description, ZGuid branch, ZGuid department, ZDecimal percentage)
		{
			AccApportionmentTemplateLines line = template.Lines.AddNew();
			line.Y0_A0 = template.PK;
			line.Y0_Description = description;
			line.Y0_GB = branch;
			line.Y0_GE = department;
			line.Y0_Percentage = percentage;
		}

		public AccApportionmentTemplate CreateSameCompanyApportionmentTemplate()
		{
			AccApportionmentTemplate template = Factory.New<AccApportionmentTemplate>();
			template.A0_Description = "Description";
			template.A0_GC = GlbCompany.CurrentCompany.PK;
			template.A0_Notes = "Notes";

			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department1 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department1.PK);
			GlbDepartment department2 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department2.PK);
			GlbDepartment department3 = Factory.LoadTop1<GlbDepartment>(query);

			CreateTemplateLine(template, "Line 1", GlbBranch.CurrentBranch.PK, department1.PK, 33.333m);
			CreateTemplateLine(template, "Line 2", GlbBranch.CurrentBranch.PK, department2.PK, 33.333m);
			CreateTemplateLine(template, "Line 3", GlbBranch.CurrentBranch.PK, department3.PK, 33.334m);

			return template;
		}

		public AccApportionmentTemplate CreateIntercompanyApportionmentTemplate(ZGuid intercompanyBranch)
		{
			AccApportionmentTemplate template = Factory.New<AccApportionmentTemplate>();
			template.A0_Description = "Description";
			template.A0_GC = GlbCompany.CurrentCompany.PK;
			template.A0_Notes = "Notes";

			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department1 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department1.PK);
			GlbDepartment department2 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department2.PK);
			GlbDepartment department3 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department3.PK);
			GlbDepartment department4 = Factory.LoadTop1<GlbDepartment>(query);

			CreateTemplateLine(template, "Line 1", GlbBranch.CurrentBranch.PK, department1.PK, 33.333m);
			CreateTemplateLine(template, "Line 2", GlbBranch.CurrentBranch.PK, department2.PK, 33.333m);
			CreateTemplateLine(template, "Line 3", intercompanyBranch, department3.PK, 23.334m);
			CreateTemplateLine(template, "Line 4", intercompanyBranch, department4.PK, 10.000m);

			return template;
		}

		#endregion

		#region Registry

		#region Intercompany Clearing Configuration

		public void SetupIntercompanyClearingConfigurationRegistry(AccGLHeader headerCurrentCompany, AccGLHeader headerIntercompany, ZGuid nonCurrentCompany)
		{
			IntercompanyClearingConfigurationCollection collection = new IntercompanyClearingConfigurationCollection();
			collection.AddDefaultValues(Guid.Empty);
			GlbCompany company = Factory.Load<GlbCompany>(nonCurrentCompany);
			foreach (IntercompanyClearingConfiguration config in collection)
			{
				if (config.Company == GlbCompany.CurrentCompany.GC_Code)
				{
					config.ClearingGLAccount = headerCurrentCompany.PK;
				}
				else if (config.Company == company.GC_Code)
				{
					config.ClearingGLAccount = headerIntercompany.PK;
				}
			}
			//AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.SetValue(nonCurrentCompany.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		#endregion

		#region Back Date Invoices Configuration

		public void AddInvoiceDateConfiguration(BackDateInvoicesConfiguration backDateInvoicesConfiguration, string jobType, string direction, string mode, string broker,
			string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod, bool @override, bool today, string reversalRule = InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules)
		{
			InvoiceDateConfiguration config = backDateInvoicesConfiguration.InvoiceDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
			config.Override = @override;
			config.Today = today;
			config.ReversalRule = reversalRule;
		}

		public void AddPostDateConfiguration(BackDateAPInvoicesConfiguration backDateAPInvoicesConfiguration, string jobType, string direction, string mode, string broker,
							string significantDateCode, string priorClosedPeriod, string priorOpenPeriod, string currentPeriod, string futurePeriod,
							string reversalRule = PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules)
		{
			PostDateConfiguration config = backDateAPInvoicesConfiguration.PostDateConfigurationCollection.AddNew();
			config.JobType = jobType;
			config.DirectionCode = direction;
			config.Mode = mode;
			config.BrokerCode = broker;
			config.SignificantDateCode = significantDateCode;
			config.PriorClosedPeriod = priorClosedPeriod;
			config.PriorOpenPeriod = priorOpenPeriod;
			config.CurrentPeriod = currentPeriod;
			config.FuturePeriod = futurePeriod;
			config.ReversalRule = reversalRule;
		}

		#endregion

		#region PaymentAuthorisationSettings

		public void SetBranchDepartmentAuthorizationLevelSettings(ZGuid branchPK, ZGuid departmentPK, ZDecimal amount1, ZDecimal amount2, bool isTestingReversalRegistry = false, string mode = "")
		{
			var setting = CreateAuthorizationModeAndSettings(amount1, amount2, mode);
			if (isTestingReversalRegistry)
			{
				AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, branchPK.ToGuid(), departmentPK.ToGuid(), setting);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchPK.ToGuid(), departmentPK.ToGuid(), setting);
			}
		}

		public AuthorizationModeAndSettings CreateAuthorizationModeAndSettings(ZDecimal amount1, ZDecimal amount2, string mode = "")
		{
			var result = new AuthorizationModeAndSettings();
			var collection = result.AuthorisationSettings;
			var settings1 = collection.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = amount1;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			var settings2 = collection.AddNew();
			settings2.Range = RangeCodes.UpTo;
			settings2.Amount = amount2;
			settings2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			var settings3 = collection.AddNew();
			settings3.Range = RangeCodes.Above;
			settings3.Amount = amount2;
			settings3.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;

			if (!string.IsNullOrWhiteSpace(mode))
			{
				result.AuthorizationMode = mode;
			}

			return result;
		}

		public PaymentAuthorisationSettingsCollection CreatePaymentAuthorisationSettingsEmpty()
		{
			return new PaymentAuthorisationSettingsCollection();
		}

		public PaymentAuthorisationSettingsCollection CreatePaymentAuthorisationSettings(ZString range, ZDecimal amount, ZString authorizationRequirement)
		{
			var collection = new PaymentAuthorisationSettingsCollection();
			AddPaymentAuthorisationSetting(collection, range, amount, authorizationRequirement);
			return collection;
		}

		public PaymentAuthorisationSettingsCollection CreatePaymentAuthorisationSettingsExample()
		{
			var collection = new PaymentAuthorisationSettingsCollection();
			AddPaymentAuthorisationSetting(collection, PaymentAuthorisationSettings.RangeCodes.UpTo, 1000M, PaymentAuthorisationSettings.AuthorisationRequirementCodes.NoApprovalRequired);
			AddPaymentAuthorisationSetting(collection, PaymentAuthorisationSettings.RangeCodes.UpTo, 2000M, PaymentAuthorisationSettings.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			AddPaymentAuthorisationSetting(collection, PaymentAuthorisationSettings.RangeCodes.UpTo, 3000M, PaymentAuthorisationSettings.AuthorisationRequirementCodes.FirstAndSecondApprovalRequired);
			AddPaymentAuthorisationSetting(collection, PaymentAuthorisationSettings.RangeCodes.Above, 3000M, PaymentAuthorisationSettings.AuthorisationRequirementCodes.AllThreeApprovalRequired);
			return collection;
		}

		public void AddPaymentAuthorisationSetting(PaymentAuthorisationSettingsCollection collection, ZString range, ZDecimal amount, ZString authorizationRequirement)
		{
			PaymentAuthorisationSettings abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = range;
			abovePaymentAuthorisationSettings.Amount = amount;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = authorizationRequirement;
		}

		#endregion

		#region SetControlAccountsForGenerateJournalEntriesStartDate

		public void SetControlAccountsForGenerateJournalEntriesStartDate()
		{
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader1.PK.ToGuid());
		}

		#endregion

		#endregion

		#region Loaders

		public AccChargeCode LoadAccChargeCode(ZString chargeCode, ZGuid companyPK)
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_GC, companyPK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			return Factory.LoadTop1<AccChargeCode>(query);
		}

		#endregion

		#region Payment Approval

		public PaymentApprovalBase CreatePaymentApproval(Type type, string receiptType, AccBankAccount bankAccount, AccChequeBook chequeBook)
		{
			var payment = (PaymentApprovalBase)Factory.NewWithValidTestData(type);
			payment.AV_PaymentType = receiptType;
			payment.AV_AB = bankAccount.PK;
			payment.AV_AK = chequeBook.PK;
			return payment;
		}

		public APPaymentApprovalWithAuthorisation CreatePaymentApproval(string receiptType, AccBankAccount bankAccount, AccChequeBook chequeBook)
		{
			APPaymentApprovalWithAuthorisation payment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment.AV_PaymentType = receiptType;
			payment.AV_AB = bankAccount.PK;
			payment.AV_AK = chequeBook.PK;

			return payment;
		}

		public PaymentApprovalItem CreatePaymentApprovalItem(APPaymentApprovalWithAuthorisation payment, InvoicingBase invoice)
		{
			PaymentApprovalItem paymentItem = Factory.NewWithValidTestData<PaymentApprovalItem>();
			paymentItem.A2_AH = invoice.PK;
			paymentItem.A2_AV = payment.PK;

			return paymentItem;
		}

		public void FillPaymentMatchTransactions(ZString transactionNumber, Type invoiceType, OrgHeader org, PaymentApprovalBase paymentApproval, ZDecimal amount)
		{
			var apInv = CreateInvoiceWithLine(invoiceType, transactionNumber, AUD, 1m, amount, 0M, amount, 0M, org, CC1.PK, "FIN");
			apInv.Lines[0].GenericCharge = GLHeader1.PK;
			apInv.Lines[0].AL_Desc = "test";
			var invoicesToMatch = new BusinessObject[] { apInv };
			paymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(invoicesToMatch);
			paymentApproval.MatchingBaseObject.MatchAndClearTransactions();
		}

		#endregion

		#region Job Declaration

		public IntegrationCustoms.IBaseJobDeclaration CreateDeclaration(string declarationNumber = null)
		{
			var declaration = Factory.New<IntegrationCustoms.IBaseJobDeclaration>();
			if (declarationNumber != null)
			{
				declaration.JE_DeclarationReference = declarationNumber;
			}

			return declaration;
		}

		public IntegrationCustoms.AU.ICusContainer CreateContainer(IntegrationCustoms.IBaseJobDeclaration declaration, string containerNumber, string containerMode, ZGuid containerTypeGuid, string sealNumber)
		{
			var container = CreateContainer(declaration, containerNumber);
			var containerAsBizo = (BusinessObject)container;
			containerAsBizo[CusContainerSchema.CO_FCL_LCL_AIR] = containerMode;
			containerAsBizo[CusContainerSchema.CO_RC] = containerTypeGuid;
			containerAsBizo[CusContainerSchema.CO_Seal] = sealNumber;

			return container;
		}

		public IntegrationCustoms.AU.ICusContainer CreateContainer(IntegrationCustoms.IBaseJobDeclaration declaration, string containerNumber)
		{
			var container = Factory.New<IntegrationCustoms.AU.ICusContainer>();
			var containerAsBizo = (BusinessObject)container;
			containerAsBizo[CusContainerSchema.CO_JE] = declaration.PK;
			containerAsBizo[CusContainerSchema.CO_ContainerNumber] = containerNumber;

			return container;
		}

		#endregion

		#region ISF

		public IntegrationCustoms.US.ISF.ICusISFHeader CreateISFHeader(ZString entryType, string jobReferenceNumber = null, OrgHeader importer = null)
		{
			var isfHeader = Factory.New<IntegrationCustoms.US.ISF.ICusISFHeader>();
			if (jobReferenceNumber != null)
			{
				isfHeader.BF_JobReference = jobReferenceNumber;
			}

			if (!entryType.IsEmpty)
			{
				isfHeader.BF_EntryType = entryType;
			}

			if (importer != null)
			{
				isfHeader.BF_OH_Importer = importer.PK;
			}

			return isfHeader;
		}

		#endregion

		#region ProfitShare

		public OrgAgentRelationship SetupProfitShareRelationship(OrgHeader sendingAgent, OrgHeader receivingAgent, decimal sendingProfitSharePercentage, decimal receivingProfitSharePercentage, ZString origin, ZString destination, ZString transportMode)
		{
			var agentRelationship = CreateAgentRelationship(sendingAgent, receivingAgent);
			CreateProfitShare(agentRelationship, sendingProfitSharePercentage, receivingProfitSharePercentage, origin, destination, transportMode);

			return agentRelationship;
		}

		public OrgAgentRelationship CreateAgentRelationship(OrgHeader sendingAgent, OrgHeader receivingAgent, string profitShareType = null)
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			if (!string.IsNullOrEmpty(profitShareType))
			{
				agentRelationship.O3_ProfitShareType = profitShareType;
			}

			// A new OrgAgentRelationship has [O3_OH_SendingAgent = GlbCompany.CurrentCompany.GC_OH_OrgProxy] by default.
			agentRelationship.O3_OH_SendingAgent = sendingAgent?.PK ?? ZGuid.Empty;

			if (receivingAgent != null)
			{
				agentRelationship.O3_OH_ReceivingAgent = receivingAgent.PK;
			}

			return agentRelationship;
		}

		public OrgProfitShareDetails CreateProfitShare(
			OrgAgentRelationship agentRelationship,
			decimal sendingProfitSharePercentage,
			decimal receivingProfitSharePercentage,
			ZString origin, ZString destination,
			ZString transportMode,
			OrgHeader orgOverride = null,
			string orgType = null,
			string jobType = null,
			string gatewayAgentType = null)
		{
			var profitShare = CreateProfitShare(agentRelationship, origin, destination, transportMode, orgOverride, orgType, jobType, gatewayAgentType);

			var sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = sendingProfitSharePercentage;

			var rcvParty = profitShare.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = receivingProfitSharePercentage;

			return profitShare;
		}

		public OrgProfitShareDetails CreateGatewayProfitShareRedistribution(
			OrgAgentRelationship agentRelationship,
			decimal pickupProfitSharePercentage,
			decimal deliveryProfitSharePercentage,
			ZString origin, ZString destination,
			ZString transportMode,
			OrgHeader orgOverride = null,
			string orgType = null,
			string gatewayAgentType = null,
			string apportionmentMethod = null)
		{
			Argument.NotNullOrEmpty(apportionmentMethod, nameof(apportionmentMethod));

			var profitShare = CreateProfitShare(agentRelationship, origin, destination, transportMode, orgOverride, orgType, "GCN", gatewayAgentType, apportionmentMethod);

			var pickupParty = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			pickupParty.PS_PartyType = "SPA";
			pickupParty.PS_PartyProfitSharePercent = pickupProfitSharePercentage;

			var deliveryParty = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			deliveryParty.PS_PartyType = "SDA";
			deliveryParty.PS_PartyProfitSharePercent = deliveryProfitSharePercentage;

			return profitShare;
		}

		public OrgProfitShareDetails CreateProfitShare(
			OrgAgentRelationship agentRelationship,
			ZString origin, ZString destination,
			ZString transportMode,
			OrgHeader orgOverride = null,
			string orgType = null,
			string jobType = null,
			string gatewayAgentType = null,
			string apportionmentMethod = null)
		{
			var profitShare = agentRelationship.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = transportMode;
			profitShare.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare.O4_SendingPortOrCountry = origin;
			profitShare.O4_ReceivingPortOrCountry = destination;
			if (orgOverride != null)
			{
				profitShare.O4_OH_OrgOverride = orgOverride.PK;
				if (!string.IsNullOrEmpty(orgType))
				{
					profitShare.O4_OrgOverrideType = orgType;
				}
			}

			profitShare.O4_JobType = jobType;
			profitShare.O4_GatewayAgentType = gatewayAgentType;
			profitShare.O4_GatewayProfitApportionmentMethod = apportionmentMethod;

			return profitShare;
		}

		#endregion

		#region Test Objects

		#region Job1

		Job fJob1;
		public Job Job1
		{
			get
			{
				if (fJob1 == null)
				{
					fJob1 = CreateJob("Z00001000", LocalClient, 1M, Agent, 2M);
				}
				return fJob1;
			}
		}

		#endregion

		#region Job2

		Job fJob2;
		public Job Job2
		{
			get
			{
				if (fJob2 == null)
				{
					fJob2 = CreateJob("Z00001001", LocalClient, 1M, Agent, 2M);
				}
				return fJob2;
			}
		}

		#endregion

		#region AUD

		RefCurrency fAUD;
		public RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region TRY

		RefCurrency fTRY;
		public RefCurrency TRY => fTRY ?? (fTRY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "TRY"));

		#endregion

		#region EUR

		RefCurrency fEuro;
		public RefCurrency EUR
		{
			get
			{
				if (fEuro == null)
				{
					fEuro = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
				}
				return fEuro;
			}
		}

		#endregion

		#region AED

		RefCurrency fAED;
		public RefCurrency AED
		{
			get
			{
				if (fAED == null)
				{
					fAED = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AED");
				}
				return fAED;
			}
		}

		#endregion

		#region EGP

		public RefCurrency EGP => egp ?? (egp = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Egypt));
		RefCurrency egp;

		#endregion

		#region KRW

		public RefCurrency KRW => krw ?? (krw = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf));
		RefCurrency krw;

		#endregion

		#region SAR

		public RefCurrency SAR => sar ?? (sar = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SaudiArabia));
		RefCurrency sar;

		#endregion

		#region CC1

		AccChargeCode fCC1;
		public AccChargeCode CC1
		{
			get
			{
				if (fCC1 == null || fCC1.AC_GC != GlbCompany.CurrentCompany.PK)
				{
					fCC1 = CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, GST1, WHTFREE1);
				}
				return fCC1;
			}
		}

		#endregion

		#region CC2

		AccChargeCode fCC2;
		public AccChargeCode CC2
		{
			get
			{
				if (fCC2 == null)
				{
					fCC2 = CreateChargeCode("CC2", "Charge Code 2", Core.Constants.ChargeType.Disbursement, 100, GST1, WHT1);
				}
				return fCC2;
			}
		}

		#endregion

		#region CC3

		AccChargeCode fCC3;
		public AccChargeCode CC3
		{
			get
			{
				if (fCC3 == null)
				{
					fCC3 = CreateChargeCode("CC3", "Charge Code 3", Core.Constants.ChargeType.Margin, 0, GSTFREE1, WHT1);
				}
				return fCC3;
			}
		}

		#endregion

		#region CC4

		AccChargeCode fCC4;
		public AccChargeCode CC4
		{
			get { return fCC4 ?? (fCC4 = CreateChargeCode("CC4", "Charge Code 4", Constants.ChargeType.Margin, 100, GSTFREE1, WHTFREE1)); }
		}

		#endregion

		#region CC5

		AccChargeCode fCC5;
		public AccChargeCode CC5
		{
			get
			{
				if (fCC5 == null)
				{
					fCC5 = CreateChargeCode("CC5", "Charge Code 5", Core.Constants.ChargeType.Margin, 100, GST1, WHTFREE1);
				}
				return fCC5;
			}
		}

		#endregion

		#region CC6

		AccChargeCode fCC6;
		public AccChargeCode CC6
		{
			get
			{
				if (fCC6 == null)
				{
					fCC6 = CreateChargeCode("CC6", "Charge Code 6", Core.Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fCC6;
			}
		}

		#endregion

		#region CC7

		AccChargeCode fCC7;
		public AccChargeCode CC7
		{
			get
			{
				if (fCC7 == null)
				{
					fCC7 = CreateChargeCode("CC7", "Charge Code 7", Core.Constants.ChargeType.Margin, 100, GSTFREE1, WHTFREE1);
				}
				return fCC7;
			}
		}

		#endregion

		#region CC8

		AccChargeCode fCC8;
		public AccChargeCode CC8
		{
			get
			{
				if (fCC8 == null)
				{
					fCC8 = CreateChargeCode("CC8", "Charge Code 8", Core.Constants.ChargeType.Margin, 90, GST1, WHTFREE1);
				}
				return fCC8;
			}
		}

		#endregion

		#region CC9

		protected AccChargeCode fCC9;
		public AccChargeCode CC9
		{
			get
			{
				if (fCC9 == null)
				{
					fCC9 = CreateChargeCode("CC9", "Charge Code 9", Core.Constants.ChargeType.Margin, 0, GST1, WHTFREE1);
				}
				return fCC9;
			}
		}

		#endregion

		#region CC10

		AccChargeCode fCC10;
		public AccChargeCode CC10
		{
			get
			{
				if (fCC10 == null)
				{
					fCC10 = CreateChargeCode("CC10", "Charge Code 10", Core.Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fCC10;
			}
		}

		#endregion

		#region CC11

		AccChargeCode fCC11;
		public AccChargeCode CC11
		{
			get
			{
				if (fCC11 == null)
				{
					fCC11 = CreateChargeCode("CC11", "Charge Code 11", Core.Constants.ChargeType.Margin, 100, GSTFREE1, WHT1);
				}
				return fCC11;
			}
		}

		#endregion

		#region CC12 With NonMiscDepartment

		AccChargeCode fCC12;
		public AccChargeCode CC12
		{
			get
			{
				if (fCC12 == null)
				{
					fCC12 = CreateChargeCode("CC12", "Charge Code 12", Core.Constants.ChargeType.Margin, 100, GST1, WHTFREE1, NonCurrentDepartment.GE_Code);
				}
				return fCC12;
			}
		}

		#endregion

		#region CC13 With NonMiscDepartment

		AccChargeCode fCC13;
		public AccChargeCode CC13
		{
			get
			{
				if (fCC13 == null)
				{
					fCC13 = CreateChargeCode("CC13", "Charge Code 13", Core.Constants.ChargeType.Disbursement, 100, GST1, WHT1, NonCurrentDepartment.GE_Code);
				}
				return fCC13;
			}
		}

		#endregion

		#region CC14

		AccChargeCode fCC14;
		public AccChargeCode CC14
		{
			get
			{
				if (fCC14 == null || fCC14.AC_GC != GlbCompany.CurrentCompany.PK)
				{
					fCC14 = CreateChargeCode("CC14", "Charge Code 14", Core.Constants.ChargeType.Margin, 100, KDV18, null);
				}
				return fCC14;
			}
		}

		#endregion

		#region CC15

		AccChargeCode fCC15;
		public AccChargeCode CC15
		{
			get
			{
				if (fCC15 == null || fCC15.AC_GC != GlbCompany.CurrentCompany.PK)
				{
					fCC15 = CreateChargeCode("CC15", "Charge Code 15", Core.Constants.ChargeType.Margin, 100, KDV8, null);
				}
				return fCC15;
			}
		}

		#endregion

		#endregion

		#region CommentChargeCode

		protected AccChargeCode fCommentChargeCode;
		public AccChargeCode CommentChargeCode
		{
			get
			{
				if (fCommentChargeCode == null)
				{
					fCommentChargeCode = CreateChargeCode("CCC", "Comment Charge Code", Core.Constants.ChargeType.Comment, 0, GST1, WHTFREE1);
				}
				return fCommentChargeCode;
			}
		}
		#endregion

		#region OverheadChargeCode

		protected AccChargeCode overheadChargeCode;
		public AccChargeCode OverheadChargeCode
		{
			get
			{
				if (overheadChargeCode == null)
				{
					overheadChargeCode = CreateChargeCode("CCO", "Overhead Charge Code", Core.Constants.ChargeType.Overhead, 0, GST1, WHTFREE1);
				}
				return overheadChargeCode;
			}
		}

		#endregion

		#region ManualJobAccrualChargeCode

		protected AccChargeCode fManualJobAccrualChargeCode;
		public AccChargeCode ManualJobAccrualChargeCode
		{
			get
			{
				if (fManualJobAccrualChargeCode == null)
				{
					fManualJobAccrualChargeCode = CreateChargeCode("MJA1", "Manual Job Accrual Charge Code", Core.Constants.ChargeType.ManualJobAccrual, 0, GST1, WHTFREE1);
				}
				return fManualJobAccrualChargeCode;
			}
		}

		protected AccChargeCode fManualJobAccrualChargeCode2;
		public AccChargeCode ManualJobAccrualChargeCode2
		{
			get
			{
				if (fManualJobAccrualChargeCode2 == null)
				{
					fManualJobAccrualChargeCode2 = CreateChargeCode("MJA2", "Manual Job Accrual Charge Code", Core.Constants.ChargeType.ManualJobAccrual, 0, GST1, WHTFREE1);
				}
				return fManualJobAccrualChargeCode2;
			}
		}

		#endregion

		#region GoodsClassChargeCode

		protected AccChargeCode fGoodsClassChargeCode;
		public AccChargeCode GoodsClassChargeCode
			=> fGoodsClassChargeCode = fGoodsClassChargeCode ?? CreateChargeCode("CCGDS", "Goods Class Charge Code", Core.Constants.ChargeType.Disbursement, 0, GST1, WHTFREE1, goodsServiceType: GoodServiceTypes.Codes.GDS);

		#endregion

		#region Creditor 1

		OrgHeader fCreditor1;
		public OrgHeader Creditor1
		{
			get
			{
				if (fCreditor1 == null)
				{
					fCreditor1 = CreateOrgHeader("Creditor1", true, false);
				}
				return fCreditor1;
			}
		}

		#endregion

		#region Creditor 2

		OrgHeader fCreditor2;
		public OrgHeader Creditor2
		{
			get
			{
				if (fCreditor2 == null)
				{
					fCreditor2 = CreateOrgHeader("Creditor2", true, false);
				}
				return fCreditor2;
			}
		}

		#endregion

		#region Creditor 3

		OrgHeader fCreditor3;
		public OrgHeader Creditor3
		{
			get
			{
				if (fCreditor3 == null)
				{
					fCreditor3 = CreateOrgHeader("Creditor3", true, false);
				}
				return fCreditor3;
			}
		}

		#region Creditor 4

		OrgHeader fCreditor4;
		public OrgHeader Creditor4
		{
			get
			{
				if (fCreditor4 == null)
				{
					fCreditor4 = CreateOrgHeader("Creditor4", true, false, true, false, false, false);
				}
				return fCreditor4;
			}
		}

		#endregion

		#region Creditor 5

		OrgHeader fCreditor5;
		public OrgHeader Creditor5
		{
			get
			{
				if (fCreditor5 == null)
				{
					fCreditor5 = CreateOrgHeader("Creditor5", true, false, false, false, false, false);
				}
				return fCreditor5;
			}
		}

		#endregion

		#region Creditor 6

		OrgHeader fCreditor6;
		public OrgHeader Creditor6
		{
			get
			{
				if (fCreditor6 == null)
				{
					fCreditor6 = CreateOrgHeader("Creditor6", true, false, true, false, false, false);
				}
				return fCreditor6;
			}
		}

		#endregion

		#endregion

		#region AUD Cheque Book

		AccChequeBook fAUDChequeBook;
		public AccChequeBook AUDChequeBook
		{
			get
			{
				if (IsLoadBeforeCreate)
				{ fAUDChequeBook = Factory.LoadTop1<AccChequeBook>(new ZQuery(AccChequeBookSchema.AK_Desc, "Test AU chequebook")); }
				if (fAUDChequeBook == null)
				{
					fAUDChequeBook = CreateChequeBook("Test AU chequebook", 100, AUDBankAccount);
				}
				return fAUDChequeBook;
			}
		}

		#endregion

		#region AUD2 Cheque Book

		AccChequeBook fAUDChequeBook2;
		public AccChequeBook AUDChequeBook2
		{
			get
			{
				if (fAUDChequeBook2 == null)
				{
					fAUDChequeBook2 = CreateChequeBook("Test AU chequebook2", 100, AUDBankAccount2);
				}
				return fAUDChequeBook2;
			}
		}

		#endregion

		#region USD Cheque Book

		AccChequeBook fUSDChequeBook;
		public AccChequeBook USDChequeBook
		{
			get
			{
				if (fUSDChequeBook == null)
				{
					fUSDChequeBook = CreateChequeBook("Test US chequebook", 100, USDBankAccount);
				}
				return fUSDChequeBook;
			}
		}

		#endregion

		#region Hot Cheque

		AccHotCheque fHotCheque;
		public AccHotCheque HotCheque
		{
			get
			{
				if (fHotCheque == null)
				{
					fHotCheque = CreateHotCheque(Job1, "Luke", 1, "Test hot cheque from TransactionCreatorBaseTest");
				}
				return fHotCheque;
			}
		}

		#endregion

		#region Local Client 2

		OrgHeader fLocalClient2;
		public OrgHeader LocalClient2
		{
			get
			{
				if (fLocalClient2 == null)
				{
					fLocalClient2 = CreateOrgHeader("LOCCLT2", false, true);
				}
				return fLocalClient2;
			}
		}

		#endregion

		#region Agent

		public OrgHeader Agent
		{
			get { return agent ?? (agent = CreateOrgHeader("Agent", false, true)); }
		}
		OrgHeader agent;

		public OrgHeader Agent2
		{
			get { return agent2 ?? (agent2 = CreateOrgHeader("Agent2", false, true)); }
		}
		OrgHeader agent2;

		#endregion

		#region TaxMsg

		AccInvMsg fTaxMsg1;
		public AccInvMsg TaxMsg1
		{
			get
			{
				if (fTaxMsg1 == null)
				{
					fTaxMsg1 = CreateTaxMsg("MSG1", "MSG1 Description", "English Msg 1", "Local Msg 1");
				}
				return fTaxMsg1;
			}
		}

		AccInvMsg fTaxMsg2;
		public AccInvMsg TaxMsg2
		{
			get
			{
				if (fTaxMsg2 == null)
				{
					fTaxMsg2 = CreateTaxMsg("MSG2", "MSG2 Description", "English Msg 2", "Local Msg 2");
				}
				return fTaxMsg2;
			}
		}

		AccInvMsg fTaxMsg3;
		public AccInvMsg TaxMsg3
		{
			get
			{
				if (fTaxMsg3 == null)
				{
					fTaxMsg3 = CreateTaxMsg("MSG3", "MSG3 Description", "English Msg 3", "Local Msg 3");
				}
				return fTaxMsg3;
			}
		}

		AccInvMsg fTaxMsg4;
		public AccInvMsg TaxMsg4
		{
			get
			{
				if (fTaxMsg4 == null)
				{
					fTaxMsg4 = CreateTaxMsg("MSG4", "MSG4 Description", "English Msg 4", "Local Msg 4");
				}
				return fTaxMsg4;
			}
		}

		AccInvMsg fTaxMsg301;
		public AccInvMsg TaxMsg301
		{
			get
			{
				if (fTaxMsg301 == null)
				{
					fTaxMsg301 = CreateTaxMsg("301", "301 Tax Message Description", "Tax Message in English language", "Tax Message in local language");
				}
				return fTaxMsg301;
			}
		}

		AccInvMsg fTaxMsgWithTaxGroup;
		public AccInvMsg TaxMsgWithTaxGroup
		{
			get
			{
				if (fTaxMsgWithTaxGroup == null)
				{
					fTaxMsgWithTaxGroup = CreateTaxMsg("TAXMSGCODE", "Tax Message Description", "Tax Message in English language", "Tax Message in local language", taxGroupCode: "TGC", taxGroupDescription: "Tax Group Description");
				}
				return fTaxMsgWithTaxGroup;
			}
		}

		AccInvMsg fTaxMsg606;
		public AccInvMsg TaxMsg606
		{
			get
			{
				if (fTaxMsg606 == null)
				{
					fTaxMsg606 = CreateTaxMsg("606", "14 Uluslararası taşımacılık", "Exemption Rule 606", "301 Nolu Tevkifat Kodu");
				}
				return fTaxMsg606;
			}
		}
		#endregion

		#region KDV18

		AccTaxRate fKDV18;
		public AccTaxRate KDV18
		{
			get
			{
				if (fKDV18 == null)
				{
					fKDV18 = CreateTaxRate("KDV18", "KDV Rate 18", 18, 1, "TR");
				}
				return fKDV18;
			}
		}

		AccTaxRate fKDV18W5;
		public AccTaxRate KDV18W5
		{
			get
			{
				if (fKDV18W5 == null)
				{
					fKDV18W5 = CreateTaxRate("KDV18W5", "18% Standard VAT - Withholding 50%", AccTaxRate.Types.Rated, 18, "REF", 5, 10, "TR");
					fKDV18W5.AT_A9_DefaultVatClass = TaxMsg606.PK;
				}
				return fKDV18W5;
			}
		}

		AccTaxRate fKDV18W5TG;
		public AccTaxRate KDV18W5TG
		{
			get
			{
				if (fKDV18W5TG == null)
				{
					fKDV18W5TG = CreateTaxRate("KDV18W5", "18% Standard VAT - Withholding 50%", AccTaxRate.Types.Rated, 18, "REF", 5, 10, "TR");
					fKDV18W5TG.AT_A9_DefaultVatClass = TaxMsgWithTaxGroup.PK;
				}
				return fKDV18W5TG;
			}
		}

		AccTaxRate fKDV18W7;
		public AccTaxRate KDV18W7
		{
			get
			{
				if (fKDV18W7 == null)
				{
					fKDV18W7 = CreateTaxRate("KDV18W7", "18% Standard VAT - Withholding 70%", AccTaxRate.Types.Rated, 18, "REF", 7, 10, "TR");
					fKDV18W7.AT_A9_DefaultVatClass = TaxMsg606.PK;
				}
				return fKDV18W7;
			}
		}

		AccTaxRate fKDV20W5;
		public AccTaxRate KDV20W5
		{
			get
			{
				if (fKDV20W5 == null)
				{
					fKDV20W5 = CreateTaxRate("KDV20W5", "20% Standard VAT - Withholding 50%", AccTaxRate.Types.Rated, 20, "REF", 5, 10, "TR");
				}
				return fKDV20W5;
			}
		}

		#endregion

		#region KDV8

		AccTaxRate fKDV8;
		public AccTaxRate KDV8
		{
			get
			{
				if (fKDV8 == null)
				{
					fKDV8 = CreateTaxRate("KDV8", "KDV Rate 8", 8, 1, "TR");
				}
				return fKDV8;
			}
		}

		AccTaxRate fKDV8W5;
		public AccTaxRate KDV8W5
		{
			get
			{
				if (fKDV8W5 == null)
				{
					fKDV8W5 = CreateTaxRate("KDV8W5", "8% Standard VAT - Withholding 50%", AccTaxRate.Types.Rated, 8, "REF", 5, 10, "TR");
					fKDV8W5.AT_A9_DefaultVatClass = TaxMsg606.PK;
				}
				return fKDV8W5;
			}
		}

		AccTaxRate fKDV8W3;
		public AccTaxRate KDV8W3
		{
			get
			{
				if (fKDV8W3 == null)
				{
					fKDV8W3 = CreateTaxRate("KDV8W5", "8% Standard VAT - Withholding 30%", AccTaxRate.Types.Rated, 8, "REF", 3, 10, "TR");
					fKDV8W3.AT_A9_DefaultVatClass = TaxMsg606.PK;
				}
				return fKDV8W3;
			}
		}

		#endregion

		#region KDV1

		AccTaxRate fKDV1;
		public AccTaxRate KDV1
		{
			get
			{
				if (fKDV1 == null)
				{
					fKDV1 = CreateTaxRate("KDV1", "KDV Rate 1", 1, 1, "TR");
				}
				return fKDV1;
			}
		}

		#endregion

		#region GST1

		AccTaxRate fGST1;
		public AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1);
				}
				return fGST1;
			}
		}

		#endregion

		#region GST1WithDates

		public AccTaxRate GST1WithDates
		{
			get
			{
				if (gst1WithDates == null)
				{
					gst1WithDates = GetNewFactory().Load<AccTaxRate>(CreateTaxRate("GST1DATE", "GST Rate 1 Dates", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1).PK);

					var rate = gst1WithDates.SetRate_ForTestOnly(10, 5);
					rate.ZAT_EndDate = ZDate.Today.AddMonths(-1);
					var rate2 = gst1WithDates.SetRate_ForTestOnly(10, 1, rate.ZAT_EndDate.AddDays(2), ZDate.Today.AddMonths(1)); //AddDays(2) makes gap for GST1WithDates_DateWithNoRate
					gst1WithDates.SetRate_ForTestOnly(15, 3, rate2.ZAT_EndDate.AddDays(1), null);

					gst1WithDates.Factory.Save();
				}
				return gst1WithDates;
			}
		}
		AccTaxRate gst1WithDates;

		public ZDate GST1WithDates_DateWithNoRate => ZDate.Today.AddMonths(-1).AddDays(1);

		AccTaxRate gst11;
		public AccTaxRate GST11
		{
			get
			{
				if (gst11 == null)
				{
					gst11 = CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 11, string.Empty, 0, 1);
				}
				return gst11;
			}
		}

		AccTaxRate fGST2;
		public AccTaxRate GST2
		{
			get
			{
				if (fGST2 == null)
				{
					fGST2 = CreateTaxRate("GST2", "GST Rate 20%", AccTaxRate.Types.Rated, 20, string.Empty, 0, 1);
				}
				return fGST2;
			}
		}

		public AccTaxRate GST2WithDates
		{
			get
			{
				if (gst2WithDates == null)
				{
					gst2WithDates = GetNewFactory().Load<AccTaxRate>(CreateTaxRate("GST2DATE", "GST Rate 2 Dates", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1).PK);

					var rate = gst2WithDates.SetRate_ForTestOnly(10, 5);
					rate.ZAT_EndDate = ZDate.Today.AddMonths(-1);
					var rate2 = gst2WithDates.SetRate_ForTestOnly(10, 1, rate.ZAT_EndDate.AddDays(2), ZDate.Today.AddMonths(1)); //AddDays(2) makes gap for GST1WithDates_DateWithNoRate
					gst2WithDates.SetRate_ForTestOnly(15, 3, rate2.ZAT_EndDate.AddDays(1), null);

					gst2WithDates.Factory.Save();
				}
				return gst2WithDates;
			}
		}
		AccTaxRate gst2WithDates;

		#endregion

		#region GSTFREE

		AccTaxRate fGSTFREE1;
		public AccTaxRate GSTFREE1
		{
			get
			{
				if (fGSTFREE1 == null)
				{
					fGSTFREE1 = CreateTaxRate("GSTFREE1", "GST Free Rate 1", AccTaxRate.Types.Rated, 0, string.Empty, 0, 1);
				}
				return fGSTFREE1;
			}
		}

		AccTaxRate fGSTFREE2;
		public AccTaxRate GSTFREE2
		{
			get
			{
				if (fGSTFREE2 == null)
				{
					fGSTFREE2 = CreateTaxRate("GSTFREE2", "GST Free Rate 2", AccTaxRate.Types.Rated, 0, string.Empty, 0, 1);
					fGSTFREE2.AT_A9_DefaultVatClass = TaxMsg3.PK;
				}
				return fGSTFREE2;
			}
		}

		AccTaxRate fFREEVAT;
		public AccTaxRate FREEVAT
		{
			get
			{
				if (fFREEVAT == null)
				{
					fFREEVAT = CreateTaxRate("FREEVAT", "Free Vat", AccTaxRate.Types.Rated, 0, string.Empty, 0, 1);
					fFREEVAT.AT_A9_DefaultVatClass = TaxMsg301.PK;
				}
				return fFREEVAT;
			}
		}

		AccTaxRate fFREEVATTG;
		public AccTaxRate FREEVATTG
		{
			get
			{
				if (fFREEVATTG == null)
				{
					fFREEVATTG = CreateTaxRate("FREEVAT", "Free Vat", AccTaxRate.Types.Rated, 0, string.Empty, 0, 1);
					fFREEVATTG.AT_A9_DefaultVatClass = TaxMsgWithTaxGroup.PK;
				}
				return fFREEVATTG;
			}
		}

		#endregion

		#region GSTANDQST1

		AccTaxRate gstAndQst1;
		public AccTaxRate GSTANDQST1
		{
			get
			{
				if (gstAndQst1 == null)
				{
					gstAndQst1 = CreateTaxRate("GST&QT", "GST AND QST Rate 1", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQST, 95, 10);
				}
				return gstAndQst1;
			}
		}

		#endregion

		#region GSTANDQST1WithDates

		public AccTaxRate GSTANDQST1WithDates
		{
			get
			{
				if (gstANDQST1WithDates == null)
				{
					gstANDQST1WithDates = GetNewFactory().Load<AccTaxRate>(CreateTaxRate("GSTQST", "GST & QST Rate Dates", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQST, 95, 10).PK);

					var rate = gstANDQST1WithDates.SetRate_ForTestOnly(10, 5);
					rate.ZAT_StartDate = ZDate.Today.AddYears(-2); //AddYears(-2) makes gap for GSTANDQST1WithDates_DateWithNoRateAndExtraRate
					rate.ZAT_EndDate = ZDate.Today.AddMonths(-1);
					var rate2 = gstANDQST1WithDates.SetRate_ForTestOnly(10, 2, rate.ZAT_EndDate.AddDays(1), ZDate.Today.AddMonths(1));
					gstANDQST1WithDates.SetRate_ForTestOnly(10, 1, rate2.ZAT_EndDate.AddDays(2), null); //AddDays(2) makes gap for GSTANDQST1WithDates_DateWithNoRate

					var extraRate = gstANDQST1WithDates.SetExtraRate_ForTestOnly(9, 1);
					extraRate.ZAT_StartDate = ZDate.Today.AddYears(-2); //AddYears(-2) makes gap for GSTANDQST1WithDates_DateWithNoRateAndExtraRate
					extraRate.ZAT_EndDate = ZDate.Today.AddMonths(-2);
					var extraRate2 = gstANDQST1WithDates.SetExtraRate_ForTestOnly(95, 10, extraRate.ZAT_EndDate.AddDays(2), ZDate.Today.AddMonths(2)); //AddDays(2) makes gap for GSTANDQST1WithDates_DateWithNoExtraRate
					gstANDQST1WithDates.SetExtraRate_ForTestOnly(10, 1, extraRate2.ZAT_EndDate.AddDays(1), null);

					gstANDQST1WithDates.Factory.Save();
				}
				return gstANDQST1WithDates;
			}
		}
		AccTaxRate gstANDQST1WithDates;

		public ZDate GSTANDQST1WithDates_DateWithNoRate => ZDate.Today.AddMonths(1).AddDays(1);
		public ZDate GSTANDQST1WithDates_DateWithNoExtraRate => ZDate.Today.AddMonths(-2).AddDays(1);
		public ZDate GSTANDQST1WithDates_DateWithNoRateAndExtraRate => ZDate.Today.AddYears(-2).AddDays(-1);

		#endregion

		#region GSTANDQSTBASEDONQCT

		AccTaxRate gstAndQstBasedOnQct;
		public AccTaxRate GSTANDQSTBASEDONQCT
		{
			get
			{
				if (gstAndQstBasedOnQct == null)
				{
					gstAndQstBasedOnQct = CreateTaxRate("GST&Q2", "GST AND QST", AccTaxRate.Types.Rated, 5, AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase, 9975, 1000);
				}
				return gstAndQstBasedOnQct;
			}
		}

		public AccTaxRate STAGST
		{
			get
			{
				if (staGST == null)
				{
					staGST = CreateTaxRate("STAGST", "State GST", AccTaxRate.Types.Rated, 9, AccTaxRate.ExtraTypes.StateGST, 9, 1);
				}
				return staGST;
			}
		}
		AccTaxRate staGST;

		public AccTaxRate IntegratedGST
		{
			get
			{
				if (iGST == null)
				{
					iGST = CreateTaxRate("IGST", "Integrated GST", AccTaxRate.Types.IntegratedGST, 18, string.Empty, 0, 1);
				}
				return iGST;
			}
		}
		AccTaxRate iGST;

		#endregion

		#region SERANDEDU1

		AccTaxRate serAndEdu1;
		public AccTaxRate SERANDEDU1
		{
			get
			{
				if (serAndEdu1 == null)
				{
					serAndEdu1 = CreateTaxRate("SER&ED", "GST and EDU Rate 1", AccTaxRate.Types.Rated, 10, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);
				}
				return serAndEdu1;
			}
		}

		#endregion

		#region India Service tax

		public AccTaxRate ServiceTax
		{
			get
			{
				if (serviceTax == null)
				{
					serviceTax = CreateTaxRate("SVC", "Service Tax", AccTaxRate.Types.ServiceTax, 5, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 5, 1);
				}
				return serviceTax;
			}
		}

		AccTaxRate serviceTax;

		public AccTaxRate ExtraServiceTax
		{
			get
			{
				if (extraServiceTax == null)
				{
					extraServiceTax = CreateTaxRate("NOT", "Not applicable", AccTaxRate.Types.NotReportable, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);
				}
				return extraServiceTax;
			}
		}
		AccTaxRate extraServiceTax;
		#endregion

		#region INP7

		AccTaxRate inp7;
		public AccTaxRate INP7
		{
			get
			{
				if (inp7 == null)
				{
					inp7 = CreateTaxRate("INP7", "7% Input VAT Claimed", AccTaxRate.Types.Rated, 0, AccTaxRate.ExtraTypes.ChinaInputVATClaimed, 7, 1);
				}
				return inp7;
			}
		}

		#endregion

		#region SPV

		AccTaxRate vatspv;
		public AccTaxRate VATSPV
		{
			get
			{
				if (vatspv == null)
				{
					vatspv = CreateTaxRate("VATSPV", "VAT Remitted by Customer Tax", AccTaxRate.Types.Rated, 22, AccTaxRate.ExtraTypes.VATRemittedByCustomer, 0, 1, Core.Constants.CountryCodes.Italy);
				}
				return vatspv;
			}
		}

		AccTaxRate vatexon;
		public AccTaxRate VATEXON
		{
			get
			{
				if (vatexon == null)
				{
					vatexon = CreateTaxRate("VATEXON", "VAT Remitted by Customer Tax", AccTaxRate.Types.Rated, 22, AccTaxRate.ExtraTypes.VATRemittedByCustomer, 0, 1);
					vatexon.AT_RN_NKCountry = Core.Constants.CountryCodes.CostaRica;
				}
				return vatexon;
			}
		}

		#endregion

		#region GSTWithExtraRate

		AccTaxRate fGSTWithExtraRate;
		public AccTaxRate GSTWithExtraRate
		{
			get
			{
				if (fGSTWithExtraRate == null)
				{
					fGSTWithExtraRate = CreateTaxRate("GSTNEDU", "GSTANDEDU", AccTaxRate.Types.Rated, 12, AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax, 3, 1);
				}
				return fGSTWithExtraRate;
			}
		}

		#endregion

		#region WHT1

		AccWithholding fWHT1;
		public AccWithholding WHT1
		{
			get
			{
				if (fWHT1 == null)
				{
					fWHT1 = CreateOrLoadWithholdingTax("WHT1", "WHT Rate 1", 5);
				}
				return fWHT1;
			}
		}

		#endregion

		#region WHT2

		public AccWithholding WHT2
		{
			get
			{
				if (wHT2 == null)
				{
					wHT2 = CreateOrLoadWithholdingTax("WHT2", "WHT Rate 2", 6.5M);
				}
				return wHT2;
			}
		}
		AccWithholding wHT2;

		#endregion

		#region WHTTR90

		public AccWithholding WHTTR90
		{
			get
			{
				if (wHTTR90 == null)
				{
					wHTTR90 = CreateOrLoadWithholdingTax("WHTTR90", "WHT Rate 90", 90M);
				}
				return wHTTR90;
			}
		}
		AccWithholding wHTTR90;

		#endregion

		#region WHTFREE1

		AccWithholding fWHTFREE1;
		public AccWithholding WHTFREE1
		{
			get
			{
				if (fWHTFREE1 == null)
				{
					fWHTFREE1 = CreateOrLoadWithholdingTax("WHTFREE1", "WHT Free Rate 1", 0);
				}
				return fWHTFREE1;
			}
		}

		#endregion

		#region SVAT1

		public AccTaxRate SVAT1
		{
			get
			{
				return fSVAT1 ?? (fSVAT1 = CreateTaxRate("SVAT1", "Suspended VAT Rate 1", AccTaxRate.Types.Suspended, 12, string.Empty, 0, 1));
			}
		}
		AccTaxRate fSVAT1;

		#endregion

		#region SVAT2

		public AccTaxRate SVAT2
		{
			get
			{
				return fSVAT2 ?? (fSVAT2 = CreateTaxRate("SVAT2", "Suspended VAT Rate 2", AccTaxRate.Types.Suspended, 15, string.Empty, 0, 1));
			}
		}
		AccTaxRate fSVAT2;

		#endregion

		#region RAX

		public AccTaxRate RAX
		{
			get
			{
				return rax ?? (rax = CreateTaxRate("RAX", "RatedInAnotherCountry", AccTaxRate.Types.RatedInAnotherCountry, 17, string.Empty, 0, 1));
			}
		}
		AccTaxRate rax;

		#endregion

		#region REFZ

		public AccTaxRate REF
		{
			get
			{
				return refz ?? (refz = CreateTaxRate("REFZ", "RatedInAnotherCountry", AccTaxRate.Types.RatedInAnotherCountry, 9, "REF", 125, 10));
			}
		}
		AccTaxRate refz;

		#endregion

		#region CAP

		public AccTaxRate CAP
		{
			get
			{
				return cap ?? (cap = CreateTaxRate("CAP", "CapitalRated", AccTaxRate.Types.CapitalRated, 18, string.Empty, 0, 1));
			}
		}
		AccTaxRate cap;

		#endregion

		#region FREECAPGST

		public AccTaxRate FREECAPGST
		{
			get
			{
				if (fREECAPGST == null)
				{
					fREECAPGST = CreateTaxRateWithoutZZ("FRECAPGST", "CapitalRated", 0);
					fREECAPGST.AT_Type = AccTaxRate.Types.CapitalRated;
				}
				return fREECAPGST;
			}
		}
		AccTaxRate fREECAPGST;

		#endregion

		#region REV

		public AccTaxRate REV
		{
			get
			{
				return rev ?? (rev = CreateTaxRate("REV", "ReverseRated", AccTaxRate.Types.ReverseRated, 10, string.Empty, 0, 1));
			}
		}
		AccTaxRate rev;

		public AccTaxRate RVS1
		{
			get
			{
				if (rvs == null)
				{
					rvs = CreateTaxRate("RVS1", "RVS Rate 1", AccTaxRate.Types.ReverseRated, 18, string.Empty, 0, 1);
				}
				return rvs;
			}
		}
		AccTaxRate rvs;

		#endregion

		#region AUDBankAccount

		AccBankAccount fAUDBankAccount;
		public AccBankAccount AUDBankAccount
		{
			get
			{
				if (IsLoadBeforeCreate)
				{ fAUDBankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD")); }
				if (fAUDBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZAUDAcc";
					fAUDBankAccount = CreateBankAccount("ZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", AUD, "123456", "12345678", header);
				}
				return fAUDBankAccount;
			}
		}

		#endregion

		#region AUDBankAccount2

		AccBankAccount fAUDBankAccount2;
		public AccBankAccount AUDBankAccount2
		{
			get
			{
				if (fAUDBankAccount2 == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZAUDAcc2";
					fAUDBankAccount2 = CreateBankAccount("ZANZAUD2", "ANZ AUD ACCT", "ANZ", "AUD", AUD, "111111", "11111111", header);
				}
				return fAUDBankAccount2;
			}
		}

		#endregion

		#region USDBankAccount

		AccBankAccount fUSDBankAccount;
		public AccBankAccount USDBankAccount
		{
			get
			{
				if (fUSDBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZUSDHeader";
					fUSDBankAccount = CreateBankAccount("ZHSBCUSD", "HSBC USD ACCT", "HSBC", "USD", USD, "654321", "87654321", header);
				}
				return fUSDBankAccount;
			}
		}

		#endregion

		#region USDBankAccount2

		AccBankAccount fUSDBankAccount2;
		public AccBankAccount USDBankAccount2
		{
			get
			{
				if (fUSDBankAccount2 == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZUSDAcc2";
					fUSDBankAccount2 = CreateBankAccount("ZHSBCUSD", "HSBC USD ACCT", "HSBC", "USD", USD, "654321", "87654321", header);
				}
				return fUSDBankAccount2;
			}
		}

		#endregion

		#region GBPBankAccount

		AccBankAccount fGBPBankAccount;
		public AccBankAccount GBPBankAccount
		{
			get
			{
				if (fGBPBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZGBPHeader";
					fGBPBankAccount = CreateBankAccount("ZHSBCGBP1", "HSBC GBP ACCT1", "CNZ", "GBP", GBP, "222222", "222211111", header);
				}
				return fGBPBankAccount;
			}
		}

		#endregion

		#region CHNBankAccount

		AccBankAccount fCHNBankAccount;
		public AccBankAccount CHNBankAccount
		{
			get
			{
				if (fCHNBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZCHNACC";
					fCHNBankAccount = CreateBankAccount("ZCNZCHN", "CHN ACCT", "CNZ", "CNY", CNY, "333333", "333311111", header);
				}
				return fCHNBankAccount;
			}
		}

		#endregion

		#region CHNBankAccount2

		AccBankAccount fCHNBankAccount2;
		public AccBankAccount CHNBankAccount2
		{
			get
			{
				if (fCHNBankAccount2 == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZCHNACC2";
					fCHNBankAccount2 = CreateBankAccount("ZCNZCHN2", "CHN ACCT2", "CNZ", "CNY", CNY, "444444", "444411111", header);
				}
				return fCHNBankAccount2;
			}
		}

		#endregion

		#region EURBankAccount
		AccBankAccount fEURBankAccount;
		public AccBankAccount EURBankAccount
		{
			get
			{
				if (fEURBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZEURHeader";
					fEURBankAccount = CreateBankAccount("ZHSBCEUR1", "HSBC EUR ACCT1", "EUR", "EUR", EUR, "222222", "222211111", header);
				}
				return fEURBankAccount;
			}
		}
		public object DummyMethodToMakeThisWork() => throw new NotSupportedException("This method is required to load unit tests from CW1 when EURBankAccount was added; suspect it won't be needed at some point in the future. Without it you get a TypeLoadException: Type '__StaticArrayInitTypeSize=5316' from assembly 'Enterprise.Accounting.Business, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350' contains more methods than the current implementation allows.");
		#endregion

		#region Local Client

		OrgHeader fLocalClient;
		public OrgHeader LocalClient
		{
			get
			{
				if (fLocalClient == null)
				{
					fLocalClient = CreateOrgHeader("LOCCLT", false, true);
				}
				return fLocalClient;
			}
		}

		#endregion

		#region ActiveOrg

		protected OrgHeader fActiveOrg;
		public OrgHeader ActiveOrg
		{
			get
			{
				if (fActiveOrg == null)
				{
					fActiveOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AASDRA");
				}
				return fActiveOrg;
			}
		}
		#endregion

		#region InActiveOrg

		protected OrgHeader fInActiveOrg;
		public OrgHeader InActiveOrg
		{
			get
			{
				if (fInActiveOrg == null)
				{
					string oH_Code = "ABABEU";
					fInActiveOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, oH_Code);
				}
				return fInActiveOrg;
			}
		}

		public class MakeOrgTemporarilyInactive : IDisposable
		{
			public MakeOrgTemporarilyInactive(OrgHeader org)
			{
				UpdateOrgActiveStatusInIsolatedFactory(org.OH_Code, false);
				this.Org = org;
			}

			#region IDisposable Members
			readonly OrgHeader Org;

			void UpdateOrgActiveStatusInIsolatedFactory(string oH_Code, bool status)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, oH_Code).OH_IsActive = status;
				factory.Save();
			}

			void IDisposable.Dispose()
			{
				UpdateOrgActiveStatusInIsolatedFactory(Org.OH_Code, true);
			}

			#endregion
		}

		#endregion

		#region ABIGAS

		protected OrgHeader fABIGAS;
		public OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
					if (fABIGAS == null)
					{
						fABIGAS = Factory.New<OrgHeader>();
						fABIGAS.OH_Code = "ABIGAS";
					}
				}
				return fABIGAS;
			}
		}

		#endregion

		#region AALSHI

		protected OrgHeader fAALSHI;
		public OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
					if (fAALSHI == null)
					{
						fAALSHI = Factory.New<OrgHeader>();
						fAALSHI.OH_Code = "AALSHI";
					}

					fAALSHI.CompanyData.SetAPTaxApplicable(true);
					fAALSHI.MiscServ.OM_APWHTApplicable = true;
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ZECTRA

		protected OrgHeader fZECTRA;
		public OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		#region XLINDU

		OrgHeader fXLINDU;
		public OrgHeader XLINDU
		{
			get { return fXLINDU ?? (fXLINDU = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "XLINDU")); }
		}

		#endregion

		#region MRG100

		protected AccChargeCode fMRG100;
		public AccChargeCode MRG100
		{
			get
			{
				if (fMRG100 == null)
				{
					fMRG100 = CreateChargeCode("MRG100", "Margin 100% With GST & WHT", Core.Constants.ChargeType.Margin, 100, GST1, WHT1, "ALL");
				}
				return fMRG100;
			}
		}

		#endregion

		#region MRG60

		protected AccChargeCode fMRG60;
		public AccChargeCode MRG60
		{
			get
			{
				if (fMRG60 == null)
				{
					fMRG60 = CreateChargeCode("MRG60", "Margin 60% With GST & WHT", Core.Constants.ChargeType.Margin, 60, GST1, WHT1, "ALL");
				}
				return fMRG60;
			}
		}

		#endregion

		#region NonAccrualChargeCode

		AccChargeCode fNonAccrualChargeCode;
		public AccChargeCode NonAccrualChargeCode
		{
			get
			{
				if (fNonAccrualChargeCode == null)
				{
					fNonAccrualChargeCode = CreateChargeCode("NON", "Non Accrual", Constants.ChargeType.NonAccrual, 0, GST1, WHT1, "ALL");
				}

				return fNonAccrualChargeCode;
			}
		}

		#endregion

		AccChargeCode fRevenueChargeCode;
		public AccChargeCode RevenueChargeCode
		{
			get
			{
				if (fRevenueChargeCode == null)
				{
					fRevenueChargeCode = CreateChargeCode("REV", "Revenue", Constants.ChargeType.Revenue, 100, GST1, WHT1, "ALL");
				}

				return fRevenueChargeCode;
			}
		}

		AccChargeCode revenueNoTaxChargeCode;
		public AccChargeCode RevenueNoTaxChargeCode
		{
			get { return revenueNoTaxChargeCode ?? (revenueNoTaxChargeCode = CreateChargeCode("REVFRE", "Revenue", Constants.ChargeType.Revenue, 0, GSTFREE1, WHTFREE1)); }
		}

		#region FRT

		protected AccChargeCode fFRT;
		public AccChargeCode FRT
		{
			get
			{
				if (fFRT == null)
				{
					var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
					rate.SetRateNumerator_ForTestOnly(0);
					rate.Factory.Save();

					fFRT = Factory.Load<AccChargeCode>(Enterprise.Environment.Env.Registry.FreightChargeCode);
				}
				return fFRT;
			}
		}

		#endregion

		#region DSBChargeCode

		protected AccChargeCode fDSBChargeCode;
		public AccChargeCode DSBChargeCode
		{
			get
			{
				if (fDSBChargeCode == null)
				{
					fDSBChargeCode = CreateChargeCode("DSB", "Disbursement Code With GST & WHT", Core.Constants.ChargeType.Disbursement, 100, GST1, WHT1, "ALL");
				}
				return fDSBChargeCode;
			}
		}

		protected AccChargeCode fDSBChargeCode1;
		public AccChargeCode DSBChargeCode1
		{
			get
			{
				if (fDSBChargeCode1 == null)
				{
					fDSBChargeCode1 = CreateChargeCode("DSB1", "Disbursement Code With GST & WHT", Core.Constants.ChargeType.Disbursement, 100, GST1, WHT1, "ALL");
				}
				return fDSBChargeCode1;
			}
		}

		#endregion

		#region GLHeader1

		public AccGLHeader GLHeader1
		{
			get
			{
				if (fGLHeader1 == null)
				{
					fGLHeader1 = CreateGLHeader();
				}

				return fGLHeader1;
			}
		}

		AccGLHeader fGLHeader1;

		#endregion

		#region GLHeader2

		public AccGLHeader GLHeader2
		{
			get
			{
				if (fGLHeader2 == null)
				{
					fGLHeader2 = CreateGLHeader();
				}

				return fGLHeader2;
			}
		}

		AccGLHeader fGLHeader2;

		#endregion

		#region GLHeaderNTE1

		public AccGLHeader GLHeaderNTE1
		{
			get
			{
				if (fGLHeaderNTE1 == null)
				{
					fGLHeaderNTE1 = CreateAccGLHeader(GetRandomString(10), string.Empty, string.Empty, Constants.AccountType.Note, DebitCreditDataEntry.DR);
				}

				return fGLHeaderNTE1;
			}
		}

		AccGLHeader fGLHeaderNTE1;

		#endregion

		#region GLHeaderNTE2

		public AccGLHeader GLHeaderNTE2
		{
			get
			{
				if (fGLHeaderNTE2 == null)
				{
					fGLHeaderNTE2 = CreateAccGLHeader(GetRandomString(10), string.Empty, string.Empty, Constants.AccountType.Note, DebitCreditDataEntry.CR);
				}

				return fGLHeaderNTE2;
			}
		}

		AccGLHeader fGLHeaderNTE2;

		#endregion

		#region Existing GL Accounts

		public AccGLHeader ExchangeGainLossControlAccount
		{
			get
			{
				return Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2020.10.00"));
			}
		}

		public AccGLHeader ExchangeGainLossAdjustmentAccount
		{
			get
			{
				return Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2020.20.00"));
			}
		}

		public AccGLHeader GLJournalClearingAccount
		{
			get
			{
				return Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6820.00.00"));
			}
		}

		public AccGLHeader CashOnHandAccount
		{
			get
			{
				return Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6010.00.00"));
			}
		}

		public AccGLHeader CashAtBankAccount
		{
			get
			{
				return Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6110.00.00"));
			}
		}

		#endregion

		#region Company

		public GlbCompany CreateNewCompany(ZString companyCode, OrgHeader orgProxy = null)
		{
			return CreateNewCompany(companyCode, ZString.Empty, orgProxy);
		}

		public GlbCompany CreateNewCompany(ZString companyCode, ZString countryCode, OrgHeader orgProxy = null)
		{
			var company = GetNewFactory().New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
			if (orgProxy != null)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			company.Factory.Save();

			return Factory.Load<GlbCompany>(company.PK);
		}

		public (GlbCompany Company, GlbBranch Branch) CreateCompanyAndBranch(ZString homePortOrCountryCode, OrgHeader orgProxy)
		{
			var countryCode = homePortOrCountryCode.Substring(0, 2);

			var homePortUnloco = homePortOrCountryCode;
			if (homePortUnloco.Length != 5)
			{
				var portQuery = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
				homePortUnloco = Factory.LoadTop1<RefUNLOCO>(portQuery)?.RL_Code ?? homePortOrCountryCode.PadRight(5, 'A');
			}

			if (orgProxy == null)
			{
				orgProxy = CreateOrgHeader(countryCode + "PROXY", true, true, homePortUnloco);
			}

			var company = CreateNewCompany("C" + countryCode, countryCode, orgProxy);
			company.GC_RX_NKLocalCurrency = CurrencyCodes.UnitedStates;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "B" + countryCode;
			branch.GB_OH_OrgProxy = orgProxy.PK;
			branch.GB_RL_NKHomePort = homePortUnloco;

			return (company, branch);
		}

		public GlbCompany CreateCompanyAndBranch(ZString homePortOrCountryCode)
			=> CreateCompanyAndBranch(homePortOrCountryCode, null).Company;

		public GlbBranch CreateBranchWithCompany(ZString homePortOrCountryCode)
			=> CreateCompanyAndBranch(homePortOrCountryCode, null).Branch;

		#endregion

		#region Branch

		public GlbBranch CreateNewBranch(GlbCompany company, ZString branchCode)
		{
			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = branchCode;
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company.GC_RN_NKCountryCode)).Code;
			newBranch.GB_BranchName = "Test Branch";
			return newBranch;
		}

		#endregion

		#region UserContext / SwitchToCompany / SwitchToBranch

		public UserContext GetUserContext(GlbCompany company, GlbDepartment department = null)
		{
			return GetUserContext(company.FirstActiveBranch, department: department);
		}

		public UserContext GetUserContext(GlbBranch branch, GlbDepartment department = null)
		{
			var departmentPK = department?.PK.ToGuid() ?? Env.CurrentDepartmentPK;
			return new UserContext(Env.CurrentUserPK, branch.PK.ToGuid(), departmentPK);
		}

		public IDisposable SwitchEnvToCompany(GlbCompany company, GlbDepartment department = null)
		{
			return Env.SetTemporaryUserContext(GetUserContext(company, department: department));
		}

		public IDisposable SwitchEnvToBranch(GlbBranch branch, GlbDepartment department = null)
		{
			return Env.SetTemporaryUserContext(GetUserContext(branch, department: department));
		}

		public GlbStaff CreateGlbStaff(string userCode = "TST", string userName = "UserForTest")
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = userCode;
			staff.GS_LoginName = userName;
			return staff;
		}

		#endregion

		OrgHeader fTestOrganisation;
		public OrgHeader TestOrganisation
		{
			get
			{
				if (fTestOrganisation == null)
				{
					fTestOrganisation = CreateOrgHeader("Org", true, true, true, true, true, true);
				}

				return fTestOrganisation;
			}
		}

		OrgHeader fDebtor;
		public OrgHeader Debtor
		{
			get
			{
				if (fDebtor == null)
				{
					fDebtor = CreateOrgHeader("Debtor", false, true, false, false, true, false);
				}

				return fDebtor;
			}
		}

		OrgHeader fDebtorSisterOrgProxy;
		OrgHeader fDebtorSisterOrgProxy2;
		public OrgHeader DebtorSisterOrgProxy => fDebtorSisterOrgProxy ?? (fDebtorSisterOrgProxy = CreateOrgHeader("DebtorSOP", false, true, false, false, true, false, true));
		public OrgHeader DebtorSisterOrgProxy2 => fDebtorSisterOrgProxy2 ?? (fDebtorSisterOrgProxy2 = CreateOrgHeader("DebtorSOP2", false, true, false, false, true, false, true));

		OrgHeader fDebtor1;
		public OrgHeader Debtor1
		{
			get
			{
				if (fDebtor1 == null)
				{
					fDebtor1 = CreateOrgHeader("Debtor1", false, true, false, false, true, false);
				}

				return fDebtor1;
			}
		}

		#region DebtorTR

		protected OrgHeader fDebtorTR;
		public OrgHeader DebtorTR
		{
			get
			{
				if (fDebtorTR == null)
				{
					fDebtorTR = CreateOrgHeader("DebtorTR", false, true, true, true, true, true);
					fDebtorTR.CompanyData.OverrideBankAccountFromDebtorGroup = true;
					fDebtorTR.CompanyData.ARBankAccountToDisplay = AUDBankAccount.PK;
					AUDBankAccount.IBAN = "TR6211111111111111";
				}

				return fDebtorTR;
			}
		}

		#endregion

		#region DebtorDE

		protected OrgHeader fDebtorDE;
		public OrgHeader DebtorDE
		{
			get
			{
				if (fDebtorDE == null)
				{
					fDebtorDE = CreateOrgHeaderDE("DebtorDE", false, true, false, false, true, false, true);
				}

				return fDebtorDE;
			}
		}

		#endregion
		#region CreditorTR

		protected OrgHeader fCreditorTR;
		public OrgHeader CreditorTR
		{
			get
			{
				if (fCreditorTR == null)
				{
					fCreditorTR = CreateOrgHeader("CreditorTR", true, true, true, true, true, true);
				}

				return fCreditorTR;
			}
		}

		#endregion

		#region CashBasisVAT

		public void SetupCashBasisVAT()
		{
			var currentCompany = GetNewFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsGSTCashBasis = true;
			currentCompany.Factory.Save();

			var taxRecognitionDefaultingRules = new TaxRecognitionDefaultingRules()
			{
				APInputServices = TaxRecognitionDefaultingRules.RecognitionTypesCashCode,
				AROutputServices = TaxRecognitionDefaultingRules.RecognitionTypesCashCode
			};
			AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, taxRecognitionDefaultingRules);
		}

		public AccCashBasisVAT CreateCashBasisVAT(AccTransactionLines line, ZDecimal taxBaseAmount, ZDecimal taxAmount, AccTransactionMatchLink matchLink = null)
		{
			return CreateCashBasisVAT(line, taxBaseAmount, taxAmount, matchLink != null ? matchLink.AP_MatchDate : line.TransactionHeader.AH_FullyPaidDate, matchLink);
		}

		public AccCashBasisVAT CreateCashBasisVAT(AccTransactionLines line, ZDecimal taxBaseAmount, ZDecimal taxAmount, ZDateTime postDate, AccTransactionMatchLink matchLink = null)
		{
			var result = Factory.New<AccCashBasisVAT>();
			result.YC_AL_TransactionLine = line.PK;
			result.YC_TaxBaseAmount = taxBaseAmount;
			result.YC_TaxAmount = taxAmount;
			result.YC_PostDate = postDate;
			if (matchLink != null)
			{
				result.YC_MatchGroupNum = matchLink.AP_MatchGroupNum;
			}

			return result;
		}

		public AccCashBasisVAT CreateInvoiceWithCashBasisVAT(Type invoiceType, ZString transactionNum, RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal taxBaseAmountForCashBasisVAT, decimal taxAmountForCashBasisVAT)
		{
			var invoice = CreateInvoiceWithLine(invoiceType, transactionNum, currency, exchangeRate, oSExTaxAmount, oSTaxAmount, localExTaxAmount, localTaxAmount);
			var invoiceLine = invoice.Lines[0];
			invoiceLine.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			return CreateCashBasisVAT(invoiceLine, taxBaseAmountForCashBasisVAT, taxAmountForCashBasisVAT, ZDateTime.Now);
		}

		public InvoicingBase CreateInvoiceWithCashVATLine(Type invoiceType, decimal osExTaxAmount, decimal osTaxAmount, bool isJobRelated = true, AccChargeCode chargeCode = null, string departmentCode = null)
		{
			SetupCashBasisVAT();

			var invoice = (InvoicingBase)Factory.New(invoiceType);
			invoice.AH_OH = TestOrganisation.PK;
			invoice.AH_TransactionNum = AccountingNumberFountainWrapperFactory.Instance.APInvoiceNo.Generate(invoice);

			CreateCashVATLines(invoice, osExTaxAmount, osTaxAmount, isJobRelated, chargeCode, departmentCode);

			return invoice;
		}

		public InvoicingLineBase CreateCashVATLines(InvoicingBase invoice, decimal osExTaxAmount, decimal osTaxAmount, bool isJobRelated = true, AccChargeCode chargeCode = null, string departmentCode = null)
		{
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = GLHeader1.PK;

			if (isJobRelated)
			{
				line.GenericCharge = chargeCode != null ? chargeCode.PK : CC1.PK;
				var job = CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false, departmentCode);
				line.AL_JH = job.PK;
			}
			else
			{
				line.GenericCharge = OverheadChargeCode.PK;
			}
			line.AL_OSExTaxAmount = osExTaxAmount;
			line.AL_OSTaxAmount = osTaxAmount;

			if (isJobRelated)
			{
				CreateCharge(line);
			}

			return line;
		}

		#endregion

		#region New OS Outstanding Amount Feature

		public void UpdateInvoiceForNewOSOutstandingAmountFeature(TransactionHeader transaction)
		{
			transaction.MakeOSOutstandingAmountApplicable(transaction.AH_OSTotal);
		}

		public void UpdateMatchLinkForNewOSOutstandingAmountFeature(TransactionHeader transaction, ZDecimal? osAmountPaid = null)
		{
			var transactionMatchLink = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, transaction.PK))
				.OrderByDescending(x => x.AP_MatchDate)
				.FirstOrDefault(x => !x.IsInDatabase);
			transactionMatchLink.AP_OSAmount = osAmountPaid ?? transaction.AH_OSOutstandingAmount;
			transaction.AH_OSOutstandingAmount -= osAmountPaid ?? transaction.AH_OSOutstandingAmount;
		}

		public ARReceipt CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null, string matchGroupNumber = null)
		{
			var receipt = CreateAndMatchARReceiptForARInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid, matchGroupNumber);
			UpdateInvoiceForNewOSOutstandingAmountFeature(receipt);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(receipt);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(invoice, amountPaid);

			return receipt;
		}

		public APPayment CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null, string matchGroupNumber = null)
		{
			var payment = CreateAndMatchAPPaymentForAPInvoice(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid, matchGroupNumber, 1m, true);
			UpdateInvoiceForNewOSOutstandingAmountFeature(payment);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(payment);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(invoice, amountPaid);

			return payment;
		}

		public TransactionHeader CreateAndMatchMiscellaneousTransactionForNewOSOutstandingAmountFeature(InvoicingBase invoice, int daysPaid, ZDecimal? amountPaid = null, string matchGroupNumber = null)
		{
			var miscellaneousTransaction = CreateAndMatchMiscellaneousTransaction(invoice, ZDateTime.Today.AddDays(daysPaid), amountPaid, matchGroupNumber);
			UpdateInvoiceForNewOSOutstandingAmountFeature(miscellaneousTransaction);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(miscellaneousTransaction);
			UpdateMatchLinkForNewOSOutstandingAmountFeature(invoice, amountPaid);

			return miscellaneousTransaction;
		}

		public TransactionHeader CreateAndMatchMiscellaneousTransaction(InvoicingBase invoice, ZDateTime? matchingDate = null, ZDecimal? amountPaid = null, string matchGroupNumber = null)
		{
			bool payInFull = !amountPaid.HasValue;
			var localMatchingDate = matchingDate ?? ZDateTime.Now;
			var invoiceAsIMatching = (IMatching)invoice;
			invoiceAsIMatching.CurrentMatchGroup.RemoveAll();
			invoiceAsIMatching.OSPartialPaymentAmount = invoiceAsIMatching.OSOutstandingAmount;
			if (payInFull)
			{
				invoiceAsIMatching.FullyPay(localMatchingDate);
			}
			else
			{
				invoiceAsIMatching.OSPartialPaymentAmount = amountPaid.Value;
				invoiceAsIMatching.PartiallyPay();
			}
			invoiceAsIMatching.GenerateMatchLinks();

			var creator = invoice.AH_Ledger == LedgerTypes.AccountsPayable ?
				new MiscellaneousTransactionCreatorAP(Factory) :
				new MiscellaneousTransactionCreatorAR(Factory) as MiscellaneousTransactionCreator;
			var osAmount = invoice.Company.GC_IsReciprocal ?
					-invoiceAsIMatching.OSPartialPaymentAmount * invoice.AH_ExchangeRate :
					-invoiceAsIMatching.OSPartialPaymentAmount / invoice.AH_ExchangeRate;
			var overpayment = creator.CreateOverpayment(osAmount, 1m, invoice.Header);

			var overpaymentAsIMatching = (IMatching)overpayment;
			overpaymentAsIMatching.FullyPay(localMatchingDate);
			overpaymentAsIMatching.GenerateMatchLinks();

			invoiceAsIMatching.CurrentMatchGroup.AddRange(overpaymentAsIMatching.CurrentMatchGroup);
			overpaymentAsIMatching.CurrentMatchGroup.RemoveAll();
			invoiceAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber ?? "M001", localMatchingDate);

			return overpayment;
		}

		#endregion

		#region Matching

		public AccTransLinePay CreateLineMatchLink(TransactionMatchLink matchLink, InvoicingLineBase line, decimal? amountPaid = null, decimal? oSAmountPaid = null)
		{
			var lineMatchLink = Factory.New<AccTransLinePay>();
			lineMatchLink.A7_AP = matchLink.PK;
			lineMatchLink.A7_AL = line.PK;
			lineMatchLink.A7_Amount = amountPaid ?? line.AL_LineAmount + line.AL_GSTVAT;
			lineMatchLink.A7_OSAmount = oSAmountPaid
				?? line.Company.GetExchangeRate().LocalToForeign(lineMatchLink.A7_Amount, line.AL_ExchangeRate, line.AL_RX_NKTransactionCurrency);

			return lineMatchLink;
		}

		public TransactionMatchLink CreateMatchLink(TransactionHeader invoice, decimal? amountPaid = null, ZDateTime? matchDate = null, string matchGroupNum = null, decimal? oSAmountPaid = null)
		{
			var matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = amountPaid ?? invoice.AH_LocalTotal;
			matchLink.AP_OSAmount = invoice.AH_IsOSOutstandingAmountApplicable
				? oSAmountPaid ?? invoice.Company.GetExchangeRate().LocalToForeign(matchLink.AP_Amount, invoice.GetHighPrecisionExchangeRate(), invoice.AH_RX_NKTransactionCurrency)
				: 0m;
			matchLink.AP_MatchDate = matchDate ?? invoice.AH_PostDate;
			matchLink.AP_MatchGroupNum = matchGroupNum ?? "M0001";

			return matchLink;
		}

		public APPayment CreateAndMatchAPPaymentForAPInvoice(InvoicingBase apInvoice, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null, decimal? exchangeRate = null, bool useLocalPaidAmount = false)
		{
			return CreateAndMatchAPPaymentForAPInvoice(apInvoice, AUDBankAccount.PK, matchingDate, partPaidAmount, matchGroupNumber, exchangeRate, useLocalPaidAmount);
		}

		public APPayment CreateAndMatchAPPaymentForAPInvoice(InvoicingBase apInvoice, ZGuid bankAccountPK, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null, decimal? exchangeRate = null, bool useLocalPaidAmount = false)
		{
			bool payInFull = !partPaidAmount.HasValue;
			var localMatchingDate = matchingDate ?? ZDateTime.Now;
			var apInvoiceAsIMatching = (IMatching)apInvoice;
			apInvoiceAsIMatching.CurrentMatchGroup.RemoveAll();
			apInvoiceAsIMatching.OSPartialPaymentAmount = apInvoiceAsIMatching.OSOutstandingAmount;
			if (payInFull)
			{
				apInvoiceAsIMatching.FullyPay(localMatchingDate);
			}
			else
			{
				apInvoiceAsIMatching.OSPartialPaymentAmount = partPaidAmount.Value;
				apInvoiceAsIMatching.PartiallyPay();
			}
			apInvoiceAsIMatching.GenerateMatchLinks();

			APPayment apPayment;
			if (exchangeRate == null)
			{
				apPayment = CreateAPPayment(apInvoice.AH_ExchangeRate, -apInvoiceAsIMatching.OSPartialPaymentAmount, apInvoice.AH_PostDate, apInvoice.AH_DueDate, apInvoice.AH_OH, bankAccountPK);
			}
			else if (useLocalPaidAmount)
			{
				var osExTaxAmount = apInvoice.Company.GC_IsReciprocal ?
						-apInvoiceAsIMatching.OSPartialPaymentAmount * apInvoice.AH_ExchangeRate :
						-apInvoiceAsIMatching.OSPartialPaymentAmount / apInvoice.AH_ExchangeRate;
				apPayment = CreateAPPayment(exchangeRate.Value, osExTaxAmount, apInvoice.AH_PostDate, apInvoice.AH_DueDate, apInvoice.AH_OH, bankAccountPK);
			}
			else
			{
				apPayment = CreateAPPayment(exchangeRate.Value, -apInvoiceAsIMatching.OSPartialPaymentAmount, apInvoice.AH_PostDate, apInvoice.AH_DueDate, apInvoice.AH_OH, bankAccountPK);
			}

			var apPaymentAsIMatching = (IMatching)apPayment;
			apPaymentAsIMatching.FullyPay(localMatchingDate);
			apPaymentAsIMatching.GenerateMatchLinks();

			apInvoiceAsIMatching.CurrentMatchGroup.AddRange(apPaymentAsIMatching.CurrentMatchGroup);
			apPaymentAsIMatching.CurrentMatchGroup.RemoveAll();
			apInvoiceAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber ?? "M001", localMatchingDate);

			return apPayment;
		}

		public ARReceipt CreateAndMatchARReceiptForARInvoice(InvoicingBase arInvoice, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null, ZDateTime? postAndDueDate = null)
		{
			return CreateAndMatchARReceiptForARInvoice(arInvoice, AUDBankAccount.PK, matchingDate, partPaidAmount, matchGroupNumber, postAndDueDate);
		}

		public ARReceipt CreateAndMatchARReceiptForARInvoice(InvoicingBase arInvoice, ZGuid bankAccountPK, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null, ZDateTime? postAndDueDate = null)
		{
			bool payInFull = !partPaidAmount.HasValue;
			var localMatchingDate = matchingDate ?? ZDateTime.Now;
			var arInvoiceASIMatching = (IMatching)arInvoice;
			arInvoiceASIMatching.CurrentMatchGroup.RemoveAll();
			arInvoiceASIMatching.OSPartialPaymentAmount = arInvoiceASIMatching.OSOutstandingAmount;
			if (payInFull)
			{
				arInvoiceASIMatching.FullyPay(localMatchingDate);
			}
			else
			{
				arInvoiceASIMatching.OSPartialPaymentAmount = partPaidAmount.Value;
				arInvoiceASIMatching.PartiallyPay();
			}
			arInvoiceASIMatching.GenerateMatchLinks();

			var osExTaxAmount = arInvoice.Company.GC_IsReciprocal ?
				arInvoiceASIMatching.OSPartialPaymentAmount * arInvoice.AH_ExchangeRate :
				arInvoiceASIMatching.OSPartialPaymentAmount / arInvoice.AH_ExchangeRate;
			var arReceipt = CreateARReceipt(1m, osExTaxAmount, postAndDueDate ?? arInvoice.AH_PostDate, postAndDueDate ?? arInvoice.AH_DueDate, arInvoice.AH_OH, bankAccountPK);

			var arReceiptAsIMatching = (IMatching)arReceipt;
			arReceiptAsIMatching.FullyPay(localMatchingDate);
			arReceiptAsIMatching.GenerateMatchLinks();

			arInvoiceASIMatching.CurrentMatchGroup.AddRange(arReceiptAsIMatching.CurrentMatchGroup);
			arReceiptAsIMatching.CurrentMatchGroup.RemoveAll();
			arInvoiceASIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber ?? "M001", localMatchingDate);

			TestObjectCreator.SetupMatchLinkMatchDate(arInvoiceASIMatching);

			return arReceipt;
		}

		public ARReceipt CreateAndMatchARReceiptForCashAdvanceMatchingJournal(ARJournal cashAdvanceMattchingJournal, ZDateTime? matchingDate = null, string matchGroupNumber = null)
		{
			var localMatchingDate = matchingDate ?? ZDateTime.Now;
			var cahJournalASIMatching = (IMatching)cashAdvanceMattchingJournal;
			cahJournalASIMatching.CurrentMatchGroup.RemoveAll();
			cahJournalASIMatching.OSPartialPaymentAmount = cahJournalASIMatching.OSOutstandingAmount;
			cahJournalASIMatching.FullyPay(localMatchingDate);
			cahJournalASIMatching.GenerateMatchLinks();

			ARReceipt arReceipt = CreateARReceipt(1m, cashAdvanceMattchingJournal.Company.GC_IsReciprocal ? cahJournalASIMatching.OSPartialPaymentAmount * cashAdvanceMattchingJournal.AH_ExchangeRate : cahJournalASIMatching.OSPartialPaymentAmount / cashAdvanceMattchingJournal.AH_ExchangeRate, cashAdvanceMattchingJournal.AH_PostDate, cashAdvanceMattchingJournal.AH_DueDate, cashAdvanceMattchingJournal.AH_OH, AUDBankAccount.PK);
			var arReceiptAsIMatching = (IMatching)arReceipt;
			arReceiptAsIMatching.FullyPay(localMatchingDate);
			arReceiptAsIMatching.GenerateMatchLinks();

			arReceiptAsIMatching.CurrentMatchGroup.AddRange(cahJournalASIMatching.CurrentMatchGroup);
			cahJournalASIMatching.CurrentMatchGroup.RemoveAll();
			arReceiptAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber ?? "M001", localMatchingDate);
			SetupMatchLinkMatchDate(arReceiptAsIMatching);

			return arReceipt;
		}

		public APPayment CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(APJournal cashAdvanceMattchingJournal, ZDateTime? matchingDate = null, string matchGroupNumber = null)
		{
			var localMatchingDate = matchingDate ?? ZDateTime.Now;
			var cahJournalASIMatching = (IMatching)cashAdvanceMattchingJournal;
			cahJournalASIMatching.CurrentMatchGroup.RemoveAll();
			cahJournalASIMatching.OSPartialPaymentAmount = cahJournalASIMatching.OSOutstandingAmount;
			cahJournalASIMatching.FullyPay(localMatchingDate);
			cahJournalASIMatching.GenerateMatchLinks();

			APPayment apPayment = CreateAPPayment(1m, -1 * (cashAdvanceMattchingJournal.Company.GC_IsReciprocal ? cahJournalASIMatching.OSPartialPaymentAmount * cashAdvanceMattchingJournal.AH_ExchangeRate : cahJournalASIMatching.OSPartialPaymentAmount / cashAdvanceMattchingJournal.AH_ExchangeRate), cashAdvanceMattchingJournal.AH_PostDate, cashAdvanceMattchingJournal.AH_DueDate, cashAdvanceMattchingJournal.AH_OH, AUDBankAccount.PK);
			var apPaymentAsIMatching = (IMatching)apPayment;
			apPaymentAsIMatching.FullyPay(localMatchingDate);
			apPaymentAsIMatching.GenerateMatchLinks();

			apPaymentAsIMatching.CurrentMatchGroup.AddRange(cahJournalASIMatching.CurrentMatchGroup);
			cahJournalASIMatching.CurrentMatchGroup.RemoveAll();
			apPaymentAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber ?? "M001", localMatchingDate);
			SetupMatchLinkMatchDate(apPaymentAsIMatching);

			return apPayment;
		}

		public TransactionMatchLink CreateMatchLinkToPayAPInvoice(InvoicingBase apInvoice, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null)
		{
			CreateAndMatchAPPaymentForAPInvoice(apInvoice, matchingDate, partPaidAmount, matchGroupNumber);
			var matchLinks = ((IMatching)apInvoice).Matchlinks;
			matchLinks.Load();

			return matchLinks[0];
		}

		public TransactionMatchLink CreateMatchLinkToPayARInvoice(InvoicingBase arInvoice, ZDateTime? matchingDate = null, decimal? partPaidAmount = null, string matchGroupNumber = null)
		{
			CreateAndMatchARReceiptForARInvoice(arInvoice, matchingDate, partPaidAmount, matchGroupNumber);
			var matchLinks = ((IMatching)arInvoice).Matchlinks;
			matchLinks.Load();

			return matchLinks[0];
		}

		public void UnmatchTransaction(string matchGroupNumber)
		{
			var unmatchingRow = new UnmatchingRow(Factory) { MatchGroupNum = matchGroupNumber };
			unmatchingRow.UnmatchAnyGroup();
		}

		#endregion

		#region Reversing

		public IReversing ReverseTransaction(IReversing originalTransaction, out string cantReverseErrorMessage)
		{
			var reversing = new ReversingFactory().NewReversing(originalTransaction);
			return DoReversing(reversing, out cantReverseErrorMessage);
		}

		public IReversing WriteOffBadDebt(IBadDebtWritingOff badDebtTransaction, out string cantReverseErrorMessage)
		{
			badDebtTransaction.IsWritingOff = true;
			var reversing = new BadDebtWritingOffFactory().NewWritingOff(badDebtTransaction);
			return DoReversing(reversing, out cantReverseErrorMessage);
		}

		IReversing DoReversing(ReversingBase reversing, out string cantReverseErrorMessage)
		{
			if (!reversing.CanReverseTransaction)
			{
				cantReverseErrorMessage = reversing.CantReverseErrorMessage;
				return null;
			}
			else
			{
				reversing.Reverse();

				cantReverseErrorMessage = null;
				return reversing.ReverseTransaction;
			}
		}

		public IReversing CreateReversalTransaction(IReversing invoice, string transactionReference, string transactionNumber = "")
		{
			var reversalFactory = new ReversingFactory();
			var reversing = reversalFactory.NewReversing(invoice);
			reversing.Reverse();
			var reverseTransaction = reversing.ReverseTransaction;
			(reverseTransaction as InvoicingBase).AH_TransactionReference = transactionReference;
			(reverseTransaction as InvoicingBase).AH_TransactionNum = transactionNumber;
			return reverseTransaction;
		}

		#endregion

		#region Amending

		public (IAmending amendTransaction, string errorMessage) AmendARTransaction(string transactionType, InvoicingBase invoice)
		{
			var errorMessage = string.Empty;

			var securityHelper = new JobInvoicingSecurityHelper(invoice.InvoicingJob?.PlugInData?.InvoicingSupporter?.JobInvoicingSecurity);
			IAmending amendingInvoice = CreditNoteAmendingHelper.AmendARTransaction(transactionType, invoice, securityHelper,
				(message, caption) => { }, (message, caption) => errorMessage = message);

			return (amendingInvoice, errorMessage);
		}

		public (IAmending amendTransaction, string errorMessage) AmendAPTransaction(InvoicingBase invoice)
		{
			var errorMessage = string.Empty;

			var securityHelper = new JobInvoicingSecurityHelper(invoice.InvoicingJob?.PlugInData?.InvoicingSupporter?.JobInvoicingSecurity);
			IAmending amendingInvoice = CreditNoteAmendingHelper.AmendAPTransaction(invoice, securityHelper,
				(message, caption) => { }, (message, caption) => errorMessage = message);

			return (amendingInvoice, errorMessage);
		}

		public static IDisposable SetupAmendingTransactionCopyExchangeRateRegistry(ZString ledgerType, ZGuid companyPk, bool enableRegistry)
		{
			if (companyPk.IsEmpty)
			{
				throw new ArgumentException("companyPk cannot be empty.");
			}
			else if (ledgerType == LedgerTypes.AccountsPayable)
			{
				return AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP.SetTemporaryValue(companyPk.ToGuid(), Guid.Empty, Guid.Empty, enableRegistry);
			}
			else if (ledgerType == LedgerTypes.AccountsReceivable)
			{
				return AccountingConfigurationRegistry.Instance.AmendingTransactionCopyExchangeRateFromOriginalTransaction.SetTemporaryValue(companyPk.ToGuid(), Guid.Empty, Guid.Empty, enableRegistry);
			}
			else
			{
				throw new ArgumentException("ledgerType must be AR or AP. Amending Transaction Copy Exchange Rate from Original Transaction registry can be configured for AR and AP Transactions only.");
			}
		}

		#endregion

		#region Approval Requests

		public InvoiceType CreateAPInvoiceWithApprovalRequest<InvoiceType>(OrgHeader creditor, ZDecimal osExTaxAmount) where InvoiceType : InvoicingBase
		{
			var invoice = CreateAPInvoiceForApprovalRequest<InvoiceType>(creditor, osExTaxAmount);
			CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

			return invoice;
		}

		public APInvoiceChargesApprovalRequest CreateApprovalRequestWithLinkedInvoice<InvoiceType>(OrgHeader creditor, ZDecimal osExTaxAmount, string invoiceNumber = null) where InvoiceType : InvoicingBase
		{
			var newFactory = GetNewFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			var invoice = testObjectCreatorInNewFactory.CreateAPInvoiceForApprovalRequest<InvoiceType>(creditor, osExTaxAmount, invoiceNumber);

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.XP_ReasonDescription = "Desc";
			request.PrepareFoSaving();
			Factory.Save();

			return request;
		}

		public InvoiceType CreateAPInvoiceForApprovalRequest<InvoiceType>(OrgHeader creditor, ZDecimal osExTaxAmount, string invoiceNumber = null) where InvoiceType : InvoicingBase
		{
			return (InvoiceType)CreateAPInvoiceForApprovalRequest(typeof(InvoiceType), creditor, osExTaxAmount, invoiceNumber);
		}

		public InvoicingBase CreateAPInvoiceForApprovalRequest(Type invoiceType, OrgHeader creditor, ZDecimal osExTaxAmount, string invoiceNumber = null)
		{
			InvoicingBase invoice;
			if (invoiceNumber != null)
			{
				invoice = CreateInvoice(invoiceType, invoiceNumber, organisation: creditor);
			}
			else
			{
				invoice = CreateInvoice(invoiceType, organisation: creditor);
			}
			invoice.FillWithValidTestData();

			var line = CreateInvoiceLine(invoice, osExTaxAmount, setTaxes: false);
			line.GenericCharge = GLHeader1.PK;
			line.AL_Desc = "Desc";
			line.AL_AT = GST1.PK;
			line.AL_AG = GLHeader1.PK;

			return invoice;
		}

		public APInvoiceChargesApprovalRequest CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(InvoicingBase invoice)
		{
			var request = GetNewFactory().New<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.XP_ReasonDescription = "Desc";
			request.PrepareFoSaving();
			request.Factory.Save();

			return request;
		}

		public void SetupInvoiceApprovalRequestAuthorization()
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 100;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 100;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		public ARCreditNoteApprovalRequest CreateARCreditNoteApprovalRequest(decimal amount, string statusType = Constants.GenApprovalRequestApprovalStatus.Requested)
		{
			var job = CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = CreateCharge(job, CC3, GetRandomString(5), null, 0m, null, AUD, -(amount), AALSHI);
			var creditNote = CreateARCreditNote(GetRandomString(5), AALSHI);
			var creditNoteLine = CreateARCreditNoteLine(creditNote, job, CC3, amount);
			var request = Factory.New<ARCreditNoteApprovalRequest>();

			creditNoteLine.AL_AT = Guid.Empty;
			charge.ReverseAccrual(DateTime.Now);
			charge.JR_AL_ARLine = creditNoteLine.PK;
			request.Initialize(new[] { creditNote }, job.PK, JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			request.XP_ApprovalStatus = statusType;
			return request;
		}

		public ARCreditNoteApprovalRequest CreateInvoiceReversalApprovalRequest(decimal amount, string statusType = Constants.GenApprovalRequestApprovalStatus.Requested)
		{
			var job = CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = CreateCharge(job, CC3, GetRandomString(5), null, 0m, null, AUD, amount, AALSHI);
			var invoice = CreateInvoice(typeof(ARInvoice), GetRandomString(5), AUD, amount, AALSHI);
			invoice.Lines.Add(CreateRevenueLine(charge, invoice.PK));
			var request = Factory.New<ARCreditNoteApprovalRequest>();

			request.ChangeApprovalTypeForInvoiceReversal();
			request.Initialize(new[] { invoice }, invoice.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			request.XP_ApprovalStatus = statusType;
			return request;
		}

		#endregion

		#region SalesGroup

		public AccGroups CreateSalesGroup(ZString groupName)
		{
			var salesGroup = Factory.NewWithValidTestData<AccGroups>();
			salesGroup.AR_Code = groupName;

			return salesGroup;
		}

		protected AccGroups fAR1;
		public AccGroups AR1
		{
			get
			{
				if (fAR1 == null)
				{
					fAR1 = Factory.LoadFromNaturalKey<AccGroups>(AccGroupsSchema.AR_Code, "AR1");
					if (fAR1 == null)
					{
						fAR1 = CreateSalesGroup("AR1");
					}
				}
				return fAR1;
			}
		}

		#endregion

		#region Staff

		protected GlbStaff fGS1;
		public GlbStaff GS1
		{
			get
			{
				if (fGS1 == null)
				{
					fGS1 = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "GS1");
					if (fGS1 == null)
					{
						fGS1 = CreateStaff("GS1");
					}
				}
				return fGS1;
			}
		}

		protected GlbGroup fGG1;
		public GlbGroup GG1
		{
			get
			{
				if (fGG1 == null)
				{
					fGG1 = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "GG1");
					if (fGG1 == null)
					{
						fGG1 = CreateStaffGroup("GG1");
					}
				}
				return fGG1;
			}
		}

		public void SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(Guid departmentPK, Guid branchPK, ZGuid staffPK, bool isAllowed)
		{
			var factory = new BusinessObjectFactory();
			var firstLevelGlobalSecurityQuery = new ZQuery(GlbSecuritySchema.GU_GS, staffPK);
			firstLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_GB, branchPK);
			firstLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_GE, departmentPK);
			firstLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code);

			var firstLevelSecurity = factory.Load<GlbSecurity>(firstLevelGlobalSecurityQuery).FirstOrDefault();

			if (firstLevelSecurity == null)
			{
				var creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity = factory.New<GlbSecurity>();
				creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity.GU_GS = staffPK;
				creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity.GU_GB = branchPK;
				creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity.GU_GE = departmentPK;
				creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity.GU_SecurityRight = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code;
				creditAdjustmentNotePostingApprovalFirstLevelApprovalSecurity.GU_SecurityItemIsAllowed = isAllowed;
			}
			else
			{
				firstLevelSecurity.GU_SecurityItemIsAllowed = isAllowed;
			}

			var secondLevelGlobalSecurityQuery = new ZQuery(GlbSecuritySchema.GU_GS, staffPK);
			secondLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_GB, branchPK);
			secondLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_GE, departmentPK);
			secondLevelGlobalSecurityQuery.AddToFilter(GlbSecuritySchema.GU_SecurityRight, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code);
			var secondLevelSecurity = factory.Load<GlbSecurity>(secondLevelGlobalSecurityQuery).FirstOrDefault();
			if (secondLevelSecurity == null)
			{
				var creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity = factory.New<GlbSecurity>();
				creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity.GU_GS = staffPK;
				creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity.GU_GB = branchPK;
				creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity.GU_GE = departmentPK;
				creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity.GU_SecurityRight = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code;
				creditAdjustmentNotePostingApprovalSecondLevelApprovalSecurity.GU_SecurityItemIsAllowed = isAllowed;
			}
			else
			{
				secondLevelSecurity.GU_SecurityItemIsAllowed = isAllowed;
			}

			factory.Save();

			Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());
		}

		public GlbStaff CreateStaffWithSecurityRights(ZString loginName, ZString? code, ZString securityLevelCode, string password = null, bool isAllowed = true)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = loginName;
			if (code.HasValue)
			{
				staff.GS_Code = code.Value;
			}
			if (password != null)
			{
				staff.StaffPlainTextPassword = password;
			}
			staff.GS_ChangePasswordAtNextLogin = false;
			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = securityLevelCode;
			staffSecurity.GU_SecurityItemIsAllowed = isAllowed;
			Factory.Save();
			return staff;
		}

		public GlbStaff Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = CreateStaff("TST");
				}
				return fStaff;
			}
		}
		GlbStaff fStaff;

		public GlbStaff CreateStaff(ZString staffCode)
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, staffCode));
			if (staff != null)
			{
				return staff;
			}

			staff = new BusinessObjectFactory().NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode;
			staff.Factory.Save();

			return Factory.Load<GlbStaff>(staff.PK);
		}

		public GlbGroup CreateStaffGroup(ZString staffGroupCode)
		{
			var staffGroup = Factory.NewWithValidTestData<GlbGroup>();
			staffGroup.GG_Code = staffGroupCode;

			return staffGroup;
		}

		public void SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid companyPK, CodeDescriptionPairList statusesWhenARInvoicePostingdoesnotChangeStatus)
		{
			var defaultValue = new CodeDescriptionBoolDisallowNewCollection(new JobHeaderStatusList());
			foreach (CodeDescriptionBool item in defaultValue)
			{
				item.Bool = statusesWhenARInvoicePostingdoesnotChangeStatus != null && statusesWhenARInvoicePostingdoesnotChangeStatus.ContainsCode(item.Code);
			}
			AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetValue(companyPK, Guid.Empty, Guid.Empty, defaultValue);
		}

		#endregion

		public TransactionCreatorHashtable PostJobAsBillingTab(Job job, JobInvoicingPostingOption option)
		{
			var postManager = new InvoicingPostManager(job);
			return postManager.CreateTransactions(option);
		}

		public InvoicingBase[] PostConsolAsBillingTab(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs,
			IJobCostingPlugIn consol, ApportionmentListing apportionments, JobInvoicingPostingOption option)
		{
			var postManager = new ConsolInvoicingPostManager(fallbackFactory, jobs, consol, apportionments, null);
			var transactionHashTable = postManager.CreateTransactions(option);
			return transactionHashTable.GetAllAPInvoicesAndCreditNotes();
		}

		#region Security
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		public void ResetSecurityCore()
		{
			var security = Env.Security;
			var uc = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(uc);
			if (security == Env.Security)
			{
				throw new ApplicationException("Expected secuirty object to change");
			}
		}
		#endregion

		#region AccComplianceReport

		public void CreateConfigurationForComplianceReport(AccComplianceReport report, string baseTablePrefix = AccTransactionLinesSchema.Constants.Prefix,
			string reportLineGrouping = "", string goodsAndService = "", string taxRegistrationType = null, bool includeQueuedForPreviousPeriod = false, string reportLineOrdering = "", string roundingType = "", int rounding = 2)
		{
			var registryValue = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetValueWithoutFallback(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);

			var configuration = registryValue.ToArray<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == report.ACR_ReportType);
			if (configuration == null)
			{
				CreateConfigurationForComplianceReport(registryValue
				, report.Company.GC_RN_NKCountryCode
				, report.ACR_ReportType
				, report.ACR_ReportType + " Report Title"
				, baseTablePrefix
				, reportLineGrouping
				, goodsAndService
				, report.ACR_Periodicity
				, string.IsNullOrEmpty(taxRegistrationType) ? new OrgCodeLists().CustomsCodes_List(report.Company.Country).ToArray()[0].Code : taxRegistrationType
				, ZGuid.Empty
				, includeQueuedForPreviousPeriod
				, reportLineOrdering
				, roundingType
				, rounding);

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, registryValue);
			}
		}

		public ComplianceReportConfiguration CreateConfigurationForComplianceReport(AccComplianceReport report, ComplianceReportConfigurationCollection configCollection, string baseTablePrefix = AccTransactionLinesSchema.Constants.Prefix,
			string reportLineGrouping = "", string goodsAndService = "", string taxRegistrationType = null, bool includeQueuedForPreviousPeriod = false, string reportLineOrdering = "")
		{
			if (configCollection != null)
			{
				return CreateConfigurationForComplianceReport(configCollection
				, report.Company.GC_RN_NKCountryCode
				, report.ACR_ReportType
				, report.ACR_ReportType + " Report Title"
				, baseTablePrefix
				, reportLineGrouping
				, goodsAndService
				, report.ACR_Periodicity
				, string.IsNullOrEmpty(taxRegistrationType) ? new OrgCodeLists().CustomsCodes_List(report.Company.Country).ToArray()[0].Code : taxRegistrationType
				, ZGuid.Empty
				, includeQueuedForPreviousPeriod
				, reportLineOrdering);
			}

			return null;
		}

		public ComplianceReportConfiguration CreateConfigurationForComplianceReport(ComplianceReportConfigurationCollection configCollection, string countryCode, string reportCode, string reportTitle, string baseTablePrefix,
			string reportLineGrouping, string goodsAndService, string periodicity, string taxRegistrationType, ZGuid recipientOrgPK, bool includeQueuedForPreviousPeriod, string reportLineOrdering = "", string roundingType = "", int rounding = 2)
		{
			if (configCollection != null)
			{
				var configuration = configCollection.AddNew();
				configuration.Country = countryCode;
				configuration.ReportCode = reportCode;
				configuration.ReportTitle = reportTitle;
				configuration.ReportBaseTablePrefix = baseTablePrefix;
				configuration.ReportLineGrouping = reportLineGrouping;
				configuration.ReportLineOrdering = reportLineOrdering;
				configuration.GoodsServiceType = goodsAndService;
				configuration.ReportPeriodicity = periodicity;
				configuration.TaxRegistrationType = taxRegistrationType;
				configuration.IncludeQueuedForPreviousPeriod = includeQueuedForPreviousPeriod;
				configuration.ReportAmountsRoundingType = roundingType;
				configuration.ReportAmountsRoundingTruncating = rounding;
				if (recipientOrgPK.IsValid)
				{
					configuration.RecipientOrgPK = recipientOrgPK;
				}
				return configuration;
			}

			return null;
		}

		public ComplianceReportConfigurationSetting CreateConfigurationSettingsForComplianceReport(ComplianceReportConfiguration configuration,
			string ledgerType, string invoiceType, string organisationLocation = "", string taxRegistrationType = "", string taxInvoiceRule = "",
			string taxRegistrationLocationRule = "", string originalRule = "", string disbursementRule = "", string selfBillingRule = "", string vatGroupRule = "",
			string complianceSubType = "", string reportingDate = "POS")
		{
			if (configuration != null)
			{
				var setting = configuration.Settings.AddNew();
				setting.LedgerType = ledgerType;
				setting.InvoiceType = invoiceType;
				setting.OrganisationLocation = organisationLocation;
				setting.TaxRegistrationType = taxRegistrationType;
				setting.TaxInvoiceRule = taxInvoiceRule;
				setting.TaxRegistrationLocationRule = taxRegistrationLocationRule;
				setting.OriginalRule = originalRule;
				setting.DisbursementRule = disbursementRule;
				setting.SelfBillingRule = selfBillingRule;
				setting.VATGroupRule = vatGroupRule;
				setting.ComplianceSubType = complianceSubType;
				setting.ReportingDate = setting.ReportingDateInfo.ReadOnly ? string.Empty : reportingDate;

				return setting;
			}

			return null;
		}

		public ComplianceReportConfiguration EnsureComplianceReportConfigInRegistry(string reportCode, string taxRegistration, string periodicity, string tablePrefix, string lineGrouping = "", string countryCode = "")
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var reportConfig = complianceConfig.Cast<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == reportCode) ?? complianceConfig.AddNew();
			reportConfig.ReportCode = reportCode;
			reportConfig.ReportTitle = reportCode + " Test Tax Report";
			reportConfig.ReportPeriodicity = periodicity;
			reportConfig.Country = string.IsNullOrEmpty(countryCode) ? Env.CurrentCompany.Country.Code : countryCode;
			reportConfig.TaxRegistrationType = taxRegistration;
			reportConfig.ReportBaseTablePrefix = tablePrefix;
			reportConfig.ReportLineGrouping = string.IsNullOrEmpty(lineGrouping) ? (string)reportConfig.ReportLineGrouping : lineGrouping;
			reportConfig.Settings.RemoveAll();
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			return reportConfig;
		}

		public ComplianceReportConfigurationCollection CreateReportConfigurationWithQueuing(string groupingCode = null, string tablePrefix = ReportBaseTablePrefixListCodes.AllTransactions, GlbCompany company = null, string reportCode = "LIB", string periodicity = ReportPeriodicityCodes.DateRange)
		{
			var actualCompany = company ?? GlbCompany.CurrentCompany;
			var configurations = new ComplianceReportConfigurationCollection(Factory);
			var config = configurations.AddNew();
			config.Country = actualCompany.Country?.Code ?? ZString.Empty;
			config.ReportCode = reportCode;
			config.ReportBaseTablePrefix = tablePrefix;
			if (groupingCode != null)
			{
				config.ReportLineGrouping = groupingCode;
			}

			config.ReportTitle = $"Test Report with '{tablePrefix}' Base Table Prefix and '{groupingCode}' grouping code supported by the Service Task";
			config.TaxRegistrationType = config.Lookups.TaxRegistrationTypeList.Cast<ZArchitecture.Core.CodeDescriptionPair>().FirstOrDefault()?.Code ?? ZString.Empty;
			config.ReportPeriodicity = periodicity;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(actualCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			return configurations;
		}

		public ComplianceReportConfigurationCollection CreateReportConfigurationWithoutQueuing(GlbCompany company = null)
		{
			var actualCompany = company ?? GlbCompany.CurrentCompany;
			var configurations = new ComplianceReportConfigurationCollection(Factory);
			AddNewConfigurationWithoutQueuing(configurations, actualCompany.Country?.Code ?? ZString.Empty, "IBL", ReportBaseTablePrefixListCodes.TransactionLine);

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(actualCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			return configurations;
		}

		public ComplianceReportConfigurationCollection CreateReportConfiguration(GlbCompany company, ZString countryCode, ZString reportCode, ZString baseTablePrefix, ZString periodicity)
		{
			var configurations = new ComplianceReportConfigurationCollection(Factory);
			var config = configurations.AddNew();
			config.Country = countryCode;
			config.ReportCode = reportCode;
			config.ReportBaseTablePrefix = baseTablePrefix;
			config.ReportTitle = $"{reportCode} report configuration supported by the Service Task";
			config.TaxRegistrationType = "GCR";
			config.ReportPeriodicity = periodicity;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			return configurations;
		}

		public ComplianceReportConfiguration AddNewConfigurationWithoutQueuing(ComplianceReportConfigurationCollection configurations, string countryCode, string reportCode, string tablePrefix)
		{
			var config = configurations.AddNew();
			config.Country = countryCode;
			config.ReportCode = reportCode;
			config.ReportBaseTablePrefix = tablePrefix;

			config.ReportTitle = $"'{reportCode}' test report with '{tablePrefix}' Base Table Prefix currently not supported by the Service Task";
			config.TaxRegistrationType = config.Lookups.TaxRegistrationTypeList.Cast<ZArchitecture.Core.CodeDescriptionPair>().FirstOrDefault()?.Code ?? ZString.Empty;
			config.ReportPeriodicity = ReportPeriodicityCodes.DateRange;

			var setting = config.Settings.AddNew();
			setting.LedgerType = LedgerTypes.AccountsReceivable;
			setting.InvoiceType = TransactionTypes.AdjustmentNote;
			setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;
			setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			setting.OriginalRule = OriginalRuleCodes.AllTransactions;

			return config;
		}

		public AccComplianceReport CreateComplianceReport(string reportType, string status = "", string periodicity = ReportPeriodicityCodes.DateRange)
		{
			var report = Factory.New<AccComplianceReport>();
			SetUpComplianceReport(report, reportType, status, periodicity);
			return report;
		}

		public AccGLDComplianceReport CreateGLDComplianceReport(string reportType, string status = "", string periodicity = ReportPeriodicityCodes.DateRange)
		{
			var report = Factory.New<AccGLDComplianceReport>();
			SetUpComplianceReport(report, reportType, status, periodicity);
			return report;
		}

		public AccGLDComplianceReportUsingEDW CreateGLDComplianceReportUsingEDW(string reportType, string status = "", string periodicity = ReportPeriodicityCodes.DateRange)
		{
			var report = Factory.New<AccGLDComplianceReportUsingEDW>();
			SetUpComplianceReport(report, reportType, status, periodicity);
			return report;
		}

		void SetUpComplianceReport(AccComplianceReport report, string reportType, string status = "", string periodicity = ReportPeriodicityCodes.DateRange)
		{
			report.ACR_ReportType = reportType;
			report.ACR_Periodicity = periodicity;
			if (periodicity == ReportPeriodicityCodes.AccountingPeriod)
			{
				CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo.IsEmpty ? ZDateTime.Today : report.ACR_DateTo, report.ACR_GC_Company);
				report.AccountingPeriod = currentPeriod.AM_Period;
			}
			if (periodicity == ReportPeriodicityCodes.FinancialYear)
			{
				var startDateOfFinancialYear = ZDateTime.Today.AddMonths(-3);
				CreateTestPeriods(startDateOfFinancialYear); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo.IsEmpty ? ZDateTime.Today : report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_DateFrom = startDateOfFinancialYear.Date;
				report.ACR_DateTo = ZDateTime.Today.Date;
			}
			else if (periodicity == ReportPeriodicityCodes.DateRange)
			{
				report.ACR_DateFrom = ZDate.Today;
				report.ACR_DateTo = ZDate.Today.AddDays(10);
			}
			report.ACR_Status = string.IsNullOrEmpty(status) ? (string)report.ACR_Status : status;
		}

		public AccComplianceReport CreateComplianceReportWithTransactions(string reportType, string status = "", string periodicity = ReportPeriodicityCodes.DateRange, int numberOfTransactions = 1)
		{
			var report = CreateComplianceReport(reportType, status, periodicity);
			CreateConfigurationForComplianceReport(report, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
			Factory.Save();

			for (int i = 0; i < numberOfTransactions; i++)
			{
				CreateAPTransaction(report, i);     // If other types are required (eg: ADJ or CRD), this is where to create them.
			}

			void CreateAPTransaction(AccComplianceReport rpt, int i)
			{
				var apInvoice = CreateInvoiceWithLine(typeof(APInvoice), $"I{i:0000}", AUD, 1m, 100m, 0m, 100m, 0m);
				CreateInvoiceLine(apInvoice, AUD, 1, 80m, 0m, 0m, 80m, 0m, 0m);
				Factory.Save();

				CreateComplianceReportTransactionPivot(rpt, apInvoice.Lines[0], 1 * (i + 1), "");
				CreateComplianceReportTransactionPivot(rpt, apInvoice.Lines[1], 2 * (i + 1), "");
			}

			return report;
		}

		public ZGuid[] CreateComplianceReportQueueEntryForCompany(GlbCompany company, AccComplianceReport report, params AccTransactionHeader[] transactions)
			=> CreateComplianceReportQueueEntry(report, transactions.ToArray(), (x) => x.AH_PostDate.Date, (_) => string.Empty, company);

		public ZGuid[] CreateComplianceReportQueueEntry(AccComplianceReport report, params AccTransactionHeader[] transactions)
			=> CreateComplianceReportQueueEntry(report, transactions.ToArray(), (x) => x.AH_PostDate.Date, (_) => string.Empty);

		public ZGuid[] CreateComplianceReportQueueEntry(AccComplianceReport report, IEnumerable<AccTransactionHeader> transactions,
			Func<AccTransactionHeader, ZDate> dateFunc, Func<AccTransactionHeader, string> subCodeFunc, GlbCompany company = null)
		{
			if (!report.IsInDatabase || transactions.Any(x => !x.IsInDatabase))
			{
				throw new ArgumentException("Report and Transaction must be saved before creating a Compliance Report Queue Entry.");
			}

			var result = new List<ZGuid>();

			foreach (var transaction in transactions)
			{
				result.Add(CreateComplianceReportQueueEntryCore(report.ACR_ReportType, transaction.PK, AccTransactionHeaderSchema.Constants.Prefix,
					dateFunc(transaction), subCodeFunc(transaction), company ?? GlbCompany.CurrentCompany));
			}

			return result.ToArray();
		}

		public ZGuid[] CreateComplianceReportQueueEntry(AccComplianceReport report, params AccTransactionLines[] lines)
		{
			return CreateComplianceReportQueueEntry(report, string.Empty, null, lines);
		}

		public ZGuid[] CreateComplianceReportQueueEntry(AccComplianceReport report, string subCode, ZDateTime? overrideDate, params AccTransactionLines[] lines)
		{
			if (!report.IsInDatabase || lines.Any(x => !x.IsInDatabase))
			{
				throw new ArgumentException("Report and Transaction Line must be saved before creating a Compliance Report Queue Entry.");
			}

			var result = new List<ZGuid>();

			foreach (var line in lines)
			{
				result.Add(CreateComplianceReportQueueEntryCore(report.ACR_ReportType, line.PK, AccTransactionLinesSchema.Constants.Prefix, overrideDate.HasValue ? overrideDate.Value.Date : line.AL_PostDate.Date, subCode, GlbCompany.CurrentCompany));
			}

			return result.ToArray();
		}

		ZGuid CreateComplianceReportQueueEntryCore(ZString reportType, ZGuid parentPK, ZString parentTableCode, ZDate date, ZString subCode, GlbCompany company)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES (@PK, @ReportType, @Company, @ParentPK, @ParentTableCode, @Date, @SubCode)",
					AccTransactionComplianceReportQueueSchema.Constants.TableName,
					AccTransactionComplianceReportQueueSchema.Constants.PK,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date,
					AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportSubCode
				);

			var cmd = Db.Connection.Command(sql); // Must use direct command as we do not have BusinessObject generated for AccComplianceReportTransactionPivot

			var pk = ZGuid.NewZGuid();
			cmd.AddParameterBasedOnDbColumn("@PK", pk.ToGuid(), AccComplianceReportTransactionPivotSchema.PK);
			cmd.AddParameterBasedOnDbColumn("@ReportType", reportType.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);
			cmd.AddParameterBasedOnDbColumn("@Company", company.PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			cmd.AddParameterBasedOnDbColumn("@ParentPK", parentPK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID);
			cmd.AddParameterBasedOnDbColumn("@ParentTableCode", parentTableCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode);
			cmd.AddParameterBasedOnDbColumn("@Date", date.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);
			cmd.AddParameterBasedOnDbColumn("@SubCode", subCode.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportSubCode);

			cmd.ExecuteNonQuery();

			return pk;
		}

		public void ClearComplianceReportQueue()
		{
			var cmd = Db.Connection.Command($"DELETE FROM {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}"); // Must use direct command as we do not have BusinessObject generated for AccTransactionComplianceReportQueue
			cmd.ExecuteNonQuery();
		}

		public ZGuid CreateComplianceReportTransactionPivot(AccComplianceReport report, AccTransactionHeader transaction, int sequence = 1, string subCode = "")
		{
			if (!report.IsInDatabase || !transaction.IsInDatabase)
			{
				throw new ArgumentException("Both Report and Transaction must be saved before creating a Compliance Report Pivot.");
			}

			return CreateComplianceReportTransactionPivotCore(report.PK, transaction.PK, AccTransactionHeaderSchema.Constants.Prefix, sequence, subCode);
		}

		public ZGuid CreateComplianceReportTransactionPivot(AccComplianceReport report, AccTransactionLines line, int sequence = 1, string subCode = "")
		{
			if (!report.IsInDatabase || !line.IsInDatabase)
			{
				throw new ArgumentException("Both Report and Transaction Line must be saved before creating a Compliance Report Pivot.");
			}

			return CreateComplianceReportTransactionPivotCore(report.PK, line.PK, AccTransactionLinesSchema.Constants.Prefix, sequence, subCode);
		}

		ZGuid CreateComplianceReportTransactionPivotCore(ZGuid reportPK, ZGuid parentPK, ZString parentTableCode, int sequence, string subcode)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}) VALUES (@PK, @Report, @Company, @ParentPK, @ParentTableCode, @Sequence, @SubCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
					AccComplianceReportTransactionPivotSchema.Constants.TableName,
					AccComplianceReportTransactionPivotSchema.Constants.PK,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_GC_Company,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_ParentID,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_ParentTableCode,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_ReportSequence,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_ReportSubCode,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_SystemCreateTimeUtc,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_SystemCreateUser,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_SystemLastEditTimeUtc,
					AccComplianceReportTransactionPivotSchema.Constants.ACL_SystemLastEditUser
				);

			var cmd = Db.Connection.Command(sql); // Must use direct command as we do not have BusinessObject generated for AccComplianceReportTransactionPivot

			var pk = ZGuid.NewZGuid();
			cmd.AddParameterBasedOnDbColumn("@PK", pk.ToGuid(), AccComplianceReportTransactionPivotSchema.PK);
			cmd.AddParameterBasedOnDbColumn("Report", reportPK.ToGuid(), AccComplianceReportTransactionPivotSchema.ACL_ACR_Report);
			cmd.AddParameterBasedOnDbColumn("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccComplianceReportTransactionPivotSchema.ACL_GC_Company);
			cmd.AddParameterBasedOnDbColumn("@ParentPK", parentPK.ToGuid(), AccComplianceReportTransactionPivotSchema.ACL_ParentID);
			cmd.AddParameterBasedOnDbColumn("@ParentTableCode", parentTableCode.ToString(), AccComplianceReportTransactionPivotSchema.ACL_ParentTableCode);
			cmd.AddParameterBasedOnDbColumn("@Sequence", sequence, AccComplianceReportTransactionPivotSchema.ACL_ReportSequence);
			cmd.AddParameterBasedOnDbColumn("@SubCode", subcode, AccComplianceReportTransactionPivotSchema.ACL_ReportSubCode);

			cmd.ExecuteNonQuery();

			return pk;
		}

		#region Queue Data Loader

		public DynamicBusinessObjectCollection LoadReportQueuesByParentID(ZGuid parentID)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(string.Format(CultureInfo.InvariantCulture, @"select Q.*, ACQ_ReportSubCode as ReportSubCode
from dbo.AccTransactionComplianceReportQueue Q
where ACQ_ParentID = '{0}'", parentID));
			return result;
		}

		public DynamicBusinessObjectCollection LoadReportQueuedLinesByHeaderPK(ZGuid headerPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(string.Format(CultureInfo.InvariantCulture, @"select Q.*, ACQ_ReportSubCode as ReportSubCode
from dbo.AccTransactionComplianceReportQueue Q
join dbo.AccTransactionLines L on AL_PK = ACQ_ParentID
where AL_AH = '{0}'", headerPK));
			return result;
		}

		#endregion

		#endregion

		#region TransactionPendingAllocation

		public TransactionPendingAllocation CreateTransactionPendingAllocation(ZString transactionNumber, OrgHeader creditor, ZDecimal amount, ZDecimal? taxAmount = null, RefCurrency currency = null, ZDecimal? exchangeRate = null)
		{
			var transaction = Factory.New<TransactionPendingAllocation>();

			// AccTransactionHeader is not valid with empty AH_Ledger.
			transaction.AH_Ledger = transaction.AH_Ledger.IsEmpty ? (ZString)LedgerTypes.AccountsReceivable : transaction.AH_Ledger;
			transaction.AH_TransactionNum = transactionNumber;
			transaction.AH_InvoiceDate = ZDateTime.Today;
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.AH_OH = creditor.PK;
			transaction.AH_DueDate = ZDateTime.Today.AddMonths(1);
			if (currency != null)
			{
				transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			}
			if (exchangeRate.HasValue)
			{
				transaction.AH_ExchangeRate = exchangeRate.Value;
			}
			transaction.AH_OSExTaxAmount = amount;
			if (taxAmount.HasValue)
			{
				transaction.AH_OSTaxAmount = taxAmount.Value;
			}

			return transaction;
		}

		public InvoicingBase CreateInvoiceWithUniversalTransactionInAllocationApprovalRequest(Type invoiceType, TransactionInfo universalTransaction, bool isCrossLedgerImportFromXML = false)
		{
			if (invoiceType != typeof(APInvoice) && invoiceType != typeof(APCreditNote))
			{
				return null;
			}

			var newFactory = GetNewFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);

			var isInvoice = invoiceType == typeof(APInvoice);
			var multiplier = isInvoice ? 1 : -1;

			var universalTransactionXml = universalTransaction.Serialize();
			var unallocatedTransaction = testObjectCreatorInNewFactory.CreateTransactionPendingAllocation("INV1", newFactory.Load<OrgHeader>(Creditor1.PK), 100 * multiplier);
			var request = unallocatedTransaction.Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransactionXml, isCrossLedgerImportFromXML);
			unallocatedTransaction.Factory.Save();

			var invoice = (InvoicingBase)Factory.Load(invoiceType, unallocatedTransaction.PK);
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = isInvoice ? TransactionTypes.Invoice : TransactionTypes.CreditNote;
			invoice.SubmittedFromInvoicingForm = true;

			return invoice;
		}

		public InvoicingBase CreateAndAllocateInvoiceWithUniversalTransactionInAllocationApprovalRequest(Type invoiceType, TransactionInfo universalTransaction, bool isCrossLedgerImportFromXML = false, string transactionNum = "INV1")
		{
			if (invoiceType != typeof(APInvoice) && invoiceType != typeof(APCreditNote))
			{
				return null;
			}

			var newFactory = GetNewFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);

			var multiplier = invoiceType == typeof(APInvoice) ? 1 : -1;

			var universalTransactionXml = universalTransaction.Serialize();
			var unallocatedTransaction = testObjectCreatorInNewFactory.CreateTransactionPendingAllocation(transactionNum, newFactory.Load<OrgHeader>(Creditor1.PK), 100 * multiplier);
			var request = unallocatedTransaction.Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(unallocatedTransaction, universalTransactionXml, isCrossLedgerImportFromXML);
			unallocatedTransaction.Factory.Save();

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(unallocatedTransaction).Invoice;
			return invoice;
		}

		#endregion

		#region StaffAssignment
		public OrgStaffAssignments CreateStaffAssignment(ZGuid orgPK, ZGuid companyPK, ZString departmentCode, ZString role, ZString gS_Code)
		{
			var orgStaffAssignent1 = Factory.NewWithValidTestData<OrgStaffAssignments>();
			orgStaffAssignent1.O8_OH = orgPK;
			orgStaffAssignent1.O8_GC = companyPK;
			orgStaffAssignent1.O8_Department = departmentCode;
			orgStaffAssignent1.O8_Role = role;
			orgStaffAssignent1.O8_GS_NKPersonResponsible = gS_Code;
			return orgStaffAssignent1;
		}
		#endregion

		#region UXML

		public Shipment GetUniversalShipmentDataObject(BusinessObject topLevelBO, UniversalXmlSchema schema, string dataSourceKey)
		{
			var dataObject = (Shipment)GetDataObject(topLevelBO, schema, (x, y) => ((IShipmentDataContextManager)x).GetShipmentDataObjectWriter(y));

			return string.IsNullOrEmpty(dataSourceKey) ? dataObject : dataObject.GetExactDataObject(dataSourceKey);
		}

		public TransactionInfo GetUniversalTransactionDataObject(BusinessObject topLevelBO, UniversalXmlSchema schema)
		{
			return (TransactionInfo)GetDataObject(topLevelBO, schema, (x, y) => ((ITransactionDataContextManager)x).GetTransactionDataObjectWriter(y));
		}

		//implemented according to Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject which is not accessible here and support only shipment
		public ITopLevelDataObject GetDataObject(BusinessObject topLevelBO, UniversalXmlSchema schema,
			Func<IDataContextManager, DataWritingManager, ITopLevelDataObjectWriter> getDataObjectWriter)
		{
			ITopLevelDataObject result = null;

			if (topLevelBO.GetUniversalDataContextManager() is IDataContextManager manager)
			{
				var writer = getDataObjectWriter(manager, new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO), null, schema));
				using (((IExternalFetchHintSupporter)topLevelBO.Factory).SetupCreator())
				{
					result = writer.GetDataObject(topLevelBO);
				}
			}

			return result;
		}

		#endregion

		#region Legacy Gateway Job

		public Job CreateJobForLegacyGateway(ForwardingConsol consol)
		{
			var newFactory = GetNewFactory();

			var job = newFactory.NewJobForTesting<Job>();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			AccountingTestHelper.AssertNotNull("Consol must be saved.", consolInNewFactory);
			job.Parent = consolInNewFactory;

			var jobNumberMaxLength = job.JH_JobNumInfo.MaxLength - 2;
			var jobNumber = job.JH_JobNum.Length > jobNumberMaxLength ? job.JH_JobNum.Substring(0, jobNumberMaxLength) : job.JH_JobNum;
			job.JH_JobNum = jobNumber + Constants.GatewaySuffixForJobHeaderDeprecated;

			newFactory.Save();

			AccountingTestHelper.AssertEquals("IsLegacyGateway", true, consol.IsLegacyGateway);

			return Factory.Load<Job>(job.PK);
		}

		public Job SetupGatewayLegacyJobAndEnableJRJ(bool enableJRJ = true)
		{
			var result = Factory.NewJobWithValidTestDataForTesting<Job>();

			result.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			result.JH_JobNum = Constants.GatewaySuffixForJobHeaderDeprecated;   // Make it a legacy Gateway Job
			Factory.Save();

			if (!result.IsGatewayLegacyJob)
			{
				throw new InvalidOperationException("Gateway Legacy Job should be flagged as such");
			}
			if (!result.IsGatewayBillingJob())
			{
				throw new InvalidOperationException("Gateway Legacy Job should be flagged as Gateway Billing Job");
			}

			if (enableJRJ)
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			}

			return result;
		}

		#endregion

		#region EDI Communications

		public void AddEdiCommunication(OrgHeader participantOrg1, ZString module, ZString fileFormat, ZString communicationsTransport, ZString destination)
		{
			EDICommunicationsModeDependentCollection modes_Participant1 = new EDICommunicationsModeDependentCollection(participantOrg1);
			var participant1Mode = modes_Participant1.AddNew();
			participant1Mode.EK_Module = module;
			participant1Mode.EK_FileFormat = fileFormat;
			participant1Mode.EK_CommunicationsTransport = communicationsTransport;
			participant1Mode.EK_Destination = destination;
		}

		public EDICommunicationsMode CreateEdiCommunication(OrgHeader participantOrg1, ZString module, ZString fileFormat, ZString communicationsTransport, ZString destination)
		{
			var ediCommunicationMode = Factory.New<EDICommunicationsMode>();
			ediCommunicationMode.EK_ParentID = participantOrg1.PK;
			ediCommunicationMode.EK_Module = module;
			ediCommunicationMode.EK_FileFormat = fileFormat;
			ediCommunicationMode.EK_CommunicationsTransport = communicationsTransport;
			ediCommunicationMode.EK_Destination = destination;
			return ediCommunicationMode;
		}

		#endregion

		#region Create CashBook Transaction

		public T CreateCashBookTransaction<T>() where T : DirectTransactionHeaderBase
		{
			var transaction = Factory.NewWithValidTestData<T>();
			transaction.Lines.AddNew();
			transaction.Lines[0].FillWithValidTestData();
			return transaction;
		}

		#endregion

		public RefAirline CreateAirLine(string airline3CharCode)
		{
			var query = new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airline3CharCode);
			if (Factory.Exists(typeof(RefAirline), query))
			{
				var airline = Factory.LoadTop1<RefAirline>(query);
				airline.RM_TwoCharacterCode = string.Empty;
				airline.RM_ThreeLetterCode = string.Empty;
				return airline;
			}
			else
			{
				var airline = Factory.NewWithValidTestData<RefAirline>();
				airline.RM_EagleAddedAirlinePrefixOrAccountingCode = airline3CharCode;
				return airline;
			}
		}

		public APAccQueryClaim CreateAPClaim(decimal claimAmount, string mawbNumber = "17267828073", string claimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open, APInvoice apInvoice = null)
		{
			if (apInvoice == null)
			{
				apInvoice = Factory.NewWithValidTestData<APInvoice>();
				apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				apInvoice.AH_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			}

			var claim = Factory.New<APAccQueryClaim>();
			claim.FillWithValidTestData();
			claim.AY_AH = apInvoice.PK;
			claim.AY_MasterBillNumber = mawbNumber;
			claim.AY_QueryClaimAmount = claimAmount;
			claim.AY_QueryClaimStatus = claimStatus;
			return claim;
		}

		public AccQueryClaim CreateClaim(Type claimType, decimal claimAmount, ZGuid transactionHeaderPK, ZGuid orgPK, ZGuid orgContactPK, string claimStatus)
		{
			var claim = (AccQueryClaim)Factory.New(claimType);
			claim.AY_OH_Debtor = orgPK;
			claim.AY_OC = orgContactPK;
			claim.AY_AH = transactionHeaderPK;
			claim.AY_MasterBillNumber = "XXXXX";
			claim.AY_QueryClaimAmount = claimAmount;
			claim.AY_QueryClaimStatus = claimStatus;
			claim.AY_QueryClaimReasonCode = "DMG";
			claim.AY_QueryClaimReference = "TEST1";
			claim.AY_HoldOption = "DNM";
			return claim;
		}

		public static void SetupCASSBillingLine(CASSBillingLine cassBillingLine, bool setAdjustmentValues = true, bool setupAmount = true, bool isRejectedRecordType = false, int multiplier = 1, string currencyCode = "AUD", string awbNumber = "67828073")
		{
			SetupCASSCostComponent(cassBillingLine, false, 800.00M, 150.00M, 50.00M, 200.00M, 100.00M, 19.82M, setupAmount: setupAmount, isRejectedRecordType: isRejectedRecordType, multiplier: multiplier, currency: currencyCode, awbSerialNumber: awbNumber);
			if (setAdjustmentValues)
			{
				SetupCASSCostComponent(cassBillingLine, true, 800.00M, 200.00M, 15.68M, 0M, 0M, 0M, setupAmount: setupAmount, isRejectedRecordType: isRejectedRecordType, currency: currencyCode, multiplier: multiplier);
			}
		}

		public static void SetupCASSCostComponent(CASSBillingLine cassBillingLine, bool isAdjustedAmount, ZDecimal pWCAmount, ZDecimal pVCAmount, ZDecimal pCCAmount, ZDecimal cOAAmount, ZDecimal cOMAmount, ZDecimal dOIAmount, decimal vatDueAirlineAmount = 0M, decimal adjustedVatDueAirlineAmount = 0M,
			string vatIndicator = "Y", string airlinePrefix = "172", string awbSerialNumber = "67828073", string agentCode = "23470068510", string origin = "LEJ", string destination = "MEX", decimal weight = 2150M, string weightUnit = "KG", string currency = "AUD",
			bool setupAmount = true, bool isRejectedRecordType = false, int multiplier = 1, decimal vatDueAgentAmount = 0M, decimal adjustedVatDueAgentAmount = 0M)
		{
			var today = ZDateTime.Today;
			var costLine = new CASSCostExportLine(new BusinessObjectFactory(), isAdjustedAmount ? CASSCostLineType.Adjustment : (isRejectedRecordType ? CASSCostLineType.Rejected : CASSCostLineType.Billing));

			costLine.RecordType = isAdjustedAmount ? "DCO" : "AWM";
			costLine.VATIndicator = vatIndicator;
			costLine.AirlinePrefix = airlinePrefix;
			costLine.AWBSerialNumber = awbSerialNumber;
			costLine.AgentCode = agentCode;
			costLine.DateAWBExecution = today.AddMonths(-3);
			costLine.DateOfArrival = today.AddMonths(-2);
			costLine.DateOfDelivery = today.AddMonths(-1);
			costLine.Origin = origin;
			costLine.Destination = destination;
			costLine.Weight = weight;
			costLine.WeightUnit = weightUnit;
			costLine.CurrencyCode = currency;
			if (setupAmount)
			{
				costLine.WeightChargePP = pWCAmount * multiplier;
				costLine.ValuationChargePP = pVCAmount * multiplier;
				costLine.ChargesDueCarrierPP = pCCAmount * multiplier;
				costLine.ChargesDueAgentCC = cOAAmount * multiplier;
				costLine.Commission = cOMAmount * multiplier;
				costLine.Discount = dOIAmount * multiplier;
				costLine.VATDueAirline = (isAdjustedAmount ? adjustedVatDueAirlineAmount : vatDueAirlineAmount) * multiplier;
				costLine.VATDueAgent = (isAdjustedAmount ? adjustedVatDueAgentAmount : vatDueAgentAmount) * multiplier;
			}
			cassBillingLine.AddCostLine(costLine, costLine.CurrencyCode);
		}

		public static void SetupCreditorAirline(RefAirline airline, OrgHeader organisation)
		{
			organisation.MiscServ.OM_RM_Airline = airline.PK;
		}

		public static void AddActiveContactIfRequires(OrgHeader creditor)
		{
			if (!creditor.Contacts.Any())
			{
				OrgContact contact1 = creditor.Contacts.AddNew();
				contact1.OC_IsActive = true;
			}
		}

		public static OrgPatternMatchOverride AddMatchingRuleForOrganization(OrgHeader ruleHolder, ZString foreignCode, OrgHeader localOrganization)
		{
			var rule = ruleHolder.CreatePatternMatchOverrideForTest();
			rule.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			rule.OO_ForeignCode = foreignCode;
			rule.OO_LocalGuid = localOrganization.PK;

			return rule;
		}

		public static OrgPatternMatchOverride AddMatchingRuleForChargeCode(OrgHeader ruleHolder, ZString foreignCode, AccChargeCode localChargeCode)
		{
			var rule = ruleHolder.CreatePatternMatchOverrideForTest();
			rule.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			rule.OO_ForeignCode = foreignCode;
			rule.OO_LocalCode = localChargeCode.AC_Code;

			return rule;
		}

		public static void SetUpCostVarianceApprovalRegistry(ZString option, bool monitor = false, APInvoice invoice = null,
			decimal positiveAmount1 = 100M, decimal positiveAmount2 = 150M, decimal negativeAmount1 = 50M,
			decimal positiveTotalAmount1 = 150M, decimal positiveTotalAmount2 = 200M, decimal negativeTotalAmount1 = 100M)
		{
			AccountingConfigurationRegistry.Instance.PayableFinalFlag.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var valuesForTest = new CostVarianceApproval
			{
				VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount,
				VarianceComparisonOption = option,
				AutoTickFinalFlag = ZBool.True
			};

			var upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = positiveAmount1;

			var upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			upTo2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo2.Amount = positiveAmount2;

			var above = valuesForTest.AuthorisationRequirements.AddNew();
			above.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo2.Amount;

			var upToNegative = valuesForTest.AuthorisationRequirements.AddNew();
			upToNegative.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Minus;
			upToNegative.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upToNegative.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToNegative.Amount = negativeAmount1;

			var aboveNegative = valuesForTest.AuthorisationRequirements.AddNew();
			aboveNegative.VarianceSign = CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Minus;
			aboveNegative.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			aboveNegative.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			aboveNegative.Amount = upToNegative.Amount;

			if (monitor)
			{
				upTo1.MonitorTotalInvoiceVariance = true;
				upTo1.TotalInvoiceVarianceAmount = positiveTotalAmount1;
				upTo2.MonitorTotalInvoiceVariance = true;
				upTo2.TotalInvoiceVarianceAmount = positiveTotalAmount2;
				above.MonitorTotalInvoiceVariance = true;
				above.TotalInvoiceVarianceAmount = upTo2.TotalInvoiceVarianceAmount;
				upToNegative.MonitorTotalInvoiceVariance = true;
				upToNegative.TotalInvoiceVarianceAmount = negativeTotalAmount1;
				aboveNegative.MonitorTotalInvoiceVariance = true;
				aboveNegative.TotalInvoiceVarianceAmount = upToNegative.TotalInvoiceVarianceAmount;
			}

			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			if (invoice != null)
			{
				invoice.ClearCachedCostVarianceApproval_ForTestOnly();
				invoice.ConsolCosting.ClearCachedCostVarianceApproval_ForTestOnly();
			}
		}

		public static void SetupMatchLinkMatchDate(AccTransactionMatchLink link)
		{
			if (link != null && link.AP_MatchDate.IsEmpty)
			{
				if (link.AP_AH.IsEmpty)
				{
					link.AP_MatchDate = ZDate.Today;
				}
				else
				{
					var header = link.Factory.Load<AccTransactionHeader>(link.AP_AH);
					if (header != null && !header.AH_PostDate.IsEmpty)
					{
						link.AP_MatchDate = header.AH_PostDate.Date;
					}
					else
					{
						link.AP_MatchDate = ZDate.Today;
					}
				}
			}
		}

		public static void SetupMatchLinkMatchDate(TransactionMatchLinkCollection collection)
		{
			if (collection != null)
			{
				collection.Cast<AccTransactionMatchLink>().ForEach(x => SetupMatchLinkMatchDate(x));
			}
		}

		public static void SetupMatchLinkMatchDate(IMatching matching)
		{
			if (matching != null)
			{
				SetupMatchLinkMatchDate(matching.CurrentMatchGroup);
			}
		}

		public static bool IsContainSubStrings(string actualString, params string[] expectedSubStrings) => expectedSubStrings.All(x => actualString.Contains(x));

		public void SetupTermsInfo(OrgARTerms term, string jobType, ZGuid branchPK, ZGuid deptPK, string direction, string transportMode, string invoiceType, string invoiceTerm, int termDays, ZString agreedPayment)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
				term.PY_InvoiceTerm = invoiceTerm;
				term.PY_InvoiceDays = (ZByte)termDays;
				term.PY_AgreedPaymentMethod = agreedPayment;
			}
		}
		public StmMenuItem CreateDuplicateMenuItem(string menuName, IDocumentSupportable receiptOrPayment)
		{
			var filter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, string.Empty);

			DocumentCommandCollection documentCommands = new DocumentCommandCollection(receiptOrPayment);
			documentCommands.Load();
			BusinessObject[] menuItems = documentCommands.Find(filter);

			var originalMenu = menuItems[0];
			var duplicateReportMenu = Factory.New<StmMenuItem>();
			duplicateReportMenu.CopyPersistentValuesFrom(originalMenu);
			duplicateReportMenu.SU_IsSystemDefined = false;
			return duplicateReportMenu;
		}

		public static (Mock<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider> eRequestProviderMock, Mock<IEInvoicingPivotActionTypeProvider> pivotActionTypeProviderMock)
		MockCountryFactoryForTPAAeInvoicing(bool transactionEligibilityDefaultReturnValue = true, bool isTPAAeInvoicingProviderImplemented = true, string defaultPivotActionType = "CRX")
		{
			var globalAccountingCountryFactoryMock = new Mock<IGlobalAccountingCountryFactory>();
			var eRequestProviderMock = new Mock<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>();
			var pivotActionTypeProviderMock = new Mock<IEInvoicingPivotActionTypeProvider>();
			var eligibilityMock = new Mock<IEInvoicingEligibilityDecider>();

			eRequestProviderMock.Setup(x => x.IsTransactionEligibleToCreateApprovalRequest(It.IsAny<AccTransactionHeader>())).Returns(transactionEligibilityDefaultReturnValue);
			eRequestProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(transactionEligibilityDefaultReturnValue);
			pivotActionTypeProviderMock.Setup(x => x.GetPivotActionType(It.IsAny<AccTransactionHeader>())).Returns(defaultPivotActionType);
			eligibilityMock.Setup(x => x.IsTransactionEligible(It.IsAny<IEInvoicingEligibilityLiteTransaction>())).Returns(true);

			var accountingCountryFactoryMock = new Mock<IAccountingCountryFactory>();
			accountingCountryFactoryMock.As<IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>>().Setup(x => x.Get()).Returns(isTPAAeInvoicingProviderImplemented ? eRequestProviderMock.Object : null);
			accountingCountryFactoryMock.As<IInstanceProvider<IEInvoicingPivotActionTypeProvider>>().Setup(x => x.Get()).Returns(pivotActionTypeProviderMock.Object);
			accountingCountryFactoryMock.As<IInstanceProvider<IEInvoicingEligibilityDecider>>().Setup(x => x.Get()).Returns(eligibilityMock.Object);

			globalAccountingCountryFactoryMock.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(accountingCountryFactoryMock.Object);
			ObjectFactory.Substitute(globalAccountingCountryFactoryMock.Object);

			return (eRequestProviderMock, pivotActionTypeProviderMock);
		}

		#region AccTaxReturn

		public AccTaxReturn CreateAccTaxReturn(AccComplianceReport report = null, bool addTaxReturnColumn = false, bool addTaxReturnLine = false)
		{
			report = report ?? Factory.NewWithValidTestData<AccComplianceReport>();

			var taxReturn = Factory.New<AccTaxReturn>();
			taxReturn.ATR_ACR_ComplianceReport = report.PK;
			taxReturn.ATR_GovtReceiptInformation = "ReturnVersion#1";
			taxReturn.ATR_GovtReceiptInformation = "GB-MTD-001";

			if (addTaxReturnColumn)
			{
				var column = taxReturn.Columns.AddNew();
				column.ATC_Amount = 25;
				column.ATC_ColumnName = "Box1";
				column.ATC_Comment = "No Comment";
				column.ATC_GroupCode = "NGR";
			}
			if (addTaxReturnLine)
			{
				var line = taxReturn.Lines.AddNew();
				line.ARL_OH_Organisation = ABIGAS.PK;
				line.ARL_TotalAmountIncludingTax = 25;
			}

			return taxReturn;
		}

		#endregion

		#region Rollup Setup

		public OrgInvoiceRollupOrGroup CreateOrgInvoiceRollupOrGroup(OrgHeader org,
			string jobType = "", string transportMode = "", string serviceDirection = "",
			string groupOrSubtotalStyle = "", string invoiceLineDisplayOption = "", string invoicePostingStyle = "", string groupOrSubTotal = "")
		{
			var invoiceRollUp = org.CompanyData.InvoiceRollupOrGroups.AddNew();

			if (!string.IsNullOrEmpty(jobType))
			{
				invoiceRollUp.PG_JobType = jobType;
			}
			if (!string.IsNullOrEmpty(transportMode))
			{
				invoiceRollUp.PG_TransportMode = transportMode;
			}
			if (!string.IsNullOrEmpty(serviceDirection))
			{
				invoiceRollUp.PG_ServiceDirection = serviceDirection;
			}
			if (!string.IsNullOrEmpty(groupOrSubtotalStyle))
			{
				invoiceRollUp.PG_GroupOrSubtotalStyle = groupOrSubtotalStyle;
			}
			if (!string.IsNullOrEmpty(invoiceLineDisplayOption))
			{
				invoiceRollUp.PG_InvoiceLineDisplayOption = invoiceLineDisplayOption;
			}
			if (!string.IsNullOrEmpty(invoicePostingStyle))
			{
				invoiceRollUp.PG_InvoicePostingStyle = invoicePostingStyle;
			}
			if (!string.IsNullOrEmpty(groupOrSubTotal))
			{
				invoiceRollUp.PG_GroupOrSubTotal = groupOrSubTotal;
			}

			return invoiceRollUp;
		}

		#endregion

		#region Auto Job Closure Registry Setup

		public JobClosureConfigurationHeader CreateJobClosureConfiguration(params JobClosureConfiguration[] config)
		{
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.AddRange(config);
			return regValue;
		}

		public JobClosureConfiguration CreateJobClosureConfigLine(string jobType = "ALL", string direction = "", string mode = "",
			string dateOption = "JOP", int offset = 0, bool closeJobWithOpenWip = false, bool closeJobWithOpenAcr = false, int restrictionOffset = 0,
			ZGuid? departmentPK = null, string offsetType = "DAY", string restrictionOffsetType = "DAY",
			string recognizedChargeFilter = "ALL", string fromJobStatus = "", string configurationType = "CLS")
		{
			var closureConfigLine = new JobClosureConfiguration();
			UpdateConfigLine(closureConfigLine,
			jobType, direction, mode, departmentPK ?? ZGuid.Empty,
			closeJobWithOpenWip, closeJobWithOpenAcr, configurationType, fromJobStatus, recognizedChargeFilter,
			dateOption, offset, offsetType, restrictionOffset, restrictionOffsetType);
			return closureConfigLine;
		}

		void UpdateConfigLine(JobClosureConfiguration closureConfigLine,
			string jobType, string direction, string mode, ZGuid departmentPK,
			bool closeJobWithOpenWip, bool closeJobWithOpenAcr, string configurationType, string fromJobStatus, string recognizedChargeFilter,
			string dateOption, ZInt closeOffset, ZString closeOffsetType, ZInt restrictionOffset, ZString restrictionOffsetType)
		{
			closureConfigLine.JobType = jobType;
			closureConfigLine.DirectionCode = direction;
			closureConfigLine.Mode = mode;
			closureConfigLine.DepartmentPK = departmentPK;

			closureConfigLine.ConfigurationType = configurationType;
			closureConfigLine.FromJobStatus = fromJobStatus;

			closureConfigLine.CloseJobsWithOpenWip = closeJobWithOpenWip;
			closureConfigLine.CloseJobsWithOpenAcr = closeJobWithOpenAcr;
			closureConfigLine.JobChargeRecognitionFilter = recognizedChargeFilter;

			closureConfigLine.JobClosureDateOptionCode = dateOption;
			closureConfigLine.Offset = closeOffset;
			closureConfigLine.OffsetType = closeOffsetType;
			closureConfigLine.ReopenRestrictionOffset = restrictionOffset;
			closureConfigLine.ReopenRestrictionOffsetType = restrictionOffsetType;
		}

		#endregion

		#region Unique Number

		public int UniqueNumber
		{
			get
			{
				if (randomNumbers == null)
				{
					var random = new Random();
					randomNumbers = new Queue<int>(Enumerable.Range(10001, 89999).OrderBy(x => random.Next()));
				}

				return randomNumbers.Dequeue();
			}
		}

		Queue<int> randomNumbers;

		#endregion

		#region Temporarily Customise Transaction Number Generator

		public IDisposable TemporarilyCustomiseTransactionNumberGenerator(GlbCompany company, params TransactionNumberSequenceCustomisation[] customisations)
			=> TemporarilyCustomiseTransactionNumberGenerator(company.PK, customisations);

		public IDisposable TemporarilyCustomiseTransactionNumberGenerator(ZGuid companyPk, params TransactionNumberSequenceCustomisation[] customisations)
			=> TemporarilyCustomiseTransactionNumberGenerator(companyPk.ToGuid(), customisations);

		public IDisposable TemporarilyCustomiseTransactionNumberGenerator(Guid companyPk, params TransactionNumberSequenceCustomisation[] customisations)
		{
			if (customisations == null || customisations.Length == 0)
			{
				return null;
			}

			var customisation = new TransactionNumberSequenceCustomisationCollection();
			foreach (var c in customisations)
			{
				customisation.Add(c);
			}

			return AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, customisation);
		}

		public TransactionNumberSequenceCustomisation[] CreateTestPrefixAndSequenceNumberCustomisation()
			=> new[]
			{
				new TransactionNumberSequenceCustomisation()
				{
					Order = 1,
					ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1,
					Code = "TST",
					Include = true,
				},
				new TransactionNumberSequenceCustomisation()
				{
					Order = 2,
					ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber,
					Length = 8,
					Include = true,
				}
			};

		#endregion

		#region Surcharge

		public AccSurchargeApplication CreateAccSurchargeApplication(ZString jobType, ZString homeCountryOrZone, ZString organizationCategory, ZString placeOfSupplyType, ZString placeOfSupply, ZString supplyType, ZString surchargeCode)
		{
			var application = Factory.New<AccSurchargeApplication>();
			application.ASP_GC_Company = GlbCompany.CurrentCompany.PK;
			application.ASP_JobType = jobType;
			application.ASP_HomeCountryOrZone = homeCountryOrZone;
			application.ASP_OrganizationCategory = organizationCategory;
			application.ASP_PlaceOfSupplyType = placeOfSupplyType;
			application.ASP_PlaceOfSupply = placeOfSupply;
			application.ASP_SupplyType = supplyType;
			application.ASP_ASC_NKSurchargeCode = surchargeCode;
			return application;
		}

		#endregion

		#region CashAdvance

		public AccCashAdvanceRequestHeader CreateCashAdvanceRequestHeader(Job job, OrgHeader org, ZString ledgerType, ZDecimal localAmount, ZDecimal osAmount, ZString osCurrency, string requestNumber = "")
		{
			var cah = Factory.New<AccCashAdvanceRequestHeader>();
			cah.CAH_GC_Company = GlbCompany.CurrentCompany.PK;
			cah.CAH_RequestReferenceNumber = string.IsNullOrEmpty(requestNumber) ? GetRandomString(8) : requestNumber;
			cah.CAH_JH_Job = job.PK;
			cah.CAH_Ledger = ledgerType;
			cah.CAH_LocalAmount = localAmount;
			cah.CAH_OSAmount = osAmount;
			cah.CAH_RX_NKTransactionCurrency = osCurrency;
			cah.CAH_OH_Organization = org.PK;
			cah.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			return cah;
		}

		public AccCashAdvanceRequestLine CreateCashAdvanceRequestLine(AccCashAdvanceRequestHeader requestHeader, ZDecimal localAmount, ZDecimal osAmount, ZString status, Charge charge, decimal localPaidAmount = 0M, decimal osPaidAmount = 0M)
		{
			var line = CreateCashAdvanceRequestLine(requestHeader, localAmount, osAmount, localPaidAmount, osPaidAmount);
			line.CAL_Status = status;
			if (charge != null)
			{
				if (requestHeader.CAH_Ledger == LedgerTypes.AccountsPayable)
				{
					charge.JR_IsAPCashAdvance = true;
					charge.JR_CAL_APLine = line.PK;
				}
				else if (requestHeader.CAH_Ledger == LedgerTypes.AccountsReceivable)
				{
					charge.JR_IsARCashAdvance = true;
					charge.JR_CAL_ARLine = line.PK;
				}
			}
			return line;
		}

		public AccCashAdvanceRequestLine CreateCashAdvanceRequestLine(AccCashAdvanceRequestHeader header, ZDecimal localAmount, ZDecimal osAmount, decimal localPaidAmount = 0M, decimal osPaidAmount = 0M)
		{
			var cal = Factory.New<AccCashAdvanceRequestLine>();
			cal.CAL_CAH_RequestHeader = header.PK;
			cal.CAL_GC_Company = GlbCompany.CurrentCompany.PK;
			cal.CAL_LocalAmount = localAmount;
			cal.CAL_OSAmount = osAmount;
			cal.CAL_LocalPaidAmount = localPaidAmount;
			cal.CAL_OSPaidAmount = osPaidAmount;
			header.Lines.Add(cal);
			header.CAH_LocalAmount += cal.CAL_LocalAmount;
			header.CAH_OSAmount += cal.CAL_OSAmount;
			header.CAH_LocalPaidAmount += cal.CAL_LocalPaidAmount;
			header.CAH_OSPaidAmount += cal.CAL_OSPaidAmount;
			return cal;
		}

		#endregion

		#region Production Rules Engine

		public ProductionRuleSet CreateProductionRuleSet(string context, string subContext, ZShort number, ZGuid company = default)
		{
			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = $"RuleSet{number}";
			ruleSet.PRS_Description = $"RuleSet{number}";
			ruleSet.PRS_Context = context;
			ruleSet.PRS_ContextSubType = subContext;
			ruleSet.PRS_GC_Company = company;
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			return ruleSet;
		}

		public ProductionRule CreateProductionRules(ProductionRuleSet ruleSet, string context, ZShort number, ZString matchFiled, ZGuid matchValue, ZGuid defaultToPK)
		{
			var actionState = context.Equals("JBR") || context.Equals("JTB") ? "SetDefaultBranchState" : "SetDefaultDepartmentState";
			var action = context.Equals("JBR") || context.Equals("JTB") ? "branch" : "department";

			var rule = Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE{number}";
			rule.PRL_Description = $"RULE{number}";
			rule.PRL_Priority = number;
			rule.PRL_RuleDefinition = $@"{{
	""conditions"": [
		{{
			""fieldPath"": ""{matchFiled}"",
			""operation"": ""equals"",
			""value"": ""{matchValue}""
		}}
	],
	""action"": {{
		""$type"": ""{actionState}"",
		""{action}"": ""{defaultToPK}""
	}}
}}";
			return rule;
		}

		public ProductionRule CreateProductionRuleWithOriginCountryCode(ProductionRuleSet ruleSet, string context, ZShort number, ZString originCountryCode, ZGuid defaultToPK)
		{
			var actionState = context.Equals("JBR") || context.Equals("JTB") ? "SetDefaultBranchState" : "SetDefaultDepartmentState";
			var action = context.Equals("JBR") || context.Equals("JTB") ? "branch" : "department";

			var rule = Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE{number}";
			rule.PRL_Description = $"RULE{number}";
			rule.PRL_Priority = number;
			rule.PRL_RuleDefinition = $@"{{
	""conditions"": [
		{{
			""fieldPath"": ""Origin.Country.Code"",
			""operation"": ""equals"",
			""value"": ""{originCountryCode}""
		}}
	],
	""action"": {{
		""$type"": ""{actionState}"",
		""{action}"": ""{defaultToPK}""
	}}
}}";
			return rule;
		}

		public ProductionRule CreateProductionRuleWithConsignorAddressCountryCode(ProductionRuleSet ruleSet, string context, ZShort number, ZString countryCode, ZGuid defaultToPK)
		{
			var actionState = context.Equals("JBR") || context.Equals("JTB") ? "SetDefaultBranchState" : "SetDefaultDepartmentState";
			var action = context.Equals("JBR") || context.Equals("JTB") ? "branch" : "department";

			var rule = Factory.New<ProductionRule>();
			rule.PRL_PRS_RuleSet = ruleSet.PK;
			rule.PRL_Name = $"RULE{number}";
			rule.PRL_Description = $"RULE{number}";
			rule.PRL_Priority = number;
			rule.PRL_RuleDefinition = $@"{{
	""conditions"": [
		{{
			""fieldPath"": ""Consignor.MainAddress.Country.Code"",
			""operation"": ""equals"",
			""value"": ""{countryCode}""
		}}
	],
	""action"": {{
		""$type"": ""{actionState}"",
		""{action}"": ""{defaultToPK}""
	}}
}}";
			return rule;
		}

		#endregion

		public void PrepareAutoJRJTestEnvironment()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			var taxRegOrgCusCode = Enterprise.ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.Country.Code);
			AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.NumberTypeForDisplay = taxRegOrgCusCode;
			AutoJRJChargeBranchOrgProxy.PrimaryRegistrationNumber.NumberTypeForDisplay = taxRegOrgCusCode;

			var nonCurrenctBranch = NonCurrentBranch;
			nonCurrenctBranch.GB_OH_OrgProxy = AutoJRJCreditorOrDebtor.PK;
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = AutoJRJChargeBranchOrgProxy.PK;
			GlbBranch.CurrentBranch.Factory.Save();
		}

		public void PrepareAutoJRJTestEnvironment(bool isTaxRegNumTheSame)
		{
			PrepareAutoJRJTestEnvironment();
			PrepareAutoJRJTaxRegistrationNumbers(isTaxRegNumTheSame);
		}

		public void PrepareAutoJRJTaxRegistrationNumbers(bool isTaxRegNumTheSame, bool shouldSaveFactory = true)
		{
			if (isTaxRegNumTheSame)
			{
				AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.Number = "73004700400";
				AutoJRJChargeBranchOrgProxy.PrimaryRegistrationNumber.Number = "73004700400";
			}
			else
			{
				AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.Number = "73004700411";
				AutoJRJChargeBranchOrgProxy.PrimaryRegistrationNumber.Number = "73004700499";
			}

			if (shouldSaveFactory)
			{
				Factory.Save();
			}
		}

		public OrgHeader AutoJRJChargeBranchOrgProxy => ABIGAS;
		public OrgHeader AutoJRJCreditorOrDebtor => AALSHI;

		public GlbGroup CreateRecipientGroup(string staffCode, string groupCode, string mailAddress)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode;
			staff.GS_EmailAddress = mailAddress;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = groupCode;
			group.Staff.Add(staff);
			Factory.Save();

			return group;
		}

		#region Database script test

		public Guid DefaultCompanyPK => new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");

		public GlbCompany DefaultCompany => Factory.Load<GlbCompany>(DefaultCompanyPK);

		public Guid DefaultBranchPK
		{
			get
			{
				if (defaultBranchPK == Guid.Empty)
				{
					defaultBranchPK = CreateBranch("ZZB", DefaultCompany).PK.ToGuid();
				}
				return defaultBranchPK;
			}
		}
		Guid defaultBranchPK;

		public Guid DefaultDepartmentPK
		{
			get
			{
				if (defaultDepartmentPK == Guid.Empty)
				{
					defaultDepartmentPK = CreateDepartment("ZZD").PK.ToGuid();
				}
				return defaultDepartmentPK;
			}
		}
		Guid defaultDepartmentPK;

		Guid DepartmentBrnPK => new Guid("86BB1C22-0865-4685-996E-D56CBD136491");

		public void InsertAmountsFor(Guid plAccountPK, int firstPeriod, int lastPeriod)
		{
			var amountsAndPeriods = YieldAccountingPeriods(firstPeriod, lastPeriod)
					.Zip(YieldPrimes(lastPeriod - firstPeriod), (period, amount) => new { period, amount });
			foreach (var x in amountsAndPeriods)
			{
				CreateAccGLAggregate(x.amount, x.period, plAccountPK, transactionCategory: string.Empty);
			}
		}

		public IEnumerable<int> YieldAccountingPeriods(int firstPeriod, int lastPeriod)
		{
			var year = int.Parse(firstPeriod.ToString().Substring(0, 4));
			var period = int.Parse(firstPeriod.ToString().Substring(4, 2));

			while ((year * 100) + period <= lastPeriod)
			{
				yield return (year * 100) + period;

				if (period == 12)
				{
					++year;
					period = 1;
				}
				else
				{
					++period;
				}
			}
		}

		public IEnumerable<decimal> YieldPrimes(int count)
		{
			yield return 2m;

			var next = 3;
			for (int i = 1; i < count; i++)
			{
				if (IsPrime(next))
				{
					yield return next;
				}
				++next;
			}

			bool IsPrime(int num)
			{
				for (int i = 2; i < num / 2; i++)
				{
					if (num % i == 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		public string CreateGLAggregate(string categoryPrefix, int categoryGroupCount, decimal amount, int period, Guid glAccountPK, Guid departmentPK, Guid branchPK, Guid companyPK)
		{
			var categotyGroupBuilder = new StringBuilder();
			for (int index = 0; index < categoryGroupCount; index++)
			{
				var category = string.IsNullOrEmpty(categoryPrefix) ? categoryPrefix : $"{categoryPrefix}{index.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
				CreateAccGLAggregate(amount, period, glAccountPK, branchPK, companyPK, departmentPK, category);
				categotyGroupBuilder.Append($"{category},");
			}
			return categotyGroupBuilder.ToString().TrimEnd(',');
		}

		public Guid InsertGLHeader(string accountNum, string description, string accountType, string statisticalUnits, string debitCredit = "CR", bool isControlAccount = true, string reportSection = "TS")
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = accountNum;
			glHeader.AG_AccountType = accountType;
			glHeader.AG_StatisticalUnits = statisticalUnits;
			glHeader.AG_Column = reportSection;
			glHeader.AG_ControlAccount = isControlAccount;
			glHeader.AG_DebitCredit = debitCredit;
			glHeader.AG_Description = description;
			Factory.Save();
			return glHeader.PK.ToGuid();
		}

		public void InsertStmData(string name, Guid owner, string value, string type = "DT")
		{
			var pk = Guid.NewGuid();
			var sql = $@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue)
						VALUES(NEWID(), '{name}', '{owner}', NULL, '{type}', convert(varbinary(8000), N'{value}'), NULL)";
			Db.Connection.ExecuteNonQuery(sql);
		}

		#endregion

		public EInvoicingCertificateCredential CreateCompanyCertificate(GlbCompany company, string issuerName, string mailBoxID, DateTime expiryDate)
		{
			var credential = CreateCertificate<GlbCompanyEInvoicingCertificateCredential>(issuerName, mailBoxID, expiryDate);
			credential.GP_GC = company.PK;
			credential.GP_GB = ZGuid.Empty;

			return credential;
		}

		public EInvoicingCertificateCredential CreateBranchCertificate(GlbBranch branch, string issuerName, string mailBoxID, DateTime expiryDate)
		{
			var credential = CreateCertificate<GlbBranchEInvoicingCertificateCredential>(issuerName, mailBoxID, expiryDate);
			credential.GP_GC = branch.GB_GC;
			credential.GP_GB = branch.PK;
			return credential;
		}

		T CreateCertificate<T>(string issuerName, string mailBoxID, DateTime expiryDate)
			where T : EInvoicingCertificateCredential
		{
			const string dummyPsd = "password12345678";

			using (var rootCertificate = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName($"CN={issuerName}"), expiryDate.AddYears(-1), expiryDate))
			{
				var credential = Factory.New<T>();
				credential.GP_PasswordType = PasswordTypesList.Codes.EIM;
				credential.GP_Certificate = rootCertificate.Export(X509ContentType.Pfx, new NetworkCredential("user", dummyPsd).SecurePassword);
				credential.CurrentDecryptedCertificatePassphrase = dummyPsd;
				credential.GP_MailBoxID = mailBoxID;
				credential.GP_ExpiryDate = expiryDate;

				return credential;
			}
		}

		#region Reporting book

		public AccAlternateChart CreateAlternateChart(string code, string description = "description", bool isFixedLength = true,
			 bool isGlobal = false, string balancesheetStyle = BalanceSheetStyleCode.ELA, string reportOrder = "PTB", Guid? secondReportStartAccount = null)
		{
			var chart = Factory.New<AccAlternateChart>();
			chart.AAC_Code = code;
			chart.AAC_Description = description;
			chart.AAC_IsFixedLength = isFixedLength;
			chart.AAC_IsGlobal = isGlobal;
			chart.AAC_BalanceSheetStyle = balancesheetStyle;
			chart.AAC_AGA_SecondReportStartAccount = secondReportStartAccount ?? ZGuid.Empty;
			chart.AAC_ReportOrder = reportOrder;

			if (!isGlobal)
			{
				chart.AAC_GC_Company = GlbCompany.CurrentCompany.PK;
			}
			return chart;
		}

		public AccAlternateChartFormat CreateAccAlternateChartFormat(AccAlternateChart chart, ZShort tier, string format,
			string description = "test", string separator = ".")
		{
			var chartFormat = chart.AlternateChartFormats.AddNew();
			chartFormat.ANF_Tier = tier;
			chartFormat.ANF_AAC_AlternateChart = chart.PK;
			chartFormat.ANF_Format = format;
			chartFormat.ANF_Description = description;
			chartFormat.ANF_Separator = separator;
			return chartFormat;
		}

		public AccAlternateGLAccount CreateAccAlternateGlAccount(ZGuid chartPK, string accountNum, string accountType,
			string drCR = "CR", int totalLevel = 1, string reportSection = "OV",
			int printSequence = 1, string description = "desc", ZGuid? alternateNum = null, ZGuid? percentNum = null, ZGuid? consolidate = null, ZGuid? totalReference = null)
		{
			var account = Factory.New<AccAlternateGLAccount>();
			account.AGA_AAC_AlternateChart = chartPK;
			account.AGA_AccountNum = accountNum;
			account.AGA_Description = accountNum;
			account.AGA_AccountType = accountType;
			account.AGA_DebitCredit = drCR;
			account.AGA_TotalLevel = totalLevel;
			account.AGA_ReportSection = reportSection;
			account.AGA_PrintSequence = printSequence;
			account.AGA_Description = description;
			account.AGA_AGA_AlternateNum = alternateNum ?? Guid.Empty;
			account.AGA_AGA_PercentNum = percentNum ?? Guid.Empty;
			account.AGA_AGA_ConsolidationNum = consolidate ?? Guid.Empty;
			account.AGA_AGA_HeaderDependsOnTotal = totalReference ?? Guid.Empty;
			return account;
		}

		public AccAlternateGLAccountAttribute CreateAccAlternateGlAccountAttribute(AccAlternateGLAccount account, ZGuid glHeaderPK, int sequence = 1, string attribute = "OCG", string attributeValue = "", Guid? attributeValuePK = null)
		{
			var attributeBO = account.AlternateGLAccountAttributes.AddNew();
			attributeBO.AAA_AAC_AlternateChart = account.AGA_AAC_AlternateChart;
			attributeBO.AAA_AG_GLHeader = glHeaderPK;
			attributeBO.AAA_Sequence = sequence;
			attributeBO.AAA_Attribute = attribute;
			attributeBO.AAA_Value = attributeValue;
			if (attributeValuePK != null)
			{
				attributeBO.AAA_AttributeValueID = attributeValuePK.Value;
			}
			return attributeBO;
		}

		public List<AccAlternateGLAccountAttribute> CreateAttributesForAlteranteGLAccount(AccAlternateGLAccount alternateGLAccount, AccGLHeader glHeader, int seq, Guid organizationPK, string oCGValue, string lFEValue, string lFOValue, string tICValue, string sPRValue)
		{
			var attributes = new List<AccAlternateGLAccountAttribute>();
			if (organizationPK != Guid.Empty)
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "ORG", attributeValuePK: organizationPK));
			}
			if (!string.IsNullOrEmpty(oCGValue))
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "OCG", oCGValue));
			}
			if (!string.IsNullOrEmpty(lFOValue))
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "LFO", lFOValue));
			}
			if (!string.IsNullOrEmpty(lFEValue))
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "LFE", lFEValue));
			}
			if (!string.IsNullOrEmpty(tICValue))
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "TIC", tICValue));
			}
			if (!string.IsNullOrEmpty(sPRValue))
			{
				attributes.Add(CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, seq, "SPR", sPRValue));
			}

			return attributes;
		}

		public AccAlternateGLAccountDissection CreateAccAlternateGLAccountDissection(AccGLHeader glHeader, ZGuid chartPK, ZString attribute, ZBool separateNumbering)
		{
			var dissection = glHeader.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = chartPK;
			dissection.ADC_Attribute = attribute;
			dissection.ADC_SeparateNumbering = separateNumbering;
			return dissection;
		}

		public DataTransfer.Xml.XsdVersion1.AlternateGLAccountsAlternateGLAccount CreateAlternateGlAccountForCSVImport(DataTransfer.Xml.XsdVersion1.AlternateGLAccountsAlternateGLAccountCollection alternateGLAccountCollection, ZString glHeaderNum, string accountNum, string accountType, ZString chartCode, string accountName, string debitCredit, string reportSection, int totalLevel, int printSequence,
			string alternateNum = null, string consolidate = null, string percentNum = null, string totalReference = null, string orgAttrValue = null, string ocgAttrValue = null, string lfoAttrValue = null, string lfeAttrValue = null, string ticAttrValue = null, string sprAttrValue = null)
		{
			var alternateGLAccount = alternateGLAccountCollection.AddNew();
			alternateGLAccount.ParentAccount = glHeaderNum;
			alternateGLAccount.AccountNum = accountNum;
			alternateGLAccount.AccountType = accountType;
			alternateGLAccount.ChartCode = chartCode;
			alternateGLAccount.AccountName = accountName;
			alternateGLAccount.DebitCredit = debitCredit;
			alternateGLAccount.ReportSection = reportSection;
			alternateGLAccount.TotalLevel = totalLevel;
			alternateGLAccount.PrintSequence = printSequence;
			alternateGLAccount.AlternateNum = alternateNum;
			alternateGLAccount.ConsolidationNum = consolidate;
			alternateGLAccount.PercentNum = percentNum;
			alternateGLAccount.TotalReference = totalReference;
			alternateGLAccount.ORGAttrValue = orgAttrValue;
			alternateGLAccount.OCGAttrValue = ocgAttrValue;
			alternateGLAccount.LFOAttrValue = lfoAttrValue;
			alternateGLAccount.LFEAttrValue = lfeAttrValue;
			alternateGLAccount.TICAttrValue = ticAttrValue;
			alternateGLAccount.SPRAttrValue = sprAttrValue;

			return alternateGLAccount;
		}

		public AccReportingBook CreateReportingBook(string code, string description, ZGuid chartPK, string includePresentationJournals, bool includeChildPresentation = false)
		{
			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = code;
			reportingBook.ARB_Description = description;
			reportingBook.ARB_AAC_AlternateChart = chartPK;
			reportingBook.ARB_GC_CompanyOfPeriod = Env.CurrentCompanyPK;
			reportingBook.ARB_IsActive = true;
			reportingBook.ARB_IncludePresentationJournals = includePresentationJournals;
			reportingBook.ARB_IncludeChildPresentation = includeChildPresentation;
			return reportingBook;
		}

		public AccTransactionLineDissectionAttribute CreateTransactionLineDissectionAttribtues(ZGuid linePk, string attr, string attrValue = "", Guid? attrValueID = null)
		{
			var lineDissectionAttribute = Factory.New<AccTransactionLineDissectionAttribute>();

			if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
			{
				lineDissectionAttribute.ALD_AttributeValueID = attrValueID ?? ZGuid.Empty;
			}
			else
			{
				lineDissectionAttribute.ALD_AttributeValue = attrValue;
			}
			lineDissectionAttribute.ALD_Attribute = attr;
			lineDissectionAttribute.ALD_AL_TransactionLine = linePk;
			return lineDissectionAttribute;
		}

		#endregion

		#region EDIMessage

		public EDIMessage CreateAndLinkUniversalEvent(IStmALogParent parentBO, string universalEvent,
			Event eventType,
			string applicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging,
			string messageType = EDIMessageTypeList.Codes.XDC,
			string messageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
			string receiveTransmit = EDIMessage.Direction.Receive,
			string status = EDIMessage.Direction.Receive
		)
		{
			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = universalEvent;

			var linker = new MessageDataLogLinker(eventType, Factory, ZString.Empty);
			linker.LinkMessageToParentBOLogs(message, parentBO);
			Factory.Save();

			return message;
		}

		#endregion

		#region Assign Journal Number

		public void SetJournalEntriesNumberAllocationOptionToGen(GlbCompany company)
		{
			var companyPK = company.PK.ToGuid();
			InitBeforeSetJournalEntriesNumberAllocationOptionToGen(company);
			var setting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(companyPK, Guid.Empty, Guid.Empty))
			{
				AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation
				.SetValue(companyPK, Guid.Empty, Guid.Empty, setting);
		}

		public void InitBeforeSetJournalEntriesNumberAllocationOptionToGen(GlbCompany company)
		{
			CreateTestPeriodsForEntireYear(company, Today.Year);
			SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions
				.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}

		#endregion

		#region AccDraftInvoice

		public AccDraftInvoiceHeader CreateUploadedDraftInvoice(ZString currency)
			=> CreateDraftTransaction(TransactionTypes.Invoice, AccDraftInvoiceHeaderStatus.Analyzing, currency);

		public AccDraftInvoiceHeader CreateDraftInvoice(ZString transactionNumber, ZString internalReferenceNumber, ZGuid creditorPk, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZString currency, string description = "Overriden AP Invoice")
			=> CreateDraftTransaction(TransactionTypes.Invoice, AccDraftInvoiceHeaderStatus.Draft, currency
				, internalReferenceNumber: internalReferenceNumber
				, transactionNumber: transactionNumber
				, creditorPk: creditorPk
				, description: description
				, osExTaxAmount: osExTaxAmount
				, osTaxAmount: osTaxAmount);

		public AccDraftInvoiceHeader CreateDraftCreditNote(ZString transactionNumber, ZString internalReferenceNumber, ZGuid creditorPk, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZString currency, string description = "Overriden AP Credit Note")
			=> CreateDraftTransaction(TransactionTypes.CreditNote, AccDraftInvoiceHeaderStatus.Draft, currency
				, internalReferenceNumber: internalReferenceNumber
				, transactionNumber: transactionNumber
				, creditorPk: creditorPk
				, description: description
				, osExTaxAmount: osExTaxAmount
				, osTaxAmount: osTaxAmount);

		public AccDraftInvoiceHeader CreateDraftTransaction(string transactionType, ZString status, ZString currency, ZString? internalReferenceNumber = null,
			ZString? transactionNumber = null, ZGuid? creditorPk = null, ZString? description = null, ZDecimal? osExTaxAmount = null, ZDecimal? osTaxAmount = null)
		{
			var draftInvoiceHeader = Factory.New<AccDraftInvoiceHeader>();

			draftInvoiceHeader.AIH_TransactionType = transactionType;
			draftInvoiceHeader.AIH_Status = status;
			draftInvoiceHeader.AIH_TransactionNumber = transactionNumber ?? ZString.Empty;
			draftInvoiceHeader.AIH_InternalReference = internalReferenceNumber ?? "001";
			draftInvoiceHeader.AIH_Description = description ?? GetDefaultDescription(transactionType);
			draftInvoiceHeader.AIH_OH_Creditor = creditorPk ?? ZGuid.Empty;
			draftInvoiceHeader.AIH_GC_Company = GlbBranch.CurrentBranch.GB_GC;
			draftInvoiceHeader.AIH_GB_Branch = GlbBranch.CurrentBranch.PK;
			draftInvoiceHeader.AIH_GE_Department = GlbDepartment.CurrentDepartment.PK;
			draftInvoiceHeader.AIH_RX_NKTransactionCurrency = currency;
			draftInvoiceHeader.AIH_ExchangeRate = 1;
			draftInvoiceHeader.AIH_ExpectedOSExTaxAmount = osExTaxAmount ?? ZDecimal.Zero;
			draftInvoiceHeader.AIH_ExpectedOSTaxAmount = osTaxAmount ?? ZDecimal.Zero;
			draftInvoiceHeader.AIH_ExpectedOSTotalAmount = draftInvoiceHeader.AIH_ExpectedOSExTaxAmount + draftInvoiceHeader.AIH_ExpectedOSTaxAmount;

			return draftInvoiceHeader;

			ZString GetDefaultDescription(string transactionType)
			{
				switch (transactionType)
				{
					case TransactionTypes.Invoice:
						return "AP Invoice";
					case TransactionTypes.CreditNote:
						return "AP Credit Note";
					default:
						return ZString.Empty;
				}
			}
		}

		public AccDraftInvoiceJob AddJobToTheCluster(AccDraftInvoiceJobCluster cluster, IJobHeaderParent jobHeaderParent)
		{
			var draftJob = cluster.OperationalJobs.AddNew();
			draftJob.AIJ_GC_Company = cluster.AIC_GC_Company;
			draftJob.AIJ_ParentID = jobHeaderParent.PK;
			draftJob.AIJ_ParentTableCode = jobHeaderParent.TablePrefix();
			return draftJob;
		}

		public AccDraftInvoiceJob AddConsolToTheCluster(AccDraftInvoiceJobCluster cluster, IJobCostingPlugIn jobHeaderParent)
		{
			var draftJob = cluster.OperationalJobs.AddNew();
			draftJob.AIJ_GC_Company = cluster.AIC_GC_Company;
			draftJob.AIJ_ParentID = jobHeaderParent.PK;
			draftJob.AIJ_ParentTableCode = (jobHeaderParent as BusinessObject).TablePrefix;
			return draftJob;
		}

		public AccDraftInvoiceJobCluster AddClusterToDraftTransaction(AccDraftInvoiceHeader draftTransaction, ZDecimal clusterAmount)
		{
			var cluster = draftTransaction.JobClusters.AddNew();
			cluster.AIC_Amount = clusterAmount;
			cluster.AIC_GC_Company = draftTransaction.AIH_GC_Company;
			cluster.AIC_RX_NKCurrency = draftTransaction.AIH_RX_NKTransactionCurrency;
			return cluster;
		}

		public AccDraftInvoiceExRate AddDraftInvocieExchangeRate(AccDraftInvoiceHeader draftInvoiceHeader, RefCurrency currency, decimal exchangeRate, bool isReciprocal = true)
		{
			var exRate1 = draftInvoiceHeader.ExchangeRates.AddNew();
			exRate1.AIE_RX_NKRateCurrency = currency.Code;
			exRate1.AIE_ExchangeRate = exchangeRate;
			exRate1.AIE_IsReciprocal = isReciprocal;
			return exRate1;
		}

		public AccDraftInvoiceHeader CreateDraftWithJob(string transactionType, ZString transactionNumber, ZGuid creditorPk, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZString currency, IJobHeaderParent job)
		{
			var draft = CreateDraftTransaction(transactionType, AccDraftInvoiceHeaderStatus.Draft, currency,
				internalReferenceNumber: transactionNumber,
				transactionNumber: $"{transactionType}-{transactionNumber}",
				creditorPk: creditorPk,
				description: $"AP {transactionType} {transactionNumber}",
				osExTaxAmount: osExTaxAmount,
				osTaxAmount: osTaxAmount);
			var cluster = AddClusterToDraftTransaction(draft, osExTaxAmount);
			_ = AddJobToTheCluster(cluster, job);

			return draft;
		}

		public AccDraftInvoiceHeader CreateDraftWithConsol(string transactionType, ZString transactionNumber, ZGuid creditorPk, ZDecimal osExTaxAmount, ZDecimal osTaxAmount, ZString currency, IJobCostingPlugIn consol)
		{
			var draft = CreateDraftTransaction(transactionType, AccDraftInvoiceHeaderStatus.Draft, currency,
				internalReferenceNumber: transactionNumber,
				transactionNumber: $"{transactionType}-{transactionNumber}",
				creditorPk: creditorPk,
				description: $"AP {transactionType} {transactionNumber}",
				osExTaxAmount: osExTaxAmount,
				osTaxAmount: osTaxAmount);
			var cluster = AddClusterToDraftTransaction(draft, osExTaxAmount);
			_ = AddConsolToTheCluster(cluster, consol);

			return draft;
		}

		#endregion

		public AccCFXUpliftConfiguration CreateCFXUplift(AccCFXUpliftConfigurationCollection cfxConfigurations, string jobType = "ALL",
			string serviceDirection = "ALL", string origin = "", string destination = "", string transportMode = "ALL",
			string currencyCode = "", decimal percentage = 0, decimal minimum = 0, ZDate? startDate = null, ZDate? expiryDate = null)
		{
			var newCfxUplift = cfxConfigurations.AddNew();
			newCfxUplift.JCF_JobType = jobType;
			newCfxUplift.JCF_ServiceDirection = serviceDirection;
			newCfxUplift.JCF_TransportMode = transportMode;
			newCfxUplift.JCF_RN_NKOriginCountry = origin;
			newCfxUplift.JCF_RN_NKDestinationCountry = destination;
			newCfxUplift.JCF_RX_NKCurrency = currencyCode;
			newCfxUplift.JCF_CFXPercentage = percentage;
			newCfxUplift.JCF_CFXMinimum = minimum;
			newCfxUplift.JCF_StartDate = startDate ?? ZDate.Empty;
			newCfxUplift.JCF_ExpiryDate = expiryDate ?? ZDate.Empty;

			return newCfxUplift;
		}

		public RefAccElectronicProcessingFee CreateRefAccElectronicProcessingFee(string systemCode, string categoryCode, string electronicProcessingFeeCode, string currency, decimal price, ZDateTime validFromDate, string countryCode = "", string direction = "ALL")
		{
			var refAccElectronicProcessingFee = Factory.New<RefAccElectronicProcessingFee>();
			refAccElectronicProcessingFee.EPF_SystemCode = systemCode;
			refAccElectronicProcessingFee.EPF_Category = categoryCode;
			refAccElectronicProcessingFee.EPF_Code = electronicProcessingFeeCode;
			refAccElectronicProcessingFee.EPF_Description = "Electronic Processing Fee";
			refAccElectronicProcessingFee.EPF_Currency = currency;
			refAccElectronicProcessingFee.EPF_Price = price;
			refAccElectronicProcessingFee.EPF_ValidFrom = validFromDate;
			refAccElectronicProcessingFee.EPF_CountryCode = countryCode;
			refAccElectronicProcessingFee.EPF_JobDirection = direction;

			return refAccElectronicProcessingFee;
		}

		public void MockNudgeGLDProcessData(DataRow[] dataRows)
		{
			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				ObjectFactory.Get<IGeneralLedgerDataProcessor>().ProcessData(dataRows);
			}
		}

		public void MockNudgeGLDProcessData(BusinessObject[] gLDDataSources)
		{
			MockNudgeGLDProcessData(gLDDataSources.Select(x => ((IBusinessObjectInternals)x).Row).ToArray());
		}
	}
}

#endif
