using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public class HyperlinkAlertBusinessObject : NonPersistentBusinessObject, IObsoleteValidation, IHyperlinkAlertBusinessObject
	{
		public HyperlinkAlertBusinessObject(ZString messageLabel, ZString longMessageText)
		{
			this.messageLabel = messageLabel;
			this.longMessageText = longMessageText;
		}
		readonly ZString messageLabel;
		readonly ZString longMessageText;
		public ZString LongMessageText
		{
			get
			{
				return longMessageText;
			}
		}

		public ZString MessageLabel
		{
			get
			{
				return messageLabel;
			}
		}
	}
}
