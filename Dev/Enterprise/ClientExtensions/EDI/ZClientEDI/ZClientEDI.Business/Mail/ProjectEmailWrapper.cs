using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class ProjectEmailWrapper : CustomerServiceEmailBaseWrapper
	{
		ProjectEmailWrapper(EDIProject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public static ProjectEmailWrapper New(EDIProject objectToWrap, BusinessObjectFactory factory)
		{
			return new ProjectEmailWrapper(objectToWrap, factory);
		}

		protected override OrgContact Contact
		{
			get { return WrappedObject.Contact; }
		}

		protected override ZString ClientName
		{
			get
			{
				var org = WrappedObject?.ClientOrganisation ?? Contact?.Header;
				return org?.OH_FullNameTruncated ?? ZString.Empty;
			}
		}

		internal override void SetContact(OrgContact contact)
		{
			WrappedObject.WKP_OC_Contact = contact != null ? contact.PK : ZGuid.Empty;
		}

		internal override void SetClient(OrgHeader client)
		{
			if (client != null)
			{
				WrappedObject.WKP_OA_ClientAddress = client.MainAddress.PK;
				WrappedObject.ClientOrganisationPK = client.PK;
			}
			else
			{
				WrappedObject.WKP_OA_ClientAddress = ZGuid.Empty;
				WrappedObject.ClientOrganisationPK = ZGuid.Empty;
			}
		}

		[DocumentField("Current Staff Name And Title")]
		public ZString CurrentStaffNameAndTitle
		{
			get
			{
				ZString result = CurrentUser.FullName;
				if (!CurrentUser.Title.IsEmpty)
				{
					result += "<br />" + CurrentUser.Title;
				}
				return result;
			}
		}

		[DocumentField("Default Email Address")]
		public ZString DefaultEmailAddress
		{
			get { return WrappedObject.OverridingDefaultFromEmailAddress; }
		}

		public new EDIProject WrappedObject
		{
			get { return (EDIProject)base.WrappedObject; }
		}
	}
}
