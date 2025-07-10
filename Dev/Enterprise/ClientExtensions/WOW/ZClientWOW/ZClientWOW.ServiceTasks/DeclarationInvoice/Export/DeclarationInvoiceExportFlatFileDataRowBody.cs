using CargoWise.Types;
using Enterprise.ClientSharedComponents;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export
{
	class DeclarationInvoiceExportFlatFileDataRowBody : FixedWidthFlatFileDataRow
	{
		public DeclarationInvoiceExportFlatFileDataRowBody()
			: base(Constants.BodyRecord.FieldsCount)
		{
		}

		public DeclarationInvoiceExportFlatFileDataRowBody(ZString lineData)
			: base(lineData)
		{
		}

		[IntField(Constants.BodyRecord.IndicateBodyRecord, Constants.BodyRecord.IndicateBodyRecordMaxLength, AlignTypes.Right, '0')]
		public ZInt IndicateBodyRecord { get; set; }

		[Field(Constants.BodyRecord.JobNumber, Constants.BodyRecord.JobNumberMaxLength, AlignTypes.Left, ' ')]
		public ZString JobNumber { get; set; }

		[Field(Constants.BodyRecord.InvoiceNumberForLine, Constants.BodyRecord.InvoiceNumberForLineMaxLength, AlignTypes.Left, ' ')]
		public ZString InvoiceNumberForLine { get; set; }

		[Field(Constants.BodyRecord.IndentPONumber, Constants.BodyRecord.IndentPONumberMaxLength, AlignTypes.Left, ' ')]
		public ZString IndentPONumber { get; set; }

		[Field(Constants.BodyRecord.MaterialProductCode, Constants.BodyRecord.MaterialProductCodeMaxLength, AlignTypes.Left, ' ')]
		public ZString MaterialProductCode { get; set; }

		[DecimalField(Constants.BodyRecord.UnitPrice, Constants.BodyRecord.UnitPriceMaxLength, 3, AlignTypes.Right, '0')]
		public ZDecimal UnitPrice { get; set; }

		[IntField(Constants.BodyRecord.Quantity, Constants.BodyRecord.QuantityMaxLength, AlignTypes.Right, '0')]
		public ZInt Quantity { get; set; }
	}
}
