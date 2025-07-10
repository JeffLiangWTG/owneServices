
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public class AccHeaderFlatFileDataRow : NavisionFlatFileDataRow
	{
		public AccHeaderFlatFileDataRow() : base(AccHeaderFlatFileDataRow.NumberOfFields)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty DocumentType = new FlatFileFieldProperty(0, 11);
			public static readonly FlatFileFieldProperty SellToCustomerNumber = new FlatFileFieldProperty(1, 20);
			public static readonly FlatFileFieldProperty InvoiceNumber = new FlatFileFieldProperty(2, 20);
			public static readonly FlatFileFieldProperty BillToCustomerNumber = new FlatFileFieldProperty(3, 20);
			public static readonly FlatFileFieldProperty BillToName = new FlatFileFieldProperty(4, 30);
			public static readonly FlatFileFieldProperty BillToName2 = new FlatFileFieldProperty(5, 30);
			public static readonly FlatFileFieldProperty BillToAddress = new FlatFileFieldProperty(6, 30);
			public static readonly FlatFileFieldProperty BillToAddress2 = new FlatFileFieldProperty(7, 30);
			public static readonly FlatFileFieldProperty BillToCity = new FlatFileFieldProperty(8, 30);
			public static readonly FlatFileFieldProperty BillToContact = new FlatFileFieldProperty(9, 30);
			public static readonly FlatFileFieldProperty YourReference = new FlatFileFieldProperty(10, 30);
			public static readonly FlatFileFieldProperty OrderDate = new FlatFileFieldProperty(11, 8);
			public static readonly FlatFileFieldProperty PostingDate = new FlatFileFieldProperty(12, 8);
			public static readonly FlatFileFieldProperty ShipmentDate = new FlatFileFieldProperty(13, 8);
			public static readonly FlatFileFieldProperty PostingDescription = new FlatFileFieldProperty(14, 50);
			public static readonly FlatFileFieldProperty PaymentTermsCode = new FlatFileFieldProperty(15, 10);
			public static readonly FlatFileFieldProperty DueDate = new FlatFileFieldProperty(16, 11);
			public static readonly FlatFileFieldProperty ShipmentMethodCode = new FlatFileFieldProperty(17, 10);
			public static readonly FlatFileFieldProperty ShortCutDimension1Code = new FlatFileFieldProperty(18, 20);
			public static readonly FlatFileFieldProperty ShortCutDimension2Code = new FlatFileFieldProperty(19, 20);
			public static readonly FlatFileFieldProperty CustomerPostingGroup = new FlatFileFieldProperty(20, 10);
			public static readonly FlatFileFieldProperty Currency = new FlatFileFieldProperty(21, 10);
			public static readonly FlatFileFieldProperty CustomerPriceGroup = new FlatFileFieldProperty(22, 10);
			public static readonly FlatFileFieldProperty SellToCustomerName = new FlatFileFieldProperty(23, 30);
			public static readonly FlatFileFieldProperty SellToCustomerName2 = new FlatFileFieldProperty(24, 30);
			public static readonly FlatFileFieldProperty SellToAddress = new FlatFileFieldProperty(25, 30);
			public static readonly FlatFileFieldProperty SellToAddress2 = new FlatFileFieldProperty(26, 30);
			public static readonly FlatFileFieldProperty SellToCity = new FlatFileFieldProperty(27, 30);
			public static readonly FlatFileFieldProperty SellToContact = new FlatFileFieldProperty(28, 30);
			public static readonly FlatFileFieldProperty BillToPostCode = new FlatFileFieldProperty(29, 20);
			public static readonly FlatFileFieldProperty BillToCounty = new FlatFileFieldProperty(30, 30);
			public static readonly FlatFileFieldProperty BillToCountryCode = new FlatFileFieldProperty(31, 10);
			public static readonly FlatFileFieldProperty SellToPostCode = new FlatFileFieldProperty(32, 20);
			public static readonly FlatFileFieldProperty SellToCounty = new FlatFileFieldProperty(33, 30);
			public static readonly FlatFileFieldProperty SellToCountryCode = new FlatFileFieldProperty(34, 10);
			public static readonly FlatFileFieldProperty DocumentDate = new FlatFileFieldProperty(35, 8);
			public static readonly FlatFileFieldProperty ExternalDocumentNumber = new FlatFileFieldProperty(36, 20);
			public static readonly FlatFileFieldProperty VATBusinessPostingGroup = new FlatFileFieldProperty(37, 10);
			public static readonly FlatFileFieldProperty AdjustmentAppliesTo = new FlatFileFieldProperty(38, 20);
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

		public ZString InvoiceNumber
		{
			get { return this[Schema.InvoiceNumber.Name]; }
			set { SetField(Schema.InvoiceNumber, value, true); }
		}

		public ZString BillToCustomerNumber
		{
			get { return this[Schema.BillToCustomerNumber.Name]; }
			set { SetField(Schema.BillToCustomerNumber, value); }
		}

		public ZString BillToName
		{
			get { return this[Schema.BillToName.Name]; }
			set { SetField(Schema.BillToName, value); }
		}

		public ZString BillToName2
		{
			get { return this[Schema.BillToName2.Name]; }
			set { SetField(Schema.BillToName2, value); }
		}

		public ZString BillToAddress
		{
			get { return this[Schema.BillToAddress.Name]; }
			set { SetField(Schema.BillToAddress, value); }
		}

		public ZString BillToAddress2
		{
			get { return this[Schema.BillToAddress2.Name]; }
			set { SetField(Schema.BillToAddress2, value); }
		}

		public ZString BillToCity
		{
			get { return this[Schema.BillToCity.Name]; }
			set { SetField(Schema.BillToCity, value); }
		}

		public ZString BillToContact
		{
			get { return this[Schema.BillToContact.Name]; }
			set { SetField(Schema.BillToContact, value); }
		}

		public ZString YourReference
		{
			get { return this[Schema.YourReference.Name]; }
			set { SetField(Schema.YourReference, value); }
		}

		public ZDateTime OrderDate
		{
			get { return GetFieldAsZDateTime(Schema.OrderDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.OrderDate, value); }
		}

		public ZDateTime PostingDate
		{
			get { return GetFieldAsZDateTime(Schema.PostingDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.PostingDate, value); }
		}

		public ZDateTime ShipmentDate
		{
			get { return GetFieldAsZDateTime(Schema.ShipmentDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.ShipmentDate, value); }
		}

		public ZString PostingDescription
		{
			get { return this[Schema.PostingDescription.Name]; }
			set { SetField(Schema.PostingDescription, value); }
		}

		public ZString PaymentTermsCode
		{
			get { return this[Schema.PaymentTermsCode.Name]; }
			set { SetField(Schema.PaymentTermsCode, value); }
		}

		public ZDateTime DueDate
		{
			get { return GetFieldAsZDateTime(Schema.DueDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.DueDate, value); }
		}

		public ZString ShipmentMethodCode
		{
			get { return this[Schema.ShipmentMethodCode.Name]; }
			set { SetField(Schema.ShipmentMethodCode, value); }
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

		public ZString CustomerPostingGroup
		{
			get { return this[Schema.CustomerPostingGroup.Name]; }
			set { SetField(Schema.CustomerPostingGroup, value); }
		}

		public ZString Currency
		{
			get { return this[Schema.Currency.Name]; }
			set { SetField(Schema.Currency, value); }
		}

		public ZString CustomerPriceGroup
		{
			get { return this[Schema.CustomerPriceGroup.Name]; }
			set { SetField(Schema.CustomerPriceGroup, value); }
		}

		public ZString SellToCustomerName
		{
			get { return this[Schema.SellToCustomerName.Name]; }
			set { SetField(Schema.SellToCustomerName, value); }
		}

		public ZString SellToCustomerName2
		{
			get { return this[Schema.SellToCustomerName2.Name]; }
			set { SetField(Schema.SellToCustomerName2, value); }
		}

		public ZString SellToAddress
		{
			get { return this[Schema.SellToAddress.Name]; }
			set { SetField(Schema.SellToAddress, value); }
		}

		public ZString SellToAddress2
		{
			get { return this[Schema.SellToAddress2.Name]; }
			set { SetField(Schema.SellToAddress2, value); }
		}

		public ZString SellToCity
		{
			get { return this[Schema.SellToCity.Name]; }
			set { SetField(Schema.SellToCity, value); }
		}

		public ZString SellToContact
		{
			get { return this[Schema.SellToContact.Name]; }
			set { SetField(Schema.SellToContact, value); }
		}

		public ZString BillToPostCode
		{
			get { return this[Schema.BillToPostCode.Name]; }
			set { SetField(Schema.BillToPostCode, value); }
		}

		public ZString BillToCounty
		{
			get { return this[Schema.BillToCounty.Name]; }
			set { SetField(Schema.BillToCounty, value); }
		}

		public ZString BillToCountryCode
		{
			get { return this[Schema.BillToCountryCode.Name]; }
			set { SetField(Schema.BillToCountryCode, value); }
		}

		public ZString SellToPostCode
		{
			get { return this[Schema.SellToPostCode.Name]; }
			set { SetField(Schema.SellToPostCode, value); }
		}

		public ZString SellToCounty
		{
			get { return this[Schema.SellToCounty.Name]; }
			set { SetField(Schema.SellToCounty, value); }
		}

		public ZString SellToCountryCode
		{
			get { return this[Schema.SellToCountryCode.Name]; }
			set { SetField(Schema.SellToCountryCode, value); }
		}

		public ZDateTime DocumentDate
		{
			get { return GetFieldAsZDateTime(Schema.DocumentDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.DocumentDate, value); }
		}

		public ZString ExternalDocumentNumber
		{
			get { return this[Schema.ExternalDocumentNumber.Name]; }
			set { SetField(Schema.ExternalDocumentNumber, value); }
		}

		public ZString VATBusinessPostingGroup
		{
			//			get { return this[Schema.VATBusinessPostingGroup.Name]; }
			get { return Constants.BlankSpaceForLastColumn; }
			set { SetField(Schema.VATBusinessPostingGroup, value); }
		}

		public ZString AdjustmentAppliesTo
		{
			get { return this[Schema.AdjustmentAppliesTo.Name]; }
			set { SetField(Schema.AdjustmentAppliesTo, value); }
		}

		#endregion

		const int NumberOfFields = 39;
	}
}
