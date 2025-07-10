using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocSalesCall : DocumentWrapper
	{
		DocSalesCall(OrgSalesCall orgSalesCall, BusinessObjectFactory factoryToWrap)
			: base(orgSalesCall, factoryToWrap)
		{
		}

		public static DocSalesCall New(OrgSalesCall orgSalesCall, BusinessObjectFactory factoryToWrap)
		{
			return (orgSalesCall != null) ? new DocSalesCall(orgSalesCall, factoryToWrap) : null;
		}

		OrgSalesCall OrgSalesCall
		{
			get { return (OrgSalesCall)WrappedObject; }
		}

		public override string ToString()
		{
			return CallSummary;
		}

		#region Sales Call Properties

		public ZDateTime CallDate
		{
			get { return OrgSalesCall.OQ_CallDate; }
		}

		public ZString CallType
		{
			get
			{
				if (!OrgSalesCall.OQ_TypeOfCall.IsEmpty)
				{
					return OrgSalesCall.OQ_TypeOfCall + " - " + OrgSalesCall.Lookups.OQ_TypeOfCall_List.GetDescriptionFromCode(OrgSalesCall.OQ_TypeOfCall);
				}
				return "";
			}
		}

		public ZString CallStatus
		{
			get
			{
				if (!OrgSalesCall.OQ_Status.IsEmpty)
				{
					return OrgSalesCall.OQ_Status + " - " + OrgSalesCall.Lookups.OQ_Status_List.GetDescriptionFromCode(OrgSalesCall.OQ_Status);
				}
				return "";
			}
		}

		public ZDateTime NextCallDate
		{
			get { return OrgSalesCall.OQ_NextCall; }
		}

		public ZString CallContact
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrgSalesCall.Contact != null)
				{
					DocContacts contact = DocContacts.New(OrgSalesCall.Contact, Factory);
					result += ToTitleCase(contact.ContactName);
				}
				return result;
			}
		}

		[DocumentField("Staff Coordinator Code")]
		public ZString StaffCoordinatorCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrgSalesCall.SalesRep != null)
				{
					result += ToTitleCase(OrgSalesCall.SalesRep.GS_Code);
				}
				return result;
			}
		}

		[DocumentField("Staff Coordinator")]
		public ZString StaffCoordinator
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrgSalesCall.SalesRep != null)
				{
					result += ToTitleCase(OrgSalesCall.SalesRep.GS_FullName);
				}
				return result;
			}
		}

		public ZString OtherAttendee
		{
			get
			{
				return AddAdditionalAttendees(ZString.Empty, OrgSalesCall.AdditionalAttendeesOther);
			}
		}

		public ZString Attendees
		{
			get
			{
				ZString result = ZString.Empty;
				result = AddAdditionalAttendees(result, OrgSalesCall.AdditionalAttendeesContact);
				result = AddAdditionalAttendees(result, OrgSalesCall.AdditionalAttendeesStaff);
				result = AddAdditionalAttendees(result, OrgSalesCall.AdditionalAttendeesOther);
				return result;
			}
		}

		ZString AddAdditionalAttendees(ZString result, OrgSalesCallAdditionalAttendeeCollection collection)
		{
			foreach (OrgSalesCallAdditionalAttendee additionalAttendee in collection)
			{
				ZString name = additionalAttendee.Name;
				if (!name.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						result += "\n";
					}

					name = ToTitleCase(name);
					result += name;
				}
			}
			return result;
		}

		ZString ToTitleCase(ZString @string)
		{
			return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(@string.ToLower());
		}

		public ZString CallNotes
		{
			get { return Enterprise.ZArchitecture.Core.ORtfTextUtil.RtfToText(OrgSalesCall.OQ_SalesCallNotes).Replace("\r", ""); }
		}

		public ZString FollowUpNotes
		{
			get { return Enterprise.ZArchitecture.Core.ORtfTextUtil.RtfToText(OrgSalesCall.OQ_FollowupNotes).Replace("\r", ""); }
		}

		public ZString CallSummary
		{
			get { return OrgSalesCall.OQ_CallSummary; }
		}

		public ZString DiscussedTradeLanes
		{
			get
			{
				ZString result = "";
				foreach (OrgSales tradeLane in OrgSalesCall.AssociatedTradeLanesPivots.GetTradeLanes())
				{
					tradeLane.ParentOrganisation = OrgSalesCall.Header;
					result += tradeLane.TradeLaneDescription + "\n";
				}

				return result.TrimEnd();
			}
		}

		[DocumentField("Method Code")]
		public ZString MethodCode
		{
			get
			{
				if (!OrgSalesCall.OQ_TypeOfCall.IsEmpty)
				{
					return OrgSalesCall.OQ_TypeOfCall;
				}
				return "";
			}
		}

		[DocumentField("Method of Communication")]
		public ZString Method
		{
			get
			{
				if (!OrgSalesCall.OQ_TypeOfCall.IsEmpty)
				{
					return OrgSalesCall.Lookups.OQ_TypeOfCall_List.GetDescriptionFromCode(OrgSalesCall.OQ_TypeOfCall);
				}
				return "";
			}
		}

		[DocumentField("Purpose Code")]
		public ZString PurposeCode
		{
			get { return OrgSalesCall.OQ_Category; }
		}

		[DocumentField("Purpose of Communication")]
		public ZString Purpose
		{
			get { return OrgSalesCall.OQ_CategoryDescription; }
		}

		[DocumentField("Communication Organization Code where an Organization exists, Fallback is Organization Name")]
		public ZString OrganizationCode
		{
			get { return OrgSalesCall.Header?.OH_Code ?? OrgSalesCall.LinkedInquiry?.O1_CompanyName ?? ZString.Empty; }
		}

		[DocumentField("Organization Name")]
		public ZString OrganizationFullName
		{
			get { return OrgSalesCall.Header?.OH_FullName ?? OrgSalesCall.LinkedInquiry?.O1_CompanyName ?? ZString.Empty; }
		}

		[DocumentField("Primary Contact")]
		public ZString PrimaryContact
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrgSalesCall.Contact != null)
				{
					DocContacts contact = DocContacts.New(OrgSalesCall.Contact, Factory);
					result += ToTitleCase(contact.ContactName);
				}
				return result;
			}
		}

		[DocumentField("Subject")]
		public ZString CommunicationSubject
		{
			get { return OrgSalesCall.OQ_CallSummary; }
		}

		public DocOrganisation ClientOrg => clientOrg ?? (clientOrg = DocOrganisation.New(OrgSalesCall.Header, Factory));
		DocOrganisation clientOrg;

		#endregion
	}
}
