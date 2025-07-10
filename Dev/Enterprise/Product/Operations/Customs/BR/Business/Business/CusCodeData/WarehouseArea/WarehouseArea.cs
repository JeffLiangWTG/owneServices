using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class WarehouseArea : CusCodeData
	{
		public WarehouseArea(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override CusCodeDataValidation GetNewValidation() => new WarehouseAreaValidation(this);

		public new WarehouseAreaValidation Validation => (WarehouseAreaValidation)base.Validation;

		public new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.WarehouseArea;
		}

		[MaxLength(10)]
		[ResourceStringData("Enterprise.Customs.BR.Business.WarehouseArea|CY_Code", Caption = "Warehouse Area ID")]
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
