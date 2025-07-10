using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.Customs.EU.GUI.SADH;
using Enterprise.Customs.EU.GUI.SingleLineEntry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected MenuItem SADHDataEntryFormMenuItem => sADHDataEntryFormMenuItem ??= new ZMenuItem(EDIMenuCaptions.SADHDataEntryForm, SADHDataEntryFormMenuItem_Click);
		MenuItem sADHDataEntryFormMenuItem;

		protected MenuItem SingleLineEntryMenuItem => singleLineEntryMenuItem ??= new ZMenuItem(EDIMenuCaptions.SingleLineEntry, SingleLineEntry_Click);
		MenuItem singleLineEntryMenuItem;

		protected MenuItem AutoPopulateAuthorizationsMenuItem => autoPopulateAuthorizationsMenuItem ??= new ZMenuItem(EDIMenuCaptions.AutoPopulateAuthorizations, AutoPopulateAuthorizations_Click);
		MenuItem autoPopulateAuthorizationsMenuItem;

		MenuItem InventoriesSelectionFromTSRegisterManagementMenuItem => inventoriesSelectionFromTSRegisterManagementMenuItem ??= new ZMenuItem(EDIMenuCaptions.InventoriesSelectionFromTSRegisterManagement, InventoriesSelectionFromTSRegisterManagementMenuItem_Click);
		MenuItem inventoriesSelectionFromTSRegisterManagementMenuItem;

		MenuItem TSRegisterManagementMenuItem => tsRegisterManagementMenuItem ??= new ZMenuItem(EDIMenuCaptions.TSRegisterManagement, new MenuItem[] { InventoriesSelectionFromTSRegisterManagementMenuItem });
		MenuItem tsRegisterManagementMenuItem;

		protected MenuItem SupplementaryEntryMenuItem => supplementaryEntryMenuItem ??= new ZMenuItem(EDIMenuCaptions.SupplementaryEntry);
		MenuItem supplementaryEntryMenuItem;

		protected MenuItem NewRelatedDeclarationMenuItem => newRelatedDeclarationMenuItem ??= new ZMenuItem(EDIMenuCaptions.NewRelatedDeclaration, (s, e) => SupplementaryHelperUI.NewRelatedDeclaration(Declaration));
		MenuItem newRelatedDeclarationMenuItem;

		protected MenuItem NewEntryInstructionMenuItem => newEntryInstructionMenuItem ??= new ZMenuItem(EDIMenuCaptions.NewEntryInstruction, (s, e) => SupplementaryHelperUI.NewEntryInstruction(Declaration));
		MenuItem newEntryInstructionMenuItem;

		protected MenuItem ReUseEntryInstructionMenuItem => reUseEntryInstructionMenuItem ??= new ZMenuItem(EDIMenuCaptions.ReUseEntryInstruction, (s, e) => SupplementaryHelperUI.ReUseEntryInstruction(Declaration));
		MenuItem reUseEntryInstructionMenuItem;

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Add(SADHDataEntryFormMenuItem);
			MenuItems.Add(SingleLineEntryMenuItem);
			dataMenuItem.MenuItems.Add(AutoPopulateAuthorizationsMenuItem);

			TSRegisterManagementMenuItem.Name = nameof(TSRegisterManagementMenuItem);
			var bondedWarehouseMenuItemIndex = MenuItems.IndexOf(bondedWarehouseMenuItem);
			MenuItems.Add(bondedWarehouseMenuItemIndex + 1, TSRegisterManagementMenuItem);

			AddSupplementaryMenuItems();
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			TSRegisterManagementMenuItem.Visible = Declaration?.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible ?? false;
			SupplementaryEntryMenuItem.Visible = Declaration?.IsSupplementaryMenuVisible ?? false;
		}

		void SADHDataEntryFormMenuItem_Click(object sender, EventArgs e)
		{
			var dataManager = new SADHFormDataManager(Declaration);
			ZFormModaliser.ShowDialogAndDispose(new SADHEntryForm(dataManager));
		}

		void SingleLineEntry_Click(object sender, EventArgs e)
		{
			var singleLineEntryManager = GetNewSingleLineEntryManager();
			ZFormModaliser.ShowDialogAndDispose(GetNewSingleLineEntryForm(singleLineEntryManager));
		}

		void AutoPopulateAuthorizations_Click(object sender, EventArgs e)
		{
			Declaration.AuthorizationUsageUpdater.UpdateEntryInstructionAuthorizations();
		}

		void AddSupplementaryMenuItems()
		{
			MenuItems.Add(SupplementaryEntryMenuItem);
			SupplementaryEntryMenuItem.MenuItems.Add(NewRelatedDeclarationMenuItem);
			SupplementaryEntryMenuItem.MenuItems.Add(NewEntryInstructionMenuItem);
			SupplementaryEntryMenuItem.MenuItems.Add(ReUseEntryInstructionMenuItem);
		}

		void InventoriesSelectionFromTSRegisterManagementMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(Declaration.PK);
				newFactoryDeclaration.Reload();

				var temporaryStorageSelectionHeader = new CusTempStorageRegLinesSelectionHeader(factory);
				using (var form = new TemporaryStorageRegisterLinesSelectionForm(temporaryStorageSelectionHeader))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						if (newFactoryDeclaration.MapSelectedInventoryFromTS(temporaryStorageSelectionHeader.SelectedLines))
						{
							try
							{
								factory.Save();
								ReloadBaseForm();

								Globals.Message.ShowInformation(ResString.GetMultilingualString("4F284A12-5799-4726-8BA2-BB528A9AA1CD", "Import success; all has been saved"));
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
						else
						{
							Globals.Message.Show(ResString.GetMultilingualString("F6A5D9DB-4233-41BE-BD02-007D41BF6D32", "Import not success; nothing has been saved"));
						}
					}
				}
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected virtual ZForm ReloadBaseForm()
		{
			return Form.ReloadForm();
		}

		protected virtual SingleLineEntryManager GetNewSingleLineEntryManager() =>
			new SingleLineEntryManager(Declaration);

		protected virtual SingleLineEntryForm
			GetNewSingleLineEntryForm(SingleLineEntryManager singleLineEntryManager) =>
			new SingleLineEntryForm(singleLineEntryManager);
	}
}
