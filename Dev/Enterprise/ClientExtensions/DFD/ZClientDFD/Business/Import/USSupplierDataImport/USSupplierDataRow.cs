using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import
{
	class USSupplierDataRow : SharedFlatFileDataRow
	{
		public USSupplierDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		[Field(0)]
		public ZString MFName { get; set; }

		[Field(1)]
		public ZString ManufacturerID { get; set; }

		[Field(2)]
		public ZString MFADDR { get; set; }

		[Field(3)]
		public ZString MFCITY { get; set; }

		[Field(4)]
		public ZString MFZIP { get; set; }

		[Field(5)]
		public ZString MFCTCD { get; set; }

		[DateTimeField(6, "MM/dd/yyyy")]
		public ZDateTime LastEntryDate { get; set; }
	}
}
