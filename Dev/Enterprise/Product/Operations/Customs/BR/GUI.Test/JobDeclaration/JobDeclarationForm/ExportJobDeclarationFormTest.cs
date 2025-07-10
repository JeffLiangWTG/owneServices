using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestRoutingVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Routing);

				Assert("Routing should be enabled", plugIn.Enabled);
			}
		}

		public override ZString MessageTypeForFormBashing => Common.BR.BRJobMessageTypeList.Codes.Export;

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();

			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1224);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(790);
			using (var form = new JobDeclarationForm(declaration))
			{
				Assert("BR Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("BR Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		protected override void SetupDeclarationForSpecificFormBashing(BaseJobDeclarationForm form)
		{
			base.SetupDeclarationForSpecificFormBashing(form);
			currentFormBashingDeclaration.FixedJobMessageType = MessageTypeForFormBashing;
		}
	}
}
