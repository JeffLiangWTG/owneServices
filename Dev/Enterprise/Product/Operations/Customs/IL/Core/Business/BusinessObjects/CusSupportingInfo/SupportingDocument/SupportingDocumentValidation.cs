using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentValidation : CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
			this.parentListInternals = parent;
			this.zValidationInternals = this;
		}

		public void ValidateEDoc()
		{
			zValidationInternals.Validate(Parent.EDocInfo, GetEDocValidationInvoker());
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (parentListInternals.SuspendListChanged())
			{
				ValidateEDoc();
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		protected virtual void CheckEDoc()
		{
		}

		RunValidationInvoker GetEDocValidationInvoker()
		{
			return CheckEDoc;
		}

		#region Implementation

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
		readonly ISingleElementListInternal parentListInternals;
		readonly IValidationInternals zValidationInternals;

		#endregion
	}
}
