using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseEntryInstructionDetailsUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();
			InitializeEntryInstructionsGrid();
			SetUpContextMenu();

			BondHolderOrganisationControl.Visible = false;
			NewOwnerOrganisationControl.Visible = false;
			RemoverOrganisationControl.Visible = false;
			BondHolderRemoverPanel.Visible = false;
			EntryInstructionsGrid.AfterBind += EntryInstructionsGrid_AfterBind;
		}

		void EntryInstructionsGrid_AfterBind(object sender, EventArgs e)
		{
			if (EntryInstructionsGrid.ListManager is CurrencyManager currencyManager)
			{
				currencyManager.CurrentChanged += EntryInstructionsGridListManager_CurrentChanged;
			}
			HookCurrentEntryInstructionEventsIfChanged();

			if (JobDeclaration?.InvoiceLines is IBindingList jobInvoiceLinesBinding)
			{
				jobInvoiceLinesBinding.ListChanged += JobInvoiceLinesBinding_Changed;
			}

			RefreshLastBindPropertyInfo();
			RefreshTabControlVisibility();
		}

		void EntryInstructionsGridListManager_CurrentChanged(object sender, EventArgs e)
		{
			HookCurrentEntryInstructionEventsIfChanged();
			RefreshLastBindPropertyInfo();
			RefreshTabControlVisibility();
		}

		void HookCurrentEntryInstructionEventsIfChanged()
		{
			var newInstruction = EntryInstructionsGrid.ListManager?.GetCurrent() is CusEntryInstruction currentGridInstruction && !currentGridInstruction.IsDeleted
				? currentGridInstruction : null;

			if (currentInstruction != newInstruction)
			{
				UnHookInstructionEvents(currentInstruction);
				HookInstructionEvents(newInstruction);
				currentInstruction = newInstruction;
			}
		}

		void UnHookInstructionEvents(CusEntryInstruction instruction)
		{
			if (instruction != null)
			{
				instruction.RiskManagements.CountChanged -= RiskManagements_CountChanged;
			}
		}

		void HookInstructionEvents(CusEntryInstruction instruction)
		{
			if (instruction != null)
			{
				instruction.RiskManagements.CountChanged -= RiskManagements_CountChanged;
				instruction.RiskManagements.CountChanged += RiskManagements_CountChanged;
			}
		}

		CusEntryInstruction currentInstruction;

		void RiskManagements_CountChanged(object sender, CollectionCountChangedEventArgs e) => RefreshTabControlVisibility();

		void JobInvoiceLinesBinding_Changed(object sender, ListChangedEventArgs e)
		{
			if (RefreshLastBindPropertyInfo())
			{
				RefreshTabControlVisibility();
			}
		}

		bool RefreshLastBindPropertyInfo()
		{
			var currentProcedure = currentInstruction?.InvoiceLines.FirstOrDefault()?.JI_Procedure ?? ZString.Empty;

			if (lastInvoiceLineProcedure == currentProcedure)
			{
				return false;
			}

			lastInvoiceLineProcedure = currentProcedure;
			return true;
		}

		ZString lastInvoiceLineProcedure;

		void RefreshTabControlVisibility()
		{
			TransitPermitsTabPage.TabVisible = currentInstruction?.IsTransitPermitsTabPageVisible ?? false;
			RiskTabPage.TabVisible = currentInstruction?.IsRiskTabPageTabVisible ?? false;
			TabControl.Visible = TabControl.TabPages.Cast<ZTabPage>().Any(x => x.TabVisible);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (JobDeclaration is JobDeclaration declaration)
				{
					foreach (CusEntryInstruction instruction in declaration.CustomsEntryInstructions)
					{
						instruction.UnlockGuaranteeManagementMutex();
					}
					if (declaration.InvoiceLines is IBindingList jobInvoiceLinesBinding)
					{
						jobInvoiceLinesBinding.ListChanged -= JobInvoiceLinesBinding_Changed;
					}
				}
				if (EntryInstructionsGrid.ListManager is CurrencyManager currencyManager)
				{
					currencyManager.CurrentChanged -= EntryInstructionsGridListManager_CurrentChanged;
				}
				UnHookInstructionEvents(currentInstruction);
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeEntryInstructionsGrid()
		{
			EntryInstructionsGrid.ColumnStyles.AddRange(new[]
			{
				new ZGuidFindBoxColumnStyleInfo
				{
					ColumnName = CusEntryInstruction.Schema.CEI_OA_Warehouse,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},

				new ZGuidFindBoxColumnStyleInfo
				{
					ColumnName = CusEntryInstruction.Schema.CEI_OA_Warehouse2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},

				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryInstruction.Schema.EntryNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
			});

			var styleColumnStyleInfo = EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_Style);
			styleColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var dateForDutyColumnStyleInfo = EntryInstructionsGrid.GetColumnStyle(CusEntryInstruction.Schema.CEI_DateForDuty);
			dateForDutyColumnStyleInfo.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("C153DBDC-BF59-494D-94A8-D9C64F6AFF66", "Date for Duty");

			EntryInstructionsGrid.ReOrderColumns(columnsInOrder);
		}

		void SetUpContextMenu()
		{
			PermitContainersGrid.ContextMenu.MenuItems.Add(new ZMenuItem(Res.GetString("ded59b7b-b1da-48e3-acd6-4fe7d3e82dcc", "Add All Containers"), PermitContainersGridMenuItem_AddAllContainers));
		}

		void PermitContainersGridMenuItem_AddAllContainers(object sender, EventArgs e)
		{
			if (CusInBondPermitsGrid.GetCurrent() is CusInBondMoveHeader permit)
			{
				permit.AddAllContainers();
			}
		}

		readonly string[] columnsInOrder = new[]
		{
			CusEntryInstruction.Schema.CEI_Style,
			CusEntryInstruction.Schema.CEI_Description,
			CusEntryInstruction.Schema.CEI_OA_Warehouse,
			CusEntryInstruction.Schema.CEI_OA_Warehouse2,
			CusEntryInstruction.Schema.CEI_DateForDuty,
			CusEntryInstruction.Schema.EntryNumber,
		};
	}
}
