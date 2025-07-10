using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class HyperlinkAlertForm : ZChildForm
	{
		public HyperlinkAlertForm(IHyperlinkAlertBusinessObject messages) : base(messages)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Buttons
		void OKButtonClick(object sender, System.EventArgs e)
		{
			this.Close();
		}

		void ShowDetailClick(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.HyperlinkFormDetailMessageTextBox = new ZTextBox();

			this.HyperlinkFormDetailMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HyperlinkFormDetailMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 114, true);
			this.HyperlinkFormDetailMessageTextBox.Multiline = true;
			this.HyperlinkFormDetailMessageTextBox.Name = "HyperlinkFormDetailMessageTextBox";
			this.HyperlinkFormDetailMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 203, true);
			this.HyperlinkFormDetailMessageTextBox.ScrollBars = ScrollBars.Both;
			this.HyperlinkFormDetailMessageTextBox.TabIndex = 3;
			this.HyperlinkFormDetailMessageTextBox.ReadOnly = true;
			this.BindingSource.SetBindingMember(this.HyperlinkFormDetailMessageTextBox, "LongMessageText");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IHyperlinkAlertBusinessObject)(null)).LongMessageText);

			this.Controls.Add(HyperlinkFormDetailMessageTextBox);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 187 + 203, true);

			this.HyperlinkFormViewDetailLinkLabel.Text = Res.GetString("6D573CCE-CA2C-4D25-BA49-FBAE02F7C4D2", @"Hide Details");
			this.HyperlinkFormViewDetailLinkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(ShowDetailClick);
			this.HyperlinkFormViewDetailLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(HideDetailClick);
		}

		void HideDetailClick(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.Controls.Remove(this.HyperlinkFormDetailMessageTextBox);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 187, true);
			this.HyperlinkFormDetailMessageTextBox = null;

			this.HyperlinkFormViewDetailLinkLabel.Text = Res.GetString("67187073-DCD0-4F74-A928-47BB37952DFF", @"View Details");
			this.HyperlinkFormViewDetailLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(ShowDetailClick);
			this.HyperlinkFormViewDetailLinkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(HideDetailClick);
		}
		#endregion

		readonly System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
