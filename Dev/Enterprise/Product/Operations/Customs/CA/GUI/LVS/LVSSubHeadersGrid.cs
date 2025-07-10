using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class LVSSubHeadersGrid : ZGrid
	{
		public LVSSubHeadersGrid()
		{
			this.RowsDeleting += LVSSubHeadersGrid_RowsDeleting;
			this.DoubleClick += LVSSubHeadersGrid_DoubleClick;
		}

		void LVSSubHeadersGrid_DoubleClick(object sender, EventArgs e)
		{
			if (CurrentInvoice?.IsAttachedToPersistentLVXDeclaration ?? false)
			{
				OpenAttachedLVXForm();
			}
		}

		void OpenAttachedLVXForm()
		{
			var declaration = CurrentInvoice?.JobDeclaration;
			if (declaration != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.CA.CALVXJobs);
				controller.SetFormsModalTo(FindForm());
				controller.ShowEditForm(declaration);
			}
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)DataSource; }
		}

		JobComInvoiceHeader CurrentInvoice
		{
			get
			{
				var listManager = ListManager;
				return listManager == null ? null : (JobComInvoiceHeader)listManager.GetCurrent();
			}
		}

		#region Detach Menu Item

		ZMenuItem detachMenuItem;

		void DetachMenuItem_Click(object sender, EventArgs e)
		{
			if (List != null)
			{
				List<BusinessObject> invoicesCanDetach = new List<BusinessObject>();
				foreach (JobComInvoiceHeader invoice in SelectedElements)
				{
					if (invoice != null && Declaration.Invoices.IsAdditionalInvoice(invoice))
					{
						invoicesCanDetach.Add(invoice);
					}
				}
				if (invoicesCanDetach.Count < SelectedElements.Length)
				{
					Globals.Message.ShowError(
						Res.GetString("fcf419bd-b97e-4366-a170-756ed3d796ee", "Shipments that have not been consolidated from the Courier LVS Declarations module may not be detached. They may be deleted."),
						Res.GetString("16bae26c-19f8-400e-91d8-e9b738035771", "Some Invoices Cannot Be Detached"));
				}

				var cannotDetachReasons = new List<string>();
				foreach (JobComInvoiceHeader invoice in invoicesCanDetach)
				{
					if (invoice != null)
					{
						if (Declaration.HasAB3AcceptedOrWaiting)
						{
							var reasonForNotAbleToDetach = string.Format(LVXJobsConsolidateHelper.GetLVXHasAdditionalDeclarationWithB3AcceptedLog, new object[] { invoice.JobDeclaration.JE_DeclarationReference, Declaration.JE_DeclarationReference });

							if (!cannotDetachReasons.Contains(reasonForNotAbleToDetach))
							{
								cannotDetachReasons.Add(reasonForNotAbleToDetach);
							}
						}
						else
						{
							LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(invoice, Declaration);
						}
					}
				}

				if (cannotDetachReasons.Count > 0)
				{
					Globals.Message.ShowError(new ZStringBuilder(cannotDetachReasons).ToStringWithNewLineBetweenAppends(), Res.GetString("1bfb16b5-53d2-4883-9b96-5bd2ca6d5ffd", "Cannot Be Detached"));
				}
			}
		}

		void AddDetachMenuItem()
		{
			detachMenuItem = new ZMenuItem(ResString.GetMultilingualString("5db1bda0-eb12-485d-812a-4f16ddc13d5c", "Detach"));
			detachMenuItem.Click += DetachMenuItem_Click;

			int posOfDeleteItem = ContextMenu.MenuItems.IndexOf(DeleteMenuItem);
			ContextMenu.MenuItems.Add((posOfDeleteItem == -1 ? 0 : posOfDeleteItem + 1), detachMenuItem);
		}

		#endregion

		#region Edit Menu Item

		ZMenuItem editMenuItem;

		void EditMenuItem_Click(object sender, EventArgs e)
		{
			OpenAttachedLVXForm();
		}

		void AddEditMenuItem()
		{
			editMenuItem = new ZMenuItem(ResString.GetMultilingualString("c6e8700d-4047-41a1-b77a-f6eb4ef60383", "Edit"));
			editMenuItem.Click += EditMenuItem_Click;

			int posOfDeleteItem = ContextMenu.MenuItems.IndexOf(DeleteMenuItem);
			ContextMenu.MenuItems.Add((posOfDeleteItem == -1 ? 0 : posOfDeleteItem), editMenuItem);
		}

		#endregion

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			detachMenuItem.Visible = DeleteMenuItem.Visible;
			detachMenuItem.Enabled = DeleteMenuItem.Enabled;
			editMenuItem.Visible = DeleteMenuItem.Visible;
			editMenuItem.Enabled = DeleteMenuItem.Enabled && (CurrentInvoice?.IsAttachedToPersistentLVXDeclaration ?? false);
		}

		#region Override

		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();
			AddDetachMenuItem();
			AddEditMenuItem();
		}

		protected override void HookContextMenu()
		{
			base.HookContextMenu();
			ContextMenu.Popup += ContextMenu_Popup;
		}

		protected override void UnHookContextMenu()
		{
			base.UnHookContextMenu();
			ContextMenu.Popup -= ContextMenu_Popup;
		}

		#endregion

		#region Delete

		void LVSSubHeadersGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			List<BusinessObject> invoicesCanDelete = new List<BusinessObject>();

			foreach (JobComInvoiceHeader invoice in e.Objects)
			{
				if (!Declaration.Invoices.IsAdditionalInvoice(invoice))
				{
					invoicesCanDelete.Add(invoice);
				}
			}

			if (invoicesCanDelete.Count < e.Objects.Count())
			{
				Globals.Message.ShowError(
					Res.GetString("f5755aa4-3080-4c23-bf38-1bc6f4f99e3c", "Shipments which have been consolidated from the Courier LVS Declarations module may not be deleted. They may be detached."),
					Res.GetString("a635dab3-999c-4668-b257-e992d283765d", "Some Invoices Cannot Be Deleted"));
			}

			if (invoicesCanDelete.Count == 0)
			{
				e.Cancel = true;
			}
		}

		#endregion
	}
}
