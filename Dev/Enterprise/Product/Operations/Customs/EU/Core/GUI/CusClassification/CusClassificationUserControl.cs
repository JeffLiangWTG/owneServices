using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using TariffFormatter = Enterprise.Customs.EU.Business.TariffFormatter;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CusClassificationUserControl : BaseClassificationUserControl
	{
		public CusClassificationUserControl()
		{
			InitializeComponent();
			InitializeTariffFindBox();
		}

		public new CusClassification CurrentDataItem => (CusClassification)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				SetControlVisibility(BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(CurrentDataItem).NumberOfCodes > 0, supplementLabel2, cC_EcAdditionalSupplementsTextBox, additionalSupplementaryCodesEditButton);
			}
		}

		void SetControlVisibility(bool condition, params Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Visible = condition;
			}
		}

		void InitializeTariffFindBox()
		{
			var tariffFindBox = new Universal.GUI.TariffFindBox();
			tariffFindBox.GetCountryCode = GetCountryCode;
			tariffFindBox.GetTariffType = GetTariffType;
			tariffFindBox.Name = "TariffFindBox";
			tariffFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CusClassificationUserControl|b1044a4e-ea80-43fc-8cbd-87a49c5e1e61", "Commodity (Tariff)");
			tariffFindBox.PreBoundMaxLength = 15;
			tariffFindBox.ShowDescriptionBox = false;
			tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 18, isInStandardDpi: true);
			tariffFindBox.TabIndex = 1;
			BindingSource.SetBindingMember(tariffFindBox, "CC_FormattedTariffNum");
			BaseClassificationGroupBox.Controls.Add(tariffFindBox);
		}

		string GetCountryCode()
		{
			var countryCode = ZString.Empty;
			if (!this.IsDesignMode())
			{
				var classification = CurrentDataItem;
				countryCode = classification == null || classification.IsDeleted ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : classification.CC_RN_NKCountryCode;
			}
			return countryCode;
		}

		ZString GetTariffType()
		{
			var classification = CurrentDataItem;
			return TariffFormatter.GetTariffType(classification != null && !classification.IsDeleted && classification.IsExport);
		}

		void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				AdditionalSupplementaryCodesForm.ShowDialog(CurrentDataItem, ParentForm);
			}
		}
	}
}
