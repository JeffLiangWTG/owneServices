using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ShowModuleUrlHandlerTest : TestCaseWithFactory
	{
		#region Create

		public void TestCreate()
		{
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=ShowModule&ModuleID=RefUNLOCO&Hash=%2bOsfxaSdCN7SYnCAVz4POnbGo8yuT9Ivi",
				UrlHandler.Create(ModuleIDs.RefUNLOCO));
		}

		public void TestCreateString()
		{
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=ShowModule&ModuleID=RefUNLOCO&Hash=%2bOsfxaSdCN7SYnCAVz4POnbGo8yuT9Ivi",
				UrlHandler.Create("RefUNLOCO"));
		}

		#endregion

		#region UrlHandler Overrides

		public void TestCanHandle()
		{
			AssertEquals("No Command", false, UrlHandler.CanHandle(QueryString));

			QueryString["Command"] = "InvalidCommand";
			AssertEquals("Command=InvalidCommand", false, UrlHandler.CanHandle(QueryString));

			QueryString["Command"] = "ShowModule";
			AssertEquals("Command=ShowModule", true, UrlHandler.CanHandle(QueryString));
		}

		#endregion

		#region Show Module

		public void TestShowModuleUrlHandler_InvalidModule()
		{
			var invalidModuleIdentifier = new ModuleIdentifier(Style.Normal, (NoResString)"Wrong");

			var url = ShowModuleUrlHandler.Instance.Create(invalidModuleIdentifier);
			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Exception should be thrown");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals($"Module type is unknown to the current version of {Enterprise.Core.Constants.ProductName}.", ex.Message);
			}
		}

		public void TestShowModuleUrlHandler_Success()
		{
			var url = ShowModuleUrlHandler.Instance.Create(ModuleIDs.RefAirline);
			AssertNull("Precondition", OpenModuleForm);
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertNotNull("Form shown", OpenModuleForm);
			OpenModuleForm.Dispose();
		}

		public void TestShowModuleUrlHandler_InvokeOnFormInitializedAction()
		{
			var formInitializedActionInvoked = false;
			var previousAction = ShowModuleUrlHandler.Instance.OnFormInitialized;
			ShowModuleUrlHandler.Instance.OnFormInitialized = form =>
			{
				formInitializedActionInvoked = true;
			};

			Assert(!formInitializedActionInvoked);
			var url = ShowModuleUrlHandler.Instance.Create(ModuleIDs.RefAirline);
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			Assert(formInitializedActionInvoked);

			ShowModuleUrlHandler.Instance.OnFormInitialized = previousAction;
			OpenModuleForm.Dispose();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		Form OpenModuleForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is ZFilterModule.EmbeddedModulePopupWithNoButtonPanelAndNoModality)
					{
						return form;
					}
				}
				return null;
			}
		}

		#endregion

		#region Implementation

		ShowModuleUrlHandler UrlHandler
		{
			get { return ShowModuleUrlHandler.Instance; }
		}

		readonly QueryString QueryString = new QueryString();

		protected override void SetUp()
		{
			base.SetUp();
			EnterpriseUrlHandlerService.UnregisterUrlHandler(ShowModuleUrlHandler.Instance);
			EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandler);
		}

		protected override void TearDown()
		{
			base.TearDown();
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandler);
			EnterpriseUrlHandlerService.RegisterUrlHandler(ShowModuleUrlHandler.Instance);
		}

		#endregion
	}
}
