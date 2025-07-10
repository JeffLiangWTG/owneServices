namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageChooserItemValidation : AutoMessageChooserItemValidation
	{
		public MessageChooserItemValidation(AutoMessageChooserItem parent) : base(parent)
		{
		}

		protected new MessageChooserItem Parent => (MessageChooserItem)base.Parent;
	}
}
