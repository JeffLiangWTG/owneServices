using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class DocMyAccountWebContractEmail : DocBaseWrapper
	{
		DocMyAccountWebContractEmail(MyAccountWebContract objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		public static DocMyAccountWebContractEmail New(MyAccountWebContract objectToWrap, BusinessObjectFactory factory)
		{
			return new DocMyAccountWebContractEmail(objectToWrap, factory);
		}

		public new MyAccountWebContract WrappedObject
		{
			get { return (MyAccountWebContract)base.WrappedObject; }
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

		[DocumentField("Client Name")]
		public ZString ClientName
		{
			get { return WrappedObject != null && WrappedObject.LoggedInOrganisation != null ? WrappedObject.LoggedInOrganisation.OH_FullNameTruncated : ZString.Empty; }
		}

		[DocumentField("Contact's Full Name")]
		public ZString ContactName
		{
			get { return WrappedObject != null && WrappedObject.LoggedInContact != null ? WrappedObject.LoggedInContact.OC_ContactName.CapitaliseFirstLettersOfWords() : ""; }
		}

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get
			{
				ZString result = ZString.Empty;
				if (WrappedObject != null)
				{
					OrgContact contact = WrappedObject.LoggedInContact;
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

		[DocumentField("Your Company's Name")]
		public ZString CurrentCompanyName
		{
			get { return CurrentCompany.Name; }
		}

		[DocumentField("Your Company's OrgProxy Name")]
		public ZString CurrentCompanyOrgProxyName
		{
			get { return CurrentCompany.Organisation.Name; }
		}

		[DocumentField("Web Contract (Terms And Conditions) Content")]
		public ZString WebContractContent
		{
			get { return WrappedObject.WebContractContent; }
		}

		[DocumentField("Web Contract (Terms And Conditions) Version")]
		public ZString WebContractVersion
		{
			get { return WrappedObject.WebContractVersion; }
		}
	}
}

