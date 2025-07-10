using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class TransportDocumentMasterValidation : CusSupportingInfoValidation
	{
		public TransportDocumentMasterValidation(TransportDocumentMaster parent) : base(parent)
		{
		}
		protected new TransportDocumentMaster Parent => (TransportDocumentMaster)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			if (parent.Parent is CUSPRLCusTempStorageLine)
			{
				if (!parent.CSI_ReferenceNumber.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_CodeInfo, Res.GetString("F2A5BC4F-B2C6-46BC-BF36-E12A5007A564", "Transport Document Master Type"));
				}
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			if (parent.Parent is CUSPRLCusTempStorageLine)
			{
				if (!parent.CSI_Code.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo, Res.GetString("5E8D9CDC-433B-4AC6-8F80-9452B15355F80", "Transport Document Master Reference Number"));
				}
			}
		}
	}
}
