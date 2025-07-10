using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class TaxDetailProvider : IXlsxProvider
	{
		public TaxDetailProvider(TaxDetail taxDetail)
		{
			this.taxDetail = Argument.NotNull(taxDetail, nameof(taxDetail));
		}

		readonly TaxDetail taxDetail;

		[XlsxField(1, "MRN")]
		public ZString Mrn => taxDetail.Mrn;

		[XlsxField(2, "Version")]
		public ZInt Version => taxDetail.Version;

		[XlsxField(3, "1D3")]
		public ZInt _1D3 => taxDetail._1D3;

		[XlsxField(4, "1A1")]
		public ZInt _1A1 => taxDetail._1A1;

		[XlsxField(5, "1B2")]
		public ZInt _1B2 => taxDetail._1B2;

		[XlsxField(6, "A00")]
		public ZInt _A00 => taxDetail._A00;

		[XlsxField(7, "1B3")]
		public ZInt _1B3 => taxDetail._1B3;

		[XlsxField(8, "1D5")]
		public ZInt _1D5 => taxDetail._1D5;

		[XlsxField(9, "A45")]
		public ZInt _A45 => taxDetail._A45;

		[XlsxField(10, "B00")]
		public ZInt _B00 => taxDetail._B00;

		[XlsxField(11, "1D6")]
		public ZInt _1D6 => taxDetail._1D6;

		[XlsxField(12, "A35")]
		public ZInt _A35 => taxDetail._A35;

		[XlsxField(13, "B00EX")]
		public ZInt _B00EX => taxDetail._B00EX;

		[XlsxField(14, "1S1")]
		public ZInt _1S1 => taxDetail._1S1;

		[XlsxField(15, "1E1")]
		public ZInt _1E1 => taxDetail._1E1;

		[XlsxField(16, "A40")]
		public ZInt _A40 => taxDetail._A40;

		[XlsxField(17, "A30")]
		public ZInt _A30 => taxDetail._A30;

		[XlsxField(18, "1C1")]
		public ZInt _1C1 => taxDetail._1C1;

		[XlsxField(19, "2E2")]
		public ZInt _2E2 => taxDetail._2E2;

		[XlsxField(20, "A20")]
		public ZInt _A20 => taxDetail._A20;
	}
}
