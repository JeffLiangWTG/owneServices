using CargoWise.Types;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import
{
	class USOrganisationDataRow : SharedFlatFileDataRow
	{
		public USOrganisationDataRow(FlatFileDataRow row)
			: base(row)
		{
		}

		[Field(0)]
		public ZString LegacyCode { get; set; }

		[Field(1)]
		public ZString CUCode { get; set; }

		[Field(2)]
		public ZString CUName { get; set; }

		[Field(3)]
		public ZString EIN { get; set; }

		[Field(4)]
		public ZString SuretyCode { get; set; }

		[Field(5)]
		public ZString BondNumber { get; set; }

		[DecimalField(6)]
		public ZDecimal BondAmount { get; set; }

		[Field(7)]
		public ZString BondType { get; set; }

		[Field(8)]
		public ZString CustomsStatementType { get; set; }

		[Field(9)]
		public ZString POA { get; set; }

		[DecimalField(10)]
		public ZDecimal POA_Exp { get; set; }

		[Field(11)]
		public ZString ACHPayerUnit { get; set; }

		[DateTimeField(12, "MM/dd/yyyy")]
		public ZDateTime MaxImpData { get; set; }

		[Field(13)]
		public ZString LastSID { get; set; }
	}
}
