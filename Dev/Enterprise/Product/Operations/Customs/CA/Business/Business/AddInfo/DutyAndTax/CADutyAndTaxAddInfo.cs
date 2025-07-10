using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CADutyAndTax)]
	public class CADutyAndTaxAddInfo : AutoCADutyAndTaxAddInfo
	{
		public CADutyAndTaxAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			Parent = addInfoProperty.BizObj;
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public override ZString C1_TaxType
		{
			get
			{
				return base.C1_TaxType;
			}
			set
			{
				var hasChanges = base.C1_TaxType != value;
				if (hasChanges && !IsCopying)
				{
					base.C1_TaxType = value;
					if (Parent is DutyAndTax tax && tax.Parent is CusClassPartPivot pivot)
					{
						pivot.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
