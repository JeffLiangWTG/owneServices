using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaAdditionalInfoValidation : CusSupportingInfoValidation
	{
		public AsycudaAdditionalInfoValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (Parent.CSI_Description.IsEmpty)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_ReferenceNumberInfo, Parent.CSI_CodeInfo, Constants.AsycudaAdditionalInfoValidation.StatementCodeOrContentMustBeProvided);
			}

			var codesHaveList = new HashSet<ZString>
			{
				Constants.AsycudaAdditionalInfoCodes.UNLOCO,
				Constants.AsycudaAdditionalInfoCodes.ExporterTypeID,
				Constants.AsycudaAdditionalInfoCodes.NightStop,
				Constants.AsycudaAdditionalInfoCodes.IsCooling,
				Constants.AsycudaAdditionalInfoCodes.MultipleDeals,
				Constants.AsycudaAdditionalInfoCodes.CargoType,
				Constants.AsycudaAdditionalInfoCodes.ActionCode
			};

			var parent = Parent;
			if (codesHaveList.Contains(parent.CSI_Code))
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			if (Parent.CSI_ReferenceNumber.IsEmpty)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_DescriptionInfo, Parent.CSI_CodeInfo, Constants.AsycudaAdditionalInfoValidation.StatementCodeOrContentMustBeProvided);
			}

			var codesHaveList = new HashSet<ZString>
			{
				Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery
			};

			var parent = Parent;
			if (codesHaveList.Contains(parent.CSI_Code))
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_DescriptionInfo);
			}
		}
	}
}
