using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.CreditLimitService;
using Enterprise.Accounting.DataTransfer.Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Exceptions = Enterprise.MasterFiles.Business.OrgCreditLimitAndBalanceExceptions;

namespace Enterprise.Accounting.DataTransfer.Integration
{
	public partial class OrgCreditLimitAndBalanceDetails : IOrgCreditLimitAndBalanceDetails
	{
		public OrgCreditLimitAndBalanceDetails(string orgCode)
			: this(orgCode, Env.CurrentCompany.Code)
		{
		}

		protected OrgCreditLimitAndBalanceDetails(string orgCode, string companyCode)
		{
			Argument.NotNullOrEmpty(orgCode, "OrgCode");
			Argument.NotNullOrEmpty(companyCode, "CompanyCode");

			this.orgCode = orgCode;
			this.companyCode = companyCode;
			this.localCurrencyDecimals = Env.CurrentCompany.LocalCurrency.Decimals;
			lockRoot = new object();
		}

		readonly object lockRoot;

		public string OrgCode
		{
			get { return orgCode; }
		}

		readonly string orgCode;

		public string CompanyCode
		{
			get { return companyCode; }
		}

		readonly string companyCode;

		readonly int localCurrencyDecimals;

		public void FetchCreditLimitAndBalanceFromWebService(string ledger, params string[] options)
		{
			lock (lockRoot)
			{
				var detailsFieldStrategies = GenerateDetailsFieldStrategies();
				var detailsFieldStrategiesForWebService = detailsFieldStrategies.Where(x => x.UseWebService).ToArray();
				var optionsToFetch = options.Any() ? options : new string[] {
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue
					};

				foreach (var option in optionsToFetch)
				{
					if (!cachedValuesFromWebService.ContainsKey(new Key(ledger, option)))
					{
						GetCreditLimitAndBalanceFromWebService(ledger, option, detailsFieldStrategiesForWebService);
					}
				}
				lastDetailStrategies = detailsFieldStrategies;
			}
		}

