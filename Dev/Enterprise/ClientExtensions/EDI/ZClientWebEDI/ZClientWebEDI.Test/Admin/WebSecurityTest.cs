using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class WebSecurityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageLoad()
		{
			var page = GetPageForTest();
			page.AppInstance.SiteUser.Login(Org.OH_Code, Contact.OC_Email, "1234");
			page.DoPageLoad();
		}

		[ExpectNoExceptions]
		public void TestConcurrencyExceptionOnSaving()
		{
			var page = GetPageForTest();
			page.AppInstance.SiteUser.Login(Org.OH_Code, Contact.OC_Email, "1234");
			page.DoPageLoad();
			var hasConcurrencyException = false;
			page.Factory.Saving += factory =>
			{
				hasConcurrencyException = true;
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Simulated concurrency exception"), ((IBusinessObjectInternals)Contact).Row, ((IDbConnected)factory).Connection), factory);
			};
			var errorConfirmationDiv = page.Controls.OfType<Panel>().Single(x => x.ID == "ErrorConfirmationDiv");
			AssertEquals(WebSecurityCssConstants.WsModalOff, errorConfirmationDiv.CssClass);
			AssertEquals(false, hasConcurrencyException);
			page.SaveButton_Click_Exposed();
			AssertEquals(true, hasConcurrencyException);
			AssertEquals(WebSecurityCssConstants.WsModalOn, errorConfirmationDiv.CssClass);
		}

		OrgHeader Org;
		OrgContact Contact;
		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_Code = "MEHMEH";
			Org.OH_FullName = "MEHMEH";
			Contact = Org.Contacts.AddNew();
			Contact.OC_ContactName = "NewUser";
			Contact.OC_Email = "newuser@cargowise.com";
			Contact.OC_WebAccessEnabled = true;
			Contact.SetHashedPassword("1234");
			Factory.Save();
		}

		WebSecurityForTest GetPageForTest()
		{
			var testPage = new WebSecurityForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		class WebSecurityForTest : WebSecurity
		{
			public void DoPageLoad()
			{
				BulkUpdateSecurityGrid = new ZGrid();
				SecurityGrid = new ZGrid();
				ErrorConfirmationDiv = new Panel()
				{ ID = "ErrorConfirmationDiv" };
				ErrorConfirmationDiv.CssClass = WebSecurityCssConstants.WsModalOff;
				Controls.Add(ErrorConfirmationDiv);
				base.OnLoad(EventArgs.Empty);
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void SaveButton_Click_Exposed() => SaveButton_Click(null, null);
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
