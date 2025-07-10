using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	/// <summary>
	/// Invoice Header.
	/// </summary>
	internal class SagAPInvoiceHeaderDataRow : SagInvoiceHeaderDataRow
	{
		public SagAPInvoiceHeaderDataRow() : base() { }

		public new abstract class Schema : SagInvoiceHeaderDataRow.Schema
		{
			public static readonly FlatFileFieldProperty InvoiceDescription = new FlatFileFieldProperty(6, 256);
			public static readonly FlatFileFieldProperty TransactionNumber = new FlatFileFieldProperty(7, 256);
		}

		protected override void AddFieldProperties()
		{
			base.AddFieldProperties();
			FieldProperties.Add(Schema.InvoiceDescription);
			FieldProperties.Add(Schema.TransactionNumber);
		}

		/// <summary>
		/// aka SecondReference
		/// </summary>
		public ZString InvoiceDescription
		{
			get { return GetField(Schema.InvoiceDescription); }
			set { SetField(Schema.InvoiceDescription, value); }
		}

		/// <summary>
		/// aka TransactionReference
		/// </summary>
		public ZString TransactionNumber
		{
			get { return GetField(Schema.TransactionNumber); }
			set { SetField(Schema.TransactionNumber, value); }
		}
	}
}
