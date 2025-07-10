using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public interface IReportDataService
	{
		void DeleteConfiguration(Guid reportId, string name);
		void SaveConfiguration(SelectedValueConfigurationData configurationData);
		List<ConfigurationData> GetConfigurations(Guid reportId);
		List<ReportSummaryData> GetReportSummaryCollection(string businessContext);
		List<ReportSummaryData> GetReportSummaryCollectionForContact(List<string> businessContexts);
		ReportData GetReportData(Guid id);
		ReportBinaryData GetReportBytes(SelectedValueReportData reportData);
		string GetDependencyValueForLookupFilter(Guid reportId, string filterName, string selectedValue);
		List<CodeDescription> GetDependencyValueForCodeListMultipleChoiceFilter(Guid reportId, string filterName, string selectedValue);
		List<CodeDescription> GetOnlinePrinters();
		List<CodeDescription> GetDeliveryMethods();
		List<CodeDescription> GetAttachmentTypes(Guid? reportId);
		List<CodeDescription> GetSalutations();
		void DeliverReport(DeliveryData deliveryData);
		List<SecurityRightNodeData> GetSecurityRights();
		List<CodeDescription> GetOrganisationRegistrationCodeTypes(string countryCode);
		IBusinessObjectCollection GetLookupData(LookupFilterSearchArgs searchParam, Func<ModuleIdentifier, IBusinessObjectCollection, LookupFilterSearchArgs, IBusinessObjectCollection> collectionLoader);

		void CalcStartDateOfAccountingPeriod(CalcStartDateOfAccountingPeriodData calcData);
		double GetUtcOffset();
		IGlbBranch GetBranch(Guid branchPk);
		List<IGlbStaff> GetPrintUsers(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery);
		List<CodeDescription> GetDeliverRecipientTypes();
		List<CodeDescription> GetScheduleDeliveryMethods();
		List<CodeDescription> GetScheduleAttachmentTypes(Guid reportId);
		List<CodeDescription> GetScheduleRecipientPrinters(Guid currentPrinterId);
		List<CodeDescription> GetBlankReportActivities();
		List<CodeDescription> GetEmailFromAddressList(Guid printUserId);
		List<IGlbStaff> GetStaffRecipients(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery);
		List<IGlbGroup> GetGroups(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery);
		List<CodeDescription> GetDeliveryContactNames(Guid organizationId);
		List<CodeDescription> GetScheduleContactNames(Guid organizationId);
		List<CodeDescription> GetContactEmails(string copyRecipientType, Guid organizationId);
		string GetContactEmail(string contactName, Guid organizationId);
		List<CodeDescription> GetCopyRecipientsEmails(string copyRecipientType, Guid organizationId);
		string GetDeliveryAddress(GetDeliveryAddressArgs args);
		List<ZGuid> GetRelatedOrganizationIDs(ZGuid contactPK);
		List<OrgHeader> GetRelatedOrganizations(ZGuid contactPK, ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery);
		DateScheduleData CalculateDateSchedule(DateScheduleData data, ReportScheduleTaskData reportScheduleTaskData);
		DateScheduleData GetDateSchedule(DateTime? storageValue, ReportScheduleTaskData reportScheduleTaskData);
		AccPeriodScheduleData CalculateAccPeriodSchedule(AccPeriodScheduleData data, ReportScheduleTaskData reportScheduleTaskData);
		AccPeriodScheduleData GetAccPeriodSchedule(DateTime? storageValue, ReportScheduleTaskData reportScheduleTaskData);
		void ScheduleReport(ReportScheduleData scheduleData);
		ReportScheduleData GetReportScheduleData(Guid reportScheduleTaskId);
		void DeleteReportScheduleTask(Guid reportScheduleTaskId);
		bool CheckPermissionForScheduleReport(DataOperations dataOperations, Guid reportScheduleTaskId);
		ReportRunningError RunningError { get; }
		void ClearRunningError();
	}

	public class ReportRunningError
	{
		public ReportServiceErrorType ErrorType { get; set; }
		public List<string> Errors { get; } = new List<string>();
	}

	public class ReportBinaryData
	{
		public string Name { get; set; }
		public FileType FileType { get; set; }
		public ZBlob Data { get; set; }
	}

	// This functionality is only for GLOW usage, please DO NOT use it in CW1.
	public class ReportDataService : IReportDataService
	{
		public ReportDataService() { }

		ReportRunningError runningError;
		public ReportRunningError RunningError => runningError ?? (runningError = new ReportRunningError());

		public void ClearRunningError() => runningError = null;

		public ZGuid? ContactPk
		{
			get => contactPk;
			set
			{
				contactPk = value;
				if (contactPk != null && (contactPk?.IsValid ?? false))
				{
					Contact = Factory.Load<OrgContact>((ZGuid)contactPk);
				}
				else
				{
					Contact = null;
				}
			}
		}

		ZGuid? contactPk;
		OrgContact Contact;

		ReportCommand GetReportCommand(Guid reportId, ReportScheduleTask scheduleTask = null)
		{
			var reportCommand = Factory.Load<ReportCommand>(reportId);
			if (reportCommand == null)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ValidationError, (NoResString)"The related Report does not exist in database.");
				return null;
			}

			if (!CanCurrentUserAccessReportCommand(reportCommand, scheduleTask))
			{
				return null;
			}

			return reportCommand;
		}

		public void DeleteConfiguration(Guid reportId, string name)
		{
			var reportCommand = GetReportCommand(reportId);
			if (reportCommand == null)
			{
				return;
			}

			using (var report = reportCommand.GetReport())
			{
				var configuration = report.ColumnHeadingManager.ConfigurationManagersForAllSavedConfigurations.FirstOrDefault(c => c.UniqueDescription == name);
				if (configuration == null)
				{
					RunningError.AddRunningError(ReportServiceErrorType.ValidationError, (NoResString)"The configuration for the related Report does not exist.");
					return;
				}

				configuration.Delete();
			}
		}

		public void SaveConfiguration(SelectedValueConfigurationData configurationData)
		{
			var reportCommand = GetReportCommand(configurationData.ReportId);
			if (reportCommand == null)
			{
				return;
			}

			using (var report = reportCommand.GetReport())
			{
				if (configurationData.LinkPk == Guid.Empty && report.LinkedLookupField != null)
				{
					RunningError.AddRunningError(ReportServiceErrorType.ValidationError, (NoResString)"Please provide a LinkPK.");
					return;
				}

				var configuration = report.FillAndReturnReportConfiguration(configurationData, RunningError);

				try
				{
					configuration?.Save(report.FilterCollection, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.Orientation, report.Language);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					RunningError.AddRunningError(ReportServiceErrorType.ConfigurationError, e.Message);
				}
			}
		}

		public List<ConfigurationData> GetConfigurations(Guid reportId)
		{
			var reportCommand = GetReportCommand(reportId);
			if (reportCommand == null)
			{
				return null;
			}

			return reportCommand.GetConfigurations();
		}

		public List<ReportSummaryData> GetReportSummaryCollection(string businessContext)
		{
			if (string.IsNullOrEmpty(businessContext))
			{
				RunningError.AddRunningError(ReportServiceErrorType.ValidationError, (NoResString)"Business context is mandatory for staff user.");
				return null;
			}

			var collection = new ReportCommandCollection(Factory, businessContext);
			collection.LoadApplicableReports();
			return collection.ConvertToReportSummaryDataCollection();
		}

		public List<ReportSummaryData> GetReportSummaryCollectionForContact(List<string> businessContexts)
		{
			var collection = WebReportHelper.GetAvailableWebReports(Factory, businessContexts.ToZStringList(), true, Contact.IsRightGrantedWithCheckingSecurityGroups);
			return collection.ConvertToReportSummaryDataCollection().Where(r => r.IsPublished).ToList();
		}

		// This functionality is only for GLOW usage, please DO NOT use it in CW1.
		public ReportData GetReportData(Guid id)
		{
			var reportCommand = GetReportCommand(id);
			if (reportCommand == null)
			{
				return null;
			}

			return reportCommand.GetReportData(RunningError);
		}

		// This functionality is only for GLOW usage, please DO NOT use it in CW1.
		public ReportBinaryData GetReportBytes(SelectedValueReportData reportData)
		{
			var reportCommand = GetReportCommand(reportData.Id);
			if (reportCommand == null)
			{
				return null;
			}

			var result = new ReportBinaryData { FileType = reportData.FileType };
			using (var report = reportCommand.GetReport())
			{
				result.Name = reportCommand.SU_MenuName;
				report.FillReportData(reportData);
				if (report.HasValidationErrors(RunningError))
				{
					return null;
				}

				byte[] binaryData;
				using (var stream = new MemoryStream())
				{
					try
					{
						report.Save(stream);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						RunningError.AddRunningError(ReportServiceErrorType.RunningError, e.Message);
						return null;
					}

					binaryData = stream.ToArray();
				}

				var fileType = reportData.FileType.ToString();
				var isFileTypeCorrect = Enum.TryParse<OutputFormatType>(fileType, true, out var outputFormatType);
				result.Data = DocumentConverter.ConvertFromExcel(binaryData, isFileTypeCorrect ? outputFormatType : OutputFormatType.XLS, ColourDepth.BlackAndWhite);
				if (!isFileTypeCorrect)
				{
					result.FileType = FileType.XLS;
				}
				return result;
			}
		}

		public string GetDependencyValueForLookupFilter(Guid reportId, string filterName, string selectedValue)
		{
			var reportCommand = GetReportCommand(reportId);
			if (reportCommand == null)
			{
				return null;
			}

			return reportCommand.GetDependencyValueForLookupFilter(filterName, selectedValue);
		}

		public List<CodeDescription> GetDependencyValueForCodeListMultipleChoiceFilter(Guid reportId, string filterName, string selectedValue)
		{
			var reportCommand = GetReportCommand(reportId);
			if (reportCommand == null)
			{
				return null;
			}

			return reportCommand.GetDependencyValueForCodeListMultipleChoiceFilter(filterName, selectedValue);
		}

		public List<CodeDescription> GetOnlinePrinters()
		{
			var result = new List<CodeDescription>();
			var printerCollection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, false);
			printerCollection.GetOnlinePrinterNames().CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetDeliveryMethods()
		{
			var result = new List<CodeDescription>();
			DeliveryContact.NotifyModes.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetAttachmentTypes(Guid? reportId)
		{
			if (reportId.HasValue)
			{
				var reportCommand = GetReportCommand(reportId.Value);
				if (reportCommand != null)
				{
					DeliveryContact.Initialise(reportCommand, null);
				}
			}

			var result = new List<CodeDescription>();
			((CodeDescriptionPairList)DeliveryContact.AttachmentTypes).CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetSalutations()
		{
			var result = new List<CodeDescription>();
			DeliveryContact.Salutations.CopyToCodeDescriptionList(result);
			return result;
		}

		public void DeliverReport(DeliveryData deliveryData)
		{
			var reportCommand = GetReportCommand(deliveryData.ReportData.Id);
			if (reportCommand == null)
			{
				return;
			}

			var printSet = new ReportPrintSet(reportCommand);
			var instructions = new DeliveryInstructions(printSet[0])
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};
			deliveryData.SyncToDeliveryInstructions(instructions, Contact);

			if (instructions.Recipients.Count == 0)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ValidationError, Res.GetString("19aececa-917d-4b1b-8a18-37d45bc62ddd", "You have not specified any recipients."));
				return;
			}

			using var report = printSet[0][0] as Report;
			report.PrepareForRender();
			if (report.HasProcessingErrors(RunningError))
			{
				return;
			}
			report.FillReportData(deliveryData.ReportData);

			if (report.LinkedLookupField != null && Contact?.ParentOrg != null)
			{
				report.LinkedLookupField.ZValue = Contact.ParentOrg.PK;
			}

			if (report.HasValidationErrors(RunningError) || instructions.HasValidationErrors(RunningError))
			{
				return;
			}

			try
			{
				if (Env.Registry.DeliverReportsInBackground)
				{
					var scheduleTask = instructions.Factory.New<ReportScheduleTask>();
					scheduleTask.SetScheduleFromDeliveryInstructions(instructions);
					if (Contact != null)
					{
						scheduleTask.S5_OC_ScheduledBy = Contact.PK;
					}
					if (!scheduleTask.HasValidationErrors(RunningError))
					{
						scheduleTask.Factory.Save();
					}
				}
				else
				{
					printSet.Run(instructions);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RunningError.AddRunningError(ReportServiceErrorType.DeliveryError, e.Message);
			}
		}

		bool CanContactViewReportScheduleTask(ReportScheduleTask scheduleTask)
		{
			return scheduleTask != null && (Contact.Header.Contacts.OfType<OrgContact>().Any(x => x.PK == scheduleTask.S5_OC_ScheduledBy) || scheduleTask.Recipients.OfType<StmScheduleTaskRecipient>().Any(r => r.S6_OC == Contact.PK));
		}

		internal bool CanCurrentUserAccessReportCommand(ReportCommand reportCommand, ReportScheduleTask scheduleTask = null)
		{
			var hasPermission = false;
			if (Contact != null)
			{
				if (Env.CurrentUser.IsWebUser)
				{
					if (CanContactViewReportScheduleTask(scheduleTask))
					{
						return true;
					}

					hasPermission = reportCommand.SU_IsPublished && reportCommand.SU_IsVisibleOnWeb && Contact.IsRightGrantedWithCheckingSecurityGroups(ReportsWebSecurityRights.GetSecurityRightForReport(reportCommand));
					if (!hasPermission)
					{
						RunningError.AddRunningError(ReportServiceErrorType.Unauthorized, (NoResString)"You do not have permission to access this resource. Please check if the report is published, or is visible on the web, or security granted for this user.");
					}
				}
			}
			else
			{
				if(!reportCommand.SU_IsPublished)
				{
					hasPermission = reportCommand.SU_SystemCreateUser == Env.CurrentUser.Initials;
					if (!hasPermission)
					{
						RunningError.AddRunningError(ReportServiceErrorType.Unauthorized, Res.GetString("9e4c907d-011f-47ae-b9e5-077f2c14e477", "You do not have permission to access this resource. Please contact your administrator to publish the report and grant permission to access."));
					}
				}
				else
				{
					var checkPoint = StmMenuItemBaseCheckpointFinder.FindSecurityCheckpointByPK(reportCommand.PK);
					hasPermission = checkPoint?.IsAllowed ?? true;

					if (!hasPermission)
					{
						RunningError.AddRunningError(ReportServiceErrorType.Unauthorized, Env.Security.GetErrorMessageForNotAllowedInGLOW((SecurityCheckpoint)checkPoint));
					}
				}
			}

			return hasPermission;
		}

		public List<SecurityRightNodeData> GetSecurityRights()
		{
			var securityRights = new List<SecurityRightNodeData>();
			var securityVector = new SecurityVector();
			securityVector.Initialise(Env.Security);

			foreach (var info in securityVector.Nodes)
			{
				var right = NewSecurityRightNodeData(info);
				securityRights.Add(right);
				AddSecurityRights(info, right);
			}

			return securityRights;
		}

		void AddSecurityRights(ISecurityInfo info, SecurityRightNodeData parentSecurity)
		{
			foreach (var child in info.Nodes)
			{
				var right = NewSecurityRightNodeData(child);
				parentSecurity.ChildRights.Add(right);
				AddSecurityRights(child, right);
			}
		}

		SecurityRightNodeData NewSecurityRightNodeData(ISecurityInfo info)
		{
			return new SecurityRightNodeData { Name = info.Name, Code = info.Checkpoint.Code.ToUpper(), ChildRights = new List<SecurityRightNodeData>() };
		}

		public List<CodeDescription> GetOrganisationRegistrationCodeTypes(string countryCode)
		{
			var result = new List<CodeDescription>();
			new OrgCodeLists().CustomsCodes_List(countryCode).CopyToCodeDescriptionList(result);
			return result;
		}

		public IBusinessObjectCollection GetLookupData(LookupFilterSearchArgs searchParam, Func<ModuleIdentifier, IBusinessObjectCollection, LookupFilterSearchArgs, IBusinessObjectCollection> collectionLoader)
		{
			try
			{
				var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, searchParam.LookupType);
				if (collectionProvider == null)
				{
					RunningError.AddRunningError(ReportServiceErrorType.LookupError, $"Unknown lookup type '{searchParam.LookupType}'");
					return null;
				}

				return collectionLoader?.Invoke(collectionProvider.ModuleID, collectionProvider.CollectionForFindbox, searchParam);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RunningError.AddRunningError(ReportServiceErrorType.LookupError, e.Message);
			}
			return null;
		}

		public double GetUtcOffset()
		{
			return StmScheduleTaskRecurrence.CurrentBranchUtcOffset;
		}

		public void CalcStartDateOfAccountingPeriod(CalcStartDateOfAccountingPeriodData calcData)
		{
			var scheduleTask = Factory.New<ReportScheduleTask>();
			scheduleTask.Recurrence.StartDateLocalForUser = calcData.StartDateLocal;
			if (calcData.IsCalculatedByDay)
			{
				scheduleTask.Recurrence.DayOfAccountingPeriod = calcData.DayOfAccountingPeriod;
			}

			calcData.StartDateLocal = scheduleTask.Recurrence.StartDateLocalForUser.ToDateTime();
			calcData.DayOfAccountingPeriod = scheduleTask.Recurrence.DayOfAccountingPeriod;
		}

		public IGlbBranch GetBranch(Guid branchPk)
		{
			return Factory.Load<IGlbBranch>(branchPk);
		}

		public List<IGlbStaff> GetPrintUsers(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery)
		{
			var task = Factory.New<ReportScheduleTask>();
			return FilterStaffs(task.Lookups.AllStaff, args, buildQuery, includeSystemAccounts: true);
		}

		public List<CodeDescription> GetDeliverRecipientTypes()
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			var result = new List<CodeDescription>();
			recipient.Lookups.DeliveryRecipientTypes.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetScheduleDeliveryMethods()
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			var result = new List<CodeDescription>();
			recipient.Lookups.NotifyModes.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetScheduleAttachmentTypes(Guid reportId)
		{
			var task = Factory.New<ReportScheduleTask>();
			task.S5_ParentID = reportId;
			var recipient = task.Recipients.AddNew();
			var result = new List<CodeDescription>();
			recipient.Lookups.AttachmentTypes.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetScheduleRecipientPrinters(Guid currentPrinterId)
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			recipient.S6_SQ = currentPrinterId;
			var result = new List<CodeDescription>();
			recipient.Lookups.PrintersWithParent.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetBlankReportActivities()
		{
			var blankReportActivities = new EmptyReportContingencyList();
			var result = new List<CodeDescription>();
			blankReportActivities.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetEmailFromAddressList(Guid printUserId)
		{
			var task = Factory.New<ReportScheduleTask>();
			task.UserFK = printUserId;
			var recipient = task.Recipients.AddNew();
			var result = new List<CodeDescription>();
			recipient.EmailFromAddressList.CopyToCodeDescriptionList(result, codeSelector: o => o.Description, descriptionSelector: o => o.Code);
			return result;
		}

		public List<IGlbStaff> GetStaffRecipients(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery)
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			return FilterStaffs(recipient.Lookups.Recipients, args, buildQuery);
		}

		List<IGlbStaff> FilterStaffs(GlbStaffCollection staffs, ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery, bool includeSystemAccounts = false)
		{
			var isSearchTermsInvalidGuid = ZGuid.TryParse(args.SearchTerms ?? string.Empty, out var pk) && !pk.IsValid;
			if (isSearchTermsInvalidGuid)
			{
				return new List<IGlbStaff>();
			}

			var query = new ZQuery(GlbStaffSchema.GS_IsSystemAccount, ZBool.False);
			if (includeSystemAccounts)
			{
				var whiteListSystemAccounts = new ZQuery(GlbStaffSchema.GS_Code, new[] { User.WebUserCode, User.SupportUserCode });
				query.AddToFilter(whiteListSystemAccounts, JoinCondition.Or);
			}
			if (!string.IsNullOrEmpty(args.SearchTerms))
			{
				buildQuery?.Invoke(query, typeof(GlbStaff), args.SearchTerms);
			}

			query.MaximumRows = args.Top;
			query.AddToFilter(staffs.AdditionalFilter);
			staffs.AdditionalFilter = query;

			return staffs.OfType<IGlbStaff>().ToList();
		}

		public List<IGlbGroup> GetGroups(ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery)
		{
			var isSearchTermsInvalidGuid = ZGuid.TryParse(args.SearchTerms ?? string.Empty, out var pk) && !pk.IsValid;
			if (isSearchTermsInvalidGuid)
			{
				return new List<IGlbGroup>();
			}

			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			var groups = recipient.Lookups.Groups;

			var query = new ZQuery();
			if (!string.IsNullOrEmpty(args.SearchTerms))
			{
				buildQuery?.Invoke(query, groups.TypeOfElements, args.SearchTerms);
			}

			query.MaximumRows = args.Top;
			groups.LoadWithMoreFiltering(query);

			return groups.OfType<IGlbGroup>().ToList();
		}

		public List<CodeDescription> GetDeliveryContactNames(Guid organizationId)
		{
			DeliveryContact.OrgHeaderPK = organizationId;
			return DeliveryContact.Contacts.Select(c => new CodeDescription() { Code = c.OC_ContactName.ToString(), Description = c.OC_Title.ToString() }).ToList();
		}

		public List<CodeDescription> GetScheduleContactNames(Guid organizationId)
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			recipient.S6_OH = organizationId;

			var result = new List<CodeDescription>();
			recipient.Lookups.ContactNames.CopyToCodeDescriptionList(result);
			return result;
		}

		public List<CodeDescription> GetContactEmails(string copyRecipientType, Guid organizationId)
		{
			DeliveryContact.OrgHeaderPK = organizationId;

			NonPersistentCopyRecipientCollection copyRecipients = default;
			switch (copyRecipientType.ToUpper())
			{
				case Core.Constants.CopyRecipientType.EmailToRecipient:
					copyRecipients = DeliveryContact.EmailToRecipients;
					break;
				case Core.Constants.CopyRecipientType.CarbonCopyRecipient:
					copyRecipients = DeliveryContact.EmailCarbonCopyRecipients;
					break;
				case Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient:
					copyRecipients = DeliveryContact.EmailBlindCarbonCopyRecipients;
					break;
				default:
					break;
			}

			var result = new List<CodeDescription>();
			if (copyRecipients != null)
			{
				var copyRecipient = copyRecipients.AddNew();
				copyRecipient.AvailableEmailAddressList.CopyToCodeDescriptionList(result);
			}

			return result.Where(pair => !string.IsNullOrEmpty(pair.Code)).ToList();
		}

		public string GetContactEmail(string contactName, Guid organizationId)
		{
			DeliveryContact.OrgHeaderPK = organizationId;
			DeliveryContact.Name = contactName;
			return DeliveryContact.Email;
		}

		public List<CodeDescription> GetCopyRecipientsEmails(string copyRecipientType, Guid organizationId)
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			recipient.S6_OH = organizationId;

			StmScheduleTaskCopyRecipientCollection copyRecipients = default;
			switch (copyRecipientType.ToUpper())
			{
				case Core.Constants.CopyRecipientType.EmailToRecipient:
					copyRecipients = recipient.EmailToRecipients;
					break;
				case Core.Constants.CopyRecipientType.CarbonCopyRecipient:
					copyRecipients = recipient.CarbonCopyRecipients;
					break;
				case Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient:
					copyRecipients = recipient.BlindCarbonCopyRecipients;
					break;
				default:
					break;
			}

			var result = new List<CodeDescription>();
			if (copyRecipients != null)
			{
				var copyRecipient = copyRecipients.AddNew();
				copyRecipient.Lookups.SCR_AvailableEmailAddress_List.CopyToCodeDescriptionList(result);
			}

			return result.Where(pair => !string.IsNullOrEmpty(pair.Code)).ToList();
		}

		public string GetDeliveryAddress(GetDeliveryAddressArgs args)
		{
			var recipient = Factory.New<ReportScheduleTaskRecipient>();
			recipient.S6_OH = args.OrganizationId;
			recipient.ContactName = args.ContactName;
			recipient.S6_GS_NKRecipient = args.StaffCode;
			recipient.S6_GG = args.GroupId;
			recipient.S6_EmptyReportDeliveryOptions = args.EmptyReportDeliveryOptions;
			recipient.S6_DeliveryToType = args.DeliveryToType;
			recipient.S6_DeliveryMethod = args.DeliveryMethod;

			return recipient.DeliveryAddress;
		}

		public List<ZGuid> GetRelatedOrganizationIDs(ZGuid contactPK)
		{
			var result = new List<ZGuid>();
			var contact = Factory.Load<OrgContact>(contactPK);
			if (contact != null)
			{
				result.Add(contact.ParentOrg.PK);
				result.AddRange(contact.ParentOrg.AllRelatedParties.Select(p => p.RelatedParty.PK));
			}
			return result;
		}

		public List<OrgHeader> GetRelatedOrganizations(ZGuid contactPK, ReportLookupSearchArgs args, Action<ZQuery, Type, string> buildQuery)
		{
			var query = new ZQuery();
			buildQuery?.Invoke(query, typeof(OrgHeader), args.SearchTerms);
			query.MaximumRows = args.Top;
			query.AddToFilter(new ZQuery(OrgHeaderSchema.PK, GetRelatedOrganizationIDs(contactPK)));

			var organizations = new ActiveBusinessObjectCollection<OrgHeader>(Factory);
			organizations.AdditionalFilter = query;

			return organizations.ToList();
		}

		public DateScheduleData CalculateDateSchedule(DateScheduleData data, ReportScheduleTaskData reportScheduleTaskData)
		{
			var scheduleTask = Factory.New<ReportScheduleTask>();
			scheduleTask.FillData(reportScheduleTaskData);
			var dateSchedule = new DateSchedule(scheduleTask);
			dateSchedule.FillData(data);
			if (!dateSchedule.IsValid)
			{
				RunningError.ErrorType = ReportServiceErrorType.ValidationError;
				RunningError.Errors.AddRange(dateSchedule.GetErrors().GetUniqueMessageList());
				return null;
			}
			return dateSchedule.ExtractData();
		}

		public DateScheduleData GetDateSchedule(DateTime? storageValue, ReportScheduleTaskData reportScheduleTaskData)
		{
			if (storageValue.HasValue && DateSchedule.TryParse(storageValue.Value, out var dateSchedule))
			{
				var scheduleTask = Factory.New<ReportScheduleTask>();
				scheduleTask.FillData(reportScheduleTaskData);
				dateSchedule.ScheduleTask = scheduleTask;
				return dateSchedule.ExtractData();
			}

			return null;
		}

		public AccPeriodScheduleData CalculateAccPeriodSchedule(AccPeriodScheduleData data, ReportScheduleTaskData reportScheduleTaskData = null)
		{
			var schedule = new AccPeriodSchedule();

			if (reportScheduleTaskData != null)
			{
				var scheduleTask = Factory.New<ReportScheduleTask>();
				scheduleTask.FillData(reportScheduleTaskData);
				schedule.ScheduleTask = scheduleTask;
			}

			schedule.FillData(data);
			if (!schedule.IsValid)
			{
				RunningError.ErrorType = ReportServiceErrorType.ValidationError;
				RunningError.Errors.AddRange(schedule.GetErrors().GetUniqueMessageList());
				return null;
			}
			return schedule.ExtractData();
		}

		public AccPeriodScheduleData GetAccPeriodSchedule(DateTime? storageValue, ReportScheduleTaskData reportScheduleTaskData)
		{
			if (storageValue.HasValue && AccPeriodSchedule.TryParse(storageValue.Value, out var accPeriodSchedule))
			{
				var scheduleTask = Factory.New<ReportScheduleTask>();
				scheduleTask.FillData(reportScheduleTaskData);
				accPeriodSchedule.ScheduleTask = scheduleTask;
				return accPeriodSchedule.ExtractData();
			}

			return null;
		}

		public void ScheduleReport(ReportScheduleData scheduleData)
		{
			if (Contact != null)
			{
				scheduleData.ScheduleTask.UserFk = Env.CurrentUserPK;
				scheduleData.Recipients.ForEach(recipient =>
				{
					recipient.DeliveryRecipientType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
					recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					if (recipient.S6_OH == Guid.Empty)
					{
						recipient.S6_OH = Contact.OC_OH.ToGuid();
						recipient.ContactName = Contact.OC_ContactName;
					}
				});
			}

			var scheduleTaskIdentifier = scheduleData.ScheduleTask.Identifier ?? ZGuid.Empty;
			var scheduleTask = scheduleTaskIdentifier.IsEmpty
				? Factory.New<ReportScheduleTask>()
				: Factory.Load<ReportScheduleTask>(scheduleTaskIdentifier);

			if (scheduleTask == null)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, ScheduleTaskNotExistError);
				return;
			}

			var dataOperations = scheduleTaskIdentifier.IsEmpty ? DataOperations.NEW : DataOperations.EDIT;
			if (!CheckPermissionForScheduleReport(scheduleTask, dataOperations))
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, Res.GetString("e86d9c3b-128d-4159-8761-0bbfb56a2c6b", "You do not have permissions to operate."));
				return;
			}

			var reportCommand = GetReportCommand(scheduleData.SelectedValueReportData.Id);
			if (reportCommand == null)
			{
				return;
			}

			scheduleTask.FillData(scheduleData.ScheduleTask);
			if (Contact != null)
			{
				scheduleTask.S5_OC_ScheduledBy = Contact.PK;
			}
			scheduleTask.FillRecipientsData(scheduleData.Recipients);

			if (!scheduleTask.S5_IsPrivate)
			{
				var pack = new DocumentPack(reportCommand);
				using var report = (Report)pack[0];
				if (report == null)
				{
					RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, Res.GetString("1340652c-a5cc-4100-ae9a-2d8d3929fbff", "The report does not contain any template. Please set the report again."));
					return;
				}

				report.SetScheduleTask(scheduleTask);

				report.PrepareForRender();
				if (report.HasProcessingErrors(RunningError))
				{
					return;
				}
				report.FillReportData(scheduleData.SelectedValueReportData);

				if (report.LinkedLookupField != null && Contact?.ParentOrg != null)
				{
					report.LinkedLookupField.ZValue = Contact.ParentOrg.PK;
				}

				var reportError = report.HasValidationErrors(RunningError);

				scheduleTask.S5_ParentID = scheduleData.SelectedValueReportData.Id;
				scheduleTask.S5_ScheduleState = scheduleTask.Serialize(report);

				if (reportError)
				{
					return;
				}
			}

			var scheduleError = scheduleTask.HasValidationErrors(RunningError);
			if (scheduleError)
			{
				return;
			}

			try
			{
				Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, e.Message);
			}
		}

		public bool CheckPermissionForScheduleReport(DataOperations dataOperations, Guid reportScheduleTaskId)
		{
			ReportScheduleTask scheduleTask = null;
			if (dataOperations != DataOperations.NEW)
			{
				if (reportScheduleTaskId != Guid.Empty)
				{
					scheduleTask = Factory.Load<ReportScheduleTask>(reportScheduleTaskId);
				}

				if (scheduleTask == null)
				{
					RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, ScheduleTaskNotExistError);
					return false;
				}
			}

			return CheckPermissionForScheduleReport(scheduleTask, dataOperations);
		}

		bool CheckPermissionForScheduleReport(ReportScheduleTask scheduleTask, DataOperations dataOperations)
		{
			if (Contact != null)
			{
				return dataOperations == DataOperations.NEW || Contact.PK == scheduleTask.S5_OC_ScheduledBy;
			}
			else
			{
				var checkpoint = dataOperations switch
				{
					DataOperations.NEW => Env.Security.ScheduledTaskNew,
					DataOperations.EDIT => scheduleTask.S5_SystemCreateUser == Env.CurrentUser.Initials ? Env.Security.ScheduledTaskEdit : Env.Security.ScheduledTaskEditOtherReport,
					DataOperations.DELETE => scheduleTask.S5_SystemCreateUser == Env.CurrentUser.Initials ? Env.Security.ScheduledTaskDelete : Env.Security.ScheduledTaskDeleteOtherReport,
					_ => throw new InvalidEnumArgumentException(nameof(dataOperations), (int)dataOperations, typeof(DataOperations)),
				};
				return checkpoint.IsAllowed;
			}
		}

		public ReportScheduleData GetReportScheduleData(Guid reportScheduleTaskId)
		{
			var scheduleTask = Factory.Load<ReportScheduleTask>(reportScheduleTaskId);
			if (scheduleTask == null)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, ScheduleTaskNotExistError);
				return null;
			}
			var reportCommand = GetReportCommand(scheduleTask.S5_ParentID.ToGuid(), scheduleTask);
			if (reportCommand == null)
			{
				return null;
			}
			var deserializedValue = scheduleTask.CreateReportFromTask();
			using var pack = new DocumentPack(reportCommand);
			pack.DeserializeDocPackFromReportCollection(deserializedValue.Report, null);
			var report = pack.GetFirstReport();
			report.DeserializedReport = deserializedValue.Report;
			report.PrepareForRender();

			report.ColumnHeadingManager.SaveLastSavedSetting(report);

			var data = new ReportScheduleData
			{
				ScheduleTask = scheduleTask.ExtractData(),
				Recipients = scheduleTask.ExtractRecipientsData(),
				SelectedValueReportData = report.GetSelectedValueReportData(),
				Configurations = report.GetConfigurations(),
				ReportData = reportCommand.GetReportData(RunningError),
			};

			return data;
		}

		public void DeleteReportScheduleTask(Guid reportScheduleTaskId)
		{
			if (reportScheduleTaskId == Guid.Empty)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, ScheduleTaskNotExistError);
				return;
			}
			var scheduleTask = Factory.Load<ReportScheduleTask>(reportScheduleTaskId);
			if (scheduleTask == null)
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, ScheduleTaskNotExistError);
				return;
			}

			if (!CheckPermissionForScheduleReport(scheduleTask, DataOperations.DELETE))
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, Res.GetString("c8b950df-cafc-4a6d-9ce8-31df2f11dbf8", "You do not have permission to delete the scheduled report."));
				return;
			}

			try
			{
				scheduleTask.Delete();
				Factory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RunningError.AddRunningError(ReportServiceErrorType.ScheduleError, e.Message);
			}
		}

		static string ScheduleTaskNotExistError => Res.GetString("4ea2b653-0053-4aef-8537-7b81a938e237", "The schedule task does not exist or have been deleted.");

		DocDeliveryContact DeliveryContact => deliveryContact ?? (deliveryContact = new DocDeliveryContact(Factory));
		DocDeliveryContact deliveryContact;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { NameForDebugging = "Report Data Service Factory" });

		BusinessObjectFactory factory;
	}
}
