using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationCode : CusCodeData
	{
		public ValuationDeclarationCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public const int CY_Data_MaxLength = 50;
		}
		public override bool SupportsNotes => false;

		[MaxLength(Schema.CY_Data_MaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[List(nameof(Lookups) + "." + nameof(ValuationDeclarationCodeLookups.PriceDeclarationItemCodeList))]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}
		public override bool CY_DataAllowWesternEuropeanCharactersOnly => false;
		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobComInvoiceHeader));
		public new ValuationDeclarationCodeLookups Lookups => (ValuationDeclarationCodeLookups)base.Lookups;

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new ValuationDeclarationCodeValidation(this);
		protected override CusCodeDataLookups GetNewLookups() => new ValuationDeclarationCodeLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ValuationDeclarationCode;
		}
	}
}
