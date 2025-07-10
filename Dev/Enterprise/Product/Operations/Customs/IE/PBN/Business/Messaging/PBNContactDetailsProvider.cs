using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging
{
	public class PBNContactDetailsProvider
	{
		public PBNContactDetailsProvider(PBNContactDetails jsonObject)
		{
			contactDetailsMessageObject = jsonObject;
		}

		public ZString Email => contactDetailsMessageObject.Email ?? ZString.Empty;

		public ZString MobileNum1 => contactDetailsMessageObject.MobileNum1 ?? ZString.Empty;

		public ZString MobileNum2 => contactDetailsMessageObject.MobileNum2 ?? ZString.Empty;

		readonly PBNContactDetails contactDetailsMessageObject;
	}
}
