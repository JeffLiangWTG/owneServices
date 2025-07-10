using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GroupInvoiceCharge : TypeSafeGroupInvoiceCharge, Integration.Customs.EU.IGroupInvoiceCharge, IEUCommonInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool J7_IsDutiable
		{
			get { return base.J7_IsDutiable; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsDutiable != value;
				base.J7_IsDutiable = value;
				if (isDiffAndNotCopying)
				{
					UpdateIsDutiableRelatedFieldsIfNecessary();
				}
			}
		}

		protected virtual void UpdateIsDutiableRelatedFieldsIfNecessary()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsImport)
			{
				if (J7_IsDutiable && !J7_IsApportionedCharge)
				{
					J7_IsStatisticalValueApplicable = true;
				}
			}
		}

		[ResourceStringData("EU.Business.Declaration.GroupInvoiceCharge|Export|J7_IsStatisticalValueApplicable", Caption = "Stat. Value appl.", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsStatisticalValueApplicable
		{
			get { return base.J7_IsStatisticalValueApplicable; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsStatisticalValueApplicable != value;
				base.J7_IsStatisticalValueApplicable = value;
				if (isDiffAndNotCopying)
				{
					UpdateIsStatisticalValueApplicableRelatedFieldsIfNecessary();
					MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("EU.Business.Declaration.GroupInvoiceCharge|Export|J7_Percentage", Caption = "Fixed %", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZDecimal J7_Percentage { get => base.J7_Percentage; set => base.J7_Percentage = value; }

		[ResourceStringData("EU.Business.Declaration.GroupInvoiceCharge|Export|J7_IsIncludedInITOT", Caption = "Include in Line?", MultipleKey = JobDeclaration.CaptionKeyChargesExport)]
		public override ZBool J7_IsIncludedInITOT { get => base.J7_IsIncludedInITOT; set => base.J7_IsIncludedInITOT = value; }

		protected virtual void UpdateIsStatisticalValueApplicableRelatedFieldsIfNecessary()
		{
			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsImport)
			{
				if (J7_IsStatisticalValueApplicable && !J7_IsApportionedCharge)
				{
					J7_IsGSTApplicable = true;
				}
			}
		}

		public ZDecimal AmountCorrection => ZDecimal.Zero;
	}
}
