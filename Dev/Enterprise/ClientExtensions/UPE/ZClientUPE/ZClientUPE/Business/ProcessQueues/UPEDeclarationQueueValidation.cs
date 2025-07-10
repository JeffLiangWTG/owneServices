namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationQueueValidation : UPECustomsProcessQueueValidation
	{
		public UPEDeclarationQueueValidation(UPEDeclarationQueue parent)
			: base(parent)
		{
		}

		protected override UPECustomsQueueValidationHelper GetNewCustomsQueueValidationHelper()
		{
			return new UPEDeclarationQueueValidationHelper(Parent);
		}

		protected new UPEDeclarationQueue Parent
		{
			get { return (UPEDeclarationQueue)base.Parent; }
		}
	}
}
