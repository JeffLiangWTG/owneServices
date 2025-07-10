using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CusAttributeFilter : Customs.Business.CusAttributeFilter
	{
		public CusAttributeFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

		public new OrgSupplierPart Part => (OrgSupplierPart)base.Part;

		public override ZString BG_AttributeValue1
		{
			get => base.BG_AttributeValue1;
			set
			{
				var oldValue = BG_AttributeValue1;
				base.BG_AttributeValue1 = value;
				if (!IsCopying && oldValue != BG_AttributeValue1)
				{
					var part = Part;
					if (part != null)
					{
						part.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Canada).ForEach(p => p.MarkAsNeedingValidation());
					}
				}
			}
		}

		public new CusAttributeFilterValidation Validation => (CusAttributeFilterValidation)base.Validation;

		protected override Customs.Business.CusAttributeFilterValidation GetNewValidation() => new CusAttributeFilterValidation(this);

		public override void Delete()
		{
			var pivot = Pivot;
			base.Delete();
			if (pivot != null)
			{
				pivot.MarkAsNeedingValidation();
			}
		}
	}
}
