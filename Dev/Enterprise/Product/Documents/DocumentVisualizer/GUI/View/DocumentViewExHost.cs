using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DocumentViewExHost : IDocumentViewExHost
	{
		DocumentViewExHost(ZForm form, ZTabControl tabControl, ZTabPage tabPage, ZPanel panel)
		{
			this.form = form ?? throw new ArgumentNullException(nameof(form));
			this.tabControl = tabControl ?? throw new ArgumentNullException(nameof(tabControl));
			this.tabPage = tabPage ?? throw new ArgumentNullException(nameof(tabPage));
			this.tabPage = tabPage ?? throw new ArgumentNullException(nameof(tabPage));
			this.panel = panel ?? throw new ArgumentNullException(nameof(panel));
		}

		readonly ZForm form;
		readonly ZTabControl tabControl;
		readonly ZTabPage tabPage;
		readonly ZPanel panel;

		public static DocumentViewExHost Create(ZForm form, ZTabControl tabControl, out ZTabPage tabPage)
		{
			if (form == null
				|| tabControl == null)
			{
				tabPage = null;
				return null;
			}

			tabPage = new ZTabPage();
			tabPage.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("dedebe7e-fead-4cc5-961a-6388d0864440", "Document");
			tabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			tabPage.Name = "tabPage";
			tabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 581, true);
			tabPage.TabIndex = 0;

			var mainPanel = new ZPanel();
			mainPanel.AllowDrop = true;
			mainPanel.Dock = DockStyle.Fill;
			mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainPanel.Name = "mainPanel";
			mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 581, true);
			mainPanel.TabIndex = 1;

			tabPage.Controls.Add(mainPanel);

			tabControl.Controls.Add(tabPage);

			return new DocumentViewExHost(form, tabControl, tabPage, mainPanel);
		}

		public string Text
		{
			get => tabPage.Text;
			set => tabPage.Text = value;
		}

		public void ShowDocumentViewEx(IDocumentViewEx view)
		{
			if (view is Control control)
			{
				control.Dock = DockStyle.Fill;
				panel.Controls.Add(control);
			}
		}

		public void ShowLogsView(IStmALogParent logParent)
		{
			if (logParent != null
				&& !HaveLogsTabFor(logParent))
			{
				var eventsTabPage = new ZStmALogTabPage();
				eventsTabPage.CaptionResourceString = Res.GetData("8d53293c-2972-429f-b5a9-b6e83465d65c", "Events");
				eventsTabPage.Tag = logParent;
				tabControl.Controls.Add(eventsTabPage);
				eventsTabPage.SetDataBinding(logParent, "");
			}
		}

		bool HaveLogsTabFor(IStmALogParent logParent)
		{
			return tabControl
				.AllTabPages
				.OfType<ZStmALogTabPage>()
				.Any(page => page.Tag == logParent);
		}

		public void Close() => form.Close();
		public void Focus() => form.Focus();
	}
}
