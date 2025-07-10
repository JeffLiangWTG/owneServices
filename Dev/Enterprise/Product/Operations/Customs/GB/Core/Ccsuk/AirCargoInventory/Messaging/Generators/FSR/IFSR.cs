using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public interface IFSR
	{
		/// <summary>
		/// AN3, M
		/// </summary>
		ZString AirwaybillPrefixAndAirwaybillNumber { get; }

		/// <summary>
		/// AN8	C	If any
		/// </summary>
		ZString HousewaybillNumber { get; }

		/// <summary>
		/// N2	C	If waybill has been split for entry
		/// </summary>
		ZString SplitReference { get; }

		/// <summary>
		/// A3	C	FSA if consignment details are required. FSN if transmission of an FSN is required. If absent - FSA assumed
		/// </summary>
		ZString ResponseRequiredIndicator { get; }

		/// <summary>
		/// A3†	C	Used to identify airport where shed is located.
		/// </summary>
		ZString Airport { get; }

		/// <summary>
		/// A3	C	Used to identify Shed for which details are required (where a consignment may exist in more than one shed)
		/// </summary>
		ZString ShedOperatorIdentity { get; }

		/// <summary>
		/// AN..21	C	Used when response type is FSN to give the CCS address to which the FSN should be sent
		/// </summary>
		ZString RecipientID { get; }
	}
}
