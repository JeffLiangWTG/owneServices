using System.Collections.Generic;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyModuleButtonGridWithCustomButton : ZDummyModuleButtonGrid
	{
		const string CustomButtonName = "MCLAREN";

		protected override IList<ToolStripItem> CreateButtons()
		{
			var customButton = new ZToolStripButton();
			customButton.Name = CustomButtonName;
			customButton.CaptionResourceString = new ResourceStringData("McLaren", "McLaren");
			customButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23);

			return new[]
			{
				new ZToolStripButton { Name = Buttons.New },
				new ZToolStripButton { Name = Buttons.Edit },
				customButton,
				new ZToolStripButton { Name = Buttons.Attach },
				new ZToolStripButton { Name = Buttons.Detach }
			};
		}

		public ZToolStripButton CustomButton
		{
			get { return GetButton(CustomButtonName); }
		}

		public new IModuleDecisionProvider GetNewModuleDecisionProvider(IFindBox findBox)
			=> base.GetNewModuleDecisionProvider(findBox);
	}
}
