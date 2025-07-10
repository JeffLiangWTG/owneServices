namespace Enterprise.Registry.GUI
{
	public partial class HouseBillOfLadingTermsAndConditionsCollectionControl : RegistryZUserControl
	{
		public HouseBillOfLadingTermsAndConditionsCollectionControl()
		{
			InitializeComponent();

			TermsAndConditionsImageSelectionControl.ImageObjectChangedByUser += (sender, e) =>
			{
				NotifyChanges();
			};
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TermsAndConditionsGrid.ReadOnly = readOnly;
			TermsAndConditionsImageSelectionControl.ReadOnly = readOnly;
		}
	}
}
