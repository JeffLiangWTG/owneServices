using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
#if !WINZOR
using Enterprise.RemoteDesktopServices.Server;
#endif

namespace Enterprise.Core.Forms
{
	public partial class EnterpriseInformationCWNextForm : KForm, ICaptionRenderingSupport
	{
		public EnterpriseInformationCWNextForm()
		{
			InitializeComponent();

			Text = Res.GetString("EnterpriseInformationCWNextForm.Text", "About {0}", "CargoWise Next");
			CopyrightLabel.Text = CopyrightLabel.Text.Replace("CR", new string((char)169, 1));

			this.MoveFormByMouseDrag(true);
		}

		public new static void Show()
		{
			var originalUserInteractive = Globals.IsUserInteractive;
			try
			{
				// Even things that are not normally interactive, e.g. the batch processor, must be interactive if the user has clicked a menu.
				Globals.IsUserInteractive = true;
				using (var form = new EnterpriseInformationCWNextForm())
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
			finally
			{
				Globals.IsUserInteractive = originalUserInteractive;
			}
		}

		#region Information Display

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "About Screen Text")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				//ReleaseLabel.Text = EnterpriseInfo.Release;
				VersionNumberLabel.Text = EnterpriseInfo.VersionNumber;
				VersionDateLabel.Text = EnterpriseInfo.VersionDate;
				DBVersionNumber.Text = EnterpriseInfo.DBVersionNumber;
				TerminalServerModeLabel.Text = EnterpriseInfo.TerminalServerMode ? "Yes" : "No";
#if !WINZOR
				if (EnterpriseInfo.TerminalServerMode)
				{
					TerminalServerModeLabel.Text += ", " + (InitializationMessageHandler.RegisteredRemoteMessageTypes.Length > 0 ? "With" : "Without") + (ObjectFactory.Get<TerminalService>().IsCitrixICA ? " Citrix Services" : " RD Services");
				}
#endif

				DBServerLabel.Text = EnterpriseInfo.DBServerName;
				DBNameLabel.Text = EnterpriseInfo.DBDatabaseName;
				DbSecurityDataLabel.Text = EnterpriseInfo.DatabaseSecurityMode;
				SystemTypeDataLabel.Text = EnterpriseInfo.SystemLicenceType;
				CompanyNameLabel.Text = EnterpriseInfo.CurrentCompanyName;
				CountryLabel.Text = EnterpriseInfo.CurrentCompanyCountry;
				FrameworkVersionLabel.Text = EnterpriseInfo.FrameworkVersion;
				SysDocVersionLabel.Text = EnterpriseInfo.SystemDocumentsVersion;
				ClientDocVersionLabel.Text = EnterpriseInfo.ClientDocumentsName + " - Version " + EnterpriseInfo.ClientDocumentsVersion;
				LicenceCodeLabel.Text = EnterpriseInfo.LicenceCode;
				CopyrightLabel.Text = CopyrightLabel.Text.Replace("<CompanyName>", BrandingFactory.Instance.CompanyName);
				SQLServerVersionLabel.Text = EnterpriseInfo.SqlServerVersion;
				AzureApplicationClientIdLabel.Text = EnterpriseInfo.AzureApplicationClientId;
				clientIPAddressHeadingLabel.Visible = Globals.IsWinzor;
				clientIPAddressValueLabel.Visible = Globals.IsWinzor;
				clientIPAddressValueLabel.Text = Globals.WinzorClientIpAddress?.ToString();
				this.AzureApplicationClientIdLabel.Font = new Font("SansSerif", 6.75F, FontStyle.Regular);

				var complianceVersionNumber = EnterpriseInfo.ComplianceVersionNumber;
				if (!string.IsNullOrEmpty(complianceVersionNumber))
				{
					ShowComplianceVersionNumber(complianceVersionNumber);
				}
			}
		}

		void ShowComplianceVersionNumber(string complianceVersionNumber)
		{
			var complianceVersionLabel = new ZLabel();
			var complianceVersionValue = new ZLabel();
			complianceVersionValue.Anchor = AnchorStyles.Top;
			complianceVersionValue.AutoSize = true;
			complianceVersionValue.Font = new Font("Arial", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			complianceVersionValue.ForeColor = Color.Black;
			complianceVersionValue.Location = ControlDpiScalingHelper.NewScaledPoint(117, 156, true);
			complianceVersionValue.Name = "ComplianceVersionValue";
			complianceVersionValue.Size = ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			complianceVersionValue.TabIndex = 30;
			complianceVersionValue.Text = complianceVersionNumber;

			complianceVersionLabel.Anchor = AnchorStyles.Top;
			complianceVersionLabel.CaptionResourceString = Res.GetData("e1023f63-e3d6-4803-98a2-80061909e368", "Compliance Version:");
			complianceVersionLabel.Font = new Font("Arial", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			complianceVersionLabel.ForeColor = Color.FromArgb(0, 168, 225);
			complianceVersionLabel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 156, true);
			complianceVersionLabel.Name = "ComplianceVersionLabel";
			complianceVersionLabel.Size = ControlDpiScalingHelper.NewScaledSize(116, 14, true);
			complianceVersionLabel.TabIndex = 31;
			complianceVersionLabel.TextAlign = ContentAlignment.MiddleRight;

			zGroupBox1.Controls.Add(complianceVersionLabel);
			zGroupBox1.Controls.Add(complianceVersionValue);
			zGroupBox1.Size = ControlDpiScalingHelper.NewScaledSize(387, 179, true);
			ClientSize = ControlDpiScalingHelper.NewScaledSize(412, 605, true);

			ControlDpiScalingHelper.SetTop(CopyrightLabel, CopyrightLabel.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
			ControlDpiScalingHelper.SetTop(zGroupBox2, zGroupBox2.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(16), false);
			ControlDpiScalingHelper.SetTop(zGroupBox3, zGroupBox3.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(16), false);
		}

#if !WINZOR

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			using (var pen = new Pen(Color.FromArgb(196, 199, 200)))
			{
				e.Graphics.DrawRectangle(pen, ControlDpiScalingHelper.NewScaledRectangle(
					this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1), this.ClientRectangle.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false));
			}
		}

#endif

		void CopyToClipboardButton_Click(object sender, EventArgs e)
		{
			if (!SafeClipboard.SetText(EnterpriseInfo.ToString()))
			{
				Globals.Message.Show(SafeClipboard.ClipboardNotAccessibleWarning);
			}
		}

		EnterpriseInformationRetriever EnterpriseInfo
		{
			get { return fEnterpriseInfo ?? (fEnterpriseInfo = new EnterpriseInformationRetriever()); }
		}

		EnterpriseInformationRetriever fEnterpriseInfo;

		#endregion

		#region ICaptionRenderingSupport Members

		[Category(ZGUIConstants.DesignerCategory)]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool? CaptionRenderingEnabled
		{
			get { return captionRenderingEnabled; }
			set
			{
				if (captionRenderingEnabled != value)
				{
					captionRenderingEnabled = value;
					OnCaptionRenderingEnabledChanged(EventArgs.Empty);
				}
			}
		}

		bool? captionRenderingEnabled;

		public event EventHandler CaptionRenderingEnabledChanged;

		void OnCaptionRenderingEnabledChanged(EventArgs e)
		{
			if (CaptionRenderingEnabledChanged != null)
			{
				CaptionRenderingEnabledChanged(this, e);
			}
		}
		#endregion
	}
}
