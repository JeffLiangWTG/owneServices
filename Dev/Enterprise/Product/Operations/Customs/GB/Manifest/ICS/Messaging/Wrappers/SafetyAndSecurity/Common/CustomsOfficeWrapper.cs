using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class CustomsOfficeWrapper : ICustomsOffice
	{
		public CustomsOfficeWrapper(ZString referenceNumber)
		{
			this.referenceNumber = referenceNumber;
		}
		readonly ZString referenceNumber;

		public string ReferenceNumber => referenceNumber;
	}

	class FirstEntryCustomsOfficeWrapper : CustomsOfficeWrapper, IFirstEntryCustomsOffice
	{
		public FirstEntryCustomsOfficeWrapper(ZString referenceNumber, ZDateTime expectedDateAndTimeOfArrival)
			: base(referenceNumber)
		{
			this.expectedDateAndTimeOfArrival = expectedDateAndTimeOfArrival.ToString("yyyyMMddHHmm");
		}
		readonly ZString expectedDateAndTimeOfArrival;

		public string ExpectedDateAndTimeOfArrival => expectedDateAndTimeOfArrival;
	}
}
