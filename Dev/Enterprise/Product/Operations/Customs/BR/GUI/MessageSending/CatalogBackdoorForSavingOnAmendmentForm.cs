namespace Enterprise.Customs.BR.GUI
{
	public partial class CatalogBackdoorForSavingOnAmendmentForm : Customs.GUI.BackdoorForSavingOnAmendmentForm
	{
		public CatalogBackdoorForSavingOnAmendmentForm(Business.CatalogDeferredAmendmentSavingOptions savingOptions)
			: base(savingOptions)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
