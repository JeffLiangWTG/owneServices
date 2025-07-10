using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMComponentsUserControl : ZUserControl
	{
		public BMComponentsUserControl()
		{
			InitializeComponent();
			RemoveNewReleaseGateColumnsIfDisabledInRegistry();

			SchematicPictureBox.AllowOverlap(labelInvalidSchematicMessage);
			FilterRulesControl.AllowOutsideOfParent();
		}

		#region ZUserControl Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				BMComponentGrid.ListManager.CurrentChanged += (_, x_) => UpdateBindingForSelectedLinkChange();
				ComponentLinkGrid.ListManager.CurrentChanged += (_, x_) => UpdateBindingForSelectedLinkChange();
				UpdateBindingForSelectedLinkChange();

				if (!WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.Value)
				{
					var columnInfos = ReleaseGroupLinksGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					ReleaseGroupLinksGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationOutsideGroup));
					ReleaseGroupLinksGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationInsideGroup));
					ReleaseGroupLinksGrid.ColumnStyles.Remove(columnInfos.Single(i => i.ColumnName == BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationThrottleFactor));
				}
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (this.bmSystem != null)
			{
				this.bmSystem.HasChangesChanged -= SystemOrChildren_HasChangesChanged;
			}

			var bmSystemFromSource = BindingSource != null ? BindingSource.DataSource as BMSystem : null;

			if (bmSystemFromSource != null && !bmSystemFromSource.IsDeleted)
			{
				bmSystem = bmSystemFromSource;
				bmSystem.HasChangesChanged += SystemOrChildren_HasChangesChanged;
			}

			DrawSchematic();
		}

		BMSystem bmSystem;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (bmSystem != null)
				{
					bmSystem.HasChangesChanged -= SystemOrChildren_HasChangesChanged;
				}
			}
		}

		#endregion

		#region New Release Gate

		void RemoveNewReleaseGateColumnsIfDisabledInRegistry()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
				{
					var releaseGroupColumnInfos = ReleaseGroupLinksGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					ReleaseGroupLinksGrid.ColumnStyles.Remove(releaseGroupColumnInfos.Single(i => i.ColumnName == "FO_ReleaseGateMode"));
					ReleaseGroupLinksGrid.ColumnStyles.Remove(releaseGroupColumnInfos.Single(i => i.ColumnName == "FO_ReleaseWhenInProgress"));

					var resourceMembershipColumnInfos = ResourceMembershipGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
					ResourceMembershipGrid.ColumnStyles.Remove(resourceMembershipColumnInfos.Single(i => i.ColumnName == "FD_StaffStartableWorkflowLimit"));
				}
			}
		}

		#endregion

		#region Filter Rules

		public void UpdateBindingForSelectedLinkChange()
		{
			var link = ComponentLinkGrid.ListManager.GetCurrent() as BMComponentLink;
			var filter = link?.FilterRule;
			var showFilterDetails = filter != null;

			if (showFilterDetails)
			{
				using (filter.SuspendSettingHasChanges())
				{
					FilterRulesControl.SetDataBinding(filter, string.Empty);
				}
			}

			FilterStripsLabelWithBinding.Visible = showFilterDetails;
			FilterStripsLabelWithoutBinding.Visible = !showFilterDetails;

			FilterRulesControl.Visible = showFilterDetails;
			FilterRulesControl.Refresh();
		}

		#endregion

		#region Schematic Visualisation

		int lastDraw = -1;

		void DrawSchematic()
		{
			if (bmSystem != null && !bmSystem.IsDeleted)
			{
				if (bmSystem.Factory.CacheVersion != lastDraw)
				{
					lastDraw = bmSystem.Factory.CacheVersion;
					if (SchematicPictureBox.Image != null)
					{
						SchematicPictureBox.Image.Dispose();
					}
					try
					{
						SchematicPictureBox.Image = new BufferManagementSystemDrawer().GetSchematicImage(bmSystem, false);
						if (SchematicPictureBox.Image != null)
						{
							SchematicPictureBox.BringToFront();
						}
						else
						{
							labelInvalidSchematicMessage.Text = Res.GetString("20FBA099-20D9-4B05-AC25-6C84863676B5", "Schematic is too large to preview. Please select 'View Schematic Full-Screen' to view the schematic.");
							labelInvalidSchematicMessage.BringToFront();
						}
					}
					catch (Exception ex)
					{
						if (ex.IsCriticalException())
						{
							throw;
						}
						// goodness knows what Zubin's code will do
						SchematicPictureBox.SendToBack();
					}
				}
			}
		}

		void SystemOrChildren_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			bmSystem.TextSchematicInfo.RefreshBinding();
			UserIdleWorker.QueueWorkItem(this, new MethodInvoker(DrawSchematic));
		}

		void VisualiseSchematicButton_Click(object sender, EventArgs e)
		{
			if (bmSystem.HasChanges || !bmSystem.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("eb017b78-3a22-49fe-93db-f497f2659070", "Please save this system before attempting to visualize it."));
			}
			else
			{
				var drawMode = SchematicTabControl.SelectedTab == TextSchematicTabPage ? SchematicDrawMode.Text : SchematicDrawMode.Graphical;
				new BMSystemSchematicForm(bmSystem, drawMode).Show();
			}
		}

		#endregion

		#region Constant Strings

		internal const string ComponentLinkFilterIdentifier = "ComponentLinkFilter";

		#endregion

		#region For Test
#if DEBUG

		public ZGrid ReleaseGroupLinksGrid_ExposedForTest => ReleaseGroupLinksGrid;
		public ZGrid ResourceMembershipGrid_ExposedForTest => ResourceMembershipGrid;

		public void BindAllComponentLinksTabs()
		{
			for (int i = 0; i < ComponentLinksTabControl.TabCount; i++)
			{
				ComponentLinksTabControl.SelectTab(i);
			}
		}

#endif
		#endregion
	}
}
