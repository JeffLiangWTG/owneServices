using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CustomsEnclosure : CusCodeData
	{
		public CustomsEnclosure(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override CusCodeDataValidation GetNewValidation() => new CustomsEnclosureValidation(this);

		public new CustomsEnclosureValidation Validation => (CustomsEnclosureValidation)base.Validation;

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CustomsEnclosure;
		}

		[MaxLength(7)]
		public override ZString CY_Data { get => base.CY_Data; set => base.CY_Data = value; }

		public override void OnSaving()
		{
			if (CY_Data.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				base.CY_Code = value;
				Parent?.MarkAsNeedingValidation();
			}
		}
	}
}
