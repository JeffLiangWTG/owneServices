using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class VATDetailControl : ZUserControl
	{
		public VATDetailControl() : this(null)
		{
		}

		public VATDetailControl(GuidedDecisionMakingVAT gDMVAT) : this(gDMVAT, null, false)
		{
		}

		public VATDetailControl(GuidedDecisionMakingVAT gDMVAT, VATControl parent = null, bool isInSummary = false)
		{
			InitializeComponent();
			this.vat = gDMVAT;
			this.parent = parent;
			this.isInSummary = isInSummary;
			this.VATDetailLabel.Text = gDMVAT.DisplayText;
			InitializeComponentExtend();
		}

		void InitializeComponentExtend()
		{
			if (!isInSummary)
			{
				this.VATDetailRadioButton.CheckedChanged += RadioButton_CheckedChanged;
				this.VATDetailLabel.Click += Control_Clicked;
				this.VATDetailLabel.MouseDown += (s, e) => { this.OnMouseDown(e); };
				this.Click += Control_Clicked;
				this.VATDetailRadioButton.MouseEnter += Control_Enter;
				this.VATDetailRadioButton.MouseLeave += Control_Leave;
				this.VATDetailLabel.MouseEnter += Control_Enter;
				this.VATDetailLabel.MouseLeave += Control_Leave;
				this.MouseEnter += Control_Enter;
				this.MouseLeave += Control_Leave;
			}
		}

		internal void Tick(bool isTick = true)
		{
			this.VATDetailRadioButton.Checked = isTick;
		}

		void RadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.VATDetailRadioButton.Checked && parent != null)
			{
				parent.TickExclusiveAdditionalCodeDetailControl(this.BoundCodeText);
			}
		}

		void Control_Clicked(object sender, EventArgs e)
		{
			this.VATDetailRadioButton.Checked = true;
		}

		void Control_Enter(object sender, EventArgs e)
		{
			this.BackColor = SystemColors.ControlLight;
			this.VATDetailRadioButton.BackColor = SystemColors.ControlLight;
		}

		void Control_Leave(object sender, EventArgs e)
		{
			this.BackColor = OriginalBackColor;
			this.VATDetailRadioButton.BackColor = OriginalBackColor;
		}

		internal ZString BoundCodeText => (vat is GuidedDecisionMakingVAT) ? new ZString($"{vat.Category}-{vat.AdditionalCode}-{vat.VATCode}") : ZString.Empty;

		readonly GuidedDecisionMakingVAT vat;

		readonly VATControl parent;

		readonly bool isInSummary;

		Color OriginalBackColor => Color.FromArgb(244, 246, 247);
	}
}
