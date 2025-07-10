using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class DocumentConditionDetailControl : ZUserControl
	{
		public DocumentConditionDetailControl(GuidedDecisionMakingConditionDetail conditionDetail, bool isInSummary = false)
		{
			this.conditionDetail = conditionDetail;
			this.isInSummary = isInSummary;
			InitializeComponent();
			InitializeComponentExtend();
		}

		void InitializeComponentExtend()
		{
			IsTickedCheckBox.Name = $"{ConditionDetailCode}_CheckBox";
			ConditionCodeFindBox.Name = $"{ConditionDetailCode}_CodeFindBox";
			ConditionCodeFindBox.CodeBox.Name = $"{ConditionDetailCode}_CodeBox";
			ConditionCodeFindBox.DescriptionBox.Name = $"{ConditionDetailCode}_DescriptionBox";
			ReferenceTextBox.Name = $"{ConditionDetailCode}_ReferenceTextBox";
			DateOfIssueDateEdit.Name = $"{ConditionDetailCode}_DateOfIssueDateEdit";
			if (!isInSummary)
			{
				ConditionCodeFindBox.Click += CodeFindBoxAndInternalControls_Clicked;
				ConditionCodeFindBox.CodeBox.Click += CodeFindBoxAndInternalControls_Clicked;
				ConditionCodeFindBox.DescriptionBox.Click += CodeFindBoxAndInternalControls_Clicked;
			}

			if (conditionDetail.Type.Equals(Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber))
			{
				ReferenceTextBox.Visible = false;
				DateOfIssueDateEdit.Visible = false;
				ConditionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 17, true);
			}
			else
			{
				ReferenceTextBox.Visible = true;
				DateOfIssueDateEdit.Visible = true;
				ConditionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 17, true);
			}

			ReferenceTextBox.MaxLengthChanged += ReferenceTextBox_MaxLengthChanged;
		}

		void ReferenceTextBox_MaxLengthChanged(object sender, EventArgs e)
		{
			ReferenceTextBox.MaxLength = Business.Declaration.MultiLineAddInfos.SupportingDocument.Schema.ReferenceNumberMaxLength;
		}

		readonly bool isInSummary;

		void CodeFindBoxAndInternalControls_Clicked(object sender, EventArgs e)
		{
			var parent = this.IsTickedCheckBox;
			if (parent is ZCheckBox checkBox)
			{
				checkBox.Checked = !checkBox.Checked;
			}
		}

		ZString ConditionDetailCode => conditionDetail?.Code ?? ZString.Empty;
		readonly GuidedDecisionMakingConditionDetail conditionDetail;
	}
}
