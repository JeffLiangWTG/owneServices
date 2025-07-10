using System;
using System.Collections;
using System.Drawing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Registry.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public partial class ColorSelectorFindBox : ZGridFindBox
	{
		public ColorSelectorFindBox()
		{
			InitializeComponent();
			CodeBox.TextChanged += CodeBox_TextChanged;
		}

		void CodeBox_TextChanged(object sender, EventArgs e)
		{
			if (DepotAddressColorSoundValidation.IsValidRGBValue(Code))
			{
				var rgb = Code.Split(',');
				var r = Convert.ToInt32(rgb[0]);
				var g = Convert.ToInt32(rgb[1]);
				var b = Convert.ToInt32(rgb[2]);
				CurrentColor.BackColor = Color.FromArgb(r, g, b);
				CurrentColor.Visible = true;
			}
			else
			{
				CurrentColor.Visible = false;
			}
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return new ColorManager();
		}

		protected override IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}
	}
}
