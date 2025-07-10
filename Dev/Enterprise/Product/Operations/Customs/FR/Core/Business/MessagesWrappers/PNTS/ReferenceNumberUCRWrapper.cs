namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class ReferenceNumberUCRWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IReferenceNumberUCR
	{
		ReferenceNumberUCRWrapper(string billAblUCRNumber)
		{
			this.billAblUCRNumber = billAblUCRNumber;
		}
		readonly string billAblUCRNumber;

		public string ReferenceNumberUCRProperty => referenceNumberUCRProperty ?? (referenceNumberUCRProperty = billAblUCRNumber);
		string referenceNumberUCRProperty;

		public static ReferenceNumberUCRWrapper New(string billAblUCRNumber) => billAblUCRNumber == null ? null : new ReferenceNumberUCRWrapper(billAblUCRNumber);
	}
}
