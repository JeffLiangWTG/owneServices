using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PersonalItemDecQuestion : CusCodeData
	{
		public PersonalItemDecQuestion(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : CusCodeData.Schema
		{
			public new const int CY_CodeMaxLength = 1;
			public new const int CY_DataMaxLength = 1;
		}
		public override bool SupportsNotes => false;

		[MaxLength(Schema.CY_CodeMaxLength)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[MaxLength(Schema.CY_DataMaxLength)]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => new PersonalItemDecQuestionValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.PersonalItemDecQuestion;
		}
	}
}
