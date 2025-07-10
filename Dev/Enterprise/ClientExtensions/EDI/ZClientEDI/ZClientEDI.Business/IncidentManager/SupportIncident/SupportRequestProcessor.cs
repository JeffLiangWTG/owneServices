using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Constants = Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public interface ISupportRequestProcessor
	{
		void Process(Xsd.CustomerServiceRequest request, bool fromEHub);
		void ProcessNewEdocs(BusinessObjectFactory factory, ZGuid incidentRequestPk, IEnumerable<ZGuid> eDocPks);
	}

	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage()]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class SupportRequestProcessor : IEmailAttachmentProcessor, ISupportRequestProcessor
	{
		public SupportRequestProcessor(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		public SupportRequestProcessor()
			: this(null)
		{
		}

		public const string DeemedResolvedLogReference = "This eRequest has been deemed resolved";
		public const string DeemedResolvedLogERequestPortal = "This eRequest has been deemed resolved - eRequest Management Portal";
		public const string DeemedResolvedLogEmail = "This eRequest has been deemed resolved - Email";

		public ILogger ServiceLogger { get; private set; }

		#region Process

		public void ProcessAllWebUpdates()
		{
			ProcessAllNewWebRequests();
			ProcessAllNewWebMessages();
			ProcessAllWebStatusUpdates();
			ProcessERequestDocumentQueue();
		}

		public void ProcessAllWebStatusUpdates()
		{
			bool keepGoing = true;
			while (keepGoing)
			{
				keepGoing = ProcessBatchOfWebStatusUpdates();
			}
		}

		bool ProcessBatchOfWebStatusUpdates()
		{
			bool anyProcessedWithoutError = false;
			var batchFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZDBOnlyQuery(typeof(SupportIncident));
			query.AddFilterAndZSQLParameterCollection(@"
IM_PK IN
(
	SELECT top 100 IM_PK FROM dbo.EdiViewIncidentStatusChange
)", new ZSqlParameterCollection());

			var batch = batchFactory.Load<SupportIncident>(query);

			foreach (var incident in batch)
			{
				ServiceLogger.Information(incident.IM_IncidentNumber + " processing status change");
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				// reload in new factory to ensure we have the latest data
				anyProcessedWithoutError |= ProcessWebStatusUpdateAndHandleErrors(factory.Load<SupportIncident>(incident.PK));
			}
			return anyProcessedWithoutError;
		}

		bool ProcessWebStatusUpdateAndHandleErrors(SupportIncident incident)
		{
			if (incident == null)
			{
				return true;
			}

			try
			{
				ProcessWebStatusUpdate(incident);
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Error(incident.IM_IncidentNumber + " failed to update status ", ex);
				ErrorReporter.ReportOnce("Failed to update status", incident.IM_IncidentNumber + " failed to update status " + ex.Message, ex);
				return false;
			}
		}

		void ProcessWebStatusUpdate(SupportIncident incident)
		{
			var snapShotBeforeUpdate = new SupportIncidentStatusSnapShot();
			snapShotBeforeUpdate.TakeSnapShot(incident, true);

			var request = incident.Request;
			var newCustomerStatus = request.INC_Status;
			var originalStatus = incident.IM_RequestStatus;

			incident.SetCriticalityWithoutLoggingReason(request.INC_Criticality);
			UpdateDates(newCustomerStatus, incident);

			var statusChanged = newCustomerStatus != originalStatus;
			if (statusChanged)
			{
				UpdateStatusAndDisposition(newCustomerStatus, incident);
				CloseIncidentIfRequired(newCustomerStatus, incident, request);
				if (!incident.HasChanges)
				{
					// Customer was able to change to a status that we don't currently have any processing for.
					// For now, set it back to the original value.
					ServiceLogger.Warning(incident.IM_IncidentNumber + " customer status change discarded " + newCustomerStatus + ", set to original " + originalStatus);
					request.INC_Status = originalStatus;
				}
			}

			incident.CustomerNotifier = new CustomerChangeIncidentCustomerNotifier(incident, new WebRequestNotificationSender());
			incident.Factory.Save();

			SendEmails(incident, snapShotBeforeUpdate);
		}

		void CloseIncidentIfRequired(ZString newCustomerStatus, SupportIncident incident, EdiIncidentRequest request)
		{
			var requestDeemResolvedLogQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, DeemedResolvedLogReference);
			requestDeemResolvedLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MiscellaneousEventCode);
			var requestDeemResolvedLog = request.Logs.Find(requestDeemResolvedLogQuery).OrderByDescending(x => x.SL_PostedTimeUtc).ToArray().FirstOrDefault();
			if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.Closed && requestDeemResolvedLog != null)
			{
				if (incident.IM_ClosureResolution.IsEmpty)
				{
					var resolvedTimeUtcOverride = ZDateTime.Empty;

					if (incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
					{
						var latestAwaitingResponseEvent = incident.GetLatestAwaitingResponseEvent();

						if (latestAwaitingResponseEvent != null)
						{
							resolvedTimeUtcOverride = latestAwaitingResponseEvent.SL_EventTimeUtc;
						}
					}

					var registryValue = incident.Lookups.GetClosureDispositionList(activeOnly: true, incident.IM_Category, incident.IM_Priority, incident.IM_Product);
					var isSRSValid = registryValue.ContainsCode(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved);
					incident.CloseIncident(isSRSValid ? SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved : SupportIncidentLookups.DispositionList.Constants.Closed.Completed, string.Empty, resolvedTimeUtcOverride);

					if (incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.Resolved)
					{
						incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
					}
				}
				else
				{
					var logText = IncidentConstants.LogFreeText.CloseSkipResolvedChangedUserLog;
					var statusUpdatedDescription = FormattableString.Invariant($"{logText} {GlbStaff.CurrentUser.GS_Code}");
					incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
					incident.AddStatusUpdatedEvent(statusUpdatedDescription);
				}

				if (requestDeemResolvedLog.SL_Reference.Contains(DeemedResolvedLogERequestPortal))
				{
					incident.AddPublicSystemLogMessage("This eRequest was closed via the confirm resolved action button on the eRequest Management Portal");
				}

				if (requestDeemResolvedLog.SL_Reference.Contains(DeemedResolvedLogEmail))
				{
					incident.AddPublicSystemLogMessage("This eRequest was closed via the confirm resolved action button on email");
				}
			}
		}

		void SendEmails(SupportIncident incident, SupportIncidentStatusSnapShot snapShotBeforeUpdate)
		{
			SendCustomiseRequestEmailToProductManager(incident, snapShotBeforeUpdate);
			SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, snapShotBeforeUpdate, messagesFromCustomer: null, shouldSaveEmail: true, excludeEmails: null, shouldSendIncidentUpdateEmail: true);
		}

		public static void SendCustomiseRequestEmailToProductManager(SupportIncident incident, SupportIncidentStatusSnapShot snapShotBeforeUpdate)
		{
			if (incident != null
				&& snapShotBeforeUpdate.IM_Priority != Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest
				&& incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest
				&& incident.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest
				&& incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate)
			{
				var template = new ProductManagerCustomizeRequestEmailContentBuilder(incident);
				var email = EDIEmailBuilder.GetInstance(incident).BuildHtmlEmailDefByTemplate(template);

				if (email != null)
				{
					email.FromAddress = SupportIncident.SupportEmailAddress;
					email.FromDisplayName = SupportIncident.SupportDisplayName;
					try
					{
						if (incident.ProductAreaAssignedStaff != null)
						{
							email.AddRecipientForUserCommunication(incident.ProductAreaAssignedStaff.GS_EmailAddress);
							Env.OutgoingMailManager.CreateAndSave(email);
						}
						else
						{
							Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.IncidentSupportGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.IncidentSupportGroup));
						}
					}
					catch (EmailSendFailedException)
					{
					}
				}
			}
		}

		public static void UpdateDates(ZString newCustomerStatus, SupportIncident incident)
		{
			if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.FormalQuotationRequested && incident.Estimate.CIE_QuoteRequestedUTC.IsEmpty)
			{
				incident.Estimate.CIE_QuoteRequestedUTC = ZDateTime.UtcNow;
			}
			else if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.FormalQuotationAccepted && incident.Quote.CIQ_QuoteAcceptedDateUTC.IsEmpty)
			{
				incident.Quote.CIQ_QuoteAcceptedDateUTC = ZDateTime.UtcNow;
			}
		}

		public static void UpdateStatusAndDisposition(ZString newCustomerStatus, SupportIncident incident)
		{
			if (incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest)
			{
				incident.ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging();
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FeatureAccepted, incident);
				if (incident.WorkflowItems.AllTasksClosedOrCancelled)
				{
					incident.AddPublicSystemLogMessage("Feature Request is created and auto closed by eRequest processor.");
					incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "");
				}
			}
			else if (incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
			{
				if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested)
				{
					if (incident.IM_Category != SupportIncidentCategoriesList.Codes.FeatureRequest)
					{
						incident.ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging();
					}
					incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
				}
				else if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.FormalQuotationRequested)
				{
					incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
					IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FormalQuotationRequested, incident);
					incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation;
				}
				else if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.FormalQuotationAccepted)
				{
					incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
					IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FormalQuotationAccepted, incident);
					incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted;
				}
				else if (newCustomerStatus == SupportIncidentLookups.LegacyStatusCodes.FormalQuotationDeclined)
				{
					incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
					IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FormalQuotationDeclined, incident);
					incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined;
				}
			}
		}

		public bool ProcessNewWebRequest(IncidentRequest request)
		{
			var reportedBy = request.ReportedBy;

			if (reportedBy == null)
			{
				request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.Closed;
				ServiceLogger.Warning(request.INC_IncidentNumber + " Reported By is blank. Closing and ignoring since this should be impossible.");
				request.Factory.Save();
				return true;
			}

			request.INC_Status = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;
			var incident = request.Factory.New<SupportIncident>();
			PopulateIncidentFromRequest(incident, request);
			var approvedBy = request.ApprovedBy;
			string incidentReportingLog = string.Format(CultureInfo.InvariantCulture, "This incident was reported by {0} ({1}) and approved by {2} ({3})",
				reportedBy.OC_ContactName,
				reportedBy.OC_Email,
				approvedBy?.OC_ContactName ?? ZString.Empty,
				approvedBy?.OC_Email ?? ZString.Empty);

			try
			{
				incident.AddPublicSystemLogMessage(incidentReportingLog);
			}
			catch (InvalidOperationException e) when (e.Message.Equals(JobConversation.SqlLockExceptionMessage))
			{
				incident.Delete();
				ServiceLogger.Warning("The incident could not be processed as its Job Conversation is already being processed in another instance");
				return false;
			}

			SetStageStatusAndEventsForNewIncident(incident);
			using (incident.SetTempContext(SupportIncident.Context.NewWebRequest))
			{
				using (SetTemporaryUserContextForInternalLicence(incident))
				{
					incident.CustomerNotifier = new CustomerChangeIncidentCustomerNotifier(incident, new WebRequestNotificationSender());
					SavingFactoryForTest?.Invoke(incident.Factory);
					incident.Factory.Save();
				}
			}
			SendInternalEmailsForNewIncident(incident, reportedBy.OC_ContactName);
			AddFollowUpLogsAndRelatedItemsIfRequired(request, incident);

			return true;
		}

		static public void PopulateIncidentFromRequest(SupportIncident incident, IncidentRequest request)
		{
			incident.IM_INC_Request = request.PK;
			incident.IM_Product = request.INC_Type;
			incident.SetClientOnly(request.ReportedBy.OC_OH);
			incident.IM_OC_Contact = request.INC_OC_ReportedBy;
			incident.IM_Description = request.INC_Summary;
			incident.SetLongDetailNoteText(ConvertLineFeedToNewLine(request.INC_Details));
			incident.IM_Module = request.INC_SubType;
			incident.IM_Language = !request.INC_Language.IsEmpty ? request.INC_Language : new ZString(Core.SharedConstants.Languages.English);
			incident.IM_Priority = request.INC_Criticality;
			incident.IM_SourceModuleId = request.INC_Area;
			incident.IM_RN_NKCountry = request.INC_RN_NKCountry;
			incident.IM_ServiceType = request.INC_ServiceType;
			if (request.INC_SystemCreateUser == EDIDataRegistry.Instance.ISAlertsServiceAccount.Value)
			{
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundInternal;
			}
			else
			{
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			}
			PopulateLicence(incident, request.INC_ProductLicence);
			if (!request.INC_RN_NKCountry.IsEmpty)
			{
				incident.IM_RN_NKCountry = request.INC_RN_NKCountry;
			}

			// Fill in mandatory fields
			if (incident.IM_Module.IsEmpty)
			{
				var list = incident.Lookups.ModuleListEnabledModulesOnly;
				if (list.Count > 0)
				{
					incident.IM_Module = list[0].Code;
				}
			}
		}

		void AddFollowUpLogsAndRelatedItemsIfRequired(IncidentRequest request, SupportIncident incident)
		{
			//Must happen post first save of SupportIncident so the incident number is populated
			var followUpEvent = request.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Contains(FollowUpIncidentCreatedDescription));

			if (followUpEvent == null)
			{
				return;
			}

			var success = followUpEvent.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, out var originalIncidentNumber);

			if (!success)
			{
				return;
			}

			var originalIncidentQuery = new ZDBOnlyQuery(typeof(SupportIncident));
			var originalIncidentRequestQuery = new ZDBOnlySubQuery(typeof(IncidentRequest), IncidentMainSchema.IM_INC_Request);
			originalIncidentRequestQuery.AddToFilter(IncidentRequestSchema.INC_IncidentNumber, originalIncidentNumber);
			originalIncidentQuery.AddSubQuery(originalIncidentRequestQuery, JoinCondition.And);
			var originalIncident = incident.Factory.LoadTop1<SupportIncident>(originalIncidentQuery);

			if (originalIncident == null)
			{
				ErrorReporter.ReportOnce("Incident number does not match log", FormattableString.Invariant($"Current Incident Request={request.INC_IncidentNumber}|Original Incident Request: {originalIncidentNumber}"));
				return;
			}

			if (incident.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Contains(FollowUpIncidentCreatedDescription)))
			{
				//events have already been added
				return;
			}

			var originalIncidentFollowUpParamList = new List<KeyValuePair<string, string>>();
			originalIncidentFollowUpParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FollowUpIncidentCreatedDescription));
			originalIncidentFollowUpParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, incident.IM_IncidentNumber));

			originalIncident.Logs.AddNew(
				AutoEvents.MiscellaneousEvent,
				originalIncidentFollowUpParamList.ToArray()
			);

			var followUpIncidentParamList = new List<KeyValuePair<string, string>>();
			followUpIncidentParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FollowUpIncidentCreatedDescription));
			followUpIncidentParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalIncident.IM_IncidentNumber));

			incident.Logs.AddNew(
				AutoEvents.MiscellaneousEvent,
				followUpIncidentParamList.ToArray()
			);

			incident.RelatedItems.Add(originalIncident);

			incident.Factory.Save();
		}

		const string FollowUpIncidentCreatedDescription = "Follow up incident created";

		static IDisposable SetTemporaryUserContextForInternalLicence(SupportIncident incident)
		{
			var isInternalLicence = false;
			if (incident.Database != null)
			{
				isInternalLicence = incident.Database.LicEnterprise?.LE_IsInternal ?? false;
			}
			else if (incident.Client is EDIOrgHeader client)
			{
				isInternalLicence = client?.LicCompany?.LicEnterprise?.LE_IsInternal ?? false;
			}
			if (!isInternalLicence)
			{
				return null;
			}

			var contact = incident.Contact;
			if (contact == null || contact.OC_Email.IsEmpty)
			{
				return null;
			}

			var staffQuery = new ZQuery(GlbStaffSchema.GS_EmailAddress, contact.OC_Email);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsDevice, false);
			var staffList = incident.Factory.Load<GlbStaff>(staffQuery);
			if (staffList.Length > 0)
			{
				var staff = staffList.Length > 1 ? staffList.FirstOrDefault(x => x.GS_PER == contact.OC_PER) ?? staffList[0] : staffList[0];
				var userContext = new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK, null, true, incident.Factory);
				return Env.SetTemporaryUserContext(userContext);
			}

			return null;
		}

		static void PopulateLicence(SupportIncident incident, ZString licenceCode)
		{
			if (licenceCode.Length != 9 && licenceCode.IndexOf('.') < 0)
			{
				incident.UpdateLicenceFromClient(false);
				return;
			}

			if (licenceCode.IndexOf('.') > 0)
			{
				var ids = licenceCode.Split('.');
				if (ids.Length != 2)
				{
					return;
				}

				var enterpriseId = ids[0];
				var dbId = ids[1];
				var dbNumber = Base27Encoding.Decode(dbId);
				var dbQuery = new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, dbNumber);
				var db = incident.Factory.LoadTop1<LicenceDatabase>(dbQuery);
				if (db != null && db.EnterpriseID == enterpriseId)
				{
					incident.SetDatabaseOnly(db);
				}
			}
			else if (licenceCode.Length == 9)
			{
				var dbQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
				dbQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, licenceCode.Substring(6));

				var entQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
				entQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, licenceCode.Substring(0, 3));

				dbQuery.AddSubQuery(entQuery, JoinCondition.And);

				var db = incident.Factory.LoadTop1<LicenceDatabase>(dbQuery);
				if (db != null)
				{
					incident.SetDatabaseOnly(db);

					var companyQuery = new ZDBOnlyQuery(typeof(ClientCompany));
					companyQuery.AddToFilter(ClientCompanySchema.LCC_Code, licenceCode.Substring(3, 3));
					companyQuery.AddToFilter(ClientCompanySchema.LCC_LD, db.PK);

					var clientCompany = incident.Factory.LoadTop1<ClientCompany>(companyQuery);
					if (clientCompany != null)
					{
						incident.SetClientCompanyOnly(clientCompany);
					}
					else
					{
						if (incident.IM_Description.Contains("CG6 Test"))
						{
							var cliCompanyQueryLD = new ZQuery();
							cliCompanyQueryLD.AddToFilter(ClientCompanySchema.LCC_Code, licenceCode.Substring(3, 3));
							var cliCompanyLD = incident.Factory.LoadTop1<ClientCompany>(cliCompanyQueryLD);
							var cliCompanyLccLd = cliCompanyLD == null ? "" : cliCompanyLD.LCC_LD.ToString();

							var companyQueryServerCode = new ZQuery();
							companyQueryServerCode.AddToFilter(ClientCompanySchema.LCC_LD, db.PK);
							var clientCompanyServerCode = incident.Factory.LoadTop1<ClientCompany>(companyQueryServerCode);
							var cliCompanyServerCodeLccLd = clientCompanyServerCode == null ? "" : clientCompanyServerCode.LCC_LD.ToString();
							var cliCompanyServerCode = clientCompanyServerCode == null ? "" : clientCompanyServerCode.LCC_Code.ToString();

							var licenceEnterprise = incident.Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, licenceCode.Substring(0, 3)));
							var licenceEnterprisePK = licenceEnterprise == null ? "" : licenceEnterprise.PK.ToString();
							var licenceEnterpriseEnterpriseCode = licenceEnterprise == null ? "" : licenceEnterprise.LE_EnterpriseCode.ToString();
							ErrorReporter.ReportOnce("Failed to get correct clientCompany.", $"LicenceDatabase PK:{db.PK} client Company LCC_LD:{cliCompanyLccLd} client Company LCC_LD:{cliCompanyServerCodeLccLd} client Company ServerCode:{cliCompanyServerCode}\r\n licenceEnterprise PK:{licenceEnterprisePK} licenceEnterprise EnterpriseCode:{licenceEnterpriseEnterpriseCode}");
						}
					}
				}
			}
		}

		bool ProcessNewWebRequestAndHandleErrors(IncidentRequest request)
		{
			try
			{
				return ProcessNewWebRequestWithRetry(request);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var msg = request.INC_IncidentNumber + " failed to process new request";
				ServiceLogger.Error(msg, ex);
				ErrorReporter.ReportOnce(msg, ex);
				SendInternalNotification(msg, ex.ToString());
				return false;
			}
		}

		bool ProcessNewWebRequestWithRetry(IncidentRequest requestInReadFactory)
		{
			IncidentRequest request = null;
			const int MaxTryCount = 3;
			for (int tryCount = 1; tryCount <= MaxTryCount; ++tryCount)
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				if (request == null)
				{
					request = factory.ImportFromAnotherFactorySafe(requestInReadFactory);
				}
				else
				{
					request = factory.Load<IncidentRequest>(requestInReadFactory.PK);
				}

				try
				{
					return ProcessNewWebRequest(request);
				}
				catch (ZSaveException) when (tryCount < MaxTryCount)
				{
				}
			}

			return false;
		}

		void SendInternalNotification(string subject, string body)
		{
			try
			{
				EmailDef mail = new EmailDef();
				mail.Subject = subject;

				var regItem = EDIDataRegistry.Instance.InternalNotificationGroup;
				mail.Body = body + System.Environment.NewLine + System.Environment.NewLine
					+ "You are receiving this email because you are a member of the group in registry " + FullName(regItem);

				Env.OutgoingMailManager.CreateAndSave(mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
		}

		static string FullName(IRegistryItem regItem)
		{
			return string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;
		}

		bool ProcessBatchOfNewWebRequests()
		{
			bool anyProcessedWithoutError = false;
			var batchFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(IncidentRequestSchema.INC_Status, SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent);
			query.MaximumRows = 100;
			var batch = batchFactory.Load<IncidentRequest>(query);
			foreach (var request in batch)
			{
				ServiceLogger.Information(request.INC_IncidentNumber + " processing new");
				anyProcessedWithoutError |= ProcessNewWebRequestAndHandleErrors(request);
			}
			return anyProcessedWithoutError;
		}

		public void ProcessAllNewWebRequests()
		{
			bool keepGoing = true;
			while (keepGoing)
			{
				keepGoing = ProcessBatchOfNewWebRequests();
			}
		}

		#region Portal Messages

		public void ProcessAllNewWebMessages()
		{
			MessageInfo msg = null;
			while (null != (msg = GetNextNewMessage()))
			{
				if (!msg.IsLegacy)
				{
					ProcessNewWebMessageWithRetry(msg.Pk);
				}

				DeleteMessageFromQueue(msg);
			}
		}

		void ProcessNewWebMessageWithRetry(Guid messagePk)
		{
			const int MaxTryCount = 3;
			for (int tryCount = 1; tryCount <= MaxTryCount; ++tryCount)
			{
				try
				{
					ProcessNewWebMessage(messagePk);
					return;
				}
				catch (ZSaveException ex)
				{
					if (tryCount == MaxTryCount)
					{
						ServiceLogger.Error("Failed to process new web message " + messagePk, ex);
					}
				}
			}
		}

		void ProcessNewWebMessage(Guid messagePk)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var msg = factory.Load<JobConversationMessage>(messagePk);
			if (msg == null)
			{
				return;
			}

			var request = (IncidentRequest)msg.Conversation.Parent;
			if (request == null)
			{
				return;
			}

			var incident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request.PK));
			if (incident == null)
			{
				return;
			}

			HandleNewMessages(incident, new[] { msg }, true);
		}

		void HandleNewMessages(SupportIncident incident, IEnumerable<IConversationMessage> messages, bool save)
		{
			SupportIncidentStatusSnapShot snapShotBeforeUpdate = new SupportIncidentStatusSnapShot();
			snapShotBeforeUpdate.TakeSnapShot(incident, true);

			incident.ManagementGroup?.ProcessIncidentMessageReceived(incident);
			if (!incident.IsGroupControlled)
			{
				incident.EnsureOpenTask(isLegacyReopenRequest: false, isByNewMessage: true, isByNewEmail: false);
			}
			incident.IncidentManagementLink?.TrySendGroupAutoReply();

			incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
			SavingFactoryForTest?.Invoke(incident.Factory);
			foreach (JobConversationMessage conversationMessage in messages)
			{
				if (incident.IM_SystemLastEditTimeUtc < conversationMessage.JCM_SystemCreateTimeUtc)
				{
					incident.IM_SystemLastEditTimeUtc = conversationMessage.JCM_SystemCreateTimeUtc;
				}
			}
			if (save)
			{
				incident.Factory.Save();
			}

			SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, snapShotBeforeUpdate, messages, shouldSaveEmail: save, excludeEmails: null, shouldSendIncidentUpdateEmail: true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		internal event BusinessObjectFactory.SavingEventHandler SavingFactoryForTest;

		sealed class MessageInfo
		{
			public MessageInfo(Guid pk, DateTime postedTimeUtc, bool isLegacy)
			{
				Pk = pk;
				PostedTimeUtc = postedTimeUtc;
				IsLegacy = isLegacy;
			}

			public readonly Guid Pk;
			public readonly DateTime PostedTimeUtc;
			public readonly bool IsLegacy;
		}

		MessageInfo GetNextNewMessage()
		{
			const string sql =
@"select top 1 JCQ_JCM, JCQ_PostedTimeUtc, IsLegacy = case when ELC_PK is null then cast(0 as bit) else cast(1 as bit) end
from dbo.EdiIncidentConversationMessageQueue
left join dbo.EdiLegacyConversationMessage on ELC_JCM_Message = JCQ_JCM
order by JCQ_PostedTimeUtc
";
			using (var cmd = Db.Connection.Command(sql))
			{
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var pk = reader.GetGuid(0);
						var utc = reader.GetDateTime(1);
						var isLegacy = reader.GetBoolean(2);
						return new MessageInfo(pk, utc, isLegacy);
					}
				}
			}

			return null;
		}

		void DeleteMessageFromQueue(MessageInfo messageInfo)
		{
			const string sql =
@"delete from dbo.EdiIncidentConversationMessageQueue where JCQ_PostedTimeUtc = @JCQ_PostedTimeUtc and JCQ_JCM = @JCQ_JCM;
if @IsLegacy = 1
	delete from dbo.EdiLegacyConversationMessage where ELC_JCM_Message = @JCQ_JCM;
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@JCQ_PostedTimeUtc", System.Data.SqlDbType.DateTime, messageInfo.PostedTimeUtc);
				cmd.AddParameter("@JCQ_JCM", System.Data.SqlDbType.UniqueIdentifier, messageInfo.Pk);
				cmd.AddParameter("@IsLegacy", System.Data.SqlDbType.Bit, messageInfo.IsLegacy);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region EdiERequestDocumentQueue

		public void ProcessERequestDocumentQueue()
		{
			var dynamicCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			dynamicCollection.Load(
@"SELECT INC_PK, EDQ_PK
FROM dbo.EdiERequestDocumentQueue
JOIN dbo.IncidentRequest ON EDQ_INC_ReferenceID = INC_ReferenceID
WHERE INC_ReferenceID IS NOT NULL
ORDER BY INC_SystemCreateTimeUtc;");

			foreach (DynamicBusinessObject obj in dynamicCollection)
			{
				ProcessERequestDocumentQueueSafe((ZGuid)obj[IncidentRequestSchema.PK], (ZGuid)obj[EdiERequestDocumentQueueSchema.PK]);
			}

			Db.Connection.ExecuteNonQuery("DELETE dbo.EdiERequestDocumentQueue WHERE EDQ_SystemCreateTimeUtc < DATEADD(wk, -1, GETUTCDATE());");
		}

		void ProcessERequestDocumentQueueSafe(ZGuid incidentRequestPk, ZGuid eRequestDocPk)
		{
			var factory = new BusinessObjectFactory();
			var incidentRequest = factory.Load<IncidentRequest>(incidentRequestPk);
			var requestDoc = factory.Load<EdiERequestDocumentQueue>(eRequestDocPk);

			if (incidentRequest != null && requestDoc != null)
			{
				var fileName = requestDoc.EDQ_FileName;
				try
				{
					var attachedFile = incidentRequest.DocManagerInfo.AddFileOrDocument(requestDoc.EDQ_Data, requestDoc.EDQ_FileName, "COR");
					attachedFile.IsPublished = requestDoc.EDQ_IsPublished;
					requestDoc.Delete();
					BusinessObjectFactory.SaveTogether(factory, incidentRequest.DocManagerInfo.MasterFactory);

					ServiceLogger.Information(FormattableString.Invariant($"{attachedFile.FileName} added to {incidentRequest.INC_IncidentNumber} successfully."));
				}
				catch (ZSaveConcurrencyException)
				{
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var failedToAddFileMessage = FormattableString.Invariant($"Failed to add {fileName} to {incidentRequest.INC_IncidentNumber}");
					ServiceLogger.Error(failedToAddFileMessage, ex);
					ErrorReporter.ReportOnce("Failed to add file to incident", failedToAddFileMessage, ex);
				}
			}
		}

		#endregion

		public void Process(string xmlData)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));
			StringReader reader = new StringReader(xmlData);
			Xsd.CustomerServiceRequest request = (Xsd.CustomerServiceRequest)serializer.Deserialize(reader);
			Process(request, false);
		}

		public void Process(Xsd.CustomerServiceRequest request, bool fromEHub)
		{
			if (request.Action == SupportIncidentLookups.LegacyActions.Update)
			{
				return;
			}

			string status = null;

			if (request.ClientReferenceNumber.IsEmpty || !IsDuplicate(request))
			{
				// Fixup: XML deserialization of strings converts "\r\n" to "\n"
				request.IncidentDetails = ConvertLineFeedToNewLine(request.IncidentDetails);
				status = ProcessNew(request, fromEHub);
			}
			else
			{
				status = "Incident already exists. Nothing to do.";
			}

			if (ServiceLogger != null)
			{
				string msg = string.Format(CultureInfo.CurrentCulture, "Request: {0}, Incident: {1}, Licence: {2}, Approving User: \"{3}\" <{4}>, Status: {5}",
					request.ClientReferenceNumber,
					Incident != null && !Incident.PreventSaving ? Incident.IM_IncidentNumber : ZString.Empty,
					request.LicenceCode,
					request.ApprovingUser,
					Incident != null && Incident.Contact != null ? Incident.Contact.OC_Email : ZString.Empty,
					status);
				ServiceLogger.Log(LogType.Information, msg);
			}
		}

		static string ConvertLineFeedToNewLine(string text)
		{
			return text.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
		}

		string ProcessNew(Xsd.CustomerServiceRequest request, bool fromEHub)
		{
			string status = string.Empty;

			// 5 attempts with 2s intervals, 120 total timeout
			var retryHandler = new RetryHandler("2s;2s;2s;2s", "120s");
			try
			{
				retryHandler.Invoke(() =>
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					factory.SuspendValidation();
					Incident = CreateIncident(factory, request, out status);
					if (fromEHub)
					{
						Incident.IM_Source = SupportIncidentLookups.SourceListConstants.APIInboundExternal;
					}
					var sender = ERequestCustomerNotificationSender.CreateSenderFromXmlRequest(Incident, fromEHub);
					var notifier = new CustomerChangeIncidentCustomerNotifier(Incident, sender);
					Incident.CustomerNotifier = notifier;
					SaveIncident(factory);
				});
			}
			catch (ZSaveConcurrencyException ex)
			{
				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Warning, "Error creating incident", ex);
				}
				throw;
			}

			SendInternalEmailsForNewIncident(Incident, request.ReportingStaffMemberName);

			return status;
		}

		public static string SetStageStatusAndEventsForNewIncident(SupportIncident incident)
		{
			string status;

			if (incident.IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest)
			{
				incident.ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging();
				incident.IM_GS_NKCustServiceContact = ZString.Empty;
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.FeatureAccepted, incident);
				if (incident.WorkflowItems.AllTasksClosedOrCancelled)
				{
					incident.AddPublicSystemLogMessage("Feature Request is created and auto closed by eRequest processor.");
					incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "");
					status = "Feature Request created and closed.";
				}
				else
				{
					status = "Feature Request created.";
				}
			}
			else if (incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest)
			{
				incident.ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging(SupportIncidentLookups.Status.Working, SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate);
				incident.IM_GS_NKCustServiceContact = ZString.Empty;
				incident.Estimate.CIE_QuoteRequestedUTC = ZDateTime.UtcNow;
				IncidentEventFactory.TriggerEvent(IncidentEventFactory.Codes.DevelopmentEstimateRequested, incident);
				status = "Quote Request created.";
			}
			else
			{
				status = "Incident created.";
			}

			return status;
		}

		static SupportIncident CreateIncident(BusinessObjectFactory factory, Xsd.CustomerServiceRequest request, out string status)
		{
			status = string.Empty;

			SupportIncident incident = factory.New<SupportIncident>();
			ServiceRequestDataAdapter adapter = new ServiceRequestDataAdapter();
			adapter.ImportFromValueObject(incident, request, new ValueObjectImportContext(factory, new NotificationBuffer()));
			ChangeModuleIfIsLegacy(incident);

			status = SetStageStatusAndEventsForNewIncident(incident);

			return incident;
		}

		static void ChangeModuleIfIsLegacy(SupportIncident incident)
		{
			if (incident.IM_Product == ProductTypes.Codes.Enterprise || incident.IM_Product == ProductTypes.Codes.EHub)
			{
				var moduleType = IncidentApprovalLookups.GetModuleListType(incident.IM_Priority);
				var legacyMapping =
					(moduleType == ModuleListType.MenuSection) ? EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetMapping(incident.IM_Module) :
					(moduleType == ModuleListType.Cr8) ? EDIDataRegistry.Instance.LegacyCr8ModuleMappings.Value.GetMapping(incident.IM_Module) :
					(moduleType == ModuleListType.Cr9) ? EDIDataRegistry.Instance.LegacyCr9ModuleMappings.Value.GetMapping(incident.IM_Module) :
					null;

				if (legacyMapping != null)
				{
					if (!string.IsNullOrEmpty(legacyMapping.CriticalityMapping))
					{
						incident.IM_Priority = legacyMapping.CriticalityMapping;
					}
					if (!string.IsNullOrEmpty(legacyMapping.CountryMapping))
					{
						incident.IM_RN_NKCountry = legacyMapping.CountryMapping;
					}

					incident.IM_Module = legacyMapping.ModuleMapping;
				}
			}
		}

		protected virtual void SaveIncident(BusinessObjectFactory factory)
		{
			factory.Save();
		}

		bool IsDuplicate(Xsd.CustomerServiceRequest request)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory() { RefreshEnabled = false };
			ZQuery query = GetIncidentQuery(factory, request);
			if (query != null)
			{
				return factory.ExistsInDatabase(SupportIncident.Schema.TableName, query);
			}
			return false;
		}

		ZQuery GetIncidentQuery(BusinessObjectFactory factory, Xsd.CustomerServiceRequest request)
		{
			ZQuery query = null;

			LicenceDatabase database = null;
			LicenceHeader clientLicence = LicenceHeader.LoadFromLicenceCode(factory, request.LicenceCode);
			if (clientLicence != null)
			{
				database = clientLicence.Database;
			}
			else
			{
				database = factory.LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, request.DatabaseNumber));
			}

			if (database != null)
			{
				const int MaxMonthsSinceIncidentModified = 3;
				query = new ZQuery();
				query.AddToFilter(IncidentMainSchema.IM_LD, database.PK);
				if (!request.IncidentNumber.IsEmpty)
				{
					query.AddToFilter(IncidentMainSchema.IM_IncidentNumber, request.IncidentNumber);
				}
				else
				{
					query.AddToFilter(IncidentMainSchema.IM_ClientIncidentReference, request.ClientReferenceNumber);
				}
				query.AddToFilter(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
				query.AddToFilter(IncidentMainSchema.IM_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-MaxMonthsSinceIncidentModified));
			}

			return query;
		}

		internal SupportIncident Incident;

		#endregion

		#region Notification Email

		void SendInternalEmailsForNewIncident(SupportIncident incident, string reportedByName)
		{
			try
			{
				SendCriticalIncidentEmail(incident, reportedByName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Catch all non-critical exceptions sending emails since the incident has been saved.
				// Any problems sending the emails or logging the sent emails
				// can be considered minor.

				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Warning, "Error sending emails", ex);
					ErrorReporter.ReportOnce("Error sending emails", ex.Message, ex);
				}
			}
		}

		static void SendCriticalIncidentEmail(SupportIncident incident, string reportedByName)
		{
			if (incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR1_SystemDown
				|| incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR2_ModuleDown
				|| incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround)
			{
				var template = new NewIncidentRaisedInternalEmailContentBuilder(incident, reportedByName);
				var email = EDIEmailBuilder.GetInstance(incident).BuildHtmlEmailDefByTemplate(template);
				if (email != null)
				{
					email.FromAddress = SupportIncident.SupportEmailAddress;
					email.FromDisplayName = SupportIncident.SupportDisplayName;

					try
					{
						Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup));
					}
					catch (EmailSendFailedException)
					{
					}
				}
			}
		}

		public void ProcessNewEdocs(BusinessObjectFactory factory, ZGuid incidentRequestPk, IEnumerable<ZGuid> eDocPks)
		{
			if (!eDocPks.Any())
			{
				return;
			}

			var request = factory.Load<IncidentRequest>(incidentRequestPk);
			var allEDocs = request.DocManagerInfo.AllEDocs;
			JobConversation convo = null;
			List<JobConversationMessage> newMsgs = null;

			foreach (var pk in eDocPks)
			{
				var eDoc = allEDocs.GetFromUniqueKey(pk.ToGuid());
				if (eDoc != null)
				{
					if (convo == null)
					{
						convo = JobConversation.GetOrCreate(request);
						newMsgs = new List<JobConversationMessage>();
					}

					string message = "Attached to eDocs: " + eDoc.FileName;
					var msg = convo.Messages.AddNew(null, message, false);
					msg.JCM_IsSystem = true;
					msg.JCM_IsLocal = false;
					msg.JCM_PostedTimeUtc = eDoc.DateAdded;
					newMsgs.Add(msg);
				}
			}

			if (newMsgs != null)
			{
				var incident = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_INC_Request, request.PK));
				if (incident != null)
				{
					HandleNewMessages(incident, newMsgs, false);
				}
			}
		}

		#endregion
	}
}

