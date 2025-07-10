using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeProperty(IncidentMainSchema.Constants.IM_IncidentNumber), DescriptionProperty(IncidentMainSchema.Constants.IM_Description)]
	public partial class ProfessionalServicesQuote :
		IncidentMainBase,
		IJobInvoicingPlugIn
	{
		#region Emailing

		public virtual bool AllowOutlookEmailSending
		{
			get { return true; }
		}

		/// <summary>
		/// Get an Outlook email item that will be sent to the client. An ApplicationException is thrown with a user-friendly message
		/// if there is a problem.
		/// </summary>
		public IOutlookMailItem GetNewOutlookMailItem()
		{
			CheckIsInValidStateToSendEmail();

			IOutlookMailItem mailItem = NewOutlookMailItem();
			return SetupAndReturnMailItem(mailItem);
		}

		protected virtual IOutlookMailItem NewOutlookMailItem()
		{
			OutlookApplication outlook = new OutlookApplication();
			return outlook.CreateMailItem(null);
		}

		protected string ReplaceEmailTags(string body, bool useSupportAsSignOff)
		{
			string details = ORtfTextUtil.RtfToText(IM_Details).Trim();
			if (details.Length == 0)
			{
				details = IM_Description;
			}

			ZString contactName = (Contact.OC_Salutation.IsEmpty) ? Contact.OC_ContactName : Contact.OC_Salutation;
			body = body.Replace("<ContactName>", contactName.CapitaliseFirstLettersOfWords());
			body = body.Replace("<Details>", details);
			body = body.Replace("<IncidentNumber>", IM_IncidentNumber);
			body = body.Replace("<ClientFullName>", Client.OH_FullName);
			body = body.Replace("<ClientPhone>", IM_Calc_BranchPhone);
			body = body.Replace("<ClientFax>", IM_Calc_BranchFax);
			body = body.Replace("<CargoWisePhone>", GlbCompany.CurrentCompany.GC_Phone);
			body = body.Replace("<CargoWiseFax>", GlbCompany.CurrentCompany.GC_Fax);
			if (!useSupportAsSignOff)
			{
				body = body.Replace("<LoginFullName>", GlbStaff.CurrentUser.GS_FullName.CapitaliseFirstLettersOfWords());
				body = body.Replace("<LoginEmail>", GlbStaff.CurrentUser.GS_EmailAddress);
			}
			else
			{
				body = body.Replace("<LoginFullName>", IncidentConstants.SupportDisplayName);
				body = body.Replace("<LoginEmail>", SupportIncidentLookups.SupportEmailAddress);
			}

			return body;
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();

			base.OnSaving();

			PopulateIncidentNumberOnSaving();
			PostChangeEvents();
		}

		void PopulateIncidentNumberOnSaving()
		{
			if (!IsInDatabase)
			{
				IM_IncidentNumber = GetNewIncidentNumber();
			}
		}

		#endregion

		#region Properties

		public string IncidentTypeName
		{
			get { return IncidentConstants.GetIncidentTypeDescription(IM_IncidentType); }
		}

		#region IM_Calc_ContactPhone

		public ZString IM_Calc_ContactPhone
		{
			get { return (Contact != null) ? Contact.OC_Phone : ZString.Empty; }
		}

		public ZPropertyInfo IM_Calc_ContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(IM_Calc_ContactPhone)); }
		}

		#endregion

		#region IM_Calc_ContactEmail

		public ZString IM_Calc_ContactEmail
		{
			get { return (Contact != null) ? Contact.OC_Email : ZString.Empty; }
		}

		public ZPropertyInfo IM_Calc_ContactEmailInfo
		{
			get { return GetZPropertyInfo(nameof(IM_Calc_ContactEmail)); }
		}

		#endregion

		#region IM_ClientContractStatus

		public ZString IM_ClientContractStatus
		{
			get { return "Not functional on old incidents."; }
		}

		public ZPropertyInfo IM_ClientContractStatusInfo
		{
			get { return base.GetZPropertyInfo(nameof(IM_ClientContractStatus)); }
		}

		#endregion

		#region IM_IsEmailSent

		public ZBool IM_IsEmailSent
		{
			get
			{
				if (!IsIM_IsEmailSentCalculated)
				{
					IsIM_IsEmailSentCalculated = true;
					fIM_IsEmailSent = (StmALogEntryLocator.Instance.GetLastPostEventOfType(this, AutoEvents.IncidentEmailSent) != null);
				}

				return fIM_IsEmailSent;
			}
		}

		public ZPropertyInfo IM_IsEmailSentInfo
		{
			get { return base.GetZPropertyInfo(nameof(IM_IsEmailSent)); }
		}

		bool IsIM_IsEmailSentCalculated;
		bool fIM_IsEmailSent;

		#endregion

		#region IM_TeamDesc

		public ZString IM_TeamDesc
		{
			get { return Team == null ? ZString.Empty : Team.GG_Desc; }
		}

		public ZPropertyInfo IM_TeamDescInfo
		{
			get { return GetZPropertyInfo(nameof(IM_TeamDesc)); }
		}

		#endregion

		#region IM_CurrentlyAssignedToInitials

		public ZString IM_CurrentlyAssignedToInitials
		{
			get { return AssignedToCurrent == null ? ZString.Empty : AssignedToCurrent.GS_Code; }
		}

		public ZPropertyInfo IM_CurrentlyAssignedToInitialsInfo
		{
			get { return GetZPropertyInfo(nameof(IM_CurrentlyAssignedToInitials)); }
		}

		#endregion

		#region Client Header

		public virtual ZGuid ClientHeader
		{
			get { return IM_OH_Client; }
		}

		public ZPropertyInfo ClientHeaderInfo
		{
			get { return GetZPropertyInfo(nameof(ClientHeader)); }
		}

		#endregion

		#region Description Header

		public ZString DescriptionHeader
		{
			get { return IM_Description; }
		}

		public ZPropertyInfo DescriptionHeaderInfo
		{
			get { return GetZPropertyInfo(nameof(DescriptionHeader)); }
		}

		#endregion

		protected override ReadOnlyCodeDescriptionPairList Statuses
		{
			get { return Lookups.StatusList; }
		}

		#region IM_Details

		public override ZBlob IM_Details
		{
			get { return base.IM_Details; }
			set
			{
				base.IM_Details = value;

				if (IsDefaultingDescriptionFromDetails && IM_Description.Trim().Length == 0)
				{
					IM_Description = GetDetailsFirstLine();
				}
			}
		}

		protected virtual bool IsDefaultingDescriptionFromDetails
		{
			get { return true; }
		}

		ZString GetDetailsFirstLine()
		{
			string result = ORtfTextUtil.GetRtfFirstTextLine(IM_Details, IM_DescriptionInfo.MaxLength);

			// Remove the leading Env.CurrentUser.InitialsAndDateTime, if it exists
			int index = result.IndexOf(":", StringComparison.Ordinal);

			if (index != -1)
			{
				if (index + 5 < result.Length && result[index + 3] == ':')
				{
					result = result.Substring(index + 5);
				}
			}

			return result;
		}

		#endregion

		#region IM_DetailsAsText

		public ZString IM_DetailsAsText
		{
			get
			{
				return (ZString)ORtfTextUtil.RtfToText(IM_Details);
			}
		}

		public ZPropertyInfo IM_DetailsAsTextInfo
		{
			get { return GetZPropertyInfo(nameof(IM_DetailsAsText)); }
		}

		#endregion

		#region IM_Status

		public override ZString IM_Status
		{
			get { return base.IM_Status; }
			set
			{
				if (IM_Status != value)
				{
					base.IM_Status = value;

					if (!IsCopying)
					{
						// Update the date field when closing
						if (value == IncidentConstants.IncidentStatus.Closed)
						{
							IM_CloseTimeUtc = ZDateTime.UtcNow;
						}
						else if (value != IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification && !IsClosedOrCancelled)
						{
							IM_ResolutionCode = new ZString(IM_ResolutionCodeInfo.DatabaseDefault);
						}

						if (!IsClosedOrCancelled)
						{
							IM_CloseTimeUtc = ZDateTime.Empty;
						}

						// When changed to unassigned, there should be no AssignedToCurrent person
						if (value == IncidentConstants.IncidentStatus.Unassigned)
						{
							IM_GS_NKAssignedToCurrent = String.Empty;
						}
					}

					if (ReadOnly && !IsClosedOrCancelled)
					{
						ReadOnly = false;
					}
				}
			}
		}

		#endregion

		public bool IM_Resolution_ReadOnly
		{
			get
			{
				return !(IM_Status == IncidentConstants.IncidentStatus.FinishedPendingCodeReview ||
				  IM_Status == IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification ||
				  IM_Status == IncidentConstants.IncidentStatus.Closed ||
				  IM_Status == IncidentConstants.IncidentStatus.FinishedPendingCheckIn ||
				  IM_Status == IncidentConstants.IncidentStatus.FinishedCheckedInAndPendingDeploy);
			}
		}

		public bool IM_IncidentType_ReadOnly
		{
			get { return IsInDatabase; }
		}

		public bool IM_OC_Contact_ReadOnly
		{
			get { return (IM_OA_BranchAddress.IsEmpty || !IM_OA_BranchAddress.IsValid); }
		}

		#region IM_CallbackBy

		public bool IM_CallbackBy_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region IM_IncidentNumberInfo

		[MaxLength(18)]
		[ReadOnly(true)]
		public override ZString IM_IncidentNumber
		{
			get { return base.IM_IncidentNumber; }
			set { base.IM_IncidentNumber = value; }
		}

		#endregion

		#region IM_CloseTimeUtc

		public bool IM_CloseTimeUtc_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region Can the User Assign/Close/Cancel Incident

		protected bool CanAssignToPerson
		{
			get { return !IsClosedOrCancelled; }
		}

		protected bool CanCloseIncident
		{
			get { return ((IM_Status != IncidentConstants.IncidentStatus.Unassigned) && !IsClosedOrCancelled); }
		}

		protected bool CanCancelIncident
		{
			get { return !IsClosedOrCancelled; }
		}

		#endregion

		#region Object State (IsCurrentlyAssignedChanged, IsClosing, etc)

		/// <summary>
		/// Has the IM_CurrentlyAssignedTo changed since the underlying record was retrieved?
		/// </summary>
		protected ZBool IsCurrentlyAssignedChanged
		{
			get { return OriginalIM_GS_NKAssignedToCurrent != IM_GS_NKAssignedToCurrent; }
		}

		/// <summary>
		/// Has the incident been marked as fixed (changed from some other state, other than closed) since the record was retrieved?
		/// </summary>
		protected bool IsBeingMarkedAsFixed
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification &&
					!CheckClosedOrCancelled(OriginalIM_Status) &&
					IM_Status == IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification);
			}
		}

		/// <summary>
		/// Has the incident been re-opened (changed from closed state) since the underlying record was retrieved?
		/// </summary>
		protected bool IsReopening
		{
			get { return (CheckClosedOrCancelled(OriginalIM_Status) && !IsClosedOrCancelled); }
		}

		/// <summary>
		/// Has the incident been closed (changed from some other state to the closed state) since the record was retrieved?
		/// </summary>
		/// 
		protected bool IsClosing
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.Closed &&
					IM_Status == IncidentConstants.IncidentStatus.Closed);
			}
		}

		bool IsAwaitingDevelopmentWork
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.AwaitingDevelopmentWork &&
					IM_Status == IncidentConstants.IncidentStatus.AwaitingDevelopmentWork);
			}
		}

		bool IsAwaitingSchedulingTeam
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.AwaitingSchedulingTeam &&
					IM_Status == IncidentConstants.IncidentStatus.AwaitingSchedulingTeam);
			}
		}

		bool IsAwaitingTeamScheduler
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.AwaitingTeamScheduler &&
					IM_Status == IncidentConstants.IncidentStatus.AwaitingTeamScheduler);
			}
		}

		/// <summary>
		/// Has the incident been closed (changed from some other state to the closed state) since the record was retrieved?
		/// </summary>
		public ZBool IsBeingCancelled
		{
			get
			{
				return (OriginalIM_Status != IncidentConstants.IncidentStatus.Cancelled &&
					IM_Status == IncidentConstants.IncidentStatus.Cancelled);
			}
		}

		bool IsStatusChanging
		{
			get { return (OriginalIM_Status != IM_Status); }
		}

		public bool IsClosedOrCancelled
		{
			get { return CheckClosedOrCancelled(IM_Status); }
		}

		public bool IsClosed
		{
			get { return (IM_Status == IncidentConstants.IncidentStatus.Closed); }
		}

		bool CheckClosedOrCancelled(string statusCode)
		{
			return ((statusCode == IncidentConstants.IncidentStatus.Closed) || (statusCode == IncidentConstants.IncidentStatus.Cancelled));
		}

		protected ZString OriginalIM_Status;
		ZString OriginalIM_GS_NKAssignedToCurrent;

		#endregion

		#region User Initiated Actions (MarkIncidentAsFixed, CloseIncident, CancelIncident, etc)

		public void MarkIncidentAsFixed(string resolutionCode)
		{
			IM_ResolutionCode = resolutionCode;
			IM_Status = IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification;
		}

		public void CloseIncident()
		{
			IM_Status = IncidentConstants.IncidentStatus.Closed;
		}

		public void CancelIncident()
		{
			IM_Status = IncidentConstants.IncidentStatus.Cancelled;
		}

		public void ReopenIncident()
		{
			IM_Status = ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress;
		}

		#endregion

		#region Email Notify Sent

		/// <summary>
		/// Log an event that an email has been sent to the client.
		/// </summary>
		public void NotifyMailSent()
		{
			NotifyMailSent("");
		}

		public void NotifyMailSent(string reference)
		{
			AddLog(Events.IncidentEmailSent, reference);
			IsIM_IsEmailSentCalculated = false;
			IM_IsEmailSentInfo.RefreshBinding();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected void CheckIsInValidStateToSendEmail()
		{
			if (HasChanges)
			{
				throw new ApplicationException("Changes have been made to this Incident. You must save before sending an Email.");
			}
			else
			{
				CheckAdditionalFieldsAreValidForSendingEmail();
			}
		}

		protected string GetIncidentEmailSubject(string header)
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}Incident #{1} from Eagle Datamation International ({2})",
				header, IM_IncidentNumber, IM_Description);
		}

		#endregion

		#region Post Change Events

		void PostChangeEvents()
		{
			string fullName = "";
			Logs.StartAddingLogs();

			if (AssignedToCurrent != null)
			{
				fullName = AssignedToCurrent.GS_FullName;
			}

			if (IsCurrentlyAssignedChanged)
			{
				AddLog(Events.AssignedUserChanged, fullName);
			}

			if (IsClosing)
			{
				AddLog(Events.IncidentClosed, fullName);
			}

			if (IsAwaitingDevelopmentWork || IsAwaitingSchedulingTeam || IsAwaitingTeamScheduler)
			{
				AddLog(Events.HoldAwaiting, fullName);
			}

			if (IsBeingCancelled)
			{
				AddLog(Events.JobClose, fullName);
			}

			if (IsBeingMarkedAsFixed)
			{
				AddLog(Events.Delivered, fullName);
			}

			if (IsReopening)
			{
				AddLog(Events.IncidentReopened, fullName);
			}

			if (IsInDatabase && !IsCurrentlyAssignedChanged && !IsClosing && !IsBeingCancelled && !IsReopening && !IsBeingMarkedAsFixed &&
				!IsAwaitingDevelopmentWork && !IsAwaitingSchedulingTeam && !IsAwaitingTeamScheduler)
			{
				AddLog(Events.EditedARecord, fullName);
			}

			if (IsStatusChanging)
			{
				AddLog(Events.StatusChange, string.Format(CultureInfo.CurrentCulture, "'{0}' changed to '{1}'", OriginalIM_Status, IM_Status));
			}
		}

		void AddLog(Event @event, ZString reference)
		{
			Logs.AddLog(@event, reference);
		}

		#endregion

		#region Email Sender

		protected EmailDef GetNewIncidentManagerEmailTemplate()
		{
			EmailDef result = new EmailDef();

			result.FromDisplayName = "Incident Manager";
			result.FromAddress = IncidentConstants.ManagerEmailAddress;
			result.ReplyTo = IncidentConstants.ManagerEmailAddress;

			return result;
		}

		#endregion

		#region Logs

		public new IncidentLogs Logs
		{
			get { return (IncidentLogs)base.Logs; }
		}

		protected override Logs GetNewLogs()
		{
			return new IncidentLogs(this);
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return BillingNumberPrefix + IM_IncidentNumber; }
		}

		protected virtual string BillingNumberPrefix
		{
			get { return string.Empty; }
		}

		#endregion

		#region IJobHeaderParent
			
		protected override void SetJobNumberFieldOnSaving()
		{
			PopulateIncidentNumberOnSaving();
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		IncidentMainInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = GetNewInvoicingSupporter()); }
		}

		#endregion
	}

	#region Invoicing Supporter

	public class IncidentMainInvoicingSupporter : JobInvoicingSupporter
	{
		public IncidentMainInvoicingSupporter(IncidentMainBase parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly IncidentMainBase Parent;

		protected virtual JobInvoicingConsumerType ConsumerTypeCore
		{
			get { return null; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return ConsumerTypeCore; }
		}

		public override OrgHeader Consignee
		{
			get { return Parent.Client; }
		}

		public override OrgHeader Consignor
		{
			get { return Parent.Client; }
		}

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return EDISecurityCheckpoints.ProfessionalServicesQuoteAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return EDISecurityCheckpoints.ProfessionalServicesQuoteJobInvoicing;
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}
	}

	#endregion

}
