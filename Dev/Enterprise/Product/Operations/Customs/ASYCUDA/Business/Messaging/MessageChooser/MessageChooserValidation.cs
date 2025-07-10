namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageChooserValidation : AutoMessageChooserValidation
	{
		public MessageChooserValidation(AutoMessageChooser parent)
			: base(parent)
		{
		}
		protected new MessageChooser Parent => (MessageChooser)base.Parent;
	}
}
