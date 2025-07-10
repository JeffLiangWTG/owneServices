using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class InvestigationItemForm : ZTemplateForm
	{
		public InvestigationItemForm(InvestigationItem investigationItem)
			: base(investigationItem)
		{
			SpellChecker.InitialiseSpellcheck(itemTextTextBox, "InvestigationItemForm_ItemTextBox");
			SetupActionMenuItemsEvent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		internal InvestigationItem investigationItem => BusinessEntity as InvestigationItem;

		void SetupActionMenuItemsEvent()
		{
			ActionsMenuItem.MenuItems.AddRange(GenericMenuItems.ToArray());
		}

		List<MenuItem> GenericMenuItems
		{
			get
			{
				if (genericMenuItems == null)
				{
					genericMenuItems = new List<MenuItem>();
					var setAllNullResultsMenuItem = new ZMenuItem(ResString.GetMultilingualString("f8dce68f-6bd9-456f-8e07-46e168b617c1", "Set all null results to"));

					confirmMenuItem = new ZMenuItem(ResString.GetMultilingualString("88b1c3fc-8594-4e8e-93b8-9a3b49514cd8", "Confirm"), ConfirmItem_Click);
					negateMenuItem = new ZMenuItem(ResString.GetMultilingualString("d73782b5-6f50-4527-b886-c02eb265c668", "Negate"), NegateItem_Click);

					setAllNullResultsMenuItem.MenuItems.Add(confirmMenuItem);
					setAllNullResultsMenuItem.MenuItems.Add(negateMenuItem);
					genericMenuItems.Add(setAllNullResultsMenuItem);
				}
				return genericMenuItems;
			}
		}

		protected List<MenuItem> genericMenuItems;

		void ConfirmItem_Click(object sender, EventArgs e)
		{
			var selectedRow = investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.ListManager.GetCurrent() as DiagnosticCriteriaInvestigationItemLink;
			if (selectedRow != null)
			{
				selectedRow.InvestigationResultPivots.Where(p => p.DCR_ResponseResult == DiagnosticCriteriaInvestigationResults.Codes.Null).ForEach(p => p.DCR_ResponseResult = DiagnosticCriteriaInvestigationResults.Codes.Confirm);
			}
		}

		void NegateItem_Click(object sender, EventArgs e)
		{
			var selectedRow = investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.ListManager.GetCurrent() as DiagnosticCriteriaInvestigationItemLink;
			if (selectedRow != null)
			{
				selectedRow.InvestigationResultPivots.Where(p => p.DCR_ResponseResult == DiagnosticCriteriaInvestigationResults.Codes.Null).ForEach(p => p.DCR_ResponseResult = DiagnosticCriteriaInvestigationResults.Codes.Negate);
			}
		}

		protected MenuItem confirmMenuItem;
		protected MenuItem negateMenuItem;

		public override string FormCaption
		{
			get
			{
				var item = investigationItem;
				if (item != null && item.INV_ItemNumber != string.Empty)
				{
					var caption = new StringBuilder();
					caption.Append(item.INV_ItemNumber);
					caption.Append(" - ");
					caption.Append(item.INV_Description);
					return caption.ToString();
				}
				else
				{
					return "Investigation Item";
				}
			}
		}

		void ResponseOptionsGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			DialogResult dialogResult = Globals.Message.Show("This response option is used in one or more diagnostic criteria links. Deleting the option will also delete the option from all links. Do you wish to proceed?", "Confirm Delete...", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (dialogResult == DialogResult.No)
			{
				e.Cancel = true;
			}
		}
	}
}
