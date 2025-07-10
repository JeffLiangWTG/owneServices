using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class Guarantee : IDataObject
	{
		[MaxLength(1)]
		public CodeDescriptionPair1Char BondType { get; set; }

		[MaxLength(3)]
		public CodeDescriptionPair ActivityCode { get; set; }

		[MaxLength(8)]
		public CodeDescriptionPair8Char BondFiledPort { get; set; }

		[MaxLength(35)]
		public ZString? BondNumber { get; set; }

		[MaxLength(35)]
		public ZString? BondNumber2 { get; set; }

		[MaxLength(3)]
		public ZString? SuretyCode { get; set; }

		[MaxLength(4)]
		public ZString? AccessCode { get; set; }

		public ZDecimal? BondAmount { get; set; }

		[MaxLength(35)]
		public ZString? HolderIdentification { get; set; }

		[MaxLength(3)]
		public Currency BondCurrency { get; set; }

		[MaxLength(2)]
		public Country CountryOfIssue { get; set; }

		[MaxLength(35)]
		public ZString? ValidityLimitation { get; set; }
	}
}
