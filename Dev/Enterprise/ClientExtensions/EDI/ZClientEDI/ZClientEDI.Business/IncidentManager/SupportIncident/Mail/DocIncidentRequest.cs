using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentWrappers;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DocIncidentRequest : DocBaseWrapper
	{
		DocIncidentRequest(IncidentRequest objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
			Parent = objectToWrap;
		}

		public static DocIncidentRequest New(IncidentRequest objectToWrap, BusinessObjectFactory factory) => new DocIncidentRequest(objectToWrap, factory);

		EDIOrgContact Contact => Parent.ReportedBy as EDIOrgContact;
		readonly IncidentRequest Parent;

		[DocumentField("Html Style Sheet")]
		public ZString HtmlStyleSheet => SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value;

		[DocumentField("Current Date")]
		public ZString CurrentDate => ZDateTime.Now.ToShortDateString();

		[DocumentField("The Contact's full name")]
		public ZString ContactName => Contact?.OC_ContactName.CapitaliseFirstLettersOfWords() ?? "";

		[DocumentField("The Contact's email")]
		public ZString ContactEmail => Contact?.OC_Email ?? "";

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get
			{
				var result = ZString.Empty;

				if (Contact != null)
				{
					result = Contact.OC_Salutation.IsEmpty ? Contact.OC_ContactName : Contact.OC_Salutation;
					result = result.CapitaliseFirstLettersOfWords();
				}

				return result;
			}
		}

		[DocumentField("The Contact's org. code")]
		public ZString OrgCode => Contact?.OrgCode ?? ZString.Empty;

		[DocumentField("The Contact's company name")]
		public ZString CompanyName => Contact?.CompanyNameForBindingOnly ?? ZString.Empty;

		[DocumentField("The Contact's workplace location")]
		public ZString WorkplaceLocation => Contact?.BranchForBindingOnly ?? ZString.Empty;

		[DocumentField("Your company's name")]
		public ZString CurrentCompanyName => CurrentCompany.Name;

		[DocumentField("Your company's OrgProxy Name")]
		public ZString CurrentCompanyOrgProxyName => CurrentCompany.Organisation.Name;

		[DocumentField("CurrentCompanyWebSite")]
		public ZString CurrentCompanyWebSite => CurrentCompany.WebAddress;

		[DocumentField("Incident Number")]
		public ZString IncidentNumber => Parent?.INC_IncidentNumber ?? ZString.Empty;

		[DocumentField("Client Reference Number")]
		public ZString ClientReferenceNumber => Parent?.INC_ClientReference ?? ZString.Empty;

		[DocumentField("Incident Summary")]
		public ZString Summary => Parent?.INC_Summary ?? ZString.Empty;

		[DocumentField("Incident Detailed Description")]
		public ZString DetailedDescription => Parent?.INC_Details ?? ZString.Empty;

		[DocumentField("Incident Criticality")]
		public ZString Criticality => Parent?.INC_Criticality ?? ZString.Empty;

		[DocumentField("Incident Link URL")]
		public ZString IncidentLinkURL
		{
			get
			{
				var url = "";
				if (Parent != null)
				{
					var pkMacro = EDIDataRegistry.GlowEditERequestPageUri_PkMacro;
					var rootUrl = EDIDataRegistry.Instance.GlowPortalRootUrl.Value.TrimEnd('/');
					var portalPageUri = EDIDataRegistry.Instance.GlowEditERequestPageUri.Value;
					url = rootUrl + "/" + portalPageUri.TrimStart('/').Replace(pkMacro, Parent.PK.ToString());
				}
				return url;
			}
		}
	}
}
