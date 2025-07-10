using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CountryCompliance;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();

			if (Parent.EntryInstruction != null && !Parent.EntryInstruction.CEI_SpecialCustomsClearance.IsEmpty && !Parent.JI_Procedure.IsEmpty)
			{
				var procedureAttributes = Parent?.CusProcedure?.GetAttributeValues(AttributeNames.Codes.SpecialClearance).ToList();

				if (procedureAttributes == null || !procedureAttributes.Contains(Parent.EntryInstruction.CEI_SpecialCustomsClearance))
				{
					var indexOfEmptyString = procedureAttributes != null ? procedureAttributes.IndexOf(string.Empty) : -1;

					if (indexOfEmptyString >= 0)
					{
						procedureAttributes[indexOfEmptyString] = Res.GetString("bf1a5b1a-4931-4611-9709-925b053b6995", "none");
					}

					var errorMessage = procedureAttributes != null && procedureAttributes.Any()
							? Res.GetString("08a5adc0-4efd-4c40-a82b-4cc64f50796c", "You have entered the CPC {0}, but the Special Clearance differs from {1}. Please Check the CPC against the Special Clearance.", Parent.JI_Procedure, string.Join(", ", procedureAttributes))
							: Res.GetString("9c11d763-dd1a-4ec6-a069-74aa6e5ade5f", "You have entered the CPC {0}. Please Check the CPC against the Special Clearance.", Parent.JI_Procedure);

					Parent.JI_ProcedureInfo.AddMessageError(errorMessage);
				}
			}
		}

		protected override void CheckJI_RN_NKCountryOfExport()
		{
			base.CheckJI_RN_NKCountryOfExport();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_RN_NKCountryOfExportInfo);
		}

		protected override void CheckComplementaryDescription()
		{
			base.CheckComplementaryDescription();

			if (!Parent.ComplementaryDescription.IsEmpty
				&& Parent.EntryInstruction != null
				&& Parent.EntryInstruction.CEI_LegalDocument == LegalDocumentList.Codes.NoInvoice)
			{
				Parent.ComplementaryDescriptionInfo.AddMessageError(Res.GetString("1885D633-22E2-4286-889A-0169CD9D80F3", "Legal Document is SNF – No Invoice. Complementary description should not have a value."));
			}
		}

		protected override void CheckJI_NFeNumber()
		{
			base.CheckJI_NFeNumber();
			if (!Parent.JI_NFeNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.JI_NFeNumberInfo.AddMessageError(Res.GetString("0aceefb1-817d-44a8-9645-dab1a1237406", "Invalid Number"));
			}

			if (Parent.JI_NFeNumber.IsEmpty)
			{
				if (Parent != null && Parent.Declaration != null)
				{
					var instruction = Parent.EntryInstruction;

					if (instruction != null && instruction.CEI_LegalDocument == BrazilComplianceInfo.ComplianceSubTypeCodes.NFE)
					{
						Parent.JI_NFeNumberInfo.AddMessageError(MessageLegalDocumentIsNFEAndNFENumberFieldIsBlank);
					}
				}

				if (!Parent.JI_NFeItemNumber.IsEmpty)
				{
					Parent.JI_NFeNumberInfo.AddMessageError(MessageHasNFEItemNumberAndNFeNumberFieldIsBlank);
				}
			}
		}

		protected override void CheckJI_NFeItemNumber()
		{
			base.CheckJI_NFeItemNumber();
			if (!Parent.JI_NFeItemNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.JI_NFeItemNumberInfo.AddMessageError(Res.GetString("0aceefb1-817d-44a8-9645-dab1a1237406", "Invalid Number"));
			}

			if (!Parent.JI_NFeNumber.IsEmpty && Parent.JI_NFeItemNumber.IsEmpty)
			{
				Parent.JI_NFeItemNumberInfo.AddMessageError(MessageHasNFENumberAndNFEItemNumberFieldIsBlank);
			}
		}

		protected override void CheckJI_CargoPriority()
		{
			base.CheckJI_CargoPriority();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_CargoPriorityInfo, Parent.Lookups.CargoPriorityList);
		}

		protected override void CheckJI_SecondCPC()
		{
			base.CheckJI_SecondCPC();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_SecondCPCInfo);
		}

		protected override void CheckJI_ThirdCPC()
		{
			base.CheckJI_ThirdCPC();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_ThirdCPCInfo);
		}

		protected override void CheckJI_FinancedValue()
		{
			base.CheckJI_FinancedValue();
			MandatoryValidation.CheckNotNegative(Parent.JI_FinancedValueInfo);
		}

		protected override void CheckJI_FourthCPC()
		{
			base.CheckJI_FourthCPC();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_FourthCPCInfo);
		}

		protected override void CheckJI_IntendedTermDays()
		{
			base.CheckJI_IntendedTermDays();
			MandatoryValidation.CheckNotNegative(Parent.JI_IntendedTermDaysInfo);

			if (Parent.JI_IntendedTermDays > 9999)
			{
				Parent.JI_IntendedTermDaysInfo.AddMessageError(Res.GetString("6F41BDA8-ECE7-449D-8B69-5BB4BC6EB410", "Intended term must be up to 4 digit"));
			}
		}

		protected override void CheckJI_DigitalServiceDossier()
		{
			base.CheckJI_DigitalServiceDossier();
			if (!Parent.JI_DigitalServiceDossier.IsNumbersOnlyOrEmpty)
			{
				Parent.JI_DigitalServiceDossierInfo.AddMessageError(Res.GetString("158E880A-D8ED-4107-8770-C587A3E13F67", "Digital Service Dossiers should be only numbers"));
			}
		}

		protected override void CheckJI_AgentCommissionPercentage()
		{
			base.CheckJI_AgentCommissionPercentage();
			MandatoryValidation.CheckNotNegative(Parent.JI_AgentCommissionPercentageInfo);

			if (Parent.JI_AgentCommissionPercentage > 100)
			{
				Parent.JI_AgentCommissionPercentageInfo.AddMessageError(Res.GetString("6b8d5970-8ab9-4161-9558-6d7575ca9a71", "Agent Commission must be up to 100"));
			}
		}

		public static string MessageLegalDocumentIsNFEAndNFENumberFieldIsBlank =>
			Res.GetString("2C6889AD-184E-4DA0-B5A4-D8BCE434FE5B", "Please enter the NFE Key");

		public static string MessageHasNFEItemNumberAndNFeNumberFieldIsBlank => Res.GetString(
			"8065961E-44C2-4365-9CEB-2585FFE0B3ED",
			"You have entered the NFE Item Number but none NFE Key. Please enter the NFE Key");

		public static string MessageHasNFENumberAndNFEItemNumberFieldIsBlank => Res.GetString(
			"88C20056-B63B-430B-B3E0-DDCE5A10343B",
			"You have entered the NFE Key but none NFE Item Number. Please enter the NFE Item Number");
	}
}
