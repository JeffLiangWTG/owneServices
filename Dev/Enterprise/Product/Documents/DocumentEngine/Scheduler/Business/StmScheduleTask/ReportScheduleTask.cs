using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

#if DEBUG
using Enterprise.ZArchitecture.Core.Testing;
#endif

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ReportScheduleTask : StmScheduleTask, Enterprise.Integration.DocumentEngine.IReportScheduleTask
	{
		public ReportScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			controllerID = ControllerIDs.ScheduledReports;
			businessObjectGuid = PK.ToGuid();
			businessObjectName = ReportScheduleTaskName;
		}

		static string ReportScheduleTaskName
		{
			get { return Res.GetString("1144AB13-BBFE-46B5-8890-6F3927FE6ADD", "Report Schedule"); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ScheduleType = ScheduleType;
			S5_ParentTableCode = StmMenuItemSchema.Constants.Prefix;
			S5_GS_NKPrintUser = GlbStaff.CurrentUser.GS_Code;
		}

		public const string ScheduleType = "REP";

		#region Lookups / Validation

		public new ReportScheduleTaskLookups Lookups => (ReportScheduleTaskLookups)base.Lookups;

		protected override StmScheduleTaskLookups GetNewLookups()
		{
			return new ReportScheduleTaskLookups(this);
		}

		public new ReportScheduleTaskValidation Validation => (ReportScheduleTaskValidation)base.Validation;

		protected override StmScheduleTaskValidation GetNewValidation()
		{
			return new ReportScheduleTaskValidation(this);
		}

		#endregion

		[ChildEditable(true)]
		public new ReportScheduleTaskRecipientDependentCollection Recipients => (ReportScheduleTaskRecipientDependentCollection)base.Recipients;

		public StmScheduleTaskRecipientDependentCollection ActiveRecipientsCollection => activeRecipientsCollection ??= GetActiveRecipientsCollection(Recipients);
		StmScheduleTaskRecipientDependentCollection activeRecipientsCollection;

		protected override StmScheduleTaskRecipientDependentCollection NewRecipients()
		{
			return new ReportScheduleTaskRecipientDependentCollection(this);
		}

		public override ZBool S5_IsActive
		{
			get => base.S5_IsActive;
			set
			{
				base.S5_IsActive = value;
				S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			}
		}

		public override ZGuid S5_ParentID
		{
			get => base.S5_ParentID;
			set
			{
				if (S5_ParentID != value)
				{
					hasReportBeenAnalyzed = false;
				}
				base.S5_ParentID = value;
				S5_ScheduleDescriptionInfo.RefreshBinding();
			}
		}

		public bool S5_ParentID_ReadOnly { get; set; }

		public override ZString DescriptionForLog
		{
			get
			{
				ZString description = S5_ScheduleDescription.IsEmpty ? "" : Res.GetString("0e39d247-6cbf-4344-a955-e71c4a0e9461", "Description: {0}", S5_ScheduleDescription);
				var parentMenu = Factory.Load<StmMenuItem>(S5_ParentID);
				if (parentMenu != null)
				{
					string nameOfReport = Res.GetString("2d943473-2f0b-400c-9063-df19e583f68a", "Report name: {0}", parentMenu.SU_MenuName);
					description = description.IsEmpty ? nameOfReport : nameOfReport + ", " + description;
				}
				return description;
			}
		}

		public void SkipCurrentQueuedRun()
		{
			if (S5_IsPrivate)
			{
				Delete();
				return;
			}
			UpdateNextScheduledDate();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!S5_IsPrivate && S5_IsActive && (!IsInDatabase || RecurrenceRangeHasChanges))
			{
				switch (S5_TaskPeriod)
				{
					case ScheduleRecurrenceType.Daily:
						if (!ValidateNextScheduleTimeForDailyPeriod())
						{
							S5_NextScheduledPrintRunTimeUtc = Recurrence.CalculateNextScheduleDate(S5_NextScheduledPrintRunTimeUtc, isRecurrencePatternChanged: true);
						}
						break;

					case ScheduleRecurrenceType.Weekly:
						if (!ValidateNextScheduleTimeForWeeklyPeriod())
						{
							S5_NextScheduledPrintRunTimeUtc = Recurrence.CalculateNextScheduleDate(S5_NextScheduledPrintRunTimeUtc, isRecurrencePatternChanged: true);
						}
						break;

					case ScheduleRecurrenceType.Monthly:
						if (!ValidateNextScheduleTimeForMonthlyPeriod())
						{
							S5_NextScheduledPrintRunTimeUtc = Recurrence.CalculateNextScheduleDate(S5_NextScheduledPrintRunTimeUtc, isRecurrencePatternChanged: true);
						}
						break;

					case ScheduleRecurrenceType.AccountingPeriod:
						if (!ValidateNextScheduleTimeForAccountingPeriod())
						{
							S5_NextScheduledPrintRunTimeUtc = Recurrence.CalculateNextScheduleDate(S5_NextScheduledPrintRunTimeUtc, isRecurrencePatternChanged: true);
						}
						break;

					case ScheduleRecurrenceType.Yearly:
						if (!ValidateNextScheduleTimeForYearlyPeriod())
						{
							S5_NextScheduledPrintRunTimeUtc = Recurrence.CalculateNextScheduleDate(S5_NextScheduledPrintRunTimeUtc, isRecurrencePatternChanged: true);
						}
						break;
				}
			}
		}

		#region GetPropertiesToExcludeFromCloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var properties = new List<string>(base.GetPropertiesToExcludeFromCloning());
			if (!Env.Instance.Security.ScheduleOtherStaffAsPrintUser.IsAllowed)
			{
				properties.Add(StmScheduleTaskSchema.Constants.S5_GS_NKPrintUser);
			}
			return properties;
		}

		#endregion

		#region Next Run Time Validation

		bool ValidateNextScheduleTimeForDailyPeriod()
		{
			if (CalcNextRunTimeLocal < ZDateTime.Now)
			{
				return false;
			}

			return !S5_WeekDaysOnly || (CalcNextRunTimeLocal.DayOfWeek != DayOfWeek.Sunday && CalcNextRunTimeLocal.DayOfWeek != DayOfWeek.Saturday);
		}

		bool ValidateNextScheduleTimeForWeeklyPeriod()
		{
			if (CalcNextRunTimeLocal < ZDateTime.Now)
			{
				return false;
			}

			var startIndex = CalcNextRunTimeLocal.DayOfWeek == DayOfWeek.Sunday ? 0 : (int)CalcNextRunTimeLocal.DayOfWeek;

			return Recurrence.DayList.SubstringSafe(startIndex, 1) == "Y";
		}

		bool ValidateNextScheduleTimeForMonthlyPeriod()
		{
			if (CalcNextRunTimeLocal < ZDateTime.Now)
			{
				return false;
			}

			if (Recurrence.WeekDayOccurrenceNumber == 0)
			{
				if (Recurrence.DayNumber == 99) // IsLastDay
				{
					return CalcNextRunTimeLocal.Day == DateTime.DaysInMonth(CalcNextRunTimeLocal.Year, CalcNextRunTimeLocal.Month);
				}

				return CalcNextRunTimeLocal.Day == Recurrence.DayOfMonth;
			}

			var correctDate = new ZDateTime(CalcNextRunTimeLocal.Year, CalcNextRunTimeLocal.Month, 1);

			while (correctDate.DayOfWeek != Recurrence.GetDayOfWeek())
			{
				correctDate = correctDate.AddDays(1);
			}

			correctDate = correctDate.AddDays(7 * (Recurrence.WeekDayOccurrenceNumber - 1));
			return correctDate.Date == CalcNextRunTimeLocal.Date;
		}

		bool ValidateNextScheduleTimeForYearlyPeriod()
		{
			if (CalcNextRunTimeLocal < ZDateTime.Now)
			{
				return false;
			}

			if (Recurrence.WeekDayOccurrenceNumber == 0)
			{
				return CalcNextRunTimeLocal.Day == Recurrence.DayOfMonth && Recurrence.MonthNumber == CalcNextRunTimeLocal.Month;
			}

			var correctDate = new ZDateTime(CalcNextRunTimeLocal.Year, CalcNextRunTimeLocal.Month, 1);

			while (correctDate.DayOfWeek != Recurrence.GetDayOfWeek())
			{
				correctDate = correctDate.AddDays(1);
			}

			correctDate = correctDate.AddDays(7 * (Recurrence.WeekDayOccurrenceNumber - 1));
			return correctDate.Date == CalcNextRunTimeLocal.Date;
		}

		bool ValidateNextScheduleTimeForAccountingPeriod()
		{
			if (CalcNextRunTimeLocal < ZDateTime.Now)
			{
				return false;
			}

			var currentAccPeriod = Recurrence.CalculateAccountingPeriodBasedOnDate(CalcNextRunTimeLocal.Date);

			if (currentAccPeriod == AccountingPeriodCalculator.InvalidPeriod)
			{
				return false;
			}

			if (Recurrence.WeekDayOccurrenceNumber == 0)
			{
				if (Recurrence.AccountingLastDay)
				{
					return CalcNextRunTimeLocal.Date == Recurrence.CalculateLastDayForAccountingPeriod(currentAccPeriod);
				}

				return Recurrence.DayOfAccountingPeriod == (CalcNextRunTimeLocal.Date - Recurrence.CalculateFirstDayForAccountingPeriod(currentAccPeriod)).Days + 1;
			}

			var correctDate = Recurrence.CalculateLocalDateForAccountingPeriod(Recurrence.WeekDayOccurrenceNumber, Recurrence.GetDayOfWeek(), currentAccPeriod, CalcNextRunTimeLocal.Date);

			return correctDate.Date == CalcNextRunTimeLocal.Date;
		}

		#endregion Next Run Time Validation

		#region Staff

		[List("Lookups.AllStaff")]
		public ZGuid UserFK
		{
			get
			{
				if (userFK == null)
				{
					if (!string.IsNullOrEmpty(S5_GS_NKPrintUser))
					{
						GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, S5_GS_NKPrintUser);
						if (staff != null)
						{
							userFK = staff.PK;
						}
					}
				}
				return userFK ?? ZGuid.Empty;
			}
			set
			{
				if (userFK != value)
				{
					userFK = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateUserFK();
					}
					UserFKInfo.RefreshBinding();
					HasChanges = true;
					SetPrintUser(value);
				}
			}
		}
		ZGuid? userFK;

		void SetPrintUser(ZGuid userPK)
		{
			var staff = Factory.Load<GlbStaff>(userPK) ?? GlbStaff.CurrentUser;
			S5_GS_NKPrintUser = staff.GS_Code;

			foreach (ReportScheduleTaskRecipient recipient in Recipients)
			{
				if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
				{
					recipient.S6_EmailFromAddress = staff.GS_EmailAddress;
				}
			}
		}

		public ZString EmailAddressForReportingErrors
		{
			get
			{
				var emailAddress = ZString.Empty;
				if (!S5_SystemLastEditUser.IsEmpty)
				{
					emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, S5_SystemLastEditUser);
					if (!emailAddress.IsEmpty)
					{
						return emailAddress;
					}
				}
				if (!S5_SystemLastEditUser.Equals(S5_SystemCreateUser) && !S5_SystemCreateUser.IsEmpty)
				{
					emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, S5_SystemCreateUser);
					if (!emailAddress.IsEmpty)
					{
						return emailAddress;
					}
				}

				GlbStaff staff = Factory.Load<GlbStaff>(UserFK);
				return staff.GS_EmailAddress;
			}
		}

		public ZPropertyInfo UserFKInfo => GetZPropertyInfo(nameof(UserFK));

		#endregion

		#region Delivery Instructions / Report

		public void SetScheduleFromDeliveryInstructions(DeliveryInstructions instructions, bool instantDelivery = true)
		{
			S5_IsPrivate = true;

			var report = instructions.DocPack.GetFirstReport();
			if (report != null)
			{
				report.ShouldUpdateSchedulableFilters = false;
			}

			PopulateDefaultsFromDeliveryInstructions(instructions);

			if (instantDelivery)
			{
				S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			}
		}

		public void PopulateDefaultsFromDeliveryInstructions(DeliveryInstructions instructions)
		{
			using (GetValidationSuspender())
			{
				DeliveryInstructions = instructions;
				IStmMenuItem menuItem = instructions.DocPack.StmMenuCommand;

				if (menuItem != null)
				{
					PopulateDefaultsFromMenuItem(menuItem, false);
					S5_ScheduleState = Serialize();
					SetPrintUser(UserFK);
					S5_ScheduleActualRunCount = 0;
					S5_IsActive = true;
					S5_TaskPeriodCount = 1;
				}

				Recipients.RemoveAndDeleteAll();
				foreach (DocDeliveryContact docContact in DeliveryInstructions.Recipients)
				{
					var recipient = Recipients.AddNew();
					recipient.PopulateFromContact(DeliveryInstructions, docContact);
				}
			}
		}

		public void PopulateDefaultsFromMenuItem(IStmMenuItem menuItem)
		{
			PopulateDefaultsFromMenuItem(menuItem, true);
		}

		void PopulateDefaultsFromMenuItem(IStmMenuItem menuItem, bool suspendValidation)
		{
			IDisposable validationSuspender = suspendValidation ? GetValidationSuspender() : null;
			using (validationSuspender)
			{
				S5_ParentID = menuItem.PK;
				S5_ScheduleDescription = menuItem.SU_MenuName.SubstringSafe(0, StmScheduleTaskSchema.S5_ScheduleDescription.MaxLength);
			}
		}

		void AddScheduleRecipientsToInstructions(DeliveryInstructions instructions, StmScheduleTaskRecipient[] recipients, INotifications notifications)
		{
			Dictionary<KeyValuePair<ZGuid, ZString>, List<ZString>> addedRecipients = new Dictionary<KeyValuePair<ZGuid, ZString>, List<ZString>>();

			foreach (StmScheduleTaskRecipient recipient in recipients)
			{
				DocDeliveryContact[] contacts = null;

				if (!recipient.S6_SQ.IsEmpty)
				{
					if (instructions.PrinterDelivery.PrintQueuePK.IsEmpty)
					{
						instructions.PrinterDelivery.PrintQueuePK = recipient.S6_SQ;
					}
					else if (recipient.S6_SQ != instructions.PrinterDelivery.PrintQueuePK)
					{
						throw new ArgumentException("Recipients must not contain items with different values of S6_SQ.", nameof(recipients));
					}
				}

				if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
				{
					contacts = CreateDocDeliveryContactsForGroup(recipient, notifications);
					if (contacts.Length == 0)
					{
						string message = string.Empty;
						if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
						{
							message = Res.GetString("f689df0f-ca1d-4c7f-aafe-a080f4de461f", "There were no staff members with an email address in the {0} group.", recipient.Group.GG_Code);
						}
						else if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Fax)
						{
							message = Res.GetString("542d6d79-271a-401a-b27c-8a6e564d2fe6", "There were no staff members with a fax number in the {0} group.", recipient.Group.GG_Code);
						}
						else
						{
							message = Res.GetString("43872331-2ec9-448f-8945-89c5d6cbe843", "There were no staff members in the {0} group.", recipient.Group.GG_Code);
						}
						notifications.Notify(new InfoNotification(message));
					}
				}
				else if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc)
				{
					var contact = new DocDeliveryContact(Factory);
					contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
					contact.AttachmentType = recipient.S6_AttachmentType;
					contact.OrgHeaderPK = recipient.S6_OH;
					contact.Name = recipient.ContactName;
					contact.StaffCode = recipient.S6_GS_NKRecipient;
					contacts = new DocDeliveryContact[] { contact };
				}
				else
				{
					DocDeliveryContact contact = CreateSingleDocDeliveryContact(recipient, notifications);
					if (contact != null)
					{
						contacts = new DocDeliveryContact[] { contact };
					}
				}

				if ((contacts != null) && (contacts.Length > 0))
				{
					foreach (DocDeliveryContact contact in contacts)
					{
						List<ZString> previousMethods;
						KeyValuePair<ZGuid, ZString> key = new KeyValuePair<ZGuid, ZString>(contact.OrgHeaderPK, contact.Name);
						string method = contact.DeliveryMethod + "/" + contact.AttachmentType;

						if (!addedRecipients.TryGetValue(key, out previousMethods))
						{
							addedRecipients[key] = previousMethods = new List<ZString>();
						}

						if (!previousMethods.Contains(method))
						{
							previousMethods.Add(method);
							if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
							{
								instructions.Recipients.Add(contact);
							}
						}
						if (recipient.S6_DeliveryToType != ScheduledReportDeliveryRecipientConstants.RecipientType.Group)
						{
							instructions.Recipients.Add(contact);
						}
					}
				}
			}
		}

		DocDeliveryContact CreateDocDeliveryContactTemplate(StmScheduleTaskRecipient recipient)
		{
			DocDeliveryContact result = new DocDeliveryContact(Factory);
			result.DeliveryMethod = recipient.S6_DeliveryMethod;
			result.AttachmentType = recipient.S6_AttachmentType;
			result.EmailSubjectMacro = recipient.S6_EmailSubjectLineOverride;
			result.EmptyReportContingency = new EmptyReportContingency(recipient.S6_EmptyReportDeliveryOptions, GetDeliverToAddress(recipient), recipient.S6_CarbonCopyRecipientsAsString, recipient.S6_BlindCarbonCopyRecipientsAsString);

			return result;
		}

		string GetDeliverToAddress(StmScheduleTaskRecipient recipient)
		{
			var overriddenValue = recipient.CanSetEmail ? recipient.S6_EmailToRecipientsAsString : recipient.S6_FaxOverride;
			return string.IsNullOrEmpty(overriddenValue) ? recipient.DeliveryAddress : overriddenValue;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // "need to make sure that 'ftp' is always in lower case"
		DocDeliveryContact CreateSingleDocDeliveryContact(StmScheduleTaskRecipient recipient, INotifications notifications)
		{
			DocDeliveryContact result = null;
			DocDeliveryContact contact = CreateDocDeliveryContactTemplate(recipient);

			if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Contact)
			{
				contact.OrgHeaderPK = recipient.S6_OH;
				contact.Name = recipient.ContactName;
				if (recipient.S6_DeliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Ftp)
				{
					if (string.IsNullOrEmpty(SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress))
					{
						if (EnvProxy.Instance.IsProductionSystem)
						{
							string replacement = recipient.S6_FtpAddress.Substring(0, 3);
							replacement = replacement.ToLowerInvariant();
							if (replacement.Equals((NoResString)"ftp", StringComparison.Ordinal))
							{
								recipient.S6_FtpAddress = replacement + recipient.S6_FtpAddress.Remove(0, 3);
							}

							contact.FileLocation = recipient.S6_FtpAddress;
							contact.UserName = recipient.S6_UserName;
							contact.Password = recipient.S6_Password;
						}
						else
						{
							var message = Res.GetString("30FF82E2-2602-400D-92DB-B8A07B800D63", "The FTP Destination Override setting has not been set. Since this is a non-production system, the report was not delivered to the intended FTP address. Please enter a FTP Destination Override setting in the Registry at System > Testing > FTP Destination Override.");
							notifications.AddError(message);
						}
					}
					else
					{
						contact.FileLocation = SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress;
						contact.UserName = SystemDataRegistry.Instance.FTPDestinationOverride.Value.UserName;
						contact.Password = SystemDataRegistry.Instance.FTPDestinationOverride.Value.Password;
					}
				}
				else if (!recipient.S6_FaxOverride.IsEmpty)
				{
					contact.DeliveryAddress = recipient.S6_FaxOverride;
				}
			}
			else
			{
				GlbStaff staff = recipient.Recipient;
				if (staff != null)
				{
					contact.Name = staff.GS_FullName;
					contact.StaffCode = staff.GS_Code;
				}

				if (recipient.S6_DeliveryMethod == Enterprise.Core.Constants.ContactNotifyModes.Ftp)
				{
					if (string.IsNullOrEmpty(SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress))
					{
						if (EnvProxy.Instance.IsProductionSystem)
						{
							contact.FileLocation = recipient.S6_FtpAddress;
							contact.UserName = recipient.S6_UserName;
							contact.Password = recipient.S6_Password;
						}
						else
						{
							var message = Res.GetString("30FF82E2-2602-400D-92DB-B8A07B800D63", "The FTP Destination Override setting has not been set. Since this is a non-production system, the report was not delivered to the intended FTP address. Please enter a FTP Destination Override setting in the Registry at System > Testing > FTP Destination Override.");
							notifications.AddError(message);
						}
					}
					else
					{
						contact.FileLocation = SystemDataRegistry.Instance.FTPDestinationOverride.Value.FtpAddress;
						contact.UserName = SystemDataRegistry.Instance.FTPDestinationOverride.Value.UserName;
						contact.Password = SystemDataRegistry.Instance.FTPDestinationOverride.Value.Password;
					}
				}
				else if (!recipient.S6_FaxOverride.IsEmpty)
				{
					contact.DeliveryAddress = recipient.S6_FaxOverride;
				}
				else
				{
					contact.DeliveryAddress = recipient.DeliveryAddress;
				}
			}

			FillOutRecipientsFromContact(recipient, contact, notifications);

			if (recipient.S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Print && contact.DeliveryAddress.IsEmpty)
			{
				if (EnvProxy.Instance.IsProductionSystem)
				{
					string message = string.Empty;
					if (contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
					{
						message = Res.GetString("49c5fc2e-2309-4d1a-8704-9382b6fda4f8", "{0} does not have an email address or is inactive.", contact.Name);
					}
					else if (contact.DeliveryMethod == Core.Constants.ContactNotifyModes.Ftp)
					{
						message = Res.GetString("52d4e1dd-e821-4091-ad62-19bbcd013aa3", "{0} does not have a FTP destination address.", contact.Name);
					}
					else
					{
						message = Res.GetString("46379f21-1983-4a43-926e-30511b4c5894", "{0} does not have a fax number or is inactive.", contact.Name);
					}
					notifications.Notify(new InfoNotification(message));
				}
			}
			else
			{
				result = contact;
			}

			return result;
		}

		static void CopyRecipientsEmailsToContact(StmScheduleTaskRecipient recipient, StmScheduleTaskCopyRecipientCollection copyRecipients, NonPersistentCopyRecipientCollection contactEmails)
		{
			foreach (var scheduleTaskRecipient in copyRecipients)
			{
				var emailRecipient = new NonPersistentCopyRecipient(recipient.Header, scheduleTaskRecipient.SCR_RecipientType) { EmailAddress = scheduleTaskRecipient.SCR_EmailAddress };
				contactEmails.Add(emailRecipient);
			}
		}

		DocDeliveryContact[] CreateDocDeliveryContactsForGroup(StmScheduleTaskRecipient recipient, INotifications notifications)
		{
			List<DocDeliveryContact> result = new List<DocDeliveryContact>();

			if (recipient.S6_FaxOverride.IsEmpty && recipient.S6_EmailToRecipientsAsString.IsEmpty)
			{
				foreach (GlbStaff staff in recipient.Group.Staff)
				{
					bool shouldAdd = true;
					DocDeliveryContact contact = CreateDocDeliveryContactTemplate(recipient);
					contact.Name = staff.GS_FullName;
					contact.EmptyReportContingency = new EmptyReportContingency(recipient.S6_EmptyReportDeliveryOptions, staff.GS_EmailAddress);

					if (recipient.S6_DeliveryMethod != Core.Constants.ContactNotifyModes.Print)
					{
						if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
						{
							contact.DeliveryAddress = staff.GS_EmailAddress;

							if (!recipient.S6_EmailFromAddress.IsEmpty && ValidateRecipientEmailFromAddress(recipient.S6_EmailFromAddress, notifications))
							{
								contact.EmailFromAddress = recipient.S6_EmailFromAddress;
							}
						}
						else
						{
							contact.DeliveryAddress = staff.GS_FaxNum;
						}

						shouldAdd = !contact.DeliveryAddress.IsEmpty;
					}

					if (shouldAdd)
					{
						result.Add(contact);
					}
				}
			}
			else
			{
				DocDeliveryContact contact = CreateDocDeliveryContactTemplate(recipient);
				contact.DeliveryAddress = recipient.S6_EmailToRecipientsAsString;
				result.Add(contact);
			}

			if (result.Any())
			{
				FillOutRecipientsFromContact(recipient, result[0], notifications);
			}

			return result.ToArray();
		}

		void FillOutRecipientsFromContact(StmScheduleTaskRecipient recipient, DocDeliveryContact contact, INotifications notifications)
		{
			if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Email)
			{
				if (!recipient.EmailToRecipients.IsNullOrEmpty() && !contact.EmailToRecipients.IsNullOrEmpty())
				{
					contact.EmailToRecipients.RemoveAndDeleteAll();
				}
				CopyRecipientsEmailsToContact(recipient, recipient.EmailToRecipients, contact.EmailToRecipients);
				CopyRecipientsEmailsToContact(recipient, recipient.CarbonCopyRecipients, contact.EmailCarbonCopyRecipients);
				CopyRecipientsEmailsToContact(recipient, recipient.BlindCarbonCopyRecipients, contact.EmailBlindCarbonCopyRecipients);

				if (!recipient.S6_EmailFromAddress.IsEmpty && ValidateRecipientEmailFromAddress(recipient.S6_EmailFromAddress, notifications))
				{
					contact.EmailFromAddress = recipient.S6_EmailFromAddress;
				}
			}
		}

		DeliveryInstructions DeliveryInstructions;

		bool hasReportBeenAnalyzed;

		void AnalyzeReport()
		{
			if (!hasReportBeenAnalyzed)
			{
				reportHasColumnHeaders = false;
				disableXLSXExport = false;

				var command = Factory.Load<ReportCommand>(S5_ParentID);
				if (command != null)
				{
					using (var documentPack = new DocumentPack(command))
					using (var report = documentPack.GetFirstReport())
					{
						if (report != null)
						{
							report.PrepareForRender();
							reportHasColumnHeaders = !report.ColumnHeadingManager.IsEmpty;
							disableXLSXExport = report.Analyser != null && report.Analyser.Config != null && report.Analyser.Config.DisableXLSXExport;
							disableCSVExport = report.Analyser != null && report.Analyser.Config != null && report.Analyser.Config.DisableCSVExport;
							excludedAttachmentTypes = report.Analyser?.GetExcludedAttachmentTypes();
						}
					}
				}

				hasReportBeenAnalyzed = true;
			}
		}

		bool ValidateRecipientEmailFromAddress(ZString email, INotifications notifications)
		{
			var staff = Factory.Load<GlbStaff>(UserFK);

			if (staff == null)
			{
				notifications.AddError(Res.GetString("49C1E463-A429-4FC7-B468-48322C73C5E4", "No print user has been set. Please enter a print user."));

				return false;
			}

			if (staff.EmailAddresses.FindByEmailAddressString(email) == null)
			{
				notifications.AddError(Res.GetString("7F09E55D-836C-4B05-A4A2-EB4847AC9B47", "Staff '{0}' ({1}) does not have the email address '{2}'. The default sender address will be used. Please choose a valid sender address in schedule task '{3}'.", staff.GS_FullName, staff.GS_Code, email, S5_ScheduleDescription));

				return false;
			}

			return true;
		}

		public bool ReportHasColumnHeaders
		{
			get
			{
				AnalyzeReport();
				return reportHasColumnHeaders.Value;
			}
		}
		bool? reportHasColumnHeaders;

		public bool DisableXLSXExport
		{
			get
			{
				AnalyzeReport();
				return disableXLSXExport ?? false;
			}
		}
		bool? disableXLSXExport;

		public bool DisableCSVExport
		{
			get
			{
				AnalyzeReport();
				return disableCSVExport ?? false;
			}
		}
		bool? disableCSVExport;

		public IReadOnlyCollection<string> ExcludedAttachmentTypes
		{
			get
			{
				AnalyzeReport();
				return excludedAttachmentTypes ?? Array.Empty<string>();
			}
		}
		string[] excludedAttachmentTypes;

		#endregion

		#region Serialize / Deserialize

