using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ProductCondition : CusCodeData
	{
		public ProductCondition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.EXDOCProductCondition;
		}

		[MaxLength(4)]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		public new QuarantineExDocLine Parent
		{
			get { return (QuarantineExDocLine)base.Parent; }
			set { base.Parent = value; }
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(QuarantineExDocLine));

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ProductConditionValidation(this);
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ProductConditionLookups(this);
		}
	}
}
