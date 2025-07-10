using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[DependentBusinessObject(typeof(CusSeaManOBLHeaderCargoLine), "Details")]
	public class CusSeaManOBLDetailCargoLine : BaseCusSeaManOBLDetail
	{
		public CusSeaManOBLDetailCargoLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusSeaManOBLHeaderCargoLine Header
		{
			get
			{
				return Factory.Load<CusSeaManOBLHeaderCargoLine>(BD_BO);
			}
		}

		public new CusSeaManOBLDetailCargoLineValidation Validation
		{
			get { return (CusSeaManOBLDetailCargoLineValidation)base.Validation; }
		}

		protected override Customs.Business.CusSeaManOBLDetailValidation GetNewValidation()
		{
			return new CusSeaManOBLDetailCargoLineValidation(this);
		}

		public override ZString BD_LineCargoType
		{
			get { return base.BD_LineCargoType; }
			set
			{
				bool hasChanged = base.BD_LineCargoType != value;
				base.BD_LineCargoType = value;
				if (hasChanged && Header != null && !IsCopying)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}
	}
}
