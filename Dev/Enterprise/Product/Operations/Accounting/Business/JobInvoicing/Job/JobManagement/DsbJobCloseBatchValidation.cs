//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDsbJobCloseBatchValidation
//
//    This class should be used for overriding validation in AutoDsbJobCloseBatchValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobCloseBatchValidation : AutoDsbJobCloseBatchValidation
	{
		public DsbJobCloseBatchValidation(AutoDsbJobCloseBatch parent) : base(parent)
		{
		}

		protected override void CheckJBB_BatchNumber()
		{
			base.CheckJBB_BatchNumber();

			MandatoryValidation.CheckEntered(Parent.JBB_BatchNumberInfo);
		}

		protected override void CheckJBB_BatchStatus()
		{
			base.CheckJBB_BatchStatus();

			MandatoryValidation.CheckEntered(Parent.JBB_BatchStatusInfo);
		}

		protected override void CheckJBB_GC()
		{
			base.CheckJBB_GC();

			MandatoryValidation.CheckEntered(Parent.JBB_GCInfo);
		}
	}
}
