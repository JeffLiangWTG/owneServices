//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAdditionalInfoValidation
//
//    This class should be used for overriding validation in AutoAdditionalInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfoValidation : Customs.Business.CusSupportingInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCSI_NctsExportFromEC();
			}
		}

		public void ValidateCSI_NctsExportFromEC()
		{
			ValidateCalculatedProperty(Parent.CSI_NctsExportFromECInfo);
		}

		protected virtual void CheckCSI_NctsExportFromEC()
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		protected override void CheckCSI_Code()
		{
			if (IsCodeEnabled)
			{
				if (IsCodeMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
				}

				if (ShouldCodeBeInTheList)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
				}

				CheckCSI_CodeRuleC0612();
			}
		}

		protected override void CheckCSI_Status()
		{
			if (IsOtherFieldsEnabled)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_StatusInfo);
				ValidateCSI_Code();
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			CheckRuleBR2038IfApplicable();
		}

		void CheckRuleBR2038IfApplicable()
		{
			var parent = Parent;
			if (parent.CSI_Code == UniversalReferenceConstants.SupportingDocumentTypes.N741
				&& parent.Parent is IAdditionalInfosProviderWithValidationDecider supporter
				&& supporter.ValidationDecider is IAdditionalInfoValidationDecider validationSupporter
				&& validationSupporter.IsBR2038Rule
			)
			{
				new AirWayBillValidator().Validate(Parent.CSI_ReferenceNumberInfo);
			}
		}

		void CheckCSI_CodeRuleC0612()
		{
			var parent = Parent;

			if (parent.IsAnAdditionalReference
				&& !parent.ParentIsJobComInvoiceLine
				&& parent.Parent is IAdditionalInfosProviderWithValidationDecider supporter
				&& (supporter.ValidationDecider?.IsC0612Rule ?? false))
			{
				CusSupportingInfoValidationHelper.ValidateRuleC0612(parent.CSI_Code, parent.CSI_CodeInfo);
			}
		}

		protected virtual bool IsCodeEnabled => true;

		protected virtual bool IsCodeMandatory => true;

		protected virtual bool ShouldCodeBeInTheList => Parent.CSI_Status != AdditionalInfoIssuerList.Codes.Other;

		protected virtual bool IsOtherFieldsEnabled => true;
	}
}
