using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DP2
{
	public class DP2FlatFileDataRow : FlatFileDataRow
	{
		public DP2FlatFileDataRow()
			: base(DP2Constants.DP2ARDataRowFieldsCount)
		{
			SetField(Schema.Code.Name, DP2Constants.TransType);
			SetField(Schema.Zero1.Name, DP2Constants.FixedZero);
			SetField(Schema.Zero2.Name, DP2Constants.FixedZero);
			SetField(Schema.One.Name, DP2Constants.FixedOne);
			SetField(Schema.A.Name, DP2Constants.FixedA);
		}

		internal class Schema
		{
			public static readonly FlatFileFieldProperty Code = new FlatFileFieldProperty(0, 2);
			public static readonly FlatFileFieldProperty ClientAccount = new FlatFileFieldProperty(1, 5);
			public static readonly FlatFileFieldProperty GLAccount = new FlatFileFieldProperty(2, 10);
			public static readonly FlatFileFieldProperty Date = new FlatFileFieldProperty(3, 8);
			public static readonly FlatFileFieldProperty Desc = new FlatFileFieldProperty(4, 30);
			public static readonly FlatFileFieldProperty Amount = new FlatFileFieldProperty(5, 10);
			public static readonly FlatFileFieldProperty Zero1 = new FlatFileFieldProperty(6, 1);
			public static readonly FlatFileFieldProperty One = new FlatFileFieldProperty(7, 1);
			public static readonly FlatFileFieldProperty Zero2 = new FlatFileFieldProperty(8, 1);
			public static readonly FlatFileFieldProperty Year = new FlatFileFieldProperty(9, 4);
			public static readonly FlatFileFieldProperty A = new FlatFileFieldProperty(10, 1);
			public static readonly FlatFileFieldProperty Invoice = new FlatFileFieldProperty(11, 10);
			public static readonly FlatFileFieldProperty TaxCode = new FlatFileFieldProperty(12, 5);
		}

		public ZString ClientAccount
		{
			set { SetField(Schema.ClientAccount.Name, value); }
		}

		public ZString GLAccount
		{
			set { SetField(Schema.GLAccount.Name, value); }
		}

		public ZDateTime Date
		{
			set { SetField(Schema.Date.Name, value, DP2Constants.DateFormat); }
		}

		public ZString Desc
		{
			set { SetField(Schema.Desc.Name, value); }
		}

		public ZDecimal Amount
		{
			set { SetField(Schema.Amount.Name, value, 0); }
		}

		public ZString TaxCode
		{
			set { SetField(DP2FlatFileDataRow.Schema.TaxCode.Name, value); }
		}

		public ZDateTime Year
		{
			set { SetField(Schema.Year.Name, value, "yyyy"); }
		}

		public ZString Invoice
		{
			set { SetField(Schema.Invoice.Name, value); }
		}
	}
}
