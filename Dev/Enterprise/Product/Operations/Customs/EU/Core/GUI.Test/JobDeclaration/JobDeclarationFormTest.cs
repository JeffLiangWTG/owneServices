using System.Drawing;
using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public abstract class JobDeclarationFormTest<TBusinessObject> : BaseJobDeclarationFormAbstractTest<TBusinessObject>
		where TBusinessObject : JobDeclaration
	{
		public void TestHasDocumentVisualizer()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertNotNull("DocumentVaisualizer", form.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer));
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(900);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("EU Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("EU Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		public void TestExitSummaryPlugIn()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertNotNull(ControllerIDs.Customs.EU.ExitSummaryController.Name, form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.ExitSummaryController));
			}
		}

		protected override TBusinessObject GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();

			declaration.SupportingDocuments.AddNew();

			var invoice = (JobComInvoiceHeader)declaration.Invoices.First();
			invoice.SupportingDocuments.AddNew();

			var invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.First();
			invoiceLine.SupportingDocuments.AddNew();

			return declaration;
		}

		public void TestSpecificCircumstanceIndicatorImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				var specificCircumstanceDropEdit = (ZDropEdit)(form.Controls.Find("SpecificCircumstanceDropEdit", true).Single());

				form.Show();
				AssertEquals("SpecificCircumstanceDropEdit is invisible for import.", false, specificCircumstanceDropEdit.Visible);

				declaration.JE_MessageType = "EXP";
				AssertEquals("SpecificCircumstanceDropEdit is visible for export.", true, specificCircumstanceDropEdit.Visible);
			}
		}

		public void TestInitializeBottomPanel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			using (var form = new JobDeclarationForm(declaration))
			{
				var bottomNewPanel = form.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel;

				AssertNotNull("The ZPanel should be initialized.", bottomNewPanel);
				AssertEquals("The ZPanel should have the correct location.", new Point(311, 1), bottomNewPanel.Location);
				AssertEquals("The ZPanel should have the correct size.", new Size(700, 30), bottomNewPanel.Size);
				AssertEquals("The ZPanel should not be visible initially.", false, bottomNewPanel.Visible);
			}
		}
	}
}
