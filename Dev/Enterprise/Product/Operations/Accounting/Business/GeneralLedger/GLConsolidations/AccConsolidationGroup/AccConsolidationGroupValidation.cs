//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccConsolidationGroupValidation
//
//    This class should be used for overriding validation in AutoAccConsolidationGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	using System.Collections.Generic;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;

	public class AccConsolidationGroupValidation : AutoAccConsolidationGroupValidation
	{
		public AccConsolidationGroupValidation(AutoAccConsolidationGroup parent)
			: base(parent)
		{
		}

		protected override void CheckYR_Code()
		{
			base.CheckYR_Code();
			MandatoryValidation.CheckEntered(Parent.YR_CodeInfo);

			if (!Parent.YR_CodeInfo.HasErrors())
			{
				var query = new ZQuery(AccConsolidationGroupSchema.YR_Code, Parent.YR_Code);
				query.AddToFilter(AccConsolidationGroupSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.LoadTop1<AccConsolidationGroup>(query) != null)
				{
					Parent.YR_CodeInfo.AddError(Res.GetString("72a4da0e-b6fa-4606-875b-0fbd3c0eb169", "Consolidation Group Code must be unique."));
				}
			}
		}

		protected override void CheckYR_Description()
		{
			base.CheckYR_Description();
			MandatoryValidation.CheckEntered(Parent.YR_DescriptionInfo);
		}

		protected override void CheckYR_YR_ConsolidationGroup()
		{
			base.CheckYR_YR_ConsolidationGroup();

			if (Parent.YR_YR_ConsolidationGroup.IsValid && !Parent.YR_YR_ConsolidationGroup.IsEmpty)
			{
				if (Parent.YR_YR_ConsolidationGroup == Parent.PK)
				{
					Parent.YR_YR_ConsolidationGroupInfo.AddError(Res.GetString("4a5fe4af-3af0-4e4c-8ba1-ccbfd149a79b", "A group cannot be its own parent."));
				}

				if (!Parent.YR_YR_ConsolidationGroupInfo.HasErrors())
				{
					var processedGroups = new List<ZString>();
					processedGroups.Add(Parent.YR_Code);
					var child = Parent;
					bool error = false;

					while (child.ParentGroup != null)
					{
						if (processedGroups.Contains(child.ParentGroup.YR_Code))
						{
							error = true;
							processedGroups.Add(child.ParentGroup.YR_Code);
							break;
						}
						else
						{
							processedGroups.Add(child.ParentGroup.YR_Code);
							child = child.ParentGroup;
						}
					}

					if (error)
					{
						Parent.YR_YR_ConsolidationGroupInfo.AddError(Res.GetString("7198a524-9fe1-475c-80b5-a7c733f09be3", "A group cannot be its own parent. This group is its own parent as a result of other groups: {0}", string.Join(" -> ", processedGroups)));
					}
				}
			}
		}

		protected new AccConsolidationGroup Parent
		{
			get { return (AccConsolidationGroup)base.Parent; }
		}
	}
}