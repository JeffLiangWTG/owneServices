using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUContainerUserControlTest : Customs.GUI.Testing.BaseCustomsCusContainersWithTrackingUserControlTest
	{
		public void TestAddedColumns()
		{
			using (var userControl = new AUContainerUserControl())
			{
				AssertNotNull("There is a column bound to SealStartNumber", userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealStartNumber));
				AssertNotNull("There is a column bound to SealEndNumber", userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealEndNumber));
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			declaration.JE_ApplicationCode = "CMR";
			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				var userControl = testForm.ContainerUserControlForTesting;
				Assert("SealStartNumber is visible", userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealStartNumber).IsVisible);
				Assert("SealEndNumber is visible", userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealEndNumber).IsVisible);
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			using (var testForm = new AUCustomsDeclarationFormForTest(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				var userControl = testForm.ContainerUserControlForTesting;
				Assert("SealStartNumber is not visible", !userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealStartNumber).IsVisible);
				Assert("SealEndNumber is not visible", !userControl.CusContainersBoundGrid.InnerGrid.GetColumnStyle(CusContainer.Schema.SealEndNumber).IsVisible);
			}
		}

		sealed class AUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public AUCustomsDeclarationFormForTest(JobDeclaration jobDeclaration)
				: base(jobDeclaration)
			{
			}

			public AUCustomsDeclarationFormForTest()
				: this(null)
			{
			}

			internal AUContainerUserControl ContainerUserControlForTesting => (AUContainerUserControl)CustomsBrokerageUserControl.ContainerUserControl;
		}
	}
}
