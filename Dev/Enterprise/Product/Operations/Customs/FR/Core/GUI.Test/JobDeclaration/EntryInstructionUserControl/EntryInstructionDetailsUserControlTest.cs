using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestAuthorizationUserControlType_DeltaG()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionDetailsUserControl())
			using (var userControlGrid = new EntryInstructionGridUserControl())
			{
				userControl.Controls.Add(userControlGrid);
				form.Controls.Add(userControl);
				form.Show();

				var gridUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
				AssertEquals("EntryInstructionAuthorisationsUserControl.UserControlType", typeof(DeltaGEntryInstructionAuthorisationsUserControl), gridUserControl.UserControlType);
			}
		}

		public void TestAuthorizationUserControlType_UCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionDetailsUserControl())
			using (var userControlGrid = new EntryInstructionGridUserControl())
			{
				userControl.Controls.Add(userControlGrid);
				form.Controls.Add(userControl);
				form.Show();
				var gridUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("AuthorisationsUserControl");
				AssertEquals("EntryInstructionAuthorisationsUserControl.UserControlType", typeof(EU.GUI.EntryInstructionAuthorisationsUserControl), gridUserControl.UserControlType);
			}
		}

		public void TestDetailsUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var detailsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl");
				AssertEquals("DetailsUserControl.UserControlType", typeof(EntryInstructionDetailBasicUserControl), detailsUserControl.UserControlType);
			}
		}

		public void TestGridUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var gridUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionGridUserControl");
				AssertEquals("EntryInstructionGridUserControl.UserControlType", typeof(EntryInstructionGridUserControl), gridUserControl.UserControlType);
			}
		}

		public void TestGuaranteesUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionDetailsUserControl())
			using (var userControlGrid = new EntryInstructionGridUserControl())
			{
				userControl.Controls.Add(userControlGrid);
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				userControlGrid.CurrentEntryInstruction.CEI_Style = "H1";
				userControl.SetTabPagesVisibility();
				var guaranteesTabPage = userControl.FindSingle<ZTabPage>("GuaranteesTabPage");
				AssertEquals("GuaranteesTabPage should be Visible for DeltaIE declaration only.", true, guaranteesTabPage.TabVisible);

				var gridUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("GuaranteesUserControl");
				AssertEquals("EntryInstructionGuaranteesUserControl.UserControlType", typeof(EntryInstructionGuaranteesUserControl), gridUserControl.UserControlType);
			}
		}

		public void TestGuaranteesTabVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";

			using (var form = new ZForm(declaration))
			using (var userControl = new EntryInstructionDetailsUserControl())
			using (var userControlGrid = new EntryInstructionGridUserControl())
			{
				userControl.Controls.Add(userControlGrid);
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
				userControlGrid.CurrentEntryInstruction.CEI_Style = "H1";
				userControl.SetTabPagesVisibility();
				AssertNull("GuaranteesTabPage should be not Visible for DeltaG declaration.", userControl.FindSingleOrDefault<ZTabPage>("GuaranteesTabPage"));
				var detailTabPage = userControl.FindSingle<ZTabPage>("DetailsTabPage");
				AssertEquals("DetailsTabPage should always be Visible.", true, detailTabPage.TabVisible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				userControlGrid.CurrentEntryInstruction.CEI_Style = "H1";
				userControl.SetTabPagesVisibility();
				var guaranteesTabPage = userControl.FindSingle<ZTabPage>("GuaranteesTabPage");
				AssertEquals("GuaranteesTabPage should be Visible for DeltaIE declaration only.", true, guaranteesTabPage.TabVisible);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				userControlGrid.CurrentEntryInstruction.CEI_Style = "H5";
				userControl.SetTabPagesVisibility();
				AssertNull("GuaranteesTabPage should be not Visible for DeltaIE declaration and cei style H5.", userControl.FindSingleOrDefault<ZTabPage>("GuaranteesTabPage"));
			}
		}
	}
}