		public bool OnCreditHold(string ledger)
		{
			return GetDetails<bool>("OnCreditHold", ledger,
				Constants.BalanceOverdueAgingOption.Balance,
				Constants.BalanceOverdueAgingOption.Overdue,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
		}

		public bool IsOverCreditLimit(string ledger)
		{
			return GetDetails<bool>("IsOverCreditLimit", ledger,
				Constants.BalanceOverdueAgingOption.Balance,
				Constants.BalanceOverdueAgingOption.Overdue,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
		}

		public bool IsOverCreditTerms(string ledger)
		{
			return GetDetails<bool>("IsOverCreditTerms", ledger,
				Constants.BalanceOverdueAgingOption.Overdue);
		}

		public decimal CreditLimit(string ledger)
		{
			return GetDetails<decimal>("CreditLimit", ledger,
				Constants.BalanceOverdueAgingOption.Balance,
				Constants.BalanceOverdueAgingOption.Overdue,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
		}

		public decimal OutstandingBalance(string ledger)
		{
			return GetDetails<decimal>("OutstandingBalance", ledger,
				Constants.BalanceOverdueAgingOption.Balance,
				Constants.BalanceOverdueAgingOption.Overdue,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "DB Column")]
		public decimal Claim(string ledger)
		{
			return GetDetails<decimal>("Claim", ledger,
				Constants.BalanceOverdueAgingOption.Balance);
		}

		public decimal OutstandingBalanceOverdue(string ledger)
		{
			return GetDetails<decimal>("OutstandingBalanceOverdue", ledger,
				Constants.BalanceOverdueAgingOption.Overdue);
		}

		public decimal OutstandingBalanceNotOverdue(string ledger)
		{
			return GetDetails<decimal>("OutstandingBalanceNotOverdue", ledger,
				Constants.BalanceOverdueAgingOption.Overdue);
		}

		public decimal UnpostedRevenueRecognised(string ledger)
		{
			return GetDetails<decimal>("UnpostedRevenueRecognised", ledger,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue,
				Constants.BalanceOverdueAgingOption.Overdue);
		}

		public decimal UnpostedRevenueUnrecognised(string ledger)
		{
			return GetDetails<decimal>("UnpostedRevenueUnrecognised", ledger,
				Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue,
				Constants.BalanceOverdueAgingOption.Overdue);
		}

		public decimal UnpostedRevenue(string ledger)
		{
			return UnpostedRevenueRecognised(ledger) + UnpostedRevenueUnrecognised(ledger);
		}

		public bool IsARGlobalCreditApproved
		{
			get
			{
				return GetDetails<bool>("IsGlobalCreditApproved", LedgerTypes.AccountsReceivable,
					Constants.BalanceOverdueAgingOption.Balance,
					Constants.BalanceOverdueAgingOption.Overdue,
					Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			}
		}

		public bool OnARGlobalCreditHold
		{
			get
			{
				return IsARGlobalCreditApproved && GetDetails<bool>("OnGlobalCreditHold", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			}
		}

		public bool IsOverARGlobalCreditLimit
		{
			get
			{
				return IsARGlobalCreditApproved && GetDetails<bool>("IsOverGlobalCreditLimit", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			}
		}

		public string ARGlobalCreditCurrencyCode
		{
			get
			{
				return IsARGlobalCreditApproved ? GetDetails<ZString>("GlobalCreditCurrencyCode", LedgerTypes.AccountsReceivable,
					Constants.BalanceOverdueAgingOption.Balance,
					Constants.BalanceOverdueAgingOption.Overdue,
					Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue) :
					ZString.Empty;
			}
		}

		public decimal ARGlobalCreditLimit
		{
			get
			{
				return IsARGlobalCreditApproved ? GetDetails<decimal>("GlobalCreditLimit", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue) :
					decimal.Zero;
			}
		}

		public decimal ARGlobalOutstandingBalance
		{
			get
			{
				return IsARGlobalCreditApproved && !UnableToCalculateARGlobalOutstandingBalance ?
					GetDetails<decimal>("GlobalOutstandingBalance", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue) :
					decimal.Zero;
			}
		}

		public decimal ARGlobalClaim
		{
			get
			{
				return IsARGlobalCreditApproved && !UnableToCalculateARGlobalOutstandingBalance ?
					GetDetails<decimal>("GlobalClaim", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance) :
					decimal.Zero;
			}
		}

		public bool UnableToCalculateARGlobalOutstandingBalance
		{
			get
			{
				return IsARGlobalCreditApproved && GetDetails<bool>("UnableToCalculateGlobalOutstandingBalance", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.Balance,
						Constants.BalanceOverdueAgingOption.Overdue,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			}
		}

		public decimal ARGlobalUnpostedRevenueRecognised
		{
			get
			{
				return IsARGlobalCreditApproved && !UnableToCalculateARGlobalUnpostedRevenue ?
					GetDetails<decimal>("GlobalUnpostedRevenueRecognised", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue) :
					decimal.Zero;
			}
		}

		public decimal ARGlobalUnpostedRevenueUnrecognised
		{
			get
			{
				return IsARGlobalCreditApproved && !UnableToCalculateARGlobalUnpostedRevenue ?
					GetDetails<decimal>("GlobalUnpostedRevenueUnrecognised", LedgerTypes.AccountsReceivable,
						Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue) :
					decimal.Zero;
			}
		}

		public bool UnableToCalculateARGlobalUnpostedRevenue
		{
			get
			{
				return IsARGlobalCreditApproved && GetDetails<bool>("UnableToCalculateGlobalUnpostedRevenue", LedgerTypes.AccountsReceivable,
					Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue);
			}
		}

		public string GetErrorMessage(string errorContext, Exception exception)
		{
			var dbException = exception.InnerException as Exceptions.DbException;
			var webServiceException = exception.InnerException as Exceptions.WebServiceException;
			string notInCacheErroMessage = Res.GetString("d3cbcf10-e1a3-4529-ad93-f24f13571e61", "Data was not retrieved.");
			string message = errorContext;
			if (dbException != null)
			{
				message += " " + Res.GetString("4d305e58-fc63-424d-99cb-82b953269acd", "An error occurred when retrieving data from a database.");
				switch (dbException.Reason)
				{
					case Exceptions.DbException.ExceptionReason.EmptyData:
						message += " " + Res.GetString("5016652d-2c9a-4caf-b269-51582fd9fab6", "Data was not found.");
						break;

					case Exceptions.DbException.ExceptionReason.InvalidData:
						message += " " + Res.GetString("01a54e46-c866-4900-9888-16d4960e46fc", "Data is invalid.");
						break;

					case Exceptions.DbException.ExceptionReason.NoDataInCache:
						message += notInCacheErroMessage;
						break;
				}
			}
			else if (webServiceException != null)
			{
				message += " " + Res.GetString("c3f5a064-e2f0-4af4-8d30-9278129c9bc0", @"An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {0} registry or a problem with an external system.", Core.Constants.ProductName);

				string reason = "";
				switch (webServiceException.Reason)
				{
					case Exceptions.WebServiceException.ExceptionReason.Configuration:
						reason = Res.GetString("77b6ed97-976d-49d7-8d4e-c84d9a5cb363", "Registry Item '{0}' is not set.",
							((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl).Location);
						break;

					case Exceptions.WebServiceException.ExceptionReason.EmptyDataInResponse:
						reason = Res.GetString("546cefc2-7a4e-428b-aadc-4323a9495407", "No data was retrieved from an external system. {0}", webServiceException.Message);
						break;

					case Exceptions.WebServiceException.ExceptionReason.Exception:
						reason = Res.GetString("46e9c675-9a62-4dad-8ff3-2d3cb6ccbf3c", "Request error: {0}", webServiceException.InnerException.Message);
						break;

					case Exceptions.WebServiceException.ExceptionReason.Initialisation:
						reason = Res.GetString("43dcaa60-cf04-4f9f-a560-e6d5c67d4900", "External system can't be initialized. {0}", webServiceException.InnerException.Message);
						break;

					case Exceptions.WebServiceException.ExceptionReason.InvalidDataInResponse:
						reason = Res.GetString("9afc6344-8a38-411a-96cc-a667b37f3d71", "Invalid data was retrieved from an external system. {0}", webServiceException.Message);
						break;

					case Exceptions.WebServiceException.ExceptionReason.ResponseFail:
						reason = Res.GetString("b5647bd2-2ac6-4922-ad20-c142cc1c06ef", "External system failed to process a request: {0}", webServiceException.Message);
						break;

					case Exceptions.WebServiceException.ExceptionReason.NoDataInCache:
						reason = notInCacheErroMessage;
						break;

					case Exceptions.WebServiceException.ExceptionReason.NullResponse:
						reason = Res.GetString("95a6d2e3-484b-4d01-b42e-c26a7d12db56", "External system returned null response.");
						break;
				}

				if (!string.IsNullOrEmpty(reason))
				{
					message += System.Environment.NewLine + Res.GetString("2a6e395e-d0c5-4279-bca1-077aff0df165", @"Error message: '{0}'.", reason);
				}
			}

			return message;
		}

		#region Implementation

		T GetDetails<T>(string fieldName, string ledger, string option, params string[] additionalOptionsToCheck) where T : struct
		{
			lock (lockRoot)
			{
				Argument.NotNullOrEmpty(fieldName, "fieldName");
				Argument.NotNullOrEmpty(ledger, "ledger");
				Argument.NotNullOrEmpty(option, "option");
				Argument.NotNull(additionalOptionsToCheck, "additionalOptionsToCheck");

				var detailsFieldStrategies = GenerateDetailsFieldStrategies();

				var keyList = new List<Key>(additionalOptionsToCheck.Length + 1);
				keyList.AddRange(additionalOptionsToCheck.Select(x => new Key(ledger, x)));
				keyList.Add(new Key(ledger, option));

				if (IsConfigurationChanged(lastDetailStrategies, detailsFieldStrategies))
				{
					cachedValuesFromDb.Clear();
					cachedValuesFromWebService.Clear();
				}

				var cachedValuesFromDbContainsKey = keyList.Where(x => cachedValuesFromDb.ContainsKey(x));
				if (!cachedValuesFromDbContainsKey.Any())
				{
					var detailsFieldStrategiesForDb = detailsFieldStrategies.Where(x => !x.UseWebService).ToArray();
					GetCreditLimitAndBalanceFromSystemDb(ledger, option, detailsFieldStrategiesForDb);
				}
				var cachedValuesFromWebServiceContainsKey = keyList.Where(x => cachedValuesFromWebService.ContainsKey(x));
				if (!cachedValuesFromWebServiceContainsKey.Any())
				{
					var detailsFieldStrategiesForWebService = detailsFieldStrategies.Where(x => x.UseWebService).ToArray();
					GetCreditLimitAndBalanceFromWebService(ledger, option, detailsFieldStrategiesForWebService);
				}
				lastDetailStrategies = detailsFieldStrategies;

				var detailsFieldStrategy = GetDetailsFieldStrategy<T>(detailsFieldStrategies, fieldName);
				var cachedValues = detailsFieldStrategy.UseWebService ? cachedValuesFromWebService : cachedValuesFromDb;
				Details details = null;
				if (cachedValues.Any())
				{
					foreach (var key in keyList)
					{
						if (cachedValues.TryGetValue(key, out details))
						{
							var detailsField = detailsFieldStrategy.GetDetailsField(details);
							if (detailsField.HasValue)
							{
								return detailsField.Value;
							}
						}
					}
				}

				Exceptions.ExceptionBase relatedException;
				if (detailsFieldStrategy.UseWebService)
				{
					relatedException = LastWebServiceException ??
						new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.NoDataInCache);
				}
				else
				{
					relatedException = LastDbException ??
						new Exceptions.DbException(Exceptions.DbException.ExceptionReason.NoDataInCache);
				}
				throw new InvalidOperationException("Data was not retrieved.", relatedException); // Exception message does not need to be localized
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Ececuting stored Procedure, Have to use native Db Access to make it threadsafe, SQL Exception Message")]
		void GetCreditLimitAndBalanceFromSystemDb(string ledger, string balanceOverdueAgingOption, DetailsFieldStrategy[] detailsFieldStrategies)
		{
			if (!detailsFieldStrategies.Any())
			{
				return;
			}

			var details = new Details();
			var key = new Key(ledger, balanceOverdueAgingOption);
			try
			{
				LastDbException = null;
				string command = "OrgCreditLimitAndBalanceDetails";

				using (Db.DisposableActionForDbConnection())
				using (var sqlCommand = Db.Connection.Command(command))
				{
					sqlCommand.CommandType = CommandType.StoredProcedure;
					sqlCommand.AddParameterBasedOnDbColumn("@OrgCode", OrgCode, OrgHeaderSchema.OH_Code);
					sqlCommand.AddParameterBasedOnDbColumn("@CompanyCode", CompanyCode, GlbCompanySchema.GC_Code);
					sqlCommand.AddParameterBasedOnDbColumn("@AccLedger", ledger, AccTransactionHeaderSchema.AH_Ledger);
					sqlCommand.AddParameterBasedOnDbColumn("@AgingPeriod", 30, CargoWise.Schema.Schema.GenericIntSchemaColumn);
					sqlCommand.AddParameter("@BalanceOverdueAgingOption", SqlDbType.VarChar, 3, balanceOverdueAgingOption);

					using (var reader = sqlCommand.ExecuteReader())
					{
						if (reader.Read())
						{
							int multiplier = 1;
							Constants.BalanceOverdueAgingOptionEnum optionValue = getBalanceOverdueAgingOptionValue(balanceOverdueAgingOption);

							foreach (var strategy in detailsFieldStrategies)
							{
								strategy.SetValue(details, reader, optionValue, multiplier);
							}

							if (reader.Read())
							{
								throw new Exceptions.DbException(Exceptions.DbException.ExceptionReason.InvalidData);
							}
						}
						else
						{
							throw new Exceptions.DbException(Exceptions.DbException.ExceptionReason.EmptyData);
						}
					}
				}
			}
			catch (Exceptions.DbException ex)
			{
				LastDbException = ex;
				details = new Details();
			}
			catch (System.Data.Common.DbException ex) when (ex.Message.Contains("Arithmetic overflow error"))
			{
				LastDbException = new Exceptions.DbException(Exceptions.DbException.ExceptionReason.InvalidData, ex);
				details = new Details();
			}
			finally
			{
				cachedValuesFromDb.Add(key, details);
			}
		}

		#region TimoutRetrySuspensionControl

		static int MaxFailureCountBeforeSuspending => AccountingConfigurationRegistry.Instance.MaxTimeoutCountBeforeSuspendingWebService.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);

		[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		static int SuspensionPeriodInMinutes => AccountingConfigurationRegistry.Instance.WebServiceCallSuspendingPeriodInMinutes.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty);

		[ThreadSafe]
		static readonly ConcurrentDictionary<Guid, (int TimeoutCounter, DateTime SuspensionEndTime)> TimeoutRetrySuspensionControl = new ConcurrentDictionary<Guid, (int TimeoutCounter, DateTime SuspensionEndTime)>();

		static bool IsInRetrySuspensionPeriod()
		{
			return TimeoutRetrySuspensionControl.TryGetValue(Env.CurrentCompanyPK, out var suspensionControl) && suspensionControl.SuspensionEndTime >= ZDateTime.UtcNow.ToDateTime();
		}

		static void IncrementTimeoutCounter()
		{
			TimeoutRetrySuspensionControl.AddOrUpdate(Env.CurrentCompanyPK,
				addValueFactory: companyPK => (TimeoutCounter: 1, SuspensionEndTime: DateTime.MinValue),
				updateValueFactory: (companyPK, suspensionControl) => (TimeoutCounter: suspensionControl.TimeoutCounter + 1,
					SuspensionEndTime: (suspensionControl.TimeoutCounter >= MaxFailureCountBeforeSuspending - 1 ? ZDateTime.UtcNow.AddMinutes(SuspensionPeriodInMinutes).ToDateTime() : suspensionControl.SuspensionEndTime)));
		}

		static void ResetTimeoutCounter()
		{
			TimeoutRetrySuspensionControl.TryRemove(Env.CurrentCompanyPK, out _);
		}

#if DEBUG
		internal static void ClearTimeoutRetrySuspensionControl_ForTestOnly() => TimeoutRetrySuspensionControl.Clear();
#endif

		#endregion

		void GetCreditLimitAndBalanceFromWebService(string ledger, string balanceOverdueAgingOption, DetailsFieldStrategy[] detailsFieldStrategies)
		{
			if (!detailsFieldStrategies.Any())
			{
				return;
			}

			var details = new Details();
			var key = new Key(ledger, balanceOverdueAgingOption);
			try
			{
				LastWebServiceException = null;

				if (IsInRetrySuspensionPeriod())
				{
					throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.Exception,
						new TimeoutException(Res.GetString("3f87dbbc-bd2d-4959-9b75-aacadfbf466f", "Retrying suspended due to continuous web service timeouts.")));
				}

				var service = CreateNewServiceClient();
				using ((IDisposable)service)
				{
					var request = new CreditLimitAndBalanceRequest();
					request.OrgCode = OrgCode;
					request.CompanyCode = CompanyCode;
					request.AccLedger = ledger;
					request.OverdueAgingPeriod = 30;
					request.BalanceOverdueAgingOption = balanceOverdueAgingOption;

					CreditLimitAndBalanceResponse response;
					try
					{
						var serviceResponse = service.GetCreditLimitAndBalanceDetails(new GetCreditLimitAndBalanceDetailsRequest(GetSecuritySoapHeader(), request));
						response = serviceResponse != null ? serviceResponse.GetCreditLimitAndBalanceDetailsResult : null;
						ResetTimeoutCounter();
					}
					catch (Exception ex)
					{
						if (ex is TimeoutException)
						{
							IncrementTimeoutCounter();
						}
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.Exception, ex);
					}

					if (response == null)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.NullResponse);
					}
					else if (!response.Succeeded)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.ResponseFail, response.ErrorMessage);
					}
					else if (response.CreditLimitAndBalanceDetails == null)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.InvalidDataInResponse,
							Res.GetString("9680df7d-6bfa-4557-9153-0f6309028bdf", "(The {0} collection should contain one element but the collection was null)", "CreditLimitAndBalanceDetails"));
					}
					else if (response.CreditLimitAndBalanceDetails.Length == 0)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.EmptyDataInResponse,
							GetCreditLimitAndBalanceDetailsHasWrongNumberOfElementsMessage(0));
					}
					else if (response.CreditLimitAndBalanceDetails.Length > 1)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.InvalidDataInResponse,
							GetCreditLimitAndBalanceDetailsHasWrongNumberOfElementsMessage(response.CreditLimitAndBalanceDetails.Length));
					}
					else if (response.CreditLimitAndBalanceDetails[0] == null)
					{
						throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.EmptyDataInResponse,
							Res.GetString(
								"4a1dca54-9dd4-4600-aa9d-688586171dbc",
								"The first {0} element in the {1} collection was empty",
								nameof(CreditLimitAndBalanceInfo),
								nameof(response.CreditLimitAndBalanceDetails)));
					}

					int multiplier = ledger == LedgerTypes.AccountsPayable ? -1 : 1;
					CreditLimitAndBalanceInfo value = response.CreditLimitAndBalanceDetails[0];

					foreach (var strategy in detailsFieldStrategies)
					{
						strategy.SetValue(details, value, multiplier);
					}
				}
			}
			catch (Exceptions.WebServiceException ex)
			{
				LastWebServiceException = ex;
				details = new Details();
			}
			finally
			{
				var existingValue = new Details();
				if (cachedValuesFromWebService.ContainsKey(key))
				{
					existingValue = cachedValuesFromWebService[key];
					if (!existingValue.Equals(details))
					{
						var existingValues = string.Join(" ", existingValue.GetType()
								.GetProperties()
								.Select(prop => prop.Name + ":" + prop.GetValue(existingValue)));

						var newValues = string.Join(" ", details.GetType()
								.GetProperties()
								.Select(prop => prop.Name + ":" + prop.GetValue(details)));
						ErrorReporter.ReportOnce("OrgCreditLimitAndBalanceDetails|AnItemWithTheSameKeyHasAlreadyBeenAdded", $"An item with the same key has already been added to cachedValuesFromWebService, registry info:\n{AccountingUtils.CollectRegistryInfoForOrganizationCreditLimitCheck()}\nKey details: {key.Ledger} {key.BalanceOverdueAgingOption}\nExisting Values: {existingValues}\nNew Values: {newValues}");
					}
				}
				else
				{
					cachedValuesFromWebService.Add(key, details);
				}
			}
		}

		string GetCreditLimitAndBalanceDetailsHasWrongNumberOfElementsMessage(int numberOfElements)
		{
			return Res.GetString("481db397-a8aa-422d-adde-59b62f95daa7", "(The {0} collection should contain one element but the collection contained {1} elements)", "CreditLimitAndBalanceDetails", numberOfElements);
		}

