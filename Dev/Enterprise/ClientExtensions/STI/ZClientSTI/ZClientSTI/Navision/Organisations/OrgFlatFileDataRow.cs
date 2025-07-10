
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public class OrgFlatFileDataRow : NavisionFlatFileDataRow
	{
		public OrgFlatFileDataRow() : base(OrgFlatFileDataRow.NumberOfFields)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty OrgCode = new FlatFileFieldProperty(0, 20);
			public static readonly FlatFileFieldProperty OrgName = new FlatFileFieldProperty(1, 30);
			public static readonly FlatFileFieldProperty SearchName = new FlatFileFieldProperty(2, 30);
			public static readonly FlatFileFieldProperty Name2 = new FlatFileFieldProperty(3, 30);
			public static readonly FlatFileFieldProperty Address = new FlatFileFieldProperty(4, 30);
			public static readonly FlatFileFieldProperty Address2 = new FlatFileFieldProperty(5, 30);
			public static readonly FlatFileFieldProperty City = new FlatFileFieldProperty(6, 30);
			public static readonly FlatFileFieldProperty Contact = new FlatFileFieldProperty(7, 30);
			public static readonly FlatFileFieldProperty PhoneNumber = new FlatFileFieldProperty(8, 30);
			public static readonly FlatFileFieldProperty GlobalDimension1Code = new FlatFileFieldProperty(9, 20);
			public static readonly FlatFileFieldProperty GlobalDimension2Code = new FlatFileFieldProperty(10, 20);
			public static readonly FlatFileFieldProperty CreditLimit = new FlatFileFieldProperty(11, 50);
			public static readonly FlatFileFieldProperty CustomerPostingGroup = new FlatFileFieldProperty(12, 10);
			public static readonly FlatFileFieldProperty CustomerPriceGroup = new FlatFileFieldProperty(13, 10);
			public static readonly FlatFileFieldProperty PaymentTermsCode = new FlatFileFieldProperty(14, 10);
			public static readonly FlatFileFieldProperty SalesPersonCode = new FlatFileFieldProperty(15, 10);
			public static readonly FlatFileFieldProperty ShipmentMethodCode = new FlatFileFieldProperty(16, 10);
			public static readonly FlatFileFieldProperty InvoiceDisc = new FlatFileFieldProperty(17, 20);
			public static readonly FlatFileFieldProperty CountryCode = new FlatFileFieldProperty(18, 10);
			public static readonly FlatFileFieldProperty Blocked = new FlatFileFieldProperty(19, 3);
			public static readonly FlatFileFieldProperty PrintStatements = new FlatFileFieldProperty(20, 3);
			public static readonly FlatFileFieldProperty BillToCustomerNumber = new FlatFileFieldProperty(21, 20);
			public static readonly FlatFileFieldProperty ApplicationMethod = new FlatFileFieldProperty(22, 15);
			public static readonly FlatFileFieldProperty FaxNumber = new FlatFileFieldProperty(23, 30);
			public static readonly FlatFileFieldProperty GenBusinessPostingGroup = new FlatFileFieldProperty(24, 10);
			public static readonly FlatFileFieldProperty PostCode = new FlatFileFieldProperty(25, 20);
			public static readonly FlatFileFieldProperty County = new FlatFileFieldProperty(26, 30);
			public static readonly FlatFileFieldProperty Email = new FlatFileFieldProperty(27, 80);
			public static readonly FlatFileFieldProperty HomePage = new FlatFileFieldProperty(28, 80);
			public static readonly FlatFileFieldProperty GSTBusinessPostingGroup = new FlatFileFieldProperty(29, 10);
			public static readonly FlatFileFieldProperty ResponsibilityCentre = new FlatFileFieldProperty(30, 10);
			public static readonly FlatFileFieldProperty ABN = new FlatFileFieldProperty(31, 30);
			public static readonly FlatFileFieldProperty ABNDivisionPartNumber = new FlatFileFieldProperty(32, 3);
		}

		#endregion

		#region Properties

		public ZString OrgCode
		{
			get { return this[Schema.OrgCode.Name]; }
			set { this.SetField(Schema.OrgCode, value, true); }
		}

		public ZString OrgName
		{
			get { return this[Schema.OrgName.Name]; }
			set { this.SetField(Schema.OrgName, value); }
		}

		public ZString SearchName
		{
			get { return this[Schema.SearchName.Name]; }
			set { this.SetField(Schema.SearchName, value); }
		}

		public ZString Name2
		{
			get { return this[Schema.Name2.Name]; }
			set { this.SetField(Schema.Name2, value); }
		}

		public ZString Address
		{
			get { return this[Schema.Address.Name]; }
			set { this.SetField(Schema.Address, value); }
		}

		public ZString Address2
		{
			get { return this[Schema.Address2.Name]; }
			set { this.SetField(Schema.Address2, value); }
		}

		public ZString City
		{
			get { return this[Schema.City.Name]; }
			set { this.SetField(Schema.City, value); }
		}

		public ZString Contact
		{
			get { return this[Schema.Contact.Name]; }
			set { this.SetField(Schema.Contact, value); }
		}

		public ZString PhoneNumber
		{
			get { return this[Schema.PhoneNumber.Name]; }
			set { this.SetField(Schema.PhoneNumber, value); }
		}

		public ZString GlobalDimension1Code
		{
			get { return this[Schema.GlobalDimension1Code.Name]; }
			set { this.SetField(Schema.GlobalDimension1Code, value); }
		}

		public ZString GlobalDimension2Code
		{
			get { return this[Schema.GlobalDimension2Code.Name]; }
			set { this.SetField(Schema.GlobalDimension2Code, value); }
		}

		public ZDecimal CreditLimit
		{
			get { return GetFieldAsZDecimal(Schema.CreditLimit.Name, 2); }
			set { this.SetField(Schema.CreditLimit, value); }
		}

		public ZString CustomerPostingGroup
		{
			get { return this[Schema.CustomerPostingGroup.Name]; }
			set { this.SetField(Schema.CustomerPostingGroup, value); }
		}

		public ZString CustomerPriceGroup
		{
			get { return this[Schema.CustomerPriceGroup.Name]; }
			set { this.SetField(Schema.CustomerPriceGroup, value); }
		}

		public ZString PaymentTermsCode
		{
			get { return this[Schema.PaymentTermsCode.Name]; }
			set { this.SetField(Schema.PaymentTermsCode, value); }
		}

		public ZString SalesPersonCode
		{
			get { return this[Schema.SalesPersonCode.Name]; }
			set { this.SetField(Schema.SalesPersonCode, value); }
		}

		public ZString ShipmentMethodCode
		{
			get { return this[Schema.ShipmentMethodCode.Name]; }
			set { this.SetField(Schema.ShipmentMethodCode, value); }
		}

		public ZString InvoiceDisc
		{
			get { return this[Schema.InvoiceDisc.Name]; }
			set { this.SetField(Schema.InvoiceDisc, value); }
		}

		public ZString CountryCode
		{
			get { return this[Schema.CountryCode.Name]; }
			set { this.SetField(Schema.CountryCode, value); }
		}

		public ZString Blocked
		{
			get { return this[Schema.Blocked.Name]; }
			set { this.SetField(Schema.Blocked, value); }
		}

		public ZString PrintStatements
		{
			get { return this[Schema.PrintStatements.Name]; }
			set { this.SetField(Schema.PrintStatements, value); }
		}

		public ZString BillToCustomerNumber
		{
			get { return this[Schema.BillToCustomerNumber.Name]; }
			set { this.SetField(Schema.BillToCustomerNumber, value); }
		}

		public ZString ApplicationMethod
		{
			get { return this[Schema.ApplicationMethod.Name]; }
			set { this.SetField(Schema.ApplicationMethod, value); }
		}

		public ZString FaxNumber
		{
			get { return this[Schema.FaxNumber.Name]; }
			set { this.SetField(Schema.FaxNumber, value); }
		}

		public ZString GenBusinessPostingGroup
		{
			get { return this[Schema.GenBusinessPostingGroup.Name]; }
			set { this.SetField(Schema.GenBusinessPostingGroup, value); }
		}

		public ZString PostCode
		{
			get { return this[Schema.PostCode.Name]; }
			set { this.SetField(Schema.PostCode, value); }
		}

		public ZString County
		{
			get { return this[Schema.County.Name]; }
			set { this.SetField(Schema.County, value); }
		}

		public ZString Email
		{
			get { return this[Schema.Email.Name]; }
			set { this.SetField(Schema.Email, value); }
		}

		public ZString HomePage
		{
			get { return this[Schema.HomePage.Name]; }
			set { this.SetField(Schema.HomePage, value); }
		}

		public ZString GSTBusinessPostingGroup
		{
			get { return this[Schema.GSTBusinessPostingGroup.Name]; }
			set { this.SetField(Schema.GSTBusinessPostingGroup, value); }
		}

		public ZString ResposibilityCentre
		{
			get { return this[Schema.ResponsibilityCentre.Name]; }
			set { this.SetField(Schema.ResponsibilityCentre, value); }
		}

		public ZString ABN
		{
			get { return this[Schema.ABN.Name]; }
			set { this.SetField(Schema.ABN, value); }
		}

		public ZString ABNDivisionPartNumber
		{
			//			get { return this[Schema.ABNDivisionPartNumber.Name]; }
			get { return Constants.BlankSpaceForLastColumn; }
			set { this.SetField(Schema.ABNDivisionPartNumber, value); }
		}

		#endregion

		const int NumberOfFields = 33;
	}
}
