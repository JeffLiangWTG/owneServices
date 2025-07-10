using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.GUI;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI
{
	public sealed partial class CreateDeclarationForm : ZChildForm
	{
		public CreateDeclarationForm(CreateDeclarationBizObj bizObj) : base(bizObj)
		{
			InitializeComponent();
			BusinessEntity.HasChangesChanged += BusinessEntityOnHasChangesChanged;
			OkButton.Enabled = BusinessEntity.CanCreateDeclarations;

			if (bizObj.CreateFromWarehouseOrder)
			{
				DeclarantsRefTextBox.PlaceHolderText = Res.GetString("232F1923-F975-45F6-B436-50169212B452", "Auto-generated from Order");
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			BusinessEntityOnHasChangesChanged(BusinessEntity, HasChangesChangedEventArgs.Create(true, this));
		}

		void BusinessEntityOnHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			OkButton.Enabled = BusinessEntity.CanCreateDeclarations;
			CustomsDeadlineEdit.Visible = DeclarationTypeDropEdit.CodeBox.Text.Equals(ImportDeclarationTypeList.Codes.AVABR);
		}

		public new CreateDeclarationBizObj BusinessEntity => (CreateDeclarationBizObj)base.BusinessEntity;

		public override string FormVerb => string.Empty;

		void okButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.CanCreateDeclarations)
			{
				var error = false;
				if (BusinessEntity.CreateFromWarehouseOrder)
				{
					var factory = BusinessEntity.Factory;
					Hide();
					var order = PromptForOrderSelection(factory);

					if (order != null)
					{
						var msg = BusinessEntity.CreateDeclarationsForWarehouseOrder(order);
						if (!string.IsNullOrEmpty(msg))
						{
							Show();
							Globals.Message.ShowError(msg);
							error = true;
						}
					}
				}
				else
				{
					var inventorySelectionHeader = BusinessEntity is CreateDeclarationIPR
						? new InventorySelectionHeaderForIPRDeclarationCreation(BusinessEntity) : new InventorySelectionHeaderForDeclarationCreation(BusinessEntity);

					using (var inventorySelectionForm = new InventorySelectionForm(inventorySelectionHeader))
					{
						Hide();
						ZFormModaliser.ShowDialogWithoutDispose(inventorySelectionForm);
					}
				}

				if (!error)
				{
					Close();
				}
			}
		}

		IWhsOrder PromptForOrderSelection(BusinessObjectFactory factory)
		{
			var collection = BondedWarehousingHelper.GetCollectionForWhsOrderSelection(factory);

			var emptyFilter = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(StmModuleFilter) }.New<StmModuleFilter>();
			var order = (IWhsOrder)BusinessObjectModulePicker.PickOneRecordFromModuleScreen<BusinessObject>(collection, ModuleIDs.WhsOrder, emptyFilter, shouldLoadLayoutEvenWhenUnsaved: true, okButtonCaption: Res.GetData("b7027a87-634d-48f2-b1a1-7a8a3d072b37", "Select").Caption);

			return order;
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
