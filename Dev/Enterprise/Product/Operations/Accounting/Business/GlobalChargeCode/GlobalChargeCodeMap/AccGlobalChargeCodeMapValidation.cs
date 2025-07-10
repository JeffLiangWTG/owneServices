//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGlobalChargeCodeMapValidation
//
//    This class should be used for overriding validation in AutoAccGlobalChargeCodeMapValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class AccGlobalChargeCodeMapValidation : AutoAccGlobalChargeCodeMapValidation
	{
		public AccGlobalChargeCodeMapValidation(AutoAccGlobalChargeCodeMap parent) : base(parent)
		{
		}

		#region Properties

		protected override void CheckYG_Code()
		{
			base.CheckYG_Code();
			MandatoryValidation.CheckEntered(Parent.YG_CodeInfo);
			if (!IsCodeUnique())
			{
				Parent.YG_CodeInfo.AddError(Res.GetString("B66925C3-F03E-4ea2-A23C-1D7FF7314266", "Code must be unique."));
			}
		}

		protected override void CheckYG_Desc()
		{
			base.CheckYG_Desc();
			MandatoryValidation.CheckEntered(Parent.YG_DescInfo);
		}

		#endregion

		#region Implementation

		bool IsCodeUnique()
		{
			ZQuery filter = new ZQuery(AccGlobalChargeCodeMapSchema.YG_Code, Parent.YG_Code);
			if (Parent.YG_OH.IsEmpty)
			{
				filter.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, null);
			}
			else
			{
				filter.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, Parent.YG_OH);
			}
			filter.AddToFilter(AccGlobalChargeCodeMapSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			return !Parent.Factory.ExistsInDatabase(AccGlobalChargeCodeMapSchema.Constants.TableName, filter);
		}

		#endregion
	}
}

