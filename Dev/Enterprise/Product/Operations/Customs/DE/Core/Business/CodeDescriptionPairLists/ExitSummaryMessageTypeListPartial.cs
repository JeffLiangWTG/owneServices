using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public partial class ExitSummaryMessageTypeList
	{
		public static CodeDescriptionPairList ArrivalMessageTypes
		{
			get
			{
				if (arrivalMessageTypes == null)
				{
					arrivalMessageTypes = new CodeDescriptionPairList();
					arrivalMessageTypes.AddPair(Codes.Anticipation, Descriptions.Anticipation);
					arrivalMessageTypes.AddPair(Codes.Presentation, Descriptions.Presentation);
				}
				return arrivalMessageTypes;
			}
		}
		static CodeDescriptionPairList arrivalMessageTypes;

		public static CodeDescriptionPairList DepartureMessageTypes
		{
			get
			{
				if (departureMessageTypes == null)
				{
					departureMessageTypes = new CodeDescriptionPairList();
					departureMessageTypes.AddPair(Codes.Notification, Descriptions.Notification);
				}
				return departureMessageTypes;
			}
		}
		static CodeDescriptionPairList departureMessageTypes;
	}
}
