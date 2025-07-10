
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public class AccLinesFlatFileDataRow : NavisionFlatFileDataRow
	{
		public AccLinesFlatFileDataRow() : base(AccLinesFlatFileDataRow.NumberOfFields)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty DocumentType = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty SellToCustomerNumber = new FlatFileFieldProperty(1, 20);
			public static readonly FlatFileFieldProperty DocumentNumber = new FlatFileFieldProperty(2, 20);
			public static readonly FlatFileFieldProperty LineNumber = new FlatFileFieldProperty(3, 20);
			public static readonly FlatFileFieldProperty Type = new FlatFileFieldProperty(4, 11);
			public static readonly FlatFileFieldProperty GLAccountNumber = new FlatFileFieldProperty(5, 20);
			public static readonly FlatFileFieldProperty LocationCode = new FlatFileFieldProperty(6, 10);
			public static readonly FlatFileFieldProperty PostingGroup = new FlatFileFieldProperty(7, 10);
			public static readonly FlatFileFieldProperty ShipmentDate = new FlatFileFieldProperty(8, 8);
			public static readonly FlatFileFieldProperty Description = new FlatFileFieldProperty(9, 50);
			public static readonly FlatFileFieldProperty UnitOfMeasure = new FlatFileFieldProperty(10, 30);
			public static readonly FlatFileFieldProperty Quantity = new FlatFileFieldProperty(11, 1);
			public static readonly FlatFileFieldProperty UnitPrice = new FlatFileFieldProperty(12, 20);
			public static readonly FlatFileFieldProperty UnitCost = new FlatFileFieldProperty(13, 20);
			public static readonly FlatFileFieldProperty GSTPercentage = new FlatFileFieldProperty(14, 20);
			public static readonly FlatFileFieldProperty AmountExcludingGST = new FlatFileFieldProperty(15, 12);
			public static readonly FlatFileFieldProperty AmountIncludingGST = new FlatFileFieldProperty(16, 12);
			public static readonly FlatFileFieldProperty ShortCutDimension1Code = new FlatFileFieldProperty(17, 20);
			public static readonly FlatFileFieldProperty ShortCutDimension2Code = new FlatFileFieldProperty(18, 21);
			public static readonly FlatFileFieldProperty CustomerPriceGroup = new FlatFileFieldProperty(19, 10);
			public static readonly FlatFileFieldProperty JobNumber = new FlatFileFieldProperty(20, 20);
			public static readonly FlatFileFieldProperty ReasonCode = new FlatFileFieldProperty(21, 10);
			public static readonly FlatFileFieldProperty GenBusinessPostingGroup = new FlatFileFieldProperty(22, 10);
			public static readonly FlatFileFieldProperty GenProdPostingGroup = new FlatFileFieldProperty(23, 10);
			public static readonly FlatFileFieldProperty VATBusinessPostingGroup = new FlatFileFieldProperty(24, 10);
			public static readonly FlatFileFieldProperty VATProdPostingGroup = new FlatFileFieldProperty(25, 10);
			public static readonly FlatFileFieldProperty Currency = new FlatFileFieldProperty(26, 10);
			public static readonly FlatFileFieldProperty UnitOfMeasureCode = new FlatFileFieldProperty(27, 10);
			public static readonly FlatFileFieldProperty ChargeCode = new FlatFileFieldProperty(28, 10);
		}

		#endregion

		#region Properties

		public ZString DocumentType
		{
			get { return this[Schema.DocumentType.Name]; }
			set { SetField(Schema.DocumentType, value, true); }
		}

		public ZString SellToCustomerNumber
		{
			get { return this[Schema.SellToCustomerNumber.Name]; }
			set { SetField(Schema.SellToCustomerNumber, value); }
		}

		public ZString DocumentNumber
		{
			get { return this[Schema.DocumentNumber.Name]; }
			set { SetField(Schema.DocumentNumber, value, true); }
		}

		public ZString LineNumber
		{
			get { return this[Schema.LineNumber.Name]; }
			set { SetField(Schema.LineNumber, value); }
		}

		public ZString Type
		{
			get { return this[Schema.Type.Name]; }
			set { SetField(Schema.Type, value); }
		}

		public ZString GLAccountNumber
		{
			get { return this[Schema.GLAccountNumber.Name]; }
			set { SetField(Schema.GLAccountNumber, value); }
		}

		public ZString LocationCode
		{
			get { return this[Schema.LocationCode.Name]; }
			set { SetField(Schema.LocationCode, value); }
		}

		public ZString PostingGroup
		{
			get { return this[Schema.PostingGroup.Name]; }
			set { SetField(Schema.PostingGroup, value); }
		}

		public ZDateTime ShipmentDate
		{
			get { return GetFieldAsZDateTime(Schema.ShipmentDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.ShipmentDate, value); }
		}

		public ZString Description
		{
			get { return this[Schema.Description.Name]; }
			set { SetField(Schema.Description, value); }
		}

		public ZString UnitOfMeasure
		{
			get { return this[Schema.UnitOfMeasure.Name]; }
			set { SetField(Schema.UnitOfMeasure, value); }
		}

		public ZInt Quantity
		{
			get { return GetFieldAsZInt(Schema.Quantity.Name); }
			set { SetField(Schema.Quantity, value); }
		}

		public ZDecimal UnitPrice
		{
			get { return GetFieldAsZDecimal(Schema.UnitPrice.Name, 2); }
			set { SetField(Schema.UnitPrice, value); }
		}

		public ZDecimal UnitCost
		{
			get { return GetFieldAsZDecimal(Schema.UnitCost.Name, 2); }
			set { SetField(Schema.UnitCost, value); }
		}

		public ZDecimal GSTPercentage
		{
			get { return GetFieldAsZDecimal(Schema.GSTPercentage.Name, 2); }
			set { SetField(Schema.GSTPercentage, value); }
		}

		public ZDecimal AmountExcludingGST
		{
			get { return GetFieldAsZDecimal(Schema.AmountExcludingGST.Name, 2); }
			set { SetField(Schema.AmountExcludingGST, value); }
		}

		public ZDecimal AmountIncludingGST
		{
			get { return GetFieldAsZDecimal(Schema.AmountIncludingGST.Name); }
			set { SetField(Schema.AmountIncludingGST, value); }
		}

		public ZString ShortCutDimension1Code
		{
			get { return this[Schema.ShortCutDimension1Code.Name]; }
			set { SetField(Schema.ShortCutDimension1Code, value); }
		}

		public ZString ShortCutDimension2Code
		{
			get { return this[Schema.ShortCutDimension2Code.Name]; }
			set { SetField(Schema.ShortCutDimension2Code, value); }
		}

		public ZString CustomerPriceGroup
		{
			get { return this[Schema.CustomerPriceGroup.Name]; }
			set { SetField(Schema.CustomerPriceGroup, value); }
		}

		public ZString JobNumber
		{
			get { return this[Schema.JobNumber.Name]; }
			set { SetField(Schema.JobNumber, value); }
		}

		public ZString ReasonCode
		{
			get { return this[Schema.ReasonCode.Name]; }
			set { SetField(Schema.ReasonCode, value); }
		}

		public ZString GenBusinessPostingGroup
		{
			get { return this[Schema.GenBusinessPostingGroup.Name]; }
			set { SetField(Schema.GenBusinessPostingGroup, value); }
		}

		public ZString GenProdPostingGroup
		{
			get { return this[Schema.GenProdPostingGroup.Name]; }
			set { SetField(Schema.GenProdPostingGroup, value); }
		}

		public ZString VATBusinessPostingGroup
		{
			get { return this[Schema.VATBusinessPostingGroup.Name]; }
			set { SetField(Schema.VATBusinessPostingGroup, value); }
		}

		public ZString VATProdPostingGroup
		{
			get { return this[Schema.VATProdPostingGroup.Name]; }
			set { SetField(Schema.VATProdPostingGroup, value); }
		}

		public ZString Currency
		{
			get { return this[Schema.Currency.Name]; }
			set { SetField(Schema.Currency, value); }
		}

		public ZString UnitOfMeasureCode
		{
			get { return this[Schema.UnitOfMeasureCode.Name]; }
			set { SetField(Schema.UnitOfMeasureCode, value); }
		}

		public ZString ChargeCode
		{
			get { return this[Schema.ChargeCode.Name]; }
			set { SetField(Schema.ChargeCode, value); }
		}

		#endregion

		const int NumberOfFields = 29;
	}
}
