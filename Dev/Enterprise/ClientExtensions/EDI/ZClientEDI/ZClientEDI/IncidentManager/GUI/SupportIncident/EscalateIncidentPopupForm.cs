using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EscalateIncidentPopupForm : BaseIncidentPopupForm
	{
		public EscalateIncidentPopupForm(SupportIncidentEscalateAction action)
			: base(action)
		{
		}

		SupportIncidentEscalateAction Action
		{
			get { return BusinessEntity as SupportIncidentEscalateAction; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HideModuleSelection();
			EscalationCriticalityInfo_ValueChanged(null, EventArgs.Empty);
			Action.EscalationCriticalityInfo.ValueChanged += EscalationCriticalityInfo_ValueChanged;
			AddShowSourceModuleFinderPopupEventHandlers();
		}

		void HideModuleSelection()
		{
			ShowOrHideModuleSelectionControls(new Control[] { ProductAreaLabel, ProductAreaDropEdit, ModuleDropEdit, ModuleLabel }, false);
			ShowOrHideModuleSelectionControls(new Control[] { MenuItemLabel, MenuItemPathLabel }, false);
		}

		void ShowModuleSelection()
		{
			ShowOrHideModuleSelectionControls(new Control[] { ProductAreaLabel, ProductAreaDropEdit, ModuleDropEdit, ModuleLabel }, true);
		}

		void ShowOrHideModuleSelectionControls(Control[] moduleSelectionControls, bool visible)
		{
			if (moduleSelectionControls.Length > 0 && moduleSelectionControls[0].Visible != visible)
			{
				foreach (var moduleSelectionControl in moduleSelectionControls)
				{
					moduleSelectionControl.Visible = visible;
				}

				int controlsMoveDistance = ControlDpiScalingHelper.ScaleToCurrentDpiY(visible ? 26 : -26);
				MessageLabel.Location = ControlDpiScalingHelper.NewScaledPoint(MessageLabel.Location.X, MessageLabel.Location.Y + controlsMoveDistance, false);
				CommentTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(CommentTextBox.Location.X, CommentTextBox.Location.Y + controlsMoveDistance, false);

				ControlDpiScalingHelper.SetHeight(CommentTextBox, CommentTextBox.Height - controlsMoveDistance, false);
				ControlDpiScalingHelper.SetHeight(this, Height + controlsMoveDistance, false);
			}
		}

		void EscalationCriticalityInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Action.EscalationCriticality.IsEmpty || !Action.HasModuleListTypeChanged)
			{
				HideModuleSelection();
			}
			else
			{
				ShowModuleSelection();
			}

			ModuleLabel.Text = Action.GetSectionRequirementServiceLabelText() + ":";

			if (Action.MenuItem.IsEmpty)
			{
				ShowOrHideModuleSelectionControls(new Control[] { MenuItemLabel, MenuItemPathLabel }, false);
			}
		}

		#region Source Module Override

		void ShowSourceModuleFinderPopupIfRequired()
		{
			if (Action.ProductArea.IsEmpty)
			{
				return;
			}

			Action.ValidateSectionRequirementService();
			if (Action.SectionRequirementServiceInfo.HasErrors())
			{
				return;
			}

			var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(Action.ModuleListType);
			if (productAreaModuleMappingsRegistryItem == null)
			{
				return;
			}

			var mapping = productAreaModuleMappingsRegistryItem.Value.GetMapping(Action.Incident.IM_Product, Action.SectionRequirementService);
			if (mapping == null || mapping.SourceModuleMappings.Count == 0)
			{
				return;
			}

			var recalculatedProductArea = Action.RecalculateProductArea();
			if (recalculatedProductArea != Action.ProductArea)
			{
				ShowSourceModuleFinderPopup();
			}
		}

		void ShowSourceModuleFinderPopup()
		{
			var findReason = string.Format(@"You have selected a '{0}' that is shared by multiple Product Areas.
The current Menu Item does not belong to the currently selected Product Area ({1}), please select the correct Menu Item to determine the correct Product Area.", EnumExtensions.GetCaption(Action.ModuleListType), Action.ProductAreaDescription);

			var finder = new SourceModuleFinder(Action.Incident.IM_Product, Action.ModuleListType, findReason, Action.Factory);
			finder.ProductAreaFilter = Action.ProductArea;
			finder.ModuleFilter = Action.SectionRequirementService;

			bool isSourceModuleSelected = false;
			var form = new SourceModuleFinderForm(finder);
			form.ModuleMappingWithSourceModuleSelected += (sender, e) =>
			{
				RemoveShowSourceModuleFinderPopupEventHandlers();
				Action.MenuItem = e.ModuleMappingWithSourceModule.SourceModuleCode;
				Action.SectionRequirementService = e.ModuleMappingWithSourceModule.ModuleCode;
				Action.ProductArea = e.ModuleMappingWithSourceModule.ProductArea;
				isSourceModuleSelected = true;
				AddShowSourceModuleFinderPopupEventHandlers();
			};

			form.FormClosing += (sender, e) =>
			{
				ShowOrHideModuleSelectionControls(new Control[] { MenuItemLabel, MenuItemPathLabel }, true);
				if (!isSourceModuleSelected)
				{
					Action.ProductArea = Action.RecalculateProductArea();
				}
			};

			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void AddShowSourceModuleFinderPopupEventHandlers()
		{
			Action.ProductAreaInfo.ValueChanged += ProductAreaInfo_ValueChanged;
			Action.SectionRequirementServiceInfo.ValueChanged += SectionRequirementServiceInfo_ValueChanged;
		}

		void RemoveShowSourceModuleFinderPopupEventHandlers()
		{
			Action.ProductAreaInfo.ValueChanged -= ProductAreaInfo_ValueChanged;
			Action.SectionRequirementServiceInfo.ValueChanged -= SectionRequirementServiceInfo_ValueChanged;
		}

		void SectionRequirementServiceInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowSourceModuleFinderPopupIfRequired();
		}

		void ProductAreaInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowSourceModuleFinderPopupIfRequired();
		}

		#endregion
	}
}
