using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlRoot("Import5ULHeader")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Import5ULHeader : IImport5ULHeader
	{
		public string RefundDeclarationNumber { get; set; }
		public string RefundType { get; set; }
		public string RefundCauseCode { get; set; }
		public string RefundReasonCode { get; set; }
		public bool Are5FE_5ULToBeSentTogether { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public string BankAccountNumber { get; set; }
		public string BankCode { get; set; }
		public Organisation Payer { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.TotalRefundAmount)]
		public decimal TotalRefundAmount { get; set; }
		public string TaxOfficeCode { get; set; }
		public Import5ULEntryLine[] EntryLines { get; set; }

		ZString IImport5ULHeader.RefundDeclarationNumber => RefundDeclarationNumber;
		ZString IImport5ULHeader.RefundType => RefundType;
		ZString IImport5ULHeader.RefundCauseCode => RefundCauseCode;
		ZString IImport5ULHeader.RefundReasonCode => RefundReasonCode;
		ZBool IImport5ULHeader.Are5FE_5ULToBeSentTogether => Are5FE_5ULToBeSentTogether;
		ZString IImport5ULHeader.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport5ULHeader.DeclarationCustomsDivision => DeclarationCustomsDivision;
		ZString IImport5ULHeader.BankAccountNumber => BankAccountNumber;
		ZString IImport5ULHeader.BankCode => BankCode;
		IOrganization IImport5ULHeader.Payer => Payer;
		ZDecimal IImport5ULHeader.TotalRefundAmount => TotalRefundAmount;
		ZString IImport5ULHeader.TaxOfficeCode => TaxOfficeCode;
		IEnumerable<IImport5ULEntryLine> IImport5ULHeader.EntryLines => EntryLines;
	}
}
