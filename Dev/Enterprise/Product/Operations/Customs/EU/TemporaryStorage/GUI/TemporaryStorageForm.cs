using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TemporaryStorageForm : ZTemplateForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public TemporaryStorageForm(TemporaryStorageHeader header) : base(header)
		{
			Header.NotifyUnableToCompleteActionAsHouseBillExists = NotifyUnableToCompleteActionAsHouseBillExists;
			InitializeComponent();
			SetMainDetailsLayout();

			InitializeMessageControls();
			AddMessagingMenu();
			WorkflowTabPage.Initialize(header);
			UpdateBillsTabPageCaption();

			PlugIns.AddJobInvoicing(header.InvoicingSupporter);
			PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
				TemporaryStorageHeader_AMA_MessageTypeChanged(this, EventArgs.Empty);
			}
		}

		protected virtual void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs e)
		{
			if (Header != null)
			{
				ContainerTabPage.TabVisible = !Header.IsTransfer;
			}
		}

		public DynamicLayoutPanel MainDynamicLayoutPanel { get; private set; }

		ITemporaryStorageLayoutProvider fLayoutProvider;
		ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ?? (fLayoutProvider = TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header));

		void SetMainDetailsLayout()
		{
			var layout = LayoutProvider?.GetTemporaryStorageDetailsLayout();
			if (layout != null && MainDynamicLayoutPanel == null)
			{
				MainTabPage.Controls.RemoveAndDisposeAll();
				MainDynamicLayoutPanel = new DynamicLayoutPanel
				{
					Name = nameof(MainDynamicLayoutPanel),
					Dock = DockStyle.Fill,
					AutoScroll = true,
				};
				MainTabPage.Controls.Add(MainDynamicLayoutPanel);
				MainDynamicLayoutPanel.UpdateLayout(layout);
			}
		}

		void NotifyUnableToCompleteActionAsHouseBillExists(string message)
		{
			Globals.Message.Show(message);
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
		}

		public override string FormCaption
		{
			get
			{
				string caption = FormCaptionCore;
				var header = Header;
				if (!header.AMA_JobReference.IsEmpty)
				{
					var sb = new CargoWise.Types.ZStringBuilder();
					caption = sb.Append(FormCaptionCore).Append(header.AMA_JobReference).ToStringWithDelimiterBetweenAppends(" - ");
				}
				return caption;
			}
		}
		protected string FormCaptionCore => Res.GetString("C7892EF1-7792-422E-AF65-E7E7160F56BE", "Temporary Storage - UCC");

		void UpdateBillsTabPageCaption()
		{
			BillsTabPage.CaptionResourceString = BillsTabPageCaption;
		}

		protected virtual ResourceStringData BillsTabPageCaption => Res.GetData("db24bb14-a435-4c0f-a1a0-326923ec3129", "Bills");

		protected TemporaryStorageHeader Header => (TemporaryStorageHeader)BusinessEntity;

		void AddMessagingMenu()
		{
			var messagingMenu = GetNewMessagingMenu();
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenu);
		}

		protected virtual ZMenuItem GetNewMessagingMenu() => new TemporaryStorageMessagesMenu(this);

		protected override void Dispose(bool disposing)
		{
			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
			}
			base.Dispose(disposing);
		}

		void InitializeMessageControls()
		{
			MessagesUserControl.UserControlType = GetMessagesTabUserControlType();
			BindingSource.SetBindingMember(MessagesUserControl, "Messages");
		}

		protected virtual Type GetMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is UCC6TemporaryStorageBillGridControl && previousControl is UCC6TemporaryStorageBillGridControl);
		}
	}
}
