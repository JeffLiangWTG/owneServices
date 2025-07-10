using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		JobDeclaration Declaration => Parent;

		protected override void CheckJE_Box18TransportNationality()
		{
			base.CheckJE_Box18TransportNationality();
			CheckRuleC810_N01();
		}

		void CheckRuleC810_N01()
		{
			var declaration = Declaration;
			if (declaration?.Validation.ValidationDecider is IDeclarationValidationDecider validationDecider
				&& validationDecider.IsRuleC0810_N01Active)
			{
				if (declaration.ZG_Box18TransportNationality.IsEmpty
					&& declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.IsTradingWithSpecialFiscalTerritoriesProcedure))
				{
					declaration.ZG_Box18TransportNationalityInfo.AddMessageError(Res.GetString("9DE74789-5711-4890-BC6F-943F37F6B13A", "[C0810_N01] Nationality for Active border transport means is mandatory."));
				}
				else if (!declaration.ZG_Box18TransportNationality.IsEmpty
					&& declaration.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.IsTradingWithSpecialFiscalTerritoriesProcedure))
				{
					declaration.ZG_Box18TransportNationalityInfo.AddMessageError(Res.GetString("0EB77002-6511-498C-AC8F-87ACB0CB62EF", "[C0810_N01] Transport Nationality must be empty when all invoice lines Customs procedure end with F15 (Trading With Special Fiscal Territories)."));
				}
			}
		}

		protected override void CheckJE_VATDeferNumber()
		{
			base.CheckJE_VATDeferNumber();
			var declaration = Declaration;
			if (declaration.ZG_VATDeferType == VATProcedureList.Codes._2 && declaration.Ai2Permit is null)
			{
				declaration.ZG_VATDeferNumberInfo.AddMessageError(Res.GetString("ba1d90d1-0941-4f5f-a6cc-bf21e2ad9651", "Could not find matching AI2 permit, please check the number and expiration date."));
			}
			else if (declaration.ZG_VATDeferType == VATProcedureList.Codes.L && declaration.JE_VATDeferNumber.IsEmpty)
			{
				declaration.ZG_VATDeferNumberInfo.AddMessageError(Res.GetString("E487A843-0E97-4C2C-8CE3-556924307EFC", "VAT number is mandatory when VAT Defer Type is L."));
			}
		}

		protected override void CheckJE_AgreedPlaceCode()
		{
			base.CheckJE_AgreedPlaceCode();
			if (!Declaration.IsUCC6AndIsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_AgreedPlaceCodeInfo);
			}
		}

		protected override void AddMessageErrorOrWarningForMismatch()
		{
			var parent = Parent;
			if (parent.IsUCC6AndIsImport)
			{
				parent.ZG_AgreedPlaceCodeInfo.AddWarning(DeclarationValidationConstants.IncotermPlaceCodeMismatch);
			}
			else
			{
				base.AddMessageErrorOrWarningForMismatch();
			}
		}

		protected override void AddMessageErrorForRequiredZG_AgreedPlaceCode()
		{
			if (!Declaration.IsUCC6AndIsImport)
			{
				base.AddMessageErrorForRequiredZG_AgreedPlaceCode();
			}
		}

		protected override void CheckJE_VATDeferType()
		{
			base.CheckJE_VATDeferType();
			var parent = Parent;
			var declaration = Declaration;

			ListValidation.MessageErrorIfInvalidCode(parent.ZG_VATDeferTypeInfo, Declaration.AddInfoLookups.DeferTypeList);

			if (declaration.IsExport && (parent.ZG_VATDeferType == VATProcedureList.Codes.L || parent.ZG_VATDeferType == VATProcedureList.Codes._2))
			{
				parent.ZG_VATDeferTypeInfo.AddMessageError(Res.GetString("C7CE2FD6-B867-4724-A2BC-7D1C46E2B2F8", "This VAT procedure is not allowed for exports."));
			}

			if (declaration.IsImport)
			{
				declaration.VATNumberSupporter.ValidateVATNumber(parent.ZG_VATDeferTypeInfo);
			}
		}

		protected override void CheckJE_VATCANACode()
		{
			var parent = Parent;
			if (parent.ZG_VATDeferType == VATProcedureList.Codes._2)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ZG_VATCANACodeInfo);
				CheckNAT_041Quinquies(parent);
			}

			var declaration = Declaration;
			var specialMentionToHave = declaration.GetVatCanaSpecialMention(parent.JE_VATCANACode);
			if (!specialMentionToHave.IsEmpty
				&& !declaration.AdditionalInfos.Cast<AdditionalInfo>().Any(x => string.Compare(x.CSI_Code, specialMentionToHave, System.StringComparison.OrdinalIgnoreCase) == 0))
			{
				parent.ZG_VATCANACodeInfo.AddMessageError(Res.GetString("AD545848-D573-4945-9C3A-323A5E2467D3", "VAT CANA {0} requires special mention {1}", parent.JE_VATCANACode, specialMentionToHave));
			}
		}

		void CheckNAT_041Quinquies(JobDeclaration parent)
		{
			var declaration = Declaration;
			if (declaration.Validation.ValidationDecider is IDeclarationValidationDecider validationDecider && validationDecider.IsRuleNAT_041QuinquiesActive)
			{
				var hasForbiddenTax = declaration.CustomsEntryHeaders.SelectMany(h => h.MergedLines).SelectMany(line => line.Fees.OfType<CusEntryLineFee>())
					.Any(fee =>
					   fee.NationalFeeTypeCode == UniversalReferenceConstants.RefCusRateCodes.A435 ||
					   fee.NationalFeeTypeCode == UniversalReferenceConstants.RefCusRateCodes.A825);

				if (hasForbiddenTax && !parent.ZG_VATCANACode.IsEmpty)
				{
					parent.ZG_VATCANACodeInfo.AddMessageError(Res.GetString("A1F3D2B7-9C4E-4A6F-B123-8D9E7C5B4A6F", "[Nat_041_05] CANA AI2 cannot be selected when tax A435 or A825 must be paid."));
				}
			}
		}
	}
}
