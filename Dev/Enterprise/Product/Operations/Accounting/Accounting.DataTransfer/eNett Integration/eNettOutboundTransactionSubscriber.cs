using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using WTG.StaticAnalysis.Annotation;
using Events = Enterprise.ZArchitecture.Business.Events;
using Res = Enterprise.Accounting.DataTransfer.Res;

namespace Enterprise.BatchProcessor.Accounting
{
	[Serializable]
	public class eNettOutboundTransactionSubscriber : LogSubscriber
	{
		public eNettOutboundTransactionSubscriber()
		{
			Integrator = "CARGOWISE";
			IntegratorKey = AccountingConfigurationRegistry.Instance.ENettIntegratorKey.Value;
		}

		[Immutable]
		class eNettOutboundTransactionSubscriberLogBatcher : ILogBatcher
		{
			public static eNettOutboundTransactionSubscriberLogBatcher Instance { get; } = new eNettOutboundTransactionSubscriberLogBatcher();

			eNettOutboundTransactionSubscriberLogBatcher() { }

			LogsGroupContext ILogBatcher.SetContextForLogsGroup(object groupKey, IEnumerable<IQueuedLog> queuedLogs) => new LogsGroupContext(false);

			void ILogBatcher.AddGroupingFetchHints(IEnumerable<IQueuedLog> enumerable) { }

			object ILogBatcher.GetGroupLogKey(IQueuedLog log) => log.PK;
		}

		#region Properties

#if DEBUG
		public eNettWebServiceWrapper EnettWebServiceWrapper_ForTestOnly { get => enettWebServiceWrapper_ForTestOnly; private set => enettWebServiceWrapper_ForTestOnly = value; }

		[NonSerialized]
		eNettWebServiceWrapper enettWebServiceWrapper_ForTestOnly;

		public eNettWebServiceFailedEmail EnettWebServiceFailedEmail_ForTestOnly { get => enettWebServiceFailedEmail_ForTestOnly; private set => enettWebServiceFailedEmail_ForTestOnly = value; }

		[NonSerialized]
		eNettWebServiceFailedEmail enettWebServiceFailedEmail_ForTestOnly;

		public BusinessObjectFactory Factory_ForTest { get => factory_ForTest; private set => factory_ForTest = value; }

		[NonSerialized]
		BusinessObjectFactory factory_ForTest;

		public class eNettBatchingUpdateEventArgs : EventArgs
		{
			public ZGuid BatchHeaderPK { get; set; }
			public int SJ_RetryCount { get; set; }
		}

		static readonly object actionLock = new object();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static Action<object, eNettBatchingUpdateEventArgs> eNettBeforeBatchingCommit;

		public static void SeteNettBeforeBatchingCommit(Action<object, eNettBatchingUpdateEventArgs> action)
		{
			lock (actionLock)
			{
				eNettBeforeBatchingCommit = action;
			}
		}

#endif

		public readonly string Integrator;
		public readonly string IntegratorKey;

		#endregion

		#region Overrides

		public override string[] TableNames
		{
			get { return new string[] { AccTransactionHeaderSchema.Constants.TableName, AccTransactionMatchLinkSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code, Events.DirectDebitOutboundSubscriberBatchingRecordUpdate.Code }; }
		}

		public override string Name
		{
			get { return "eNettTransactionSender"; }
		}

