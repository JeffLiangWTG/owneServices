namespace Enterprise.ReportWriter
{
	public class DocumentHeaderRowValidation : RowDataValidation
	{
		public DocumentHeaderRowValidation(DocumentHeaderRow parent)
			: base(parent)
		{
		}

		public new DocumentHeaderRow Parent
		{
			get { return (DocumentHeaderRow)base.Parent; }
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateIsCustomisedColumnRow();
			ValidateBackgroundColorInArgb();
		}

		public void ValidateIsCustomisedColumnRow()
		{
			ValidateCalculatedProperty(Parent.IsCustomisedColumnRowInfo);
		}

		protected void CheckIsCustomisedColumnRow()
		{
		}

		public void ValidateBackgroundColorInArgb()
		{
			ValidateCalculatedProperty(Parent.BackgroundColorInArgbInfo);
		}

		protected void CheckBackgroundColorInArgb()
		{
		}
	}
}
