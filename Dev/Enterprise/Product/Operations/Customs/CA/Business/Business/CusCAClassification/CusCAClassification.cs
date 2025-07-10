using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAClassification : AutoCusCAClassification
	{
		public CusCAClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid CCA_ParentID
		{
			get => base.CCA_ParentID;
			set
			{
				var oldValue = CCA_ParentID;
				base.CCA_ParentID = value;
				if (!IsCopying && oldValue != value)
				{
					var parent = Parent;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		public BusinessObject Parent
		{
			get
			{
				if (fParent == null && !CCA_ParentTableCode.IsEmpty && !CCA_ParentID.IsEmpty)
				{
					fParent = Factory.Load(CCA_ParentTableCode, CCA_ParentID);
				}
				return fParent;
			}
		}
		BusinessObject fParent;

		public CusClassPartPivot Pivot
		{
			get
			{
				if (Parent is CusClassPartPivot pivot)
				{
					return pivot;
				}
				return null;
			}
		}

		public CusClassification Classification
		{
			get
			{
				if (Parent is CusClassification classification)
				{
					return classification;
				}
				return null;
			}
		}

		public override ZString CCA_RN_NKOrigin
		{
			get => base.CCA_RN_NKOrigin;
			set
			{
				var oldValue = CCA_RN_NKOrigin;
				base.CCA_RN_NKOrigin = value;
				if (!IsCopying && oldValue != value)
				{
					var parent = Parent;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CCA_SIMADumpingNumber
		{
			get => base.CCA_SIMADumpingNumber;
			set
			{
				var oldValue = CCA_SIMADumpingNumber;
				base.CCA_SIMADumpingNumber = value;
				if (!IsCopying && oldValue != value)
				{
					var parent = Parent;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString CCA_ParentTableCode
		{
			get => base.CCA_ParentTableCode;
			set
			{
				var oldValue = CCA_ParentTableCode;
				base.CCA_ParentTableCode = value;
				if (!IsCopying && oldValue != value)
				{
					var parent = Parent;
					if (parent != null)
					{
						parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override CusCAClassificationValidation GetNewValidation()
		{
			return (Pivot?.IsImport ?? false) ? new ImportCusCAClassificationValidation(this) : base.GetNewValidation();
		}
	}
}
