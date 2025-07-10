using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class CustomsSupportingInformation : IDataObject
	{
		[Mandatory]
		public CodeDescriptionPair Category { get; set; }
		public CodeDescriptionPair6Char Type { get; set; }
		public Country Country { get; set; }
		[MaxLength(100)]
		public ZString? ReferenceNumber { get; set; }
		public List<Reference> ReferenceNumberCollection { get; set; }
		public CodeDescriptionPair5Char SubType { get; set; }
		public CodeDescriptionPair Status { get; set; }
		[MaxLength(35)]
		public ZString? Tariff { get; set; }
		public ZDateTime? DateOfIssue { get; set; }
		[MaxLength(300), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		public ZDecimal? Quantity { get; set; }
		public ZDecimal? Quantity2 { get; set; }
		public ZDecimal? Quantity3 { get; set; }
		public CodeDescriptionPair4Char UnitOfQuantity { get; set; }
		public CodeDescriptionPair4Char UnitOfQuantity2 { get; set; }
		public CodeDescriptionPair4Char UnitOfQuantity3 { get; set; }
		public CodeDescriptionPair10Char CustomsOffice { get; set; }
		public CodeDescriptionPair7Char Procedure { get; set; }
		public ZInt? LineNo { get; set; }
		public ZDateTime? DateOfExpiry { get; set; }
		public Currency ValueCurrency { get; set; }
		public ZDecimal? Value { get; set; }
		[MaxLength(70)]
		public ZString? IssuingAuthority { get; set; }
		public ZInt? PackQuantity { get; set; }
		public CodeDescriptionPair PackUnitOfQuantity { get; set; }
		public ZInt? ItemNumber { get; set; }
		[MaxLength(300), AllowLineControlWhiteSpace]
		public ZString? AdditionalDescription { get; set; }
		public CodeDescriptionPair10Char IssuerType { get; set; }
		public List<AddInfo> AddInfoCollection { get; set; }
	}
}
