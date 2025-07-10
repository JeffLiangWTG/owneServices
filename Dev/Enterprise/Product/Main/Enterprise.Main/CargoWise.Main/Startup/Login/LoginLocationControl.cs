using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#if WINZOR
using CargoWise.Application;
using Enterprise.Integration;
using WTG.OpenIDConnect.Login;
#endif

using Res = CargoWise.Main.Res;

namespace Enterprise.Startup.Login
{
	public partial class LoginLocationControl : ZUserControl
	{
		public LoginLocationControl()
		{
			InitializeComponent();

			SetupLayout();
			SetDataBinding(loginLocationBusinessObject = new LoginLocationBusinessObjectForMainForm(new BusinessObjectFactory()), null);
			branchDropEdit.CodeBox.TextChanged += CodeBox_TextChanged;
			departmentDropEdit.CodeBox.TextChanged += CodeBox_TextChanged;
		}

		void SetupLayout()
		{
			BackColor = SystemDataRegistry.Instance.ColorTheme.MainFormBackgroundColor;
			PictureBox.Image = new Bitmap(BrandingFactory.Instance.CompanyLogoButton);
			SetupLoginButton();
		}

		void SetupLoginButton()
		{
			var backButton = new ZToolStripButton
			{
				Name = "BackButton",
				Image = Icons.GetImage(IconTypes.BackButtonActive),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				Text = Res.GetString("c96c0fdc-733e-4197-9f0d-3f3653bd02e3", "Back")
			};

			backButton.Click += backButton_Click;
			toolStrip.Items.Add(backButton);

			var loginButton = new ZToolStripButton
			{
				Name = "LoginButton",
				Image = Icons.GetImage(IconTypes.Login),
				ImageScaling = ToolStripItemImageScaling.SizeToFit,
				Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0),
				Text = Res.GetString("8616728c-d4a1-45ae-8ea0-3c57559a479a", "OK")
			};

			loginButton.Click += loginButton_Click;
			toolStrip.Items.Add(loginButton);
		}

		void loginButton_Click(object sender, EventArgs e)
		{
			ZFormUtilities.EnsureSelectedControlValueCommitted(this.ParentForm);
			//if you hit enter on Department dropdown you can go straight to a ZToolStrip without committing previous value - so brute force a fix
			var performControlValidationMethod = typeof(Control).GetMethod("PerformControlValidation", BindingFlags.NonPublic | BindingFlags.Instance);
			performControlValidationMethod.Invoke(companyDropEdit, new object[] { true });
			performControlValidationMethod.Invoke(branchDropEdit, new object[] { true });
			performControlValidationMethod.Invoke(departmentDropEdit, new object[] { true });
			loginLocationBusinessObject.Validation.ValidateAll();
			if (loginLocationBusinessObject.HasErrors)
			{
				errorLabel.Text = new ZNotificationCollector(loginLocationBusinessObject, false, true, ZNotificationCollector.PropertyDescriptionType.None).ToMessageListString();
			}
			else
			{
				var loginInfo = LoginDirector.Instance.LoginLocationInteractive(loginLocationBusinessObject.BranchCode, loginLocationBusinessObject.DepartmentCode);

				if (loginInfo.IsOK)
				{
					MainForm.InitializeAfterLogin();
				}
				else
				{
					errorLabel.Text = loginInfo.FailureMessage;
				}
			}
		}

		void backButton_Click(object sender, EventArgs e)
		{
			var mainForm = ParentForm as MainForm;
			if (mainForm != null)
			{
#if WINZOR
				if (ObjectFactory.Get<IOIDCConfig>().IsOIDCEnabled)
				{
					mainForm.ReLaunchWinzorAppServer(OIDCLoginRequestMessage.LoginPrompt.Login);
					return;
				}
#endif
				mainForm.UnloadLoginLocationControl();
				mainForm.ShowLoginUserControl();
			}
		}

		void CodeBox_TextChanged(object sender, EventArgs e)
		{
			ClearErrorMessage();
		}

		void ClearErrorMessage()
		{
			if (!string.IsNullOrEmpty(errorLabel.Text))
			{
				errorLabel.Text = string.Empty;
			}
		}

		internal readonly LoginLocationBusinessObjectForMainForm loginLocationBusinessObject;
	}
}
