using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using WTG.Foundation.Http;
using static Enterprise.Core.Constants;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax
{
	public class AvalaraUSSalesTaxCalculator : IUSSalesTaxCalculator
	{
		public AvalaraUSSalesTaxCalculator()
		{
			factory = new BusinessObjectFactory() { NameForDebugging = nameof(AvalaraUSSalesTaxCalculator) };
			HttpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			ObjectId = Guid.NewGuid().ToString("D", System.Globalization.CultureInfo.InvariantCulture);
			Tracer = ObjectFactory.Get<ITracer>();
		}

		#region IUSSalesTaxCalculator Members

		public string Name => "Avalara Web Service";

		public bool IsEnabled(GlbBranch branch)
			=> GetConfiguration(branch) != ConfigurationStatus.Off;

		public ConfigurationStatus GetConfiguration(GlbBranch branch)
		{
			var registryValue = EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus
									.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			switch (registryValue.ToUpperInvariant())
			{
				case AvalaraConstants.IntegrationStatus.Codes.Sandbox:
					return ConfigurationStatus.Sandbox;
				case AvalaraConstants.IntegrationStatus.Codes.Production:
					return ConfigurationStatus.Production;
				default:
					return ConfigurationStatus.Off;
			}
		}

		public (ZGuid pk, ZString code) GetChargeCode(GlbBranch branch)
		{
			var chargeCodePk = EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
									.GetValueWithoutFallback(branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var chargeCode = factory.Load<AccChargeCode>(chargeCodePk);
			return chargeCode == null
					? (ZGuid.Empty, ZString.Empty)
					: (chargeCode.PK, chargeCode.AC_Code);
		}

		public bool ShouldShowMenuItemsOnInvoiceForm(InvoicingBase transaction)
			=> TransactionShouldBeProcesssedByLedgerAndType(transaction);

		public MultilingualString CalculateMenuItemText => null;

		public MultilingualString SubmitMenuItemText
			=> ResString.GetMultilingualString("56a146c9-2d8e-464b-95fb-56822aeb5593", "Submit Transaction to Avalara as Committed");

		public SecurityCheckpoint CheckpointForCalculationMenuItem(InvoicingBase transaction)
			=> transaction == null                                    ? new DeniedSecurityCheckpoint()
			: transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? EDISecurityCheckpoints.AvalaraUSSalesTaxReceivablesRequestSalesTaxCalculation
			: new DeniedSecurityCheckpoint();

		public SecurityCheckpoint CheckpointForSubmitMenuItem(InvoicingBase transaction)
			=> transaction == null                                    ? new DeniedSecurityCheckpoint()
			: transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? EDISecurityCheckpoints.AvalaraUSSalesTaxReceivablesResubmitSalesTaxTransaction
			: new DeniedSecurityCheckpoint();

		public MultilingualString MenuItemTroubleshootingHint
			=> ResString.GetMultilingualString("c5efc10b-10a8-44c0-9dda-1a0b48c34bf9", "Further diagnostic information can be found in Help > Diagnostics > Trace Monitor > Accounting > HTTP Communication.");

		public decimal GetCurrentSalesTaxAmount(InvoicingBase transaction)
		{
			if (transaction == null)
			{
				return 0m;
			}

			var chargeCodePk = EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
									.GetValueWithoutFallback(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (chargeCodePk == ZGuid.Empty)
			{
				return 0m;
			}

			var sumOfSalesTaxChargeCodeLines =
				transaction.Lines.Cast<AccTransactionLines>()
					.Where(l => l.AL_AC == chargeCodePk)
					.Sum(l => l.AL_OSAmount);
			return sumOfSalesTaxChargeCodeLines;
		}

		public (CalculationResult result, Exception error) CalculateSalesTax(InvoicingBase transaction)
			=> DoWebRequestAndProcessResponse(transaction, MappingType.Calculate);

		public (CalculationResult result, Exception error) SubmitSalesTax(InvoicingBase transaction)
			=> DoWebRequestAndProcessResponse(transaction, MappingType.Submit);

		public bool ShouldSetSalesTaxOnPost(InvoicingBase transaction)
			=> TransactionShouldBeProcesssedByLedgerAndType(transaction)
			&& EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode.GetValueWithoutFallback(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty) != ZGuid.Empty
			&& GetMainUSOfficeAddress(transaction) != null;

		public void SetSalesTaxLineItem(InvoicingBase transaction, decimal amount)
		{
			if (!ShouldSetSalesTaxOnPost(transaction))
			{
				throw new ArgumentException("ShouldSetSalesTaxOnPost() must return true, please check that before calling SetSalesTaxLineItem()");
			}

			var chargeCodePk = EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode.GetValueWithoutFallback(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var salesTaxLines = transaction.Lines.Cast<InvoicingLineBase>().Where(l => l.AL_AC == chargeCodePk).ToArray();

			var amountMultiplier = ((ITransactionHeader)transaction).Multiplier;
			if (amount == 0m)
			{
				foreach (var l in salesTaxLines)
				{
					transaction.Lines.RemoveAndDelete(l);
				}
			}
			else if (salesTaxLines.Length == 1)
			{
				var line = salesTaxLines[0];
				line.AL_OSExTaxAmount = amount * amountMultiplier;
				SetSalesTaxPostAndReverseDate(line);
			}
			else
			{
				var currentSalesTaxAmount = GetCurrentSalesTaxAmount(transaction);
				var difference = amount - currentSalesTaxAmount;
				difference = difference * amountMultiplier;

				var line = (InvoicingLineBase)transaction.Lines.AddNew();
				line.AL_LineType = TransactionLineTypes.Revenue;
				line.GenericCharge = chargeCodePk;
				line.AL_OSExTaxAmount = difference;
				SetSalesTaxPostAndReverseDate(line);
			}

			void SetSalesTaxPostAndReverseDate(InvoicingLineBase l)
			{
				// Sales Tax is calculated after PostDate is set in OnSaving(), so must be set manually.
				// This assumes immediate revenue recognition.
				using (l.GetValidationSuspender())
				{
					if (l.AL_PostDate.IsEmpty)
					{
						l.AL_PostDate = transaction.AH_PostDate;
					}
					if (l.AL_ReverseDate.IsEmpty)
					{
						l.AL_ReverseDate = l.AL_PostDate;
					}
				}
			}
		}

		public string GetTransactionDetailsForIssueManager(InvoicingBase transaction) =>
$@"Number: {transaction.AH_TransactionNum}
Debtor: {transaction.Header?.OH_Code}
Invoice Date: {transaction.AH_InvoiceDate}
Transaction Type: {transaction.AH_TransactionType}
Total OS Amount: {transaction.AH_OSExTaxAmount:N2}
Count of Non-Zero Lines: {transaction.Lines.OfType<InvoicingLineBase>().Count(l => l.AL_OSAmount != 0m):N0}";

		#endregion

		#region Other Public Members

		public Uri GetBaseRestUrl(GlbBranch branch)
		{
			Argument.NotNull(branch, nameof(branch));

			var config = GetConfiguration(branch);
			switch (config)
			{
				case ConfigurationStatus.Off:
					return null;
				case ConfigurationStatus.Sandbox:
					return new Uri(AvalaraConstants.SandboxBaseUrl);
				case ConfigurationStatus.Production:
					return new Uri(AvalaraConstants.ProductionBaseUrl);
				default:
					throw new InvalidOperationException("Unexpected configuration value: " + config);
			}
		}

		public HttpClient GetOrCreateHttpClient(GlbBranch branch)
		{
			Argument.NotNull(branch, nameof(branch));

			var config = GetConfiguration(branch);
			switch (config)
			{
				case ConfigurationStatus.Off:
					return null;
				case ConfigurationStatus.Sandbox:
					return HttpClientForSandbox ?? (HttpClientForSandbox = CreateHttpClient(branch));
				case ConfigurationStatus.Production:
					return HttpClientForProduction ?? (HttpClientForProduction = CreateHttpClient(branch));
				default:
					throw new InvalidOperationException("Unexpected configuration value: " + config);
			}
		}

		public Exception GetValidationError(InvoicingBase transaction, MappingType type)
		{
			Argument.NotNull(transaction, nameof(transaction));

			if (!IsEnabled(transaction.Branch))
			{
				return new ApplicationException($"Avalara integration for branch/company {transaction.Branch.GB_Code}/{transaction.Company.GC_Code} is not enabled. Please see registry {EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus.HumanReadableRegistryPath()}.");
			}

			if (!transaction.IsInDatabase && type == MappingType.Submit)
			{
				return new InvalidOperationException("Transaction must be posted before US Sales Tax can be committed.");
			}

			if (!transaction.AH_OH.IsValid)
			{
				return new System.IO.InvalidDataException("US Sales Tax cannot be calculated without a valid Debtor.");
			}

			var mainUSOfficeAddress = GetMainUSOfficeAddress(transaction);
			if (mainUSOfficeAddress == null)
			{
				return new System.IO.InvalidDataException("US Sales Tax can only be calculated for Debtors with an active main office address in the US.");
			}

			return null;
		}

		public AvalaraV2CreateTransactionRequestModel MapTransactionToObject(InvoicingBase transaction, MappingType type)
		{
			Argument.NotNull(transaction, nameof(transaction));
			Argument.NotNull(transaction.Branch, "transaction.Branch");

			var address = GetMainUSOfficeAddress(transaction);

			var customerCodeFromRegistry = EDIDataRegistry.Instance.AvalaraCompanyCode
											.GetValueWithoutFallback(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var salesTaxChargeCodeFromRegistry = EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode
											.GetValueWithoutFallback(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

			var model = new AvalaraV2CreateTransactionRequestModel();
			model.Type = GetMessageTypeForTransaction(transaction, type);
			model.CompanyCode = customerCodeFromRegistry;
			model.Date = transaction.AH_InvoiceDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
			if (type == MappingType.Submit)
			{
				model.Code = transaction.Company.GC_Code + transaction.AH_TransactionNum;
			}
			model.CustomerCode = transaction.Header?.OH_Code.ToNullIfEmpty();
			model.Commit = type == MappingType.Submit;
			model.CurrencyCode = transaction.AH_RX_NKTransactionCurrency.ToNullIfEmpty();
			model.Description = transaction.AH_Desc.ToNullIfEmpty();

			if (transaction.AH_TransactionType == TransactionTypes.CreditNote
				&& transaction.AH_OriginalInvoiceDate.IsValid)
			{
				var reason = transaction.IsReversalTransaction ? FormattableString.Invariant($"Reversal of Original Invoice Transaction: {transaction.AH_OriginalTransactionNum}")
							: transaction.OriginalTransactionIsSet ? FormattableString.Invariant($"Credit Note with Original Invoice Transaction: {transaction.AH_OriginalTransactionNum}")
							: FormattableString.Invariant($"Credit Note with Manual Original Invoice Date: {transaction.AH_OriginalTransactionNum}");

				model.TaxOverride = new AvalaraV2TaxOverrideModel()
				{
					Type = AvalaraV2TaxOverrideType.TaxDate,
					TaxDate = transaction.AH_OriginalInvoiceDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
					Reason = reason.Trim(),
				};
			}

			if (address != null)
			{
				model.Addresses.Add(AvalaraV2AddressType.shipTo,
					new AvalaraV2AddressLocationModel()
					{
						Line1 = address.OA_Address1.ToNullIfEmpty(),
						Line2 = address.OA_Address2.ToNullIfEmpty(),
						City = address.OA_City.ToNullIfEmpty(),
						Region = address.OA_State.ToNullIfEmpty(),
						Country = address.OA_RN_NKCountryCode.ToNullIfEmpty(),
						PostalCode = address.OA_PostCode.ToNullIfEmpty(),
						Longitude = address.Longitude.ToNullIfEmpty(),
						Latitude = address.Latitude.ToNullIfEmpty(),
					});
			}
			model.Addresses.Add(AvalaraV2AddressType.shipFrom,
				new AvalaraV2AddressLocationModel()
				{
					LocationCode = "DEFAULT"
				});

			var hostedCode = GetHostedCodeFromCWLicense(transaction.Header);
			var lineMultiplier = MultiplierFor(transaction);
			model.Lines = transaction.Lines.OfType<InvoicingLineBase>()
					.Where(l => l.AL_OSAmount != 0m
							 && (salesTaxChargeCodeFromRegistry == Guid.Empty || l.AL_AC != salesTaxChargeCodeFromRegistry)
					)
					.Select(l => new AvalaraV2TransactionLineModel()
					{
						Number = l.AL_Sequence,
						Amount = l.AL_OSAmount * lineMultiplier,
						ItemCode = GetItemCodeFromChargeCodeOrGLHeader(l),
						Ref1 = hostedCode,
						Description = l.AL_Desc.ToNullIfEmpty(),
					})
					.OrderBy(l => l.Number)
					.ToList();

			return model;
		}

		public (CalculationResult result, Exception error) MapResponseToResultObject(
			Exception networkException,
			Uri url,
			HttpResponseMessage response,
			string responseJson,
			MappingType type)
		{
			if (networkException != null)
			{
				Tracer.TraceErrorEvent(AccountingTraceSourceCodes.Http, () => NetworkErrorTraceDetail(url, networkException, ObjectId));
				return (null, networkException);
			}
			Tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => ResponseTraceDetail(url, response, responseJson, ObjectId));

			if (!response.IsSuccessStatusCode)
			{
				var message = string.Empty;
				var (errorObject, parseErrorError) = MapJsonToModelOrError<AvalaraV2ErrorRootModel>(responseJson);
				if (parseErrorError != null || errorObject.Error == null)
				{
					message = FormattableString.Invariant($"{(int)response.StatusCode} {response.ReasonPhrase}.");
				}
				else
				{
					message = FormattableString.Invariant($"{errorObject.Error.Code} - {errorObject.Error.Message} ({(int)response.StatusCode} {response.ReasonPhrase}).");
				}
				return (null, new WebException(message));
			}

			var (responseObject, parseError) = MapJsonToModelOrError<AvalaraV2CreateTransactionResponseModel>(responseJson);
			if (parseError != null)
			{
				return (null, parseError);
			}
			if (responseObject.TotalTax == null)
			{
				return (null, new WebException("Web response is missing 'totalTax' field."));
			}

			var warnings = (responseObject.Messages ?? Enumerable.Empty<AvalaraV2MessageModel>())
							.Select(x => x.Details ?? string.Empty)
							.Distinct();
			var warningMessage = string.Join(System.Environment.NewLine, warnings);

			var totalSalesTaxAmount = responseObject.TotalTax.Value;
			var submissionStatus = type == MappingType.Calculate
									? string.Empty
									: responseObject.Status;

			var result = new CalculationResult(totalSalesTaxAmount,
				warningMessage: warningMessage,
				submissionStatus: submissionStatus
			);
			return (result, null);
		}

		public Exception GetResponseValidation(InvoicingBase transaction, CalculationResult result, MappingType mappingType)
		{
			if (mappingType == MappingType.Calculate)
			{
				return null;
			}
			else
			{
				var currentSalesTax = GetCurrentSalesTaxAmount(transaction);
				if (result.TotalSalesTaxAmount != currentSalesTax)
				{
					var difference = result.TotalSalesTaxAmount - currentSalesTax;
					return new ApplicationException(FormattableString.Invariant($"Mismatch between current sales tax on transaction ({currentSalesTax:N2}) and Avalara committed sales tax ({result.TotalSalesTaxAmount:N2}), difference of {difference:N2}."));
				}
				if (result.SubmissionStatus != "Committed")
				{
					return new ApplicationException(FormattableString.Invariant($"Avalara returned successful web response, but status was '{result.SubmissionStatus}'; expected 'Committed'."));
				}

				return null;
			}
		}

		#region Tracing Functions

		readonly ITracer Tracer;

		public static string RequestTraceDetail(Uri url, HttpRequestMessage request, string requestJson, HttpClient httpClient, string objectId)
		{
			var (requestObj, parseError) = MapJsonToModelOrError<object>(requestJson);
			var formattedJson = parseError != null
								? requestJson
								: JsonConvert.SerializeObject(requestObj, Formatting.Indented);

			var result = FormattableString.Invariant($@"
INTERNAL ID: {objectId}
URL: {url}
AUTH: {httpClient.DefaultRequestHeaders.Authorization}
METHOD: {request.Method}
REQUEST BODY:
{formattedJson}");
			return result;
		}

		public static string ResponseTraceDetail(Uri url, HttpResponseMessage response, string responseJson, string objectId)
		{
			var (responseObj, parseError) = MapJsonToModelOrError<object>(responseJson);
			var formattedJson = parseError != null
								? responseJson
								: JsonConvert.SerializeObject(responseObj, Formatting.Indented);

			var result = FormattableString.Invariant($@"
INTERNAL ID: {objectId}
URL: {url}
RESPONSE CODE: {(int)response.StatusCode} {response.ReasonPhrase}
RESPONSE BODY:
{formattedJson}");
			return result;
		}

		public static string NetworkErrorTraceDetail(Uri url, Exception ex, string objectId)
		{
			var result = FormattableString.Invariant($@"
INTERNAL ID: {objectId}
URL: {url}
NETWORK ERROR:
{ex}");
			return result;
		}

		#endregion

		#endregion

		#region IDisposable

		public void Dispose()
		{
			this.HttpClientFactory?.Dispose();
			this.HttpClientForProduction?.Dispose();
			this.HttpClientForSandbox?.Dispose();
		}

		#endregion

		#region Implementation

		(CalculationResult result, Exception error) DoWebRequestAndProcessResponse(InvoicingBase transaction, MappingType mappingType)
		{
			Argument.NotNull(transaction, nameof(transaction));

			var validationError = GetValidationError(transaction, mappingType);
			if (validationError != null)
			{
				return (null, validationError);
			}

			var requestObject = MapTransactionToObject(transaction, mappingType);
			if (!requestObject.Lines.Any())
			{
				return (new CalculationResult(0m, warningMessage: "No Line Items are eligible for transmission to Avalara as all mapped lines have zero amounts."), null);
			}
			var totalInvoiceAmountExcludingSalesTax = requestObject.Lines.Sum(l => l.Amount);
			var jsonContent = JsonConvert.SerializeObject(requestObject, Formatting.None);

			var baseUrl = GetBaseRestUrl(transaction.Branch);
			var url = new Uri(baseUrl, "Transactions/Create");
			using (var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json"))
			using (var request = new HttpRequestMessage(HttpMethod.Post, url))
			{
				request.Content = httpContent;
				var httpClient = GetOrCreateHttpClient(transaction.Branch);
				Tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => RequestTraceDetail(url, request, jsonContent, httpClient, ObjectId));
				var (response, responseJson, networkException) = SendToWebApiAndReadResponse(httpClient, request);

				var (result, error) = MapResponseToResultObject(networkException, url, response, responseJson, mappingType);
				if (error != null)
				{
					return (null, error);
				}

				error = GetResponseValidation(transaction, result, mappingType);
				if (error != null)
				{
					return (null, error);
				}

				result = result.WithInvoiceAmountExcludingSalesTax(totalInvoiceAmountExcludingSalesTax);
				return (result, error);
			}
		}

		readonly BusinessObjectFactory factory;

		static bool TransactionShouldBeProcesssedByLedgerAndType(InvoicingBase transaction)
			=> transaction != null
			&& transaction.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote);

		string GetMessageTypeForTransaction(InvoicingBase transaction, MappingType type)
			=> transaction.AH_TransactionType == TransactionTypes.Invoice    && type == MappingType.Calculate ? "SalesOrder"
			 : transaction.AH_TransactionType == TransactionTypes.CreditNote && type == MappingType.Calculate ? "ReturnOrder"
			 : transaction.AH_TransactionType == TransactionTypes.Invoice    && type == MappingType.Submit    ? "SalesInvoice"
			 : transaction.AH_TransactionType == TransactionTypes.CreditNote && type == MappingType.Submit    ? "ReturnInvoice"
			 : string.Empty;

		string GetHostedCodeFromCWLicense(OrgHeader org)
		{
			if (org == null)
			{
				return null;
			}
			var ediOrg = GetOrLoadEdiOrgHeader(org);
			var licenseDbs = ediOrg.LicEnterprise?.Databases?.OfType<LicenceDatabase>() ?? Enumerable.Empty<LicenceDatabase>();
			var licenseDb = licenseDbs
							.Where(d => d.LD_LicenceType == DatabaseTypes.Codes.Production
									 && (d.LD_Product == ProductTypes.Codes.CargoWiseOne || d.LD_Product == ProductTypes.Codes.CargoWiseNext || d.LD_Product == ProductTypes.Codes.CargoWise))
							.OrderByDescending(d => d.LD_IsActive).ThenBy(d => d.CurrentVersionExeDate)
							.FirstOrDefault();
			var hostedCode = licenseDb == null ? null
							: licenseDb.LD_HostedLocation == LicenceConstants.NotHostedWithCargoWise ? LicenceConstants.NotHostedWithCargoWise
							: "HST";
			return hostedCode;
		}

		string GetItemCodeFromChargeCodeOrGLHeader(InvoicingLineBase line)
			=> line.AL_AC.IsValid ? line.ChargeCode.AC_Code
			 : line.AL_AG.IsValid ? line.GLHeader.AccountNum
			 : null;

		decimal MultiplierFor(InvoicingBase transaction)
			=> transaction.AH_Ledger == LedgerTypes.AccountsPayable    ? -1m
			:  transaction.AH_Ledger == LedgerTypes.AccountsReceivable ?  1m
			: 1m;

		EDIOrgHeader GetOrLoadEdiOrgHeader(OrgHeader org)
			=> org is EDIOrgHeader ediHeader
				? ediHeader
				: factory.Load<EDIOrgHeader>(org.PK);

		StringRegistryItem GetAccountIdRegistry(GlbBranch branch)
		{
			var config = GetConfiguration(branch);
			switch (config)
			{
				case ConfigurationStatus.Off:
					return null;
				case ConfigurationStatus.Sandbox:
					return EDIDataRegistry.Instance.AvalaraAuthenticationSandboxUserId;
				case ConfigurationStatus.Production:
					return EDIDataRegistry.Instance.AvalaraAuthenticationProductionUserId;
				default:
					throw new InvalidOperationException("Unexpected configuration value: " + config);
			}
		}

		StringRegistryItem GetLicenseCodeRegistry(GlbBranch branch)
		{
			var config = GetConfiguration(branch);
			switch (config)
			{
				case ConfigurationStatus.Off:
					return null;
				case ConfigurationStatus.Sandbox:
					return EDIDataRegistry.Instance.AvalaraAuthenticationSandboxPassword;
				case ConfigurationStatus.Production:
					return EDIDataRegistry.Instance.AvalaraAuthenticationProductionPassword;
				default:
					throw new InvalidOperationException("Unexpected configuration value: " + config);
			}
		}

		OrgAddress GetMainUSOfficeAddress(InvoicingBase transaction)
			=> transaction.Header?.Addresses?.OfType<OrgAddress>()
				?.FirstOrDefault(a => a.OA_IsActive
									&& a.IsMainAddressOfType(OrgAddressType.Office)
									&& a.OA_RN_NKCountryCode == CountryCodes.UnitedStates
				);

		static (T model, Exception parseError) MapJsonToModelOrError<T>(string json) where T : class
		{
			try
			{
				var result = JsonConvert.DeserializeObject<T>(json);
				return (result, null);
			}
			catch (JsonReaderException ex)
			{
				return (null, ex);
			}
		}

		HttpClient CreateHttpClient(GlbBranch branch)
		{
			var timeoutFromRegistry = (double)EDIDataRegistry.Instance.AvalaraWebTimeoutSeconds
												.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var httpClient = HttpClientFactory.Create(TimeSpan.FromSeconds(timeoutFromRegistry));
			var authHeaderValue = GetAuthorizationHeader(branch);
			if (!string.IsNullOrEmpty(authHeaderValue))
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
			}
			return httpClient;
		}

		string GetAuthorizationHeader(GlbBranch branch)
		{
			var accountIdRegistry = GetAccountIdRegistry(branch);
			var accountIdFromRegistry = accountIdRegistry?.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) ?? string.Empty;
			var licenseKeyRegistry = GetLicenseCodeRegistry(branch);
			var licenseKeyFromRegistry = licenseKeyRegistry?.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) ?? string.Empty;

			if (string.IsNullOrEmpty(accountIdFromRegistry) || string.IsNullOrEmpty(licenseKeyFromRegistry))
			{
				return string.Empty;
			}

			var authString = FormattableString.Invariant($"{accountIdFromRegistry}:{licenseKeyFromRegistry}");
			var authStringAsBytes = System.Text.Encoding.UTF8.GetBytes(authString);
			var result = Convert.ToBase64String(authStringAsBytes);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		static (HttpResponseMessage response, string responseJson, Exception networkError) SendToWebApiAndReadResponse(HttpClient httpClient, HttpRequestMessage request)
		{
			try
			{
				var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
				var responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				return (response, responseJson ?? string.Empty, null);
			}
			catch (OperationCanceledException ex)
			{
				return (null, null, ex);
			}
			catch (TimeoutException ex)
			{
				return (null, null, ex);
			}
			catch (WebException ex) when (ex.Message == "The operation has timed out")
			{
				return (null, null, new TimeoutException("The web request has timed out", ex));
			}
			catch (HttpRequestException ex)
			{
				return (null, null, ex);
			}
			catch (WebException ex)
			{
				return (null, null, ex);
			}
		}

		readonly string ObjectId;
		HttpClient HttpClientForSandbox;
		HttpClient HttpClientForProduction;
		readonly IHttpClientFactory HttpClientFactory;

		public enum MappingType
		{
			Calculate = 1,
			Submit
		}

		#endregion
	}

	#region Implementation (Extension Methods)

	internal static class Extensions
	{
		public static string ToNullIfEmpty(this ZString str)
			=> str.IsEmpty ? null : str.ToString();

		public static decimal? ToNullIfEmpty(this ZDecimal num)
			=> num.IsEmpty ? null : (decimal?)num;

		public static string ToSimpleString(this Exception ex)
			=> FormattableString.Invariant($"{ex.GetType().Name} - {ex.Message}");
	}

	#endregion
}
