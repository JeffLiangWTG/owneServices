using System;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class SupportingDocumentValidation : IL.Business.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent) : base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
		}

		protected override void CheckEDoc()
		{
			base.CheckEDoc();
			CheckEDocIsMissing();
		}

		void CheckEDocIsMissing()
		{
			var parent = Parent;
			if (parent.CSI_Status != RequestedSupportingStatusList.Codes.CAN && parent.EDoc.IsEmpty)
			{
				parent.EDocInfo.AddWarning(ValidationCaptions.SupportingDocument.EDocIsMissing);
			}
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
