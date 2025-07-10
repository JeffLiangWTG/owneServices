using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MiscDeclarationForm : ZTemplateForm
	{
		public MiscDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
			zWorkflowTabPage.Initialize(declaration);
			AddPlugins();
			SetTabPagesVisibility();
			SetEntriesTabPageControl();
			LoadDetailsUserControl(declaration);
		}
		readonly JobDeclaration declaration;

		protected virtual void AddPlugins()
		{
			PlugIns.AddJobInvoicing(declaration.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			AddBrokerageMenuItems();
		}

		public override string FormCaption
		{
			get
			{
				var caption = ZString.Empty;
				if (!this.IsDesignMode() && declaration != null)
				{
					var additionalCaption = string.IsNullOrEmpty(declaration.JE_DeclarationReference) ? ZString.Empty : (ZString)(" - " + declaration.JE_DeclarationReference);
					if (declaration.IsPersonalItemDeclaration)
					{
						caption = string.Format(Res.GetString("AB9FD77A-B762-41EA-A65D-2ACA4729A86A", "(008) Personal Items Declaration {0}"), additionalCaption);
					}
					else if (declaration.IsD87)
					{
						caption = string.Format(Res.GetString("4CD04A32-0898-4115-AAFF-714EAB8A62A6", "(D87) Carnet Temporary Import Certificate {0}"), additionalCaption);
					}
					else if (declaration.Is5SM)
					{
						caption = string.Format(Res.GetString("46F11137-E134-4A59-BE7E-165C50F59C0B", "(5SM) Valuation Declaration Template {0}"), additionalCaption);
					}
				}
				return caption;
			}
		}

		void SetTabPagesVisibility()
		{
			ValuationTabPage.TabVisible = declaration.Is5SM;
		}

		void SetEntriesTabPageControl()
		{
			if (declaration.IsPersonalItemDeclaration)
			{
				var control = new PersonalItemsEntriesUserControl();
				control.Dock = DockStyle.Fill;
				EntriesTabPage.Controls.Add(control);
			}
			else if (declaration.IsD87)
			{
				var control = new CarnetEntryUserControl();
				control.Dock = DockStyle.Fill;
				EntriesTabPage.Controls.Add(control);
			}
			else if (declaration.Is5SM)
			{
				var control = new MessagesTabUserControl();
				control.Dock = System.Windows.Forms.DockStyle.Fill;
				this.BindingSource.SetBindingMember(control, "CustomsEntryHeaders.Messages");
				EntriesTabPage.Controls.Add(control);
				EntriesTabPage.CaptionResourceString = Res.GetData("2547B4EF-DBBE-424B-913B-80A314F3B33F", "Messages");
			}
		}

		void LoadDetailsUserControl(JobDeclaration declaration)
		{
			var userControl = GetDeclarationUserControl(declaration);
			if (userControl != null)
			{
				userControl.AutoScroll = true;
				userControl.Dock = DockStyle.Fill;
				MainTabPage.Controls.Add(userControl);
				userControl.Visible = true;
				userControl.BindingContext = new ZBindingContext();
				userControl.SetDataBinding(declaration, "");
			}
		}

		ZUserControl GetDeclarationUserControl(JobDeclaration declaration)
		{
			ZUserControl result = null;

			if (declaration.IsPersonalItemDeclaration)
			{
				result = new PersonalItemsDeclarationUserControl();
			}
			else if (declaration.IsD87)
			{
				result = new CarnetDeclarationUserControl();
			}
			else if (declaration.Is5SM)
			{
				result = new ValuationDeclarationTemplateUserControl();
				MinimumSize = ControlDpiScalingHelper.NewScaledSize(1100, 725, true);
			}

			return result;
		}
		protected IEDIMenu GetNewTopLevelMenu()
		{
			if (fMenu == null)
			{
				fMenu = declaration != null && declaration.JE_IsCancelled ? new EDIMenuStub() : GetNewTopLevelMenuCore();
			}

			return fMenu;
		}
		IEDIMenu fMenu;

		protected virtual IEDIMenu GetNewTopLevelMenuCore()
		{
			return new MiscEDIMenu();
		}

		void AddBrokerageMenuItems()
		{
			topLevelMenu = (ZMenuItem)GetNewTopLevelMenu();
			((IEDIMenu)topLevelMenu).Declaration = declaration;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), topLevelMenu);
		}
		ZMenuItem topLevelMenu;
	}
}
