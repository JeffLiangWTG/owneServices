using System;
#if NETFRAMEWORK
using System.Web.Security.AntiXss;
#else
using System.Text.Encodings.Web;
#endif
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public abstract class DocEDIOrgHeaderWebContractTest : DocumentWrapperTestCase
	{
		public void TestClientName()
		{
			WebContract.LoggedInOrganisation.OH_FullName = "Formag LTD";
			AssertEquals("Formag LTD", Wrapper.ClientName);
		}

		public void TestClientName_AntiXssHtmlEncode()
		{
			WebContract.LoggedInOrganisation.OH_FullName = "<svg/onload=alert(document.domain)>";
#if NETFRAMEWORK
			AssertEquals(AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(WebContract.LoggedInContact.OC_ContactName, false), false), Wrapper.ContactName);
#else
			AssertEquals(HtmlEncoder.Default.Encode(HtmlEncoder.Default.Encode(WebContract.LoggedInContact.OC_ContactName)), Wrapper.ContactName);
#endif
		}

		public void TestOrganisationMainAddress()
		{
			OrgAddress address = WebContract.LoggedInOrganisation.Addresses.AddNew(OrgAddressType.Office, true);
			address.OA_OH = WebContract.LoggedInOrganisation.PK;
			address.OA_Address1 = "Unit 3a";
			address.OA_Address2 = "72 O'Riordan Street";
			address.OA_City = "Alexandria";
			address.OA_State = "NSW";
			address.OA_PostCode = "2015";
			address.OA_IsActive = true;

			AssertEquals("UNIT 3A 72 O'RIORDAN STREET ALEXANDRIA NSW 2015", Wrapper.ClientMainAddressInSingleLine);
			AssertEquals("UNIT 3A<br />72 O'RIORDAN STREET<br />ALEXANDRIA NSW 2015", Wrapper.ClientMainAddressInHTMLWithoutCompanyName);
		}

		public void TestContactName()
		{
			WebContract.LoggedInContact.OC_ContactName = "Samuel";
			AssertEquals("Samuel", Wrapper.ContactName);
		}

		public void TestContactName_AntiXssHtmlEncode()
		{
			WebContract.LoggedInContact.OC_ContactName = "<svg/onload=alert(document.domain)>";
#if NETFRAMEWORK
			AssertEquals(AntiXssEncoder.HtmlEncode(AntiXssEncoder.HtmlEncode(WebContract.LoggedInContact.OC_ContactName, false), false), Wrapper.ContactName);
#else
			AssertEquals(HtmlEncoder.Default.Encode(HtmlEncoder.Default.Encode(WebContract.LoggedInContact.OC_ContactName)), Wrapper.ContactName);
#endif
		}

		[TestDate(2011, 2, 18, 10, 28, 25)]
		public void TestDates()
		{
			AssertEquals(new ZDateTime(2011, 2, 18).ToShortDateString(), Wrapper.ShortDate);
			AssertEquals(new ZDateTime(2011, 2, 18).ToDateTime().ToLongDateString(), Wrapper.LongDate);
			AssertEquals("18 February 2011", Wrapper.LongDateNoDay);
		}

		public void TestCurrentCompanyName()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, Wrapper.CurrentCompanyName);
		}

		public void TestWebContractVersion()
		{
			NotificationEmailTemplate contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailSubject = "Version 1.0";
			contractTemplate.EmailBody = "Sample web contract v1";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);
			AssertEquals("Version 1.0", Wrapper.WebContractVersion);
		}

		#region Implementation

		DocMyAccountWebContract Wrapper
		{
			get { return (DocMyAccountWebContract)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = header.Contacts.AddNew();
			MyAccountWebContract webContract = GetMyAccountWebContractForTest(contact);
			return new DocumentWrapper[] { DocMyAccountWebContract.New(webContract, Factory) };
		}

		MyAccountWebContract WebContract
		{
			get { return Wrapper.WrappedObject; }
		}

		#endregion

		protected abstract MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact);
		protected abstract TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem();
	}

	[TestedType(typeof(DocMyAccountWebContract))]
	public class DocEDIOrgHeaderWebContractOrgLevelTest : DocEDIOrgHeaderWebContractTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgHeaderWebContract(contact);
		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent;
	}

	[TestedType(typeof(DocMyAccountWebContract))]
	public class DocEDIOrgHeaderWebContractContactLevelTest : DocEDIOrgHeaderWebContractTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgContactWebContract(contact);
		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent;
	}
}
