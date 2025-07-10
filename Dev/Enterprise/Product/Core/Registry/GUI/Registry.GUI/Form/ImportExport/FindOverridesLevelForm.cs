using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class FindOverridesLevelForm : ZChildForm
	{
		public FindOverridesLevelForm(bool hideInactiveFallbacks = false)
			: base(null)
		{
			InitializeComponent();
			var factory = new BusinessObjectFactory();

			var rootLevels = RegistryItemTreeViewBuilder.AddFallbackNodes(baseLevelTreeView, factory, hideInactiveFallbacks: hideInactiveFallbacks);
			RegistryItemTreeViewBuilder.AddFallbackNodes(overrideLevelTreeView, factory, hideInactiveFallbacks: hideInactiveFallbacks);

			var treeViewSearcher = new OverridesLevelSearcher(rootLevels);
			baseLevelTreeView.TreeViewSearcher = treeViewSearcher;
			overrideLevelTreeView.TreeViewSearcher = treeViewSearcher;
		}

		public Tuple<IOverrideLevel, IOverrideLevel> SelectedValues
		{
			get
			{
				return Tuple.Create(GetSelectedLevel(baseLevelTreeView), GetSelectedLevel(overrideLevelTreeView));
			}
		}

		IOverrideLevel GetSelectedLevel(TreeView tree)
		{
			var defaultTag = new DefaultOverrideLevel();
			return tree.SelectedNode == null ? defaultTag : (IOverrideLevel)tree.SelectedNode.Tag ?? defaultTag;
		}

		void findButton_Click(object sender, EventArgs e)
		{
			var validation = new NotificationCollection();
			Validate(validation);

			if (!validation.HasErrors())
			{
				FindForm().DialogResult = DialogResult.OK;
			}
			else
			{
				var sb = new StringBuilder();
				sb.AppendLine(Res.GetString("19C81BD0-BF76-498E-B893-AE6A0D9507F8", "You must correct the following errors before you can continue."));

				foreach (var error in validation.GetErrors())
				{
					sb.AppendLine(BulletType + error.Message);
				}

				Globals.Message.ShowError(sb.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Bullet item is not localizable")]
		const string BulletType = " • ";

		void Validate(NotificationCollection validation)
		{
			ValidateSelection(baseLevelTreeView, Res.GetString("95CF9DE2-779A-43C1-9DEC-91912B3294FF", "base"), validation);
			ValidateSelection(overrideLevelTreeView, Res.GetString("521E154A-9C54-4CDE-B966-F61578CF2B2A", "override"), validation);

			if (!validation.HasErrors() && baseLevelTreeView.SelectedNode.FullPath == overrideLevelTreeView.SelectedNode.FullPath)
			{
				validation.AddError(Res.GetString("B163A858-7484-4CCE-8FDD-E459B77BEF8F", "The base level is the same as the override level. Please select a different base or override level."));
			}
		}

		void ValidateSelection(TreeView tree, string humanReadableNameOfTree, INotifications validation)
		{
			if (tree.SelectedNode == null)
			{
				validation.AddError(Res.GetString("27311015-7A30-434B-9E7E-6E674ACBAB0F", "Please select a {0} level", humanReadableNameOfTree));
			}
			else if (!RegistryItemTreeViewBuilder.IsSelectable(tree.SelectedNode))
			{
				validation.AddError(Res.GetString("38CB581F-A1EE-4940-9C82-CAB6BFE7640D", "The {0} level you have selected is not valid. Please select a valid {0} level.", humanReadableNameOfTree));
			}
		}

		#region Exposed for testing
#if DEBUG
		internal ZTreeView BaseLevelTreeView { get { return baseLevelTreeView; } }
		internal ZTreeView OverrideLevelTreeView { get { return overrideLevelTreeView; } }

		internal void Validate_Exposed(NotificationCollection validation)
		{
			Validate(validation);
		}
#endif
		#endregion
	}
}
