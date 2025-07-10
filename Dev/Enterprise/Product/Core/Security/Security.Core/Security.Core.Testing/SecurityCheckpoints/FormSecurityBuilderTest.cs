using System.Linq;
using Enterprise.Core.Environment;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class FormSecurityBuilderTest : TestCase
	{
		public void TestConstuctsTreeAccordingToConvention_View()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			var formSecurityBuilder = new FormSecurityBuilder(root, security, true, false)
				.Form("Screen")
					.Tab("Tab1")
					.Tab("Tab2");

			var formCheckPoint = security.FindCheckPoint("Screen");
			AssertFormCheckPoint(formCheckPoint, root, formSecurityBuilder);

			var viewCheckPoint = formCheckPoint.ChildCheckPoints.First();
			AssertViewCheckPoints(viewCheckPoint);
		}

		public void TestConstuctsTreeAccordingToConvention_Edit()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			var formSecurityBuilder = new FormSecurityBuilder(root, security, false, true)
				.Form("Screen")
					.Tab("Tab1")
					.Tab("Tab2");

			var formCheckPoint = security.FindCheckPoint("Screen");
			AssertFormCheckPoint(formCheckPoint, root, formSecurityBuilder);

			var editCheckPoint = formCheckPoint.ChildCheckPoints.First();
			AssertEditCheckPoints(editCheckPoint);
		}

		public void TestConstuctsTreeAccordingToConvention_ViewAndEdit()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			var formSecurityBuilder = new FormSecurityBuilder(root, security, true, true)
				.Form("Screen")
					.Tab("Tab1")
					.Tab("Tab2");

			var formCheckPoint = security.FindCheckPoint("Screen");
			AssertFormCheckPoint(formCheckPoint, root, formSecurityBuilder);

			var viewCheckPoint = formCheckPoint.ChildCheckPoints.First();
			AssertViewCheckPoints(viewCheckPoint);

			var editCheckPoint = formCheckPoint.ChildCheckPoints.Skip(1).First();
			AssertEditCheckPoints(editCheckPoint);
		}

		void AssertFormCheckPoint(ISecurityCheckpoint formCheckPoint, ISecurityCheckpoint root, FormSecurityBuilder formSecurityBuilder)
		{
			Assert(formCheckPoint != null);
			Assert(formCheckPoint.Parent == root);
			Assert(formCheckPoint.ChildCheckPoints.Count() == formSecurityBuilder.TabSecurityPoints.Count);
		}

		void AssertViewCheckPoints(ISecurityCheckpoint viewCheckPoint)
		{
			Assert(viewCheckPoint.ChildCheckPoints.Count() == 2);

			var firstTabCheckPoint = viewCheckPoint.ChildCheckPoints.First();

			Assert(firstTabCheckPoint != null);
			Assert(firstTabCheckPoint.Code == viewCheckPoint.Code + "." + "Tab1");

			var secondTabCheckPoint = viewCheckPoint.ChildCheckPoints.Skip(1).First();

			Assert(secondTabCheckPoint != null);
			Assert(secondTabCheckPoint.Code == viewCheckPoint.Code + "." + "Tab2");
		}

		void AssertEditCheckPoints(ISecurityCheckpoint editCheckPoint)
		{
			Assert(editCheckPoint.ChildCheckPoints.Count() == 2);

			var firstTabCheckPoint = editCheckPoint.ChildCheckPoints.First();

			Assert(firstTabCheckPoint != null);
			Assert(firstTabCheckPoint.Code == editCheckPoint.Code + "." + "Tab1");

			var secondTabCheckPoint = editCheckPoint.ChildCheckPoints.Skip(1).First();

			Assert(secondTabCheckPoint != null);
			Assert(secondTabCheckPoint.Code == editCheckPoint.Code + "." + "Tab2");
		}

		public void TestSetDisplayTextDifferentThanCode()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			new FormSecurityBuilder(root, security, true, true)
				.Form("Screen")
					.Tab("Tab", (NoResString)"Tab Display Text");

			var tabViewCheckPoint = security.FindCheckPoint("Screen.View.Tab");
			Assert(tabViewCheckPoint.DisplayText == "Tab Display Text");

			var tabEditCheckPoint = security.FindCheckPoint("Screen.Edit.Tab");
			Assert(tabEditCheckPoint.DisplayText == "Tab Display Text");
		}

		public void TestTrimsTabPageSuffixForDisplayText()
		{
			var security = CreateSecurityInstance();
			var root = CreateRootCheckPoint(security);

			new FormSecurityBuilder(root, security, true, true)
				.Form("Screen")
					.Tab("TestTabPage");

			var tabViewCheckPoint = security.FindCheckPoint("Screen.View.TestTabPage");
			Assert(tabViewCheckPoint.DisplayText == "Test");

			var tabEditCheckPoint = security.FindCheckPoint("Screen.Edit.TestTabPage");
			Assert(tabEditCheckPoint.DisplayText == "Test");
		}

		static SecurityCheckpoint CreateRootCheckPoint(IZSecurity security)
		{
			return new SecurityCheckpoint("Root", (NoResString)"Root", null, security);
		}

		static IZSecurity CreateSecurityInstance()
		{
			return new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}
	}
}
