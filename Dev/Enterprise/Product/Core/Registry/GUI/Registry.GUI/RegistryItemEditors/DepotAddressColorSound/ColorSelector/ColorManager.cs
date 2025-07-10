using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public class ColorManager : IFindBoxPopup
	{
		public ColorManager()
		{
		}

		void ShowColorDialog(ColorDialog dialog)
		{
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var color = dialog.Color;
				findBox.Code = string.Format("{1}{0}{2}{0}{3}", ",", color.R.ToString("D3"), color.G.ToString("D3"), color.B.ToString("D3"));
			}
		}

		ColorDialog DialogWithDefault()
		{
			var dialog = new ColorDialog();
			var rgb = RGBIntFromString(findBox.Code);
			if (rgb != null)
			{
				dialog.Color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
			}
			return dialog;
		}

		static int[] RGBIntFromString(ZString rgbValue)
		{
			if (DepotAddressColorSoundValidation.IsValidRGBValue(rgbValue))
			{
				var rgb = rgbValue.Split(',');
				return new int[] { Convert.ToInt32(rgb[0]), Convert.ToInt32(rgb[1]), Convert.ToInt32(rgb[2]) };
			}
			return null;
		}

		IFindBox findBox;

		public static Color ColorFromString(ZString rgbString)
		{
			var rgb = RGBIntFromString(rgbString);
			if (rgb != null)
			{
				return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
			}
			return Color.Empty;
		}

		public void Dispose()
		{
		}

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, ZArchitecture.GUI.Internal.EmbeddedModulePopup popup)
		{
			throw new NotImplementedException();
		}

		public void SelectRowByPK(ZGuid pK)
		{
			throw new NotImplementedException();
		}

		public void ShowModal(IFindBox findBox, System.Windows.Forms.Form parentForm)
		{
			this.findBox = findBox;
			ShowColorDialog(DialogWithDefault());
		}

		public event EventHandler Closed
		{
			add
			{
			}
			remove
			{
			}
		}
	}
}
