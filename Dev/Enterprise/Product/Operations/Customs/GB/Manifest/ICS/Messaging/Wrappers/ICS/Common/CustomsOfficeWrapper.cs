using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	internal class CustomsOfficeWrapper : ICustomsOffice
	{
		protected ZString referenceNumber;

		public CustomsOfficeWrapper(ZString referenceNumber)
		{
			this.referenceNumber = referenceNumber;
		}

		ZString ICustomsOffice.ReferenceNumber => referenceNumber;
	}

	internal class FirstEntryCustomsOfficeWrapper : CustomsOfficeWrapper, IFirstEntryCustomsOffice
	{
		readonly ZDateTime expectedDateAndTimeOfArrival;

		public FirstEntryCustomsOfficeWrapper(ZString referenceNumber, ZDateTime expectedDateAndTimeOfArrival) : base(referenceNumber)
		{
			this.expectedDateAndTimeOfArrival = expectedDateAndTimeOfArrival;
		}

		ZDateTime IFirstEntryCustomsOffice.ExpectedDateAndTimeOfArrival => expectedDateAndTimeOfArrival;
	}
}
