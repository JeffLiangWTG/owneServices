namespace Enterprise.Customs.EU.Business
{
	public class TemporaryStorageOfficeCodeValidation : EuOfficeCodeValidation
	{
		public TemporaryStorageOfficeCodeValidation(TemporaryStorageOfficeCode parent) : base(parent)
		{
		}

		protected new TemporaryStorageOfficeCode Parent => (TemporaryStorageOfficeCode)base.Parent;
	}
}
