using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Services.Testing
{
	public sealed class ShortCodeUrlHandlerTest : TestCaseWithFactory
	{
		ShortCodeUrlHandler UrlHandlerForTest => ShortCodeUrlHandler.Instance;

		public void TestShortCodeUrlHandlerIsRegistered()
		{
			AssertEquals("Form should be opened for edit", true, unregisteredHandlers.Contains(UrlHandlerForTest));
		}

		[GuiTest]
		public void TestForEmailHyperlink()
		{
			var wi = GetDummyWorkItem();
			var url = GetWorkItemUrl(wi);

			AssertEquals("Form should not be shown initially for the test", false, GetWorkItemForm());

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, GetWorkItemForm());
		}

		[GuiTest]
		public void TestForInternetExplorerHyperlink()
		{
			var wi = GetDummyWorkItem();

			var url = GetWorkItemUrl(wi);
			url = HttpUtility.UrlDecode(url); // IE does this before passing the url to the application!

			AssertEquals("Form should not be shown initially for the test", false, GetWorkItemForm());

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, GetWorkItemForm());
		}

		public void TestWhenTypeNotFound()
		{
			var type = "IfThisIsAValidTypeSomebodyHasMessedUpBigTime";
			var id = "Accordingtoallknownlawsofaviationthereisnowayabeeshouldbeabletofly";
			var url = $"edient:Command={ShortCodeUrlHandler.Command}&Type={type}&Id={id}";

			AssertExceptionThrown<EnterpriseUrlHandlerException>(
				"The ID does not exist in the database",
				$"{Enterprise.Core.Constants.ProductName} doesn't yet support Entities of type {type.ToUpper()}",
				() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
		}

		public void TestWhenRecordNotFound()
		{
			var type = "WI";
			var id = "WIGOBRRRRRRRRRRRRRRRRRR";
			var url = $"edient:Command={ShortCodeUrlHandler.Command}&Type={type}&Id={id}";

			AssertExceptionThrown<EnterpriseUrlHandlerException>(
				"record not found exception",
				$"{Enterprise.Core.Constants.ProductName} was unable to find an Entity with the Id {id.ToUpper()} for type {type.ToUpper()}",
				() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
		}

		public void TestNoOverlappingShortCodes()
		{
			var codes = ShortCodeUrlHandler.SupportedCodes.Keys;

			bool matched = false;
			(string, string) problem = (null, null);
			foreach (var code in codes)
			{
				matched = matched || codes.Any(v =>
				{
					if (code == v)
					{
						return false;
					}

					var match = v.StartsWith(code);
					if (match)
					{
						problem = (v, code);
					}
					return match;
				});
			}

			if (matched)
			{
				Fail($"{problem.Item1} starts with {problem.Item2}");
			}

			Assert("No code should start with another code.", !matched);
		}

		[GuiTest]
		public void TestCanOpenWorkItems()
		{
			var wi = GetDummyWorkItem();
			var url = GetWorkItemUrl(wi);

			AssertEquals("Form should not be shown initially for the test", false, GetWorkItemForm());

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, GetWorkItemForm());
		}

		[GuiTest]
		public void TestCanOpenSupportIncidents()
		{
			var cs = GetDummySupportIncident();
			var url = GetSupportIncidentUrl(cs);

			AssertEquals("Form should not be shown initially for the test", false, GetSupportIncidentForm());

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, GetSupportIncidentForm());
		}

		[GuiTest]
		public void TestCanOpenProjects()
		{
			var pr = getDummyProject();
			var url = getProjectUrl(pr);

			AssertEquals("Form should not be shown initially for the test", false, GetProjectForm());

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, GetProjectForm());
		}

		Project getDummyProject()
		{
			var pr = Factory.NewWithValidTestData<Project>();
			Factory.Save();
			return pr;
		}

		WorkItem GetDummyWorkItem()
		{
			var wi = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();
			return wi;
		}

		SupportIncident GetDummySupportIncident()
		{
			var wi = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			return wi;
		}

		string GetWorkItemUrl(WorkItem wi)
		{
			return GetBaseUrl("WorkItem", wi.WKI_WorkItemNumber);
		}

		string GetSupportIncidentUrl(SupportIncident cs)
		{
			return GetBaseUrl("SupportIncident", cs.IM_IncidentNumber);
		}

		string getProjectUrl(Project pr)
		{
			return GetBaseUrl("Project", pr.WKP_ProjectNumber);
		}

		string GetBaseUrl(string type, string id)
		{
			return $"edient:Command={ShortCodeUrlHandler.Command}&Type={type}&Id={id}";
		}

		bool GetWorkItemForm()
		{
			var form = GetOpenForm;
			return form != null && form is WorkItemForm;
		}

		bool GetSupportIncidentForm()
		{
			var form = GetOpenForm;
			return form != null && form is SupportIncidentForm;
		}

		bool GetProjectForm()
		{
			var form = GetOpenForm;
			return form != null && form is ProjectForm;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		Form GetOpenForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is WorkItemForm || form is SupportIncidentForm || form is ProjectForm)
					{
						return form;
					}
				}
				return null;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var handlers = EnterpriseUrlHandlerService.UrlHandlers;
			foreach (var handler in handlers)
			{
				unregisteredHandlers.Add(handler);
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
			}
			EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
		}

		readonly List<UrlHandler> unregisteredHandlers = new List<UrlHandler>();

		protected override void TearDown()
		{
			base.TearDown();

			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			foreach (var handler in unregisteredHandlers)
			{
				EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			}

			if (GetOpenForm != null)
			{
				GetOpenForm.Dispose();
			}
		}
	}
}
