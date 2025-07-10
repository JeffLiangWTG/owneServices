using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class DummyPage : ZPage
	{
		#region Initialisation

		public DummyPage()
			: base()
		{
			InitialisePage();
		}

		void InitialisePage()
		{
			LiteralControl ctrl1 = new LiteralControl();
			ctrl1.Text = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"" >
<HTML>
	<HEAD>
		<title>Login</title>
		<meta name=""GENERATOR"" Content=""Microsoft Visual Studio .NET 7.1"">
		<meta name=""CODE_LANGUAGE"" Content=""C#"">
		<meta name=""vs_defaultClientScript"" content=""JavaScript"">
		<meta name=""vs_targetSchema"" content=""http://schemas.microsoft.com/intellisense/ie5"">
	</HEAD>
";
			Controls.Add(ctrl1);

			HtmlGenericControl bodyCtrl = new HtmlGenericControl(nameof(HtmlTextWriterTag.Body));
			bodyCtrl.ID = "MainBodyControl";
			bodyCtrl.Attributes["MS_POSITIONING"] = "GridLayout";
			bodyCtrl.Attributes["runat"] = "server";

			ZTestForm form = new ZTestForm();
			form.ID = "Form1";
			form.Method = "post";
			form.Attributes["runat"] = "server";

			form.Controls.Add(TextBox);
			form.Controls.Add(DateControl);
			form.Controls.Add(SaveButton);
			form.Controls.Add(DeleteButton);
			bodyCtrl.Controls.Add(form);

			Controls.Add(bodyCtrl);
			Controls.Add(new LiteralControl(@"</HTML>"));
		}
		#endregion

		#region PageControls

		public ZTextBox TextBox
		{
			get
			{
				if (fTextBox == null)
				{
					fTextBox = new ZTextBox();
					fTextBox.ID = "TextBox";
					fTextBox.BindTo = DummyBizoSchema.Z0_VarCharMax.Name;
				}
				return fTextBox;
			}
		}
		ZTextBox fTextBox;

		ZDateEdit DateControl
		{
			get
			{
				if (fDateControl == null)
				{
					fDateControl = new ZDateEdit();
					fDateControl.ID = "Date";
					fDateControl.BindTo = DummyBizoSchema.Z0_Date.Name;
				}
				return fDateControl;
			}
		}
		ZDateEdit fDateControl;

		public ZButton SaveButton
		{
			get
			{
				if (fSaveButton == null)
				{
					fSaveButton = new ZButton();
					fSaveButton.ID = "Save";
					fSaveButton.Text = "Save";
					fSaveButton.Click += new EventHandler(SaveButton_Click);
				}
				return fSaveButton;
			}
		}
		ZButton fSaveButton;

		void SaveButton_Click(object sender, EventArgs e)
		{
			SaveDataSourceFactory();
		}

		public ZButton DeleteButton
		{
			get
			{
				if (fDeleteButton == null)
				{
					fDeleteButton = new ZButton();
					fDeleteButton.ID = "Delete";
					fDeleteButton.Text = "Delete";
					fDeleteButton.Click += new EventHandler(DeleteButton_Click);
				}
				return fDeleteButton;
			}
		}
		ZButton fDeleteButton;
		#endregion

		#region EventHandlers

		void DeleteButton_Click(object sender, EventArgs e)
		{
			DataSource.Delete();
		}
		#endregion

		#region Page Overrides

		protected override NameValueCollection RequestQueryString
		{
			get { return new NameValueCollection(); }
		}

		protected override Uri RequestUrl
		{
			get { return new Uri("http://www.test.com/Dummy.aspx"); }
		}

		protected override BrowserType GetBrowserType()
		{
			return BrowserType.IE;
		}

		protected override BusinessObject GetNewDataSource()
		{
			ZPageSaveTest.DummyBusinessObjectMessageErrorTesting bizo = Factory.New<ZPageSaveTest.DummyBusinessObjectMessageErrorTesting>();
			bizo.Z0_VarCharMax = "Splatty";
			bizo.Z0_Date = ZDateTime.Today.AddDays(-1);
			return bizo;
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			return new ZDummyTestGlobal();
		}

		protected override bool ShowLoginStatus
		{
			get
			{
				return ShowLoginStatusOverride;
			}
		}

		public bool ShowLoginStatusOverride;

		#endregion
	}
}
