using System;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public abstract class DocMyAccountWebContractEmailTest : DocumentWrapperTestCase
	{
		public void TestHtmlStyleSheet()
		{
			SystemDataRegistry.Instance.HtmlEmailStyleSheet.SetValue(Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK, "TestStyle.css");
			AssertEquals("HtmlStyleSheet", "TestStyle.css", Wrapper.HtmlStyleSheet);
		}

		[TestDate(2011, 1, 17)]
		public void TestCurrentDate()
		{
			AssertEquals("CurrentDate", ZDateTime.Now.ToShortDateString(), Wrapper.CurrentDate);
		}

		public void TestContactName()
		{
			WebContract.LoggedInContact.OC_ContactName = "milk o'tea";
			AssertEquals("ContactName", "Milk O'Tea", Wrapper.ContactName);
		}

		public void TestContactSalutation()
		{
			WebContract.LoggedInContact.OC_ContactName = "Samuel Wang";
			AssertEquals("Samuel Wang", Wrapper.ContactSalutation);

			WebContract.LoggedInContact.OC_Salutation = "Samuel";
			AssertEquals("Samuel", Wrapper.ContactSalutation);
		}

		public void TestClientName()
		{
			WebContract.LoggedInOrganisation.OH_FullName = "Formag LTD";
			AssertEquals("ClientName", "Formag LTD", Wrapper.ClientName);
		}

		public void TestCurrentCompanyName()
		{
			AssertEquals("CurrentCompanyName", GlbCompany.CurrentCompany.GC_Name, Wrapper.CurrentCompanyName);

			GlbCompany.CurrentCompany.GC_Name = "Zaporojez Limited";
			AssertEquals("CurrentCompanyName", "Zaporojez Limited", Wrapper.CurrentCompanyName);
		}

		public void TestCurrentCompanyOrgProxyName()
		{
			AssertEquals("CurrentCompanyOrgProxyName", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, Wrapper.CurrentCompanyOrgProxyName);

			OrgHeader testOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			testOrgProxy.OH_Code = "YYYY";
			testOrgProxy.OH_FullName = "TEST ORG";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgProxy.PK;
			Factory.Save();
			AssertEquals("CurrentCompanyOrgProxyName", "TEST ORG", Wrapper.CurrentCompanyOrgProxyName);
		}

		public void TestWebContractContentAndVersion()
		{
			NotificationEmailTemplate template = new NotificationEmailTemplate();
			template.EmailSubject = "Version 4.0";
			template.EmailBody = "Sample web contract";
			GetTermsAndConditionsRegistryItem().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);
			AssertEquals("Version 4.0", Wrapper.WebContractVersion);
			AssertEquals("Sample web contract", Wrapper.WebContractContent);
		}

		#region Implementation

		DocMyAccountWebContractEmail Wrapper
		{
			get { return (DocMyAccountWebContractEmail)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = header.Contacts.AddNew();
			MyAccountWebContract webContract = GetMyAccountWebContractForTest(contact);
			return new DocumentWrapper[] { DocMyAccountWebContractEmail.New(webContract, Factory) };
		}

		MyAccountWebContract WebContract
		{
			get { return Wrapper.WrappedObject; }
		}

		#endregion

		protected abstract MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact);
		protected abstract TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem();
	}

	[TestedType(typeof(DocMyAccountWebContractEmail))]
	public class DocMyAccountWebContractEmailOrgLevelTest : DocMyAccountWebContractEmailTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgHeaderWebContract(contact);
		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent;
	}

	[TestedType(typeof(DocMyAccountWebContractEmail))]
	public class DocMyAccountWebContractEmailContactLevelTest : DocMyAccountWebContractEmailTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgContactWebContract(contact);
		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent;
	}
}
