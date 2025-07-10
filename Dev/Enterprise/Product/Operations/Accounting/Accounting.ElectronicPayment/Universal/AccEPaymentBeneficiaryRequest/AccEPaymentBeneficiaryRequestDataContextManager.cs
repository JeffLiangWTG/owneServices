using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Accounting.ElectronicPayment.Universal.AccEPaymentBeneficiaryRequestMessageConstants;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	public class AccEPaymentBeneficiaryRequestDataContextManager : EventDataContextManager<AccEPaymentBeneficiaryRequest>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		#region Implementations

		public override DataContextType DataContextType => DataContextType.AccEPaymentBeneficiaryRequest;

		public override ZString DataContextKey => ParentBO.ABR_InternalReference;

		public override string DefaultOutputDirectory => null;

		public bool ManagesTransactions => false;

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AccEPaymentBeneficiaryRequestEventParentFinder(factory, this, logger);
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			Argument.NotNull(matchingValues, nameof(matchingValues));
			Argument.NotNull(factory, nameof(factory));
			var requestNumber = matchingValues.Key;
			if (requestNumber.IsEmpty || !(matchingValues.DataObject is UniversalEvent universalEvent))
			{
				return ZQuery.NoResultQuery;
			}

			var providerCode = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, universalEvent.EventParameters) ?? ZString.Empty;
			if (!(universalEvent.DataContext is UniversalDataBuss.DataObjects.Universal._2012_11.DataContext))
			{
				return ZQuery.NoResultQuery;
			}

			var context = universalEvent.ContextCollection?.FirstOrDefault(x => x.Type == XUEFieldNames.CompanyCode);
			GlbCompany company = null;
			if (context == null || !context.Value.HasValue)
			{
				return ZQuery.NoResultQuery;
			}
			else
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, context.Value.Value);
			}

			var query = new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_InternalReference, requestNumber);
			query.AddToFilter(new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_GC_Company, company?.PK ?? ZGuid.Empty));
			query.AddToFilter(new ZQuery(AccEPaymentBeneficiaryRequestSchema.ABR_ProviderCode, providerCode));

			return query;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject is AccEPaymentBeneficiaryRequest parent)
			{
				var eventObject = (UniversalEvent)eventDataObject;
				if (eventObject.ContextCollection == null)
				{
					var errorMessage = Res.GetString("4780cca5-94d4-4b7e-8024-f732df8f2bc1", "Event does not contain a Context Collection.");
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, errorMessage);
					return;
				}

				if (eventDataObject.EventType == Events.InterchangeAcknowledgedCode)
				{
					HandleIAKMessage(logger, eventObject, parent, message);
				}
				else if (eventDataObject.EventType == Events.InterchangeRejectedCode)
				{
					HandleIRJMessage(logger, eventObject, parent, message);
				}
				else
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("AC3EA007-090D-4005-A86A-570AE3624D1D", "Unexpected Event Type encountered: {0}.", eventDataObject.EventType));
				}
			}
			else
			{
				LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("A7095D6A-1EBA-4B9E-A7C9-6A8F289E8838", "Unable to process - unsupported type detected, Parent BO type is {0}", businessObject.GetType()));
				return;
			}
		}

		#endregion

		void HandleIAKMessage(IXmlSessionTracker logger, UniversalEvent eventObject, AccEPaymentBeneficiaryRequest parent, IEDIMessage message)
		{
			if (!CheckForMissingOrEmptyContextFields(logger, eventObject, out var fieldValues))
			{
				var provider = GetProviderCodeFromEventMessage(eventObject);
				var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters);
				if (!messageSubType.HasValue || messageSubType.Value != MessageSubTypes.SearchBeneficiary)
				{
					var errorMessage = Res.GetString("dfa3cf8d-c26a-4676-8616-7dab21e9e7ee", "<MessageSubType> key has '{0}' as its value which is invalid.", messageSubType);
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, errorMessage);
					return;
				}

				try
				{
					var base64SerializedSearchBenResult = fieldValues[XUEFieldNames.BeneficiarySearchResult];
					var serializedSearchBenResult = new UTF8Encoding(false).GetString(Convert.FromBase64String(base64SerializedSearchBenResult));

					var beneficiarySearchResult = DataObjectSerializer.Deserialize<BeneficiarySearchResult>(serializedSearchBenResult);
					if (beneficiarySearchResult != null)
					{
						if (beneficiarySearchResult.Beneficiaries.Any())
						{
							SaveAccEPaymentBeneficiaries(parent.Factory, provider, parent.ABR_GC_Company, beneficiarySearchResult.Beneficiaries);
							if (!beneficiarySearchResult.IsComplete)
							{
								var reference = FormattableString.Invariant($"Received information of {beneficiarySearchResult.Beneficiaries.Count} beneficiaries from {provider} against beneficiary search request: {parent.ABR_InternalReference}. This is a partial list of beneficiaries."); // Log Reference does not need to be translated.
								AddLog(parent, Events.All[eventObject.EventType], reference, message);

								var lLogger = ReIssueSearchBeneficiaryRequest(parent, beneficiarySearchResult.EndPageNumber + 1);
								if (lLogger.HasError)
								{
									UpdateStatus(parent, eventObject, StatusCodes.Error);
									SetErrorDescription(parent, FormattableString.Invariant($"Error occurred while creating GEP request to get rest of the Beneficiaries from {parent.ABR_ProviderCode}")); // Log Reference does not need to be translated.
									LoggerWrapper.ReportAndLogError(logger, LogType.Error, lLogger.GetErrorsAsString());
								}
								else
								{
									UpdateStatus(parent, eventObject, StatusCodes.Partial);
								}
							}
							else
							{
								var reference = FormattableString.Invariant($"All beneficiaries are returned successfully by {provider} against beneficiary search request: {parent.ABR_InternalReference}"); // Log Reference does not need to be translated.
								AddLog(parent, Events.All[eventObject.EventType], reference, message);
								UpdateStatus(parent, eventObject, StatusCodes.Received);
							}
						}
						else
						{
							var reference = FormattableString.Invariant($"No Beneficiary is returned by {provider}"); // Log Reference does not need to be translated.
							AddLog(parent, Events.All[eventObject.EventType], reference, message);
							UpdateStatus(parent, eventObject, StatusCodes.Received);
						}
					}
				}
				catch (InvalidOperationException ex)
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, ex.Message);
					return;
				}
			}
		}

		void HandleIRJMessage(IXmlSessionTracker logger, UniversalEvent eventObject, AccEPaymentBeneficiaryRequest parent, IEDIMessage message)
		{
			if (!CheckForMissingOrEmptyContextFields(logger, eventObject, out var fieldValues))
			{
				var errorMessage = fieldValues[XUEFieldNames.ErrorMessage];
				UpdateStatus(parent, eventObject, StatusCodes.Error);
				AccEPaymentHelper.InvalidateStaffTokenIfRefreshTokenInvalid(errorMessage, parent.ABR_SystemCreateUser, parent.ABR_GC_Company, logger);
				SetErrorDescription(parent, errorMessage);
				var providerCode = GetProviderCodeFromEventMessage(eventObject);
				var reference = $"{providerCode} E-Payment Error: {errorMessage}"; // Log Reference does not need to be translated.
				AddLog(parent, Events.All[eventObject.EventType], reference, message);
			}
		}

		void AddLog(AccEPaymentBeneficiaryRequest parent, ZArchitecture.Business.Event @event, string reference, IEDIMessage message)
		{
			var logToUpdate = parent.Logs.Find(l => !l.IsInDatabase && l.SL_SE_NKEvent == @event.Code).FirstOrDefault();
			if (logToUpdate != null)
			{
				var formattedReference = reference.Length > StmALogSchema.SL_Reference.MaxLength ? reference.Substring(0, StmALogSchema.SL_Reference.MaxLength) : reference;
				logToUpdate?.UpdateReference(formattedReference);
			}
		}

		void UpdateStatus(AccEPaymentBeneficiaryRequest parent, UniversalEvent eventObject, ZString statusCode)
		{
			//TODO: eventObject.EventTime is local time. But it is considered UTC. This should be fixed.
			var eventTime = eventObject.EventTime.GetValueOrDefault();
			parent.ABR_LastResponseReceivedUtc = eventTime.IsEmpty ? ZDateTime.UtcNow : eventTime.ToZDateTime();
			parent.ABR_Status = statusCode;
		}

		ZString GetProviderCodeFromEventMessage(UniversalEvent eventObject) => EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageType, eventObject.EventParameters) ?? ZString.Empty;

		void SaveAccEPaymentBeneficiaries(BusinessObjectFactory benFactory, string provider, ZGuid companyPK, List<BeneficiaryDetails> beneficiaries)
		{
			var existingBens = GetExistingAccEPaymentBeneficiaries();
			var apBankAccDetails = GetLinkedAPBankAccounts();
			var countries = new RefCountryCollection(benFactory);

			foreach (var beneficiary in beneficiaries)
			{
				var accEPaymentBen = GetOrCreate(beneficiary.ProviderReference);
				PopulateProperties(accEPaymentBen, beneficiary);
				if (apBankAccDetails.ContainsKey(accEPaymentBen.PK))
				{
					MatchEPaymentRecipients.SetAccountDetailsValuesFromBeneficiary(apBankAccDetails[accEPaymentBen.PK], accEPaymentBen);
				}
			}

			Dictionary<ZString, AccEPaymentBeneficiary> GetExistingAccEPaymentBeneficiaries()
			{
				var accountNames = beneficiaries.Select(b => (ZString)b.ProviderReference).ToList();
				if (accountNames.Any())
				{
					var query = new ZDBOnlyQuery(typeof(AccEPaymentBeneficiary));
					query.AddToFilter(AccEPaymentBeneficiarySchema.ABF_ProviderCode, provider);
					query.AddToFilter(AccEPaymentBeneficiarySchema.ABF_ProviderReference, accountNames);
					var accEPaymentBeneficiaries = benFactory.Load<AccEPaymentBeneficiary>(query).ToDictionary(b => b.ABF_ProviderReference);
					return accEPaymentBeneficiaries;
				}
				else
				{
					return new Dictionary<ZString, AccEPaymentBeneficiary>();
				}
			}

			Dictionary<ZGuid, AccAPAccountDetails> GetLinkedAPBankAccounts()
			{
				var abfPKs = existingBens.Values.Select(b => b.PK).ToList();
				if (abfPKs.Any())
				{
					var query = new ZDBOnlyQuery(typeof(AccAPAccountDetails));
					query.AddToFilter(AccAPAccountDetailsSchema.A1_EPaymentBeneficiaryId, abfPKs);
					var accAPAccountDetails = benFactory.Load<AccAPAccountDetails>(query).ToDictionary(b => b.A1_EPaymentBeneficiaryId);
					return accAPAccountDetails;
				}
				else
				{
					return new Dictionary<ZGuid, AccAPAccountDetails>();
				}
			}

			AccEPaymentBeneficiary GetOrCreate(string providerGivenAccountName)
			{
				AccEPaymentBeneficiary accEPaymentBeneficiary;
				if (existingBens.ContainsKey(providerGivenAccountName))
				{
					accEPaymentBeneficiary = existingBens[providerGivenAccountName];
				}
				else
				{
					accEPaymentBeneficiary = benFactory.New<AccEPaymentBeneficiary>();
					accEPaymentBeneficiary.ABF_GC_Company = companyPK;
				}
				return accEPaymentBeneficiary;
			}

			void PopulateProperties(AccEPaymentBeneficiary to, BeneficiaryDetails from)
			{
				to.ABF_BankAccount = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankAccount.MaxLength, from.BankAccount);
				to.ABF_BankAddress1 = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankAddress1.MaxLength, from.BankAddress);
				to.ABF_BankBranchName = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankBranchName.MaxLength, from.BankBranchName);
				to.ABF_BankBsb = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankBsb.MaxLength, FormattableString.Invariant($"{from.BankCode}{from.BranchCode}")); // These are code. Translation is not required.
				to.ABF_BankName = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankName.MaxLength, from.BankName);
				to.ABF_BankSwift = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BankSwift.MaxLength, from.BankSwift);
				to.ABF_BeneficiaryFullName = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BeneficiaryFullName.MaxLength, from.BeneficiaryFullName);
				to.ABF_BeneficiaryNickName = NomraliseString(AccEPaymentBeneficiarySchema.ABF_BeneficiaryNickName.MaxLength, from.BeneficiaryNickName);
				to.ABF_EmailAddress = NomraliseString(AccEPaymentBeneficiarySchema.ABF_EmailAddress.MaxLength, from.EmailAddress);
				to.ABF_ProviderClassification = NomraliseString(AccEPaymentBeneficiarySchema.ABF_ProviderClassification.MaxLength, from.ProviderClassification);
				to.ABF_ProviderCode = from.ProviderCode;
				to.ABF_ProviderReference = from.ProviderReference;
				to.ABF_RN_NKCountryCode = GetCountryCode(countries, from.Country);
				to.ABF_RX_NKAccountCurrency = from.Currency;
			}

			string NomraliseString(int maxLength, string val) => val.Length > maxLength ? val.Substring(0, maxLength - 1) : val;
		}

		bool CheckForMissingOrEmptyContextFields(IXmlSessionTracker logger, UniversalEvent eventObject, out Dictionary<ZString, ZString> fieldValues)
		{
			var isMissingFields = false;
			var eventType = eventObject.EventType.GetValueOrDefault();
			fieldValues = eventObject.ContextCollection.Where(x => x.Type.Type.HasValue && x.Value.HasValue && !x.Value.Value.IsEmpty)
													.ToDictionary(x => x.Type.Type.Value, x => x.Value.Value);

			if (eventType == Events.InterchangeAcknowledgedCode)
			{
				var messageSubType = EventParameters.GetEventParameter(EventReferenceParameters.Codes.MessageSubType, eventObject.EventParameters) ?? ZString.Empty;
				var missingFieldsForIAKEvent = XUEFieldNames.GetRequiredFieldsForIAK().Except(fieldValues.Keys);
				ReportMissingFieldsIfRequired(missingFieldsForIAKEvent);
			}
			else if (eventType == Events.InterchangeRejectedCode)
			{
				var missingFieldsForIRJEvent = XUEFieldNames.GetRequiredFieldsForIRJ().Except(fieldValues.Keys);
				ReportMissingFieldsIfRequired(missingFieldsForIRJEvent);
			}
			return isMissingFields;

			void ReportMissingFieldsIfRequired(IEnumerable<ZString> missingFields)
			{
				if (missingFields.Any())
				{
					LoggerWrapper.ReportAndLogError(logger, LogType.Error, Res.GetString("c90b6d4c-5e75-48cb-80cd-5b29249b4930", @"The following Context fields are either empty or missing:
{0}", string.Join(System.Environment.NewLine, missingFields)));
					isMissingFields = true;
				}
			}
		}

		ListLogger ReIssueSearchBeneficiaryRequest(AccEPaymentBeneficiaryRequest parent, int newStartPageNumber)
		{
			var lLogger = new ListLogger();
			var factory = new BusinessObjectFactory();
			var reloadedParent = factory.Load<AccEPaymentBeneficiaryRequest>(parent.PK);
			var queue = new PartialBeneficiaryRequestQueue(reloadedParent, newStartPageNumber);
			var processor = new GlobalElectronicPaymentProcessor(new[] { queue }, new[] { parent.Company });
			processor.ProcessEPayments(lLogger, CancellationToken.None);
			return lLogger;
		}

		string GetCountryCode(RefCountryCollection countries, string sourceCountry)
		{
			if (sourceCountry.Length == 2)
			{
				var countryCode = countries.FirstOrDefault(c => c.RN_Code == sourceCountry.ToUpper())?.RN_Code ?? ZString.Empty;
				return countryCode;
			}
			else
			{
				var countryCode = countries.FirstOrDefault(c => c.RN_Desc == sourceCountry.ToUpper())?.RN_Code ?? ZString.Empty;
				return countryCode;
			}
		}

		void SetErrorDescription(AccEPaymentBeneficiaryRequest request, ZString errorMessage)
		{
			request.ABR_ErrorDescription = AccEPaymentHelper.GetErrorDescription(errorMessage, AccEPaymentBeneficiaryRequestSchema.ABR_ErrorDescription.MaxLength);
		}

		public class ListLogger : ILogger
		{
			public ListLogger()
			{
				logs = new List<(LogType Type, string Message)>();
			}

			readonly List<(LogType Type, string Message)> logs;

			public void Log(LogType type, string message)
			{
				logs.Add((type, message));
			}

			public void Log(LogType type, string message, Exception ex)
			{
				logs.Add((type, message));
			}

			public bool HasError => logs.Any(l => l.Type == LogType.Error);

			public string GetErrorsAsString() => string.Join("\r\n", logs.Where(l => l.Type == LogType.Error).Select(l => l.Message).ToArray());
		}
	}
}