#if DEBUG

		public CreditLimitServiceSoap CreateNewServiceClient_ForTest()
		{
			CreditLimitServiceSoap client;
			using (MockProviderCreationSuspender.GetSuspender())
			{
				client = CreateNewServiceClient();
			}
			return client;
		}

		FunctionalitySuspender MockProviderCreationSuspender => mockProviderCreationSuspender ?? (mockProviderCreationSuspender = new FunctionalitySuspender());
		FunctionalitySuspender mockProviderCreationSuspender;
#endif

		CreditLimitServiceSoap CreateNewServiceClient()
		{
			var url = AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl.Value;

			if (string.IsNullOrEmpty(url))
			{
				throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.Configuration);
			}

			CreditLimitServiceSoap result;

			try
			{
				var uri = new Uri(url);
				var securityMode = (uri.Scheme == Uri.UriSchemeHttps) ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None;
				var timeout = TimeSpan.FromSeconds(AccountingConfigurationRegistry.Instance.UseWebServiceTimeout.Value);
				var binding = new BasicHttpBinding(securityMode)
				{
					SendTimeout = timeout,
					ReceiveTimeout = timeout,
					OpenTimeout = timeout,
					CloseTimeout = timeout,
				};

				result = CreateClientProvider(binding, uri).GetClient();
			}
			catch (Exception ex)
			{
				throw new Exceptions.WebServiceException(Exceptions.WebServiceException.ExceptionReason.Initialisation, ex);
			}

			return result;
		}

		protected virtual CreditLimitServiceSoapClientProvider CreateClientProvider(Binding binding, Uri url)
		{
			CreditLimitServiceSoapClientProvider provider = null;
#if DEBUG
			if (Globals.IsTest && !MockProviderCreationSuspender.IsSuspended)
			{
				provider = new MockCreditLimitServiceClientProvider(binding);
			}
			else
			{
#endif
				provider = new CreditLimitServiceSoapClientProvider(binding, url);
#if DEBUG
			}
#endif
			return provider;
		}

		static SecuritySOAPHeader GetSecuritySoapHeader()
		{
			var result = new SecuritySOAPHeader();
			result.UserName = AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUserName.Value;
			result.Password = AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServicePassword.Value;
			return result;
		}

		Constants.BalanceOverdueAgingOptionEnum getBalanceOverdueAgingOptionValue(string balanceOverdueAgingOptionValue)
		{
			if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Balance)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Balance;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue)
			{
				return Constants.BalanceOverdueAgingOptionEnum.RecognisedAndUnrecognisedRevenue;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Overdue)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Overdue;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Aging)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Aging;
			}
			return Constants.BalanceOverdueAgingOptionEnum.Unknown;
		}

		Exceptions.DbException LastDbException { get; set; }

		Exceptions.WebServiceException LastWebServiceException { get; set; }

		readonly Dictionary<Key, Details> cachedValuesFromDb = new Dictionary<Key, Details>();
		readonly Dictionary<Key, Details> cachedValuesFromWebService = new Dictionary<Key, Details>();