#if DEBUG
		internal
#endif
		ZBlob Serialize()
		{
			if (DeliveryInstructions.DocPack.Count > 0)
			{
				return Serialize(DeliveryInstructions, (Report)DeliveryInstructions.DocPack[0]);
			}
			else
			{
				return ZBlob.Empty;
			}
		}

		protected override bool IsErrorThatShouldRetry(Exception ex)
		{
			return (ex is MaxConcurrentReportConnectionsExceeded) || (ex is MetadataHasChangedException) || (ex is GhostRecordsBeingDeletedException);
		}

		protected override TimeSpan RetryWaitPeriod => TimeSpan.FromSeconds(1);

		protected override void UpdateNextScheduledDateCore()
		{
			if (shouldUpdateNextScheduledDate)
			{
				base.UpdateNextScheduledDateCore();
			}
		}

		public ZBlob Serialize(Report report)
		{
			return Serialize(null, report);
		}

		ZBlob Serialize(DeliveryInstructions instructions, Report report)
		{
			if (report != null)
			{
				var staff = Factory.Load<GlbStaff>(UserFK);
				var reportSerializationInfo =
					staff != null
						? new ReportSerializationInfo(instructions, report, staff.GS_LoginName)
						: new ReportSerializationInfo(instructions, report);
				var jsonResult = JsonConverterHelper.Serialize(reportSerializationInfo);
				return new ZBlob(Encoding.UTF8.GetBytes(jsonResult));
			}
			else
			{
				return ZBlob.Empty;
			}
		}

		public ReportSerializationInfo CreateReportFromTask()
		{
			ReportSerializationInfo result = ScheduledReportHelper.DeserializeStreamToReportSerializationInfo(S5_ScheduleState);
			if (result != null && result.Report != null)
			{
				result.Report.SetScheduleTask(this);
			}
			return result;
		}

		internal void AutoHealOrgFilterSerializedByCode(ReportSerializationInfo serializationInfo)
		{
			if (serializationInfo != null && serializationInfo.Report != null)
			{
				var orgFilters = serializationInfo.Report.FilterCollection.Where(x => x is MultipleSelectionLookup multipleSelectionLookup && multipleSelectionLookup.ModuleID == ModuleIDs.Organisation && !multipleSelectionLookup.SerialisedByPK);
				if (orgFilters.Any())
				{
					var reportScheduleTaskFactory = new BusinessObjectFactory();
					foreach (MultipleSelectionLookup orgFilter in orgFilters)
					{
						orgFilter.SerialisedByPK = true;
					}

					var task = reportScheduleTaskFactory.Load<ReportScheduleTask>(PK);
					var jsonResult = JsonConverterHelper.Serialize(serializationInfo);
					task.S5_ScheduleState = new ZBlob(Encoding.UTF8.GetBytes(jsonResult));
					reportScheduleTaskFactory.Save();
				}
			}
		}

		#endregion

		#region Run
		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			ReportCommand command;
			ReportSerializationInfo reportInfo;

			SqlParameterNameGenerator.Reset();
			if ((command = Factory.Load<ReportCommand>(S5_ParentID)) == null)
			{
				notifications.AddWarning(Res.GetString("0e88700d-3763-4ab6-9cfd-188f84bb85c8", "No Report Command found for Scheduled Report '{0}'. Parent ID: {1}", S5_ScheduleDescription, S5_ParentID));
			}
			else
			{
				if (StmReportRun != null)
				{
					StmReportRun.RRI_ReportName = command.SU_MenuName;
				}
				if ((reportInfo = CreateReportFromTask()) == null)
				{
					notifications.AddWarning(Res.GetString("f63c0455-adc3-43a6-9806-c96589f084b6", "No Report Info created for Scheduled Report '{0}'.", S5_ScheduleDescription));
					return;
				}
				using (var pack = new DocumentPack(command))
				{
					try
					{
						using (Env.Instance.SuppressSwitchContextCheck())
						using (reportInfo.SwitchUserTemporarily(S5_GS_NKPrintUser))
						{
							pack.DeserializeDocPackFromReportCollection(reportInfo.Report, notifications);
#if DEBUG
							IsSerializedReportDesrializedBeforeRunning = true;
#endif
						}
					}
					catch (ReportSerializationInfo.InvalidPrintUserException invalidPrintUserException)
					{
						MarkTaskAsInactive();

						notifications.AddError(invalidPrintUserException.IsInactive ?
							Res.GetString("C4C38B12-8324-48ED-A4A9-EA69B0FD1C60", "Scheduled Report '{0}' was deactivated because it is configured to run under user '{1}' which is inactive.", S5_ScheduleDescription, invalidPrintUserException.User) :
							Res.GetString("3192d3f4-be54-4b25-bb15-ed6ee08e9c7e", "Scheduled Report '{0}' was deactivated because it is configured to run under user '{1}' which cannot be found.", S5_ScheduleDescription, invalidPrintUserException.User));
						return;
					}

					if (ReportsFiltersMatchTemplateFilters(notifications, pack, reportInfo))
					{
						var report = pack.GetFirstReport();
						if (report != null)
						{
							report.ReportServerNameChanged += (sender, reportServerName) =>
							{
								DbServerName = reportServerName;
							};
							ChangeReportDbForTestingIfRequired(report);
						}

						AutoHealOrgFilterSerializedByCode(reportInfo);
						var reportStatistics = new ReportStatistics();

						var isRenderAndSaved = false;
						var dbServerInfo = new StringBuilder();
						if (StmReportRun != null && report != null)
						{
							StmReportRun.RRI_ReportName = command.SU_MenuName;
							report.stmReportRun = StmReportRun;
							report.OnRenderAndSave += (sender, runningConnection) =>
							{
								if (!isRenderAndSaved)
								{
									reportStatistics.StartSQLCPU = SQLCPUTime(runningConnection);
								}
							};
							report.AfterRenderAndSave += (sender, runningConnection) =>
							{
								if (!isRenderAndSaved)
								{
									reportStatistics.EndSQLCPU = SQLCPUTime(runningConnection);
									isRenderAndSaved = true;
								}
							};
							report.AddDBServerInfo += (sender, message) =>
							{
								dbServerInfo.AppendLine(message);
							};
							reportStatistics.Start = ZDateTime.UtcNow;
							reportStatistics.StartCPU = Process.GetCurrentProcess().TotalProcessorTime;
						}
						RunPack(notifications, pack, reportInfo);
						if (StmReportRun != null)
						{
							StmReportRun.RRI_Status = Enterprise.Core.Constants.StmReportRunState.Finished;

							if (report != null)
							{
								var endCPU = Process.GetCurrentProcess().TotalProcessorTime;
								var end = ZDateTime.UtcNow;
								StmReportRun.RRI_SQLRunDurationMilliseconds = Math.Max(0, (int)(report.SqlElapsedTime * 1000));
								StmReportRun.RRI_TotalRunDurationMilliseconds = Math.Max(0, (int)Math.Floor((end - reportStatistics.Start).TotalMilliseconds));
								StmReportRun.RRI_ClientCPUDurationMilliseconds = Math.Max(0, (int)Math.Floor((endCPU - reportStatistics.StartCPU).TotalMilliseconds));
								StmReportRun.RRI_SQLCPUDurationMilliseconds = Math.Max(0, reportStatistics.EndSQLCPU - reportStatistics.StartSQLCPU);
								StmReportRun.RRI_ClientRunDurationMilliseconds = Math.Max(0, StmReportRun.RRI_TotalRunDurationMilliseconds - StmReportRun.RRI_SQLRunDurationMilliseconds);
							}

							if (S5_IsPrivate)
							{
								StmReportRun.Factory.Save();
							}

							if (dbServerInfo.Length > 0)
							{
								//StmReportRun.Factory isn't owned by us, so we can't use it (add new bizos in it) without causing CrossThreadAccessException
								var stmReportRunInLocalFactory = Factory.Load<StmReportRun>(StmReportRun.PK);
								var note = stmReportRunInLocalFactory.Notes.AddNew();
								note.ST_Description = (NoResString)"DB Server Info";
								note.ST_NoteDataAsText = dbServerInfo.ToString();
								note.ST_NoteType = nameof(StmNoteVisibility.PUB);
								Factory.Save();
							}
						}
					}
					else
					{
						MarkTaskAsInactive();
					}
				}
			}
		}

		[Conditional("DEBUG")]
		internal void ChangeReportDbForTestingIfRequired(Report report)
		{
#if DEBUG
			if (ReportScheduleTask.ShouldChangeReportDbForTesting.Value)
			{
				var reportDb = new ReportDbForTesting(Db.ServerName, Db.DatabaseName);
				var manager = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
				((IReportForReportDbTesting)report).SetReportDbManagerForTesting(manager);
				report.OverrideReportDbOption = false;
			}
#endif
		}

