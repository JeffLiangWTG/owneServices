using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AIS
{
	public sealed class ControlDetailsProvider
	{
		readonly MControlDetailsType controlDetailsObject;

		public ControlDetailsProvider(MControlDetailsType controlDetailsObject)
		{
			this.controlDetailsObject = controlDetailsObject;
		}

		public ZString SequenceNumber => controlDetailsObject.SequenceNumber;

		public ZString TypeOfDiscrepancies => controlDetailsObject.TypeOfDiscrepancies;

		public ZString AttributePointer => controlDetailsObject.AttributePointer;

		public ZString CorrectedValue => controlDetailsObject.CorrectedValue;

		public ZString Remarks => controlDetailsObject.Remarks;
	}
}
