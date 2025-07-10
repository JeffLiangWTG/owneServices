//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmPrintQueueValidation
//
//    This class should be used for overriding validation in AutoStmPrintQueueValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintQueueValidation : AutoStmPrintQueueValidation
	{
		public StmPrintQueueValidation(AutoStmPrintQueue validatee)
			: base(validatee)
		{
			Validatee = (StmPrintQueue)validatee;
		}
		readonly StmPrintQueue Validatee;

		protected override void CheckSQ_ColumnScale()
		{
			base.CheckSQ_ColumnScale();
			if (!Validatee.SQ_ColumnScaleInfo.HasErrors())
			{
				CheckIntegerFieldRange(50, 150, Validatee.SQ_ColumnScale, Validatee.SQ_ColumnScaleInfo);
			}
		}

		protected override void CheckSQ_PrintLanguage()
		{
			base.CheckSQ_PrintLanguage();
			MandatoryValidation.CheckEntered(Validatee.SQ_PrintLanguageInfo);
			ListValidation.ErrorIfInvalidCode(Validatee.SQ_PrintLanguageInfo, Validatee.SQ_PrintLanguage_List);
		}

		protected override void CheckSQ_RowScale()
		{
			base.CheckSQ_RowScale();
			if (!Validatee.SQ_RowScaleInfo.HasErrors())
			{
				CheckIntegerFieldRange(50, 150, Validatee.SQ_RowScale, Validatee.SQ_RowScaleInfo);
			}
		}

		protected override void CheckSQ_Scale()
		{
			base.CheckSQ_Scale();
			if (!Validatee.SQ_ScaleInfo.HasErrors())
			{
				CheckIntegerFieldRange(50, 150, Validatee.SQ_Scale, Validatee.SQ_ScaleInfo);
			}
		}

		void CheckIntegerFieldRange(ZInt minimum, ZInt maximum, ZDecimal decValue, ZPropertyInfo info)
		{
			if (decValue < minimum)
			{
				info.AddError(Res.GetString("b3178f14-a006-491d-ab4b-d9ce20149c90", "Please enter a value of at least {0}", minimum.ToString()));
			}
			else if (decValue > maximum)
			{
				info.AddError(Res.GetString("595eec30-31ac-4430-8331-be35d0f03198", "Please enter a value no greater than {0}", maximum.ToString()));
			}
		}
	}
}
