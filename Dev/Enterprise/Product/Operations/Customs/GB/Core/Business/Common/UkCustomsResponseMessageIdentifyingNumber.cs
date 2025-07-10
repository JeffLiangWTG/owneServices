using CargoWise.Types;

namespace Enterprise.Customs.GB.Business
{
	public enum UkResponseMsgIdNumberType
	{
		/// <summary>
		/// DECLN-UCR or DECLN-UCR/DECLN-PART-NO
		/// </summary>
		UniqueConsignmentNumberWithPart,

		/// <summary>
		/// ENT-NO
		/// </summary>
		EntryNumber,

		/// <summary>
		/// SYS-CAR
		/// </summary>
		CommonAccessReference,

		/// <summary>
		/// Interchange number of outbound interchange
		/// </summary>
		InterchangeControlReference,

		Box7TdrOwnRefEnt,

		MasterUCR
	}

	/// <summary>
	/// Used a simple class to pass around received reference numbers within inbound message processors
	/// </summary>
	public class UkResponseMsgIdNumber
	{
		public UkResponseMsgIdNumber(ZString referenceNumber, UkResponseMsgIdNumberType type)
		{
			this.Reference = referenceNumber;
			this.TypeOfReference = type;
		}

		public ZString Reference { get; private set; }
		public UkResponseMsgIdNumberType TypeOfReference { get; private set; }
	}
}