#if DEBUG
		internal static Overridable<bool> ShouldChangeReportDbForTesting = new Overridable<bool>();
#endif

#if DEBUG
		protected virtual
#endif
			int SQLCPUTime(DbConnection connection)
		{
			return StmReportHelper.SQLCPUTime(connection);
		}

#if DEBUG
		public bool IsSerializedReportDesrializedBeforeRunning;
#endif

		void RunPack(INotifications notifications, DocumentPack pack, ReportSerializationInfo reportInfo)
		{
			using (Env.Instance.SuppressSwitchContextCheck())
			using (reportInfo.SwitchUserTemporarily(S5_GS_NKPrintUser))
			{
				SetTemplateSheetCollection(notifications, pack);

				RunPrintTasksForReport(notifications, pack, reportInfo);
			}
		}

		void ResetDocumentPackALLAnalyserRendererAndExcelInterface(DocumentPack pack, ZGuid overriddenValueForOrgLookupFilter)
		{
			foreach (var deliverable in pack)
			{
				if (deliverable is Report report)
				{
					report.ResetAnalyserRendererAndExcelInterface();
					report.ResetCachedExcelFile();
					report.OverriddenValueForOrgLookupFilter = overriddenValueForOrgLookupFilter;
				}
			}
		}

		void RunPrintTasksForReport(INotifications notifications, DocumentPack pack, ReportSerializationInfo reportInfo)
		{
			PrintTask printTask = null;
			try
			{
				var deliveryInstructions = GetDeliveryInstructionsAndAddWarningsIfEmpty(notifications, pack, reportInfo);

				foreach (var instructions in deliveryInstructions)
				{
					if (instructions.OverriddenValueForOrgLookupFilter.IsValid)
					{
						ResetDocumentPackALLAnalyserRendererAndExcelInterface(pack, instructions.OverriddenValueForOrgLookupFilter);
					}

					printTask = GetNewPrintTaskForRun();
					printTask.Add(pack);
					printTask.Run(instructions);
				}
			}
			catch (SQLExecutionException ex)
			{
				notifications.AddError(Res.GetString("ea797dc9-1922-4902-8db3-edbcd64b5dac", "Scheduled Report '{0}' could not be delivered due to the following error:\r\n{1}", S5_ScheduleDescription, ex.Message));

				if (printTask == null || !printTask.GetDocumentPacks().Any(docPack => docPack.OfType<Report>().Any(r => r.ContainsAnyCustomisation)))
				{
					throw;
				}
			}
			catch (InvalidDeliveryGroupException ex)
			{
				var message = Res.GetString("B52E1344-6AB8-4CDF-9C8C-FB5130C13821", "'{0}' cannot be delivered, there is something wrong with the delivery group, the error reason is: '{1}'.", ex.Name, ex.Reason);
				notifications.AddError(message);
			}
			catch (MaxConcurrentReportConnectionsExceeded ex)
			{
				var message = Res.GetString("d9a449c5-0f1f-4693-b8ea-fbe655bec71f", "Scheduled Report '{0}' could not be delivered at this time due to maximum concurrent report limit reached, and will be processed again on next run cycle.\r\nThe following reports are currently running:\r\n\r\n{1}", S5_ScheduleDescription, ex.Message);
				notifications.AddWarning(message);

				shouldUpdateNextScheduledDate = false;
				throw;
			}
			catch (Exception exception)
			{
				if (IsInfrastructureDbError(exception))
				{
					shouldUpdateNextScheduledDate = false;
				}
				throw;
			}
		}

		protected override void NotifyStaffThatTaskWasDeactivatedDueToException(Exception e, INotifications notifications)
		{
			if (e is MaxConcurrentReportConnectionsExceeded)
			{
				var retryFailMessage = Res.GetString("8707CE98-A62C-4577-9FDE-64DCB0F3DD0B", "After 10 retry attempts, Scheduled Task - {0} has been deactivated on {1}. Please review your report settings and runtime environment, or reactivate the task for another attempt.", S5_ScheduleDescription, ZDateTime.Now);
				var notificationBuffer = notifications is NotificationBuffer buffer ? buffer : new NotificationBuffer(notifications);
				SendErrorNotificationEmailToUser(notificationBuffer, retryFailMessage);
			}
		}

		bool ReportsFiltersMatchTemplateFilters(INotifications notifications, DocumentPack pack, ReportSerializationInfo reportInfo)
		{
			try
			{
				using (var deserializedReport = reportInfo.Report)
				{
					return pack.All(report => (new ReportScheduleValidityChecker(this, deserializedReport, (Report)report)).IsDeserializedReportFiltersMatchingWithTemplateFilters());
				}
			}
			catch (EmailHasNoFromAddressException ex)
			{
				notifications.AddError(Res.GetString("D58CEDF1-E469-46F3-AC77-FFF3A207383C", "Could not send an email because the 'from address' was empty. Message: {0}", ex.Message));
				return false;
			}
		}

		void SetTemplateSheetCollection(INotifications notifications, DocumentPack pack)
		{
			if (!pack.Any())
			{
				notifications.AddWarning(Res.GetString("eacba4f3-76f7-4afe-b14f-9789a3a8771d", "Document pack is empty for Scheduled Report '{0}'.", S5_ScheduleDescription));
			}

			foreach (Report report in pack)
			{
				report.IsScheduledReport = true;
				if (report.DeserializedReport != null)
				{
					report.OptionalTemplateSheetCollection = report.DeserializedReport.OptionalTemplateSheetCollection;
				}
			}
		}

		IEnumerable<DeliveryInstructions> GetDeliveryInstructionsAndAddWarningsIfEmpty(INotifications notifications, DocumentPack pack, ReportSerializationInfo reportInfo)
		{
			var deliveryInstructions = GetDeliveryInstructions(reportInfo, pack, notifications);

			if (EnvProxy.Instance.IsProductionSystem)
			{
				if (ActiveRecipientsCollection.IsNullOrEmpty())
				{
					notifications.AddWarning(Res.GetString("36C21E4A-865B-431E-AD94-738644665C8B", "No active recipients found for Scheduled Report '{0}'.", S5_ScheduleDescription));
				}
				else if (!deliveryInstructions.Any())
				{
					notifications.AddWarning(Res.GetString("3880dc6a-16c0-4cd7-b39a-0820e2c540f5", "No delivery instructions found for Scheduled Report '{0}'.", S5_ScheduleDescription));
				}
				else if (deliveryInstructions.All(instructions => instructions.Recipients.Count == 0))
				{
					notifications.AddWarning(Res.GetString("bbdf1f7d-098e-4e03-83a9-8ec517b519f1", "No recipients found for Scheduled Report '{0}'.", S5_ScheduleDescription));
				}
			}

			return deliveryInstructions;
		}

		bool shouldUpdateNextScheduledDate = true;

		internal void MarkTaskAsInactive()
		{
			ReportScheduleTask task = Factory.Load<ReportScheduleTask>(this.PK);
			task.S5_IsActive = false;
			Factory.Save();
		}

		protected virtual PrintTask GetNewPrintTaskForRun()
		{
			return new PrintTask();
		}

		DeliveryInstructions[] GetDeliveryInstructions(ReportSerializationInfo reportInfo, DocumentPack pack, INotifications notifications)
		{
			var result = new List<DeliveryInstructions>();

			if (ActiveRecipientsCollection.IsNullOrEmpty())
			{
				return result.ToArray();
			}

			var recipientsBatches = new Dictionary<ZGuid, List<StmScheduleTaskRecipient>>();
			var firstPrinterPK = ZGuid.Empty;
			recipientsBatches[ZGuid.Empty] = new List<StmScheduleTaskRecipient>();

			var recipientsBatchesForOrganisation = new Dictionary<ZGuid, List<StmScheduleTaskRecipient>>();
			List<Guid> serializedOrgFilterValuesThatShouldBeOverridden = null;
			var shouldOverrideOrgFilter = false;
			var templateOrgFiltersThatShouldBeOverridden = pack.GetFirstReport()?.FilterCollection.OfType<LookupField>().Where(l => l.LinkToScheduledReportRecipientForOrganisation).ToArray();
			if (templateOrgFiltersThatShouldBeOverridden?.Length > 0)
			{
				shouldOverrideOrgFilter = true;
				serializedOrgFilterValuesThatShouldBeOverridden = reportInfo.Report?.FilterCollection.OfType<LookupField>()
					.Where(r => templateOrgFiltersThatShouldBeOverridden.OfType<LookupField>().Any(l => r.DisplayName == l.DisplayName && r.Value != Guid.Empty))
					.Select(x => x.Value).Distinct().ToList();
			}

			foreach (StmScheduleTaskRecipient recipient in ActiveRecipientsCollection)
			{
				if (recipient.S6_DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
				{
					if (firstPrinterPK.IsEmpty || recipient.S6_SQ == firstPrinterPK)
					{
						firstPrinterPK = recipient.S6_SQ;
						recipientsBatches[ZGuid.Empty].Add(recipient);
					}
					else
					{
						AddRecipientsBatches(recipient.S6_SQ, recipientsBatches, recipient);
					}
				}
				else if (shouldOverrideOrgFilter && recipient.S6_OH.IsValid && (serializedOrgFilterValuesThatShouldBeOverridden == null || serializedOrgFilterValuesThatShouldBeOverridden.Count == 0 || serializedOrgFilterValuesThatShouldBeOverridden.Any(r => r != recipient.S6_OH)))
				{
					AddRecipientsBatches(recipient.S6_OH, recipientsBatchesForOrganisation, recipient);
				}
				else
				{
					recipientsBatches[ZGuid.Empty].Add(recipient);
				}
			}

			foreach (var key in recipientsBatches.Keys)
			{
				CreateSingleDeliveryInstructions(pack, recipientsBatches[key].ToArray(), notifications, reportInfo, ZGuid.Empty, result);
			}

			foreach (var key in recipientsBatchesForOrganisation.Keys)
			{
				CreateSingleDeliveryInstructions(pack, recipientsBatchesForOrganisation[key].ToArray(), notifications, reportInfo, key, result);
			}

			return result.ToArray();
		}

		void AddRecipientsBatches(ZGuid key, Dictionary<ZGuid, List<StmScheduleTaskRecipient>> recipientsBatches, StmScheduleTaskRecipient recipient)
		{
			List<StmScheduleTaskRecipient> recipientsForOrganisation;
			if (!recipientsBatches.TryGetValue(key, out recipientsForOrganisation))
			{
				recipientsForOrganisation = recipientsBatches[key] = new List<StmScheduleTaskRecipient>();
			}
			recipientsForOrganisation.Add(recipient);
		}

		void CreateSingleDeliveryInstructions(DocumentPack pack, StmScheduleTaskRecipient[] recipients, INotifications notifications, ReportSerializationInfo reportInfo, ZGuid overriddenValueForOrgLookupFilter, List<DeliveryInstructions> result)
		{
			var instructions = new DeliveryInstructions(pack, PK);
			instructions.Recipients.RemoveAll(); // Clear any default recipients from the instructions.
			AddScheduleRecipientsToInstructions(instructions, recipients, notifications);
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			instructions.OverriddenValueForOrgLookupFilter = overriddenValueForOrgLookupFilter;

			if (instructions.Recipients.Count > 0)
			{
				if (reportInfo.Instructions != null)
				{
					instructions.Language = reportInfo.Instructions.Language;
					instructions.CoverNote = reportInfo.Instructions.CoverNote;
					instructions.IncludeCoverNote = reportInfo.Instructions.IncludeCoverNote;
					if (instructions.PrinterDelivery != null && reportInfo.Instructions.PrinterDelivery != null)
					{
						instructions.PrinterDelivery.NumberOfCopies = reportInfo.Instructions.PrinterDelivery.NumberOfCopies;
					}
				}
				else
				{
					instructions.Language = reportInfo.Language;
				}
				result.Add(instructions);
			}
		}

		StmScheduleTaskRecipientDependentCollection GetActiveRecipientsCollection(StmScheduleTaskRecipientDependentCollection recipientCollection)
		{
			var scheduleTaskRecipientList = new StmScheduleTaskRecipientDependentCollection(this);
			foreach (StmScheduleTaskRecipient recipient in recipientCollection)
			{
				if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Contact
					&& (recipient.S6_OC == ZGuid.Empty || recipient.S6_OC != ZGuid.Empty && recipient.Contact.OC_IsActive))
				{
					scheduleTaskRecipientList.Add(recipient);
				}
				else if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Staff
					&& (recipient.S6_GS_NKRecipient == string.Empty || recipient.S6_GS_NKRecipient != string.Empty && recipient.Staff.GS_IsActive))
				{
					scheduleTaskRecipientList.Add(recipient);
				}
				else if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.Group
					&& (recipient.S6_GG == ZGuid.Empty || recipient.S6_GG != ZGuid.Empty && recipient.Group.GG_IsActive))
				{
					scheduleTaskRecipientList.Add(recipient);
				}
				else if (recipient.S6_DeliveryToType == ScheduledReportDeliveryRecipientConstants.RecipientType.EDoc && StmReportRun != null)
				{
					scheduleTaskRecipientList.Add(recipient);
				}
				else if (recipient.S6_DeliveryToType == string.Empty)
				{
					scheduleTaskRecipientList.Add(recipient);
				}
			}
			return scheduleTaskRecipientList;
		}

		#endregion

		#region FillScheduleData

		internal void FillData(ReportScheduleTaskData reportScheduleTaskData, ReportRunningError errorCollector = null)
		{
			S5_ScheduleDescription = reportScheduleTaskData.ScheduleDescription;
			S5_IsActive = reportScheduleTaskData.IsActive;
			S5_GB = reportScheduleTaskData.Branch;
			UserFK = reportScheduleTaskData.UserFk;
			if (reportScheduleTaskData.NextRunTimeLocal.HasValue)
			{
				CalcNextRunTimeLocal = ConvertTimeZoneToLocalTime(reportScheduleTaskData.NextRunTimeLocal.Value, reportScheduleTaskData.Recurrence?.RecurringStartTimeZone);
			}

			if (reportScheduleTaskData.Recurrence != null && !reportScheduleTaskData.IsPrivate)
			{
				FillRecurrenceData(reportScheduleTaskData.Recurrence, errorCollector);
			}
		}

		void FillRecurrenceData(ScheduleRecurrenceData recurrenceData, ReportRunningError errorCollector = null)
		{
			if (recurrenceData.StartDateLocal.HasValue)
			{
				Recurrence.StartDateLocalForUser = ConvertTimeZoneToLocalTime(recurrenceData.StartDateLocal.Value, recurrenceData.RecurringStartTimeZone);
			}
			Recurrence.TaskPeriod = recurrenceData.RecurrenceType;
			switch (Recurrence.TaskPeriod)
			{
				case ScheduleRecurrenceType.Daily:
					Recurrence.WeekDaysOnly = recurrenceData.IsWeekDayOnly;
					break;
				case ScheduleRecurrenceType.Weekly:
					if (recurrenceData.WeekDayList != null && recurrenceData.WeekDayList.Count == 7)
					{
						Recurrence.DayList = new string(recurrenceData.WeekDayList.Select(o => o ? 'Y' : 'N').ToArray());
					}
					break;
				case ScheduleRecurrenceType.Monthly:
					Recurrence.MonthlyDay = recurrenceData.IsMonthlyDay;
					Recurrence.MonthLastDay = recurrenceData.IsMonthlyLastDay;
					if (!Recurrence.DayOfMonthInfo.ReadOnly)
					{
						Recurrence.DayOfMonth = recurrenceData.DayOfMonth;

						// After setting DayOfMonth, it will change StartDateLocalForUser and S5_StartDate, reset StartDateLocalForUser for error correcting.
						if (recurrenceData.StartDateLocal.HasValue)
						{
							Recurrence.StartDateLocalForUser = ConvertTimeZoneToLocalTime(recurrenceData.StartDateLocal.Value, recurrenceData.RecurringStartTimeZone);
						}
					}

					if (!Recurrence.WeekCountAsStringInfo.ReadOnly)
					{
						Recurrence.WeekCountAsString = recurrenceData.WeekDayOccurrenceNumber.ToString();
					}

					if (!Recurrence.DayNameInfo.ReadOnly)
					{
						Recurrence.DayName = Recurrence.Lookups.WeekDays.GetCodeFromDescription(recurrenceData.WeekDayNumber.ToString());
					}
					break;
				case ScheduleRecurrenceType.AccountingPeriod:
					Recurrence.AccountingDay = recurrenceData.IsAccountingDay;
					Recurrence.AccountingLastDay = recurrenceData.IsAccountingLastDay;

					if (!Recurrence.DayOfAccountingPeriodInfo.ReadOnly)
					{
						Recurrence.DayOfAccountingPeriod = recurrenceData.DayOfAccountingPeriod;
					}

					if (!Recurrence.WeekCountAsStringInfo.ReadOnly)
					{
						Recurrence.WeekCountAsString = recurrenceData.WeekDayOccurrenceNumber.ToString();
					}

					if (!Recurrence.DayNameInfo.ReadOnly)
					{
						Recurrence.DayName = Recurrence.Lookups.WeekDays.GetCodeFromDescription(recurrenceData.WeekDayNumber.ToString());
					}
					break;
				case ScheduleRecurrenceType.Yearly:
					Recurrence.YearlyEvery = recurrenceData.IsYearlyDay;
					if (!Recurrence.EveryMonthNumberDayAsStringInfo.ReadOnly)
					{
						Recurrence.EveryMonthNumberDayAsString = recurrenceData.EveryMonthNumber.ToString();
					}

					if (!Recurrence.DayOfMonthInfo.ReadOnly)
					{
						Recurrence.DayOfMonth = recurrenceData.DayOfMonth;
						// After setting DayOfMonth, it will change StartDateLocalForUser and S5_StartDate, reset StartDateLocalForUser for error correcting.
						if (recurrenceData.StartDateLocal.HasValue)
						{
							Recurrence.StartDateLocalForUser = ConvertTimeZoneToLocalTime(recurrenceData.StartDateLocal.Value, recurrenceData.RecurringStartTimeZone);
						}
					}

					if (!Recurrence.WeekCountAsStringInfo.ReadOnly)
					{
						Recurrence.WeekCountAsString = recurrenceData.WeekDayOccurrenceNumber.ToString();
					}

					if (!Recurrence.DayNameInfo.ReadOnly)
					{
						Recurrence.DayName = Recurrence.Lookups.WeekDays.GetCodeFromDescription(recurrenceData.WeekDayNumber.ToString());
					}

					if (!Recurrence.MonthNumberAsStringInfo.ReadOnly)
					{
						Recurrence.MonthNumberAsString = recurrenceData.MonthNumber.ToString();
						// After setting MonthNumberAsString, it will change StartDateLocalForUser and S5_StartDate, reset StartDateLocalForUser for error correcting.
						if (recurrenceData.StartDateLocal.HasValue)
						{
							Recurrence.StartDateLocalForUser = ConvertTimeZoneToLocalTime(recurrenceData.StartDateLocal.Value, recurrenceData.RecurringStartTimeZone);
						}
					}
					break;
				default:
					if (errorCollector != null)
					{
						errorCollector.ErrorType = ReportServiceErrorType.ValidationError;
						errorCollector.Errors.Add((NoResString)"Invalid Task Period");
					}
					break;
			}

			if (!Recurrence.TaskPeriodCountInfo.ReadOnly)
			{
				Recurrence.TaskPeriodCount = recurrenceData.TaskPeriodCount;
			}

			if (recurrenceData.RecurringStartTimeLocal.HasValue)
			{
				Recurrence.RecurringStartTimeLocal = ConvertTimeZoneToLocalTime(recurrenceData.RecurringStartTimeLocal.Value, recurrenceData.RecurringStartTimeZone);
			}

			Recurrence.EndAfterCount = recurrenceData.EndAfterCount;
			if (!Recurrence.EndAfter && recurrenceData.EndDateLocal.HasValue)
			{
				Recurrence.EndDateLocalForUser = ConvertTimeZoneToLocalTime(recurrenceData.EndDateLocal.Value, recurrenceData.RecurringStartTimeZone);
			}
		}

		DateTime ConvertTimeZoneToLocalTime(DateTime dateTime, double? offSetHours)
		{
			if (!offSetHours.HasValue)
			{
				return dateTime;
			}
			var unspecifiedDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
			var dateTimeLocal = new DateTimeOffset(unspecifiedDateTime, TimeSpan.FromHours(offSetHours.Value));
			return Env.Time.GetLocalTimeFromUtc(dateTimeLocal.UtcDateTime);
		}

		internal void FillRecipientsData(List<ReportScheduleRecipientData> recipientsData)
		{
			var deletedRecipientPks = Recipients.Select(r => r.PK.ToGuid()).Except(recipientsData.Select(r => r.Identifier)).ToList();
			foreach (var deletedRecipientPk in deletedRecipientPks)
			{
				Recipients.RemoveAndDelete(Recipients.FindByPK(deletedRecipientPk));
			}

			foreach (var recipientData in recipientsData)
			{
				var recipient = Recipients.FindByPK(recipientData.Identifier) as ReportScheduleTaskRecipient
				?? Recipients.AddNew();

				recipient.S6_DeliveryToType = recipientData.DeliveryRecipientType;
				if (!recipient.S6_OHInfo.ReadOnly)
				{
					recipient.S6_OH = recipientData.S6_OH;
				}

				if (!recipient.ContactNameInfo.ReadOnly)
				{
					recipient.ContactName = recipientData.ContactName;
				}

				if (!recipient.S6_GS_NKRecipientInfo.ReadOnly)
				{
					recipient.S6_GS_NKRecipient = recipientData.S6_GS_NKRecipient;
				}

				if (!recipient.S6_GGInfo.ReadOnly)
				{
					recipient.S6_GG = recipientData.S6_GG;
				}

				if (!recipient.S6_DeliveryMethodInfo.ReadOnly)
				{
					recipient.S6_DeliveryMethod = recipientData.S6_DeliveryMethod;
				}

				if (!recipient.S6_AttachmentTypeInfo.ReadOnly)
				{
					recipient.S6_AttachmentType = recipientData.S6_AttachmentType;
				}

				if (!recipient.S6_SQInfo.ReadOnly)
				{
					recipient.S6_SQ = recipientData.S6_SQ;
				}

				if (!recipient.S6_EmptyReportDeliveryOptionsInfo.ReadOnly)
				{
					recipient.S6_EmptyReportDeliveryOptions = recipientData.S6_EmptyReportDeliveryOptions;
				}

				if (!recipient.EmailFromAddressInfo.ReadOnly)
				{
					recipient.EmailFromAddress = recipientData.EmailFromAddress;
				}

				if (!recipient.ToFaxOrEmailInfo.ReadOnly)
				{
					recipient.ToFaxOrEmail = recipientData.ToFaxOrEmail;
				}

				if (!recipient.S6_CarbonCopyRecipientsAsStringInfo.ReadOnly && recipientData.CarbonCopyRecipients != null)
				{
					recipient.S6_CarbonCopyRecipientsAsString = string.Join(",", recipientData.CarbonCopyRecipients);
				}

				if (!recipient.S6_BlindCarbonCopyRecipientsAsStringInfo.ReadOnly && recipientData.BlindCarbonCopyRecipients != null)
				{
					recipient.S6_BlindCarbonCopyRecipientsAsString = string.Join(",", recipientData.BlindCarbonCopyRecipients);
				}

				if (!recipient.S6_FtpAddressInfo.ReadOnly)
				{
					recipient.S6_FtpAddress = recipientData.S6_FtpAddress;
				}

				if (!recipient.S6_UserNameInfo.ReadOnly)
				{
					recipient.S6_UserName = recipientData.S6_UserName;
				}

				if (!recipient.S6_PasswordInfo.ReadOnly)
				{
					recipient.S6_Password = recipientData.S6_Password;
				}

				if (recipient is IServiceDtoMapper mapper)
				{
					mapper.Identifier = recipientData.Identifier.ToString();
				}
			}
		}

		internal ReportScheduleTaskData ExtractData()
		{
			var data = new ReportScheduleTaskData();
			data.ScheduleDescription = S5_ScheduleDescription;
			data.IsActive = S5_IsActive;
			data.Branch = S5_GB.IsValid ? S5_GB.ToGuid() : default;
			data.UserFk = UserFK.IsValid ? UserFK.ToGuid() : default;
			if (S5_DateScheduleFirstRun.IsValid)
			{
				data.DateScheduleFirstRun = GetLocalTimeOfCurrentNKUNLOCO(S5_DateScheduleFirstRun.ToDateTime(), Branch.GB_RL_NKHomePort);
			}
			data.IsPrivate = S5_IsPrivate;
			data.ScheduleActualRunCount = S5_ScheduleActualRunCount;
			if (CalcNextRunTimeLocal.IsValid)
			{
				data.NextRunTimeLocal = CalcNextRunTimeLocal.ToDateTime();
			}

			data.Recurrence = ExtractRecurrenceData(Recurrence);
			return data;
		}

		DateTime GetLocalTimeOfCurrentNKUNLOCO(DateTime dateTime, string unlocoFrom)
		{
			if (string.IsNullOrEmpty(unlocoFrom) || Env.Instance.CurrentNKUNLOCO == unlocoFrom)
			{
				return dateTime;
			}
			return Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(unlocoFrom, dateTime, Env.Instance.CurrentNKUNLOCO);
		}

		ScheduleRecurrenceData ExtractRecurrenceData(StmScheduleTaskRecurrence recurrence)
		{
			var data = new ScheduleRecurrenceData();
			if (recurrence.StartDateLocalForUser.IsValid)
			{
				data.StartDateLocal = recurrence.StartDateLocalForUser.ToDateTime();
			}
			data.RecurrenceType = recurrence.TaskPeriod;
			data.IsWeekDayOnly = recurrence.WeekDaysOnly;
			data.WeekDayList = recurrence.WeekDays.Select(o => (bool)o).ToArray();
			data.IsMonthlyDay = recurrence.MonthlyDay;
			data.IsMonthlyLastDay = recurrence.MonthLastDay;
			data.DayOfMonth = recurrence.DayOfMonth;
			data.WeekDayOccurrenceNumber = recurrence.WeekDayOccurrenceNumber;
			data.WeekDayNumber = recurrence.DayNumber;
			data.IsAccountingDay = recurrence.AccountingDay;
			data.IsAccountingLastDay = recurrence.AccountingLastDay;
			data.DayOfAccountingPeriod = recurrence.DayOfAccountingPeriod;
			data.IsYearlyDay = recurrence.YearlyEvery;
			data.EveryMonthNumber = recurrence.MonthNumber;
			data.MonthNumber = recurrence.MonthNumber;
			data.TaskPeriodCount = recurrence.TaskPeriodCount;
			if (recurrence.RecurringStartTimeLocal.IsValid)
			{
				data.RecurringStartTimeLocal = recurrence.RecurringStartTimeLocal.ToDateTime();
			}
			data.RecurringStartTimeZone = StmScheduleTaskRecurrence.CurrentBranchUtcOffset;
			data.EndAfterCount = recurrence.EndAfterCount;
			if (recurrence.EndDateLocalForUser.IsValid)
			{
				data.EndDateLocal = recurrence.EndDateLocalForUser.ToDateTime();
			}
			return data;
		}

		internal List<ReportScheduleRecipientData> ExtractRecipientsData()
		{
			var data = new List<ReportScheduleRecipientData>();

			foreach (ReportScheduleTaskRecipient recipient in Recipients)
			{
				data.Add(new ReportScheduleRecipientData
				{
					DeliveryRecipientType = recipient.S6_DeliveryToType,
					S6_OH = recipient.S6_OH.IsValid ? recipient.S6_OH.ToGuid() : default,
					ContactName = recipient.ContactName,
					S6_GS_NKRecipient = recipient.S6_GS_NKRecipient,
					S6_GG = recipient.S6_GG.IsValid ? recipient.S6_GG.ToGuid() : default,
					S6_DeliveryMethod = recipient.S6_DeliveryMethod,
					S6_AttachmentType = recipient.S6_AttachmentType,
					S6_SQ = recipient.S6_SQ.IsValid ? recipient.S6_SQ.ToGuid() : default,
					S6_EmptyReportDeliveryOptions = recipient.S6_EmptyReportDeliveryOptions,
					EmailFromAddress = recipient.EmailFromAddress,
					ToFaxOrEmail = recipient.ToFaxOrEmail,
					CarbonCopyRecipients = recipient.CarbonCopyRecipients.Emails,
					BlindCarbonCopyRecipients = recipient.BlindCarbonCopyRecipients.Emails,
					S6_FtpAddress = recipient.S6_FtpAddress,
					S6_UserName = recipient.S6_UserName,
					S6_Password = recipient.S6_Password,
					Identifier = recipient.PK.ToGuid(),
				});
			}
			return data;
		}

		#endregion

		#region DEBUG Test Points
