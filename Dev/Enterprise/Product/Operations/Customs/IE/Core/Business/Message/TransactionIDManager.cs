using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.TransactionIDRequest;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class TransactionIDManager
	{
		public static TransactionIDManager New(ILogger logger, bool useEMCS)
		{
			return useEMCS ?
				new TransactionIDManager(logger, CusTransactionNumberTypeList.Codes.IECustomsEMCS, new[] { EDIMessage.ApplicationCodes.IECustomsEMCS }, EDIMessage.ApplicationCodes.IECustomsEMCS, true, new[] { EmcsCustomsDataRegistry.Instance.EmcsSendMessageErrors }) :
				new TransactionIDManager(logger, CusTransactionNumberTypeList.Codes.IECustoms, new[] { EDIMessage.ApplicationCodes.IECustomsExport, EDIMessage.ApplicationCodes.IECustomsImport, EDIMessage.ApplicationCodes.IECustomsUCC5Import, EDIMessage.ApplicationCodes.IECustomsNCTS }, EDIMessage.ApplicationCodes.IECustomsCommon, false, new IRegistryItem[]
				{
					EUCustomsDataRegistry.Instance.SendExportMessageErrors,
					ExitControlCustomsDataRegistry.Instance.SendExitControlErrors,
					EUCustomsDataRegistry.Instance.SendNctsErrors
				});
		}

		protected TransactionIDManager(ILogger logger, string transactionType, string[] messageApplicationCodes, string transactionIDApplicationCode, bool filterOnCredential, IRegistryItem[] registryItems)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = new BusinessObjectFactory();
			this.transactionType = transactionType;
			this.messageApplicationCodes = messageApplicationCodes;
			this.transactionIDApplicationCode = transactionIDApplicationCode;
			this.webServiceEndPoint = WebServiceEndPointProvider.GetTransactionIDURL(factory, transactionIDApplicationCode);
			this.filterOnCredential = filterOnCredential;
			this.registryItems = Argument.NotNull(registryItems, nameof(registryItems));
			this.credentialValidities = new Dictionary<ZGuid, bool>();
			this.companyPKToCredentialPKMapping = new Dictionary<ZGuid, ZGuid>();
		}
		protected readonly ILogger logger;
		protected readonly BusinessObjectFactory factory;
		readonly string transactionType;
		readonly string[] messageApplicationCodes;
		readonly string transactionIDApplicationCode;
		readonly string webServiceEndPoint;
		readonly bool filterOnCredential;
		readonly IRegistryItem[] registryItems;
		readonly Dictionary<ZGuid, bool> credentialValidities;
		readonly Dictionary<ZGuid, ZGuid> companyPKToCredentialPKMapping;

		public void Process(CancellationToken youMustReactToThisToken)
		{
			var messageProcessingNoOfDays = IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.Value;
			var noOfDays = new RefSysConfig.Loader(factory).GetDecimalValue(Customs.Universal.Constants.RefSysConfig.ConfigCodes.NoOfDaysIETransactionIDShouldBeKept, 3m, ZDateTime.UtcToday).ToZInt();
			ExecuteBatch(new TransactionIDRequestProcessor(SendErrorNotification, noOfDays, transactionType, transactionIDApplicationCode), youMustReactToThisToken);
			try
			{
				if (HasOutgoingMessageInLastNoOfDays(messageProcessingNoOfDays))
				{
					transactionsAndMessagesData = new Dictionary<ZGuid, Dictionary<ZGuid, TransactionAndMessageData>>();
					Replenish(youMustReactToThisToken, noOfDays, messageProcessingNoOfDays);
					Allocate(youMustReactToThisToken, noOfDays);
				}
			}
			finally
			{
				transactionsAndMessagesData = null;
			}
		}

		public static void SetTransactionNumbersAsUsed(GlbCompany company, ZGuid credentialPK, ZString type)
		{
			var query = new ZQuery(CusTransactionNumberSchema.TN_IsUsed, ZBool.False);
			query.AddToFilter(CusTransactionNumberSchema.TN_Type, type);
			query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, company.PK);
			if (credentialPK.IsValid)
			{
				query.AddToFilter(CusTransactionNumberSchema.TN_GP_ExternalPassword, credentialPK);
			}
			else
			{
				query.AddToFilter(CusTransactionNumberSchema.TN_GP_ExternalPassword, DBNull.Value);
			}
			query.FetchOnlyFromLocalCache = !company.IsInDatabase;
			company.Factory.Load<CusTransactionNumber>(query).ForEach(transactionNumber => transactionNumber.TN_IsUsed = ZBool.True);
		}

		void ExecuteBatch(BatchProcess processor, CancellationToken youMustReactToThisToken)
		{
			try
			{
				processor.Logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
				processor.ExecuteBatch(youMustReactToThisToken);
			}
			finally
			{
				processor.Logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
			}
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			logger.Log(logType, log.Trim());
		}

		Dictionary<ZGuid, Dictionary<ZGuid, TransactionAndMessageData>> transactionsAndMessagesData;

		bool HasOutgoingMessageInLastNoOfDays(int messageProcessingNoOfDays)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, messageApplicationCodes);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(-1 + -messageProcessingNoOfDays));
			return factory.ExistsInDatabase(EDIMessageSchema.Constants.TableName, query);
		}

		class TransactionAndMessageData
		{
			public readonly List<CusTransactionNumber> AvailableTransactionIDs = new List<CusTransactionNumber>();
			public readonly List<EDIMessage> Messages = new List<EDIMessage>();
			public readonly HashSet<ZGuid> TrackingReferences = new HashSet<ZGuid>();
			public int TotalUsed;
			public int Average;
			public int Pending;
		}

		Queue<CusTransactionNumber> CreateQueueTransactionNumber(IEnumerable<CusTransactionNumber> transactionNumbers) => new Queue<CusTransactionNumber>(transactionNumbers.OrderBy(x => x.TN_SystemCreateTimeUtc).ThenBy(y => y.TN_TransactionReference));

		void Replenish(CancellationToken youMustReactToThisToken, int noOfDays, int messageProcessingNoOfDays)
		{
			needSaving = false;
			GetTransactionsData(noOfDays);
			GatherMessagesThatNeedTransactionID(transactionsAndMessagesData, messageProcessingNoOfDays);
			if (transactionsAndMessagesData.Count > 0)
			{
				ReplenishIfNeeded(youMustReactToThisToken);
			}
			SaveDataIfNeeded();
		}

		void ReplenishIfNeeded(CancellationToken youMustReactToThisToken)
		{
			foreach (var credentialData in transactionsAndMessagesData)
			{
				var companyDictionary = credentialData.Value;
				var credentialPK = credentialData.Key;
				var isValidCredentialPK = credentialPK.IsValid;
				if (isValidCredentialPK && !IsCredentialValid(credentialPK))
				{
					continue;
				}
				foreach (var companyData in companyDictionary)
				{
					var companyPK = companyData.Key;
					var company = factory.Load<GlbCompany>(companyPK);
					var companyName = company.CompanyName;
					if (youMustReactToThisToken.IsCancellationRequested)
					{
						return;
					}
					else
					{
						ZGuid interchangeCredentialPK;
						if (isValidCredentialPK)
						{
							interchangeCredentialPK = credentialPK;
						}
						else
						{
							var companyCredentialPK = GetCredentialPKFromCompanyPK(companyPK);
							if (!IsCredentialValid(companyCredentialPK))
							{
								continue;
							}
							else
							{
								interchangeCredentialPK = companyCredentialPK;
							}
						}
						var transactionAndMessageData = companyData.Value;
						var dailyAverage = transactionAndMessageData.Average;
						var totalAvailable = transactionAndMessageData.AvailableTransactionIDs.Count;
						var totalPending = transactionAndMessageData.Pending;
						var messages = transactionAndMessageData.Messages;
						var messagesCount = messages.Count;
						if (messagesCount > 0)
						{
							var lastSendMessage = messages.Select(x => x.EM_SystemCreateTimeUtc).Max();
							var lastestTransactionIDRequestMesssage = GetLastestTransactionIDRequestSince(lastSendMessage, transactionIDApplicationCode, companyPK, interchangeCredentialPK, EDIInterchange.Direction.Transmit);
							var hours = lastSendMessage.Date == ZDateTime.UtcToday ? 1 : 3;
							if (ShouldSendRequest(lastestTransactionIDRequestMesssage, companyPK, hours))
							{
								var firstMessage = messages[0];
								var branchPK = firstMessage.EM_GB;
								var totalNeeded = messagesCount;
								if (CreateTransactionIDRequest(transactionType, transactionIDApplicationCode, companyName, companyPK, credentialPK, interchangeCredentialPK, branchPK, dailyAverage, totalAvailable, totalPending, totalNeeded) > 0)
								{
									needSaving = true;
								}
							}
						}
						else if (company != null && dailyAverage - totalAvailable - totalPending > 0)
						{
							var lastestTransactionIDRequestMesssage = GetLastestTransactionIDRequestSince(ZDateTime.UtcToday, transactionIDApplicationCode, companyPK, interchangeCredentialPK, EDIInterchange.Direction.Transmit);
							if (ShouldSendRequest(lastestTransactionIDRequestMesssage, companyPK, 3) && CreateTransactionIDRequest(transactionType, transactionIDApplicationCode, companyName, companyPK, credentialPK, interchangeCredentialPK, company.FirstActiveBranch.PK, dailyAverage, totalAvailable, totalPending, 0) > 0)
							{
								needSaving = true;
							}
						}
					}
				}
			}
		}

		bool ShouldSendRequest(EDIInterchange lastestTransactionIDRequestMesssage, ZGuid companyPK, int valueToAdd)
		{
			var shouldSendRequest = false;
			if (lastestTransactionIDRequestMesssage == null)
			{
				shouldSendRequest = true;
			}
			else if (!lastestTransactionIDRequestMesssage.EI_Status.EqualsIgnoringCase(EDIInterchange.Status.Queued))
			{
				var systemCreateTimeUtc = lastestTransactionIDRequestMesssage.EI_SystemCreateTimeUtc;
				if (systemCreateTimeUtc.AddHours(valueToAdd) < ZDateTime.UtcNow)
				{
					shouldSendRequest = true;
				}
				else
				{
					var lastestTransactionIDRequestResponseMesssage = GetLastestTransactionIDRequestSince(systemCreateTimeUtc, transactionIDApplicationCode, companyPK, ZGuid.Empty, EDIInterchange.Direction.Receive, new ZQuery(EDIInterchangeSchema.EI_SessionGUID, lastestTransactionIDRequestMesssage.EI_SessionGUID));
					shouldSendRequest = lastestTransactionIDRequestResponseMesssage != null && lastestTransactionIDRequestResponseMesssage.EI_Status.EqualsIgnoringCase(EDIInterchange.Status.Received);
				}
			}
			return shouldSendRequest;
		}

		bool IsCredentialValid(ZGuid credentialPK)
		{
			if (!credentialValidities.TryGetValue(credentialPK, out var result))
			{
				var credential = factory.Load<GlbExternalPassword>(credentialPK);
				if (credential != null)
				{
					if ((credential as IGlbExternalPasswordWithCertificate).HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false))
					{
						if (credential.GP_PasswordStatusInfo.HasChanges)
						{
							NotifyCredentialHasExpired(credential);
						}
					}
					else
					{
						result = true;
					}
				}
				credentialValidities.Add(credentialPK, result);
			}
			return result;
		}

		void NotifyCredentialHasExpired(GlbExternalPassword credential)
		{
			var subject = Res.GetString("{C0D31E10-50ED-455C-9996-1F2E758CA250}", "Revenue Online Service Credential Has Expired");
			var credentialDescription = credential.GP_MailBoxIDInfo.Description;
			var credentialMailBoxID = credential.GP_MailBoxID;
			var credentialExpiryDate = credential.GP_ExpiryDate.ToLongTimeString();
			var body = Res.GetString("{84BAED14-5DBE-49CB-A12F-95665DC8483E}", "The credential for {0} '{1}' expired on {2}.", credentialDescription, credentialMailBoxID, credentialExpiryDate);
			var branchPK = credential.Company?.FirstActiveBranch?.PK ?? ZGuid.Empty;
			SendErrorNotification(credential.GP_GC, branchPK, subject, body);
			logger.Log(LogType.Error, string.Format("The Revenue Online Service Credential for {0} '{1}' expired on {2}.", credentialDescription, credentialMailBoxID, credentialExpiryDate));
		}

		public delegate void SendErrorNotificationDelegate(ZGuid companyPK, ZGuid branchPK, string subject, string body);

		protected void SendErrorNotification(ZGuid companyPK, ZGuid branchPK, string subject, string body)
		{
			using (branchPK.IsValid && branchPK != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPK.ToGuid()) : null)
			{
				var email = EmailDefBuilder.GetEmail(subject, body);
				registryItems.SelectMany(registryItem => GetEmailAddressesFromNominatedGroups(GetEmailGroupPK(registryItem, companyPK))).Distinct().ForEach(emailAddress => email.AddRecipientForSystemCommunication(emailAddress, RecipientDef.RecipientTypes.TO));
				if (email.Recipients.Count > 0)
				{
					Env.OutgoingCustomsMailManager.CreateAndSave(email);
				}
			}
		}

		ZGuid GetEmailGroupPK(IRegistryItem registryItem, ZGuid companyPK)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				var emailGroup = registryItem.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
				if (emailGroup is GroupNotification groupNotification)
				{
					result = groupNotification.SendGroupPK;
				}
				else if (emailGroup is Guid)
				{
					result = (Guid)emailGroup;
				}
			}
			return result;
		}

		IEnumerable<ZString> GetEmailAddressesFromNominatedGroups(params ZGuid[] groupPKs)
		{
			var emailGroupUtility = new EmailGroupUtility();
			foreach (var groupPK in groupPKs)
			{
				if (groupPK.IsValid && factory.Load<GlbGroup>(groupPK) is GlbGroup notificationGroup)
				{
					foreach (var staff in notificationGroup.Staff.OfType<GlbStaff>().Where(x => x.GS_IsActive))
					{
						var emaillAddress = staff.GS_EmailAddress;
						if (!emaillAddress.IsEmpty && !emailGroupUtility.IsHostNotificationEmail(emaillAddress))
						{
							yield return emaillAddress;
						}
					}
				}
			}
		}

		EDIInterchange GetLastestTransactionIDRequestSince(ZDateTime dateTimeFrom, ZString applicationCode, ZGuid companyPK, ZGuid credentialPK, ZString? direction = null, ZQuery additionalFilter = null)
		{
			var query = new ZDBOnlyQuery(typeof(EDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, applicationCode);
			if (credentialPK.IsValid)
			{
				query.AddToFilter(EDIInterchangeSchema.EI_GP, credentialPK);
			}
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
			if (direction.HasValue)
			{
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, direction);
			}
			query.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, dateTimeFrom);
			var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, companyPK);
			query.AddSubQuery(EDIInterchangeSchema.EI_GB, branchSubQuery, JoinCondition.And);
			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			query.OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc + OrderByClause.Descending;
			return factory.LoadTop1<EDIInterchange>(query);
		}

		void Allocate(CancellationToken youMustReactToThisToken, int noOfDaysOld)
		{
			if (transactionsAndMessagesData.Count > 0)
			{
				needSaving = false;

				foreach (var credentialData in transactionsAndMessagesData)
				{
					var credentialPK = credentialData.Key;
					var companyDictionary = credentialData.Value;
					foreach (var companyData in companyDictionary)
					{
						if (youMustReactToThisToken.IsCancellationRequested)
						{
							break;
						}
						else
						{
							var companyPK = companyData.Key;
							var transactionAndMessageData = companyData.Value;
							var messages = new Queue<EDIMessage>(transactionAndMessageData.Messages.OrderBy(o => o.EM_SystemCreateTimeUtc));
							var messagesCount = messages.Count;
							if (messagesCount > 0)
							{
								var company = factory.Load<GlbCompany>(companyPK);
								var companyName = company.CompanyName;
								logger.Log(LogType.Information, string.Format("{0}: Found {1} message(s) waiting for Transaction ID allocation", companyName, messagesCount));
								var availableTransactionIDs = CreateQueueTransactionNumber(transactionAndMessageData.AvailableTransactionIDs);
								Allocate(youMustReactToThisToken, messages, availableTransactionIDs);
							}
						}
					}
					if (youMustReactToThisToken.IsCancellationRequested)
					{
						break;
					}
				}
				SaveDataIfNeeded();
			}
		}

		void Allocate(CancellationToken youMustReactToThisToken, Queue<EDIMessage> messages, Queue<CusTransactionNumber> availableIDs)
		{
			while (!youMustReactToThisToken.IsCancellationRequested)
			{
				if (messages.Count == 0 || availableIDs.Count == 0)
				{
					break;
				}
				var message = messages.Dequeue();
				var transactionNumber = availableIDs.Dequeue();
				transactionNumber.TN_IsUsed = ZBool.True;
				var transactionReference = transactionNumber.TN_TransactionReference;
				message.EM_ApplicationReference = transactionReference;
				message.EM_Status = EDIMessage.Status.Pending;
				logger.Log(LogType.Information, string.Format("Allocating Transaction ID '{0}' to Message #{1}", transactionReference, message.EM_MessageNum));
				needSaving = true;
			}
		}

		void DiscardMessageAndFailLogicalStatus(EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.Discarded;
			if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				messageAttachee.LogicalStatus = LogicalStatusList.Codes.Failed;
			}
			needSaving = true;
		}

		void SaveDataIfNeeded()
		{
			if (needSaving)
			{
				needSaving = false;
				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
		bool needSaving;

		int CreateTransactionIDRequest(ZString transactionType, ZString applicationCode, ZString companyName, ZGuid companyPK, ZGuid credentialPK, ZGuid interchangeCredentialPK, ZGuid branchPK, int dailyAverage, int totalAvailable, int totalPending, int totalNeeded)
		{
			int noOfRequest = 0;
			var remaining = totalAvailable - totalNeeded + totalPending;
			var requestAmount = remaining < 0 ? dailyAverage + Math.Abs(remaining) : dailyAverage - remaining;
			if (requestAmount > 0)
			{
				noOfRequest = (int)Math.Ceiling((decimal)requestAmount / MaximumNumberOfIDsPerRequest);
				for (var i = 1; i <= noOfRequest; i++)
				{
					var transactionIDRequest = new TransactionIdRequest()
					{
						Transactions = new Transactions()
						{
							NumberOfTxIds = MaximumNumberOfIDsPerRequest
						}
					};
					var outgoingInterchange = InterchangeCreator.CreateOutgoingInterchange(factory, applicationCode, CommonInterchangeTypeList.Codes.TransactionID, branchPK, webServiceEndPoint, IEXmlObjectSerializer.Serialize(transactionIDRequest), credentialPK: interchangeCredentialPK);
					SeedTransactionInventory(transactionType, companyPK, credentialPK, outgoingInterchange.EI_SessionGUID);
				}
				logger.Log(LogType.Information, string.Format("{0}: {1} TID message(s) created", companyName, noOfRequest));
			}
			return noOfRequest;
		}
		const int MaximumNumberOfIDsPerRequest = 100;

		void SeedTransactionInventory(ZString transactionType, ZGuid companyPK, ZGuid credentialPK, ZGuid sessionGUID)
		{
			var trackingReference = sessionGUID.ToString();
			for (var i = 0; i < MaximumNumberOfIDsPerRequest; i++)
			{
				var transactionNumber = factory.New<CusTransactionNumber>();
				transactionNumber.TN_Type = transactionType;
				transactionNumber.TN_GC_Company = companyPK;
				transactionNumber.TN_GP_ExternalPassword = credentialPK;
				transactionNumber.TN_TrackingReference = trackingReference;
			}
		}

		void GetTransactionsData(int noOfDays)
		{
			GatherTransactionsData(factory.Load<CusTransactionNumber>(TransactionIDCreator.GetNonExpiredTransactionNumberQuery(noOfDays, transactionType)));
			foreach (var companyData in transactionsAndMessagesData.Values)
			{
				foreach (var data in companyData.Values)
				{
					data.Average = data.TotalUsed / noOfDays;
					data.Pending = GetPendingTransactionCount(data.TrackingReferences);
				}
			}
		}

		void GatherTransactionsData(CusTransactionNumber[] transactionNumbers)
		{
			foreach (var transactionNumber in transactionNumbers)
			{
				var credentialPK = transactionNumber.TN_GP_ExternalPassword;
				var dictionary = transactionsAndMessagesData.GetOrAdd(credentialPK, () => new Dictionary<ZGuid, TransactionAndMessageData>());
				var companyPK = transactionNumber.TN_GC_Company;
				var transactionData = dictionary.GetOrAdd(companyPK, () => new TransactionAndMessageData());
				if (transactionNumber.TN_TransactionReference.IsEmpty)
				{
					var reference = transactionNumber.TN_TrackingReference;
					if (!reference.IsEmpty && ZGuid.TryParse(reference, out var trackingReference))
					{
						transactionData.TrackingReferences.Add(trackingReference);
					}
				}
				else if (transactionNumber.TN_IsUsed)
				{
					transactionData.TotalUsed++;
				}
				else
				{
					transactionData.AvailableTransactionIDs.Add(transactionNumber);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:DoNotUseFactory.GetDatabaseCount", Justification = "No need to load the business object collection just for the count. This is purely the pending transaction count.")]
		int GetPendingTransactionCount(HashSet<ZGuid> trackingReferences)
		{
			var result = 0;
			if (trackingReferences.Count > 0)
			{
				var query = new ZDBOnlyQuery(typeof(EDIInterchange));
				query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, transactionIDApplicationCode);
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, trackingReferences);
				query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
				var subQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.EI_SessionGUID, true);
				subQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, transactionIDApplicationCode);
				subQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, CommonInterchangeTypeList.Codes.TransactionID);
				subQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
				subQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, trackingReferences);
				subQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
				query.AddSubQuery(EDIInterchangeSchema.EI_SessionGUID, subQuery, JoinCondition.And);
				result = factory.GetDatabaseCount(typeof(EDIInterchange), query) * MaximumNumberOfIDsPerRequest;
			}
			return result;
		}

		void GatherMessagesThatNeedTransactionID(Dictionary<ZGuid, Dictionary<ZGuid, TransactionAndMessageData>> credentialDictionary, int messageProcessingNoOfDays)
		{
			var query = new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, messageApplicationCodes);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, ZString.Empty);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
			if (filterOnCredential)
			{
				query.AddToFilter(EDIMessageSchema.EM_GP, SQLComparisonOperator.NotEqual, null);
			}
			var messages = factory.Load<EDIMessage>(query);
			if (messages.Length > 0)
			{
				var lastDayForMessageSending = ZDateTime.UtcNow.AddDays(-messageProcessingNoOfDays);
				if (filterOnCredential)
				{
					foreach (var message in messages)
					{
						var credentialPK = message.EM_GP;
						if (message.EM_SystemCreateTimeUtc < lastDayForMessageSending || !IsCredentialValid(credentialPK))
						{
							DiscardMessageAndFailLogicalStatus(message);
							continue;
						}
						var companyDictionary = credentialDictionary.GetOrAdd(credentialPK, () => new Dictionary<ZGuid, TransactionAndMessageData>());
						var companyPK = message.Branch.GB_GC;
						var data = companyDictionary.GetOrAdd(companyPK, () => new TransactionAndMessageData());
						data.Messages.Add(message);
					}
				}
				else
				{
					var companyDictionary = credentialDictionary.GetOrAdd(ZGuid.Empty, () => new Dictionary<ZGuid, TransactionAndMessageData>());
					foreach (var message in messages)
					{
						var companyPK = message.Branch.GB_GC;
						var credentialPK = GetCredentialPKFromCompanyPK(companyPK);
						if (message.EM_SystemCreateTimeUtc < lastDayForMessageSending || !IsCredentialValid(credentialPK))
						{
							DiscardMessageAndFailLogicalStatus(message);
							continue;
						}
						var data = companyDictionary.GetOrAdd(companyPK, () => new TransactionAndMessageData());
						data.Messages.Add(message);
					}
				}
			}
		}

		ZGuid GetCredentialPKFromCompanyPK(ZGuid companyPK)
		{
			if (!companyPKToCredentialPKMapping.TryGetValue(companyPK, out var credentialPK))
			{
				credentialPK = ((IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(factory.Load<GlbCompany>(companyPK)))?.GetGlbExternalPassword()?.PK ?? ZGuid.Empty;
				companyPKToCredentialPKMapping.Add(companyPK, credentialPK);
			}
			return credentialPK;
		}
	}
}
