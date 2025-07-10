//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccConsolidationMemberValidation
//
//    This class should be used for overriding validation in AutoAccConsolidationMemberValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;

	public class AccConsolidationMemberValidation : AutoAccConsolidationMemberValidation
	{
		public AccConsolidationMemberValidation(AutoAccConsolidationMember parent) : base(parent)
		{
		}

		protected override void CheckYM_OH_Organisation()
		{
			base.CheckYM_OH_Organisation();

			if (Parent.Company != null && Parent.Organisation != null)
			{
				Parent.YM_OH_OrganisationInfo.AddError(Res.GetString("17a3be76-9928-4722-9702-1ab328dee9dc", "select only one out of organization and company"));
			}
			else if (Parent.Company == null && Parent.Organisation == null)
			{
				Parent.YM_OH_OrganisationInfo.AddError(Res.GetString("3a8f72d0-5ed0-4e0f-b6f6-e84c231d2033", "please select an organization or company"));
			}

			if (Parent.Organisation != null)
			{
				var query = new ZQuery(AccConsolidationMemberSchema.YM_OH_Organisation, Parent.YM_OH_Organisation);
				query.AddToFilter(AccConsolidationMemberSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var duplicates = Parent.Factory.Load<AccConsolidationMember>(query);

				if (duplicates.Length > 0)
				{
					var otherGroups = string.Join(", ", duplicates.Select(x => x.ConsolidationGroup != null ? x.ConsolidationGroup.YR_Code : ZString.Empty).ToArray());
					Parent.YM_OH_OrganisationInfo.AddWarning(Res.GetString("3aa01c17-d772-4a38-91c5-374090f359ef", "This organization is also part of these groups: {0}.", otherGroups));
				}
			}
		}

		protected override void CheckYM_GC_Company()
		{
			base.CheckYM_GC_Company();

			if (Parent.Company != null && Parent.Organisation != null)
			{
				Parent.YM_GC_CompanyInfo.AddError(Res.GetString("17a3be76-9928-4722-9702-1ab328dee9dc", "select only one out of organization and company"));
			}
			else if (Parent.Company == null && Parent.Organisation == null)
			{
				Parent.YM_GC_CompanyInfo.AddError(Res.GetString("3a8f72d0-5ed0-4e0f-b6f6-e84c231d2033", "please select an organization or company"));
			}

			if (Parent.Company != null)
			{
				var query = new ZQuery(AccConsolidationMemberSchema.YM_GC_Company, Parent.YM_GC_Company);
				query.AddToFilter(AccConsolidationMemberSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var duplicates = Parent.Factory.Load<AccConsolidationMember>(query);

				if (duplicates.Length > 0)
				{
					var otherGroup = duplicates[0].ConsolidationGroup.YR_Code;
					Parent.YM_GC_CompanyInfo.AddError(Res.GetString("99f1613e-7a65-4bbf-8393-396f63f9932f", "A company can only belong to one group. This company is already part of the '{0}' group.", otherGroup));
				}
			}
		}

		protected new AccConsolidationMember Parent
		{
			get { return (AccConsolidationMember)base.Parent; }
		}
	}
}