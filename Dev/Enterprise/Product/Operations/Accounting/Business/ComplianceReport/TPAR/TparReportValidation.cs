namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	public class TparReportValidation : AccTaxReturnValidation
	{
		public TparReportValidation(TparReport parent) : base(parent)
		{
		}

		new TparReport Parent => (TparReport)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateSenderContactPhone();
		}

		void ValidateSenderContactPhone()
		{
			var errorMessage = Res.GetString("0c57ad21-686b-4453-b929-3c41e9f9a04b", "Missing Sender Contact Phone. Add a work phone number in your contact numbers or organization proxy phone on the details tab.");
			Parent.RemoveRowError(errorMessage);

			if (!Parent.HasErrors && string.IsNullOrEmpty(Parent.SenderContactPhone))
			{
				Parent.AddRowError(errorMessage);
			}
		}
	}
}
