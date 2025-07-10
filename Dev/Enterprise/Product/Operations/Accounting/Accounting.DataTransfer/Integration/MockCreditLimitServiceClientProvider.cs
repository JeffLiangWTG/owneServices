#region Test
#if DEBUG

using System;
using System.ServiceModel.Channels;
using CargoWise.Application;
using Enterprise.Accounting.DataTransfer.CreditLimitService;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.DataTransfer.Integration.Testing
{
	public class MockCreditLimitServiceClientProvider
		: CreditLimitServiceSoapClientProvider
	{
		public MockCreditLimitServiceClientProvider(Binding binding)
			: base(binding, null)
		{
		}

		public override CreditLimitServiceSoap GetClient()
		{
			var result = new Mock<CreditLimitServiceSoap>();
			result.As<IDisposable>().Setup(disposable => disposable.Dispose());

			result
				.Setup(r => r.GetCreditLimitAndBalanceDetails(It.IsAny<GetCreditLimitAndBalanceDetailsRequest>()))
				.Returns((GetCreditLimitAndBalanceDetailsRequest x) =>
				{
					CountCheckCreditLimitAndBalanceDetailsWasInvoked++;

					if (CreditLimitReturnTimeoutForCount > 0)
					{
						CreditLimitReturnTimeoutForCount--;
						throw new TimeoutException("Timeout for Test");
					}

					var request = x.request;

					var creditLimitAndBalanceInfo = new CreditLimitAndBalanceInfo();
					var response = new GetCreditLimitAndBalanceDetailsResponse();
					var typedResult = response.GetCreditLimitAndBalanceDetailsResult = new CreditLimitAndBalanceResponse();
					typedResult.ErrorMessage = string.Empty;
					typedResult.Succeeded = true;
					typedResult.CreditLimitAndBalanceDetails = new CreditLimitAndBalanceInfo[1];
					typedResult.CreditLimitAndBalanceDetails[0] = creditLimitAndBalanceInfo;

					if (request != null)
					{
						creditLimitAndBalanceInfo.OrgCode = request.OrgCode;
						creditLimitAndBalanceInfo.CompanyCode = request.CompanyCode;
						creditLimitAndBalanceInfo.AccLedger = request.AccLedger;
						creditLimitAndBalanceInfo.OverdueAgingPeriod = request.OverdueAgingPeriod;
						creditLimitAndBalanceInfo.BalanceOverdueAgingOption = request.BalanceOverdueAgingOption;

						if (LoadDataFromSystemDB)
						{
							LoadCreditDetailsFromSystemDB(request.OrgCode, creditLimitAndBalanceInfo);
						}
						else if (request.OrgCode == "AALSHI")
						{
							creditLimitAndBalanceInfo.OnCreditHoldHasValue = true;
							creditLimitAndBalanceInfo.OnCreditHold = true;
							creditLimitAndBalanceInfo.IsOverCreditLimitHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditLimit = true;
							creditLimitAndBalanceInfo.IsOverCreditTermsHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditTerms = true;
							creditLimitAndBalanceInfo.CreditLimitHasValue = true;
							creditLimitAndBalanceInfo.CreditLimit = 1000.00m;
							creditLimitAndBalanceInfo.AccountBalanceTotalHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceTotal = 1500.00m;
							creditLimitAndBalanceInfo.OverdueTotalHasValue = true;
							creditLimitAndBalanceInfo.OverdueTotal = 500.00m;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdueHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdue = 100.00m;
							creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotalHasValue = true;
							creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotal = 100.00m;
							creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotalHasValue = true;
							creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotal = 100.00m;
							creditLimitAndBalanceInfo.ClaimTotalHasValue = true;
							creditLimitAndBalanceInfo.ClaimTotal = 0m;
							creditLimitAndBalanceInfo.GlobalCreditGroupOrgCode = "tst";
							if (CountCheckCreditLimitAndBalanceDetailsWasInvoked > 1)
							{
								creditLimitAndBalanceInfo.GlobalCreditGroupOrgCode = "new";
							}
						}
						else if (request.OrgCode == "ABIGAS")
						{
							creditLimitAndBalanceInfo.OnCreditHoldHasValue = true;
							creditLimitAndBalanceInfo.OnCreditHold = false;
							creditLimitAndBalanceInfo.IsOverCreditLimitHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditLimit = false;
							creditLimitAndBalanceInfo.IsOverCreditTermsHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditTerms = false;
							creditLimitAndBalanceInfo.CreditLimitHasValue = true;
							creditLimitAndBalanceInfo.CreditLimit = 1000.00m;
							creditLimitAndBalanceInfo.AccountBalanceTotalHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceTotal = 1500.00m;
							creditLimitAndBalanceInfo.OverdueTotalHasValue = true;
							creditLimitAndBalanceInfo.OverdueTotal = 500.00m;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdueHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdue = 100.00m;
							creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotalHasValue = true;
							creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotal = 200.00m;
							creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotalHasValue = true;
							creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotal = 200.00m;
							creditLimitAndBalanceInfo.ClaimTotalHasValue = true;
							creditLimitAndBalanceInfo.ClaimTotal = 0m;
						}
						else if (request.OrgCode == "XXFAIL")
						{
							typedResult.Succeeded = false;
							typedResult.ErrorMessage = "Some fail error message.";
						}
						else if (request.OrgCode == "EXCEPT")
						{
							throw new Exception("Some exception error message.");
						}
						else if (request.OrgCode == "EMPTYD")
						{
							typedResult.CreditLimitAndBalanceDetails = Array.Empty<CreditLimitAndBalanceInfo>();
						}
						else if (request.OrgCode == "NULL1ST")
						{
							typedResult.CreditLimitAndBalanceDetails[0] = null;
						}
						else if (request.OrgCode == "INVALD")
						{
							typedResult.CreditLimitAndBalanceDetails = new CreditLimitAndBalanceInfo[2];
						}
						else if (request.OrgCode == "NULLD")
						{
							typedResult.CreditLimitAndBalanceDetails = null;
						}
						else if (request.OrgCode == "NULLRESP")
						{
							response = null;
						}
						else if (request.OrgCode == "ZCLAIM")
						{
							creditLimitAndBalanceInfo.AccountBalanceTotalHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceTotal = 0m;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdueHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdue = 0m;
							creditLimitAndBalanceInfo.CreditLimitHasValue = true;
							creditLimitAndBalanceInfo.CreditLimit = 0m;
							creditLimitAndBalanceInfo.ClaimTotalHasValue = true;
							creditLimitAndBalanceInfo.ClaimTotal = 100m;
							creditLimitAndBalanceInfo.OnCreditHoldHasValue = true;
							creditLimitAndBalanceInfo.OnCreditHold = false;
							creditLimitAndBalanceInfo.IsOverCreditLimitHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditLimit = false;
							creditLimitAndBalanceInfo.IsOverCreditTermsHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditTerms = false;
						}
						else if (request.OrgCode == "ZWOCLAIMAMNT")
						{
							creditLimitAndBalanceInfo.AccountBalanceTotalHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceTotal = 0m;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdueHasValue = true;
							creditLimitAndBalanceInfo.AccountBalanceNotOverdue = 0m;
							creditLimitAndBalanceInfo.CreditLimitHasValue = true;
							creditLimitAndBalanceInfo.CreditLimit = 0m;
							creditLimitAndBalanceInfo.OnCreditHoldHasValue = true;
							creditLimitAndBalanceInfo.OnCreditHold = false;
							creditLimitAndBalanceInfo.IsOverCreditLimitHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditLimit = false;
							creditLimitAndBalanceInfo.IsOverCreditTermsHasValue = true;
							creditLimitAndBalanceInfo.IsOverCreditTerms = false;
						}
					}
					return response;
				});

			result
				.Setup(r => r.CheckTransactionPaymentStatus(It.IsAny<CheckTransactionPaymentStatusRequest>()))
				.Returns((CheckTransactionPaymentStatusRequest x) =>
				{
					var request = x.request;

					CountCheckTransactionPaymentStatusWasInvoked++;

					var response = new CheckTransactionPaymentStatusResponse();
					var typedResult = response.CheckTransactionPaymentStatusResult = new TransactionPaymentStatusResponse();
					typedResult.ErrorMessage = string.Empty;
					typedResult.Succeeded = true;
					typedResult.TransactionPaymentStatus = new TransactionPaymentStatusInfo();

					if (request != null)
					{
						typedResult.TransactionPaymentStatus.AccLedger = request.AccLedger;
						typedResult.TransactionPaymentStatus.CompanyCode = request.CompanyCode;
						typedResult.TransactionPaymentStatus.JobTransactionNumber = request.JobTransactionNumber;
						typedResult.TransactionPaymentStatus.OrgCode = request.OrgCode;
						typedResult.TransactionPaymentStatus.TransactionNumber = request.TransactionNumber;
						typedResult.TransactionPaymentStatus.TransactionType = request.TransactionType;
					}

					typedResult.TransactionPaymentStatus.CurrencyCode = Constants.CurrencyCodes.Australia;

					if (request.TransactionNumber == "00001000")
					{
						typedResult.TransactionPaymentStatus.InvoiceTotal = 100.00m;
						typedResult.TransactionPaymentStatus.PaidAmount = 60.00m;
						typedResult.TransactionPaymentStatus.PaymentStatus = "PARTPAID";
						typedResult.TransactionPaymentStatus.FullyPaidDateHasValue = false;
					}
					else if (request.TransactionNumber == "00002000")
					{
						typedResult.TransactionPaymentStatus.InvoiceTotal = 100.00m;
						typedResult.TransactionPaymentStatus.PaidAmount = 0.00m;
						typedResult.TransactionPaymentStatus.PaymentStatus = "UNPAID";
						typedResult.TransactionPaymentStatus.FullyPaidDateHasValue = false;
					}
					else if (request.TransactionNumber == "00003000")
					{
						typedResult.TransactionPaymentStatus.InvoiceTotal = 100.00m;
						typedResult.TransactionPaymentStatus.PaidAmount = 100.00m;
						typedResult.TransactionPaymentStatus.PaymentStatus = "PAID";
						typedResult.TransactionPaymentStatus.FullyPaidDateHasValue = true;
						typedResult.TransactionPaymentStatus.FullyPaidDate = new DateTime(2010, 10, 31);
					}
					else if (string.IsNullOrEmpty(request.TransactionNumber) && request.JobTransactionNumber == "S00001000")
					{
						typedResult.TransactionPaymentStatus.InvoiceTotal = 200.00m;
						typedResult.TransactionPaymentStatus.PaidAmount = 200.00m;
						typedResult.TransactionPaymentStatus.PaymentStatus = "PAID";
						typedResult.TransactionPaymentStatus.FullyPaidDateHasValue = true;
						typedResult.TransactionPaymentStatus.FullyPaidDate = new DateTime(2010, 11, 30);
					}
					else if (request.TransactionNumber == "00004000")
					{
						typedResult.Succeeded = false;
						typedResult.ErrorMessage = "Transaction not found";
					}

					return response;
				});

			return result.Object;
		}

		void LoadCreditDetailsFromSystemDB(string orgCode, CreditLimitAndBalanceInfo creditLimitAndBalanceInfo)
		{
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var provider = ObjectFactory.Get<IOrgCreditLimitAndBalanceDetailsProvider>();
				var creditInfo = provider.GetOrgCreditLimitAndBalanceDetails(orgCode);

				creditLimitAndBalanceInfo.OnCreditHoldHasValue = true;
				creditLimitAndBalanceInfo.OnCreditHold = creditInfo.OnCreditHold(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.IsOverCreditLimitHasValue = true;
				creditLimitAndBalanceInfo.IsOverCreditLimit = creditInfo.IsOverCreditLimit(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.IsOverCreditTermsHasValue = true;
				creditLimitAndBalanceInfo.IsOverCreditTerms = creditInfo.IsOverCreditTerms(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.CreditLimitHasValue = true;
				creditLimitAndBalanceInfo.CreditLimit = creditInfo.CreditLimit(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.AccountBalanceTotalHasValue = true;
				creditLimitAndBalanceInfo.AccountBalanceTotal = creditInfo.OutstandingBalance(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.OverdueTotalHasValue = true;
				creditLimitAndBalanceInfo.OverdueTotal = creditInfo.OutstandingBalanceOverdue(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.AccountBalanceNotOverdueHasValue = true;
				creditLimitAndBalanceInfo.AccountBalanceNotOverdue = creditInfo.OutstandingBalanceNotOverdue(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotalHasValue = true;
				creditLimitAndBalanceInfo.UnpostedRevenueRecognisedTotal = creditInfo.UnpostedRevenueRecognised(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotalHasValue = true;
				creditLimitAndBalanceInfo.UnpostedRevenueUnrecognisedTotal = creditInfo.UnpostedRevenueUnrecognised(LedgerTypes.AccountsReceivable);
				creditLimitAndBalanceInfo.ClaimTotalHasValue = true;
				creditLimitAndBalanceInfo.ClaimTotal = creditInfo.Claim(LedgerTypes.AccountsReceivable);
			}
		}

		public static int CountCheckTransactionPaymentStatusWasInvoked
		{
			get { return countCheckTransactionPaymentStatusWasInvoked; }
			private set { countCheckTransactionPaymentStatusWasInvoked = value; }
		}
		[ThreadStatic]
		static int countCheckTransactionPaymentStatusWasInvoked;

		public static int CountCheckCreditLimitAndBalanceDetailsWasInvoked
		{
			get { return countCheckCreditLimitAndBalanceDetailsWasInvoked; }
			private set { countCheckCreditLimitAndBalanceDetailsWasInvoked = value; }
		}
		[ThreadStatic]
		static int countCheckCreditLimitAndBalanceDetailsWasInvoked;

		public static void Reset(bool loadFromDB = false)
		{
			LoadDataFromSystemDB = loadFromDB;
			CountCheckTransactionPaymentStatusWasInvoked = 0;
			CountCheckCreditLimitAndBalanceDetailsWasInvoked = 0;
			CreditLimitReturnTimeoutForCount = 0;
		}

		[ThreadStatic]
		static bool LoadDataFromSystemDB;

		[ThreadStatic]
		public static int CreditLimitReturnTimeoutForCount;
	}
}

#endif
#endregion
