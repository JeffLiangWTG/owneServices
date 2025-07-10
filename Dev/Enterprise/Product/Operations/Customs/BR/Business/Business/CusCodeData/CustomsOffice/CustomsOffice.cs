using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class CustomsOffice : CusCodeData
	{
		public CustomsOffice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override CusCodeDataValidation GetNewValidation() => new CustomsOfficeValidation(this);

		public new CustomsOfficeValidation Validation => (CustomsOfficeValidation)base.Validation;

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override bool SupportsNotes => false;

		public override bool IsSavedByFactory => isPersistent && base.IsSavedByFactory;

		public bool IsPersistent => isPersistent;
		bool isPersistent = true;

		public void MakeNonPersistent()
		{
			isPersistent = false;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CustomsOffice;
		}

		[MaxLength(7)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				base.CY_Data = value;
				Parent?.MarkAsNeedingValidation();
			}
		}

		public override void OnSaving()
		{
			if (CY_Data.IsEmpty && !CY_IsOverridden)
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

		public override ZBool CY_IsOverridden
		{
			get { return base.CY_IsOverridden; }
			set
			{
				base.CY_IsOverridden = value;
				Parent?.MarkAsNeedingValidation();
			}
		}
	}
}
