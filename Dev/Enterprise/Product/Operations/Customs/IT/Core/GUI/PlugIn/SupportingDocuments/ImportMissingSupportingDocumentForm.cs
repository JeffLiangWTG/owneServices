using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ImportMissingSupportingDocumentForm : ZChildForm
{
	public ImportMissingSupportingDocumentForm()
		: base()
	{
	}

	public ImportMissingSupportingDocumentForm(MissingSupportingDocumentParent missedSupportingDocumentParent)
		: base(missedSupportingDocumentParent)
	{
	}

	public override string FormHeading
	{
		get { return Res.GetString("085924C0-847B-4F1B-B6D7-23F1E6BDDE9E", "Set Supporting Documents"); }
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		InitializeMissingSupportingDocumentsLayout();
	}

	#region Implementation

	void InitializeMissingSupportingDocumentsLayout()
	{
		var missingSupportingDocumentGrouped = MissingSupportingDocumentParent.MissingSupportingDocumentGrouped;
		if (missingSupportingDocumentGrouped.Any())
		{
			AddMissingSupportingDocumentGroupBoxes(missingSupportingDocumentGrouped.Reverse());
		}
		else
		{
			SetNoMissingSupportingDocumentsToSelectLabel();
		}
	}

	void SetNoMissingSupportingDocumentsToSelectLabel()
	{
		titleLabel.CaptionResourceString = Res.GetData("D65EBC83-C41B-4193-AB99-715930419263", "No missing supporting documents to select");
		importButton.Enabled = false;
		setIntoAllMergableInvoiceLinesCheckBox.Visible = false;
	}

	void AddMissingSupportingDocumentGroupBoxes(IEnumerable<GroupedMissingSupportingDocument> missingSupportingDocumentGrouped)
	{
		foreach (var missingSupportingDocumentGroup in missingSupportingDocumentGrouped)
		{
			var missingSupportingDocumentGroupBox = GetNewMissingSupportingDocumentGroupBox(missingSupportingDocumentGroup);
			mainPanel.Controls.Add(missingSupportingDocumentGroupBox);
			missingSupportingDocumentGroupBox.AllowOutsideOfParent();

			var conditionTypeCommentLabel = GetNewConditionTypeCommentLabel(missingSupportingDocumentGroup);
			missingSupportingDocumentGroupBox.Controls.Add(conditionTypeCommentLabel);
			missingSupportingDocumentGroupBox.Controls.SetChildIndex(conditionTypeCommentLabel, 0);
			conditionTypeCommentLabel.AllowOutsideOfParent();

			var missingSupportingDocumentCheckedListBox = GetNewMissingSupportingDocumentCheckedListBox(missingSupportingDocumentGroup);
			missingSupportingDocumentGroupBox.Controls.Add(missingSupportingDocumentCheckedListBox);
			missingSupportingDocumentGroupBox.Controls.SetChildIndex(missingSupportingDocumentCheckedListBox, 0);
			missingSupportingDocumentCheckedListBox.AllowOutsideOfParent();

			AdjustMissingSupportingDocumentGroupBoxHeight(missingSupportingDocumentGroupBox, missingSupportingDocumentCheckedListBox, missingSupportingDocumentGroup);
		}
	}

	ZLabel GetNewConditionTypeCommentLabel(GroupedMissingSupportingDocument missingSupportingDocumentGroup)
	{
		var conditionTypeCommentLabel = new ZLabel();
		conditionTypeCommentLabel.Dock = DockStyle.Top;
		LabelCaptionRenderProvider.SetLabelCaptionVisible(conditionTypeCommentLabel, false);
		conditionTypeCommentLabel.Text = missingSupportingDocumentGroup.ConditionTypeComment;
		conditionTypeCommentLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3);
		conditionTypeCommentLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3);
		conditionTypeCommentLabel.AutoSize = true;

		return conditionTypeCommentLabel;
	}

	ZCheckedListBox GetNewMissingSupportingDocumentCheckedListBox(GroupedMissingSupportingDocument missingSupportingDocumentGroup)
	{
		var missingSupportingDocumentCheckedListBox = new ZCheckedListBox();
		var missingSupportingDocumentsBoolDescriptionPairListDataMember = nameof(missingSupportingDocumentGroup.MissingSupportingDocumentsBoolDescriptionPairList);

		var missingSupportingDocumentsCheckedListBindingSource = new ZBindingSource();
		missingSupportingDocumentsCheckedListBindingSource.SetBindingMember(missingSupportingDocumentCheckedListBox, missingSupportingDocumentsBoolDescriptionPairListDataMember);
		missingSupportingDocumentsCheckedListBindingSource.SetDataBinding(missingSupportingDocumentGroup, "");

		missingSupportingDocumentCheckedListBox.CheckOnClick = true;
		missingSupportingDocumentCheckedListBox.BindTo = missingSupportingDocumentsBoolDescriptionPairListDataMember;
		missingSupportingDocumentCheckedListBox.Dock = DockStyle.Fill;
		missingSupportingDocumentCheckedListBox.ScrollAlwaysVisible = true;
		missingSupportingDocumentCheckedListBox.ItemHeight = 35;
		return missingSupportingDocumentCheckedListBox;
	}

	ZGroupBox GetNewMissingSupportingDocumentGroupBox(GroupedMissingSupportingDocument missingSupportingDocumentGroup)
	{
		var missingSupportingDocumentGroupBox = new ZGroupBox();
		LabelCaptionRenderProvider.SetLabelCaptionVisible(missingSupportingDocumentGroupBox, false);
		missingSupportingDocumentGroupBox.Text = missingSupportingDocumentGroup.ConditionTypeDescription;
		missingSupportingDocumentGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 4, 4, 0);
		missingSupportingDocumentGroupBox.Dock = DockStyle.Top;
		return missingSupportingDocumentGroupBox;
	}

	void AdjustMissingSupportingDocumentGroupBoxHeight(ZGroupBox missingSupportingDocumentGroupBox, ZCheckedListBox missingSupportingDocumentCheckedListBox, GroupedMissingSupportingDocument missingSupportingDocumentGroup)
	{
		var itemsCount = missingSupportingDocumentGroup.MissingSupportingDocumentsBoolDescriptionPairList.Count;

		missingSupportingDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(missingSupportingDocumentGroupBox.Width,
			(itemsCount + MinItemsCountWhichHeightAdjustmentIsRequired + EmptyRowsInZCheckedListBox) * (CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(missingSupportingDocumentCheckedListBox.ItemHeight) + 1));
	}

	void importButton_Click(object sender, System.EventArgs e)
	{
		DialogResult = DialogResult.OK;
		Close();
	}

	MissingSupportingDocumentParent MissingSupportingDocumentParent => DataSource as MissingSupportingDocumentParent;

	void cancelButton_Click(object sender, System.EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}

	#endregion

	const int MinItemsCountWhichHeightAdjustmentIsRequired = 3;
	const int EmptyRowsInZCheckedListBox = 1;
}