		protected override ILogBatcher GetLogBatcher()
		{
			return eNettOutboundTransactionSubscriberLogBatcher.Instance;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error report does not need to be translated")]
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			LogInfoToFile($"\tStart ProcessLogQueueItems (Size of {queuedLogs.Length})");

			var firstFactory = queuedLogs.Select(log => log?.Factory).FirstOrDefault();
			if (firstFactory != null)
			{
				firstFactory.SetContext(Enterprise.Integration.Accounting.BusinessContext.eNettOutboundSubscriberLWKServiceTask);
				var companiesWithComPayConfiguration = GetCompaniesConfiguredForComPay(firstFactory);
				if (companiesWithComPayConfiguration.Any())
				{
					foreach (IQueuedLog queuedLog in queuedLogs)
					{
						LogInfoToFile($"\tStart Processing QueuedLog {queuedLog.SJ_ParentID}");
						try
						{
							Process(queuedLog, companiesWithComPayConfiguration);
						}
						catch (Exception e) when (e is WebException || e is SocketException || e is InvalidOperationException)
						{
							LogErrorToFile(e.ToString());
							ErrorReporter.Instance.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an exception", e);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							LogErrorToFile(e.ToString());
							ErrorReporter.Instance.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an unhandled exception", e);
						}
						LogInfoToFile($"\tEnd Processing QueuedLog {queuedLog.SJ_ParentID}");
					}
				}
			}
			LogInfoToFile($"\tEnd ProcessLogQueueItems (Size of {queuedLogs.Length})");
		}

		protected override ExceptionHandlingResult TryHandleExceptionCore(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Exception Occurred in ProcessLogQueueItems; retryCount:{retryCount}, SJ_ParentID Values:");
			foreach (var queueLog in logs)
			{
				stringBuilder.AppendLine(queueLog.SJ_ParentID.ToString());
			}
			LogErrorToFile(stringBuilder.ToString());
			LogErrorToFile(ex.ToString());

			return ExceptionHandlingResult.Unhandled;
		}

		IEnumerable<GlbCompany> GetCompaniesConfiguredForComPay(BusinessObjectFactory factory)
		{
			ZDBOnlyQuery companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			ZDBOnlySubQuery registrySubQuery = new ZDBOnlySubQuery(typeof(StmData), StmDataSchema.SD_Owner);
			registrySubQuery.AddToFilter(StmDataSchema.SD_Name, "ENETTREGISTRATION");
			companyQuery.AddSubQuery(registrySubQuery, JoinCondition.And);
			return factory.Load<GlbCompany>(companyQuery);
		}

#if DEBUG

		protected virtual
#endif
		void Process(IQueuedLog queuedLog, IEnumerable<GlbCompany> companiesWithComPayConfiguration)
		{
			if (!eNettHelper.ENettMessageAlreadyExists(queuedLog.Factory, queuedLog.SJ_ParentID))
			{
				var eNettService = eNettWebServiceWrapper.CreateNewWebService();
				eNettWebServiceWrapper wrapper = new eNettWebServiceWrapper(eNettService, false, queuedLog.Factory);

#if DEBUG
				Factory_ForTest = queuedLog.Factory;
				EnettWebServiceWrapper_ForTestOnly = wrapper;
#endif

				WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
				eNettService.Proxy = WebRequest.DefaultWebProxy;

				if (queuedLog.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					ProcessTransactionHeader(queuedLog, eNettService, companiesWithComPayConfiguration);
				}
				else if (queuedLog.SJ_ParentTableCode == AccTransactionMatchLinkSchema.Constants.Prefix)
				{
					ProcessTransactionMatchLink(queuedLog, wrapper, companiesWithComPayConfiguration);
				}
			}
			else if ((queuedLog.SJ_SE_NKEvent == Events.DirectDebitOutboundSubscriberBatchingRecordUpdate.Code) && (queuedLog.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix))
			{
				ProcessTransactionHeaderUpdateBatchingAfterPayment(queuedLog);
			}
		}

