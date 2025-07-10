using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class HeaderDetailsTemplateUserControlTest : TestCaseWithFactory
	{
		public void TestBindingWithControls()
		{
			var declaration = Factory.New<CusReconDeclaration>();

			using (var form = new ZForm(declaration))
			using (var panel = new DynamicLayoutPanel())
			{
				var bag = HeaderDetailsControlBag.Instance;
				var layout = new PanelLayout();
				RegisterControlBag(layout, bag);

				form.Controls.Add(panel);
				panel.UpdateLayout(new LayoutsForTesting(layout));
				form.Show();
				UserIdleWorker.Flush();

				System.Windows.Forms.Application.DoEvents();

				CombineAssertions("TestBindingWithControls", () =>
				{
					var isDeclarantImporterCheckBox = panel.FindSingle<ZCheckBox>(bag.IsDeclarantImporterCheckBox.ControlName);
					AssertEquals("IsDeclarantImporterCheckBox", nameof(declaration.IsDeclarantImporter), isDeclarantImporterCheckBox.BindTo);

					var registrationNumberTextBox = panel.FindSingle<ZTextBox>(bag.RegistrationNumberTextBox.ControlName);
					AssertEquals("RegistrationNumberTextBox BindTo", nameof(declaration.RegistrationNumber), registrationNumberTextBox.BindTo);
					AssertEquals("RegistrationNumberTextBox CharacterCasing", CharacterCasing.Normal, registrationNumberTextBox.CharacterCasing);

					var isFinalizedCheckBox = panel.FindSingle<ZCheckBox>(bag.IsFinalizedCheckBox.ControlName);
					AssertEquals("IsFinalizedCheckBox", nameof(declaration.IsFinalized), isFinalizedCheckBox.BindTo);

					var branchGuidFindBox = panel.FindSingle<ZGuidFindBox>(bag.BranchGuidFindBox.ControlName);
					AssertEquals("branchGuidFindBox", nameof(declaration.CRD_GB_Branch), branchGuidFindBox.BindTo);

					var unlinkedDeclarationsNumberLabel = panel.FindSingle<ZLabel>(bag.UnlinkedDeclarationsNumberLabel.ControlName);
					AssertEquals("UnlinkedDeclarationsNumberMessage", nameof(declaration.UnlinkedDeclarationsNumberMessage), unlinkedDeclarationsNumberLabel.BindTo);
				});
			}
		}

		void RegisterControlBag(PanelLayout layout, HeaderDetailsControlBag bag)
		{
			layout.RegisterControlBag(bag);
			layout.Include(bag.IsDeclarantImporterCheckBox);
			layout.Include(bag.RegistrationNumberTextBox);
			layout.Include(bag.IsFinalizedCheckBox);
			layout.Include(bag.BranchGuidFindBox);
			layout.Include(bag.UnlinkedDeclarationsNumberLabel);
		}
	}
}
