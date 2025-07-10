using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.AWB
{
	[XsdSchema(Placement.Outer)]
	public class AWBParty : IDataObject
	{
		[MaxLength(14)]
		public ZString? AccountCode { get; set; }
		[MaxLength(50)]
		public ZString? Name { get; set; }
		[MaxLength(50)]
		public ZString? AddressLine1 { get; set; }
		[MaxLength(50)]
		public ZString? AddressLine2 { get; set; }
		[MaxLength(17)]
		public ZString? City { get; set; }
		[MaxLength(9)]
		public ZString? State { get; set; }
		[MaxLength(9)]
		public ZString? PostCode { get; set; }
		public Country Country { get; set; }
		public CodeDescriptionPair ContactType { get; set; }
		[MaxLength(25)]
		public ZString? ContactDetail { get; set; }
		[MaxLength(35)]
		public ZString? ContactName { get; set; }
		[MaxLength(50)]
		public ZString? CompanyIDCode { get; set; }
		[MaxLength(35)]
		public ZString? CompanyID { get; set; }
		public ZBool? IsAddressOverriddenForPaperWaybill { get; set; }
		[MaxLength(75)]
		public ZString? PaperOverrideLine1 { get; set; }
		[MaxLength(75)]
		public ZString? PaperOverrideLine2 { get; set; }
		[MaxLength(75)]
		public ZString? PaperOverrideLine3 { get; set; }
		[MaxLength(75)]
		public ZString? PaperOverrideLine4 { get; set; }
		[MaxLength(75)]
		public ZString? PaperOverrideLine5 { get; set; }
	}
}