		void ProcessTransactionHeaderUpdateBatchingAfterPayment(IQueuedLog queuedLog)
		{			
			LogInfoToFile($"Adding Payment to DDR Batch:\tPayment PK:{queuedLog.SJ_ParentID}");
			var header = queuedLog.Factory.Load<TransactionHeader>(queuedLog.SJ_ParentID);
			var sj_Reference = queuedLog.SJ_Reference;
			if (sj_Reference.StartsWith("Paid|") && header.AH_ReceiptBatchNo.IsEmpty)
			{
				var parts = sj_Reference.Split('|');
				if ((parts.Length > 0) && DateTime.TryParseExact(parts[1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var batchDate))
				{
					var headerAsPayment = header.Factory.Load<Payment>(header.PK);
					var manager = new eNettPaymentBatchManager(header.Factory);
					manager.AddToExistingBatchOrCreateNewBatch(headerAsPayment, batchDate, false);
#if DEBUG
					lock (actionLock)
					{
						if (eNettBeforeBatchingCommit != null)
						{
							var args = new eNettBatchingUpdateEventArgs()
							{
								BatchHeaderPK = queuedLog.SJ_ParentID,
								SJ_RetryCount = queuedLog.SJ_RetryCount
							};
							eNettBeforeBatchingCommit.Invoke(this, args);
						}
					}
#endif
				}
			}
		}

		void ProcessTransactionHeader(IQueuedLog queuedLog, IeNettWebServiceClient eNettService, IEnumerable<GlbCompany> companiesWithComPayConfiguration)
		{
			TransactionHeader header = queuedLog.Factory.Load<TransactionHeader>(queuedLog.SJ_ParentID);
			if (header != null && header.Branch != null)
			{
				GlbCompany company = queuedLog.Factory.Load<GlbCompany>(header.Branch.GB_GC);
				if (companiesWithComPayConfiguration.Contains(company))
				{
					using (new TemporaryUserContext() { BranchPK = header.AH_GB.ToGuid(), DepartmentPK = header.AH_GE.ToGuid() }.Set())
					{
						if (!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty)
						{
							switch (header.AH_TransactionType)
							{
								case TransactionTypes.Invoice:
									if (!header.AH_IsCancelled &&
										header.AH_Ledger == LedgerTypes.AccountsReceivable &&
										IsTransactionValidForENett(header))
									{
										CreateInvoice(eNettService, header);
									}
									break;

								case TransactionTypes.CreditNote:
									if (header.AH_IsCancelled && header.AH_Ledger == LedgerTypes.AccountsReceivable)
									{
										ZQuery query = new ZQuery(AccTransactionHeaderSchema.PK, header.AH_TransactionBelongsToGroup);
										query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, header.AH_Ledger);
										TransactionHeader originalInvoice = queuedLog.Factory.LoadTop1<TransactionHeader>(query);
										if (originalInvoice != null &&
											IsTransactionValidForENett(header) && eNettHelper.ENettMessageHasBeenSucessfullySent(queuedLog.Factory, originalInvoice.PK))
										{
											CancelInvoice(eNettService, originalInvoice);
										}
									}
									break;

								case TransactionTypes.Payment:
									if (!header.AH_IsCancelled &&
										header.AH_ReceiptType == ReceiptTypes.eNettDirectDebit &&
										IsPaymentValidForENett(header) &&
										(queuedLog.SJ_RetryCount <= 1))
									{
										LogInfoToFile($"Process Payment:\tHeader PK:{header.PK}");
										using (var mutex = StorageFeeInvoicePayment.GetCOMPayMutex(header.PK))
										{
											if (!mutex.IsLocked)
											{
												ProcessDirectDebit(eNettService, header);
											}
										}
									}
									else
									{
										LogInfoToFile($"Skipping ProcessDirectDebit\tHeader PK:{header.PK};SJ_RetryCount:{queuedLog.SJ_RetryCount}");
									}
									break;
							}
						}
					}
				}
			}
		}

