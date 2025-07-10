using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class CusEntryInstructionValidation : AutoBRCusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent)
			: base(parent)
		{
		}

		protected new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateMaxCountOfEntryInstructionsForImportSiscomex();
			ValidateUCRNumber();
			ValidateBillNumber();
		}

		internal static string NotAllowMultiplyEntryInstructionsMessage => Res.GetString("59663756-0F14-492A-99C1-F73897B72C50", "Only one Entry Instruction is allowed on this Shipment Type.");

		void ValidateMaxCountOfEntryInstructionsForImportSiscomex()
		{
			Parent.RemoveRowError(NotAllowMultiplyEntryInstructionsMessage);
			if (Parent.IsImportSiscomex)
			{
				var collection = Parent.JobDeclaration?.CustomsEntryInstructions;
				if (collection != null && (collection.Count > 1) && (Parent.PK != collection[0].PK))
				{
					Parent.AddRowError(NotAllowMultiplyEntryInstructionsMessage);
				}
			}
		}

		protected override void CheckCEI_Description()
		{
			base.CheckCEI_Description();
			ValidateMaxCountOfEntryInstructionsForImportSiscomex();
		}

		public void ValidateUCRNumber()
		{
			ValidateCalculatedProperty(Parent.UCRNumberInfo);
		}

		protected void CheckUCRNumber()
		{
			if (Parent.IsExport && !Parent.UCRNumber.IsEmpty && !BRCusEntryNumValidationHelper.CheckValidMasterUCREntryNumberFormat(Parent.UCRNumber))
			{
				Parent.UCRNumberInfo.AddMessageError(BRCusEntryNumValidationHelper.GetUCREntryNumberInvalidMessage(Res.GetString("BD3C4949-67E0-4632-A7C7-83143EBA669E", "UCR Number")));
			}
		}

		public void ValidateBillNumber()
		{
			ValidateCalculatedProperty(Parent.BillNumberInfo);
		}

		protected void CheckBillNumber()
		{
			if (Parent.JobDeclaration?.IsBillNumberOnEntryInstructionApplicable ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BillNumberInfo);
			}
		}

		protected override void CheckCEI_LegalDocument()
		{
			base.CheckCEI_LegalDocument();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_LegalDocumentInfo);
		}

		protected override void CheckCEI_DetailWithoutLegalDoc()
		{
			base.CheckCEI_DetailWithoutLegalDoc();

			if (!Parent.CEI_DetailWithoutLegalDoc_ReadOnly)
			{
				if (Parent.CEI_LegalDocument == LegalDocumentList.Codes.NoInvoice)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CEI_DetailWithoutLegalDocInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.CEI_DetailWithoutLegalDocInfo);
				}

				if (Parent.CEI_SpecialCustomsClearance == SpecialCustomsClearanceList.Codes._2002 && Parent.CEI_DetailWithoutLegalDoc != DetailWithoutLegalDocList.Codes._3004)
				{
					Parent.CEI_DetailWithoutLegalDocInfo.AddMessageError(Res.GetString("FF2D5E31-EB69-4840-A41D-B08F1822478E", "Special Clearance is 2002-Early Boarding. Content of the field Details of the operation without Invoice must be 3004 – Early Boarding"));
				}

				switch (Parent.CEI_DetailWithoutLegalDoc)
				{
					case DetailWithoutLegalDocList.Codes._3001:
					case DetailWithoutLegalDocList.Codes._3002:
					case DetailWithoutLegalDocList.Codes._3003:
					case DetailWithoutLegalDocList.Codes._3005:
					case DetailWithoutLegalDocList.Codes._3006:
					case DetailWithoutLegalDocList.Codes._3010:
					case DetailWithoutLegalDocList.Codes._3011:
						Parent.CEI_DetailWithoutLegalDocInfo.AddMessageError(Res.GetString("C4990BD9-9F18-44C7-BA5A-B31ACF766466", "Content of the field Details of the operation without Invoice is not allowed. According to NOTÍCIA SISCOMEX EXPORTAÇÃO Nº 013/2021 this entry must be created directly in Single Window SISCOMEX."));
						break;
				}
			}
		}

		protected override void CheckCEI_AFRMMMethodOfCalculation()
		{
			base.CheckCEI_AFRMMMethodOfCalculation();

			if (Parent.IsAFRMMApplicable)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CEI_AFRMMMethodOfCalculationInfo);
			}
		}

		protected override void CheckCEI_AFRMMRateOverride()
		{
			base.CheckCEI_AFRMMRateOverride();
			if (!Parent.CEI_AFRMMRateOverride_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_AFRMMRateOverrideInfo);
			}
		}

		protected override void CheckCEI_UtilizationFeeOverride()
		{
			base.CheckCEI_UtilizationFeeOverride();
			if (!Parent.CEI_AFRMMRateOverride_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_UtilizationFeeOverrideInfo);
			}
		}

		protected override void CheckCEI_AdditionalInformationOption()
		{
			if (Parent.IsImportExcludingLicense)
			{
				var additionalInformationLength = 0;
				if (Parent.IsSystemGeneratedInformationApplicable)
				{
					additionalInformationLength += Parent.AdditionalInformation.Length;
				}
				if (Parent.IsFreeTextInformationApplicable)
				{
					additionalInformationLength += Parent.AdditionalInformationManual.Length;
				}
				if (additionalInformationLength > CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength)
				{
					Parent.CEI_AdditionalInformationOptionInfo.AddMessageError(Res.GetString("AFF26D63-C2B9-492E-95EE-DF2E876F723A", "You are reporting size {0} when the expected size is {1}.", additionalInformationLength, CusEntryInstruction.Schema.ImportAdditionalInformationMaxLength));
				}
			}
		}
	}
}
