using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class TreatmentActiveIngredient : CusCodeData
	{
		public TreatmentActiveIngredient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public QuarantineExDocEstablishmentAndTime QuarantineExDocEstablishmentAndTime => (QuarantineExDocEstablishmentAndTime)Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(QuarantineExDocEstablishmentAndTime));

		protected override CusCodeDataLookups GetNewLookups() => new TreatmentActiveIngredientLookups(this);

		protected override CusCodeDataValidation GetNewValidation() => new TreatmentActiveIngredientValidation(this);

		public new TreatmentActiveIngredientValidation Validation => (TreatmentActiveIngredientValidation)base.Validation;

		[MaxLength(4)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.EXDOCTreatmentActiveIngredient;
		}
	}
}
