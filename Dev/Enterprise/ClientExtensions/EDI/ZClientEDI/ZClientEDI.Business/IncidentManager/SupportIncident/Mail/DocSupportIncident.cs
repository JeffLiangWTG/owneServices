using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DocSupportIncident : DocBaseWrapper
	{
		DocSupportIncident(SupportIncident objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public static DocSupportIncident New(SupportIncident objectToWrap, BusinessObjectFactory factory)
		{
			return new DocSupportIncident(objectToWrap, factory);
		}

		[DocumentField("Html Style Sheet")]
		public ZString HtmlStyleSheet
		{
			get { return SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value; }
		}

		[DocumentField("Current Date")]
		public ZString CurrentDate
		{
			get { return ZDateTime.Now.ToShortDateString(); }
		}

		[DocumentField("The Contact's full name")]
		public ZString ContactName
		{
			get { return WrappedObject != null && WrappedObject.Contact != null ? WrappedObject.Contact.OC_ContactName.CapitaliseFirstLettersOfWords() : ""; }
		}

		[DocumentField("The Contact's phone number")]
		public ZString ContactPhoneForDisplay
		{
			get { return WrappedObject != null ? WrappedObject.ContactPhoneForDisplay : ZString.Empty; }
		}

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get
			{
				ZString result = ZString.Empty;
				if (WrappedObject != null)
				{
					OrgContact contact = WrappedObject.Contact;
					if (contact == null)
					{
						result = "Dear Client";
					}
					else
					{
						result = contact.OC_Salutation.IsEmpty ? contact.OC_ContactName : contact.OC_Salutation;
						result = result.CapitaliseFirstLettersOfWords();
					}
				}
				return result;
			}
		}

		[DocumentField("The Client's code")]
		public ZString ClientCode
		{
			get { return WrappedObject != null ? WrappedObject.ClientCode : ZString.Empty; }
		}

		[DocumentField("The Client's full name")]
		public ZString ClientName
		{
			get
			{
				var incident = WrappedObject;
				var clientCompanyName = incident?.ClientCompany?.LCC_Name ?? ZString.Empty;
				if (!clientCompanyName.IsEmpty)
				{
					return clientCompanyName;
				}
				else
				{
					return incident?.ClientName ?? ZString.Empty;
				}
			}
		}

		[DocumentField("Your company's name")]
		public ZString CurrentCompanyName
		{
			get { return CurrentCompany.Name; }
		}

		[DocumentField("Your company's OrgProxy Name")]
		public ZString CurrentCompanyOrgProxyName
		{
			get { return CurrentCompany.Organisation.Name; }
		}

		[DocumentField("CurrentCompanyWebSite")]
		public ZString CurrentCompanyWebSite
		{
			get { return CurrentCompany.WebAddress; }
		}

		[DocumentField("Incident's branch phone")]
		public ZString IncidentBranchPhone
		{
			get { return WrappedObject != null && WrappedObject.BranchAddress != null ? WrappedObject.BranchAddress.OA_Phone : ZString.Empty; }
		}

		[DocumentField("Your name")]
		public ZString CurrentUserName
		{
			get { return CurrentUser.FullName; }
		}

		[DocumentField("Your title")]
		public ZString CurrentUserTitle
		{
			get { return CurrentUser.Title; }
		}

		[DocumentField("Your email address")]
		public ZString CurrentUserEmailAddress
		{
			get { return CurrentUser.EmailAddress; }
		}

		[DocumentField("Incident Number")]
		public ZString IncidentNumber
		{
			get { return WrappedObject != null ? ((ProcessManagement.Business.IWorkItemRelatedItem)WrappedObject).Number : ZString.Empty; }
		}

		[DocumentField("Client Reference Number")]
		public ZString ClientReferenceNumber
		{
			get { return WrappedObject != null ? WrappedObject.IM_ClientIncidentReference : ZString.Empty; }
		}

		[DocumentField("Incident Summary")]
		public ZString Summary
		{
			get { return WrappedObject != null ? ((ProcessManagement.Business.IWorkItemRelatedItem)WrappedObject).ItemDescription : ZString.Empty; }
		}

		[DocumentField("Incident Detailed Description")]
		public ZString DetailedDescription
		{
			get { return WrappedObject != null ? WrappedObject.DetailNoteText : ZString.Empty; }
		}

		[DocumentField("Incident Criticality Changed from")]
		public ZString CriticalityChangedFrom
		{
			get { return WrappedObject != null ? WrappedObject.PreviouslySavedCriticality : ZString.Empty; }
		}

		[DocumentField("Incident Criticality")]
		public ZString Criticality
		{
			get { return WrappedObject != null ? WrappedObject.IM_Priority : ZString.Empty; }
		}

		[DocumentField("Detailed Disposition Description")]
		public ZString DetailedDispositionDescription
		{
			get { return WrappedObject.IM_ResolutionCodeDescription; }
		}

		[DocumentField("Resolution Note")]
		public ZString ResolutionNoteText
		{
			get { return WrappedObject.ResolutionNoteText.IsEmpty ? string.Empty : "\r\n\r\nComments: " + WrappedObject.ResolutionNoteText; }
		}

		[DocumentField("Chargeable Work Billing Notice")]
		public ZString ChargeableWorkBillingNotice
		{
			get { return WrappedObject.IM_ChargableWork ? "\r\n\r\n" + WrappedObject.ChargeableWorkNoticeText : string.Empty; }
		}

		[DocumentField("Support Incident Close Type")]
		public ZString SupportIncidentCloseType
		{
			get { return WrappedObject.IncidentCloseTypeText; }
		}

		[DocumentField("Current Assigned Staff Full Name")]
		public ZString CurrentAssignedStaffFullName
		{
			get
			{
				GlbStaff staff = WrappedObject != null ? WrappedObject.AssignedToCurrent : null;
				return (staff != null) ? staff.GS_FullName.ToString() : SupportIncident.SupportDisplayName;
			}
		}

		[DocumentField("Current Assigned Staff Email Address")]
		public ZString CurrentAssignedStaffEmailAddress
		{
			get
			{
				GlbStaff staff = WrappedObject != null ? WrappedObject.AssignedToCurrent : null;
				return (staff != null) ? staff.GS_EmailAddress.ToString() : SupportIncident.SupportEmailAddress;
			}
		}

		[DocumentField("Closure Date")]
		public ZString ClosureDateText => WrappedObject.ClosureDateText;

		[DocumentField("Closing Staff Code")]
		public ZString ClosingStaffCodeText => WrappedObject.ClosingStaffCodeText;

		[DocumentField("Disposition")]
		public ZString DispositionText => WrappedObject.DispositionText;

		[DocumentField("Resolution Comment")]
		public ZString ResolutionCommentText => WrappedObject.ResolutionCommentText;

		[DocumentField("Client Reference")]
		public ZString ClientReference => (WrappedObject?.Request?.INC_ClientReference) ?? ZString.Empty;

		[DocumentField("Incident Description")]
		public ZString IncidentDescription => WrappedObject?.IM_Description ?? ZString.Empty;

		[DocumentField("Incident Type Descritpion")]
		public ZString IncidentTypeDescription => IncidentConstants.GetIncidentTypeDescription(WrappedObject.IM_IncidentType);

		[DocumentField("Resolved to Closed Day")]
		public ZString ResolvedToClosedDay => WrappedObject.ResolvedToClosedDay.ToString();

		[DocumentField("Incident Detailed Description")]
		public ZString IncidentDetailedDescription => WrappedObject?.HumanReadableName ?? ZString.Empty;

		public new SupportIncident WrappedObject
		{
			get { return (SupportIncident)base.WrappedObject; }
		}
	}
}

