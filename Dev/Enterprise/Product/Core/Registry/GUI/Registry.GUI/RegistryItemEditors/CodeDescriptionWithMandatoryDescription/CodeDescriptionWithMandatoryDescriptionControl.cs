namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithMandatoryDescriptionControl : RegistryZUserControl
	{
		public CodeDescriptionWithMandatoryDescriptionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeDescriptionWithMandatoryDescriptionGrid.ReadOnly = readOnly;
		}

		#region Column Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string DescriptionColumnName_Translatable = "Description";

		#endregion
	}
}