		void ProcessTransactionMatchLink(IQueuedLog queuedLog, eNettWebServiceWrapper wrapper, IEnumerable<GlbCompany> companiesWithComPayConfiguration)
		{
			AccTransactionMatchLink matchLink = queuedLog.Factory.Load<AccTransactionMatchLink>(queuedLog.SJ_ParentID);
			if (matchLink != null)
			{
				TransactionHeader transaction = queuedLog.Factory.Load<TransactionHeader>(matchLink.AP_AH);
				GlbCompany company = queuedLog.Factory.Load<GlbCompany>(transaction.Branch.GB_GC);
				if (transaction.Branch != null && companiesWithComPayConfiguration.Contains(company))
				{
					using (new TemporaryUserContext { BranchPK = transaction.AH_GB.ToGuid(), DepartmentPK = transaction.AH_GE.ToGuid() }.Set())
					{
						if (!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty)
						{
							var isValidActiveReceivableInvoice = transaction.AH_TransactionType == TransactionTypes.Invoice
								&& transaction.AH_Ledger == LedgerTypes.AccountsReceivable
								&& !transaction.AH_IsCancelled
								&& IsTransactionValidForENett(transaction);

							var doProcessOfflinePayment = isValidActiveReceivableInvoice
								&& eNettHelper.ENettMessageHasBeenSucessfullySent(queuedLog.Factory, transaction.PK)
								&& !IsENettInvoicePaidByENettPayment(matchLink, transaction);
							if (doProcessOfflinePayment)
							{
								wrapper.ProcessOfflinePayment(matchLink, transaction);
							}
						}
					}
				}
			}
		}

		#endregion

		#region Implementation

		bool IsENettInvoicePaidByENettPayment(AccTransactionMatchLink matchLink, TransactionHeader transaction)
		{
			string sql = @"SELECT TOP 1 AP_AH FROM dbo.AccTransactionMatchLink JOIN
                                        dbo.AccTransactionHeader ON AP_AH = AH_PK JOIN
                                        dbo.GlbBranch ON AH_GB = GB_PK
                                        WHERE
                                        AP_MatchGroupNum = @MatchGroupNum AND
                                        GB_GC = @Company AND
                                        (AH_TransactionType = @PaymentTransactionType AND
                                                AH_ReceiptType = @eNettPaymentType)";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@MatchGroupNum", matchLink.AP_MatchGroupNum, AccTransactionMatchLinkSchema.AP_MatchGroupNum);
			@params.Add("@Company", transaction.Branch.Company.PK, GlbBranchSchema.GB_GC);
			@params.Add("@PaymentTransactionType", TransactionTypes.Receipt, AccTransactionHeaderSchema.AH_TransactionType);
			@params.Add("@eNettPaymentType", ReceiptTypes.eNettDirectCredit, AccTransactionHeaderSchema.AH_ReceiptType);
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(transaction.Factory);
			collection.Load(sql, @params);
			return collection.Count != 0;
		}

		bool IsTransactionValidForENett(TransactionHeader header)
		{
			if (header.Header != null)
			{
				return !header.Header.ENettRegistrationNumber.IsEmpty &&
					!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty;
			}
			else
			{
				return false;
			}
		}

		bool IsPaymentValidForENett(TransactionHeader header)
		{
			return eNettHelper.IsOrganisationeNettRegistered(header.Header) ||
				eNettHelper.DoesOrgHaveeNettDDRAccount(header.Header);
		}

		void InsertMessageIntoDatabase(string messageSubType, bool isSuccess, string xml, ZGuid branchPK, ZGuid departmentPK, ZGuid transactionPK, BusinessObjectFactory factory)
		{
			EDIMessage message = factory.New<eNettEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageType = "ENE";
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = isSuccess ? EDIMessage.Status.Sent : EDIMessage.Status.Failed;
			message.EM_MessageText = xml;
			message.EM_GB = branchPK;
			message.EM_GE = departmentPK;
			message.EM_LinkTable = AccTransactionHeader.Schema.TableName;
			message.EM_LinkUniqueID = transactionPK;
		}

