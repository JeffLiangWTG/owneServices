using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.UniversalCopy;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI
{
	partial class EntityNodeDetailsUserControl : ZUserControl
	{
		protected EntityNodeDetailsUserControl()
		{
			InitializeComponent();
		}

		public EntityNodeDetailsUserControl(UniversalCopyManager copyManager)
		{
			InitializeComponent();
			this.copyManager = copyManager;

			gridProperties.CurrentCellChanged += gridProperties_CurrentCellChanged;
		}

		public GetPropertyListModuleIdDelegate GetPropertyListModuleIdMethod
		{
			get => zMultiControlColumnStyleInfo1.GetPropertyListModuleIdMethod;
			set => zMultiControlColumnStyleInfo1.GetPropertyListModuleIdMethod = value;
		}

		EntityCopyTemplateBizo EntityCopyBizo
		{
			get { return (EntityCopyTemplateBizo)BindingSource.Current; }
		}

		readonly UniversalCopyManager copyManager;

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				UnHookDataSource();
			}

			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				HookDataSource();
			}
		}

		protected virtual void HookDataSource()
		{
			SetPropertiesMacroRootTypes();
			InitializeFilterControl();
		}

		protected virtual void UnHookDataSource()
		{
			SaveFilterLayout();
			ClearFilterControls();
			ReloadFilterStrip();
		}

		#endregion

		#region Filter Control

		protected void InitializeFilterControl()
		{
			if (panelFilter.Controls.Count > 0)
			{
				return; // There is already some filter control here
			}

			var entityCopyBizo = EntityCopyBizo;
			var filterBizo = entityCopyBizo != null && entityCopyBizo.EntityFilter != null && entityCopyBizo.EntityFilter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter ? null : GetFilterBizo(entityCopyBizo);
			if (entityCopyBizo != null && filterBizo != null)
			{
				var filterControl = new UniversalCopyFilterStripControl(filterBizo) { Dock = DockStyle.Fill, AutoScroll = true };
				panelFilter.Controls.Add(filterControl);

				if (entityCopyBizo.EntityFilter != null && copyManager != null)
				{
					copyManager.LoadFilterLayout(filterBizo, entityCopyBizo.EntityFilter);
					using (filterControl.FilterBusinessObject.SuspendSettingHasChangesIncludingChildren())
					{
						filterControl.ForceRebuildFilterStrips();
					}
				}
			}

			tabPageCollectionFilter.TabVisible = filterBizo != null;
		}

		void ReloadFilterStrip()
		{
			var entityCopyBizo = EntityCopyBizo;
			if (entityCopyBizo == null || entityCopyBizo.EntityFilter == null || entityCopyBizo.EntityFilter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter)
			{
				return;
			}

			var filterBizo = entityCopyBizo.FilterStripBizo as FilterStripBusinessObject;
			if (filterBizo == null)
			{
				return;
			}

			copyManager?.LoadFilterLayout(filterBizo, entityCopyBizo.EntityFilter);
		}

		FilterStripBusinessObject GetFilterBizo(EntityCopyTemplateBizo entityCopyBizo)
		{
			return GetFilterStripBusinessObjectMethod != null ? GetFilterStripBusinessObjectMethod(entityCopyBizo) : null;
		}

		public delegate FilterStripBusinessObject GetFilterStripBusinessObjectDelegate(object dataSource);
		public GetFilterStripBusinessObjectDelegate GetFilterStripBusinessObjectMethod;

		internal void SaveFilterLayout()
		{
			if (panelFilter.Controls.Count != 0 && copyManager != null)
			{
				var entityCopyBizo = EntityCopyBizo;
				if (entityCopyBizo != null && entityCopyBizo.FilterStripBizo != null &&
					(entityCopyBizo.EntityFilter == null || entityCopyBizo.EntityFilter.FilterTypeId != EntityFilterTypeIds.MandatoryExpressionFilter))
				{
					entityCopyBizo.EntityFilter = copyManager.SaveFilterLayout((FilterStripBusinessObject)entityCopyBizo.FilterStripBizo, OrderBy);
				}
			}
		}

		protected virtual string OrderBy
		{
			get { return ""; }
		}

		protected void ClearFilterControls()
		{
			while (panelFilter.Controls.Count > 0)
			{
				panelFilter.Controls[0].Dispose();
			}
			tabPageCollectionFilter.TabVisible = false;
		}

		#endregion

		#region Properties grid

		protected void HidePropertiesDetails()
		{
			gridProperties.Visible = false;
			tabPagePropertiesDetails.TabVisible = false;

			if (noPropertiesLabel == null)
			{
				noPropertiesLabel = new ZLabel
				{
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter,
					IsFontBold = true,
					Text = Res.GetString("22b1f9cf-1e97-4325-8c81-5706e3ad1abd", "Properties configuration is not available with selected copy mode.")
				};
				groupBoxProperties.Controls.Add(noPropertiesLabel);
			}
			noPropertiesLabel.Visible = true;
		}

		protected void ShowPropertiesDetails()
		{
			if (noPropertiesLabel != null)
			{
				noPropertiesLabel.Visible = false;
			}

			gridProperties.Visible = true;
			tabPagePropertiesDetails.TabVisible = true;
		}

		ZLabel noPropertiesLabel;

		internal void SelectPropertyRow(PropertyCopyTemplateBizo propertyBizo)
		{
			if (gridProperties.List != null && gridProperties.List.Contains(propertyBizo))
			{
				gridProperties.SelectSingleElement(propertyBizo);
			}
		}

		void gridProperties_CurrentCellChanged(object sender, EventArgs e)
		{
			if (gridProperties.ListManager != null && gridProperties.ListManager.Position >= 0)
			{
				var propertyBizo = gridProperties.ListManager.GetCurrent() as PropertyCopyTemplateBizo;
				if (propertyBizo != null)
				{
					OnSelectedPropertyChanged(new SelectedPropertyChangedEventArgs { PropertyBizo = propertyBizo });
				}
			}
		}

		void OnSelectedPropertyChanged(SelectedPropertyChangedEventArgs e)
		{
			if (SelectedPropertyChanged != null)
			{
				SelectedPropertyChanged(this, e);
			}
		}

		public class SelectedPropertyChangedEventArgs : EventArgs
		{
			public PropertyCopyTemplateBizo PropertyBizo { get; set; }
		}

		public event EventHandler<SelectedPropertyChangedEventArgs> SelectedPropertyChanged;

		#endregion

		#region Implementation

		void SetPropertiesMacroRootTypes()
		{
			if (GetMacroRootTypesMethod != null && EntityCopyBizo != null)
			{
				Type[] rootTypes = GetMacroRootTypesMethod(EntityCopyBizo);
				if (rootTypes != null && rootTypes.Length > 0)
				{
					foreach (PropertyCopyTemplateBizo property in EntityCopyBizo.PropertyNodes)
					{
						property.RootTypes = rootTypes;
					}
				}
			}
		}

		public delegate Type[] GetMacroRootTypesDelegate(object dataSource);
		public GetMacroRootTypesDelegate GetMacroRootTypesMethod;

		#endregion
	}
}
