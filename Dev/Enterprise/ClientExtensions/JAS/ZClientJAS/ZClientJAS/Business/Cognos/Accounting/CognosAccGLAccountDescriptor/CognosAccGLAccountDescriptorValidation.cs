
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccGLAccountDescriptorValidation : AccGLAccountDescriptorValidation
	{
		public CognosAccGLAccountDescriptorValidation(CognosAccGLAccountDescriptor parent)
			: base(parent)
		{
		}

		public override AccGLAccountDescriptorValidationHelper ValidationHelper
		{
			get { return new CognosAccGLAccountDescriptorValidationHelper(Parent, Parent.AJ_Language, Parent.AJ_RN_NKCountryOfCompliance, Parent.Factory); }
		}

		public new CognosAccGLAccountDescriptor Parent
		{
			get { return (CognosAccGLAccountDescriptor)base.Parent; }
		}

		protected override void CheckAJ_AJ_ConsolidationNum()
		{
			base.CheckAJ_AJ_ConsolidationNum();

			if (Parent.RequiresCognosConsolidationAccount)
			{
				if (Parent.AJ_AJ_ConsolidationNum.IsEmpty)
				{
					Parent.AJ_AJ_ConsolidationNumInfo.AddError("Consolidation Account has to be specified for every Cognos sub-account (Local account number containing dot \".\")");
				}
				else if (Parent.ConsolidationNum != null && !Parent.AJ_LocalAccountNumber.StartsWith(Parent.ConsolidationNum.AJ_LocalAccountNumber + "."))
				{
					string errorMessage = string.Format("Cannot attach to this Consolidation account. Consolidation Account has to have the Local account number '{0}'", Parent.AJ_LocalAccountNumber.Split('.')[0]);
					Parent.AJ_AJ_ConsolidationNumInfo.AddError(errorMessage);
				}
			}
		}
	}
}
