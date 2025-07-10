using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class NationalAdditionalCodesForm : ZChildForm
	{
		public NationalAdditionalCodesForm(INationalAdditionalCodeSupporter supporter)
			: base(supporter.NationalAdditionalCodes)
		{
			this.supporter = supporter;
			nationalAdditionalCodes = supporter.NationalAdditionalCodes;

			SetNationalAdditionalCodeColumnStyleInfo();
		}

		public readonly NationalAdditionalCodeCollection nationalAdditionalCodes;
		public readonly INationalAdditionalCodeSupporter supporter;

		public static void ShowDialog(INationalAdditionalCodeSupporter supporter)
		{
			ZFormModaliser.ShowDialogAndDispose(new NationalAdditionalCodesForm(supporter));
		}

		void SetNationalAdditionalCodeColumnStyleInfo()
		{
			var nationalAdditionalCode = new ZDropEditColumnStyleInfo();
			nationalAdditionalCode.CaptionResourceString = Res.GetData("D05FD1DA-6C82-47CB-8CA9-64E19091A51D", "National Additional Code");
			nationalAdditionalCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			nationalAdditionalCode.ColumnName = NationalAdditionalCode.Schema.CY_Code;
			nationalAdditionalCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);

			nationalAdditionalCodesGrid.ColumnStyles.Add(nationalAdditionalCode);
		}

		#region Form Caption

		public override string FormVerb => "";

		#endregion

		#region Loading

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = nationalAdditionalCodes.AsString;
		}

		string fOldItems;

		#endregion

		#region Closing

		protected override void OnClosing(CancelEventArgs e)
		{
			closeButton.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				nationalAdditionalCodes.AsString = fOldItems;
			}
			else
			{
				this.BusinessEntity.RunPreSaveValidation();
				foreach (NationalAdditionalCode code in nationalAdditionalCodes)
				{
					if (code.NotificationsIncludingChildren.GetErrors().Count() > 0)
					{
						Globals.Message.ShowError(Res.GetString("09B7BB64-8C43-4B94-BFDC-3A1B5B3B3E5C", "The form has errors. Please fix them before continuing."));
						e.Cancel = true;
						break;
					}
				}
			}

			base.OnClosing(e);
		}

		#endregion

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
