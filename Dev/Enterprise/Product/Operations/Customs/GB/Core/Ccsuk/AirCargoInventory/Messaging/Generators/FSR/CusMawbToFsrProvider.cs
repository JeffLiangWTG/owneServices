using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CusAwbToFsrProvider : IFSR
	{
		/// <summary>
		/// </summary>
		/// <param name="mawb"></param>
		/// <param name="fsrRequestType">Should be one of the enumerations of FsrRequestType</param>
		public CusAwbToFsrProvider(ICcsukCusAwb awb, string fsrRequestType)
		{
			this.awb = awb;
			this.requestType = fsrRequestType;
		}

		public ZString AirwaybillPrefixAndAirwaybillNumber
		{
			get { return awb.MasterBill.Replace("-", ""); }
		}

		public ZString HousewaybillNumber
		{
			get { return HousewaybillNumberCore; }
		}

		public ZString SplitReference
		{
			get { return SplitReferenceCore; }
		}

		public ZString ResponseRequiredIndicator
		{
			get { return requestType; }
		}

		public ZString Airport
		{
			get { return awb.CargoTerminalOperatorAirport; }
		}

		public ZString ShedOperatorIdentity
		{
			get { return awb.CargoTerminalOperator; }
		}

		public ZString RecipientID
		{
			get
			{
				return (string.IsNullOrEmpty(requestType) || requestType == FsrRequestType.Codes.FsaEnquiry)
					? string.Empty
					: GetShedPima();
			}
		}

		string GetShedPima()
		{
			return string.Format("CUKAIR98{0}{1}", awb.CargoTerminalOperatorAirport, awb.CargoTerminalOperator);
		}

		protected virtual ZString HousewaybillNumberCore
		{
			get { return ""; }
		}

		protected virtual ZString SplitReferenceCore
		{
			get { return ""; }
		}

		readonly ICcsukCusAwb awb;
		readonly string requestType;
	}
}
