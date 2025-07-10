using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class TDTSegmentWithDatetime : Segment
	{
		public TDTSegmentWithDatetime()
		{
			MaximumNumberOfValues = 9;
		}

		public string TransportStageQualifier;
		public string ConveyanceReferenceNumber;
		public string XModeOfTransport;
		public string XTransportIdentification;
		public string XTransportMeans;
		public readonly CarrierElements Carrier = new CarrierElements();
		public string XTransportDirectionCoded;
		public readonly DateTimePeriodElements DateTimeC507 = new DateTimePeriodElements();

		protected override void GetValues()
		{
			ValueItems = new object[]
			{
				TransportStageQualifier,
				ConveyanceReferenceNumber,
				XModeOfTransport,
				XTransportIdentification,
				XTransportMeans,
				Carrier,
				XTransportDirectionCoded,
				DateTimeC507
			};
		}

		protected override void SetValues()
		{
			TransportStageQualifier = Values(1);
			ConveyanceReferenceNumber = Values(2);
			XModeOfTransport = Values(3);
			XTransportIdentification = Values(4);
			XTransportMeans = Values(5);
			Carrier.Parse(CharacterSet, Values(6));
			XTransportDirectionCoded = Values(7);
			DateTimeC507.Parse(CharacterSet, Values(8));
		}

		protected override string SegmentNameOverride
		{
			get
			{
				return SegmentFakeName;  // Can't be just TDT because when we come to parse it base tries to use the standard TDT.  Tsk tsk. 
			}
		}

		public static string SegmentFakeName = "TDT-CCSUK";
	}
}
