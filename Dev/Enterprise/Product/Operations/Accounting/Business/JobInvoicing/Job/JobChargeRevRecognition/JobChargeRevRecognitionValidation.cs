//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobChargeRevRecognitionValidation
//
//    This class should be used for overriding validation in AutoJobChargeRevRecognitionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
namespace Enterprise.Accounting.Business.JobInvoicing
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class JobChargeRevRecognitionValidation : AutoJobChargeRevRecognitionValidation
	{
		public JobChargeRevRecognitionValidation(AutoJobChargeRevRecognition parent) : base(parent)
		{
		}

		protected override void CheckD3_RecognitionDate()
		{
			base.CheckD3_RecognitionDate();
			if (Parent.Job != null)
			{
				JobValidation jobValidation = Parent.Job.Validation as JobValidation;
				if (jobValidation != null)
				{
					ZString error = jobValidation.GetRevenueRecognitionDateNotInGLPeriodError(Parent.D3_RecognitionDate);
					if (error != "")
					{
						Parent.D3_RecognitionDateInfo.AddError(error);
					}
				}
			}
		}

		protected override void CheckD3_RecognitionDateIsValidZDateTime()
		{
			if (Parent.D3_RecognitionDate != AccountingConstants.RevenueRecognitionDateConstants.Immediate)
			{
				base.CheckD3_RecognitionDateIsValidZDateTime();
			}
		}

		protected override void CheckD3_RecognitionDateIsValidZDateTimeRange()
		{
			if (Parent.D3_RecognitionDate != AccountingConstants.RevenueRecognitionDateConstants.Immediate)
			{
				base.CheckD3_RecognitionDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckD3_RecognitionType()
		{
			base.CheckD3_RecognitionType();
			ZQuery dublicationsFilter = new ZQuery(JobChargeRevRecognitionSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			dublicationsFilter.AddToFilter(JobChargeRevRecognitionSchema.D3_JH, Parent.D3_JH);
			dublicationsFilter.AddToFilter(JobChargeRevRecognitionSchema.D3_RecognitionType, Parent.D3_RecognitionType);
			if (Parent.Factory.LoadTop1<JobChargeRevRecognition>(dublicationsFilter) != null)
			{
				Parent.D3_RecognitionTypeInfo.AddError(Res.GetString("3c142688-4118-4991-852a-2fd1f93dbe86", "Only one Recognition Date can be added for the job with Recognition Type '{0}'", Parent.D3_RecognitionType));
			}
		}
	}
}
