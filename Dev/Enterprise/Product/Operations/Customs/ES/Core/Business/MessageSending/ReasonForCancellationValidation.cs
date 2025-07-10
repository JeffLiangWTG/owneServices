using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business
{
	public class ReasonForCancellationValidation : ZValidation
	{
		public ReasonForCancellationValidation(ReasonForCancellation parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ReasonForCancellation parent;

		public override Type AutoValidationType => typeof(ReasonForCancellationValidation);

		public override void ValidateAll()
		{
			ValidateCode();
			ValidateReason();
		}

		public void ValidateCode()
		{
			ValidateCalculatedProperty(parent.CodeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckCode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CodeInfo);
		}

		public void ValidateReason()
		{
			ValidateCalculatedProperty(parent.ReasonInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void CheckReason()
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.ReasonInfo);
		}
	}
}