#if DEBUG

		internal bool IsErrorThatShouldRetryForTesting(Exception ex)
		{
			return IsErrorThatShouldRetry(ex);
		}

		internal TimeSpan RetryWaitPeriodForTesting
		{
			get { return RetryWaitPeriod; }
		}

		public ZBlob SerializeForTesting(DeliveryInstructions instructions, Report report)
		{
			return this.Serialize(instructions, report);
		}

		internal void AddScheduleRecipientsToInstructionsForTesting(DeliveryInstructions deliveryInstructions, ReportScheduleTaskRecipient[] reportScheduleTaskRecipient, NotificationBuffer notifications)
		{
			AddScheduleRecipientsToInstructions(deliveryInstructions, reportScheduleTaskRecipient, notifications);
		}

		internal DeliveryInstructions[] GetDeliveryInstructionsForTesting(ReportSerializationInfo emptyReportInfo, DocumentPack pack, NotificationBuffer notifications)
		{
			return GetDeliveryInstructions(emptyReportInfo, pack, notifications);
		}

		internal IEnumerable<DeliveryInstructions> GetDeliveryInstructionsAndAddWarningsIfEmptyForTesting(INotifications notifications, DocumentPack pack, ReportSerializationInfo reportInfo)
		{
			return GetDeliveryInstructionsAndAddWarningsIfEmpty(notifications, pack, reportInfo);
		}

		internal DocDeliveryContact CreateSingleDocDeliveryContactForTesting(StmScheduleTaskRecipient recipient, INotifications notifications)
		{
			return CreateSingleDocDeliveryContact(recipient, notifications);
		}

#endif
		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentTableCode = StmMenuItemSchema.Constants.Prefix;
			S5_ParentID = Guid.NewGuid();

			HasChanges = false;
		}
#endif
		#endregion
	}
}
