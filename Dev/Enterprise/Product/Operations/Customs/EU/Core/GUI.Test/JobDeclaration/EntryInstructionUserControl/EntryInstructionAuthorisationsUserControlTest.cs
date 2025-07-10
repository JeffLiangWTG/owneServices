using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionAuthorisationsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_AuthorisationsGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var authorisationsGrid = control.FindSingleOrDefault<ZGrid>("AuthorisationsGrid");

					AssertEquals("Columns", 6, authorisationsGrid.ColumnStyles.Count);
					AssertEquals("AGC_Code", 40, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Code).Width);
					AssertEquals("CustomsCode", 100, authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode)).Width);
					AssertEquals("AGC_Number", 130, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Number).Width);
					AssertEquals("AGC_OH_Owner", 80, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_OH_Owner).Width);
					AssertEquals("AGC_CPH_Authorization", 130, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization).Width);
					AssertEquals("RelatedAuthorisationHeaderIgoringReferenceNumber.CPH_Number width", 130, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").Width);
					AssertEquals("RelatedAuthorisationHeaderIgoringReferenceNumber.CPH_Number readonly", expected: true, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").IsReadOnly);
					AssertEquals("RelatedAuthorisationHeaderIgoringReferenceNumber.CPH_Number hidden", expected: false, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").IsVisible);
				});
			}
		}

		public void TestAuthorisationsGrid_ColumnsCaption()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
					var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
					var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
					var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
					entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
					var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
					var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

					CombineAssertions("Export", () =>
					{
						declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
						AssertEquals("AGC_Code - Caption", "Type", authorisationsGrid.GetColumnCaption(AutoCusAuthorizationUsage.Schema.AGC_Code));
						AssertEquals("AGC_Number - Caption", "Reference", authorisationsGrid.GetColumnCaption(AutoCusAuthorizationUsage.Schema.AGC_Number));
						AssertEquals("Related Authorisation - Caption", "Related Authorization", authorisationsGrid.GetColumnCaption("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber"));
					});

					CombineAssertions("Non Export", () =>
					{
						declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
						AssertEquals("AGC_Code - Caption", "Code", authorisationsGrid.GetColumnCaption(AutoCusAuthorizationUsage.Schema.AGC_Code));
						AssertEquals("AGC_Number - Caption", "Number", authorisationsGrid.GetColumnCaption(AutoCusAuthorizationUsage.Schema.AGC_Number));
						AssertEquals("Related Authorisation - Caption", "Related Authorization", authorisationsGrid.GetColumnCaption("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber"));
					});
				}
			}
		}

		public void TestAuthorisationsAGC_CPH_AuthorizationVisibility_IsNotEnableAdHoc()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertEquals("Prerequisite: ", false, cusAuthorizationUsage.EnableAdHoc);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
				var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
				var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
				entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
				var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
				var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

				AssertEquals("AGC_CPH_Authorization - should not be visible as EnableAdHoc is false", false, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization).IsVisible);
			}
		}

		public void TestAuthorisationsAGC_CPH_AuthorizationVisibility_IsEnableAdHoc()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				AssertEquals("Prerequisite: ", true, cusAuthorizationUsage.EnableAdHoc);
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
					var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
					var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
					var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
					entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
					var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
					var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

					AssertEquals("AGC_CPH_Authorization - should be visible as EnableAdHoc is true", true, authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization).IsVisible);
				}
			}
		}

		public void TestAuthorisationsGrid_CustomsCodeColumn()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;
			cusAuthorisationHeaderProviderMock.Protected().Setup<bool>("ShowCustomsCodeCore").Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);
			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
					CombineAssertions("Prerequisite", () =>
					{
						AssertEquals("ShowCustomsCode", true, Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ShowCustomsCode);
						AssertEquals("IsUCC6", true, declaration.IsUCC6);
					});

					using (var form = new JobDeclarationForm(declaration))
					{
						form.Show();
						form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
						var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
						var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
						var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
						entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
						var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
						var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

						AssertEquals("CustomsCode Column should be available when ShowCustomsCode is true", true, !authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode)).IsUnavailable);
					}
				}
			}
		}

		public void TestAuthorisationsCPH_NumberVisibility_ShowRelatedAuthorisationWithoutReference_Default()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertEquals("Prerequisite: ", false, cusAuthorizationUsage.ShowRelatedAuthorisationWithoutReference);
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
				var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
				var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
				entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
				var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
				var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

				AssertEquals("By default CPH_Number column should not be visible as ShowRelatedAuthorisationWithoutReference is false", expected: false, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").IsVisible);
			}
		}

		public void TestAuthorisationsCPH_NumberVisibility_ShowRelatedAuthorisationWithoutReference_Enabled()
		{
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;
			cusAuthorisationHeaderProviderMock.Protected().Setup<bool>("ShowRelatedAuthorisationWithoutReferenceCore").Returns(true);
			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				AssertEquals("Prerequisite: ", true, cusAuthorizationUsage.ShowRelatedAuthorisationWithoutReference);
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.EntryInstructionDetailsTabPage;
					var control = (EntryInstructionDetailsUserControl)form.CustomsBrokerageUserControl.CustomsEntryInstructionUserControl;
					var splitContainer = control.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
					var entryInstructionTabControl = splitContainer.Panel2.FindSingle<ZTabControl>("EntryInstructionTabControl");
					entryInstructionTabControl.SelectedTab = control.AuthorisationsTabPage;
					var authorisationsUserControl = control.AuthorisationsTabPage.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
					var authorisationsGrid = authorisationsUserControl.FindSingle<ZGrid>("AuthorisationsGrid");

					AssertEquals("With ShowRelatedAuthorisationWithoutReference set to true the CPH_Number column should be visible", expected: true, authorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").IsVisible);
				}
			}
		}
	}
}
