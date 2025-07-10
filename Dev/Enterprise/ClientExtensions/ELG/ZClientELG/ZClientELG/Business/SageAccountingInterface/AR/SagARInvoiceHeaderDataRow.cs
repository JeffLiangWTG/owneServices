using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ELG
{
	/// <summary>
	/// Invoice Header.
	/// </summary>
	internal class SagARInvoiceHeaderDataRow : SagInvoiceHeaderDataRow
	{
		public SagARInvoiceHeaderDataRow() : base() { }

		public new abstract class Schema : SagInvoiceHeaderDataRow.Schema
		{
			public static readonly FlatFileFieldProperty TransactionNumber = new FlatFileFieldProperty(6, 256);
			public static readonly FlatFileFieldProperty JobNumberReference = new FlatFileFieldProperty(7, 256);
		}

		protected override void AddFieldProperties()
		{
			base.AddFieldProperties();
			FieldProperties.Add(Schema.TransactionNumber);
			FieldProperties.Add(Schema.JobNumberReference);
		}

		/// <summary>
		/// aka TransactionReference
		/// </summary>
		public ZString TransactionNumber
		{
			get { return GetField(Schema.TransactionNumber); }
			set { SetField(Schema.TransactionNumber, value); }
		}

		/// <summary>
		/// aka SecondReference
		/// </summary>
		public ZString JobNumberReference
		{
			get { return GetField(Schema.JobNumberReference); }
			set { SetField(Schema.JobNumberReference, value); }
		}
	}
}
