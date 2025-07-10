using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[ToolboxItem(false)]
	public partial class ZFilterStripAddButton : KUserControl // this is an architecture control
	{
		public ZFilterStripAddButton()
		{
			InitializeComponent();

			AddButton.Click += new EventHandler(AddButton_Click);
			AddButton.BackgroundImageLayout = ImageLayout.Stretch;
			AddButton.FlatStyle = FlatStyle.Flat;
			AddButton.FlatAppearance.BorderSize = 0;
			AddButton.BackColor = Color.Transparent;
			if (!DesignModeFinder.IsDesigning)
			{
				UpdateButtonImage(IconTypes.AddButtonRest);
				AddButton.MouseEnter += delegate { UpdateButtonImage(IconTypes.AddButtonActive); };
				AddButton.MouseLeave += delegate { UpdateButtonImage(IconTypes.AddButtonRest); };
			}

			MissingResourceStringChecker.ExcludeFromTest(AddButton);
		}

		void UpdateButtonImage(IconTypes enabledIcon)
		{
			AddButton.BackgroundImage = Icons.GetImage(AddButton.Enabled ? enabledIcon : IconTypes.AddButtonDisabled);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			UpdateButtonImage(IconTypes.AddButtonRest);
		}

		void AddButton_Click(object sender, EventArgs e)
		{
			OnClick(e);
		}
	}
}
