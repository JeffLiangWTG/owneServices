using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class SaveLayoutForm : ZChildForm
	{
		#region Construction

#if DEBUG
		public SaveLayoutForm() // for designing
		{
			InitializeComponent();
		}
#endif

		public SaveLayoutForm(SaveLayoutBizO bizO, bool saveColumnShouldBeVisible, bool saveAsUserDefinedFilterShouldBeVisible)
			: base(bizO)
		{
			InitializeComponent();

			var imageListCache = ZFilterControlImages.FilterImageList;

			if (imageListCache != null)
			{
				IsUserDefinedFilterCheckBox.Image = imageListCache.Images["FilterStrip"];
				SaveColumnsCheckBox.Image = imageListCache.Images["ColumnLayouts"];
			}

			PublishLayoutCheckBox.FlatStyle = FlatStyle.Standard;
			SaveColumnsCheckBox.FlatStyle = FlatStyle.Standard;

			SaveColumnsCheckBox.Visible = saveColumnShouldBeVisible;
			IsUserDefinedFilterCheckBox.Visible = saveAsUserDefinedFilterShouldBeVisible;

			IsOkToSave = false;

			UpdateIsLanguageEditingEnabled();

			IsUserDefinedFilterCheckBox.AllowOverlap(SaveColumnsCheckBox);
			PublishLayoutCheckBox.AllowOverlap(PublishAcrossAllCompanies);
			SaveGridColourCheckBox.AllowOverlap(SaveColumnsCheckBox);
		}

		#endregion

		#region Save

		public bool IsOkToSave;

		void SaveFilterButton_Click(object sender, EventArgs e)
		{
			SaveFilter();
		}

		void SaveFilter()
		{
			var saveAndClose = false;
			BizO.RunPreSaveValidation();

			if (BizO.HasErrors)
			{
				ZString msg = Res.GetString("27223fc1-26d6-46e7-8b10-7cb94c889ac5", "There are errors that need correcting before saving your filter layout.");

				Globals.Message.Show(msg, "Errors...", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
			else
			{
				var helper = new ExistingSavedLayoutMessageHelper(BizO);

				if (helper.LayoutExists)
				{
					var messageText = helper.ExistingLayoutExistsMessage;
					var caption = Res.GetString("2f1bcc1e-1ca4-46e9-88ca-aa45bbaf1148", "Filter Exists");

					if (helper.AllowUserToSaveWithWarning)
					{
						var dialogResult = Globals.Message.Show(messageText, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

						if (dialogResult == DialogResult.Yes)
						{
							saveAndClose = true;

							if (helper.IsExistingLayoutUserDefinedFilterStrip)
							{
								helper.ExistingLayout.DeleteWithUserData();
							}
						}
					}
					else
					{
						Globals.Message.ShowError(messageText, caption);
					}
				}
				else
				{
					saveAndClose = true;
				}
			}

			if (saveAndClose)
			{
				IsOkToSave = true;
				Close();
			}
		}

		#endregion

		#region Cancel

		void CancelSaveFilterButton_Click(object sender, EventArgs e)
		{
			CancelFilter();
		}

		void CancelFilter()
		{
			BizO.LayoutName = "";
			IsOkToSave = false;
			Close();
		}

		#endregion

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		SaveLayoutBizO BizO
		{
			get { return (SaveLayoutBizO)BusinessEntity; }
		}

		void PublishLayoutCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateIsLanguageEditingEnabled();
		}

		void UpdateIsLanguageEditingEnabled()
		{
			FilterNameTextBox.IsLanguageEditingEnabled = EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed && PublishLayoutCheckBox.Checked;
		}
	}
}
