using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportJobComInvoiceHeaderValidation : CommonJobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckSupportingDocumentRequiredCode();
		}

		protected override string GetIncoTermIsRequiredMessage(ZPropertyInfo info)
		{
			return Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.CEI_Style == ExportDeclarationTypeList.Codes.B1 || x.CEI_Style == ExportDeclarationTypeList.Codes.B2) ?
				Res.GetString("E41973BF-CF5B-4545-9492-C3FF22DC9F95", "Please enter an Incoterm. Incoterm is needed to execute correct calculation for the required statistical value.") :
				base.GetIncoTermIsRequiredMessage(info);
		}

		public override bool ShouldValidateWaterIncoTermAndTransportMode => false;

		protected override void CheckJZ_IncoTermPlaceWhenIncoTermIsNotOther()
		{
			if (Parent.ZG_AgreedPlaceCode.Length == 2 && Parent.JZ_IncoTermPlace.IsEmpty)
			{
				var info = Parent.JZ_IncoTermPlaceInfo;
				info.AddMessageError(MandatoryValidation.MustBeEnteredMessage(Res.GetString("80DAB2C3-BD97-4E1A-BB95-9C369147B8CB", "{0}. This field is mandatory when a country is entered into {1}", info.HumanReadableName, Parent.ZG_AgreedPlaceCodeInfo.HumanReadableName)));
			}
		}

		protected override void CheckJZ_AdditionalTermsWhenEmpty()
		{
			if (Parent.JZ_IncoTerm.EqualsIgnoringCase(Core.Constants.IncoTerms.Other))
			{
				var info = Parent.JZ_AdditionalTermsInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
		}

		protected override bool IsJZ_ValuationCodeMandatory => Parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(IsValuationCodeRequired);

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges => TypeOfValidationForMissingMandatoryChargesForIncoterm.MessageError;

		bool IsValuationCodeRequired(CusEntryInstruction instruction) => !instruction.IsSubStyle_B_C_E_F;

		void CheckSupportingDocumentRequiredCode()
		{
			if (!(Parent.HasSupportingDocumentForExportWithInvoiceNumber
				|| Parent.HasExportEntryInstructionWithSupportingDocumentHavingInvoiceNumber))
			{
				Parent.AddRowMessageError(Constants.ValidationMessage.SupportingDocumentMustContainAnInvoiceDocumentWithInvoiceNumber);
			}
		}

		protected override bool IncoTermRequired => Parent.AllRelatedEntryInstructionsStatisticalValueRequired;
	}
}
