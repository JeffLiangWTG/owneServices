using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class TemporaryStorageForm : ZTemplateForm
	{
		public TemporaryStorageForm(CusTempStorageJobHeader header)
			: base(header)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(header);
			AddMessagingMenu();
			SetTabVisibility();
			SetTabSortOrder();
		}

		void AddMessagingMenu()
		{
			if (Header.IsReExport)
			{
				var messagingReExportMenu = new ReExportMessagingMenu { Header = Header };
				MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), messagingReExportMenu);
			}
			else
			{
				var messagingSumAMenu = new SumAMessagingMenu { Header = Header };
				MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), messagingSumAMenu);
			}
		}

		void SetTabVisibility()
		{
			var isReExport = Header.IsReExport;
			SumADeclarationTabPage.TabVisible = !isReExport;
			AmendmentsTabPage.TabVisible = !isReExport;
			RexDeclarationTabPage.TabVisible = isReExport;
			MainTabPage.Text = isReExport ? TemporaryStorageApplicationCodeList.Descriptions.REX : TemporaryStorageApplicationCodeList.Descriptions.SumA;
			REXDISMessagesTabPage.TabVisible = isReExport;
		}

		void SetTabSortOrder()
		{
			var isReExport = Header.IsReExport;
			if (isReExport)
			{
				var tabPages = new List<ZTabPage>(new[] { MainTabPage, RexDeclarationTabPage, REXDISMessagesTabPage, WorkflowTabPage, NotesTabPage, LogsTabPage });
				tabPages.AddRange(MainTabControl.TabPages.Cast<ZTabPage>().Where(x => !tabPages.Contains(x)));
				MainTabControl.TabPages.Clear();
				MainTabControl.TabPages.AddRange(tabPages.ToArray());
			}
		}

		CusTempStorageJobHeader Header => (CusTempStorageJobHeader)BusinessEntity;

		public override string FormCaption
		{
			get
			{
				string caption = FormCaptionCore;
				var header = Header;
				if (!header.SJH_JobReference.IsEmpty && header.Customer != null && !header.Customer.OH_Code.IsEmpty && header.Branch != null && !header.Branch.GB_Code.IsEmpty)
				{
					var sb = new CargoWise.Types.ZStringBuilder();
					caption = sb.Append(FormCaptionCore).Append(header.SJH_JobReference).Append(header.Customer.OH_Code).Append(header.Branch.GB_Code).ToStringWithDelimiterBetweenAppends(" - ");
				}
				return caption;
			}
		}

		protected string FormCaptionCore
		{
			get
			{
				var header = Header;
				var desc = header.Factory.GetCachedValue<TemporaryStorageApplicationCodeList>().GetDescriptionFromCode(header.SJH_AppCode);
				return string.IsNullOrWhiteSpace(desc)
					? Res.GetString("71cf1caa-b034-4e8d-93a4-dbda5c887c5f", "Declaration")
					: Res.GetString("2b89eb99-f36b-4936-94eb-3f68b602df18", "{0} Declaration", desc);
			}
		}

		void DeclarationTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (SumADeclarationTabControl.SelectedTab == DeclarationTabPage)
			{
				CUSPRLTabPageUserControl.LoadUserControl();
			}
		}

		void MainTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (MainTabControl.SelectedTab == SumADeclarationTabPage && SumADeclarationTabControl.SelectedTab == DeclarationTabPage)
			{
				CUSPRLTabPageUserControl.LoadUserControl();
			}
		}
	}
}
