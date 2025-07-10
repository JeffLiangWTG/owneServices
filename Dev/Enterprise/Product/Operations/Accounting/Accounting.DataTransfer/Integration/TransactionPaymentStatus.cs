using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
#if DEBUG
using Enterprise.Accounting.DataTransfer.Integration.Testing;
#endif
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.CreditLimitService;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.Integration
{
	public class TransactionPaymentStatus : ITransactionPaymentStatus
	{
		public TransactionPaymentStatus(string orgCode)
			: this(orgCode, GlbCompany.CurrentCompany.GC_Code)
		{
		}

		protected TransactionPaymentStatus(string orgCode, string companyCode)
		{
			Argument.NotNullOrEmpty(orgCode, "OrgCode");
			Argument.NotNullOrEmpty(companyCode, "CompanyCode");

			this.orgCode = orgCode;
			this.companyCode = companyCode;
			this.cachedValues = new Dictionary<Key, Detail>();
		}

		readonly Dictionary<Key, Detail> cachedValues;

		class Detail
		{
			public string AccLedger { get; set; }
			public string CompanyCode { get; set; }
			public string CurrencyCode { get; set; }
			public decimal InvoiceTotal { get; set; }
			public string JobTransactionNumber { get; set; }
			public string OrgCode { get; set; }
			public decimal PaidAmount { get; set; }
			public string PaymentStatus { get; set; }
			public string TransactionNumber { get; set; }
			public string TransactionType { get; set; }
			public DateTime? FullyPaidDate { get; set; }
		}

		class Key
		{
			public Key(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
			{
				Ledger = ledger;
				TransactionType = transactionType;
				TransactionNumber = transactionNumber;
				JobTransactionNumber = jobTransactionNumber;
			}

			public readonly string Ledger;
			public readonly string TransactionType;
			public readonly string TransactionNumber;
			public readonly string JobTransactionNumber;

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					throw new ArgumentNullException(nameof(obj));
				}
				if (obj is Key)
				{
					Key key = obj as Key;
					return this.Ledger == key.Ledger && this.TransactionType == key.TransactionType
						&& this.TransactionNumber == key.TransactionNumber && this.JobTransactionNumber == key.JobTransactionNumber;
				}
				else
				{
					throw new InvalidOperationException("obj must be a Key");
				}
			}

			public override int GetHashCode()
			{
				return Ledger.GetHashCode() ^ TransactionType.GetHashCode() ^ TransactionNumber.GetHashCode() ^ JobTransactionNumber.GetHashCode();
			}
		}

		public string OrgCode
		{
			get
			{
				return orgCode;
			}
		}

		readonly string orgCode;

		public string CompanyCode
		{
			get
			{
				return companyCode;
			}
		}

		readonly string companyCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localized")]
		static string errorMessageCommon
		{
			get { return ": Failed to get value."; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localised")]
		void handleWebServiceException(Exception ex, string errorMessage, string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
		{
			if (ex.Message == "Transaction not found")
			{
				throw new TransactionNotFoundException(string.Format("A transaction was not found in the remote system for Company Code:'{0}' Organisation Code:'{1}' Ledger:'{2}' TransactionType:'{3}' TransactionNumber:'{4}' Job Transaction Number:'{5}'",
																	CompanyCode, OrgCode, ledger, transactionType, transactionNumber, jobTransactionNumber));
			}
			else
			{
				throw new InvalidOperationException(errorMessage, ex);
			}
		}

		public string PaymentStatus(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
		{
			string errorMessage = "PaymentStatus" + errorMessageCommon;

			Key key = new Key(ledger, transactionType, transactionNumber, jobTransactionNumber);
			if (!cachedValues.ContainsKey(key))
			{
				try
				{
					GetTransactionPaymentStatusFromWebService(ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					handleWebServiceException(ex, errorMessage, ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
			}

			Detail detail;
			if (cachedValues.TryGetValue(key, out detail))
			{
				return detail.PaymentStatus;
			}

			throw new InvalidOperationException(errorMessage);
		}

		public decimal OutstandingAmount(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
		{
			string errorMessage = "OutstandingAmount" + errorMessageCommon;

			Key key = new Key(ledger, transactionType, transactionNumber, jobTransactionNumber);
			if (!cachedValues.ContainsKey(key))
			{
				try
				{
					GetTransactionPaymentStatusFromWebService(ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					handleWebServiceException(ex, errorMessage, ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
			}

			Detail detail;
			if (cachedValues.TryGetValue(key, out detail))
			{
				return  detail.InvoiceTotal - detail.PaidAmount;
			}

			throw new InvalidOperationException(errorMessage);
		}

		public DateTime? FullyPaidDate(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
		{
			string errorMessage = "FullyPaidDate" + errorMessageCommon;

			Key key = new Key(ledger, transactionType, transactionNumber, jobTransactionNumber);
			if (!cachedValues.ContainsKey(key))
			{
				try
				{
					GetTransactionPaymentStatusFromWebService(ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					handleWebServiceException(ex, errorMessage, ledger, transactionType, transactionNumber, jobTransactionNumber);
				}
			}

			Detail detail;
			if (cachedValues.TryGetValue(key, out detail))
			{
				return detail.FullyPaidDate;
			}

			throw new InvalidOperationException(errorMessage);
		}

		void GetTransactionPaymentStatusFromWebService(string ledger, string transactionType, string transactionNumber, string jobTransactionNumber)
		{
			var service = CreateNewServiceClient();
			using ((IDisposable)service)
			{
				TransactionPaymentStatusRequest request = new TransactionPaymentStatusRequest();
				request.OrgCode = orgCode;
				request.CompanyCode = companyCode;
				request.AccLedger = ledger;
				request.TransactionType = transactionType;
				request.TransactionNumber = transactionNumber;
				request.JobTransactionNumber = jobTransactionNumber;

				TransactionPaymentStatusResponse response = service.CheckTransactionPaymentStatus(new CheckTransactionPaymentStatusRequest(GetSecuritySoapHeader(), request)).CheckTransactionPaymentStatusResult;
				if (response.Succeeded)
				{
					Key key = new Key(ledger, transactionType, transactionNumber, jobTransactionNumber);
					Detail detail = new Detail();

					var value = response.TransactionPaymentStatus;
					detail.InvoiceTotal = value.InvoiceTotal;
					detail.PaidAmount = value.PaidAmount;
					detail.PaymentStatus = value.PaymentStatus;
					detail.FullyPaidDate = value.FullyPaidDateHasValue ? value.FullyPaidDate : null;
					detail.AccLedger = value.AccLedger;
					detail.CompanyCode = value.CompanyCode;
					detail.CurrencyCode = value.CurrencyCode;
					detail.JobTransactionNumber = value.JobTransactionNumber;
					detail.OrgCode = value.OrgCode;
					detail.TransactionNumber = value.TransactionNumber;
					detail.TransactionType = value.TransactionType;

					if (cachedValues.ContainsKey(key))
					{
						cachedValues.Remove(key);
					}
					cachedValues.Add(key, detail);
				}
				else
				{
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						throw new Exception(response.ErrorMessage);
					}
				}
			}
		}

		static CreditLimitServiceSoap CreateNewServiceClient()
		{
			Binding binding = new BasicHttpBinding();
			string url = AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUrl.Value;
			EndpointAddress remoteAddress = !string.IsNullOrEmpty(url) ? new EndpointAddress(url) : null;

			var result =
#if DEBUG
				Globals.IsTest ? new MockCreditLimitServiceClientProvider(binding).GetClient() :
#endif
				new CreditLimitServiceSoapClient(binding, remoteAddress);

			return result;
		}

		static SecuritySOAPHeader GetSecuritySoapHeader()
		{
			var result = new SecuritySOAPHeader();
			result.UserName = AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServiceUserName.Value;
			result.Password = AccountingConfigurationRegistry.Instance.TransactionPaymentStatusWebServicePassword.Value;
			return result;
		}
	}

	public class TransactionPaymentStatusProvider : ITransactionPaymentStatusProvider
	{
		ITransactionPaymentStatus ITransactionPaymentStatusProvider.GetTransactionPaymentStatus(string orgCode)
		{
			return new TransactionPaymentStatus(orgCode);
		}
	}
}
