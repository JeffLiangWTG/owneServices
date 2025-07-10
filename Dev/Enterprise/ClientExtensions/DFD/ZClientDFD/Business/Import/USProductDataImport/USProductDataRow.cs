using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import
{
	class USProductDataRow : SharedFlatFileDataRow
	{
		public USProductDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		[Field(0)]
		public ZString Customer { get; set; }

		[Field(1)]
		public ZString ProductCode { get; set; }

		[Field(2)]
		public ZString ProductDesc1 { get; set; }

		[Field(3)]
		public ZString ProductDesc2 { get; set; }

		[Field(4)]
		public ZString HTCode { get; set; }

		[Field(5)]
		public ZString CustomerCode_Ignore { get; set; }

		[Field(6)]
		public ZString ProductCode_Ignore { get; set; }

		[Field(7)]
		public ZString FDAProductCode { get; set; }

		[Field(8)]
		public ZString CargoStorageStatus { get; set; }

		[Field(9)]
		public ZString CountryOfProduction { get; set; }

		[Field(10)]
		public ZString AffirmatnOfCompliance { get; set; }

		[Field(11)]
		public ZString AffrmtnOfCmplnceQlfr { get; set; }

		[Field(12)]
		public ZString ActualManufacturerID { get; set; }

		[Field(13)]
		public ZString ActualSupplierShipper { get; set; }

		[Field(14)]
		public ZString FDAConsigneeCode { get; set; }

		[Field(15)]
		public ZString BrandName { get; set; }

		[Field(16)]
		public ZString ContactName_Ignore { get; set; }

		[Field(17)]
		public ZString TelephoneNumber_Ignore { get; set; }

		[Field(18)]
		public ZString AffirmatnOfComplian2 { get; set; }
	}
}
