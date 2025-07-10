//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientCognosGroupingFlagsValidation
//
//    This class should be used for overriding validation in AutoClientCognosGroupingFlagsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class ClientCognosGroupingFlagsValidation : AutoClientCognosGroupingFlagsValidation
	{
		public ClientCognosGroupingFlagsValidation(AutoClientCognosGroupingFlags parent)
			: base(parent)
		{
		}

		protected override void CheckT4_Branch()
		{
			base.CheckT4_Branch();
			CompareValidation.CheckWithinRange(Parent.T4_BranchInfo, 0, 4);
			EnsureNoDuplicateGroupingOrder(Parent.T4_BranchInfo);
		}

		protected override void CheckT4_BusinessType()
		{
			base.CheckT4_BusinessType();
			CompareValidation.CheckWithinRange(Parent.T4_BusinessTypeInfo, 0, 4);
			EnsureNoDuplicateGroupingOrder(Parent.T4_BusinessTypeInfo);
		}

		protected override void CheckT4_Geographical()
		{
			base.CheckT4_Geographical();
			CompareValidation.CheckWithinRange(Parent.T4_GeographicalInfo, 0, 4);
			EnsureNoDuplicateGroupingOrder(Parent.T4_GeographicalInfo);
		}

		protected override void CheckT4_Mode()
		{
			base.CheckT4_Mode();
			CompareValidation.CheckWithinRange(Parent.T4_ModeInfo, 0, 4);
			EnsureNoDuplicateGroupingOrder(Parent.T4_ModeInfo);
		}

		protected override void CheckT4_Company()
		{
			base.CheckT4_Company();
			MandatoryValidation.CheckEntered(Parent.T4_CompanyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.T4_CompanyInfo, Parent.Lookups.IntercompanyCodeList);
		}

		#region Implementation

		void EnsureNoDuplicateGroupingOrder(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && IsGroupingOrderUsedByOtherField(info))
			{
				string errorMessage = "This grouping order has already been specified for another field.";
				info.AddError(errorMessage);
			}
		}

		bool IsGroupingOrderUsedByOtherField(ZPropertyInfo info)
		{
			return (Parent.T4_BranchInfo != info && (ZByte)Parent.T4_BranchInfo.Value == (ZByte)info.Value) ||
					(Parent.T4_ModeInfo != info && (ZByte)Parent.T4_ModeInfo.Value == (ZByte)info.Value) ||
					(Parent.T4_BusinessTypeInfo != info && (ZByte)Parent.T4_BusinessTypeInfo.Value == (ZByte)info.Value) ||
					(Parent.T4_GeographicalInfo != info && (ZByte)Parent.T4_GeographicalInfo.Value == (ZByte)info.Value);
		}

		#endregion
	}
}
