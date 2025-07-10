using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ProductConditionCollection : Customs.Business.CusCodeDataCollection<ProductCondition>
	{
		public ProductConditionCollection(QuarantineExDocLine quarantineExDocLine)
			: base(quarantineExDocLine, CusCodeDataTypeList.Codes.EXDOCProductCondition)
		{
			MaxCountValidationEnable(10);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var productCondition = (ProductCondition)child;
			productCondition.CY_Order = new ZShort(NextOrderNumber);
		}

		short NextOrderNumber => Select(x => x.CY_Order).OrderByDescending(x => x).FirstOrDefault() + 1;
	}
}
