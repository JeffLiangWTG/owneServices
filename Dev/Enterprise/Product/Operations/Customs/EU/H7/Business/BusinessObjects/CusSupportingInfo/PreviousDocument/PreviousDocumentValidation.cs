using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class PreviousDocumentValidation : CusSupportingInfoValidation
	{
		public PreviousDocumentValidation(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			if (Parent.CSI_ParentTableCode == AsycudaBillSchema.Constants.Prefix || Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo, CSI_CodeDesc);
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			if (Parent.CSI_ParentTableCode == AsycudaBillSchema.Constants.Prefix || Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo, CSI_ReferenceNumberDesc);
			}
		}

		string CSI_CodeDesc => Res.GetString("731f4105-8037-428c-a863-a25d927ad454",
			"Previous Document type");

		string CSI_ReferenceNumberDesc => Res.GetString("1784979e-6f2d-4e95-98f9-e7af87a76d47",
			"Previous Document reference number");
	}
}
