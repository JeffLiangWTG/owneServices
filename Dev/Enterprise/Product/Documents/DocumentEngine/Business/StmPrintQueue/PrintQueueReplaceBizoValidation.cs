using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class PrintQueueReplaceBizoValidation : ZValidation
	{
		readonly PrintQueueReplaceBizo Parent;

		public PrintQueueReplaceBizoValidation(PrintQueueReplaceBizo parent) : base(parent)
		{
			Parent = parent;
			this.ZValidationInternals = this;
		}

		#region Property Validation

		public void ValidateReplacePrintQueuePK()
		{
			ZValidationInternals.Validate(Parent.ReplacePrintQueuePKInfo, ReplacePrintQueuePKValidationInvoker());
		}

		readonly IValidationInternals ZValidationInternals;

		RunValidationInvoker ReplacePrintQueuePKValidationInvoker()
		{
			return delegate
			{
				ListValidation.ErrorIfInvalidPK(Parent.ReplacePrintQueuePKInfo);
				TypeValidation.CheckValidGuid(Parent.ReplacePrintQueuePKInfo);
			};
		}

		#endregion

		#region ZValidation Overrides

		public override void ValidateAll()
		{
			ValidateReplacePrintQueuePK();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		#endregion
	}
}
