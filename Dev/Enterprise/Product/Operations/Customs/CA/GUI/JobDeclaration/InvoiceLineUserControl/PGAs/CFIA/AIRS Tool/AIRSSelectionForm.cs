using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class AIRSSelectionForm : ZChildForm
	{
		public AIRSSelectionForm(AIRSWebpageNavigator aIRSWebParser) : base(aIRSWebParser)
		{
			InitializeComponent();
			this.aIRSWebParser = aIRSWebParser;
			BuildWebBrowsers();
#if DEBUG
			TypeDescriptor.AddAttributes(RadioGroupBox, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		readonly AIRSWebpageNavigator aIRSWebParser;

		AIRSWebpageNavigator AIRSSelection
		{
			get { return (AIRSWebpageNavigator)base.DataSource; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.Text = Res.GetString("4128F3E3-6C79-4914-9016-AF67EA4EF62F", "Are you sure to copy following data into CFIA PGA?");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1147:DoNotUseWebControls", Justification = "To be removed")]
		void BuildWebBrowsers()
		{
			var list = this.aIRSWebParser.LPCOList;
			if (list != null)
			{
				foreach (var set in list)
				{
					var brower = new ZWebBrowser();
					brower.DocumentText = set.AL_LPCOAndRegistrationForShow;
					brower.Dock = System.Windows.Forms.DockStyle.Top;
					brower.DocumentCompleted += (s, arg) =>
					{
						webBrowser1_DocumentCompleted(s, set);
					};
					this.MainGroupBox.Controls.Add(brower);
				}
			}
		}
		void webBrowser1_DocumentCompleted(object sender, AIRSLPCOSelection selection)
		{
			var browser = sender as ZWebBrowser;
			var adjustedHeight = browser.Document.Body.ScrollRectangle.Height;
			var radioButton = new ZRadioButton();
			radioButton.Dock = System.Windows.Forms.DockStyle.Top;
			radioButton.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(adjustedHeight);
			browser.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(adjustedHeight);
			radioButton.CheckedChanged += (s, arg) =>
			{
				selection.AL_DataSetSelected = radioButton.Checked;
			};
			this.RadioGroupBox.Controls.Add(radioButton);
			var groupBoxHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.MainGroupBox.Height);
			radioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, groupBoxHeight + adjustedHeight / 2);
			this.RadioGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(adjustedHeight + groupBoxHeight);
			this.MainGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(adjustedHeight + groupBoxHeight);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			var lpcos = this.aIRSWebParser.LPCOList;
			if (lpcos.Any() && !lpcos.Any(x => x.AL_DataSetSelected))
			{
				Globals.Message.ShowError(Res.GetString("399F5CA0-6DC6-44CA-8DDA-CB53FBADA614",
						"Please select at least one LPCO/Registration set."));
				return;
			}

			AIRSSelection.RunPreSaveValidation();
			if (!AIRSSelection.HasErrors)
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}
	}
}
