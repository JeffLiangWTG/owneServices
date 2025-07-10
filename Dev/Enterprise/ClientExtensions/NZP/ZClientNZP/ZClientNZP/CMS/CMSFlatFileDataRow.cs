
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSFlatFileDataRow : FlatFileDataRow
	{
		public CMSFlatFileDataRow() : base(Constants.NumberOfFields)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty RecordType = new FlatFileFieldProperty(0, 1);
			public static readonly FlatFileFieldProperty AccountCode = new FlatFileFieldProperty(1, 10);
			public static readonly FlatFileFieldProperty WarehouseCode = new FlatFileFieldProperty(2, 4);
			public static readonly FlatFileFieldProperty TransactionCode = new FlatFileFieldProperty(3, 4);
			public static readonly FlatFileFieldProperty ProductCode = new FlatFileFieldProperty(4, 16);
			public static readonly FlatFileFieldProperty TransactionQuantity = new FlatFileFieldProperty(5, 11);
			public static readonly FlatFileFieldProperty FileGeneratedDate = new FlatFileFieldProperty(6, 11);
			public static readonly FlatFileFieldProperty DeliveryCode = new FlatFileFieldProperty(7, 3);
			public static readonly FlatFileFieldProperty ExemptNumber = new FlatFileFieldProperty(8, 8);
			public static readonly FlatFileFieldProperty TaxTotal = new FlatFileFieldProperty(9, 11);
			public static readonly FlatFileFieldProperty LineTotal = new FlatFileFieldProperty(10, 13);
			public static readonly FlatFileFieldProperty PricingIndicator = new FlatFileFieldProperty(11, 1);
			public static readonly FlatFileFieldProperty FinalPrice = new FlatFileFieldProperty(12, 11);
			public static readonly FlatFileFieldProperty Comment = new FlatFileFieldProperty(13, 30);
			public static readonly FlatFileFieldProperty CONT = new FlatFileFieldProperty(14, 10);
			public static readonly FlatFileFieldProperty RejectCode = new FlatFileFieldProperty(15, 4);
			public static readonly FlatFileFieldProperty BatchNumber = new FlatFileFieldProperty(16, 8);
			public static readonly FlatFileFieldProperty ExternalID = new FlatFileFieldProperty(17, 10);
			public static readonly FlatFileFieldProperty Intermediary = new FlatFileFieldProperty(18, 8);
			public static readonly FlatFileFieldProperty Reference = new FlatFileFieldProperty(19, 8);
			public static readonly FlatFileFieldProperty LineItem = new FlatFileFieldProperty(20, 2);
			public static readonly FlatFileFieldProperty BatchDate = new FlatFileFieldProperty(21, 11);
			public static readonly FlatFileFieldProperty TransactionDate = new FlatFileFieldProperty(22, 11);
			public static readonly FlatFileFieldProperty Item = new FlatFileFieldProperty(23, 30);
			public static readonly FlatFileFieldProperty Weight = new FlatFileFieldProperty(24, 11);
			public static readonly FlatFileFieldProperty Volume = new FlatFileFieldProperty(25, 8);
			public static readonly FlatFileFieldProperty UserReference1 = new FlatFileFieldProperty(26, 10);
			public static readonly FlatFileFieldProperty UserReference2 = new FlatFileFieldProperty(27, 24);
			public static readonly FlatFileFieldProperty Activity1 = new FlatFileFieldProperty(28, 60);
			public static readonly FlatFileFieldProperty Activity2 = new FlatFileFieldProperty(29, 60);
			public static readonly FlatFileFieldProperty ReferenceNumber = new FlatFileFieldProperty(30, 12);
			public static readonly FlatFileFieldProperty ExtensionReference = new FlatFileFieldProperty(31, 12);
			public static readonly FlatFileFieldProperty NormalPrice = new FlatFileFieldProperty(32, 11);
			public static readonly FlatFileFieldProperty Code = new FlatFileFieldProperty(33, 10);
			public static readonly FlatFileFieldProperty Value = new FlatFileFieldProperty(34, 6);
			public static readonly FlatFileFieldProperty VersionNumber = new FlatFileFieldProperty(35, 10);
		}

		#endregion

		#region Field Properties

		public ZString RecordType
		{
			get { return this[Schema.RecordType.Name]; }
			set { SetField(Schema.RecordType, value); }
		}

		public ZString AccountCode
		{
			get { return this[Schema.AccountCode.Name]; }
			set { SetField(Schema.AccountCode, value.PadLeft(Schema.AccountCode.Length)); }
		}

		public ZString WarehouseCode
		{
			get { return this[Schema.WarehouseCode.Name]; }
			set { SetField(Schema.WarehouseCode, value); }
		}

		public ZString TransactionCode
		{
			get { return this[Schema.TransactionCode.Name]; }
			set { SetField(Schema.TransactionCode, value); }
		}

		public ZString ProductCode
		{
			get { return this[Schema.ProductCode.Name]; }
			set { SetField(Schema.ProductCode, value); }
		}

		public ZString TransactionQuantity
		{
			get { return this[Schema.TransactionQuantity.Name]; }
			set { SetField(Schema.TransactionQuantity, value); }
		}

		public ZDateTime FileGeneratedDate
		{
			get { return this.GetFieldAsZDateTime(Schema.FileGeneratedDate.Name, Constants.DateFormat); }
			set { SetField(Schema.FileGeneratedDate, value); }
		}

		public ZString DeliveryCode
		{
			get { return this[Schema.DeliveryCode.Name]; }
			set { SetField(Schema.DeliveryCode, value); }
		}

		public ZString ExemptNumber
		{
			get { return this[Schema.ExemptNumber.Name]; }
			set { SetField(Schema.ExemptNumber, value); }
		}

		public ZDecimal TaxTotal
		{
			get { return this.GetFieldAsZDecimal(Schema.TaxTotal.Name, 4); }
			set { SetField(Schema.TaxTotal, value, 4); }
		}

		public ZDecimal LineTotal
		{
			get { return this.GetFieldAsZDecimal(Schema.LineTotal.Name, 4); }
			set { SetField(Schema.LineTotal, value, 4); }
		}

		public ZString PricingIndicator
		{
			get { return this[Schema.PricingIndicator.Name]; }
			set { SetField(Schema.PricingIndicator, value); }
		}

		public ZDecimal FinalPrice
		{
			get { return this.GetFieldAsZDecimal(Schema.FinalPrice.Name, 4); }
			set { SetField(Schema.FinalPrice, value, 4); }
		}

		public ZString Comment
		{
			get { return this[Schema.Comment.Name]; }
			set { SetField(Schema.Comment, value); }
		}

		public ZString CONT
		{
			get { return this[Schema.CONT.Name]; }
			set { SetField(Schema.CONT, value); }
		}

		public ZString RejectCode
		{
			get { return this[Schema.RejectCode.Name]; }
			set { SetField(Schema.RejectCode, value); }
		}

		public ZString BatchNumber
		{
			get { return this[Schema.BatchNumber.Name]; }
			set { SetField(Schema.BatchNumber, value); }
		}

		public ZString ExternalID
		{
			get { return this[Schema.ExternalID.Name]; }
			set { SetField(Schema.ExternalID, value); }
		}

		public ZString Intermediary
		{
			get { return this[Schema.Intermediary.Name]; }
			set { SetField(Schema.Intermediary, value); }
		}

		public ZString Reference
		{
			get { return this[Schema.Reference.Name]; }
			set { SetField(Schema.Reference, value); }
		}

		public ZString LineItem
		{
			get { return this[Schema.LineItem.Name]; }
			set { SetField(Schema.LineItem, value); }
		}

		public ZDateTime BatchDate
		{
			get { return this.GetFieldAsZDateTime(Schema.BatchDate.Name, Constants.DateFormat); }
			set { SetField(Schema.BatchDate, value); }
		}

		public ZDateTime TransactionDate
		{
			get { return this.GetFieldAsZDateTime(Schema.TransactionDate.Name, Constants.DateFormat); }
			set { SetField(Schema.TransactionDate, value); }
		}

		public ZString Item
		{
			get { return this[Schema.Item.Name]; }
			set { SetField(Schema.Item, value); }
		}

		public ZDecimal Weight
		{
			get { return this.GetFieldAsZDecimal(Schema.Weight.Name, 3); }
			set { SetField(Schema.Weight, value, 3); }
		}

		public ZDecimal Volume
		{
			get { return this.GetFieldAsZDecimal(Schema.Volume.Name, 2); }
			set { SetField(Schema.Volume.Name, value, 2); }
		}

		public ZString UserReference1
		{
			get { return this[Schema.UserReference1.Name]; }
			set { SetField(Schema.UserReference1, value); }
		}

		public ZString UserReference2
		{
			get { return this[Schema.UserReference2.Name]; }
			set { SetField(Schema.UserReference2, value); }
		}

		public ZString Activity1
		{
			get { return this[Schema.Activity1.Name]; }
			set { SetField(Schema.Activity1, value); }
		}

		public ZString Activity2
		{
			get { return this[Schema.Activity2.Name]; }
			set { SetField(Schema.Activity2, value); }
		}

		public ZString ReferenceNumber
		{
			get { return this[Schema.ReferenceNumber.Name]; }
			set { SetField(Schema.ReferenceNumber, value); }
		}

		public ZString ExtensionReference
		{
			get { return this[Schema.ExtensionReference.Name]; }
			set { SetField(Schema.ExtensionReference, value); }
		}

		public ZString NormalPrice
		{
			get { return this[Schema.NormalPrice.Name]; }
			set { SetField(Schema.NormalPrice, value); }
		}

		public ZString Code
		{
			get { return this[Schema.Code.Name]; }
			set { SetField(Schema.Code, value); }
		}

		public ZString Value
		{
			get { return this[Schema.Value.Name]; }
			set { SetField(Schema.Value, value); }
		}

		public ZString VersionNumber
		{
			get { return this[Schema.VersionNumber.Name]; }
			set { SetField(Schema.VersionNumber, value); }
		}

		#endregion

		#region Implementation

		void SetField(FlatFileFieldProperty fieldProperty, ZString value)
		{
			base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
		}

		void SetField(FlatFileFieldProperty fieldProperty , ZDecimal value, int decimalPlaces)
		{
			base.SetField(fieldProperty.Name, value, decimalPlaces);
		}

		void SetField(FlatFileFieldProperty fieldProperty, ZDateTime date)
		{
			base.SetField(fieldProperty.Name, date, Constants.DateFormat);
		}

		#endregion
	}
}
