using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileHeaderRow : CASSHOTFileDataRow
	{
		public CASSHOTFileHeaderRow()
			: this(NextSchemaFieldNumber)
		{
		}

		protected CASSHOTFileHeaderRow(int fieldCount)
			: base(fieldCount)
		{
		}

		#region Properties

		public ZDateTime HeaderDatePeriodStart
		{
			get { return GetFieldAsZDateTime(Schema.HeaderDatePeriodStart, "yyMMdd"); }
		}

		public ZDateTime HeaderDatePeriodEnd
		{
			get { return GetFieldAsZDateTime(Schema.HeaderDatePeriodEnd, "yyMMdd"); }
		}

		public ZDateTime HeaderDateOfBilling
		{
			get { return GetFieldAsZDateTime(Schema.HeaderDateOfBilling, "yyMMdd"); }
		}

		public ZString BillingCurrency
		{
			get { return GetField(Schema.BillingCurrency); }
		}

		#endregion

		protected new const int NextSchemaFieldNumber = CASSHOTFileDataRow.NextSchemaFieldNumber + 4;

		public new abstract class Schema : CASSHOTFileDataRow.Schema
		{
			public const int HeaderDatePeriodStart = CASSHOTFileDataRow.NextSchemaFieldNumber;
			public const int HeaderDatePeriodEnd = CASSHOTFileDataRow.NextSchemaFieldNumber + 1;
			public const int HeaderDateOfBilling = CASSHOTFileDataRow.NextSchemaFieldNumber + 2;
			public const int BillingCurrency = CASSHOTFileDataRow.NextSchemaFieldNumber + 3;
		}
	}
}
