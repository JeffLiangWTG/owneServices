using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZToolBar : KToolBar
	{
		public delegate void ClickHandler();

		public ZToolBar()
		{
			Wrappable = false;
			Appearance = ToolBarAppearance.Flat;
			ImageList = Icons.ImageList;

			UserEventTracker.Instance.AddUserEventToControl(this);
		}

		[Localizable(true)]
		[DefaultValue(ToolBarAppearance.Flat)]
		public new ToolBarAppearance Appearance
		{
			get { return base.Appearance; }
			set { base.Appearance = value; }
		}

		[Localizable(true)]
		[DefaultValue(false)]
		public new bool Wrappable
		{
			get { return base.Wrappable; }
			set { base.Wrappable = value; }
		}

		#region OnButtonDropDown / OnButtonClick

		protected override void OnButtonDropDown(ToolBarButtonClickEventArgs e)
		{
			var button = e.Button as ZToolBarButton;

			if (button != null)
			{
				button.PerformDropDown();
			}
			base.OnButtonDropDown(e);
		}

		protected override void OnButtonClick(ToolBarButtonClickEventArgs e)
		{
			var button = e.Button as ZToolBarButton;
			if (button != null)
			{
				button.PerformClick();
			}

			base.OnButtonClick(e);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			// This is needed to force the .NET ToolBar to unhook events from the ImageList, which is needed
			// because the ImageList is a long-lived object (it is on the Icons singleton).
			ImageList = null;
			base.Dispose(disposing);
		}

		#endregion
	}
}
