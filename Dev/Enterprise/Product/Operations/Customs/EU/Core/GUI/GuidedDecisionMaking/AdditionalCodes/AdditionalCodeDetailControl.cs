using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.GUI.Internal.ZFindBoxUserControl;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalCodeDetailControl
	{
		public AdditionalCodeDetailControl(GuidedDecisionMakingAdditionalCode additionalCode, AdditionalCodesControl parent = null, bool isInSummary = false) : base()
		{
			this.additionalCode = Argument.NotNull(additionalCode, nameof(additionalCode));
			this.parent = parent;
			this.isInSummary = isInSummary;
			InitializeComponent();
			InitializeExtend();
		}

		readonly bool isInSummary;

		void InitializeExtend()
		{
			this.additionalCodeRadioButton.Name = $"RatioButton_{BoundCodeText}";
			this.additionalCodeCodeFindBox.Name = $"CodeFindBox_{BoundCodeText}";
			this.additionalCodeCodeFindBox.Click += CodeFindBoxAndInternalControls_Clicked;
			this.additionalCodeCodeFindBox.CodeBox.Click += CodeFindBoxAndInternalControls_Clicked;
			this.additionalCodeCodeFindBox.DescriptionBox.Click += CodeFindBoxAndInternalControls_Clicked;
			this.additionalCodeRadioButton.CheckedChanged += RadioButton_CheckedChanged;
		}

		public void Tick(bool isTick = true)
		{
			this.additionalCodeRadioButton.Checked = isTick;
		}

		void RadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.additionalCodeRadioButton.Checked && parent != null && !isInSummary)
			{
				parent.TickExclusiveAdditionalCodeDetailControl(this.BoundCodeText);
			}
		}

		void CodeFindBoxAndInternalControls_Clicked(object sender, EventArgs e)
		{
			if (!isInSummary)
			{
				Control parent = null;
				if (sender is ZCodeFindBox codeFindBox)
				{
					parent = codeFindBox.Parent;
				}
				else if (sender is ZCodeBox codeBox)
				{
					parent = codeBox.Parent.Parent;
				}
				else if (sender is ZTextBox descriptionBox)
				{
					parent = descriptionBox.Parent.Parent;
				}

				if (parent is ZRadioButton radioButton)
				{
					radioButton.Checked = true;
				}
			}
		}

		public ZString BoundCodeText => additionalCode?.AdditionalCode ?? ZString.Empty;

		readonly GuidedDecisionMakingAdditionalCode additionalCode;

		readonly AdditionalCodesControl parent;
	}
}
