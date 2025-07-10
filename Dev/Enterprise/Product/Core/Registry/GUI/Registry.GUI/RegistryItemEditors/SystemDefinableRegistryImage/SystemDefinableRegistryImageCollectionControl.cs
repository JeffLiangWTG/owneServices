using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SystemDefinableRegistryImageCollectionControl : RegistryZUserControl
	{
		public SystemDefinableRegistryImageCollectionControl()
		{
			InitializeComponent();

			RegistryImageSelectionControl.ImageObjectChangedByUser += (sender, e) =>
			{
				NotifyChanges();
			};
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (RegistryImageGrid.ListManager != null)
			{
				RegistryImageGrid.ListManager.PositionChanged += ListManager_PositionChanged;
				SetImageControlReadOnlyGridPositionChanged();
			}
		}

		internal void ListManager_PositionChanged(object sender, EventArgs e)
		{
			SetImageControlReadOnlyGridPositionChanged();
		}

		void SetImageControlReadOnlyGridPositionChanged()
		{
			var current = RegistryImageGrid.ListManager.GetCurrent() as SystemDefinableRegistryImage;
			if (current != null)
			{
				RegistryImageSelectionControl.ReadOnly = current.SystemDefined || ReadOnly;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			RegistryImageGrid.ReadOnly = readOnly;
			RegistryImageSelectionControl.ReadOnly = readOnly;
		}
	}
}
