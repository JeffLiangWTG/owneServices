using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public partial class HVLVPreScreeningRuleControl : RegistryZUserControl
	{
		public HVLVPreScreeningRuleControl()
		{
			InitializeComponent();
			MacrosScriptText.LostFocus += new EventHandler(templateTextBox_LostFocus);

			MacrosScriptPanel.AllowOverlap(SpecialCharactersPanel);
			DeminimusPanel.AllowOverlap(SpecialCharactersPanel);
			DeminimusPanel.AllowOverlap(MacrosScriptPanel);
			HSCodePanel.AllowOverlap(SpecialCharactersPanel);
			HSCodePanel.AllowOverlap(MacrosScriptPanel);
			HSCodePanel.AllowOverlap(DeminimusPanel);
			ScreeningValuePanel.AllowOverlap(SpecialCharactersPanel);
			ScreeningValuePanel.AllowOverlap(MacrosScriptPanel);
			ScreeningValuePanel.AllowOverlap(DeminimusPanel);
			ScreeningValuePanel.AllowOverlap(HSCodePanel);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ValidationRulesGrid.ReadOnly = readOnly;
			IsEnabledCheckBox.ReadOnly = readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				ScreeningValuePanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsScreeningValuesVisible"));
				HSCodePanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsHSCodeVisible"));
				DeminimusPanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsDeminimusVisible"));
				MacrosScriptPanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsMacrosVisible"));
				SpecialCharactersPanel.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsSpecialCharactersVisible"));
				SameConsigneeCheckBox.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "Rules.Fields.IsSameConsigneeCheckBoxVisible"));
			}
		}

		void InsertMacroButton_Click(object sender, EventArgs e)
		{
			if (mapTreePresenter == null)
			{
				mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>();
				mapTreePresenter.ParentTypes = new Type[] { BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(HVLVConsignmentSchema.Constants.Prefix) };
				mapTreePresenter.MacroSelected += new MacroSelectedEventHandler(mapTreePresenter_MacroSelected);
				mapTreePresenter.ShowEditField = false;
			}
			mapTreePresenter.ShowPresentationManagerForm();
		}

		IMapTreePresentationManager mapTreePresenter;

		void mapTreePresenter_MacroSelected(string macro)
		{
			MacrosScriptText.Focus();
			MacrosScriptText.SelectionStart = lastMacrosScriptTextBoxSelectionStart;
			MacrosScriptText.SelectionLength = lastMacrosScriptTextBoxSelectionLength;
			MacrosScriptText.SelectedText = macro;
		}

		void templateTextBox_LostFocus(object sender, EventArgs e)
		{
			lastMacrosScriptTextBoxSelectionStart = MacrosScriptText.SelectionStart;
			lastMacrosScriptTextBoxSelectionLength = MacrosScriptText.SelectionLength;
		}

		protected int lastMacrosScriptTextBoxSelectionStart;
		protected int lastMacrosScriptTextBoxSelectionLength;
	}
}