#if DEBUG
		internal Exceptions.WebServiceException GetLastWebServiceException_ForTestOnly() => LastWebServiceException;

		internal void ClearCachedValues_ForTestOnly()
		{
			cachedValuesFromDb.Clear();
			cachedValuesFromWebService.Clear();
		}
#endif

		DetailsFieldStrategy[] lastDetailStrategies;

		class Details : IEquatable<Details>
		{
			public bool? OnCreditHold { get; set; }
			public bool? IsOverCreditLimit { get; set; }
			public bool? IsOverCreditTerms { get; set; }
			public decimal? CreditLimit { get; set; }

			public decimal? OutstandingBalance { get; set; }
			public decimal? Claim { get; set; }
			public decimal? OutstandingBalanceOverdue { get; set; }
			public decimal? OutstandingBalanceNotOverdue { get; set; }

			public decimal? UnpostedRevenueRecognised { get; set; }
			public decimal? UnpostedRevenueUnrecognised { get; set; }
			public decimal? UnpostedRevenue { get; set; }

			public bool? IsGlobalCreditApproved { get; set; }
			public bool? OnGlobalCreditHold { get; set; }
			public bool? IsOverGlobalCreditLimit { get; set; }
			public string GlobalCreditCurrencyCode { get; set; }
			public string GlobalCreditGroupOrgCode { get; set; }
			public decimal? GlobalCreditLimit { get; set; }

			public decimal? GlobalOutstandingBalance { get; set; }
			public decimal? GlobalClaim { get; set; }
			public bool? UnableToCalculateGlobalOutstandingBalance { get; set; }
			public decimal? GlobalUnpostedRevenueRecognised { get; set; }
			public decimal? GlobalUnpostedRevenueUnrecognised { get; set; }
			public bool? UnableToCalculateGlobalUnpostedRevenue { get; set; }

			public DateTime TimeStamp { get; set; }

			public bool Equals(Details details)
				=> details != null
				&& OnCreditHold == details.OnCreditHold
				&& IsOverCreditLimit == details.IsOverCreditLimit
				&& IsOverCreditTerms == details.IsOverCreditTerms
				&& CreditLimit == details.CreditLimit
				&& OutstandingBalance == details.OutstandingBalance
				&& Claim == details.Claim
				&& OutstandingBalanceOverdue == details.OutstandingBalanceOverdue
				&& OutstandingBalanceNotOverdue == details.OutstandingBalanceNotOverdue
				&& UnpostedRevenueRecognised == details.UnpostedRevenueRecognised
				&& UnpostedRevenueUnrecognised == details.UnpostedRevenueUnrecognised
				&& UnpostedRevenue == details.UnpostedRevenue
				&& IsGlobalCreditApproved == details.IsGlobalCreditApproved
				&& OnGlobalCreditHold == details.OnGlobalCreditHold
				&& IsOverGlobalCreditLimit == details.IsOverGlobalCreditLimit
				&& GlobalCreditCurrencyCode == details.GlobalCreditCurrencyCode
				&& GlobalCreditGroupOrgCode == details.GlobalCreditGroupOrgCode
				&& GlobalCreditLimit == details.GlobalCreditLimit
				&& GlobalOutstandingBalance == details.GlobalOutstandingBalance
				&& GlobalClaim == details.GlobalClaim
				&& UnableToCalculateGlobalOutstandingBalance == details.UnableToCalculateGlobalOutstandingBalance
				&& GlobalUnpostedRevenueRecognised == details.GlobalUnpostedRevenueRecognised
				&& GlobalUnpostedRevenueUnrecognised == details.GlobalUnpostedRevenueUnrecognised
				&& UnableToCalculateGlobalUnpostedRevenue == details.UnableToCalculateGlobalUnpostedRevenue
				&& TimeStamp == details.TimeStamp;
		}

		class Key
		{
			public Key(string ledger, string balanceOverdueAgingOption)
			{
				Ledger = ledger;
				BalanceOverdueAgingOption = balanceOverdueAgingOption;
			}

			public readonly string Ledger;
			public readonly string BalanceOverdueAgingOption;

			public override bool Equals(object obj)
			{
				var key = obj as Key ?? throw new ArgumentNullException(nameof(obj), "obj must be a Key");

				return this.Ledger == key.Ledger && this.BalanceOverdueAgingOption == key.BalanceOverdueAgingOption;
			}

			public override int GetHashCode()
			{
				return Ledger.GetHashCode() ^ BalanceOverdueAgingOption.GetHashCode();
			}
		}

		bool IsConfigurationChanged(DetailsFieldStrategy[] detailStrategies1, DetailsFieldStrategy[] detailStrategies2)
		{
			if (detailStrategies1 == null || detailStrategies2 == null || detailStrategies1.Length != detailStrategies2.Length)
			{
				return true;
			}

			var difference = detailStrategies1.Except(detailStrategies2, new DetailsFieldStrategyComparer());
			if (difference.Any())
			{
				return true;
			}

			difference = detailStrategies2.Except(detailStrategies1, new DetailsFieldStrategyComparer());
			if (difference.Any())
			{
				return true;
			}

			return false;
		}

		DetailsFieldStrategy[] GenerateDetailsFieldStrategies()
		{
			var company = GlbCompany.CurrentCompany.GC_Code;
			var webReg1 = AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value ? "T" : "F";
			var webReg2 = AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.Value ? "T" : "F";
			var webReg3 = AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.Value ? "T" : "F";

			var cacheKey = company + "_" + webReg1 + webReg2 + webReg3;

			return DetailsFieldStrategyCacheDictionary.GetOrAdd(cacheKey, () => GenerateDetailsFieldStrategiesCore());
		}

