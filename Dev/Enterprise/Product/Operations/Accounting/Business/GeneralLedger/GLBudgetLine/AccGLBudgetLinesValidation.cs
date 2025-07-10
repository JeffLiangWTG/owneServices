//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLBudgetLinesValidation
//
//    This class should be used for overriding validation in AutoAccGLBudgetLinesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public class AccGLBudgetLinesValidation : AutoAccGLBudgetLinesValidation
	{
		public AccGLBudgetLinesValidation(AutoAccGLBudgetLines parent)
			: base(parent)
		{
		}
	}

	public class GLBudgetLinesValidation : AccGLBudgetLinesValidation
	{
		public GLBudgetLinesValidation(GLBudgetLine parent)
			: base(parent)
		{
		}

		#region Calculated Properties Validation

		#region ValidateUnsignedLineAmount

		public void ValidateUnsignedLineAmount()
		{
			ValidateCalculatedProperty(ParentLine.UnsignedAmountInfo);
		}

		protected virtual void CheckUnsignedLineAmount()
		{
			MandatoryValidation.CheckEntered(ParentLine.UnsignedAmountInfo);
		}

		#endregion

		#region ValidateDebitCreditSign

		public void ValidateDebitCreditSign()
		{
			ValidateCalculatedProperty(ParentLine.DebitCreditSignInfo);
		}

		protected virtual void CheckDebitCreditSign()
		{
			MandatoryValidation.CheckEntered(ParentLine.DebitCreditSignInfo);
			ListValidation.ErrorIfInvalidCode(ParentLine.DebitCreditSignInfo, ParentLine.DebitCreditTypes);
		}

		#endregion

		#endregion

		GLBudgetLine ParentLine
		{
			get { return Parent as GLBudgetLine; }
		}
	}
}


