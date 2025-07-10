using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[DependentBusinessObject(typeof(AccApportionmentTemplate), "Lines")]
	public class AccApportionmentTemplateLines : AutoAccApportionmentTemplateLines
	{
		public AccApportionmentTemplateLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Company

		[List("Companies")]
		public ZGuid Company
		{
			get
			{
				GlbBranch branch = Factory.Load<GlbBranch>(Y0_GB);
				return branch == null ? ZGuid.Empty : branch.GB_GC;
			}
		}

		#endregion

		#region Companies

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					fCompanies = new GlbCompanyCollection(Factory);
				}
				return fCompanies;
			}
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#region Y0_Percentage_DecimalPlaces

		public ZByte Y0_Percentage_DecimalPlaces
		{
			get
			{
				return AccApportionmentTemplateLinesSchema.Y0_Percentage.Scale;
			}
		}

		#endregion

		#region Y0_Percentage

		[DecimalPlaces(3)]
		public override ZDecimal Y0_Percentage
		{
			get
			{
				return base.Y0_Percentage;
			}
			set
			{
				base.Y0_Percentage = value;
				AccApportionmentTemplate template = Factory.Load<AccApportionmentTemplate>(Y0_A0);
				if (template != null)
				{
					template.PercentageTotalInfo.RefreshBinding();
					AccApportionmentTemplateValidation validation = template.Validation;
					if (validation != null)
					{
						validation.ValidatePercentageTotal();
					}
				}
			}
		}

		#endregion

		public override ZGuid Y0_GB
		{
			get
			{
				return base.Y0_GB;
			}
			set
			{
				base.Y0_GB = value;

				if (!IsValidationSuspended && GlbBranchCombinationValidation.ShouldValidateCombination(Branch))
				{
					Validation.ValidateY0_GE();
				}
			}
		}

		#endregion

	}
}
