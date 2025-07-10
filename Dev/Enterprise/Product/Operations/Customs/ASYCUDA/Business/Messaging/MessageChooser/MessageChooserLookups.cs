using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class MessageChooserLookups : ZLookups
	{
		public MessageChooserLookups(MessageChooser parent) : base(parent)
		{
		}

		protected new MessageChooser Parent => (MessageChooser)base.Parent;
	}
}
