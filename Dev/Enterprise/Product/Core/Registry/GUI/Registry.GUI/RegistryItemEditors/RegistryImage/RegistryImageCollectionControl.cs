namespace Enterprise.Registry.GUI
{
	public partial class RegistryImageCollectionControl : RegistryZUserControl
	{
		public RegistryImageCollectionControl()
		{
			InitializeComponent();

			RegistryImageSelectionControl.ImageObjectChangedByUser += (sender, e) =>
			{
				NotifyChanges();
			};
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			RegistryImageGrid.ReadOnly = readOnly;
			RegistryImageSelectionControl.ReadOnly = readOnly;
		}
	}
}