		bool CreateInvoice(IeNettWebServiceClient eNettService, TransactionHeader header)
		{
			bool result = false;
			ZString payLoad = SerializeInvoice(header);
			try
			{
				Response_CreateInvoice response = eNettService.CreateInvoice(Integrator,
																			 IntegratorKey,
																			 "1",
																			 "desc",
																			 "123",
																			 AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
																			 header.Header.ENettRegistrationNumber,
																			 eNettHelper.GetBrokersENettRegistrationNumber(header),
																			 header.TransactionNumberPrefixed,
																			 "",
																			 payLoad,
																			 AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);
				result = response.success;

				if (!result)
				{
					var eNettWebServiceFailedEmail = new eNettWebServiceFailedEmail("CreateInvoice", response.receivedDateTime, response.errorCode, response.errorMessage, false, header.Factory);
#if DEBUG
					EnettWebServiceFailedEmail_ForTestOnly = eNettWebServiceFailedEmail;
#endif
					eNettWebServiceFailedEmail.Send();
				}
				else
				{
					Globals.Message.Show(Res.GetString("5f4adcc2-5546-438b-8be6-0b3ee51882fa", "eNett Create Invoice Success"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.CreateNewInvoice, result, payLoad, header.AH_GB, header.AH_GE, header.PK, header.Factory);
			}

			return result;
		}

		bool CancelInvoice(IeNettWebServiceClient eNettService, TransactionHeader header)
		{
			bool result = false;

			try
			{
				Response_CancelInvoice response = eNettService.CancelInvoice(Integrator,
																			 IntegratorKey,
																			 "1",
																			 header.AH_Desc,
																			 header.AH_TransactionNum,
																			 AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
																			 header.Header.ENettRegistrationNumber,
																			 header.AH_TransactionNum,
																			 "",
																			 "",
																			 "",
																			 AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);

				result = response.success;

				if (!result)
				{
					var eNettWebServiceFailedEmail = new eNettWebServiceFailedEmail("CancelInvoice", response.processedDateTime, response.errorCode, response.errorMessage, false, header.Factory);
#if DEBUG
					EnettWebServiceFailedEmail_ForTestOnly = eNettWebServiceFailedEmail;
#endif
					eNettWebServiceFailedEmail.Send();
				}
				else
				{
					Globals.Message.Show(Res.GetString("0573d906-146e-449c-8749-d283b2005eaf", "eNett Cancel Invoice Notification Success"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.CancelInvoice, result, string.Empty, header.AH_GB, header.AH_GE, header.PK, header.Factory);
			}

			return result;
		}

		bool ProcessDirectDebit(IeNettWebServiceClient eNettService, TransactionHeader header)
		{
			bool result = false;
			string responseTextForEdiMessage = string.Empty;

			string payLoad = SerializePayment(header);
			Response_ProcessDirectDebit response = null;
			try
			{
				if (eNettHelper.IsOrganisationeNettRegistered(header.Header))
				{
					response = eNettService.ProcessDirectDebit(
						new ProcessDirectDebitDTO(
							integrator: Integrator,
							integratorKey: IntegratorKey,
							version: "1",
							sourceDescription: header.AH_Desc,
							integratorRef: header.AH_TransactionNum,
							fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
							fromAccountBSB: header.BankAccount.AB_BSB,
							fromAccountNo: header.BankAccount.AB_AccountNum,
							toClient: header.Header.ENettRegistrationNumber,
							toAccountName: "",
							toAccountBSB: "",
							toAccountNo: "",
							invoiceNo: "",
							invoiceID: "",
							amount: header.AH_OSTotalAmount,
							currency: header.AH_RX_NKTransactionCurrency,
							sendRemittance: true,
							integratorXML: payLoad,
							lineItems: null,
							authorised: true,
							adviceCCEmail: "",
							createdBy: "",
							notes: "",
							paymentDate: header.AH_InvoiceDate.ToDateTime(),
							integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode
							)
						);
					result = response.success;
				}
				else if (eNettHelper.DoesOrgHaveeNettDDRAccount((header.Header)))
				{
					AccAPAccountDetails accountDetails = eNettHelper.GetEnettBankAccountDetails(header.Header);
					response = eNettService.ProcessDirectDebit(
						new ProcessDirectDebitDTO(
							integrator: Integrator,
							integratorKey: IntegratorKey,
							version: "1",
							sourceDescription: header.AH_Desc,
							integratorRef: header.AH_TransactionNum,
							fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
							fromAccountBSB: header.BankAccount.AB_BSB,
							fromAccountNo: header.BankAccount.AB_AccountNum,
							toClient: "",
							toAccountName: accountDetails.A1_BankName,
							toAccountBSB: accountDetails.A1_BankBsb,
							toAccountNo: accountDetails.A1_BankAccount,
							invoiceNo: "",
							invoiceID: "",
							amount: header.AH_OSTotalAmount,
							currency: header.AH_RX_NKTransactionCurrency,
							sendRemittance: true,
							integratorXML: payLoad,
							lineItems: null,
							authorised: true,
							adviceCCEmail: "",
							createdBy: "",
							notes: "",
							paymentDate: header.AH_InvoiceDate.ToDateTime(),
							integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode
							)
						);
					result = response.success;
				}

				using (MemoryStream stream = new MemoryStream())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(response.GetType());
					serializer.Serialize(stream, response);
					stream.Flush();
					responseTextForEdiMessage = StreamConverter.StreamToString(stream);
				}

				if (response.success && header.AH_ReceiptBatchNo.IsEmpty)
				{
					CreateNewBatchEvent(header, response.batchDate);
				}

				if (!result)
				{
					var eNettWebServiceFailedEmail = new eNettWebServiceFailedEmail("ProcessDirectDebit", response.processedDateTime, response.errorCode, response.errorMessage, false, header.Factory);
#if DEBUG
					EnettWebServiceFailedEmail_ForTestOnly = eNettWebServiceFailedEmail;
#endif
					eNettWebServiceFailedEmail.Send();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogErrorToFile(ex.ToString());
				ErrorReporter.ReportOnce("ProcessDirectDebit Failed with Exception", ex);
				result = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.ProcessDirectDebit, result, payLoad, header.AH_GB, header.AH_GE, header.PK, header.Factory);
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.Response, result, responseTextForEdiMessage, header.AH_GB, header.AH_GE, header.PK, header.Factory);
			}

			return result;
		}

		void CreateNewBatchEvent(TransactionHeader header, DateTime batchDate)
		{
			var stmALog = header.Factory.New<StmALog>();
			using (((IUpdateFieldsLock)stmALog).LockForUpdatingKeyFields())
			{
				stmALog.SL_Table = AccTransactionHeader.Schema.TableName;
				stmALog.SL_Parent = header.PK;
				stmALog.SL_IsEstimate = false;
				stmALog.SL_Reference = $"Paid|{batchDate:yyyy-MM-dd}";
				stmALog.SL_GS_NKUser = "E";
				stmALog.SL_SE_NKEvent = Events.DirectDebitOutboundSubscriberBatchingRecordUpdate.Code; 
				stmALog.SL_FireWorkflow = false;
			}
		}

		string SerializePayment(TransactionHeader header)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				eNettPaymentDataAdapter dataAdapter = new eNettPaymentDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				serializer.ExportXmlData(stream, dataAdapter, new BusinessObject[] { header }, new ValueObjectExportContext(new NotificationBuffer()));

				stream.Flush();

				string result = StreamConverter.StreamToString(stream);
#if DEBUG
				result = result.Replace(User.SupportUserName, "username");
#endif
				return result;
			}
		}

		string SerializeInvoice(TransactionHeader header)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				eNettOutboundFinancialInvoiceDataAdapter dataAdapter = new eNettOutboundFinancialInvoiceDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				serializer.ExportXmlData(stream, dataAdapter, new BusinessObject[] { header }, new ValueObjectExportContext(new NotificationBuffer()));

				stream.Flush();

				string result = StreamConverter.StreamToString(stream);
#if DEBUG
				result = result.Replace(User.SupportUserName, "username");
#endif
				return result;
			}
		}

		void LogErrorToFile(string message)
		{
			var notifications = GetINotificationsWrapperAroundILogger();
			notifications.AddError(message);
		}
		void LogInfoToFile(string message)
		{
			var notifications = GetINotificationsWrapperAroundILogger();
			notifications.AddInfo(message);
		}

		#endregion

	}
}
