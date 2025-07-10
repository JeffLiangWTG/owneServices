using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLine : NonPersistentBusinessObject
	{
		public CATCPLine() : base(null)
		{
		}

		public ZString RecordIdentifier { get; set; }
		public ZString BusinessNumber { get; set; }
		public ZString TCPTypeCode { get; set; }
		public ZString TCPIdentifier { get; set; }
		public ZString AddressLine1 { get; set; }
		public ZString AddressLine2 { get; set; }
		public ZString City { get; set; }
		public ZString ProvinceStateCode { get; set; }
		public ZString CountryCode { get; set; }
		public ZString PostalZipCode { get; set; }
		public ZString BusinessName { get; set; }
	}
}
