using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class SourceModuleFinderForm : ZChildForm
	{
		public SourceModuleFinderForm(SourceModuleFinder sourceModuleFinder)
			: base(sourceModuleFinder)
		{
			SourceModuleGrid.GetColumnStyle("ModuleCode").Caption = string.Format("{0} Code", EnumExtensions.GetCaption(BusinessEntity.ModuleType));
			SourceModuleGrid.GetColumnStyle("ModuleDescription").Caption = string.Format("{0} Description", EnumExtensions.GetCaption(BusinessEntity.ModuleType));
		}

		new SourceModuleFinder BusinessEntity
		{
			get { return (SourceModuleFinder)base.BusinessEntity; }
		}

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (string.IsNullOrEmpty(BusinessEntity.FindReason))
			{
				FindReasonPanel.Visible = false;
			}

			DescriptionTextBox.Select();

			if (ShowCloseButtonOnly)
			{
				OkButton.Visible = false;
				CancelZButton.CaptionResourceString = Res.GetData("21724290-b9a1-4cd3-a012-4fb74b1ca214", "Close");
			}

			if (ShowDescriptionFilterOnly)
			{
				productAreaDropEdit.Visible = false;
				menuSectionDropEdit.Visible = false;
				DescriptionTextBox.CaptionResourceString = Res.GetData("d0c252b3-e771-41dc-b770-3c97b41aaf23", "Filter by Description");

				var increaseY = DescriptionTextBox.Location.Y - productAreaDropEdit.Location.Y;
				MoveControl(DescriptionTextBox, increaseY);
				MoveControl(SourceModuleGrid, increaseY, adjustHeight: true);
			}
		}

		static void MoveControl(Control control, int deltaY, bool adjustHeight = false)
		{
			var location = control.Location;
			location.Y -= deltaY;
			var size = control.Size;
			control.Location = location;

			if (adjustHeight)
			{
				size.Height += deltaY;
				control.Size = size;
			}
		}

		#endregion

		#region Event Handlers

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Enter))
			{
				SourceModuleGrid.Focus();
				if (SourceModuleGrid.SelectedRowCount == 0 && SourceModuleGrid.ListManager.Count > 0)
				{
					SourceModuleGrid.Select(SourceModuleGrid.ListManager.Position);
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		internal void OkButton_Click(object sender, EventArgs e)
		{
			HandleEnterOrDoubleClick();
		}

		void SourceModuleGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var hitTestInfo = SourceModuleGrid.HitTest(e.Location);
			if (hitTestInfo.Type == DataGrid.HitTestType.Cell || hitTestInfo.Type == DataGrid.HitTestType.RowHeader)
			{
				HandleEnterOrDoubleClick();
			}
		}

		void SourceModuleGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				HandleEnterOrDoubleClick();
			}
		}

		void HandleEnterOrDoubleClick()
		{
			var current = SourceModuleGrid.ListManager.GetCurrent() as ModuleMappingWithSourceModule ?? new ModuleMappingWithSourceModule(new ProductAreaModuleMapping { ProductArea = BusinessEntity.ProductAreaFilter, ModuleCode = BusinessEntity.ModuleFilter }, new SourceModule());

			OnSourceModuleSelected(current);
		}

		void OnSourceModuleSelected(ModuleMappingWithSourceModule moduleMappingWithSourceModule)
		{
			if (ModuleMappingWithSourceModuleSelected != null)
			{
				ModuleMappingWithSourceModuleSelected.Invoke(SourceModuleGrid, new ModuleMappingWithSourceModuleSelectedArgs(moduleMappingWithSourceModule));
				Close();
			}
		}

		void DescriptionTextBox_TextChanged(object sender, EventArgs e)
		{
			BusinessEntity.RefreshSourceModules(DescriptionTextBox.Text);
		}

		#endregion

		public event EventHandler<ModuleMappingWithSourceModuleSelectedArgs> ModuleMappingWithSourceModuleSelected;

		public bool ShowCloseButtonOnly { get; set; }
		public bool ShowDescriptionFilterOnly { get; set; }
	}

	public class ModuleMappingWithSourceModuleSelectedArgs : EventArgs
	{
		public ModuleMappingWithSourceModuleSelectedArgs(ModuleMappingWithSourceModule moduleMappingWithSourceModule)
		{
			ModuleMappingWithSourceModule = moduleMappingWithSourceModule;
		}

		public readonly ModuleMappingWithSourceModule ModuleMappingWithSourceModule;
	}
}
