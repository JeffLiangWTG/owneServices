using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class CustomerServiceEmailWrapper : CustomerServiceEmailBaseWrapper
	{
		CustomerServiceEmailWrapper(CustomerServiceEmail objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public static CustomerServiceEmailWrapper New(CustomerServiceEmail objectToWrap, BusinessObjectFactory factory)
		{
			return new CustomerServiceEmailWrapper(objectToWrap, factory);
		}

		[DocumentField("Html Style Sheet")]
		public ZString HtmlStyleSheet
		{
			get { return SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value; }
		}

		[DocumentField("Email Body")]
		public ZString EmailBody
		{
			get { return WrappedObject.Body.NormaliseWhitespaceCharactersForHtml(); }
		}

		ZString Title
		{
			get
			{
				ZString result = "";
				if (WrappedObject.FromDisplayName != SupportIncident.SupportDisplayName)
				{
					GlbStaff[] fromStaff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_FullName, WrappedObject.FromDisplayName));
					if (fromStaff.Length == 1)
					{
						result = fromStaff[0].GS_Title;
					}
				}
				return result;
			}
		}

		[DocumentField("Sign Off Name and Title")]
		public ZString SignOffNameAndTitle
		{
			get
			{
				ZString result = WrappedObject.FromDisplayName;
				if (WrappedObject.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle)
				{
					result = CurrentUser.FullName + "<br />" + CurrentUser.Title;
				}
				else
				{
					if (WrappedObject.UseCurrentUsersNameAndTitle && !CurrentUser.Title.IsEmpty)
					{
						result += "<br />" + CurrentUser.Title;
					}
					else if (!Title.IsEmpty)
					{
						result += "<br />" + Title;
					}
				}
				return result;
			}
		}

		[DocumentField("Footer Hyperlink")]
		public ZString FooterHyperlink => WrappedObject.GetFooterHyperlink();

		[DocumentField("Sign Off Email Address")]
		public ZString SignOffEmailAddress
		{
			get { return WrappedObject.FromEmailAddress; }
		}

		public new CustomerServiceEmail WrappedObject
		{
			get { return (CustomerServiceEmail)base.WrappedObject; }
		}

		protected override OrgContact Contact
		{
			get { return WrappedObject.Contact; }
		}

		protected override ZString ClientName
		{
			get
			{
				var result = Contact?.BranchAddress?.CompanyName ?? ZString.Empty;
				if (result.IsEmpty)
				{
					var incident = WrappedObject?.BusinessObjectSendingEmail as SupportIncident;
					result = incident?.ClientCompany?.LCC_Name ?? ZString.Empty;
				}

				if (result.IsEmpty)
				{
					result = Contact?.Header?.OH_FullNameTruncated ?? ZString.Empty;
				}

				if (result.IsEmpty)
				{
					result = WrappedObject?.Client?.OH_FullNameTruncated ?? ZString.Empty;
				}
				return result;
			}
		}

		internal override void SetContact(OrgContact contact)
		{
			WrappedObject.Contact = contact;
		}

		internal override void SetClient(OrgHeader client)
		{
			WrappedObject.Client = client;
		}
	}
}