#if DEBUG
		internal bool IsInDetailsFieldStrategyCacheDictionary_ForTestOnly(string key)
		{
			return DetailsFieldStrategyCacheDictionary.Any(x => x.Key == key);
		}

		internal void ClearDetailsFieldStrategyCacheDictionary_ForTestOnly()
		{
			DetailsFieldStrategyCacheDictionary.Clear();
		}
#endif

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, DetailsFieldStrategy[]> DetailsFieldStrategyCacheDictionary = new ConcurrentDictionary<string, DetailsFieldStrategy[]>();

		DetailsFieldStrategy[] GenerateDetailsFieldStrategiesCore()
		{
			var result = new List<DetailsFieldStrategy>();

			result.AddRange(GenerateCreditLimitDetailsFieldStrategies());
			result.AddRange(GenerateGlobalCreditLimitDetailsFieldStrategies());

			result.AddRange(GenerateOutstandingBalanceFieldStrategies());
			result.AddRange(GenerateGlobalOutstandingBalanceFieldStrategies());

			result.AddRange(GenerateUnpostedRevenueFieldStrategies());
			result.AddRange(GenerateGlobalUnpostedRevenueFieldStrategies());

			return result.ToArray();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		IEnumerable<DetailsFieldStrategy> GenerateCreditLimitDetailsFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit;

			result.Add(new DetailsFieldStrategy<bool>("OnCreditHold", useWebServiceRegistryItem, details => details.OnCreditHold,
				(details, dbRow, optionValue, multiplier) => details.OnCreditHold = dbRow["OnCreditHold"].ToString().Trim().ToUpper(CultureInfo.InvariantCulture) == "1",
				(details, response, multiplier) => details.OnCreditHold = response.OnCreditHoldHasValue ? response.OnCreditHold : null));

			result.Add(new DetailsFieldStrategy<bool>("IsOverCreditLimit", useWebServiceRegistryItem, details => details.IsOverCreditLimit,
				(details, dbRow, optionValue, multiplier) => details.IsOverCreditLimit = dbRow["IsOverCreditLimit"].ToString().Trim().ToUpper(CultureInfo.InvariantCulture) == "Y",
				(details, response, multiplier) => details.IsOverCreditLimit = response.IsOverCreditLimitHasValue ? response.IsOverCreditLimit : null));

			result.Add(new DetailsFieldStrategy<bool>("IsOverCreditTerms", useWebServiceRegistryItem, details => details.IsOverCreditTerms,
				(details, dbRow, optionValue, multiplier) => details.IsOverCreditTerms = optionValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue ?
						dbRow["IsOverCreditTerms"].ToString().Trim().ToUpper(CultureInfo.InvariantCulture) == "Y" : null,
				(details, response, multiplier) => details.IsOverCreditTerms = response.IsOverCreditTermsHasValue ? response.IsOverCreditTerms : null));

			result.Add(new DetailsFieldStrategy<decimal>("CreditLimit", useWebServiceRegistryItem, details => details.CreditLimit,
				(details, dbRow, optionValue, multiplier) => details.CreditLimit = !dbRow.IsDBNull(dbRow.GetOrdinal("CreditLimit")) ? (decimal)dbRow["CreditLimit"] * multiplier : null,
				(details, response, multiplier) => details.CreditLimit = response.CreditLimitHasValue ? response.CreditLimit * multiplier : null));

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		IEnumerable<DetailsFieldStrategy> GenerateGlobalCreditLimitDetailsFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit;

			result.Add(new DetailsFieldStrategy<bool>("IsGlobalCreditApproved", useWebServiceRegistryItem, details => details.IsGlobalCreditApproved,
				(details, dbRow, optionValue, multiplier) => details.IsGlobalCreditApproved = !dbRow.IsDBNull(dbRow.GetOrdinal("IsGlobalCreditApproved")) ?
					(bool)dbRow["IsGlobalCreditApproved"] : null,
				(details, response, multiplier) => details.IsGlobalCreditApproved = response.IsGlobalCreditApprovedHasValue && response.IsGlobalCreditApproved));

			result.Add(new DetailsFieldStrategy<bool>("OnGlobalCreditHold", useWebServiceRegistryItem, details => details.OnGlobalCreditHold,
				(details, dbRow, optionValue, multiplier) => details.OnGlobalCreditHold = !dbRow.IsDBNull(dbRow.GetOrdinal("OnGlobalCreditHold")) ?
					(bool)dbRow["OnGlobalCreditHold"] : null,
				(details, response, multiplier) => details.OnGlobalCreditHold = response.OnGlobalCreditHoldHasValue ? response.OnGlobalCreditHold : null));

			result.Add(new DetailsFieldStrategy<bool>("IsOverGlobalCreditLimit", useWebServiceRegistryItem, details => details.IsOverGlobalCreditLimit,
				(details, dbRow, optionValue, multiplier) => details.IsOverGlobalCreditLimit = !dbRow.IsDBNull(dbRow.GetOrdinal("IsOverGlobalCreditLimit")) ?
					(bool)dbRow["IsOverGlobalCreditLimit"] : null,
				(details, response, multiplier) => details.IsOverGlobalCreditLimit = response.IsOverGlobalCreditLimitHasValue ? response.IsOverGlobalCreditLimit : null));

			result.Add(new DetailsFieldStrategy<ZString>("GlobalCreditCurrencyCode", useWebServiceRegistryItem, details => details.GlobalCreditCurrencyCode,
				(details, dbRow, optionValue, multiplier) => details.GlobalCreditCurrencyCode = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalCreditCurrencyCode")) ?
					dbRow["GlobalCreditCurrencyCode"].ToString().Trim().ToUpper(CultureInfo.InvariantCulture) : null,
				(details, response, multiplier) => details.GlobalCreditCurrencyCode = !string.IsNullOrEmpty(response.GlobalCreditCurrencyCode) ? response.GlobalCreditCurrencyCode : null));

			result.Add(new DetailsFieldStrategy<ZString>("GlobalCreditGroupOrgCode", useWebServiceRegistryItem, details => details.GlobalCreditGroupOrgCode,
				(details, dbRow, optionValue, multiplier) => details.GlobalCreditGroupOrgCode = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalCreditGroupOrgCode")) ?
					dbRow["GlobalCreditGroupOrgCode"].ToString().Trim().ToUpper(CultureInfo.InvariantCulture) : null,
				(details, response, multiplier) => details.GlobalCreditGroupOrgCode = !string.IsNullOrEmpty(response.GlobalCreditGroupOrgCode) ? response.GlobalCreditGroupOrgCode : null));

			result.Add(new DetailsFieldStrategy<decimal>("GlobalCreditLimit", useWebServiceRegistryItem, details => details.GlobalCreditLimit,
				(details, dbRow, optionValue, multiplier) => details.GlobalCreditLimit = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalCreditLimit")) ? (decimal)dbRow["GlobalCreditLimit"] * multiplier : null,
				(details, response, multiplier) => details.GlobalCreditLimit = response.GlobalCreditLimitHasValue ? response.GlobalCreditLimit * multiplier : null));

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "DB Column")]
		IEnumerable<DetailsFieldStrategy> GenerateOutstandingBalanceFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance;

			result.Add(new DetailsFieldStrategy<decimal>("OutstandingBalance", useWebServiceRegistryItem, details => details.OutstandingBalance,
				(details, dbRow, optionValue, multiplier) => details.OutstandingBalance = Utilities.Round((decimal)dbRow["AccountBalanceTotal"], localCurrencyDecimals),
				(details, response, multiplier) => details.OutstandingBalance = response.AccountBalanceTotalHasValue ?
						Utilities.Round(response.AccountBalanceTotal, localCurrencyDecimals) : null));

			result.Add(new DetailsFieldStrategy<decimal>("Claim", useWebServiceRegistryItem, details => details.Claim,
				(details, dbRow, optionValue, multiplier) => details.Claim = Utilities.Round((decimal)dbRow["ClaimTotal"], localCurrencyDecimals),
				(details, response, multiplier) => details.Claim = response.ClaimTotalHasValue ?
						Utilities.Round(response.ClaimTotal, localCurrencyDecimals) : 0M));

			result.Add(new DetailsFieldStrategy<decimal>("OutstandingBalanceOverdue", useWebServiceRegistryItem, details => details.OutstandingBalanceOverdue,
				(details, dbRow, optionValue, multiplier) => details.OutstandingBalanceOverdue = optionValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue ?
					Utilities.Round((decimal)dbRow["OverdueTotal"] * multiplier, localCurrencyDecimals) : null,
				(details, response, multiplier) => details.OutstandingBalanceOverdue = response.OverdueTotalHasValue ?
					Utilities.Round(response.OverdueTotal * multiplier, localCurrencyDecimals) : null));

			result.Add(new DetailsFieldStrategy<decimal>("OutstandingBalanceNotOverdue", useWebServiceRegistryItem, details => details.OutstandingBalanceNotOverdue,
				(details, dbRow, optionValue, multiplier) => details.OutstandingBalanceNotOverdue = optionValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue ?
					Utilities.Round((decimal)dbRow["AccountBalanceNotOverdue"] * multiplier, localCurrencyDecimals) : null,
				(details, response, multiplier) => details.OutstandingBalanceNotOverdue = response.AccountBalanceNotOverdueHasValue ?
					Utilities.Round(response.AccountBalanceNotOverdue * multiplier, localCurrencyDecimals) : null));

			return result;
		}

		IEnumerable<DetailsFieldStrategy> GenerateGlobalOutstandingBalanceFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance;

			result.Add(new DetailsFieldStrategy<decimal>("GlobalOutstandingBalance", useWebServiceRegistryItem, details => details.GlobalOutstandingBalance,
				(details, dbRow, optionValue, multiplier) => details.GlobalOutstandingBalance = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalOutstandingBalance")) ?
					(decimal)dbRow["GlobalOutstandingBalance"] * multiplier : null,
				(details, response, multiplier) => details.GlobalOutstandingBalance = response.GlobalBalanceTotalHasValue ?
					response.GlobalBalanceTotal * multiplier : null));

			result.Add(new DetailsFieldStrategy<decimal>("GlobalClaim", useWebServiceRegistryItem, details => details.GlobalClaim,
				(details, dbRow, optionValue, multiplier) => details.GlobalClaim = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalClaim")) ?
					(decimal)dbRow["GlobalClaim"] * multiplier : null,
				(details, response, multiplier) => details.GlobalClaim = response.GlobalClaimTotalHasValue ?
					response.GlobalClaimTotal * multiplier : null));

			result.Add(new DetailsFieldStrategy<bool>("UnableToCalculateGlobalOutstandingBalance", useWebServiceRegistryItem, details => details.UnableToCalculateGlobalOutstandingBalance,
				(details, dbRow, optionValue, multiplier) => details.UnableToCalculateGlobalOutstandingBalance = !dbRow.IsDBNull(dbRow.GetOrdinal("InvalidGlobalCreditCurrencyOrMissingExRate")) ?
					(bool)dbRow["InvalidGlobalCreditCurrencyOrMissingExRate"] : null,
				(details, response, multiplier) => details.UnableToCalculateGlobalOutstandingBalance = response.InvalidGlobalCreditCurrencyOrMissingExRateHasValue ? response.InvalidGlobalCreditCurrencyOrMissingExRate : null));

			return result;
		}

		IEnumerable<DetailsFieldStrategy> GenerateUnpostedRevenueFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue;

			result.Add(new DetailsFieldStrategy<decimal>("UnpostedRevenueRecognised", useWebServiceRegistryItem, details => details.UnpostedRevenueRecognised,
				(details, dbRow, optionValue, multiplier) => details.UnpostedRevenueRecognised = optionValue >= Constants.BalanceOverdueAgingOptionEnum.RecognisedAndUnrecognisedRevenue ?
					Utilities.Round((decimal)dbRow["UnpostedRevenueRecognisedTotal"], localCurrencyDecimals) : null,
				(details, response, multiplier) => details.UnpostedRevenueRecognised = response.UnpostedRevenueRecognisedTotalHasValue ?
					Utilities.Round(response.UnpostedRevenueRecognisedTotal, localCurrencyDecimals) : null));

			result.Add(new DetailsFieldStrategy<decimal>("UnpostedRevenueUnrecognised", useWebServiceRegistryItem, details => details.UnpostedRevenueUnrecognised,
				(details, dbRow, optionValue, multiplier) => details.UnpostedRevenueUnrecognised = optionValue >= Constants.BalanceOverdueAgingOptionEnum.RecognisedAndUnrecognisedRevenue ?
					Utilities.Round((decimal)dbRow["UnpostedRevenueUnrecognisedTotal"], localCurrencyDecimals) : null,
				(details, response, multiplier) => details.UnpostedRevenueUnrecognised = response.UnpostedRevenueUnrecognisedTotalHasValue ?
					Utilities.Round(response.UnpostedRevenueUnrecognisedTotal, localCurrencyDecimals) : null));

			return result;
		}

		IEnumerable<DetailsFieldStrategy> GenerateGlobalUnpostedRevenueFieldStrategies()
		{
			var result = new List<DetailsFieldStrategy>();
			var useWebServiceRegistryItem = AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue;

			result.Add(new DetailsFieldStrategy<decimal>("GlobalUnpostedRevenueRecognised", useWebServiceRegistryItem, details => details.GlobalUnpostedRevenueRecognised,
				(details, dbRow, optionValue, multiplier) => details.GlobalUnpostedRevenueRecognised = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalUnpostedRevenueRecognised")) ?
					(decimal)dbRow["GlobalUnpostedRevenueRecognised"] : null,
				(details, response, multiplier) => details.GlobalUnpostedRevenueRecognised = response.GlobalUnpostedRevenueRecognisedTotalHasValue ?
					response.GlobalUnpostedRevenueRecognisedTotal : null));

			result.Add(new DetailsFieldStrategy<decimal>("GlobalUnpostedRevenueUnrecognised", useWebServiceRegistryItem, details => details.GlobalUnpostedRevenueUnrecognised,
				(details, dbRow, optionValue, multiplier) => details.GlobalUnpostedRevenueUnrecognised = !dbRow.IsDBNull(dbRow.GetOrdinal("GlobalUnpostedRevenueUnrecognised")) ?
					(decimal)dbRow["GlobalUnpostedRevenueUnrecognised"] : null,
				(details, response, multiplier) => details.GlobalUnpostedRevenueUnrecognised = response.GlobalUnpostedRevenueUnrecognisedTotalHasValue ?
					response.GlobalUnpostedRevenueUnrecognisedTotal : null));

			result.Add(new DetailsFieldStrategy<bool>("UnableToCalculateGlobalUnpostedRevenue", useWebServiceRegistryItem, details => details.UnableToCalculateGlobalUnpostedRevenue,
				(details, dbRow, optionValue, multiplier) => details.UnableToCalculateGlobalUnpostedRevenue = !dbRow.IsDBNull(dbRow.GetOrdinal("InvalidGlobalCreditCurrencyOrMissingExRate")) ?
					(bool)dbRow["InvalidGlobalCreditCurrencyOrMissingExRate"] : null,
				(details, response, multiplier) => details.UnableToCalculateGlobalUnpostedRevenue = response.InvalidGlobalCreditCurrencyOrMissingExRateHasValue ? response.InvalidGlobalCreditCurrencyOrMissingExRate : null));

			return result;
		}

		DetailsFieldStrategy<T> GetDetailsFieldStrategy<T>(DetailsFieldStrategy[] detailsFieldStrategies, string fieldName) where T : struct
		{
			return (DetailsFieldStrategy<T>)detailsFieldStrategies.Where(x => x.Name == fieldName).First();
		}

		class DetailsFieldStrategy
		{
			public DetailsFieldStrategy(string fieldName, BooleanRegistryItem useWebServiceRegistryItem,
				Action<Details, IDataReader, Constants.BalanceOverdueAgingOptionEnum, int> setFieldValueFromDb,
				Action<Details, CreditLimitAndBalanceInfo, int> setFieldValueFromWebService)
			{
				this.name = fieldName;
				this.useWebService = useWebServiceRegistryItem.Value;
				this.setFieldValueFromDb = setFieldValueFromDb;
				this.setFieldValueFromWebService = setFieldValueFromWebService;
			}

			public string Name
			{
				get { return name; }
			}

			public bool UseWebService
			{
				get { return useWebService; }
			}

			public void SetValue(Details details, IDataReader reader, Constants.BalanceOverdueAgingOptionEnum optionValue, int multiplier)
			{
				setFieldValueFromDb(details, reader, optionValue, multiplier);
			}

			public void SetValue(Details details, CreditLimitAndBalanceInfo value, int multiplier)
			{
				setFieldValueFromWebService(details, value, multiplier);
			}

			readonly string name;
			readonly bool useWebService;

			readonly Action<Details, IDataReader, Constants.BalanceOverdueAgingOptionEnum, int> setFieldValueFromDb;
			readonly Action<Details, CreditLimitAndBalanceInfo, int> setFieldValueFromWebService;
		}

		class DetailsFieldStrategy<T> : DetailsFieldStrategy where T : struct
		{
			public DetailsFieldStrategy(string fieldName, BooleanRegistryItem useWebServiceRegistryItem, Func<Details, T?> getDetailsField,
				Action<Details, IDataReader, Constants.BalanceOverdueAgingOptionEnum, int> setFieldValueFromDb,
				Action<Details, CreditLimitAndBalanceInfo, int> setFieldValueFromWebService)
				: base(fieldName, useWebServiceRegistryItem, setFieldValueFromDb, setFieldValueFromWebService)
			{
				this.getDetailsField = getDetailsField;
			}

			public Func<Details, T?> GetDetailsField
			{
				get { return getDetailsField; }
			}

			readonly Func<Details, T?> getDetailsField;
		}

		class DetailsFieldStrategyComparer : IEqualityComparer<DetailsFieldStrategy>
		{
			#region IEqualityComparer<DetailsFieldStrategy> Members

			public bool Equals(DetailsFieldStrategy x, DetailsFieldStrategy y)
			{
				if (Object.ReferenceEquals(x, y))
				{
					return true;
				}

				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				{
					return false;
				}

				return x.Name == y.Name && x.UseWebService == y.UseWebService;
			}

			public int GetHashCode(DetailsFieldStrategy obj)
			{
				if (Object.ReferenceEquals(obj, null))
				{
					return 0;
				}

				return obj.Name.GetHashCode() ^ obj.UseWebService.GetHashCode();
			}

			#endregion
		}

		#endregion
	}

	public class OrgCreditLimitAndBalanceDetailsProvider : IOrgCreditLimitAndBalanceDetailsProvider
	{
		IOrgCreditLimitAndBalanceDetails IOrgCreditLimitAndBalanceDetailsProvider.GetOrgCreditLimitAndBalanceDetails(string orgCode)
		{
			return new OrgCreditLimitAndBalanceDetails(orgCode);
		}
	}

	public class CreditLimitServiceSoapClientProvider
	{
		public CreditLimitServiceSoapClientProvider(Binding binding, Uri url)
		{
			channelBinding = binding;
			serviceUrl = url;
		}

		readonly Binding channelBinding;
		readonly Uri serviceUrl;

		public virtual CreditLimitServiceSoap GetClient()
		{
			var result = new CreditLimitServiceSoapClient(channelBinding, new EndpointAddress(serviceUrl));
			result.InnerChannel.OperationTimeout = channelBinding.SendTimeout;
			return result;
		}
	}
}
