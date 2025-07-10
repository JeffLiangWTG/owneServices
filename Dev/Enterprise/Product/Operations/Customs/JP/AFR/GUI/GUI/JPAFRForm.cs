using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRForm : ZTemplateForm, IInBondDetailInitiator
	{
		public JPAFRForm()
		{
			InitializeComponent();
		}

		public JPAFRForm(JPAFRHeader header)
			: base(header)
		{
			InitializeComponent();
			if (BusinessEntity.InBondDetailInitiator == null)
			{
				BusinessEntity.InBondDetailInitiator = this;
			}
			WorkflowTabPage.Initialize(header);
			jpafrMainUserControl.JPH_OverrideFreightDefaultsCheckBox.Visible = false;
			var actionMenuItemIndex = this.MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			var afrMenuItem = new AFRMainMenuItem(header);
			this.MainMenu.MenuItems.Add(actionMenuItemIndex + 1, afrMenuItem);

			DataContext = CoreConstants.DataContext.JPAFRHeader;
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			var isShippingLineEntry = header.JPH_IsShippingLineEntry;
			SetControls(isShippingLineEntry);
			if (isShippingLineEntry)
			{
				AddImportFromSailingMenuItem(afrMenuItem);
				header.Sailings.CountChanged -= OnSailingsCountChanged;
				header.Sailings.CountChanged += OnSailingsCountChanged;
				OnSailingsCountChanged(null, null);
				header.SynchroniseIfNeeded();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		void OnSailingsCountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
		{
			var sailings = BusinessEntity.Sailings;
			this.jpafrMainUserControl.JPH_OverrideFreightDefaultsCheckBox.Visible = sailings.Count > 0;
		}

		void SetControls(bool isShippingLineEntry)
		{
			jpafrMainUserControl.UpdateControlLayout(isShippingLineEntry);
			JPAFRSplitContainer.Panel1Collapsed = !isShippingLineEntry;
			this.jpafrMainUserControl.JPH_OverrideFreightDefaultsCheckBox.Select();
		}

		public new JPAFRHeader BusinessEntity
		{
			get { return (JPAFRHeader)base.BusinessEntity; }
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			try
			{
				if (isNotFinalizing)
				{
					onDisposing?.Invoke(this, EventArgs.Empty);
				}

				var header = BusinessEntity;
				if (header != null)
				{
					header.Sailings.CountChanged -= OnSailingsCountChanged;
				}
			}
			finally
			{
				base.Dispose(isNotFinalizing);
			}
		}

		public override string FormCaption
		{
			get
			{
				string aCaption = Res.GetString("JPAFRForm|FormCaption", "AFR");
				if (!this.IsDesignMode())
				{
					aCaption += " - " + BusinessEntity.HumanReadableName;
				}
				return aCaption;
			}
		}

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (BusinessEntity != null && billDetailsTabPage.TabVisible)
			{
				var bill = (JPAFRBills)BusinessEntity.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					MainTabControl.SelectedTab = billDetailsTabPage;
					var manager = (CurrencyManager)jpafrBillsUserControl.BindingContext[BusinessEntity, "Bills"];
					var index = manager.List.IndexOf(bill);
					if (index >= 0)
					{
						manager.Position = index;
					}
				}
			}
		}

		#region IInBondDetailInitiator Members

		void IInBondDetailInitiator.NotifyUserOfAnInvalidOperation(string text)
		{
			Globals.Message.ShowWarning(text);
		}

		event EventHandler IInBondDetailInitiator.OnDisposing
		{
			add { onDisposing += value; }
			remove { onDisposing -= value; }
		}
		event EventHandler onDisposing;

		#endregion

		#region Import Bills from sailing

		void AddImportFromSailingMenuItem(ZMenuItem afrMenuItem)
		{
			afrMenuItem.MenuItems.Add("-");
			afrMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("JPAFRMainMenuItem|ImportBillsFromSailingSchedule|2E36A158-A49E-4016-8600-69A55E392104", "Import Bills From Sailing Schedule"), ImportBillsFromSailingClick));
		}

		void ImportBillsFromSailingClick(object sender, EventArgs e)
		{
			var header = this.BusinessEntity;
			if (header.JPH_IsShippingLineEntry && header.Sailing != null)
			{
				var existingBillHandlers = new BillImportActionCollection(header.Bills);
				Action importAction = () => RunActionWithProcessBox(Res.GetString("JPAFRMainMenuItem|ImportBillsFromSailingSchedule|80E0D138-4689-49C9-989B-ACA6BA33A267", "Importing bills from sailing..."), () =>
				{
					header.ImportBillsOfLadingLinkedToTheSameSailing(existingBillHandlers);
				});
				if (existingBillHandlers.Count > 0)
				{
					using (var billAction = new ImportFromSailingForm(existingBillHandlers))
					{
						if (billAction.ShowDialog() == System.Windows.Forms.DialogResult.OK)
						{
							importAction();
						}
					}
				}
				else
				{
					importAction();
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("18DE6667-7C23-4DC0-9EFE-10C6E368AA5E", "There is no Sailing Schedule to import Bills Of Lading."));
			}
		}

		#endregion
	}
}
