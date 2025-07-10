using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Tools;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class CloseIncidentPopupForm : BaseIncidentPopupForm, ICloseIncidentPopupForm
	{
		public CloseIncidentPopupForm(SupportIncidentCloseAction action)
			: base(action)
		{
			spellChecker = SpellChecker.InitialiseSpellcheck(resolutionCommentTextBox, "CloseIncidentPopupForm_ResolutionCommentTextBox");
			UpdateKnownNames();
		}

		public CloseIncidentPopupForm()
		{
			spellChecker = SpellChecker.InitialiseSpellcheck(resolutionCommentTextBox, "CloseIncidentPopupForm_ResolutionCommentTextBox");
		}

		SupportIncidentCloseAction IncidentAction
		{
			get { return (SupportIncidentCloseAction)BusinessEntity; }
		}

		void UpdateKnownNames()
		{
			var knownNames = new List<string>();

			knownNames.Add(IncidentAction.Incident?.Contact?.Name);
			IncidentAction.Incident?.EConversation.ExistingConversation?.Staff.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			IncidentAction.Incident?.EConversation.ExistingConversation?.RelatedParties.ForEach(staff => knownNames.Add(staff.Parent?.Name));
			IncidentAction.Incident?.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(task => knownNames.Add(task.StaffName));

			spellChecker.UpdateWordsToIgnore(knownNames.Where(name => !string.IsNullOrWhiteSpace(name)));
		}

		void resolutionCommentTextBox_TextChanged(object sender, EventArgs e)
		{
			int xmlCharsRemoved;
			string xmlEscapedText = ZXmlValidation.EscapeInvalidXmlCharacters(resolutionCommentTextBox.Text, out xmlCharsRemoved);
			if (xmlCharsRemoved > 0)
			{
				var previousSelectionStart = resolutionCommentTextBox.SelectionStart;
				resolutionCommentTextBox.Text = xmlEscapedText;
				resolutionCommentTextBox.SelectionStart = Math.Max(previousSelectionStart - xmlCharsRemoved, 0);
			}
		}

		protected override void CloseButtonClickCore(object sender, EventArgs e)
		{
			spellChecker.CheckSpelling();
			base.CloseButtonClickCore(sender, e);
		}

		public ZDialogResult ShowDialogAndDispose()
		{
			return (ZDialogResult)ZFormModaliser.ShowDialogAndDispose(this);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			HideModuleSelection();
			EscalationCriticalityInfo_ValueChanged(null, EventArgs.Empty);
			IncidentAction.CriticalityInfo.ValueChanged += EscalationCriticalityInfo_ValueChanged;
			AddShowSourceModuleFinderPopupEventHandlers();
			if (!string.IsNullOrEmpty(IncidentAction.PrePopulateResolutionMethod) && IncidentAction.ActiveCloseStatusDispositionList.ContainsCode(IncidentAction.PrePopulateResolutionMethod))
			{
				zDropEdit1.CodeBox.Text = IncidentAction.PrePopulateResolutionMethod;
			}
		}

		void HideModuleSelection()
		{
			ShowOrHideModuleSelectionControls(new Control[] { ProductAreaLabel, ProductAreaDropEdit, ModuleDropEdit, ModuleLabel }, false);
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
				if (!visible)
				{
					if (moduleSelectionControls.Contains(ModuleDropEdit))
					{
						IncidentAction.SectionRequirementService = "";
					}
					if (moduleSelectionControls.Contains(ProductAreaDropEdit))
					{
						IncidentAction.ProductArea = "";
					}
				}

				int controlsMoveDistance = ControlDpiScalingHelper.ScaleToCurrentDpiY(visible ? 26 : -26);
				zLabel2.Location = ControlDpiScalingHelper.NewScaledPoint(zLabel2.Location.X, zLabel2.Location.Y + controlsMoveDistance, false);
				resolutionCommentTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(resolutionCommentTextBox.Location.X, resolutionCommentTextBox.Location.Y + controlsMoveDistance, false);

				ControlDpiScalingHelper.SetHeight(resolutionCommentTextBox, resolutionCommentTextBox.Height - controlsMoveDistance, false);
				ControlDpiScalingHelper.SetHeight(this, Height + controlsMoveDistance, false);
			}
		}

		void EscalationCriticalityInfo_ValueChanged(object sender, EventArgs e)
		{
			if (IncidentAction.Criticality.IsEmpty || !IncidentAction.HasModuleListTypeChanged)
			{
				HideModuleSelection();
			}
			else
			{
				ShowModuleSelection();
			}

			ModuleLabel.Text = IncidentAction.GetSectionRequirementServiceLabelText() + ":";
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
		}

		#region Source Module Override

		void ShowSourceModuleFinderPopupIfRequired()
		{
			if (IncidentAction.ProductArea.IsEmpty)
			{
				return;
			}

			IncidentAction.ValidateSectionRequirementService();
			if (IncidentAction.SectionRequirementServiceInfo.HasErrors())
			{
				return;
			}

			var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(IncidentAction.ModuleListType);
			if (productAreaModuleMappingsRegistryItem == null)
			{
				return;
			}

			var mapping = productAreaModuleMappingsRegistryItem.Value.GetMapping(IncidentAction.Incident.IM_Product, IncidentAction.SectionRequirementService);
			if (mapping == null || mapping.SourceModuleMappings.Count == 0)
			{
				return;
			}

			var recalculatedProductArea = IncidentAction.RecalculateProductArea();
			if (recalculatedProductArea != IncidentAction.ProductArea)
			{
				ShowSourceModuleFinderPopup();
			}
		}

		void ShowSourceModuleFinderPopup()
		{
			var findReason = string.Format(@"You have selected a '{0}' that is shared by multiple Product Areas.
The current Menu Item does not belong to the currently selected Product Area ({1}), please select the correct Menu Item to determine the correct Product Area.", EnumExtensions.GetCaption(IncidentAction.ModuleListType), IncidentAction.ProductAreaDescription);

			var finder = new SourceModuleFinder(IncidentAction.Incident.IM_Product, IncidentAction.ModuleListType, findReason, IncidentAction.Factory);
			finder.ProductAreaFilter = IncidentAction.ProductArea;
			finder.ModuleFilter = IncidentAction.SectionRequirementService;

			bool isSourceModuleSelected = false;
			var form = new SourceModuleFinderForm(finder);
			form.ModuleMappingWithSourceModuleSelected += (sender, e) =>
			{
				RemoveShowSourceModuleFinderPopupEventHandlers();
				IncidentAction.MenuItem = e.ModuleMappingWithSourceModule.SourceModuleCode;
				IncidentAction.SectionRequirementService = e.ModuleMappingWithSourceModule.ModuleCode;
				IncidentAction.ProductArea = e.ModuleMappingWithSourceModule.ProductArea;
				isSourceModuleSelected = true;
				AddShowSourceModuleFinderPopupEventHandlers();
			};

			form.FormClosing += (sender, e) =>
			{
				//ShowOrHideModuleSelectionControls(new Control[] { MenuItemLabel, MenuItemPathLabel }, true);
				if (!isSourceModuleSelected)
				{
					IncidentAction.ProductArea = IncidentAction.RecalculateProductArea();
				}
			};

			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void AddShowSourceModuleFinderPopupEventHandlers()
		{
			IncidentAction.ProductAreaInfo.ValueChanged += ProductAreaInfo_ValueChanged;
			IncidentAction.SectionRequirementServiceInfo.ValueChanged += SectionRequirementServiceInfo_ValueChanged;
		}

		void RemoveShowSourceModuleFinderPopupEventHandlers()
		{
			IncidentAction.ProductAreaInfo.ValueChanged -= ProductAreaInfo_ValueChanged;
			IncidentAction.SectionRequirementServiceInfo.ValueChanged -= SectionRequirementServiceInfo_ValueChanged;
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
