


namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationQueueLookupsHelper : UPECustomsQueueLookupsHelper
	{
		public UPEDeclarationQueueLookupsHelper(UPEDeclarationQueue queue)
			: base(queue)
		{
		}

		public UPEDeclarationQueueLookupsHelper(NonPersistentDeclarationQueue queue)
			: base(queue)
		{
		}

		protected override CustomsQueueCodeDescriptionPairList GetCustomsQueueList()
		{
			return new DeclarationQueueCodeDescriptionPairList();
		}
	}
}
