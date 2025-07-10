using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
	{
		public void TestCPCDropEdit()
		{
			AssertEquals("Declaration Type", control.FindSingle<ZDropEditWithFixedWidth>("CPCDropEdit").CaptionResourceString.Caption);
		}

		public void TestSplitReferenceZTextBox()
		{
			AssertEquals("Split Reference", control.FindSingle<ZTextBox>("SplitReferenceZTextBox").CaptionResourceString.Caption);
		}

		public void TestPackageCount()
		{
			AssertEquals("[UCC 6/18] Package Count", control.FindSingle<ZTextBox>("PackageCount").CaptionResourceString.Caption);
		}

		public void TestToWarehouseGroupBox()
		{
			AssertEquals("[UCC 2/7] To Warehouse", control.FindSingle<ZGroupBox>("ToWarehouseGroupBox").CaptionResourceString.Caption);
		}

		public void TestFromWarehouseGroupBox()
		{
			AssertEquals("[UCC 2/7] From Warehouse", control.FindSingle<ZGroupBox>("FromWarehouseGroupBox").CaptionResourceString.Caption);
		}

		public void TestToWarehouseAddressControl()
		{
			AssertNotNull(control.FindSingleOrDefault<ZAddressControl>("ToWarehouseAddressControl"));
		}

		public void TestFromWarehouseAddressControl()
		{
			AssertNotNull(control.FindSingleOrDefault<ZAddressControl>("FromWarehouseAddressControl"));
		}

		public void TestNewOwnerAddressControl()
		{
			AssertNotNull(control.FindSingleOrDefault<ZOrganisationControl>("NewOwnerOrganisationControl"));
		}

		public void TestToWarehouseCodeTextBox()
		{
			AssertEquals("To Warehouse Code", control.FindSingle<ZTextBox>("ToWarehouseCodeTextBox").CaptionResourceString.Caption);
		}

		public void TestFromWarehouseCodeTextBox()
		{
			AssertEquals("From Warehouse Code", control.FindSingle<ZTextBox>("FromWarehouseCodeTextBox").CaptionResourceString.Caption);
		}

		public void TestAssessmentDateEdit()
		{
			AssertEquals("Assessment Date", control.FindSingle<ZDateEdit>("AssessmentDateEdit").CaptionResourceString.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			form = new ZForm(declaration);
			control = new EntryInstructionDetailBasicUserControl();
			form.Controls.Add(control);
			form.Show();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
			form?.Dispose();
		}

		EntryInstructionDetailBasicUserControl control;
		ZForm form;
	}
}
